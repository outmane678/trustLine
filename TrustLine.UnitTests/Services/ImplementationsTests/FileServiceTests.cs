using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO.Compression;
using Microsoft.AspNetCore.Hosting;
using AnonymousComplaintsAPI.Services.Implementations;

namespace TrustLine.Tests.Services
{
    public class FileServiceTests
    {
        private FileService CreateService()
        {
            var envMock = new Mock<IWebHostEnvironment>();
            var root = Path.Combine(Path.GetTempPath(), "TrustLineTests");

            envMock.Setup(x => x.WebRootPath).Returns(root);

            var logger = new Mock<ILogger<FileService>>();

            // clean folder before test
            if (Directory.Exists(root))
                Directory.Delete(root, true);

            return new FileService(envMock.Object, logger.Object);
        }

        // =========================
        // GET UNIQUE FILE NAME
        // =========================

        [Fact]
        public void GetUniqueFileName_ShouldReturnGuidPrefixedName()
        {
            var service = CreateService();

            var result = service.GetUniqueFileName("test.pdf");

            Assert.Contains("test.pdf", result);
            Assert.Contains("_", result);
        }

        // =========================
        // VALIDATE FILE
        // =========================

        [Fact]
        public void ValidateFile_ShouldReturnFalse_WhenFileIsNull()
        {
            var service = CreateService();

            var result = service.ValidateFile(null, 1000, new[] { ".pdf" });

            Assert.False(result);
        }

        [Fact]
        public void ValidateFile_ShouldReturnTrue_WhenValid()
        {
            var fileMock = new Mock<IFormFile>();

            fileMock.Setup(f => f.Length).Returns(100);
            fileMock.Setup(f => f.FileName).Returns("test.pdf");
            fileMock.Setup(f => f.ContentType).Returns("application/pdf");

            // PDF magic bytes: %PDF
            var pdfHeader = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D };
            fileMock.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(pdfHeader));

            var service = CreateService();

            var result = service.ValidateFile(fileMock.Object, 1000, new[] { ".pdf" });

            Assert.True(result);
        }

        // =========================
        // SAVE FILE
        // =========================

        [Fact]
        public async Task SaveFile_ShouldCreateFile()
        {
            var service = CreateService();

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.txt");
            fileMock.Setup(f => f.Length).Returns(10);

            var content = Encoding.UTF8.GetBytes("hello");
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                .Returns<Stream, System.Threading.CancellationToken>((stream, token) =>
                {
                    return stream.WriteAsync(content, 0, content.Length);
                });

            var result = await service.SaveFileAsync(fileMock.Object, "Uploads");

            Assert.NotNull(result);
            Assert.Contains("Uploads", result);
        }

        // =========================
        // DELETE FILE
        // =========================

        [Fact]
        public async Task DeleteFile_ShouldNotThrow_WhenFileNotExists()
        {
            var service = CreateService();

            await service.DeleteFileAsync("fake.txt");
        }

        // =========================
        // FILE EXISTS
        // =========================

        [Fact]
        public void FileExists_ShouldReturnFalse_WhenNotExists()
        {
            var service = CreateService();

            var result = service.FileExists("notfound.txt");

            Assert.False(result);
        }

        // =========================
        // ZIP
        // =========================

        [Fact]
        public async Task CreateZip_ShouldReturnStream()
        {
            var service = CreateService();

            var result = await service.CreateZipFromFilesAsync(
                new List<string>(),
                "test.zip"
            );

            Assert.NotNull(result);
            Assert.IsType<MemoryStream>(result);
        }
    }
}