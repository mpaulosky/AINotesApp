using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AINotesApp.Services.Ai;
using FluentAssertions;
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

			// Act
			var act = async () => await wrapper.CompleteChatAsync(messages, options, cancellationToken);

			// Assert - verify the wrapper correctly forwards the call to the underlying client
			await act.Should().NotThrowAsync<ArgumentNullException>("wrapper should forward the call to the underlying chat client");
			mockChatClient.Verify(c => c.CompleteChatAsync(messages, options, cancellationToken), Times.Once);
		}

		[Fact]
		public async Task CompleteChatAsync_AllowsNullOptionsAndDefaultCancellationToken()
		{
			// Arrange
			var mockChatClient = new Mock<ChatClient>(MockBehavior.Loose);
			var messages = new List<ChatMessage> { new UserChatMessage("Test") };

			var wrapper = new OpenAiChatClientWrapper(mockChatClient.Object);

			// Act
			var act = async () => await wrapper.CompleteChatAsync(messages);

			// Assert - verify the wrapper accepts null options and default cancellation token
			await act.Should().NotThrowAsync<ArgumentNullException>("wrapper should accept null options and default cancellation token");
			mockChatClient.Verify(c => c.CompleteChatAsync(messages, null, default), Times.Once);
		}
	}
}
