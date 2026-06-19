namespace ffxiv_api.Models.Enums;

public enum DutyClassificationEnum
{
	None = 0,
	/// <summary>
	/// Any DUNGEON thats level requirement isn't the max of their expansion. (Ex: Level 1-49 dungeons, Level 51-59 dungeons, etc)
	/// </summary>
	Leveling = 1,
	/// <summary>
	/// Any DUNGEON thats level requirement is the max of their expansion. (Ex: Level 50 dungeons, Level 60 dungeons, etc)
	/// </summary>
	HighLevelDungeon = 2,
	/// <summary>
	/// Dungeons part of MSQ Roulette (Ex. Castrum Meridianum , The Praetorium and Porta Decumana)
	/// </summary>
	MsqDungeon = 3,
}

public static class DutyClassificationEnumExtensions
{
	public static string GetLabel(this DutyClassificationEnum dutyClassification)
	{
		return dutyClassification switch
		{
			DutyClassificationEnum.None             => "None",
			DutyClassificationEnum.Leveling         => "Leveling",
			DutyClassificationEnum.HighLevelDungeon => "High Level Dungeon",
			DutyClassificationEnum.MsqDungeon       => "MSQ Dungeon",

			_ => dutyClassification.ToString()
		};
	}
}
