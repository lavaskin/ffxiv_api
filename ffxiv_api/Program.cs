using Microsoft.EntityFrameworkCore;
using ffxiv_api.Data;
using ffxiv_api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddEndpointsApiExplorer();

// Add SQL connection string
var connectionString = builder.Configuration["SQL:ConnectionString"];
builder.Services.AddSingleton(connectionString ?? throw new InvalidOperationException("Connection string not found"));

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add Services
builder.Services.AddScoped<MentorRouletteService>();

// Configure CORS to allow requests from local Angular apps
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAngularApp");
}

app.Run();
