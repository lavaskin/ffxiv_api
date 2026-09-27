using ffxiv_api.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ffxiv_api.Data.Configurations;

/// <summary>
/// The schema is managed outside EF (there are no migrations), so this must mirror the real
/// <c>mentor_roulette_log</c> table by hand.
/// </summary>
public class MentorRouletteLogConfiguration : IEntityTypeConfiguration<MentorRouletteLog>
{
	public void Configure(EntityTypeBuilder<MentorRouletteLog> entity)
	{
		entity.ToTable("mentor_roulette_log");
		entity.HasKey(l => l.MentorRouletteLogId);

		entity.Property(l => l.PlayedJob)
			.HasColumnName("PlayedJobId")
			.HasConversion<long>()
			.IsRequired();

		entity.Property(l => l.SortOrder).IsRequired();

		// Nullable in the database, but always written as non-null by this API
		entity.Property(l => l.Notes).IsRequired();
		entity.Property(l => l.DatePlayed)
			.HasColumnType("datetime")
			.IsRequired();

		entity.HasOne(l => l.Duty)
			.WithMany()
			.HasForeignKey(l => l.DutyId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
