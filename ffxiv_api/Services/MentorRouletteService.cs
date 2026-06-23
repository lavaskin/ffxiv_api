using ffxiv_api.Data;
using ffxiv_api.Models.DTOs;
using ffxiv_api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ffxiv_api.Services;

public class MentorRouletteService
{
	public async Task<int?> GetNextSortOrderAsync(AppDbContext context)
	{
		try
		{
			var maxSortOrder = await context.MentorRouletteLogs.MaxAsync(log => (int?)log.SortOrder) ?? 0;
			return maxSortOrder + 1;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error getting next sort order: {ex.Message}");
			return null;
		}
	}

	public async Task<MentorRouletteStats> GetStatsAsync(AppDbContext context)
	{
		var logs = await context.MentorRouletteLogs
			.Include(log => log.DutyModel)
			.ToListAsync();

		var chartEligibleLogs = logs
			.Where(log =>
				log.DutyModel?.ExpansionId != null &&
				log.DutyModel.DutyTypeId != null &&
				log.DutyModel.DutyTypeId != (long)DutyTypeEnum.Guildhest)
			.ToList();

		var completedRoulettes = logs.Count(log => log.Completed);
		var extremeTrialLogs = logs
			.Where(log => log.DutyModel?.DutyTypeId == (long)DutyTypeEnum.ExtremeTrial)
			.ToList();

		var topSeenDuties = logs
			.Where(log => log.DutyModel != null)
			.GroupBy(log => log.DutyModel!.Name)
			.OrderByDescending(group => group.Count())
			.ThenBy(group => group.Key)
			.Take(3)
			.Select(group => new SeenDutyStat
			{
				DutyName = group.Key,
				Count = group.Count(),
			})
			.ToList();

		var topPlayedJobs = logs
			.Where(log =>
				log.PlayedJobId.HasValue &&
				Enum.IsDefined(typeof(JobEnum), (int)log.PlayedJobId.Value))
			.GroupBy(log => (JobEnum)log.PlayedJobId!.Value)
			.OrderByDescending(group => group.Count())
			.ThenBy(group => group.Key)
			.Take(3)
			.Select(group => new PlayedJobStat
			{
				JobLabel = group.Key.GetLabel(),
				Count = group.Count(),
			})
			.ToList();

		var trackedDutyTypes = Enum
			.GetValues<DutyTypeEnum>()
			.Where(dutyType => dutyType != DutyTypeEnum.Guildhest)
			.ToList();

		var dutyExpansionBreakdown = chartEligibleLogs
			.GroupBy(log => (ExpansionEnum)log.DutyModel!.ExpansionId!.Value)
			.OrderBy(group => group.Key)
			.Select(group => new DutyExpansionBreakdownStat
			{
				ExpansionLabel = group.Key.GetLabel(),
				DutyTypes = trackedDutyTypes
					.Select(dutyType => new DutyTypeBreakdownStat
					{
						DutyTypeLabel = dutyType.GetLabel(),
						Count = group.Count(log => (DutyTypeEnum)log.DutyModel!.DutyTypeId!.Value == dutyType),
					})
					.Where(stat => stat.Count > 0)
					.ToList(),
			})
			.ToList();

		return new MentorRouletteStats
		{
			TotalRuns = logs.Count,
			CompletedRoulettes = completedRoulettes,
			AchievementProgressPercent = Math.Clamp((int)Math.Round(completedRoulettes * 100.0 / 2000), 0, 100),
			TopSeenDuties = topSeenDuties,
			TopPlayedJobs = topPlayedJobs,
			TotalFailedDuties = logs.Count(log => !log.Completed),
			NumberExtremeTrials = extremeTrialLogs.Count,
			ExtremeTrialClearPercent = extremeTrialLogs.Count == 0
				? 0
				: (int)Math.Round(extremeTrialLogs.Count(log => log.Completed) * 100.0 / extremeTrialLogs.Count),
			DutyExpansionBreakdown = dutyExpansionBreakdown,
		};
	}
}
