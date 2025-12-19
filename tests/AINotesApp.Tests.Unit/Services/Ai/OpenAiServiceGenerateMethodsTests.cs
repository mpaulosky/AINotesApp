// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     OpenAiServiceGenerateMethodsTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;

using AINotesApp.Data;
using AINotesApp.Services;
using AINotesApp.Services.Ai;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Moq;

using OpenAI.Chat;

namespace AINotesApp.Tests.Unit.Services.Ai;

/// <summary>
///   Unit tests for OpenAiService Generate methods (Summary, Embedding, Tags).
///   Tests focus on input validation, error handling, and parameter verification.
/// </summary>
[ExcludeFromCodeCoverage]
public class OpenAiServiceGenerateMethodsTests
{

	private readonly ApplicationDbContext _context;

	private readonly AiServiceOptions _options;

	public OpenAiServiceGenerateMethodsTests()
	{
		var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;

		_context = new ApplicationDbContext(dbOptions);

		_options = new AiServiceOptions
		{
			ApiKey = "test-api-key",
			ChatModel = "gpt-4o-mini",
			EmbeddingModel = "text-embedding-3-small",
			MaxSummaryTokens = 150,
			RelatedNotesCount = 5,
			SimilarityThreshold = 0.7
		};
	}

	#region GenerateSummaryAsync Tests

	[Fact]
	public async Task GenerateSummaryAsync_WithNullContent_ReturnsEmptyString()
	{
		// Arrange
		var mockChat = new Mock<IChatClientWrapper>();
		var mockEmbedding = new Mock<IEmbeddingClientWrapper>();
		var service = new OpenAiService(Options.Create(_options), _context, mockChat.Object, mockEmbedding.Object);

		// Act
		var result = await service.GenerateSummaryAsync(null!);

		// Assert
		result.Should().BeEmpty();
		mockChat.Verify(x => x.CompleteChatAsync(
						It.IsAny<IEnumerable<ChatMessage>>(),
						It.IsAny<ChatCompletionOptions>(),
						It.IsAny<CancellationToken>()),
				Times.Never);
	}

	[Fact]
	public async Task GenerateSummaryAsync_WithEmptyContent_ReturnsEmptyString()
	{
		// Arrange
		var mockChat = new Mock<IChatClientWrapper>();
		var mockEmbedding = new Mock<IEmbeddingClientWrapper>();
		var service = new OpenAiService(Options.Create(_options), _context, mockChat.Object, mockEmbedding.Object);

		// Act
		var result = await service.GenerateSummaryAsync(string.Empty);

		// Assert
		result.Should().BeEmpty();
		mockChat.Verify(x => x.CompleteChatAsync(
						It.IsAny<IEnumerable<ChatMessage>>(),
						It.IsAny<ChatCompletionOptions>(),
						It.IsAny<CancellationToken>()),
				Times.Never);
	}

	[Fact]
	public async Task GenerateSummaryAsync_WithWhitespaceContent_ReturnsEmptyString()
	{
		// Arrange
		var mockChat = new Mock<IChatClientWrapper>();
		var mockEmbedding = new Mock<IEmbeddingClientWrapper>();
		var service = new OpenAiService(Options.Create(_options), _context, mockChat.Object, mockEmbedding.Object);

		// Act
		var result = await service.GenerateSummaryAsync("   \t\n  ");

		// Assert
		result.Should().BeEmpty();
		mockChat.Verify(x => x.CompleteChatAsync(
						It.IsAny<IEnumerable<ChatMessage>>(),
						It.IsAny<ChatCompletionOptions>(),
						It.IsAny<CancellationToken>()),
				Times.Never);
	}

	[Fact]
	public async Task GenerateSummaryAsync_WhenExceptionThrown_ReturnsEmptyString()
	{
		// Arrange
		var mockChat = new Mock<IChatClientWrapper>();
		mockChat.Setup(x => x.CompleteChatAsync(
						It.IsAny<IEnumerable<ChatMessage>>(),
						It.IsAny<ChatCompletionOptions>(),
						It.IsAny<CancellationToken>()))
				.ThrowsAsync(new Exception("API Error"));

		var mockEmbedding = new Mock<IEmbeddingClientWrapper>();
		var service = new OpenAiService(Options.Create(_options), _context, mockChat.Object, mockEmbedding.Object);

		// Act
		var result = await service.GenerateSummaryAsync("Some content");

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task GenerateSummaryAsync_CallsChatClient_WithCorrectParameters()
	{
		// Arrange
		var mockChat = new Mock<IChatClientWrapper>();
		var mockEmbedding = new Mock<IEmbeddingClientWrapper>();

		List<ChatMessage>? capturedMessages = null;
		ChatCompletionOptions? capturedOptions = null;

		mockChat.Setup(x => x.CompleteChatAsync(
						It.IsAny<IEnumerable<ChatMessage>>(),
						It.IsAny<ChatCompletionOptions>(),
						It.IsAny<CancellationToken>()))
				.Callback<IEnumerable<ChatMessage>, ChatCompletionOptions?, CancellationToken>((msgs, opts, ct) =>
				{
					capturedMessages = msgs.ToList();
					capturedOptions = opts;
				})
				.ThrowsAsync(new Exception("Test")); // Throw to prevent null ref on response

		var service = new OpenAiService(Options.Create(_options), _context, mockChat.Object, mockEmbedding.Object);

		// Act
		await service.GenerateSummaryAsync("Test content");

		// Assert
		capturedMessages.Should().NotBeNull();
		capturedMessages.Should().HaveCount(2);
		capturedMessages![0].Should().BeOfType<SystemChatMessage>();
		capturedMessages[1].Should().BeOfType<UserChatMessage>();

		capturedOptions.Should().NotBeNull();
		capturedOptions!.MaxOutputTokenCount.Should().Be(_options.MaxSummaryTokens);
		capturedOptions.Temperature.Should().Be(0.5f);
	}

	#endregion

	#region GenerateEmbeddingAsync Tests

	[Fact]
	public async Task GenerateEmbeddingAsync_WithNullText_ReturnsEmptyArray()
	{
		// Arrange
		var mockChat = new Mock<IChatClientWrapper>();
		var mockEmbedding = new Mock<IEmbeddingClientWrapper>();
		var service = new OpenAiService(Options.Create(_options), _context, mockChat.Object, mockEmbedding.Object);

		// Act
		var result = await service.GenerateEmbeddingAsync(null!);

		// Assert
		result.Should().BeEmpty();
		mockEmbedding.Verify(x => x.GenerateEmbeddingAsync(
						It.IsAny<string>(),
						It.IsAny<CancellationToken>()),
				Times.Never);
	}

	[Fact]
	public async Task GenerateEmbeddingAsync_WithEmptyText_ReturnsEmptyArray()
	{
		// Arrange
		var mockChat = new Mock<IChatClientWrapper>();
		var mockEmbedding = new Mock<IEmbeddingClientWrapper>();
		var service = new OpenAiService(Options.Create(_options), _context, mockChat.Object, mockEmbedding.Object);

		// Act
		var result = await service.GenerateEmbeddingAsync(string.Empty);

		// Assert
		result.Should().BeEmpty();
		mockEmbedding.Verify(x => x.GenerateEmbeddingAsync(
						It.IsAny<string>(),
						It.IsAny<CancellationToken>()),
				Times.Never);
	}

	[Fact]
	public async Task GenerateEmbeddingAsync_WithWhitespaceText_ReturnsEmptyArray()
	{
		// Arrange
		var mockChat = new Mock<IChatClientWrapper>();
		var mockEmbedding = new Mock<IEmbeddingClientWrapper>();
		var service = new OpenAiService(Options.Create(_options), _context, mockChat.Object, mockEmbedding.Object);

		// Act
		var result = await service.GenerateEmbeddingAsync("   \t\n  ");

		// Assert
		result.Should().BeEmpty();
		mockEmbedding.Verify(x => x.GenerateEmbeddingAsync(
						It.IsAny<string>(),
						It.IsAny<CancellationToken>()),
				Times.Never);
	}

	[Fact]
	public async Task GenerateEmbeddingAsync_WhenExceptionThrown_ReturnsEmptyArray()
	{
		// Arrange
		var mockChat = new Mock<IChatClientWrapper>();
		var mockEmbedding = new Mock<IEmbeddingClientWrapper>();
		mockEmbedding.Setup(x => x.GenerateEmbeddingAsync(
						It.IsAny<string>(),
						It.IsAny<CancellationToken>()))
				.ThrowsAsync(new Exception("API Error"));

		var service = new OpenAiService(Options.Create(_options), _context, mockChat.Object, mockEmbedding.Object);

		// Act
		var result = await service.GenerateEmbeddingAsync("Some text");

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task GenerateEmbeddingAsync_CallsEmbeddingClient_WithCorrectInput()
	{
		// Arrange
		var mockChat = new Mock<IChatClientWrapper>();
		var mockEmbedding = new Mock<IEmbeddingClientWrapper>();

		string? capturedInput = null;

		mockEmbedding.Setup(x => x.GenerateEmbeddingAsync(
						It.IsAny<string>(),
						It.IsAny<CancellationToken>()))
				.Callback<string, CancellationToken>((input, ct) => capturedInput = input)
				.ThrowsAsync(new Exception("Test")); // Throw to prevent null ref on response

		var service = new OpenAiService(Options.Create(_options), _context, mockChat.Object, mockEmbedding.Object);

		// Act
		const string testText = "Test text for embedding";
		await service.GenerateEmbeddingAsync(testText);

		// Assert
		capturedInput.Should().Be(testText);
		mockEmbedding.Verify(x => x.GenerateEmbeddingAsync(testText, It.IsAny<CancellationToken>()), Times.Once);
	}

	#endregion

	#region GenerateTagsAsync Tests

	[Fact]
	public async Task GenerateTagsAsync_WithNullTitleAndContent_ReturnsEmptyString()
	{
		// Arrange
		var mockChat = new Mock<IChatClientWrapper>();
		var mockEmbedding = new Mock<IEmbeddingClientWrapper>();
		var service = new OpenAiService(Options.Create(_options), _context, mockChat.Object, mockEmbedding.Object);

		// Act
		var result = await service.GenerateTagsAsync(null!, null!);

		// Assert
		result.Should().BeEmpty();
		mockChat.Verify(x => x.CompleteChatAsync(
						It.IsAny<IEnumerable<ChatMessage>>(),
						It.IsAny<ChatCompletionOptions>(),
						It.IsAny<CancellationToken>()),
				Times.Never);
	}

	[Fact]
	public async Task GenerateTagsAsync_WithEmptyTitleAndContent_ReturnsEmptyString()
	{
		// Arrange
		var mockChat = new Mock<IChatClientWrapper>();
		var mockEmbedding = new Mock<IEmbeddingClientWrapper>();
		var service = new OpenAiService(Options.Create(_options), _context, mockChat.Object, mockEmbedding.Object);

		// Act
		var result = await service.GenerateTagsAsync(string.Empty, string.Empty);

		// Assert
		result.Should().BeEmpty();
		mockChat.Verify(x => x.CompleteChatAsync(
						It.IsAny<IEnumerable<ChatMessage>>(),
						It.IsAny<ChatCompletionOptions>(),
						It.IsAny<CancellationToken>()),
				Times.Never);
	}

	[Fact]
	public async Task GenerateTagsAsync_WithWhitespaceTitleAndContent_ReturnsEmptyString()
	{
		// Arrange
		var mockChat = new Mock<IChatClientWrapper>();
		var mockEmbedding = new Mock<IEmbeddingClientWrapper>();
		var service = new OpenAiService(Options.Create(_options), _context, mockChat.Object, mockEmbedding.Object);

		// Act
		var result = await service.GenerateTagsAsync("  \t  ", "  \n  ");

		// Assert
		result.Should().BeEmpty();
		mockChat.Verify(x => x.CompleteChatAsync(
						It.IsAny<IEnumerable<ChatMessage>>(),
						It.IsAny<ChatCompletionOptions>(),
						It.IsAny<CancellationToken>()),
				Times.Never);
	}

	[Fact]
	public async Task GenerateTagsAsync_WhenExceptionThrown_ReturnsEmptyString()
	{
		// Arrange
		var mockChat = new Mock<IChatClientWrapper>();
		mockChat.Setup(x => x.CompleteChatAsync(
						It.IsAny<IEnumerable<ChatMessage>>(),
						It.IsAny<ChatCompletionOptions>(),
						It.IsAny<CancellationToken>()))
				.ThrowsAsync(new Exception("API Error"));

		var mockEmbedding = new Mock<IEmbeddingClientWrapper>();
		var service = new OpenAiService(Options.Create(_options), _context, mockChat.Object, mockEmbedding.Object);

		// Act
		var result = await service.GenerateTagsAsync("Title", "Content");

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task GenerateTagsAsync_CallsChatClient_WithCorrectParameters()
	{
		// Arrange
		var mockChat = new Mock<IChatClientWrapper>();
		var mockEmbedding = new Mock<IEmbeddingClientWrapper>();

		List<ChatMessage>? capturedMessages = null;
		ChatCompletionOptions? capturedOptions = null;

		mockChat.Setup(x => x.CompleteChatAsync(
						It.IsAny<IEnumerable<ChatMessage>>(),
						It.IsAny<ChatCompletionOptions>(),
						It.IsAny<CancellationToken>()))
				.Callback<IEnumerable<ChatMessage>, ChatCompletionOptions?, CancellationToken>((msgs, opts, ct) =>
				{
					capturedMessages = msgs.ToList();
					capturedOptions = opts;
				})
				.ThrowsAsync(new Exception("Test")); // Throw to prevent null ref on response

		var service = new OpenAiService(Options.Create(_options), _context, mockChat.Object, mockEmbedding.Object);

		// Act
		await service.GenerateTagsAsync("Test Title", "Test Content");

		// Assert
		capturedMessages.Should().NotBeNull();
		capturedMessages.Should().HaveCount(2);
		capturedMessages![0].Should().BeOfType<SystemChatMessage>();
		capturedMessages[1].Should().BeOfType<UserChatMessage>();

		capturedOptions.Should().NotBeNull();
		capturedOptions!.MaxOutputTokenCount.Should().Be(50);
		capturedOptions.Temperature.Should().Be(0.3f);
	}

	#endregion

}
