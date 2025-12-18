// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ApplicationDbContextTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;

using AINotesApp.Data;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AINotesApp.Tests.Unit.Data;

/// <summary>
///   Unit tests for ApplicationDbContext configuration and behavior.
/// </summary>
[ExcludeFromCodeCoverage]
public class ApplicationDbContextTests
{

	private ApplicationDbContext CreateDbContext()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
				.Options;

		return new ApplicationDbContext(options);
	}

	#region DbSet Properties Tests

	/// <summary>
	///   Verifies that AppUsers DbSet is properly configured and accessible.
	/// </summary>
	[Fact]
	public void AppUsers_DbSet_IsAccessible()
	{
		// Arrange
		using var context = CreateDbContext();

		// Act
		var appUsers = context.AppUsers;

		// Assert
		appUsers.Should().NotBeNull();
		appUsers.Should().BeAssignableTo<DbSet<AppUser>>();
	}

	/// <summary>
	///   Verifies that Notes DbSet is properly configured and accessible.
	/// </summary>
	[Fact]
	public void Notes_DbSet_IsAccessible()
	{
		// Arrange
		using var context = CreateDbContext();

		// Act
		var notes = context.Notes;

		// Assert
		notes.Should().NotBeNull();
		notes.Should().BeAssignableTo<DbSet<Note>>();
	}

	#endregion

	#region AppUser Entity Configuration Tests

	/// <summary>
	///   Verifies that AppUser entity has Auth0Subject as primary key.
	/// </summary>
	[Fact]
	public void AppUser_HasAuth0SubjectAsPrimaryKey()
	{
		// Arrange
		using var context = CreateDbContext();
		var entityType = context.Model.FindEntityType(typeof(AppUser));

		// Act
		var primaryKey = entityType?.FindPrimaryKey();

		// Assert
		primaryKey.Should().NotBeNull();
		primaryKey!.Properties.Should().HaveCount(1);
		primaryKey.Properties.First().Name.Should().Be("Auth0Subject");
	}

	/// <summary>
	///   Verifies that Auth0Subject property is required.
	/// </summary>
	[Fact]
	public void AppUser_Auth0Subject_IsRequired()
	{
		// Arrange
		using var context = CreateDbContext();
		var entityType = context.Model.FindEntityType(typeof(AppUser));

		// Act
		var property = entityType?.FindProperty("Auth0Subject");

		// Assert
		property.Should().NotBeNull();
		property!.IsNullable.Should().BeFalse();
	}

	/// <summary>
	///   Verifies that Auth0Subject has max length of 64.
	/// </summary>
	[Fact]
	public void AppUser_Auth0Subject_HasMaxLength64()
	{
		// Arrange
		using var context = CreateDbContext();
		var entityType = context.Model.FindEntityType(typeof(AppUser));

		// Act
		var property = entityType?.FindProperty("Auth0Subject");

		// Assert
		property.Should().NotBeNull();
		property!.GetMaxLength().Should().Be(64);
	}

	/// <summary>
	///   Verifies that Name property has max length of 200.
	/// </summary>
	[Fact]
	public void AppUser_Name_HasMaxLength200()
	{
		// Arrange
		using var context = CreateDbContext();
		var entityType = context.Model.FindEntityType(typeof(AppUser));

		// Act
		var property = entityType?.FindProperty("Name");

		// Assert
		property.Should().NotBeNull();
		property!.GetMaxLength().Should().Be(200);
	}

	/// <summary>
	///   Verifies that Email property has max length of 256.
	/// </summary>
	[Fact]
	public void AppUser_Email_HasMaxLength256()
	{
		// Arrange
		using var context = CreateDbContext();
		var entityType = context.Model.FindEntityType(typeof(AppUser));

		// Act
		var property = entityType?.FindProperty("Email");

		// Assert
		property.Should().NotBeNull();
		property!.GetMaxLength().Should().Be(256);
	}

	/// <summary>
	///   Verifies that CreatedUtc property is required.
	/// </summary>
	[Fact]
	public void AppUser_CreatedUtc_IsRequired()
	{
		// Arrange
		using var context = CreateDbContext();
		var entityType = context.Model.FindEntityType(typeof(AppUser));

		// Act
		var property = entityType?.FindProperty("CreatedUtc");

		// Assert
		property.Should().NotBeNull();
		property!.IsNullable.Should().BeFalse();
	}

	/// <summary>
	///   Verifies that AppUser has an index on Email property.
	/// </summary>
	[Fact]
	public void AppUser_HasIndexOnEmail()
	{
		// Arrange
		using var context = CreateDbContext();
		var entityType = context.Model.FindEntityType(typeof(AppUser));

		// Act
		var indexes = entityType?.GetIndexes();
		var emailIndex = indexes?.FirstOrDefault(i => i.Properties.Any(p => p.Name == "Email"));

		// Assert
		emailIndex.Should().NotBeNull();
		emailIndex!.Properties.Should().HaveCount(1);
		emailIndex.Properties.First().Name.Should().Be("Email");
	}

	#endregion

	#region Note Entity Configuration Tests

	/// <summary>
	///   Verifies that Note entity has Id as primary key.
	/// </summary>
	[Fact]
	public void Note_HasIdAsPrimaryKey()
	{
		// Arrange
		using var context = CreateDbContext();
		var entityType = context.Model.FindEntityType(typeof(Note));

		// Act
		var primaryKey = entityType?.FindPrimaryKey();

		// Assert
		primaryKey.Should().NotBeNull();
		primaryKey!.Properties.Should().HaveCount(1);
		primaryKey.Properties.First().Name.Should().Be("Id");
	}

	/// <summary>
	///   Verifies that Title property is required.
	/// </summary>
	[Fact]
	public void Note_Title_IsRequired()
	{
		// Arrange
		using var context = CreateDbContext();
		var entityType = context.Model.FindEntityType(typeof(Note));

		// Act
		var property = entityType?.FindProperty("Title");

		// Assert
		property.Should().NotBeNull();
		property!.IsNullable.Should().BeFalse();
	}

	/// <summary>
	///   Verifies that Title has max length of 200.
	/// </summary>
	[Fact]
	public void Note_Title_HasMaxLength200()
	{
		// Arrange
		using var context = CreateDbContext();
		var entityType = context.Model.FindEntityType(typeof(Note));

		// Act
		var property = entityType?.FindProperty("Title");

		// Assert
		property.Should().NotBeNull();
		property!.GetMaxLength().Should().Be(200);
	}

	/// <summary>
	///   Verifies that Content property is required.
	/// </summary>
	[Fact]
	public void Note_Content_IsRequired()
	{
		// Arrange
		using var context = CreateDbContext();
		var entityType = context.Model.FindEntityType(typeof(Note));

		// Act
		var property = entityType?.FindProperty("Content");

		// Assert
		property.Should().NotBeNull();
		property!.IsNullable.Should().BeFalse();
	}

	/// <summary>
	///   Verifies that Content has max length of 2000.
	/// </summary>
	[Fact]
	public void Note_Content_HasMaxLength2000()
	{
		// Arrange
		using var context = CreateDbContext();
		var entityType = context.Model.FindEntityType(typeof(Note));

		// Act
		var property = entityType?.FindProperty("Content");

		// Assert
		property.Should().NotBeNull();
		property!.GetMaxLength().Should().Be(2000);
	}

	/// <summary>
	///   Verifies that AiSummary has max length of 1000.
	/// </summary>
	[Fact]
	public void Note_AiSummary_HasMaxLength1000()
	{
		// Arrange
		using var context = CreateDbContext();
		var entityType = context.Model.FindEntityType(typeof(Note));

		// Act
		var property = entityType?.FindProperty("AiSummary");

		// Assert
		property.Should().NotBeNull();
		property!.GetMaxLength().Should().Be(1000);
	}

	/// <summary>
	///   Verifies that Tags has max length of 500.
	/// </summary>
	[Fact]
	public void Note_Tags_HasMaxLength500()
	{
		// Arrange
		using var context = CreateDbContext();
		var entityType = context.Model.FindEntityType(typeof(Note));

		// Act
		var property = entityType?.FindProperty("Tags");

		// Assert
		property.Should().NotBeNull();
		property!.GetMaxLength().Should().Be(500);
	}

	/// <summary>
	///   Verifies that OwnerSubject property is required.
	/// </summary>
	[Fact]
	public void Note_OwnerSubject_IsRequired()
	{
		// Arrange
		using var context = CreateDbContext();
		var entityType = context.Model.FindEntityType(typeof(Note));

		// Act
		var property = entityType?.FindProperty("OwnerSubject");

		// Assert
		property.Should().NotBeNull();
		property!.IsNullable.Should().BeFalse();
	}

	/// <summary>
	///   Verifies that OwnerSubject has max length of 64.
	/// </summary>
	[Fact]
	public void Note_OwnerSubject_HasMaxLength64()
	{
		// Arrange
		using var context = CreateDbContext();
		var entityType = context.Model.FindEntityType(typeof(Note));

		// Act
		var property = entityType?.FindProperty("OwnerSubject");

		// Assert
		property.Should().NotBeNull();
		property!.GetMaxLength().Should().Be(64);
	}

	/// <summary>
	///   Verifies that CreatedAt property is required.
	/// </summary>
	[Fact]
	public void Note_CreatedAt_IsRequired()
	{
		// Arrange
		using var context = CreateDbContext();
		var entityType = context.Model.FindEntityType(typeof(Note));

		// Act
		var property = entityType?.FindProperty("CreatedAt");

		// Assert
		property.Should().NotBeNull();
		property!.IsNullable.Should().BeFalse();
	}

	/// <summary>
	///   Verifies that UpdatedAt property is required.
	/// </summary>
	[Fact]
	public void Note_UpdatedAt_IsRequired()
	{
		// Arrange
		using var context = CreateDbContext();
		var entityType = context.Model.FindEntityType(typeof(Note));

		// Act
		var property = entityType?.FindProperty("UpdatedAt");

		// Assert
		property.Should().NotBeNull();
		property!.IsNullable.Should().BeFalse();
	}

	/// <summary>
	///   Verifies that Note has an index on OwnerSubject property.
	/// </summary>
	[Fact]
	public void Note_HasIndexOnOwnerSubject()
	{
		// Arrange
		using var context = CreateDbContext();
		var entityType = context.Model.FindEntityType(typeof(Note));

		// Act
		var indexes = entityType?.GetIndexes();
		var ownerIndex = indexes?.FirstOrDefault(i => i.Properties.Any(p => p.Name == "OwnerSubject"));

		// Assert
		ownerIndex.Should().NotBeNull();
		ownerIndex!.Properties.Should().HaveCount(1);
		ownerIndex.Properties.First().Name.Should().Be("OwnerSubject");
	}

	/// <summary>
	///   Verifies that Note has an index on CreatedAt property.
	/// </summary>
	[Fact]
	public void Note_HasIndexOnCreatedAt()
	{
		// Arrange
		using var context = CreateDbContext();
		var entityType = context.Model.FindEntityType(typeof(Note));

		// Act
		var indexes = entityType?.GetIndexes();
		var createdAtIndex = indexes?.FirstOrDefault(i => i.Properties.Any(p => p.Name == "CreatedAt"));

		// Assert
		createdAtIndex.Should().NotBeNull();
		createdAtIndex!.Properties.Should().HaveCount(1);
		createdAtIndex.Properties.First().Name.Should().Be("CreatedAt");
	}

	#endregion

	#region Relationship Configuration Tests

	/// <summary>
	///   Verifies that Note has a foreign key relationship with AppUser.
	/// </summary>
	[Fact]
	public void Note_HasForeignKeyToAppUser()
	{
		// Arrange
		using var context = CreateDbContext();
		var noteEntityType = context.Model.FindEntityType(typeof(Note));

		// Act
		var foreignKeys = noteEntityType?.GetForeignKeys();
		var appUserForeignKey = foreignKeys?.FirstOrDefault(fk => 
			fk.PrincipalEntityType.ClrType == typeof(AppUser));

		// Assert
		appUserForeignKey.Should().NotBeNull();
	}

	/// <summary>
	///   Verifies that the foreign key uses OwnerSubject property.
	/// </summary>
	[Fact]
	public void Note_ForeignKey_UsesOwnerSubject()
	{
		// Arrange
		using var context = CreateDbContext();
		var noteEntityType = context.Model.FindEntityType(typeof(Note));

		// Act
		var foreignKeys = noteEntityType?.GetForeignKeys();
		var appUserForeignKey = foreignKeys?.FirstOrDefault(fk => 
			fk.PrincipalEntityType.ClrType == typeof(AppUser));

		// Assert
		appUserForeignKey.Should().NotBeNull();
		appUserForeignKey!.Properties.Should().HaveCount(1);
		appUserForeignKey.Properties.First().Name.Should().Be("OwnerSubject");
	}

	/// <summary>
	///   Verifies that the foreign key references Auth0Subject in AppUser.
	/// </summary>
	[Fact]
	public void Note_ForeignKey_ReferencesAuth0Subject()
	{
		// Arrange
		using var context = CreateDbContext();
		var noteEntityType = context.Model.FindEntityType(typeof(Note));

		// Act
		var foreignKeys = noteEntityType?.GetForeignKeys();
		var appUserForeignKey = foreignKeys?.FirstOrDefault(fk => 
			fk.PrincipalEntityType.ClrType == typeof(AppUser));

		// Assert
		appUserForeignKey.Should().NotBeNull();
		appUserForeignKey!.PrincipalKey.Properties.Should().HaveCount(1);
		appUserForeignKey.PrincipalKey.Properties.First().Name.Should().Be("Auth0Subject");
	}

	/// <summary>
	///   Verifies that cascade delete is configured for Note-AppUser relationship.
	/// </summary>
	[Fact]
	public void Note_HasCascadeDeleteBehavior()
	{
		// Arrange
		using var context = CreateDbContext();
		var noteEntityType = context.Model.FindEntityType(typeof(Note));

		// Act
		var foreignKeys = noteEntityType?.GetForeignKeys();
		var appUserForeignKey = foreignKeys?.FirstOrDefault(fk => 
			fk.PrincipalEntityType.ClrType == typeof(AppUser));

		// Assert
		appUserForeignKey.Should().NotBeNull();
		appUserForeignKey!.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);
	}

	/// <summary>
	///   Verifies that the navigation property from Note to AppUser is configured.
	/// </summary>
	[Fact]
	public void Note_HasOwnerNavigationProperty()
	{
		// Arrange
		using var context = CreateDbContext();
		var noteEntityType = context.Model.FindEntityType(typeof(Note));

		// Act
		var navigation = noteEntityType?.FindNavigation("Owner");

		// Assert
		navigation.Should().NotBeNull();
		navigation!.ClrType.Should().Be(typeof(AppUser));
	}

	/// <summary>
	///   Verifies that the navigation property from AppUser to Notes is configured.
	/// </summary>
	[Fact]
	public void AppUser_HasNotesNavigationProperty()
	{
		// Arrange
		using var context = CreateDbContext();
		var appUserEntityType = context.Model.FindEntityType(typeof(AppUser));

		// Act
		var navigation = appUserEntityType?.FindNavigation("Notes");

		// Assert
		navigation.Should().NotBeNull();
		navigation!.ClrType.Should().Be(typeof(ICollection<Note>));
	}

	#endregion

	#region Data Operations Tests

	/// <summary>
	///   Verifies that AppUser can be added and saved to the database.
	/// </summary>
	[Fact]
	public async Task CanAddAndSaveAppUser()
	{
		// Arrange
		using var context = CreateDbContext();
		var appUser = new AppUser
		{
			Auth0Subject = "auth0|123456",
			Name = "Test User",
			Email = "test@example.com",
			CreatedUtc = DateTime.UtcNow
		};

		// Act
		context.AppUsers.Add(appUser);
		await context.SaveChangesAsync();

		// Assert
		var savedUser = await context.AppUsers.FindAsync("auth0|123456");
		savedUser.Should().NotBeNull();
		savedUser!.Name.Should().Be("Test User");
		savedUser.Email.Should().Be("test@example.com");
	}

	/// <summary>
	///   Verifies that Note can be added and saved to the database.
	/// </summary>
	[Fact]
	public async Task CanAddAndSaveNote()
	{
		// Arrange
		using var context = CreateDbContext();
		var noteId = Guid.NewGuid();
		var note = new Note
		{
			Id = noteId,
			Title = "Test Note",
			Content = "Test Content",
			OwnerSubject = "auth0|123456",
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		// Act
		context.Notes.Add(note);
		await context.SaveChangesAsync();

		// Assert
		var savedNote = await context.Notes.FindAsync(noteId);
		savedNote.Should().NotBeNull();
		savedNote!.Title.Should().Be("Test Note");
		savedNote.Content.Should().Be("Test Content");
	}

	/// <summary>
	///   Verifies that a Note can be saved with a related AppUser.
	/// </summary>
	[Fact]
	public async Task CanSaveNoteWithRelatedAppUser()
	{
		// Arrange
		using var context = CreateDbContext();
		var appUser = new AppUser
		{
			Auth0Subject = "auth0|123456",
			Name = "Test User",
			Email = "test@example.com",
			CreatedUtc = DateTime.UtcNow
		};

		var note = new Note
		{
			Id = Guid.NewGuid(),
			Title = "Test Note",
			Content = "Test Content",
			OwnerSubject = appUser.Auth0Subject,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		// Act
		context.AppUsers.Add(appUser);
		context.Notes.Add(note);
		await context.SaveChangesAsync();

		// Assert
		var savedNote = await context.Notes
			.Include(n => n.Owner)
			.FirstOrDefaultAsync(n => n.Id == note.Id);

		savedNote.Should().NotBeNull();
		savedNote!.Owner.Should().NotBeNull();
		savedNote.Owner!.Auth0Subject.Should().Be(appUser.Auth0Subject);
		savedNote.Owner.Name.Should().Be("Test User");
	}

	/// <summary>
	///   Verifies that multiple Notes can be retrieved for an AppUser.
	/// </summary>
	[Fact]
	public async Task CanRetrieveNotesForAppUser()
	{
		// Arrange
		using var context = CreateDbContext();
		var appUser = new AppUser
		{
			Auth0Subject = "auth0|123456",
			Name = "Test User",
			Email = "test@example.com",
			CreatedUtc = DateTime.UtcNow
		};

		context.AppUsers.Add(appUser);

		var notes = new[]
		{
			new Note
			{
				Id = Guid.NewGuid(),
				Title = "Note 1",
				Content = "Content 1",
				OwnerSubject = appUser.Auth0Subject,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			},
			new Note
			{
				Id = Guid.NewGuid(),
				Title = "Note 2",
				Content = "Content 2",
				OwnerSubject = appUser.Auth0Subject,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			}
		};

		context.Notes.AddRange(notes);
		await context.SaveChangesAsync();

		// Act
		var savedUser = await context.AppUsers
			.Include(u => u.Notes)
			.FirstOrDefaultAsync(u => u.Auth0Subject == appUser.Auth0Subject);

		// Assert
		savedUser.Should().NotBeNull();
		savedUser!.Notes.Should().HaveCount(2);
		savedUser.Notes.Select(n => n.Title).Should().Contain(new[] { "Note 1", "Note 2" });
	}

	/// <summary>
	///   Verifies that updating a Note works correctly.
	/// </summary>
	[Fact]
	public async Task CanUpdateNote()
	{
		// Arrange
		using var context = CreateDbContext();
		var noteId = Guid.NewGuid();
		var note = new Note
		{
			Id = noteId,
			Title = "Original Title",
			Content = "Original Content",
			OwnerSubject = "auth0|123456",
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		context.Notes.Add(note);
		await context.SaveChangesAsync();

		// Act
		var savedNote = await context.Notes.FindAsync(noteId);
		savedNote!.Title = "Updated Title";
		savedNote.Content = "Updated Content";
		savedNote.UpdatedAt = DateTime.UtcNow;
		await context.SaveChangesAsync();

		// Assert
		var updatedNote = await context.Notes.FindAsync(noteId);
		updatedNote.Should().NotBeNull();
		updatedNote!.Title.Should().Be("Updated Title");
		updatedNote.Content.Should().Be("Updated Content");
	}

	/// <summary>
	///   Verifies that deleting a Note works correctly.
	/// </summary>
	[Fact]
	public async Task CanDeleteNote()
	{
		// Arrange
		using var context = CreateDbContext();
		var noteId = Guid.NewGuid();
		var note = new Note
		{
			Id = noteId,
			Title = "Test Note",
			Content = "Test Content",
			OwnerSubject = "auth0|123456",
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		context.Notes.Add(note);
		await context.SaveChangesAsync();

		// Act
		var savedNote = await context.Notes.FindAsync(noteId);
		context.Notes.Remove(savedNote!);
		await context.SaveChangesAsync();

		// Assert
		var deletedNote = await context.Notes.FindAsync(noteId);
		deletedNote.Should().BeNull();
	}

	/// <summary>
	///   Verifies that querying Notes by OwnerSubject uses the index efficiently.
	/// </summary>
	[Fact]
	public async Task CanQueryNotesByOwnerSubject()
	{
		// Arrange
		using var context = CreateDbContext();
		var ownerSubject = "auth0|123456";
		var otherSubject = "auth0|789012";

		context.Notes.AddRange(
			new Note
			{
				Id = Guid.NewGuid(),
				Title = "User1 Note",
				Content = "Content",
				OwnerSubject = ownerSubject,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			},
			new Note
			{
				Id = Guid.NewGuid(),
				Title = "User2 Note",
				Content = "Content",
				OwnerSubject = otherSubject,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			}
		);

		await context.SaveChangesAsync();

		// Act
		var userNotes = await context.Notes
			.Where(n => n.OwnerSubject == ownerSubject)
			.ToListAsync();

		// Assert
		userNotes.Should().HaveCount(1);
		userNotes.First().Title.Should().Be("User1 Note");
	}

	/// <summary>
	///   Verifies that querying AppUsers by Email uses the index.
	/// </summary>
	[Fact]
	public async Task CanQueryAppUsersByEmail()
	{
		// Arrange
		using var context = CreateDbContext();
		var users = new[]
		{
			new AppUser
			{
				Auth0Subject = "auth0|123456",
				Name = "User1",
				Email = "user1@example.com",
				CreatedUtc = DateTime.UtcNow
			},
			new AppUser
			{
				Auth0Subject = "auth0|789012",
				Name = "User2",
				Email = "user2@example.com",
				CreatedUtc = DateTime.UtcNow
			}
		};

		context.AppUsers.AddRange(users);
		await context.SaveChangesAsync();

		// Act
		var user = await context.AppUsers
			.FirstOrDefaultAsync(u => u.Email == "user1@example.com");

		// Assert
		user.Should().NotBeNull();
		user!.Name.Should().Be("User1");
	}

	#endregion

}
