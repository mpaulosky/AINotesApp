// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     AppUser.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp
// =======================================================

namespace AINotesApp.Data;

/// <summary>
///   Simplified application user tracked by an Auth0 subject.
/// </summary>
public class AppUser
{

	/// <summary>
	///   Gets or sets the Auth0 subject for this user (primary key).
	/// </summary>
	public string Auth0Subject { get; set; } = null!;

	/// <summary>
	///   Gets or sets the display name from Auth0.
	/// </summary>
	public string? Name { get; set; }

	/// <summary>
	///   Gets or sets the email address from Auth0.
	/// </summary>
	public string? Email { get; set; }

	/// <summary>
	///   Gets or sets when this record was created in UTC.
	/// </summary>
	public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

	/// <summary>
	///   Navigation property for the user's notes.
	/// </summary>
	public ICollection<Note> Notes { get; set; } = new List<Note>();

}