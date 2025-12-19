using System;
using System.ClientModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AINotesApp.Services.Ai;
using FluentAssertions;
using Moq;
using OpenAI.Embeddings;
using Xunit;

namespace AINotesApp.Tests.Unit.Services.Ai
{
	[ExcludeFromCodeCoverage]
	public class OpenAiEmbeddingClientWrapperTests
	{
		[Fact]
		public void Constructor_ThrowsArgumentNullException_WhenEmbeddingClientIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => new OpenAiEmbeddingClientWrapper((EmbeddingClient)null!));
		}

		[Fact]
		public async Task GenerateEmbeddingAsync_ForwardsCallToEmbeddingClient_ReturnsResult()
		{
			// Arrange
			var mockEmbeddingClient = new Mock<EmbeddingClient>(MockBehavior.Loose);
			var text = "test input";
			var cancellationToken = new CancellationTokenSource().Token;

			var wrapper = new OpenAiEmbeddingClientWrapper(mockEmbeddingClient.Object);

			// Act
			var act = async () => await wrapper.GenerateEmbeddingAsync(text, cancellationToken);

			// Assert - verify the wrapper correctly forwards the call to the underlying client
			await act.Should().NotThrowAsync<ArgumentNullException>("wrapper should forward the call to the underlying embedding client");
			mockEmbeddingClient.Verify(c => c.GenerateEmbeddingAsync(text, It.IsAny<EmbeddingGenerationOptions>(), It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task GenerateEmbeddingAsync_AllowsDefaultCancellationToken()
		{
			// Arrange
			var mockEmbeddingClient = new Mock<EmbeddingClient>(MockBehavior.Loose);
			var text = "another test";

			var wrapper = new OpenAiEmbeddingClientWrapper(mockEmbeddingClient.Object);

			// Act
			var act = async () => await wrapper.GenerateEmbeddingAsync(text);

			// Assert - verify the wrapper accepts default cancellation token
			await act.Should().NotThrowAsync<ArgumentNullException>("wrapper should accept default cancellation token");
			mockEmbeddingClient.Verify(c => c.GenerateEmbeddingAsync(text, It.IsAny<EmbeddingGenerationOptions>(), It.IsAny<CancellationToken>()), Times.Once);
		}
	}
}
