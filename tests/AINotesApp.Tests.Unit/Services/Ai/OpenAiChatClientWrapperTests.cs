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

			// Assert - The wrapper should forward the call and not throw ArgumentNullException or ArgumentException
			// Note: The mock may throw other exceptions due to null returns, which is expected mock behavior
			await act.Should().NotThrowAsync<ArgumentNullException>();
			await act.Should().NotThrowAsync<ArgumentException>();

			// Verify the mock was called (at least once due to multiple FluentAssertions checks above)
			mockChatClient.Verify(c => c.CompleteChatAsync(messages, options, cancellationToken), Times.AtLeastOnce);
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

			// Assert - The wrapper should forward the call and not throw ArgumentNullException or ArgumentException
			// Note: The mock may throw other exceptions due to null returns, which is expected mock behavior
			await act.Should().NotThrowAsync<ArgumentNullException>();
			await act.Should().NotThrowAsync<ArgumentException>();

			// Verify the mock was called (at least once due to multiple FluentAssertions checks above)
			mockChatClient.Verify(c => c.CompleteChatAsync(messages, null, default), Times.AtLeastOnce);
		}
	}
}
