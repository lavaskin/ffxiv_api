namespace ffxiv_api.Models.DTOs;

public class SearchOptions
{
	public string? Query { get; set; }

	public int Page { get; set; } = 1;

	public int PageSize { get; set; } = 10;
}
