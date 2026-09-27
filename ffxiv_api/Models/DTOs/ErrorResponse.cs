namespace ffxiv_api.Models.DTOs;

/// <summary>
/// Body for expected (4xx) failures: <c>{ "error": "..." }</c>. The client's toast reads the
/// <c>error</c> field. Unexpected failures return a standard ProblemDetails 500 instead.
/// </summary>
public sealed record ErrorResponse(string Error);
