// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     NoteTestDataBuilder.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Integration
// =======================================================

using System.Diagnostics.CodeAnalysis;

using AINotesApp.Data;

namespace AINotesApp.Tests.Integration.Helpers;

/// <summary>
///   Fluent builder for creating Note test data with sensible defaults.
///   Follows the Test Data Builder pattern to reduce boilerplate in integration tests.
/// </summary>
/// <example>
///   Basic usage with defaults:
///   <code>
///   var note = NoteTestDataBuilder.CreateDefault().Build();
///   </code>
///   
///   Customized note:
///   <code>
///   var note = NoteTestDataBuilder.CreateDefault()
///       .WithTitle("My Custom Title")
///       .WithOwnerSubject("user-123")
///       .WithEmbedding(new[] { 0.1f, 0.2f, 0.3f })
///       .Build();
///   </code>
/// </example>
[ExcludeFromCodeCoverage]
public class NoteTestDataBuilder
{

	private Guid _id = Guid.NewGuid();

	private string _title = "Test Note";

	private string _content = "Test Content";

	private string _ownerSubject = "test-user";

	private DateTime _createdAt = DateTime.UtcNow;

	private DateTime _updatedAt = DateTime.UtcNow;

	private float[]? _embedding;

	private string? _tags;

	private string? _aiSummary;

	/// <summary>
	///   Creates a new instance of the builder with default test values.
	/// </summary>
	/// <returns>A new NoteTestDataBuilder instance.</returns>
	public static NoteTestDataBuilder CreateDefault()
	{
		return new NoteTestDataBuilder();
	}

	/// <summary>
	///   Sets the note ID.
	/// </summary>
	/// <param name="id">The note identifier.</param>
	/// <returns>The builder instance for method chaining.</returns>
	public NoteTestDataBuilder WithId(Guid id)
	{
		_id = id;

		return this;
	}

	/// <summary>
	///   Sets the note title.
	/// </summary>
	/// <param name="title">The note title.</param>
	/// <returns>The builder instance for method chaining.</returns>
	public NoteTestDataBuilder WithTitle(string title)
	{
		_title = title;

		return this;
	}

	/// <summary>
	///   Sets the note content.
	/// </summary>
	/// <param name="content">The note content.</param>
	/// <returns>The builder instance for method chaining.</returns>
	public NoteTestDataBuilder WithContent(string content)
	{
		_content = content;

		return this;
	}

	/// <summary>
	///   Sets the owner subject (Auth0 user ID).
	/// </summary>
	/// <param name="ownerSubject">The Auth0 subject identifier.</param>
	/// <returns>The builder instance for method chaining.</returns>
	public NoteTestDataBuilder WithOwnerSubject(string ownerSubject)
	{
		_ownerSubject = ownerSubject;

		return this;
	}

	/// <summary>
	///   Sets the creation timestamp.
	/// </summary>
	/// <param name="createdAt">The creation date and time.</param>
	/// <returns>The builder instance for method chaining.</returns>
	public NoteTestDataBuilder WithCreatedAt(DateTime createdAt)
	{
		_createdAt = createdAt;

		return this;
	}

	/// <summary>
	///   Sets the update timestamp.
	/// </summary>
	/// <param name="updatedAt">The update date and time.</param>
	/// <returns>The builder instance for method chaining.</returns>
	public NoteTestDataBuilder WithUpdatedAt(DateTime updatedAt)
	{
		_updatedAt = updatedAt;

		return this;
	}

	/// <summary>
	///   Sets the AI-generated embedding vector.
	/// </summary>
	/// <param name="embedding">The embedding vector.</param>
	/// <returns>The builder instance for method chaining.</returns>
	public NoteTestDataBuilder WithEmbedding(float[] embedding)
	{
		_embedding = embedding;

		return this;
	}

	/// <summary>
	///   Sets the note tags.
	/// </summary>
	/// <param name="tags">The comma-separated tags.</param>
	/// <returns>The builder instance for method chaining.</returns>
	public NoteTestDataBuilder WithTags(string tags)
	{
		_tags = tags;

		return this;
	}

	/// <summary>
	///   Sets the AI-generated summary.
	/// </summary>
	/// <param name="aiSummary">The AI summary text.</param>
	/// <returns>The builder instance for method chaining.</returns>
	public NoteTestDataBuilder WithAiSummary(string aiSummary)
	{
		_aiSummary = aiSummary;

		return this;
	}

	/// <summary>
	///   Builds the Note entity with the configured values.
	/// </summary>
	/// <returns>A new Note instance.</returns>
	public Note Build()
	{
		return new Note
		{
				Id = _id,
				Title = _title,
				Content = _content,
				OwnerSubject = _ownerSubject,
				CreatedAt = _createdAt,
				UpdatedAt = _updatedAt,
				Embedding = _embedding,
				Tags = _tags,
				AiSummary = _aiSummary
		};
	}

}
