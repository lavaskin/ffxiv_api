namespace ffxiv_api.Services;

/// <summary>
/// Enum labels only exist in C#, so a text search over a label column is resolved here to the
/// matching enum values, which the database then filters with <c>IN (...)</c>.
/// </summary>
public static class EnumLabelSearch
{
	/// <returns>Every value whose label contains <paramref name="term"/>, ignoring case</returns>
	public static List<TEnum> ValuesMatching<TEnum>(string term, Func<TEnum, string> getLabel)
		where TEnum : struct, Enum
	{
		return Enum.GetValues<TEnum>()
			.Where(value => getLabel(value).Contains(term, StringComparison.OrdinalIgnoreCase))
			.ToList();
	}
}
