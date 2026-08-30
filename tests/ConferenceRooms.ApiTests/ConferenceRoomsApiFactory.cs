using ConferenceRooms.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConferenceRooms.ApiTests;

public sealed class ConferenceRoomsApiFactory : WebApplicationFactory<Program>
{
    private const string UnreachableConnectionString =
        "Server=127.0.0.1,1;Database=ConferenceRoomsDb;Trusted_Connection=True;Encrypt=False;Connect Timeout=1;";

    private readonly string _environment;
    private readonly IReadOnlyDictionary<string, string?> _configurationOverrides;

    public ConferenceRoomsApiFactory()
        : this(new Dictionary<string, string?>(), Environments.Development)
    {
    }

    internal ConferenceRoomsApiFactory(
        IReadOnlyDictionary<string, string?> configurationOverrides,
        string environment = "Development")
    {
        _configurationOverrides = configurationOverrides;
        _environment = environment;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(_environment);
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = UnreachableConnectionString,
                ["RateLimiting:PermitLimit"] = "120",
                ["RateLimiting:WindowSeconds"] = "60",
                ["RequestLimits:MaxRequestBodySizeBytes"] = "65536",
            };

            foreach (var (key, value) in _configurationOverrides)
            {
                settings[key] = value;
            }

            configuration.AddInMemoryCollection(settings);
        });
        builder.ConfigureServices(services =>
        {
            var dbContextConfiguration = services.SingleOrDefault(
                service => service.ServiceType
                    == typeof(IDbContextOptionsConfiguration<ConferenceRoomsDbContext>))
                ?? throw new InvalidOperationException(
                    "The API DbContext configuration is not registered.");

            services.Remove(dbContextConfiguration);
            services.AddDbContext<ConferenceRoomsDbContext>(options =>
                options.UseSqlServer(UnreachableConnectionString));
            services.AddControllers()
                .AddApplicationPart(typeof(RequestBodyTooLargeController).Assembly);
        });
    }

    public HttpClient CreateHttpsClient()
    {
        return CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost"),
        });
    }
}
