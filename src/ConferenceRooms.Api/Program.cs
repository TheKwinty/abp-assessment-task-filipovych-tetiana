using ConferenceRooms.Api.Services;
using ConferenceRooms.Core.Pricing;
using ConferenceRooms.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is not configured.");

builder.Services.AddDbContext<ConferenceRoomsDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<HallService>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddSingleton<RentalPriceCalculator>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
