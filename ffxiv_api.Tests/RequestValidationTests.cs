using ffxiv_api.Models.DTOs;
using ffxiv_api.Models.Enums;

namespace ffxiv_api.Tests;

public class RequestValidationTests
{
	private static readonly DutyRequest ValidDuty = new()
	{
		Name = "Sastasha",
		DutyType = DutyTypeEnum.Dungeon,
		Expansion = ExpansionEnum.ARealmReborn,
		LevelRequirement = 15,
	};

	[Fact]
	public void A_valid_duty_passes()
	{
		Assert.Null(ValidDuty.Validate());
	}

	[Fact]
	public void Duty_name_is_trimmed()
	{
		Assert.Equal("Sastasha", (ValidDuty with { Name = "  Sastasha \t" }).NormalizedName);
	}

	public static TheoryData<DutyRequest, string> InvalidDuties => new()
	{
		{ ValidDuty with { Name = null }, "Name cannot be empty." },
		{ ValidDuty with { Name = "   " }, "Name cannot be empty." },
		{ ValidDuty with { Name = new string('x', 129) }, "Name cannot be longer than 128 characters." },
		{ ValidDuty with { DutyType = null }, "A valid duty type must be specified." },
		{ ValidDuty with { DutyType = (DutyTypeEnum)99 }, "A valid duty type must be specified." },
		{ ValidDuty with { Expansion = null }, "A valid expansion must be specified." },
		{ ValidDuty with { Expansion = ExpansionEnum.BaseGame }, "A valid expansion must be specified." },
		{ ValidDuty with { Expansion = (ExpansionEnum)99 }, "A valid expansion must be specified." },
		{ ValidDuty with { LevelRequirement = 0 }, "The level requirement must be between 1 and 50 for 2.0: A Realm Reborn." },
		{ ValidDuty with { LevelRequirement = 51 }, "The level requirement must be between 1 and 50 for 2.0: A Realm Reborn." },
		{ ValidDuty with { Expansion = ExpansionEnum.Dawntrail, LevelRequirement = 90 }, "The level requirement must be between 91 and 100 for 7.0: Dawntrail." },
	};

	[Theory]
	[MemberData(nameof(InvalidDuties))]
	public void Invalid_duties_are_rejected_with_a_readable_message(DutyRequest request, string expectedError)
	{
		Assert.Equal(expectedError, request.Validate());
	}

	[Theory]
	[InlineData(ExpansionEnum.ARealmReborn, 1, 50)]
	[InlineData(ExpansionEnum.Heavensward, 51, 60)]
	[InlineData(ExpansionEnum.Stormblood, 61, 70)]
	[InlineData(ExpansionEnum.Shadowbringers, 71, 80)]
	[InlineData(ExpansionEnum.Endwalker, 81, 90)]
	[InlineData(ExpansionEnum.Dawntrail, 91, 100)]
	public void Level_range_boundaries_are_inclusive(ExpansionEnum expansion, int min, int max)
	{
		var request = ValidDuty with { Expansion = expansion };

		Assert.Null((request with { LevelRequirement = min }).Validate());
		Assert.Null((request with { LevelRequirement = max }).Validate());
		Assert.NotNull((request with { LevelRequirement = min - 1 }).Validate());
		Assert.NotNull((request with { LevelRequirement = max + 1 }).Validate());
	}

	[Fact]
	public void A_valid_log_passes()
	{
		Assert.Null(new MentorRouletteLogRequest { DutyId = 1, PlayedJob = JobEnum.Sage }.Validate());
	}

	[Theory]
	[InlineData(0, (int)JobEnum.Sage, "Must have an associated duty")]
	[InlineData(1, null, "Must have a valid played job")]
	[InlineData(1, 999, "Must have a valid played job")]
	public void Invalid_logs_are_rejected(long dutyId, int? jobId, string expectedError)
	{
		var request = new MentorRouletteLogRequest { DutyId = dutyId, PlayedJob = (JobEnum?)jobId };

		Assert.Equal(expectedError, request.Validate());
	}

	private static readonly DateTimeOffset March1 = new(2026, 3, 1, 5, 0, 0, TimeSpan.Zero);

	[Fact]
	public void A_roulette_grid_request_using_every_filter_passes()
	{
		var request = new MentorRouletteLogGridRequest
		{
			Expansions = [ExpansionEnum.Endwalker],
			DutyTypes = [DutyTypeEnum.Dungeon],
			SubRoles = [JobSubRoleEnum.Healer],
			Jobs = [JobEnum.Sage],
			Completed = true,
			Replacement = false,
			PlayedFrom = March1,
			PlayedBefore = March1.AddDays(1),
		};

		Assert.Null(request.Validate());
	}

	public static TheoryData<MentorRouletteLogGridRequest, string> InvalidRouletteGridRequests => new()
	{
		{ new() { Page = 0 }, "Page must be 1 or greater." }, // the shared paging rules still run
		{ new() { PlayedFrom = March1, PlayedBefore = March1 }, "The played-from date must be before the played-before date." },
		{ new() { PlayedFrom = March1.AddDays(1), PlayedBefore = March1 }, "The played-from date must be before the played-before date." },
	};

	[Theory]
	[MemberData(nameof(InvalidRouletteGridRequests))]
	public void Invalid_roulette_grid_filters_are_rejected(MentorRouletteLogGridRequest request, string expectedError)
	{
		Assert.Equal(expectedError, request.Validate());
	}
}
