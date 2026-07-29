using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

using AnonymousComplaintsAPI.Controllers;
using AnonymousComplaintsAPI.Services.Interfaces;
using AnonymousComplaintsAPI.DTOs.Requests;
using AnonymousComplaintsAPI.DTOs.Responses;

using TypeEntity = AnonymousComplaintsAPI.Models.Entities.Type;

namespace TrustLine.Tests.Controllers
{
    public class TypeModelsControllerTests
    {
        private readonly Mock<ITypeService> _serviceMock = new();

        private TypeModelsController CreateController() =>
            new TypeModelsController(_serviceMock.Object);

        // =====================================================
        // GET ALL (paginated)
        // =====================================================
        [Fact]
        public async Task GetTypes_ShouldReturnOk()
        {
            var paginated = new PaginatedResponse<TypeModelResponse>
            {
                Total = 1,
                Data = new List<TypeModelResponse>
                {
                    new TypeModelResponse { TypeId = 1, Name = "Type1" }
                },
                Page = 1,
                PerPage = 10
            };

            _serviceMock.Setup(x => x.GetTypesPaginatedAsync(It.IsAny<PaginationRequest>()))
                .ReturnsAsync(paginated);

            var result = await CreateController().GetTypes(new PaginationRequest());

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(ok.Value);
        }

        // =====================================================
        // GET BY ID
        // =====================================================
        [Fact]
        public async Task GetTypeModel_ShouldReturnOk()
        {
            _serviceMock.Setup(x => x.GetTypeAsync(1, It.IsAny<bool>()))
                .ReturnsAsync(new TypeModelResponse { TypeId = 1, Name = "Type1" });

            var result = await CreateController().GetTypeModel(1);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task GetTypeModel_NotFound_ShouldReturnNotFound()
        {
            _serviceMock.Setup(x => x.GetTypeAsync(99, It.IsAny<bool>()))
                .ReturnsAsync((TypeModelResponse?)null);

            var result = await CreateController().GetTypeModel(99);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        // =====================================================
        // CREATE
        // =====================================================
        [Fact]
        public async Task PostTypeModel_ShouldReturnCreated()
        {
            var dto = new TypeModelResponse { Name = "NewType" };
            var created = new TypeModelResponse { TypeId = 1, Name = "NewType" };

            _serviceMock.Setup(x => x.CreateTypeAsync(dto))
                .ReturnsAsync(created);

            var result = await CreateController().PostTypeModel(dto);

            Assert.IsType<CreatedAtActionResult>(result.Result);
        }

        // =====================================================
        // UPDATE
        // =====================================================
        [Fact]
        public async Task UpdateTypeModel_ShouldReturnOk()
        {
            var dto = new TypeModelResponse { Name = "Updated" };

            _serviceMock.Setup(x => x.UpdateTypeAsync(1, dto))
                .ReturnsAsync(new TypeModelResponse { TypeId = 1, Name = "Updated" });

            var result = await CreateController().UpdateTypeModel(1, dto);

            Assert.IsType<OkResult>(result.Result);
        }

        [Fact]
        public async Task UpdateTypeModel_NotFound_ShouldReturnNotFound()
        {
            var dto = new TypeModelResponse { Name = "Updated" };

            _serviceMock.Setup(x => x.UpdateTypeAsync(99, dto))
                .ReturnsAsync((TypeModelResponse?)null);

            var result = await CreateController().UpdateTypeModel(99, dto);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        // =====================================================
        // ARCHIVE
        // =====================================================
        [Fact]
        public async Task ArchiveTypeModel_ShouldReturnNoContent()
        {
            _serviceMock.Setup(x => x.ArchiveTypeWithCategoriesAsync(1))
                .Returns(Task.CompletedTask);

            var result = await CreateController().ArchiveTypeModel(1);

            Assert.IsType<NoContentResult>(result);
        }

        // =====================================================
        // RESTORE
        // =====================================================
        [Fact]
        public async Task RestoreTypeModel_ShouldReturnNoContent()
        {
            _serviceMock.Setup(x => x.RestoreTypeWithCategoriesAsync(1))
                .Returns(Task.CompletedTask);

            var result = await CreateController().RestoreTypeModel(1);

            Assert.IsType<NoContentResult>(result);
        }

        // =====================================================
        // DELETE
        // =====================================================
        [Fact]
        public async Task DeleteTypeModel_ShouldReturnNoContent()
        {
            _serviceMock.Setup(x => x.ArchiveTypeWithCategoriesAsync(1))
                .Returns(Task.CompletedTask);

            var result = await CreateController().DeleteTypeModel(1);

            Assert.IsType<NoContentResult>(result);
        }
    }
}
