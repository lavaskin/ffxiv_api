using ffxiv_api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ffxiv_api.Tests;

/// <summary>
/// An in-memory SQLite database built from the real EF model. Use a fresh context for each
/// arrange/act/assert step so change tracking can't hide bugs.
///
/// Differences from SQL Server to keep in mind: string comparisons and ordering are
/// case-sensitive/ordinal here, while the real columns use a case-insensitive collation.
/// </summary>
public sealed class TestDatabase : IDisposable
{
	private readonly SqliteConnection _connection;

	public TestDatabase()
	{
		_connection = new SqliteConnection("DataSource=:memory:");
		_connection.Open();

		using var context = CreateContext();
		context.Database.EnsureCreated();
	}

	public AppDbContext CreateContext()
	{
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseSqlite(_connection)
			.Options;

		return new AppDbContext(options);
	}

	public void Seed(params object[] entities)
	{
		using var context = CreateContext();
		context.AddRange(entities);
		context.SaveChanges();
	}

	public void Dispose() => _connection.Dispose();
}

public sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
	public override DateTimeOffset GetUtcNow() => now;
}
