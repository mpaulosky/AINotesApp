using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     SearchNotes.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp
// =======================================================

using AINotesApp.Data;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace AINotesApp.Features.Notes.SearchNotes;

/// <summary>
///   Query to search notes by title and content.
/// </summary>
public record SearchNotesQuery : IRequest<SearchNotesResponse>
{

	/// <summary>
	///   Gets the search term to filter notes.
	/// </summary>
	public string SearchTerm { get; init; } = string.Empty;

	/// <summary>
	///   Gets the Auth0 subject for authorization.
	/// </summary>
	public string UserSubject { get; init; } = string.Empty;

	/// <summary>
	///   Gets the page number for pagination.
	/// </summary>
	public int PageNumber { get; init; } = 1;

	/// <summary>
	///   Gets the page size for pagination.
	/// </summary>
	public int PageSize { get; init; } = 10;

}

/// <summary>
///   Handler for searching notes by title and content.
/// </summary>
public class SearchNotesHandler : IRequestHandler<SearchNotesQuery, SearchNotesResponse>
{

	private readonly ApplicationDbContext _context;

	public SearchNotesHandler(ApplicationDbContext context)
	{
		_context = context;
	}

	/// <summary>
	///   Handles the search notes query.
	/// </summary>
	public async Task<SearchNotesResponse> Handle(SearchNotesQuery request, CancellationToken cancellationToken)
	{
		var query = _context.Notes
				.AsNoTracking()
				.Where(n => n.OwnerSubject == request.UserSubject);

		// Apply search filter if search term is provided
		if (!string.IsNullOrWhiteSpace(request.SearchTerm))
		{
			var searchTerm = request.SearchTerm.ToLower();

			query = query.Where(n =>
					n.Title.ToLower().Contains(searchTerm) ||
					n.Content.ToLower().Contains(searchTerm));
		}

		query = query.OrderByDescending(n => n.UpdatedAt);

		var totalCount = await query.CountAsync(cancellationToken);

		var notes = await query
				.Skip((request.PageNumber - 1) * request.PageSize)
				.Take(request.PageSize)
				.Select(n => new SearchNoteItem
				{
						Id = n.Id,
						Title = n.Title,
						AiSummary = n.AiSummary,
						Tags = n.Tags,
						CreatedAt = n.CreatedAt,
						UpdatedAt = n.UpdatedAt
				})
				.ToListAsync(cancellationToken);

		return new SearchNotesResponse
		{
				Notes = notes,
				TotalCount = totalCount,
				PageNumber = request.PageNumber,
				PageSize = request.PageSize,
				SearchTerm = request.SearchTerm
		};
	}

}

/// <summary>
///   Response containing search results.
/// </summary>
public record SearchNotesResponse
{

	/// <summary>
	///   Gets the list of notes matching the search.
	/// </summary>
	public List<SearchNoteItem> Notes { get; init; } = new();

	/// <summary>
	///   Gets the total count of matching notes.
	/// </summary>
	public int TotalCount { get; init; }

	/// <summary>
	///   Gets the current page number.
	/// </summary>
	public int PageNumber { get; init; }

	/// <summary>
	///   Gets the page size.
	/// </summary>
	public int PageSize { get; init; }

	/// <summary>
	///   Gets the total number of pages.
	/// </summary>
	public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

	/// <summary>
	///   Gets the search term used.
	/// </summary>
	public string SearchTerm { get; init; } = string.Empty;

}

/// <summary>
///   Information about a note in search results.
/// </summary>
public record SearchNoteItem
{

	/// <summary>
	///   Gets the note ID.
	/// </summary>
	public Guid Id { get; init; }

	/// <summary>
	///   Gets the note title.
	/// </summary>
	public string Title { get; init; } = string.Empty;

	/// <summary>
	///   Gets the AI-generated summary.
	/// </summary>
	public string? AiSummary { get; init; }

	/// <summary>
	///   Gets the AI-generated tags.
	/// </summary>
	public string? Tags { get; init; }

	/// <summary>
	///   Gets the creation date.
	/// </summary>
	public DateTime CreatedAt { get; init; }

	/// <summary>
	///   Gets the last update date.
	/// </summary>
	public DateTime UpdatedAt { get; init; }

}