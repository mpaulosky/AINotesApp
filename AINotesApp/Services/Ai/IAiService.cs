// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     IAiService.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp
// =======================================================

using System.Threading;

namespace AINotesApp.Services;

/// <summary>
///   Service interface for AI operations using OpenAI API.
/// </summary>
public interface IAiService
{

	/// <summary>
	///   Generates a concise summary for the given note content.
	/// </summary>
	/// <param name="content">The note content to summarize.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>A generated summary of the content.</returns>
	Task<string> GenerateSummaryAsync(string content, CancellationToken cancellationToken = default);

	/// <summary>
	///   Generates embeddings for semantic search and similarity matching.
	/// </summary>
	/// <param name="text">The text to generate embeddings for.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>A vector representation of the text as a float array.</returns>
	Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);

	/// <summary>
	///   Generates relevant tags for the given note title and content.
	/// </summary>
	/// <param name="title">The note title.</param>
	/// <param name="content">The note content.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>A comma-separated string of tags.</returns>
	Task<string> GenerateTagsAsync(string title, string content, CancellationToken cancellationToken = default);

	/// <summary>
	///   Finds related notes based on semantic similarity using embeddings.
	/// </summary>
	/// <param name="embedding">The embedding vector to compare against.</param>
	/// <param name="userSubject">The Auth0 subject to filter notes by.</param>
	/// <param name="currentNoteId">The current note ID to exclude from results.</param>
	/// <param name="topN">The number of related notes to return.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>A list of note IDs sorted by similarity.</returns>
	Task<List<Guid>> FindRelatedNotesAsync(
			float[] embedding,
			string userSubject,
			Guid? currentNoteId = null,
			int? topN = null,
			CancellationToken cancellationToken = default);

}