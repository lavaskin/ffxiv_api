namespace ffxiv_api.Models.Enums;

public enum JobRoleEnum
{
	Tank = 0,
	Healer = 1,

	/// <summary>
	/// All damage dealers, regardless of melee / magical ranged / physical ranged sub-role
	/// </summary>
	Dps = 2,
}

public static class JobRoleEnumExtensions
{
	public static string GetLabel(this JobRoleEnum jobRole)
	{
		return jobRole switch
		{
			JobRoleEnum.Tank   => "Tank",
			JobRoleEnum.Healer => "Healer",
			JobRoleEnum.Dps    => "DPS",
			_ => jobRole.ToString()
		};
	}
}
