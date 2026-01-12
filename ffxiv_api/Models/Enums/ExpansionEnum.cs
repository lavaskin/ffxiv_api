namespace ffxiv_api.Models.Enums;

public enum ExpansionEnum
{
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
			ExpansionEnum.ARealmReborn => "A Realm Reborn",
			ExpansionEnum.Heavensward => "Heavensward",
			ExpansionEnum.Stormblood => "Stormblood",
			ExpansionEnum.Shadowbringers => "Shadowbringers",
			ExpansionEnum.Endwalker => "Endwalker",
			ExpansionEnum.Dawntrail => "Dawntrail",
			_ => expansion.ToString()
		};
	}
}
