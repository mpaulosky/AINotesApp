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
/// </summary>
[ExcludeFromCodeCoverage]
public class DatabaseFixture : IDisposable
{

	public DatabaseFixture()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;

		Context = new ApplicationDbContext(options);

		// Ensure database is created
		Context.Database.EnsureCreated();
	}

	public ApplicationDbContext Context { get; private set; }

	public void Dispose()
	{
		Context?.Dispose();
	}

	public ApplicationDbContext CreateNewContext()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;

		var context = new ApplicationDbContext(options);
		context.Database.EnsureCreated();

		return context;
	}

}