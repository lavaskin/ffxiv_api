using ffxiv_api.Models.DTOs;
using ffxiv_api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ffxiv_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DutyController(DutyService dutyService) : ControllerBase
{
	/// <summary>
	/// One page of the duties grid. See <see cref="GridRequest"/> for the query string.
	/// </summary>
	[HttpGet]
	public async Task<ActionResult<PagedResponse<DutyResponse>>> GetDuties([FromQuery] DutyGridRequest request)
	{
		var result = await dutyService.GetPageAsync(request);
		return result.IsSuccess ? result.Value : result.Error.ToActionResult();
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<DutyResponse>> GetDuty(long id)
	{
		var result = await dutyService.GetAsync(id);
		return result.IsSuccess ? result.Value : result.Error.ToActionResult();
	}

	/// <summary>
	/// Duty picker search. POST and the route name are kept for the existing client.
	/// </summary>
	[HttpPost("[action]")]
	public async Task<List<ListResultItem>> GetResultItems(SearchOptions options)
	{
		return await dutyService.SearchAsync(options);
	}

	[HttpPost]
	public async Task<ActionResult<DutyResponse>> CreateNewDuty(DutyRequest request)
	{
		var result = await dutyService.CreateAsync(request);
		return result.IsSuccess
			? CreatedAtAction(nameof(GetDuty), new { id = result.Value.DutyId }, result.Value)
			: result.Error.ToActionResult();
	}

	[HttpPut("{id}")]
	public async Task<ActionResult<DutyResponse>> UpdateDuty(long id, DutyRequest request)
	{
		var result = await dutyService.UpdateAsync(id, request);
		return result.IsSuccess ? result.Value : result.Error.ToActionResult();
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteDuty(long id)
	{
		var error = await dutyService.DeleteAsync(id);
		return error is null ? NoContent() : error.ToActionResult();
	}
}
