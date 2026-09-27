using ffxiv_api.Data;
using ffxiv_api.Services;
using Microsoft.EntityFrameworkCore;

const string AngularDevCorsPolicy = "AllowAngularApp";

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration["SQL:ConnectionString"]
	?? throw new InvalidOperationException("Missing 'SQL:ConnectionString'. See the README for setting it with user secrets.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<MentorRouletteStatsCache>();
builder.Services.AddScoped<DutyService>();
builder.Services.AddScoped<MentorRouletteService>();

builder.Services.AddControllers();

// Unhandled exceptions become a ProblemDetails 500 (logged, no internals leaked).
// Expected failures are returned by services as ServiceError and mapped in the controllers.
builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
	options.AddPolicy(AngularDevCorsPolicy, policy =>
	{
		policy.WithOrigins("http://localhost:4200")
			.AllowAnyHeader()
			.AllowAnyMethod();
	});
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
	app.UseCors(AngularDevCorsPolicy);
}

app.MapControllers();

app.Run();
