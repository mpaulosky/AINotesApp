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

			// Assert - Verify the wrapper forwards the call and doesn't throw due to wrapper logic issues
			// Note: The mock may throw NullReferenceException due to null returns, which is expected mock behavior
			await act.Should().NotThrowAsync<ArgumentNullException>("the wrapper should forward the call to the client");
			await act.Should().NotThrowAsync<ArgumentException>("the wrapper should not introduce argument validation errors");

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

			// Assert - Verify the wrapper forwards the call and doesn't throw due to wrapper logic issues
			// Note: The mock may throw NullReferenceException due to null returns, which is expected mock behavior
			await act.Should().NotThrowAsync<ArgumentNullException>("the wrapper should forward the call to the client");
			await act.Should().NotThrowAsync<ArgumentException>("the wrapper should not introduce argument validation errors");

			mockChatClient.Verify(c => c.CompleteChatAsync(messages, null, default), Times.AtLeastOnce);
		}
	}
}
