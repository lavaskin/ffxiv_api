namespace ffxiv_api.Models.Enums;

public enum DutyTypeEnum
{
	Dungeon = 0,
	Trial = 1,
	ExtremeTrial = 2,
	UnrealTrial = 3,
	NormalRaid = 4,
	UltimateRaid = 5,
	AllianceRaid = 6,
	ChaoticAllianceRaid = 7,
	Guildhest = 8,
}

public static class DutyTypeEnumExtensions
{
	public static string GetLabel(this DutyTypeEnum dutyType)
	{
		return dutyType switch
		{
			DutyTypeEnum.Dungeon             => "Dungeon",
			DutyTypeEnum.Trial               => "Trial",
			DutyTypeEnum.ExtremeTrial        => "Extreme Trial",
			DutyTypeEnum.UnrealTrial         => "Unreal Trial",
			DutyTypeEnum.NormalRaid          => "Normal Raid",
			DutyTypeEnum.UltimateRaid        => "Ultimate Raid",
			DutyTypeEnum.AllianceRaid        => "Alliance Raid",
			DutyTypeEnum.ChaoticAllianceRaid => "Chaotic Alliance Raid",
			DutyTypeEnum.Guildhest           => "Guildhest",
			_ => dutyType.ToString()
		};
	}
}
