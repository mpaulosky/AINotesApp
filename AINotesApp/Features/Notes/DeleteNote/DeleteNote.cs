// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     DeleteNote.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp
// =======================================================

using AINotesApp.Data;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace AINotesApp.Features.Notes.DeleteNote;

public record DeleteNoteCommand : IRequest<DeleteNoteResponse>
{

	public Guid Id { get; init; }

	public string UserSubject { get; init; } = string.Empty;

}

public class DeleteNoteHandler : IRequestHandler<DeleteNoteCommand, DeleteNoteResponse>
{

	private readonly ApplicationDbContext _context;

	public DeleteNoteHandler(ApplicationDbContext context)
	{
		_context = context;
	}

	public async Task<DeleteNoteResponse> Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
	{
		var note = await _context.Notes
				.FirstOrDefaultAsync(n => n.Id == request.Id && n.OwnerSubject == request.UserSubject, cancellationToken);

		if (note == null)
		{
			return new DeleteNoteResponse { Success = false, Message = "Note not found or access denied." };
		}

		_context.Notes.Remove(note);
		await _context.SaveChangesAsync(cancellationToken);

		return new DeleteNoteResponse
		{
				Success = true,
				Message = "Note deleted successfully."
		};
	}

}

public record DeleteNoteResponse
{

	public bool Success { get; init; }

	public string Message { get; init; } = string.Empty;

}