using System.Diagnostics.CodeAnalysis;

namespace ffxiv_api.Services;

public enum ServiceErrorKind
{
	/// <summary>The request itself is invalid (400)</summary>
	Invalid,

	/// <summary>The thing being acted on doesn't exist (404)</summary>
	NotFound,

	/// <summary>The request is valid but clashes with existing data (409)</summary>
	Conflict,
}

/// <summary>
/// An expected failure that the client should be told about. Anything unexpected should just throw.
/// The global exception handler turns that into a 500.
/// </summary>
public sealed record ServiceError(ServiceErrorKind Kind, string Message)
{
	public static ServiceError Invalid(string message) => new(ServiceErrorKind.Invalid, message);

	public static ServiceError NotFound(string message) => new(ServiceErrorKind.NotFound, message);

	public static ServiceError Conflict(string message) => new(ServiceErrorKind.Conflict, message);
}

/// <summary>
/// Either a value or a <see cref="ServiceError"/>. Both convert implicitly, so services can
/// <c>return value;</c> or <c>return ServiceError.NotFound(...);</c>.
/// </summary>
public sealed class ServiceResult<T> where T : notnull
{
	private ServiceResult(T? value, ServiceError? error)
	{
		Value = value;
		Error = error;
	}

	public T? Value { get; }

	public ServiceError? Error { get; }

	[MemberNotNullWhen(true, nameof(Value))]
	[MemberNotNullWhen(false, nameof(Error))]
	public bool IsSuccess => Error is null;

	public static implicit operator ServiceResult<T>(T value) => new(value, null);

	public static implicit operator ServiceResult<T>(ServiceError error) => new(default, error);
}
