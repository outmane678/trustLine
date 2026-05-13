using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using AnonymousComplaintsAPI.Data;
using AnonymousComplaintsAPI.DTOs.Responses;
using AnonymousComplaintsAPI.Models.Entities;
using AnonymousComplaintsAPI.Services.EnsureServices;
using AnonymousComplaintsAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TrustLine.IntegrationTests.Helpers
{
    /// <summary>
    /// Fake EnsureService that works with InMemory DB (no external HTTP calls)
    /// </summary>
    public class FakeEnsureService : IEnsureService
    {
        private readonly AnonymousComplaintsV002Context _context;

        public FakeEnsureService(AnonymousComplaintsV002Context context)
        {
            _context = context;
        }

        public async Task<User> EnsureUserExistsAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user != null)
                return user;

            user = new User { UserId = userId };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public Task<List<ShortProfileResponseDto>?> GetExternalProfilesAsync()
        {
            return Task.FromResult<List<ShortProfileResponseDto>?>(new List<ShortProfileResponseDto>());
        }
    }

    /// <summary>
    /// Fake AccessGateService that always grants permission (no external HTTP calls)
    /// </summary>
    public class FakeAccessGateService : IAccessGateService
    {
        public Task<bool> CheckPermissionAsync(int userId, string permission)
        {
            return Task.FromResult(true);
        }
    }

    /// <summary>
    /// Test authentication handler that creates an authenticated user with an "Id" claim.
    /// This bypasses real JWT authentication for integration tests.
    /// </summary>
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "TestScheme";
        public const int DefaultUserId = 1;

        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim("Id", DefaultUserId.ToString()),
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.NameIdentifier, DefaultUserId.ToString()),
            };
            var identity = new ClaimsIdentity(claims, SchemeName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbName = $"IntegrationTestsDb_{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Add test configuration to satisfy required config sections
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["UploadsDirectory"] = "wwwroot/uploads",
                    ["JsonWebTokenKeys:ValidateIssuerSigningKey"] = "true",
                    ["JsonWebTokenKeys:IssuerSigningKey"] = "TestKey-64A631ere53-11ezeC1-49sd19-9ds133-EFAF99Asds9B456",
                    ["JsonWebTokenKeys:ValidateIssuer"] = "false",
                    ["JsonWebTokenKeys:ValidIssuer"] = "http://localhost",
                    ["JsonWebTokenKeys:ValidateAudience"] = "false",
                    ["JsonWebTokenKeys:ValidAudience"] = "http://localhost",
                    ["JsonWebTokenKeys:RequireExpirationTime"] = "false",
                    ["JsonWebTokenKeys:ValidateLifetime"] = "false",
                    ["All_Apps:0:Name"] = "AccessGate",
                    ["All_Apps:0:Ips:0"] = "http://localhost:9999/",
                    ["ConnectionStrings:DefaultConnection"] = "DataSource=:memory:",
                });
            });

            builder.ConfigureServices(services =>
            {
                // Remove ALL DbContext-related registrations to avoid dual-provider conflict
                services.RemoveAll<DbContextOptions<AnonymousComplaintsV002Context>>();
                services.RemoveAll(typeof(DbContextOptions));
                services.RemoveAll<AnonymousComplaintsV002Context>();

                // Remove the real EnsureService
                services.RemoveAll<IEnsureService>();

                // Remove the real AccessGateService
                services.RemoveAll<IAccessGateService>();

                // Add InMemory database for tests (unique per factory instance)
                services.AddDbContext<AnonymousComplaintsV002Context>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                });

                // Register fake EnsureService (no external HTTP calls)
                services.AddScoped<IEnsureService, FakeEnsureService>();

                // Register fake AccessGateService (always grants permission)
                services.AddScoped<IAccessGateService, FakeAccessGateService>();

                // Replace authentication with test scheme (bypasses JWT)
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, options => { });
            });
        }
    }
}