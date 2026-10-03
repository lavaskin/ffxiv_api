namespace ffxiv_api.Models.Enums;

/// <summary>
/// A job's combat role with DPS split into its three sub-roles. See <see cref="JobEnumExtensions.GetSubRole"/>.
/// <see cref="JobRoleEnum"/> is the coarser Tank / Healer / DPS grouping the stats report on.
/// </summary>
public enum JobSubRoleEnum
{
	Tank = 0,
	Healer = 1,
	MeleeDps = 2,
	MagicalRangedDps = 3,
	PhysicalRangedDps = 4,
}

public static class JobSubRoleEnumExtensions
{
	public static string GetLabel(this JobSubRoleEnum subRole)
	{
		return subRole switch
		{
			JobSubRoleEnum.Tank              => "Tank",
			JobSubRoleEnum.Healer            => "Healer",
			JobSubRoleEnum.MeleeDps          => "Melee DPS",
			JobSubRoleEnum.MagicalRangedDps  => "Magical Ranged DPS",
			JobSubRoleEnum.PhysicalRangedDps => "Physical Ranged DPS",
			_ => subRole.ToString()
		};
	}

	/// <summary>
	/// The coarser role: the melee, magical ranged and physical ranged sub-roles are all <see cref="JobRoleEnum.Dps"/>.
	/// </summary>
	public static JobRoleEnum GetRole(this JobSubRoleEnum subRole)
	{
		return subRole switch
		{
			JobSubRoleEnum.Tank   => JobRoleEnum.Tank,
			JobSubRoleEnum.Healer => JobRoleEnum.Healer,
			_ => JobRoleEnum.Dps
		};
	}
}
