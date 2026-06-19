using ffxiv_api.Models.Enums;

public static class DutyHelper
{
	public static bool VerifyDutyMeetsLevelRequirement(long expansionId, long levelRequirement)
	{
		int min, max = 0;

		if (expansionId == (int)ExpansionEnum.ARealmReborn)
		{
			min = 1;
			max = 50;
		}
		else if (expansionId == (int)ExpansionEnum.Heavensward)
		{
			min = 51;
			max = 60;
		}
		else if (expansionId == (int)ExpansionEnum.Stormblood)
		{
			min = 61;
			max = 70;
		}
		else if (expansionId == (int)ExpansionEnum.Shadowbringers)
		{
			min = 71;
			max = 80;
		}
		else if (expansionId == (int)ExpansionEnum.Endwalker)
		{
			min = 81;
			max = 90;
		}
		else if (expansionId == (int)ExpansionEnum.Dawntrail)
		{
			min = 91;
			max = 100;
		}
		else
		{
			return false;
		}

		return levelRequirement >= min && levelRequirement <= max;
	}
}
