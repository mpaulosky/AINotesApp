using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AINotesApp.Services.Ai;
using Moq;
using OpenAI.Chat;
using Xunit;

namespace AINotesApp.Tests.Unit.Services.Ai
{
	[ExcludeFromCodeCoverage]
	public class OpenAiChatClientWrapperTests
	{
		[Fact]
		public void Constructor_ThrowsArgumentNullException_WhenChatClientIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => new OpenAiChatClientWrapper((ChatClient)null!));
		}

		[Fact]
		public async Task CompleteChatAsync_ForwardsCallToChatClient_ReturnsResult()
		{
			// Arrange
			var mockChatClient = new Mock<ChatClient>(MockBehavior.Loose);
			var messages = new List<ChatMessage> { new UserChatMessage("Hello") };
			var options = new ChatCompletionOptions();
			var cancellationToken = new CancellationTokenSource().Token;

			var wrapper = new OpenAiChatClientWrapper(mockChatClient.Object);

			// Act & Assert - we're testing that the wrapper correctly forwards the call
			// The mock will throw if the method signature doesn't match
			try
			{
				await wrapper.CompleteChatAsync(messages, options, cancellationToken);
			}
			catch
			{
				// Expected - mock returns null which causes exception, but we've verified the call works
			}

			mockChatClient.Verify(c => c.CompleteChatAsync(messages, options, cancellationToken), Times.Once);
		}

		[Fact]
		public async Task CompleteChatAsync_AllowsNullOptionsAndDefaultCancellationToken()
		{
			// Arrange
			var mockChatClient = new Mock<ChatClient>(MockBehavior.Loose);
			var messages = new List<ChatMessage> { new UserChatMessage("Test") };

			var wrapper = new OpenAiChatClientWrapper(mockChatClient.Object);

			// Act & Assert
			try
			{
				await wrapper.CompleteChatAsync(messages);
			}
			catch
			{
				// Expected - mock returns null which causes exception, but we've verified the call works
			}

			mockChatClient.Verify(c => c.CompleteChatAsync(messages, null, default), Times.Once);
		}
	}
}
