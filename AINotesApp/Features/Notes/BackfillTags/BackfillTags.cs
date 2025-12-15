// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     BackfillTags.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp
// =======================================================

using AINotesApp.Data;
using AINotesApp.Services.Ai;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace AINotesApp.Features.Notes.BackfillTags;

/// <summary>
///   Command to backfill tags for existing notes.
/// </summary>
public record BackfillTagsCommand : IRequest<BackfillTagsResponse>
{

	/// <summary>
	///   Gets the Auth0 subject to backfill tags for.
	/// </summary>
	public string UserSubject { get; init; } = string.Empty;

	/// <summary>
	///   Gets whether to only process notes without tags.
	/// </summary>
	public bool OnlyMissing { get; init; } = true;

}

/// <summary>
///   Handler for backfilling tags on existing notes.
/// </summary>
public class BackfillTagsHandler : IRequestHandler<BackfillTagsCommand, BackfillTagsResponse>
{

	private readonly IAiService _aiService;

	private readonly ApplicationDbContext _context;

	public BackfillTagsHandler(ApplicationDbContext context, IAiService aiService)
	{
		_context = context;
		_aiService = aiService;
	}

	/// <summary>
	///   Handles the command to backfill tags.
	/// </summary>
	public async Task<BackfillTagsResponse> Handle(BackfillTagsCommand request, CancellationToken cancellationToken)
	{
		var query = _context.Notes
				.Where(n => n.OwnerSubject == request.UserSubject);

		// Filter to only notes without tags if requested
		if (request.OnlyMissing)
		{
			query = query.Where(n => string.IsNullOrEmpty(n.Tags));
		}

		var notes = await query.ToListAsync(cancellationToken);

		var processedCount = 0;
		var errors = new List<string>();

		foreach (var note in notes)
		{
			try
			{
				// Generate tags for this note
				var tags = await _aiService.GenerateTagsAsync(note.Title, note.Content, cancellationToken);

				note.Tags = tags;
				note.UpdatedAt = DateTime.UtcNow;

				processedCount++;

				// Save periodically to avoid memory issues
				if (processedCount % 5 == 0)
				{
					await _context.SaveChangesAsync(cancellationToken);
				}
			}
			catch (Exception ex)
			{
				errors.Add($"Failed to generate tags for note '{note.Title}': {ex.Message}");
			}
		}

		// Final save
		await _context.SaveChangesAsync(cancellationToken);

		return new BackfillTagsResponse
		{
				ProcessedCount = processedCount,
				TotalNotes = notes.Count,
				Errors = errors
		};
	}

}

/// <summary>
///   Response after backfilling tags.
/// </summary>
public record BackfillTagsResponse
{

	/// <summary>
	///   Gets the number of notes processed.
	/// </summary>
	public int ProcessedCount { get; init; }

	/// <summary>
	///   Gets the total number of notes found.
	/// </summary>
	public int TotalNotes { get; init; }

	/// <summary>
	///   Gets any errors that occurred.
	/// </summary>
	public List<string> Errors { get; init; } = new();

}