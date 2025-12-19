// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     IEmbeddingClientWrapper.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp
// =======================================================

using System.ClientModel;

using OpenAI.Embeddings;

namespace AINotesApp.Services.Ai;

/// <summary>
///   Wrapper interface for EmbeddingClient to enable dependency injection and testing.
/// </summary>
public interface IEmbeddingClientWrapper
{

	/// <summary>
	///   Generates an embedding for the specified text.
	/// </summary>
	/// <param name="text">The text to generate an embedding for.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>The generated embedding.</returns>
	Task<ClientResult<OpenAIEmbedding>> GenerateEmbeddingAsync(
			string text,
			CancellationToken cancellationToken = default);

}
