using ffxiv_api.Models.Entity;
using ffxiv_api.Models.Enums;

namespace ffxiv_api.Tests;

/// <summary>
/// Entity factories shared by the tests. Pass only what the test is about and let the rest default.
/// </summary>
public static class TestData
{
	public static Duty Duty(
		string name,
		DutyTypeEnum dutyType = DutyTypeEnum.Dungeon,
		ExpansionEnum expansion = ExpansionEnum.ARealmReborn,
		long level = 50) => new()
	{
		Name = name,
		DutyType = dutyType,
		Expansion = expansion,
		LevelRequirement = level,
	};
}
