namespace ffxiv_api.Models.Enums;

public enum ExpansionEnum
{
	BaseGame = 0,
	ARealmReborn = 1,
	Heavensward = 2,
	Stormblood = 3,
	Shadowbringers = 4,
	Endwalker = 5,
	Dawntrail = 6,
}

public static class ExpansionEnumExtensions
{
	public static string GetLabel(this ExpansionEnum expansion)
	{
		return expansion switch
		{
			
			ExpansionEnum.BaseGame       => "1.0 Base Game",
			ExpansionEnum.ARealmReborn   => "2.0: A Realm Reborn",
			ExpansionEnum.Heavensward    => "3.0: Heavensward",
			ExpansionEnum.Stormblood     => "4.0: Stormblood",
			ExpansionEnum.Shadowbringers => "5.0: Shadowbringers",
			ExpansionEnum.Endwalker      => "6.0: Endwalker",
			ExpansionEnum.Dawntrail      => "7.0: Dawntrail",
			_ => expansion.ToString()
		};
	}

	/// <summary>
	/// The inclusive range of duty level requirements belonging to an expansion, or null if duties
	/// can't belong to it (1.0 content no longer exists in the game).
	/// </summary>
	public static (int Min, int Max)? GetLevelRange(this ExpansionEnum expansion)
	{
		return expansion switch
		{
			ExpansionEnum.ARealmReborn   => (1, 50),
			ExpansionEnum.Heavensward    => (51, 60),
			ExpansionEnum.Stormblood     => (61, 70),
			ExpansionEnum.Shadowbringers => (71, 80),
			ExpansionEnum.Endwalker      => (81, 90),
			ExpansionEnum.Dawntrail      => (91, 100),
			_ => null
		};
	}
}
