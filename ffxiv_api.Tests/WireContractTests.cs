using System.Text.Json;
using System.Text.Json.Nodes;
using ffxiv_api.Models.DTOs;
using ffxiv_api.Models.Entity;
using ffxiv_api.Models.Enums;

namespace ffxiv_api.Tests;

/// <summary>
/// Pins the JSON shapes the mentor-roulette-tracker Angular client depends on. Uses the same
/// serializer defaults as ASP.NET Core (camelCase, case-insensitive reads, numeric enums).
/// </summary>
public class WireContractTests
{
	private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

	private static readonly Duty Sastasha = new()
	{
		DutyId = 1,
		Name = "Sastasha",
		DutyType = DutyTypeEnum.Dungeon,
		Expansion = ExpansionEnum.ARealmReborn,
		LevelRequirement = 15,
	};

	[Fact]
	public void Duty_response_has_the_fields_the_client_reads()
	{
		var json = JsonSerializer.SerializeToNode(DutyResponse.FromEntity(Sastasha), Web)!.AsObject();

		Assert.Equal(1, (long)json["dutyId"]!);
		Assert.Equal("Sastasha", (string)json["name"]!);
		Assert.Equal((int)DutyTypeEnum.Dungeon, (int)json["dutyType"]!);
		Assert.Equal("Dungeon", (string)json["dutyTypeLabel"]!);
		Assert.Equal((int)ExpansionEnum.ARealmReborn, (int)json["expansion"]!);
		Assert.Equal("2.0: A Realm Reborn", (string)json["expansionLabel"]!);
		Assert.Equal(15, (long)json["levelRequirement"]!);
	}

	[Fact]
	public void Log_response_keeps_the_legacy_dutyModel_and_playedJobId_names()
	{
		var log = new MentorRouletteLog
		{
			MentorRouletteLogId = 7,
			DutyId = 1,
			Duty = Sastasha,
			SortOrder = 3,
			PlayedJob = JobEnum.Sage,
			Notes = "gg",
			DatePlayed = new DateTime(2026, 9, 1, 12, 0, 0),
		};

		var json = JsonSerializer.SerializeToNode(MentorRouletteLogResponse.FromEntity(log), Web)!.AsObject();

		Assert.Equal("Sastasha", (string)json["dutyModel"]!["name"]!);
		Assert.Equal("Dungeon", (string)json["dutyModel"]!["dutyTypeLabel"]!);
		Assert.Equal((int)JobEnum.Sage, (int)json["playedJobId"]!);
		Assert.Equal("Sage", (string)json["playedJobLabel"]!);
		Assert.Equal(7, (long)json["mentorRouletteLogId"]!);
		Assert.Equal(3, (int)json["sortOrder"]!);
		Assert.False(json.ContainsKey("duty"));
		Assert.False(json.ContainsKey("playedJob"));
	}

	[Fact]
	public void Duty_request_binds_the_body_the_client_sends()
	{
		const string body = """{"dutyId":5,"name":"Sastasha","dutyType":0,"expansion":1,"levelRequirement":15}""";

		var request = JsonSerializer.Deserialize<DutyRequest>(body, Web)!;

		Assert.Equal(new DutyRequest
		{
			Name = "Sastasha",
			DutyType = DutyTypeEnum.Dungeon,
			Expansion = ExpansionEnum.ARealmReborn,
			LevelRequirement = 15,
		}, request);
	}

	/// <summary>
	/// The client PUTs back the whole log it received, including a stale <c>playedJob</c> after
	/// <c>playedJobId</c>. The old entity-bound API let the stale value overwrite the edit.
	/// </summary>
	[Fact]
	public void Log_update_uses_playedJobId_and_ignores_stale_and_server_owned_fields()
	{
		const string body = """
			{
				"mentorRouletteLogId": 7, "dutyId": 1, "sortOrder": 999, "playedJobId": 103,
				"completed": false, "replacement": true, "notes": "edited",
				"datePlayed": "2000-01-01T00:00:00", "playedJob": 0, "playedJobLabel": "Paladin"
			}
			""";

		var request = JsonSerializer.Deserialize<MentorRouletteLogRequest>(body, Web)!;

		Assert.Equal(new MentorRouletteLogRequest
		{
			DutyId = 1,
			PlayedJob = JobEnum.Sage,
			Completed = false,
			Replacement = true,
			Notes = "edited",
		}, request);
	}

	[Fact]
	public void Paged_response_has_the_fields_the_client_grid_reads()
	{
		var page = new PagedResponse<DutyResponse>
		{
			Items = [DutyResponse.FromEntity(Sastasha)],
			Page = 2,
			PageSize = 50,
			TotalCount = 51,
		};

		var json = JsonSerializer.SerializeToNode(page, Web)!.AsObject();

		Assert.Equal("Sastasha", (string)json["items"]![0]!["name"]!);
		Assert.Equal(2, (int)json["page"]!);
		Assert.Equal(50, (int)json["pageSize"]!);
		Assert.Equal(51, (int)json["totalCount"]!);
		Assert.Equal(4, json.Count);
	}

	[Fact]
	public void Error_body_uses_the_error_field_the_client_toast_reads()
	{
		var json = JsonSerializer.Serialize(new ErrorResponse("Duty not found."), Web);

		Assert.Equal("""{"error":"Duty not found."}""", json);
	}
}
