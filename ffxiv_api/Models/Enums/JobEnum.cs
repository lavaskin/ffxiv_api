namespace ffxiv_api.Models.Enums;

public enum JobEnum
{
	// Tanks
	Paladin = 0,
	Warrior = 1,
	DarkKnight = 2,
	Gunbreaker = 3,

	// Healers
	WhiteMage = 100,
	Scholar = 101,
	Astrologian = 102,
	Sage = 103,

	// Melee DPS
	Monk = 200,
	Dragoon = 201,
	Ninja = 202,
	Samurai = 203,
	Reaper = 204,
	Viper = 205,

	// Magical Ranged DPS
	BlackMage = 300,
	Summoner = 301,
	RedMage = 302,
	Pictomancer = 303,

	// Phys. Ranged DPS
	Bard = 400,
	Machinist = 401,
	Dancer = 402,

	// Limited Jobs
	BlueMage = 503,
	BeastMaster = 504,
}

public static class JobEnumExtensions
{
	public static string GetLabel(this JobEnum job)
	{
		return job switch
		{
			JobEnum.Paladin => "Paladin",
			JobEnum.Warrior => "Warrior",
			JobEnum.DarkKnight => "Dark Knight",
			JobEnum.Gunbreaker => "Gunbreaker",
			
			JobEnum.WhiteMage => "White Mage",
			JobEnum.Scholar => "Scholar",
			JobEnum.Astrologian => "Astrologian",
			JobEnum.Sage => "Sage",
			
			JobEnum.Monk => "Monk",
			JobEnum.Dragoon => "Dragoon",
			JobEnum.Ninja => "Ninja",
			JobEnum.Samurai => "Samurai",
			JobEnum.Reaper => "Reaper",
			JobEnum.Viper => "Viper",
			
			JobEnum.BlackMage => "Black Mage",
			JobEnum.Summoner => "Summoner",
			JobEnum.RedMage => "Red Mage",
			JobEnum.Pictomancer => "Pictomancer",
			
			JobEnum.Bard => "Bard",
			JobEnum.Machinist => "Machinist",
			JobEnum.Dancer => "Dancer",
			
			JobEnum.BlueMage => "Blue Mage",
			JobEnum.BeastMaster => "Beast Master",
			
			_ => "Unknown"
		};
	}
}
