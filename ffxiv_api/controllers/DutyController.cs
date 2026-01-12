using Microsoft.AspNetCore.Mvc;
using ffxiv_api.Data;
using ffxiv_api.Models.Entity;

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
		throw new NotImplementedException();
	}

    [HttpGet("{id}")]
	public async Task<IActionResult> GetDuty(long id)
	{
		// Implementation to retrieve Duty by id
		throw new NotImplementedException();
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

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteDuty(long id)
	{
		// Implementation to delete a Duty by id
		throw new NotImplementedException();
	}
}
