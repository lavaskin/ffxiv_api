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

	/// <summary>
	/// Collapses a job down to its combat role. The melee, magical ranged and physical
	/// ranged sub-roles are all reported as <see cref="JobRoleEnum.Dps"/>, and so are the
	/// limited jobs, which have no sub-role.
	/// </summary>
	public static JobRoleEnum GetRole(this JobEnum job)
	{
		return job.GetSubRole()?.GetRole() ?? JobRoleEnum.Dps;
	}

	/// <summary>
	/// The job's role, with DPS split into melee, magical ranged and physical ranged.
	/// Limited jobs (Blue Mage, Beast Master) have no sub-role and return null.
	/// </summary>
	public static JobSubRoleEnum? GetSubRole(this JobEnum job)
	{
		return job switch
		{
			JobEnum.Paladin => JobSubRoleEnum.Tank,
			JobEnum.Warrior => JobSubRoleEnum.Tank,
			JobEnum.DarkKnight => JobSubRoleEnum.Tank,
			JobEnum.Gunbreaker => JobSubRoleEnum.Tank,

			JobEnum.WhiteMage => JobSubRoleEnum.Healer,
			JobEnum.Scholar => JobSubRoleEnum.Healer,
			JobEnum.Astrologian => JobSubRoleEnum.Healer,
			JobEnum.Sage => JobSubRoleEnum.Healer,

			JobEnum.Monk => JobSubRoleEnum.MeleeDps,
			JobEnum.Dragoon => JobSubRoleEnum.MeleeDps,
			JobEnum.Ninja => JobSubRoleEnum.MeleeDps,
			JobEnum.Samurai => JobSubRoleEnum.MeleeDps,
			JobEnum.Reaper => JobSubRoleEnum.MeleeDps,
			JobEnum.Viper => JobSubRoleEnum.MeleeDps,

			JobEnum.BlackMage => JobSubRoleEnum.MagicalRangedDps,
			JobEnum.Summoner => JobSubRoleEnum.MagicalRangedDps,
			JobEnum.RedMage => JobSubRoleEnum.MagicalRangedDps,
			JobEnum.Pictomancer => JobSubRoleEnum.MagicalRangedDps,

			JobEnum.Bard => JobSubRoleEnum.PhysicalRangedDps,
			JobEnum.Machinist => JobSubRoleEnum.PhysicalRangedDps,
			JobEnum.Dancer => JobSubRoleEnum.PhysicalRangedDps,

			_ => null
		};
	}
}
