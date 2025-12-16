// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     DatabaseFixture.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Integration
// =======================================================

using System.Diagnostics.CodeAnalysis;

using AINotesApp.Data;

using Microsoft.EntityFrameworkCore;

namespace AINotesApp.Tests.Integration.Database;

/// <summary>
///   Fixture for integration tests that provides an in-memory database context.
///   Each fixture instance creates a unique database to ensure test isolation.
/// </summary>
/// <remarks>
///   This fixture implements IDisposable to properly clean up database resources.
///   The Context property provides access to a shared database instance for tests
///   that can share data. Use CreateNewContext() when complete isolation is needed.
/// </remarks>
[ExcludeFromCodeCoverage]
public class DatabaseFixture : IDisposable
{

	private readonly string _databaseName;

	/// <summary>
	///   Initializes a new instance of the <see cref="DatabaseFixture" /> class.
	///   Creates a new in-memory database with a unique name.
	/// </summary>
	public DatabaseFixture()
	{
		_databaseName = $"TestDb_{Guid.NewGuid()}";

		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(_databaseName)
				.Options;

		Context = new ApplicationDbContext(options);

		// Ensure database is created
		Context.Database.EnsureCreated();
	}

	/// <summary>
	///   Gets the shared ApplicationDbContext instance for this fixture.
	/// </summary>
	/// <remarks>
	///   Use this context when tests can share data. For complete isolation
	///   between test methods, use <see cref="CreateNewContext" /> instead.
	/// </remarks>
	public ApplicationDbContext Context { get; private set; }

	/// <summary>
	///   Disposes the database context and releases resources.
	/// </summary>
	public void Dispose()
	{
		Context?.Dispose();
		GC.SuppressFinalize(this);
	}

	/// <summary>
	///   Creates a new isolated ApplicationDbContext with a unique database.
	/// </summary>
	/// <returns>A new ApplicationDbContext instance with its own in-memory database.</returns>
	/// <remarks>
	///   Use this method when complete test isolation is required. Each context
	///   will have its own separate database that won't interfere with other tests.
	///   Remember to dispose the context when done (use 'await using' or 'using').
	/// </remarks>
	public ApplicationDbContext CreateNewContext()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
				.Options;

		var context = new ApplicationDbContext(options);
		context.Database.EnsureCreated();

		return context;
	}

	/// <summary>
	///   Creates a new ApplicationDbContext that shares the same database as the fixture's Context.
	/// </summary>
	/// <returns>A new ApplicationDbContext instance connected to the shared database.</returns>
	/// <remarks>
	///   Use this method when you need a separate context instance but want to access
	///   the same data as the fixture's Context. This is useful for testing scenarios
	///   where multiple contexts need to interact with the same data.
	/// </remarks>
	public ApplicationDbContext CreateSharedContext()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(_databaseName)
				.Options;

		return new ApplicationDbContext(options);
	}

}