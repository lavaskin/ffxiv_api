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
}
