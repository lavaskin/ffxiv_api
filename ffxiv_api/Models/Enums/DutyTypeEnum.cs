namespace ffxiv_api.Models.Enums;

public enum DutyTypeEnum
{
	Dungeon = 1,
	Trial = 2,
	ExtremeTrial = 3,
	UnrealTrial = 4,
	NormalRaid = 5,
	UltimateRaid = 6,
	AllianceRaid = 7,
	ChaoticAllianceRaid = 8,
	Guildhest = 9,
}

public static class DutyTypeEnumExtensions
{
	public static string GetLabel(this DutyTypeEnum dutyType)
	{
		return dutyType switch
		{
			DutyTypeEnum.Dungeon => "Dungeon",
			DutyTypeEnum.Trial => "Trial",
			DutyTypeEnum.ExtremeTrial => "Extreme Trial",
			DutyTypeEnum.UnrealTrial => "Unreal Trial",
			DutyTypeEnum.NormalRaid => "Normal Raid",
			DutyTypeEnum.UltimateRaid => "Ultimate Raid",
			DutyTypeEnum.AllianceRaid => "Alliance Raid",
			DutyTypeEnum.ChaoticAllianceRaid => "Chaotic Alliance Raid",
			DutyTypeEnum.Guildhest => "Guildhest",
			_ => dutyType.ToString()
		};
	}
}
