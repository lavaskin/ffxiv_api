using Microsoft.AspNetCore.Mvc;
using ffxiv_api.Data;
using ffxiv_api.Models.Entity;

namespace ffxiv_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MentorRouletteController : ControllerBase
{
    private readonly AppDbContext _context;

    public MentorRouletteController(AppDbContext context)
    {
        _context = context;
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
	public async Task<IActionResult> GetMentorRouletteLog(long id)
	{
		// Implementation to retrieve MentorRouletteLog by id
		throw new NotImplementedException();
	}
	
	[HttpPost]
	public async Task<IActionResult> CreateNewLog(MentorRouletteLog model)
	{
		string? validationError = model.Validate();
		if (validationError != null)
		{
			return BadRequest(new { Error = validationError });
		}

		model.MentorRouletteLogId = 0; // Ensure the ID is zero for new entries
		model.DatePlayed = DateTime.UtcNow;

		// Add the new log to the database
		_context.MentorRouletteLogs.Add(model);
		await _context.SaveChangesAsync();

		// Set NotMapped properties
		model.SetNotMapped();

		// Return the created log with a 201 status
		return CreatedAtAction(nameof(GetMentorRouletteLog), new { id = model.MentorRouletteLogId }, model);
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteLog(long id)
	{
		// Implementation to delete a MentorRouletteLog by id
		throw new NotImplementedException();
	}
}
