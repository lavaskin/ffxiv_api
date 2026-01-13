using Microsoft.AspNetCore.Mvc;
using ffxiv_api.Data;
using ffxiv_api.Models.Entity;
using Microsoft.EntityFrameworkCore;
using ffxiv_api.Models.DTOs;

namespace ffxiv_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DutyController : ControllerBase
{
    private readonly AppDbContext _context;

    public DutyController(AppDbContext context)
    {
        _context = context;
    }

	[HttpGet]
	public async Task<IActionResult> GetDuties()
	{
		try
		{
			var duties = await _context.Duties.ToListAsync();
			foreach (var duty in duties)
			{
				duty.SetNotMapped();
			}

			return Ok(duties);
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { Error = "An error occurred while retrieving duties.", Details = ex.Message });
		}
	}

    [HttpGet("{id}")]
	public async Task<IActionResult> GetDuty(long id)
	{
		try
		{
			var duty = await _context.Duties.FindAsync(id);
			if (duty == null)
			{
				return NotFound(new { Error = "Duty not found." });
			}

			duty.SetNotMapped();
			return Ok(duty);
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { Error = "An error occurred while retrieving the duty.", Details = ex.Message });
		}
	}

	[HttpGet("[action]")]
	public async Task<IActionResult> GetResultItems(SearchOptions options)
	{
		try
		{
			options.PageSize = Math.Clamp(options.PageSize, 1, 25);

			var query = _context.Duties.AsQueryable();
			if (!string.IsNullOrEmpty(options.Query))
			{
				query = query.Where(d => d.Name.Contains(options.Query));
			}

			var duties = await query
				.Take(options.PageSize)
				.ToListAsync();
			
			// Turn the found duties into result items
			List<ListResultItem> resultItems = new();
			foreach (var duty in duties)
			{
				resultItems.Add(new ListResultItem
				{
					Label = duty.Name,
					Value = duty.DutyId,
				});
			}

			// Sort the result items alphabetically by label
			resultItems = resultItems.OrderBy(ri => ri.Label).ToList();

			return Ok(resultItems);
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { Error = "An error occurred while retrieving result items.", Details = ex.Message });
		}
	}
	
	[HttpPost]
	public async Task<IActionResult> CreateNewDuty(DutyModel model)
	{
		try
		{
			string? validationError = model.Validate();
			if (validationError != null)
			{
				return BadRequest(new { Error = validationError });
			}

			model.DutyId = 0; // Ensure the ID is zero for new entries

			// Add the new duty to the database
			_context.Duties.Add(model);
			await _context.SaveChangesAsync();

			// Set NotMapped properties
			model.SetNotMapped();

			// Return the created duty with a 201 status
			return CreatedAtAction(nameof(GetDuty), new { id = model.DutyId }, model);
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { Error = "An error occurred while creating the duty.", Details = ex.Message });
		}
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> UpdateDuty(long id, DutyModel model)
	{
		try
		{
			if (id != model.DutyId)
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
			if (!await _context.Duties.AnyAsync(e => e.DutyId == id))
			{
				return NotFound(new { Error = "Duty not found." });
			}
			else
			{
				throw;
			}
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { Error = "An error occurred while updating the duty.", Details = ex.Message });
		}
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteDuty(long id)
	{
		try
		{
			var duty = await _context.Duties.FindAsync(id);
			if (duty == null)
			{
				return NotFound(new { Error = "Duty not found." });
			}

			_context.Duties.Remove(duty);
			await _context.SaveChangesAsync();

			return NoContent();
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { Error = "An error occurred while deleting the duty.", Details = ex.Message });
		}
	}
}
