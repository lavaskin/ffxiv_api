using ffxiv_api.Models.DTOs;
using ffxiv_api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ffxiv_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MentorRouletteController(MentorRouletteService mentorRouletteService) : ControllerBase
{
	/// <summary>
	/// One page of the roulettes grid. See <see cref="GridRequest"/> for the query string.
	/// </summary>
	[HttpGet]
	public async Task<ActionResult<PagedResponse<MentorRouletteLogResponse>>> GetMentorRouletteLogs([FromQuery] MentorRouletteLogGridRequest request)
	{
		var result = await mentorRouletteService.GetPageAsync(request);
		return result.IsSuccess ? result.Value : result.Error.ToActionResult();
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<MentorRouletteLogResponse>> GetMentorRouletteLog(long id)
	{
		var result = await mentorRouletteService.GetAsync(id);
		return result.IsSuccess ? result.Value : result.Error.ToActionResult();
	}

	[HttpPost]
	public async Task<ActionResult<MentorRouletteLogResponse>> CreateNewLog(MentorRouletteLogRequest request)
	{
		var result = await mentorRouletteService.CreateAsync(request);
		return result.IsSuccess
			? CreatedAtAction(nameof(GetMentorRouletteLog), new { id = result.Value.MentorRouletteLogId }, result.Value)
			: result.Error.ToActionResult();
	}

	[HttpPut("{id}")]
	public async Task<ActionResult<MentorRouletteLogResponse>> UpdateLog(long id, MentorRouletteLogRequest request)
	{
		var result = await mentorRouletteService.UpdateAsync(id, request);
		return result.IsSuccess ? result.Value : result.Error.ToActionResult();
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteLog(long id)
	{
		var error = await mentorRouletteService.DeleteAsync(id);
		return error is null ? NoContent() : error.ToActionResult();
	}

	[HttpGet("[action]")]
	public async Task<MentorRouletteStats> GetStats()
	{
		return await mentorRouletteService.GetStatsAsync();
	}
}
