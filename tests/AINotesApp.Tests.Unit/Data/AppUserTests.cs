// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     AppUserTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;

using AINotesApp.Data;

using FluentAssertions;

namespace AINotesApp.Tests.Unit.Data;

/// <summary>
///   Unit tests for AppUser entity.
/// </summary>
[ExcludeFromCodeCoverage]
public class AppUserTests
{

	/// <summary>
	///   Verifies that the default values of a new AppUser are set correctly.
	/// </summary>
	[Fact]
	public void AppUser_DefaultValues_AreSetCorrectly()
	{
		// Given & When
		var appUser = new AppUser();

		// Then
		appUser.Auth0Subject.Should().BeNull();
		appUser.Name.Should().BeNull();
		appUser.Email.Should().BeNull();
		appUser.CreatedUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
		appUser.Notes.Should().NotBeNull();
		appUser.Notes.Should().BeEmpty();
	}

	/// <summary>
	///   Verifies that setting properties on an AppUser works as expected.
	/// </summary>
	[Fact]
	public void AppUser_SetProperties_WorksCorrectly()
	{
		// Given
		var auth0Subject = "auth0|123456789";
		var name = "John Doe";
		var email = "john.doe@example.com";
		var createdUtc = DateTime.UtcNow.AddDays(-1);

		// When
		var appUser = new AppUser
		{
			Auth0Subject = auth0Subject,
			Name = name,
			Email = email,
			CreatedUtc = createdUtc
		};

		// Then
		appUser.Auth0Subject.Should().Be(auth0Subject);
		appUser.Name.Should().Be(name);
		appUser.Email.Should().Be(email);
		appUser.CreatedUtc.Should().Be(createdUtc);
	}

	/// <summary>
	///   Verifies that Auth0Subject can be set to a valid subject string.
	/// </summary>
	[Fact]
	public void AppUser_Auth0Subject_CanBeSet()
	{
		// Given
		var auth0Subject = "auth0|987654321";

		// When
		var appUser = new AppUser
		{
			Auth0Subject = auth0Subject
		};

		// Then
		appUser.Auth0Subject.Should().Be(auth0Subject);
	}

	/// <summary>
	///   Verifies that Name can be null for AppUser.
	/// </summary>
	[Fact]
	public void AppUser_Name_CanBeNull()
	{
		// Given & When
		var appUser = new AppUser
		{
			Auth0Subject = "auth0|123",
			Name = null
		};

		// Then
		appUser.Name.Should().BeNull();
	}

	/// <summary>
	///   Verifies that Email can be null for AppUser.
	/// </summary>
	[Fact]
	public void AppUser_Email_CanBeNull()
	{
		// Given & When
		var appUser = new AppUser
		{
			Auth0Subject = "auth0|123",
			Email = null
		};

		// Then
		appUser.Email.Should().BeNull();
	}

	/// <summary>
	///   Verifies that CreatedUtc defaults to current UTC time.
	/// </summary>
	[Fact]
	public void AppUser_CreatedUtc_DefaultsToCurrentUtcTime()
	{
		// Given
		var beforeCreation = DateTime.UtcNow;

		// When
		var appUser = new AppUser();

		// Then
		var afterCreation = DateTime.UtcNow;
		appUser.CreatedUtc.Should().BeOnOrAfter(beforeCreation);
		appUser.CreatedUtc.Should().BeOnOrBefore(afterCreation);
	}

	/// <summary>
	///   Verifies that Notes collection is initialized as empty list.
	/// </summary>
	[Fact]
	public void AppUser_Notes_InitializesAsEmptyList()
	{
		// Given & When
		var appUser = new AppUser();

		// Then
		appUser.Notes.Should().NotBeNull();
		appUser.Notes.Should().BeEmpty();
		appUser.Notes.Should().BeAssignableTo<ICollection<Note>>();
	}

	/// <summary>
	///   Verifies that Notes can be added to the collection.
	/// </summary>
	[Fact]
	public void AppUser_Notes_CanAddNotes()
	{
		// Given
		var appUser = new AppUser
		{
			Auth0Subject = "auth0|123"
		};

		var note = new Note
		{
			Id = Guid.NewGuid(),
			Title = "Test Note",
			Content = "Test Content",
			OwnerSubject = appUser.Auth0Subject
		};

		// When
		appUser.Notes.Add(note);

		// Then
		appUser.Notes.Should().HaveCount(1);
		appUser.Notes.Should().Contain(note);
	}

	/// <summary>
	///   Verifies that multiple Notes can be added to the collection.
	/// </summary>
	[Fact]
	public void AppUser_Notes_CanAddMultipleNotes()
	{
		// Given
		var appUser = new AppUser
		{
			Auth0Subject = "auth0|123"
		};

		var note1 = new Note
		{
			Id = Guid.NewGuid(),
			Title = "Note 1",
			Content = "Content 1",
			OwnerSubject = appUser.Auth0Subject
		};

		var note2 = new Note
		{
			Id = Guid.NewGuid(),
			Title = "Note 2",
			Content = "Content 2",
			OwnerSubject = appUser.Auth0Subject
		};

		// When
		appUser.Notes.Add(note1);
		appUser.Notes.Add(note2);

		// Then
		appUser.Notes.Should().HaveCount(2);
		appUser.Notes.Should().Contain(new[] { note1, note2 });
	}

	/// <summary>
	///   Verifies that Notes collection can be replaced.
	/// </summary>
	[Fact]
	public void AppUser_Notes_CanBeReplaced()
	{
		// Given
		var appUser = new AppUser
		{
			Auth0Subject = "auth0|123"
		};

		var newNotes = new List<Note>
		{
			new()
			{
				Id = Guid.NewGuid(),
				Title = "New Note",
				Content = "New Content",
				OwnerSubject = appUser.Auth0Subject
			}
		};

		// When
		appUser.Notes = newNotes;

		// Then
		appUser.Notes.Should().HaveCount(1);
		appUser.Notes.Should().BeSameAs(newNotes);
	}

	/// <summary>
	///   Verifies that AppUser can be created with valid Auth0 subject formats.
	/// </summary>
	[Theory]
	[InlineData("auth0|123456789")]
	[InlineData("google-oauth2|123456789")]
	[InlineData("github|123456789")]
	[InlineData("custom-user-id")]
	public void AppUser_Auth0Subject_AcceptsValidFormats(string auth0Subject)
	{
		// Given & When
		var appUser = new AppUser
		{
			Auth0Subject = auth0Subject
		};

		// Then
		appUser.Auth0Subject.Should().Be(auth0Subject);
	}

	/// <summary>
	///   Verifies that both Name and Email can be null simultaneously.
	/// </summary>
	[Fact]
	public void AppUser_NameAndEmail_CanBothBeNull()
	{
		// Given & When
		var appUser = new AppUser
		{
			Auth0Subject = "auth0|123",
			Name = null,
			Email = null
		};

		// Then
		appUser.Name.Should().BeNull();
		appUser.Email.Should().BeNull();
	}

	/// <summary>
	///   Verifies that CreatedUtc can be set to a specific date.
	/// </summary>
	[Fact]
	public void AppUser_CreatedUtc_CanBeSetToSpecificDate()
	{
		// Given
		var specificDate = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

		// When
		var appUser = new AppUser
		{
			Auth0Subject = "auth0|123",
			CreatedUtc = specificDate
		};

		// Then
		appUser.CreatedUtc.Should().Be(specificDate);
	}

	/// <summary>
	///   Verifies that AppUser with all properties set maintains values correctly.
	/// </summary>
	[Fact]
	public void AppUser_WithAllProperties_MaintainsValues()
	{
		// Given
		var auth0Subject = "auth0|123456789";
		var name = "Jane Smith";
		var email = "jane.smith@example.com";
		var createdUtc = DateTime.UtcNow.AddDays(-7);
		var notes = new List<Note>
		{
			new()
			{
				Id = Guid.NewGuid(),
				Title = "First Note",
				Content = "First Content",
				OwnerSubject = auth0Subject
			}
		};

		// When
		var appUser = new AppUser
		{
			Auth0Subject = auth0Subject,
			Name = name,
			Email = email,
			CreatedUtc = createdUtc,
			Notes = notes
		};

		// Then
		appUser.Auth0Subject.Should().Be(auth0Subject);
		appUser.Name.Should().Be(name);
		appUser.Email.Should().Be(email);
		appUser.CreatedUtc.Should().Be(createdUtc);
		appUser.Notes.Should().HaveCount(1);
		appUser.Notes.Should().Contain(notes[0]);
	}

}
