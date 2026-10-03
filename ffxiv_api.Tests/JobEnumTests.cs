using ffxiv_api.Models.Enums;

namespace ffxiv_api.Tests;

public class JobEnumTests
{
	[Theory]
	[InlineData(JobEnum.Monk, JobSubRoleEnum.MeleeDps)]
	[InlineData(JobEnum.Pictomancer, JobSubRoleEnum.MagicalRangedDps)]
	[InlineData(JobEnum.Dancer, JobSubRoleEnum.PhysicalRangedDps)]
	public void Dps_jobs_have_their_sub_role(JobEnum job, JobSubRoleEnum expected)
	{
		Assert.Equal(expected, job.GetSubRole());
	}

	[Fact]
	public void Only_limited_jobs_have_no_sub_role()
	{
		var withoutSubRole = Enum.GetValues<JobEnum>().Where(job => job.GetSubRole() is null);

		Assert.Equal([JobEnum.BlueMage, JobEnum.BeastMaster], withoutSubRole);
	}

	[Theory]
	[InlineData(JobEnum.Gunbreaker, JobRoleEnum.Tank)]
	[InlineData(JobEnum.Sage, JobRoleEnum.Healer)]
	[InlineData(JobEnum.Viper, JobRoleEnum.Dps)]
	[InlineData(JobEnum.Pictomancer, JobRoleEnum.Dps)]
	[InlineData(JobEnum.Dancer, JobRoleEnum.Dps)]
	[InlineData(JobEnum.BlueMage, JobRoleEnum.Dps)]
	public void Role_merges_the_dps_sub_roles_and_counts_limited_jobs_as_dps(JobEnum job, JobRoleEnum expected)
	{
		Assert.Equal(expected, job.GetRole());
	}
}
