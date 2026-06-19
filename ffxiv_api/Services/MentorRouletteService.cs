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

		var mostCommonExpansion = logs
			.Where(log =>
				log.DutyModel?.ExpansionId != null &&
				log.DutyModel.DutyTypeId != (long)DutyTypeEnum.Guildhest)
			.GroupBy(log => (ExpansionEnum)log.DutyModel!.ExpansionId!.Value)
			.OrderByDescending(group => group.Count())
			.ThenBy(group => group.Key)
			.Select(group => group.Key.GetLabel())
			.FirstOrDefault() ?? string.Empty;

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

		return new MentorRouletteStats
		{
			TotalRuns = logs.Count,
			CompletedRoulettes = completedRoulettes,
			AchievementProgressPercent = Math.Clamp((int)Math.Round(completedRoulettes * 100.0 / 2000), 0, 100),
			TopSeenDuties = topSeenDuties,
			MostCommonExpansion = mostCommonExpansion,
			TopPlayedJobs = topPlayedJobs,
			TotalFailedDuties = logs.Count(log => !log.Completed),
			NumberExtremeTrials = extremeTrialLogs.Count,
			ExtremeTrialClearPercent = extremeTrialLogs.Count == 0
				? 0
				: (int)Math.Round(extremeTrialLogs.Count(log => log.Completed) * 100.0 / extremeTrialLogs.Count),
		};
	}
}
