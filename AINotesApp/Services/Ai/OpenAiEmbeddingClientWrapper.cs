// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     OpenAiEmbeddingClientWrapper.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp
// =======================================================

using System.ClientModel;

using OpenAI.Embeddings;

namespace AINotesApp.Services.Ai;

/// <summary>
///   Implementation of IEmbeddingClientWrapper using OpenAI's EmbeddingClient.
/// </summary>
public class OpenAiEmbeddingClientWrapper : IEmbeddingClientWrapper
{

	private readonly EmbeddingClient _embeddingClient;

	/// <summary>
	///   Initializes a new instance of the <see cref="OpenAiEmbeddingClientWrapper" /> class.
	/// </summary>
	/// <param name="embeddingClient">The underlying EmbeddingClient instance.</param>
	public OpenAiEmbeddingClientWrapper(EmbeddingClient embeddingClient)
	{
		_embeddingClient = embeddingClient ?? throw new ArgumentNullException(nameof(embeddingClient));
	}

	/// <inheritdoc />
	public Task<ClientResult<OpenAIEmbedding>> GenerateEmbeddingAsync(
			string text,
			CancellationToken cancellationToken = default)
	{
		return _embeddingClient.GenerateEmbeddingAsync(text, cancellationToken: cancellationToken);
	}

}
