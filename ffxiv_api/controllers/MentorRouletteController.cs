using Microsoft.AspNetCore.Mvc;
using ffxiv_api.Data;
using Microsoft.EntityFrameworkCore;
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
		try
		{
			var logs = await _context.MentorRouletteLogs
				.Include(log => log.DutyModel)
				.ToListAsync();
			foreach (var log in logs)
			{
				log.SetNotMapped();
			}

			return Ok(logs);
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { Error = "An error occurred while retrieving mentor roulette logs.", Details = ex.Message });
		}
	}

    [HttpGet("{id}")]
	public async Task<IActionResult> GetMentorRouletteLog(long id)
	{
		try
		{
			var log = await _context.MentorRouletteLogs
				.Include(log => log.DutyModel)
				.FirstOrDefaultAsync(l => l.MentorRouletteLogId == id);
			if (log == null)
			{
				return NotFound(new { Error = "Mentor Roulette Log not found." });
			}

			log.SetNotMapped();
			return Ok(log);
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { Error = "An error occurred while retrieving the mentor roulette log.", Details = ex.Message });
		}
	}
	
	[HttpPost]
	public async Task<IActionResult> CreateNewLog(MentorRouletteLogModel model)
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

	[HttpPut("{id}")]
	public async Task<IActionResult> UpdateLog(long id, MentorRouletteLogModel model)
	{
		try
		{
			if (id != model.MentorRouletteLogId)
			{
				return BadRequest(new { Error = "An unknown error occurred" });
			}

			string? validationError = model.Validate();
			if (validationError != null)
			{
				return BadRequest(new { Error = validationError });
			}

			_context.Entry(model).State = EntityState.Modified;
			await _context.SaveChangesAsync();

			// Set NotMapped properties
			model.SetNotMapped();

			return Ok(model);
		}
		catch (DbUpdateConcurrencyException)
		{
			if (!await _context.MentorRouletteLogs.AnyAsync(e => e.MentorRouletteLogId == id))
			{
				return NotFound(new { Error = "Mentor Roulette Log not found." });
			}
			else
			{
				throw;
			}
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { Error = "An error occurred while updating the mentor roulette log.", Details = ex.Message });
		}
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteLog(long id)
	{
		try
		{
			var log = await _context.MentorRouletteLogs.FindAsync(id);
			if (log == null)
			{
				return NotFound(new { Error = "Mentor Roulette Log not found." });
			}

			_context.MentorRouletteLogs.Remove(log);
			await _context.SaveChangesAsync();

			return NoContent();
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { Error = "An error occurred while deleting the mentor roulette log.", Details = ex.Message });
		}
	}
}
