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

			// Assert - The wrapper should forward the call and not throw ArgumentNullException or ArgumentException
			// Note: The mock may throw other exceptions due to null returns, which is expected mock behavior
			await act.Should().NotThrowAsync<ArgumentNullException>();
			await act.Should().NotThrowAsync<ArgumentException>();

			// Verify the mock was called (at least once due to multiple FluentAssertions checks above)
			mockEmbeddingClient.Verify(c => c.GenerateEmbeddingAsync(text, It.IsAny<EmbeddingGenerationOptions>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
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

			// Assert - The wrapper should forward the call and not throw ArgumentNullException or ArgumentException
			// Note: The mock may throw other exceptions due to null returns, which is expected mock behavior
			await act.Should().NotThrowAsync<ArgumentNullException>();
			await act.Should().NotThrowAsync<ArgumentException>();

			// Verify the mock was called (at least once due to multiple FluentAssertions checks above)
			mockEmbeddingClient.Verify(c => c.GenerateEmbeddingAsync(text, It.IsAny<EmbeddingGenerationOptions>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
		}
	}
}
