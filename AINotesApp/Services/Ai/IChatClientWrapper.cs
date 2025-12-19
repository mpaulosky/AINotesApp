// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     IChatClientWrapper.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp
// =======================================================

using System.ClientModel;

using OpenAI.Chat;

namespace AINotesApp.Services.Ai;

/// <summary>
///   Wrapper interface for ChatClient to enable dependency injection and testing.
/// </summary>
public interface IChatClientWrapper
{

	/// <summary>
	///   Completes a chat conversation asynchronously.
	/// </summary>
	/// <param name="messages">The messages in the conversation.</param>
	/// <param name="options">Optional completion options.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>The chat completion result.</returns>
	Task<ClientResult<ChatCompletion>> CompleteChatAsync(
			IEnumerable<ChatMessage> messages,
			ChatCompletionOptions? options = null,
			CancellationToken cancellationToken = default);

}
