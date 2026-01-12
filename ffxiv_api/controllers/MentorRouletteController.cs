using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ffxiv_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MentorRouletteController : ControllerBase
{
    private readonly string _connectionString;

    public MentorRouletteController(string connectionString)
    {
        _connectionString = connectionString;
    }

	/// <summary>
	/// Fetches all Mentor Roulette Logs
	/// </summary>
	/// <returns>A list of all the mentor roulette logs</returns>
	[HttpGet]
	public async Task<IActionResult> GetMentorRouletteLogs()
	{
		throw new NotImplementedException();
	}

    [HttpGet("{id}")]
	public async Task<IActionResult> GetMentorRouletteLog(int id)
	{
		// Implementation to retrieve MentorRouletteLog by id
		throw new NotImplementedException();
	}
	
	[HttpPost]
	public async Task<IActionResult> CreateNewLog(MentorRouletteLog log)
	{
		// Implementation to create a new MentorRouletteLog
		throw new NotImplementedException();
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteLog(int id)
	{
		// Implementation to delete a MentorRouletteLog by id
		throw new NotImplementedException();
	}
}
