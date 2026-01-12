using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ffxiv_api.controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly string _connectionString;

    public TestController(string connectionString)
    {
        _connectionString = connectionString;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { message = "API is working!", timestamp = DateTime.UtcNow });
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { status = "alive", service = "ffxiv_api" });
    }

    [HttpGet("db-test")]
    public async Task<IActionResult> TestDatabaseConnection()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT @@VERSION as Version", connection);
            var version = await command.ExecuteScalarAsync();
            
            return Ok(new 
            { 
                message = "Database connection successful!",
                serverVersion = version?.ToString()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new 
            { 
                message = "Database connection failed",
                error = ex.Message
            });
        }
    }
}
