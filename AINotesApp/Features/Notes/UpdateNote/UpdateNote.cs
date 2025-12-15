// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     UpdateNote.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp
// =======================================================

using AINotesApp.Data;
using AINotesApp.Services.Ai;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace AINotesApp.Features.Notes.UpdateNote;

/// <summary>
///   Command to update an existing note.
/// </summary>
public record UpdateNoteCommand : IRequest<UpdateNoteResponse>
{

	/// <summary>
	///   Gets the ID of the note to update.
	/// </summary>
	public Guid Id { get; init; }

	/// <summary>
	///   Gets the updated title.
	/// </summary>
	public string Title { get; init; } = string.Empty;

	/// <summary>
	///   Gets the updated content.
	/// </summary>
	public string Content { get; init; } = string.Empty;

	/// <summary>
	///   Gets the Auth0 subject for authorization.
	/// </summary>
	public string UserSubject { get; init; } = string.Empty;

}

/// <summary>
///   Handler for updating a note with AI regeneration.
/// </summary>
public class UpdateNoteHandler : IRequestHandler<UpdateNoteCommand, UpdateNoteResponse>
{

	private readonly IAiService _aiService;

	private readonly ApplicationDbContext _context;

	public UpdateNoteHandler(ApplicationDbContext context, IAiService aiService)
	{
		_context = context;
		_aiService = aiService;
	}

	/// <summary>
	///   Handles the command to update a note.
	/// </summary>
	public async Task<UpdateNoteResponse> Handle(UpdateNoteCommand request, CancellationToken cancellationToken)
	{
		var note = await _context.Notes
				.FirstOrDefaultAsync(n => n.Id == request.Id && n.OwnerSubject == request.UserSubject, cancellationToken);

		if (note == null)
		{
			return new UpdateNoteResponse { Success = false, Message = "Note not found or access denied." };
		}

		// Regenerate AI summary, tags, and embedding if content changed
		var summaryTask = _aiService.GenerateSummaryAsync(request.Content, cancellationToken);
		var tagsTask = _aiService.GenerateTagsAsync(request.Title, request.Content, cancellationToken);
		var embeddingTask = _aiService.GenerateEmbeddingAsync(request.Content, cancellationToken);

		await Task.WhenAll(summaryTask, tagsTask, embeddingTask);

		note.Title = request.Title;
		note.Content = request.Content;
		note.AiSummary = await summaryTask;
		note.Tags = await tagsTask;
		note.Embedding = await embeddingTask;
		note.UpdatedAt = DateTime.UtcNow;

		await _context.SaveChangesAsync(cancellationToken);

		return new UpdateNoteResponse
		{
				Success = true,
				Id = note.Id,
				Title = note.Title,
				Content = note.Content,
				AiSummary = note.AiSummary,
				Tags = note.Tags,
				UpdatedAt = note.UpdatedAt
		};
	}

}

/// <summary>
///   Response after updating a note.
/// </summary>
public record UpdateNoteResponse
{

	/// <summary>
	///   Gets whether the update was successful.
	/// </summary>
	public bool Success { get; init; }

	/// <summary>
	///   Gets the error message if unsuccessful.
	/// </summary>
	public string Message { get; init; } = string.Empty;

	/// <summary>
	///   Gets the ID of the updated note.
	/// </summary>
	public Guid Id { get; init; }

	/// <summary>
	///   Gets the title of the note.
	/// </summary>
	public string Title { get; init; } = string.Empty;

	/// <summary>
	///   Gets the content of the note.
	/// </summary>
	public string Content { get; init; } = string.Empty;

	/// <summary>
	///   Gets the AI-generated summary.
	/// </summary>
	public string? AiSummary { get; init; }

	/// <summary>
	///   Gets the AI-generated tags.
	/// </summary>
	public string? Tags { get; init; }

	/// <summary>
	///   Gets the update timestamp.
	/// </summary>
	public DateTime UpdatedAt { get; init; }

}