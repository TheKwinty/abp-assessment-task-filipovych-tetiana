using System.Threading.RateLimiting;
using ConferenceRooms.Api.Services;
using ConferenceRooms.Core.Pricing;
using ConferenceRooms.Infrastructure.Data;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

const string CorsPolicyName = "ConfiguredOrigins";
const string PermitLimitKey = "RateLimiting:PermitLimit";
const string WindowSecondsKey = "RateLimiting:WindowSeconds";
const string MaxRequestBodySizeKey = "RequestLimits:MaxRequestBodySizeBytes";

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is not configured.");

builder.Services.AddDbContext<ConferenceRoomsDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<HallService>();
builder.Services.AddScoped<HallAvailabilityService>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<BookingReportService>();
builder.Services.AddSingleton<RentalPriceCalculator>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.SwaggerDoc("v1", new()
{
    Title = "Conference Rooms API",
    Version = "v1",
    Description = "Conference hall management, availability search, booking, "
        + "pricing and booking analytics API.",
}));
builder.Services.AddCors(options => options.AddPolicy(CorsPolicyName, policy =>
{
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? [];

    if (allowedOrigins.Length > 0)
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    }
}));
builder.Services.AddRateLimiter(options =>
{
    var permitLimit = GetRequiredPositiveInt32(
        builder.Configuration,
        PermitLimitKey);
    var windowSeconds = GetRequiredPositiveInt32(
        builder.Configuration,
        WindowSecondsKey);

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromSeconds(windowSeconds),
                QueueLimit = 0,
                AutoReplenishment = true,
            }));
});
builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = context =>
        context.ProblemDetails.Extensions["traceId"] =
            context.HttpContext.TraceIdentifier);
builder.WebHost.ConfigureKestrel(options =>
    options.Limits.MaxRequestBodySize = GetRequiredPositiveInt64(
        builder.Configuration,
        MaxRequestBodySizeKey));

var app = builder.Build();

ValidateHardeningConfiguration(app.Configuration);

app.UseExceptionHandler(new ExceptionHandlerOptions
{
    StatusCodeSelector = exception => exception is BadHttpRequestException badRequest
        ? badRequest.StatusCode
        : StatusCodes.Status500InternalServerError,
});
app.UseStatusCodePages();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors(CorsPolicyName);
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();

static void ValidateHardeningConfiguration(IConfiguration configuration)
{
    _ = GetRequiredPositiveInt32(configuration, PermitLimitKey);
    _ = GetRequiredPositiveInt32(configuration, WindowSecondsKey);
    _ = GetRequiredPositiveInt64(configuration, MaxRequestBodySizeKey);
}

static int GetRequiredPositiveInt32(IConfiguration configuration, string key)
{
    if (!int.TryParse(configuration[key], out var value) || value <= 0)
    {
        throw new InvalidOperationException(
            $"Configuration value '{key}' must be a positive integer.");
    }

    return value;
}

static long GetRequiredPositiveInt64(IConfiguration configuration, string key)
{
    if (!long.TryParse(configuration[key], out var value) || value <= 0)
    {
        throw new InvalidOperationException(
            $"Configuration value '{key}' must be a positive integer.");
    }

    return value;
}

public partial class Program
{
}
