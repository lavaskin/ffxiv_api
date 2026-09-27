using ffxiv_api.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ffxiv_api.Data.Configurations;

/// <summary>
/// The schema is managed outside EF (there are no migrations), so this must mirror the real
/// <c>duty</c> table by hand.
/// </summary>
public class DutyConfiguration : IEntityTypeConfiguration<Duty>
{
	public const int NameMaxLength = 128;

	public void Configure(EntityTypeBuilder<Duty> entity)
	{
		entity.ToTable("duty");
		entity.HasKey(d => d.DutyId);

		entity.Property(d => d.Name)
			.IsRequired()
			.HasMaxLength(NameMaxLength);

		// Enums are stored as their numeric value in bigint columns named *Id
		entity.Property(d => d.DutyType)
			.HasColumnName("DutyTypeId")
			.HasConversion<long>();

		entity.Property(d => d.Expansion)
			.HasColumnName("ExpansionId")
			.HasConversion<long>();

		entity.Property(d => d.LevelRequirement).IsRequired();
	}
}
