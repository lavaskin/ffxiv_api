using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ffxiv_api.Models.DTOs;
using ffxiv_api.Models.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace ffxiv_api.Tests;

/// <summary>
/// Echoes the grid request exactly as MVC bound it, the same way <c>MentorRouletteController</c> binds it
/// </summary>
[ApiController]
[Route("binding-probe")]
public class BindingProbeController : ControllerBase
{
	[HttpGet]
	public MentorRouletteLogGridRequest Echo([FromQuery] MentorRouletteLogGridRequest request) => request;
}

/// <summary>
/// Service tests build requests in C#, so they can't catch a filter that never makes it out of the
/// query string. These go through real MVC model binding (in memory, no socket) with the query the
/// Angular client sends.
/// </summary>
public sealed class QueryBindingTests : IAsyncLifetime
{
	private WebApplication _app = null!;
	private HttpClient _client = null!;

	public async Task InitializeAsync()
	{
		var builder = WebApplication.CreateBuilder();
		builder.WebHost.UseTestServer();
		builder.Services.AddControllers().AddApplicationPart(typeof(BindingProbeController).Assembly);

		_app = builder.Build();
		_app.MapControllers();
		await _app.StartAsync();

		_client = _app.GetTestClient();
	}

	public async Task DisposeAsync()
	{
		_client.Dispose();
		await _app.DisposeAsync();
	}

	private async Task<MentorRouletteLogGridRequest> Bind(string query)
	{
		var request = await _client.GetFromJsonAsync<MentorRouletteLogGridRequest>($"binding-probe?{query}", JsonSerializerOptions.Web);
		return request!;
	}

	[Fact]
	public async Task Roulette_filters_bind_from_the_query_the_client_sends()
	{
		var request = await Bind(
			"page=1&pageSize=50&expansions=5&expansions=6&dutyTypes=0&subRoles=1&subRoles=4&jobs=103"
			+ "&completed=false&replacement=true&playedFrom=2026-03-01T05:00:00.000Z&playedBefore=2026-04-01T04:00:00.000Z");

		Assert.Equal([ExpansionEnum.Endwalker, ExpansionEnum.Dawntrail], request.Expansions!);
		Assert.Equal([DutyTypeEnum.Dungeon], request.DutyTypes!);
		Assert.Equal([JobSubRoleEnum.Healer, JobSubRoleEnum.PhysicalRangedDps], request.SubRoles!);
		Assert.Equal([JobEnum.Sage], request.Jobs!);
		Assert.False(request.Completed);
		Assert.True(request.Replacement);
		Assert.Equal(new DateTimeOffset(2026, 3, 1, 5, 0, 0, TimeSpan.Zero), request.PlayedFrom);
		Assert.Equal(new DateTimeOffset(2026, 4, 1, 4, 0, 0, TimeSpan.Zero), request.PlayedBefore);
	}

	[Fact]
	public async Task Missing_filters_bind_to_no_filter()
	{
		var request = await Bind("page=2&pageSize=25");

		Assert.Equal(2, request.Page);
		Assert.Null(request.Expansions);
		Assert.Null(request.Jobs);
		Assert.Null(request.Completed);
		Assert.Null(request.PlayedFrom);
		Assert.Null(request.Validate());
	}

	/// <summary>
	/// This is what keeps unknown ids out of the filters, so <c>Validate()</c> doesn't check them again
	/// </summary>
	[Theory]
	[InlineData("expansions=5&expansions=99")]
	[InlineData("dutyTypes=99")]
	[InlineData("subRoles=99")]
	[InlineData("jobs=99")]
	[InlineData("completed=yes")]
	public async Task Values_the_filters_cant_hold_are_rejected_by_binding(string query)
	{
		var response = await _client.GetAsync($"binding-probe?{query}");

		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}
}
