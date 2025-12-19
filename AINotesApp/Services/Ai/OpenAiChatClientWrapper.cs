// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     OpenAiChatClientWrapper.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp
// =======================================================

using System.ClientModel;

using OpenAI.Chat;

namespace AINotesApp.Services.Ai;

/// <summary>
///   Implementation of IChatClientWrapper using OpenAI's ChatClient.
/// </summary>
public class OpenAiChatClientWrapper : IChatClientWrapper
{

	private readonly ChatClient _chatClient;

	/// <summary>
	///   Initializes a new instance of the <see cref="OpenAiChatClientWrapper" /> class.
	/// </summary>
	/// <param name="chatClient">The underlying ChatClient instance.</param>
	public OpenAiChatClientWrapper(ChatClient chatClient)
	{
		_chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
	}

	/// <inheritdoc />
	public Task<ClientResult<ChatCompletion>> CompleteChatAsync(
			IEnumerable<ChatMessage> messages,
			ChatCompletionOptions? options = null,
			CancellationToken cancellationToken = default)
	{
		return _chatClient.CompleteChatAsync(messages, options, cancellationToken);
	}

}
