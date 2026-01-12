var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add SQL connection string
var connectionString = builder.Configuration["SQL:ConnectionString"];
builder.Services.AddSingleton(connectionString ?? throw new InvalidOperationException("Connection string not found"));

var app = builder.Build();

app.MapControllers();

app.Run();
