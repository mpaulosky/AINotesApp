// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     IAiServiceTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;

using AINotesApp.Services;

using FluentAssertions;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AINotesApp.Tests.Unit.Services.Ai;

/// <summary>
///   Unit tests for IAiService interface contract.
/// </summary>
[ExcludeFromCodeCoverage]
public class IAiServiceTests
{

	private readonly IAiService _aiService;

	public IAiServiceTests()
	{
		_aiService = Substitute.For<IAiService>();
	}

	#region GenerateSummaryAsync Tests

	[Fact]
	public async Task GenerateSummaryAsync_WithValidContent_ReturnsSummary()
	{
		// Given
		const string content = "This is a test note with important information about AI and machine learning.";
		const string expectedSummary = "A note about AI and machine learning";
		var cancellationToken = CancellationToken.None;

		_aiService
			.GenerateSummaryAsync(content, cancellationToken)
			.Returns(expectedSummary);

		// When
		var result = await _aiService.GenerateSummaryAsync(content, cancellationToken);

		// Then
		result.Should().NotBeNullOrEmpty();
		result.Should().Be(expectedSummary);
		await _aiService.Received(1).GenerateSummaryAsync(content, cancellationToken);
	}

	[Fact]
	public async Task GenerateSummaryAsync_WithEmptyContent_ReturnsEmptyString()
	{
		// Given
		const string content = "";
		const string expectedSummary = "";
		var cancellationToken = CancellationToken.None;

		_aiService
			.GenerateSummaryAsync(content, cancellationToken)
			.Returns(expectedSummary);

		// When
		var result = await _aiService.GenerateSummaryAsync(content, cancellationToken);

		// Then
		result.Should().BeEmpty();
		await _aiService.Received(1).GenerateSummaryAsync(content, cancellationToken);
	}

	[Fact]
	public async Task GenerateSummaryAsync_WithLongContent_ReturnsCondensedSummary()
	{
		// Given
		var longContent = string.Join(" ", Enumerable.Repeat("Lorem ipsum dolor sit amet.", 100));
		const string expectedSummary = "A lengthy note containing repeated Latin text.";
		var cancellationToken = CancellationToken.None;

		_aiService
			.GenerateSummaryAsync(longContent, cancellationToken)
			.Returns(expectedSummary);

		// When
		var result = await _aiService.GenerateSummaryAsync(longContent, cancellationToken);

		// Then
		result.Should().NotBeNullOrEmpty();
		result.Length.Should().BeLessThan(longContent.Length);
		result.Should().Be(expectedSummary);
	}

	[Fact]
	public async Task GenerateSummaryAsync_WithCancellationToken_HandlesCancellation()
	{
		// Given
		const string content = "Test content";
		var cancellationTokenSource = new CancellationTokenSource();
		cancellationTokenSource.Cancel();

		_aiService
			.GenerateSummaryAsync(content, cancellationTokenSource.Token)
			.ThrowsAsyncForAnyArgs(new OperationCanceledException());

		// When & Then
		await Assert.ThrowsAsync<OperationCanceledException>(() =>
			_aiService.GenerateSummaryAsync(content, cancellationTokenSource.Token));
	}

	[Fact]
	public async Task GenerateSummaryAsync_WithDefaultCancellationToken_CompletesSuccessfully()
	{
		// Given
		const string content = "Test content";
		const string expectedSummary = "Test summary";

		_aiService
			.GenerateSummaryAsync(content, default)
			.Returns(expectedSummary);

		// When
		var result = await _aiService.GenerateSummaryAsync(content);

		// Then
		result.Should().Be(expectedSummary);
	}

	#endregion

	#region GenerateEmbeddingAsync Tests

	[Fact]
	public async Task GenerateEmbeddingAsync_WithValidText_ReturnsEmbeddingVector()
	{
		// Given
		const string text = "Sample text for embedding generation";
		var expectedEmbedding = new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f };
		var cancellationToken = CancellationToken.None;

		_aiService
			.GenerateEmbeddingAsync(text, cancellationToken)
			.Returns(expectedEmbedding);

		// When
		var result = await _aiService.GenerateEmbeddingAsync(text, cancellationToken);

		// Then
		result.Should().NotBeNull();
		result.Should().HaveCount(5);
		result.Should().BeEquivalentTo(expectedEmbedding);
		await _aiService.Received(1).GenerateEmbeddingAsync(text, cancellationToken);
	}

	[Fact]
	public async Task GenerateEmbeddingAsync_WithEmptyText_ReturnsEmptyVector()
	{
		// Given
		const string text = "";
		var expectedEmbedding = Array.Empty<float>();
		var cancellationToken = CancellationToken.None;

		_aiService
			.GenerateEmbeddingAsync(text, cancellationToken)
			.Returns(expectedEmbedding);

		// When
		var result = await _aiService.GenerateEmbeddingAsync(text, cancellationToken);

		// Then
		result.Should().NotBeNull();
		result.Should().BeEmpty();
		await _aiService.Received(1).GenerateEmbeddingAsync(text, cancellationToken);
	}

	[Fact]
	public async Task GenerateEmbeddingAsync_ReturnsConsistentVectorDimensions()
	{
		// Given
		const string text1 = "First text";
		const string text2 = "Second text";
		var embedding1 = new float[] { 0.1f, 0.2f, 0.3f };
		var embedding2 = new float[] { 0.4f, 0.5f, 0.6f };

		_aiService.GenerateEmbeddingAsync(text1, default).Returns(embedding1);
		_aiService.GenerateEmbeddingAsync(text2, default).Returns(embedding2);

		// When
		var result1 = await _aiService.GenerateEmbeddingAsync(text1);
		var result2 = await _aiService.GenerateEmbeddingAsync(text2);

		// Then
		result1.Length.Should().Be(result2.Length);
	}

	[Fact]
	public async Task GenerateEmbeddingAsync_WithCancellationToken_HandlesCancellation()
	{
		// Given
		const string text = "Test text";
		var cancellationTokenSource = new CancellationTokenSource();
		cancellationTokenSource.Cancel();

		_aiService
			.GenerateEmbeddingAsync(text, cancellationTokenSource.Token)
			.ThrowsAsyncForAnyArgs(new OperationCanceledException());

		// When & Then
		await Assert.ThrowsAsync<OperationCanceledException>(() =>
			_aiService.GenerateEmbeddingAsync(text, cancellationTokenSource.Token));
	}

	[Fact]
	public async Task GenerateEmbeddingAsync_WithLongText_ReturnsEmbedding()
	{
		// Given
		var longText = string.Join(" ", Enumerable.Repeat("word", 1000));
		var expectedEmbedding = new float[] { 0.1f, 0.2f, 0.3f };
		var cancellationToken = CancellationToken.None;

		_aiService
			.GenerateEmbeddingAsync(longText, cancellationToken)
			.Returns(expectedEmbedding);

		// When
		var result = await _aiService.GenerateEmbeddingAsync(longText, cancellationToken);

		// Then
		result.Should().NotBeNull();
		result.Should().HaveCount(3);
	}

	#endregion

	#region GenerateTagsAsync Tests

	[Fact]
	public async Task GenerateTagsAsync_WithValidTitleAndContent_ReturnsTags()
	{
		// Given
		const string title = "AI Notes";
		const string content = "This note discusses artificial intelligence and machine learning concepts.";
		const string expectedTags = "ai, machine learning, artificial intelligence";
		var cancellationToken = CancellationToken.None;

		_aiService
			.GenerateTagsAsync(title, content, cancellationToken)
			.Returns(expectedTags);

		// When
		var result = await _aiService.GenerateTagsAsync(title, content, cancellationToken);

		// Then
		result.Should().NotBeNullOrEmpty();
		result.Should().Be(expectedTags);
		result.Should().Contain(",");
		await _aiService.Received(1).GenerateTagsAsync(title, content, cancellationToken);
	}

	[Fact]
	public async Task GenerateTagsAsync_WithEmptyContent_ReturnsEmptyTags()
	{
		// Given
		const string title = "";
		const string content = "";
		const string expectedTags = "";
		var cancellationToken = CancellationToken.None;

		_aiService
			.GenerateTagsAsync(title, content, cancellationToken)
			.Returns(expectedTags);

		// When
		var result = await _aiService.GenerateTagsAsync(title, content, cancellationToken);

		// Then
		result.Should().BeEmpty();
		await _aiService.Received(1).GenerateTagsAsync(title, content, cancellationToken);
	}

	[Fact]
	public async Task GenerateTagsAsync_ReturnsCommaSeparatedTags()
	{
		// Given
		const string title = "Programming Tutorial";
		const string content = "Learn C# and .NET development";
		const string expectedTags = "programming, csharp, dotnet, tutorial";

		_aiService
			.GenerateTagsAsync(title, content, default)
			.Returns(expectedTags);

		// When
		var result = await _aiService.GenerateTagsAsync(title, content);

		// Then
		var tags = result.Split(',').Select(t => t.Trim()).ToList();
		tags.Should().HaveCountGreaterThan(0);
		tags.All(t => !string.IsNullOrWhiteSpace(t)).Should().BeTrue();
	}

	[Fact]
	public async Task GenerateTagsAsync_WithOnlyTitle_GeneratesTags()
	{
		// Given
		const string title = "Machine Learning Basics";
		const string content = "";
		const string expectedTags = "machine learning, basics";
		var cancellationToken = CancellationToken.None;

		_aiService
			.GenerateTagsAsync(title, content, cancellationToken)
			.Returns(expectedTags);

		// When
		var result = await _aiService.GenerateTagsAsync(title, content, cancellationToken);

		// Then
		result.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public async Task GenerateTagsAsync_WithCancellationToken_HandlesCancellation()
	{
		// Given
		const string title = "Test Title";
		const string content = "Test Content";
		var cancellationTokenSource = new CancellationTokenSource();
		cancellationTokenSource.Cancel();

		_aiService
			.GenerateTagsAsync(title, content, cancellationTokenSource.Token)
			.ThrowsAsyncForAnyArgs(new OperationCanceledException());

		// When & Then
		await Assert.ThrowsAsync<OperationCanceledException>(() =>
			_aiService.GenerateTagsAsync(title, content, cancellationTokenSource.Token));
	}

	#endregion

	#region FindRelatedNotesAsync Tests

	[Fact]
	public async Task FindRelatedNotesAsync_WithValidEmbedding_ReturnsRelatedNoteIds()
	{
		// Given
		var embedding = new float[] { 0.1f, 0.2f, 0.3f };
		const string userSubject = "auth0|123456";
		var currentNoteId = Guid.NewGuid();
		const int topN = 5;
		var expectedNoteIds = new List<Guid>
		{
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid()
		};
		var cancellationToken = CancellationToken.None;

		_aiService
			.FindRelatedNotesAsync(embedding, userSubject, currentNoteId, topN, cancellationToken)
			.Returns(expectedNoteIds);

		// When
		var result = await _aiService.FindRelatedNotesAsync(
			embedding, userSubject, currentNoteId, topN, cancellationToken);

		// Then
		result.Should().NotBeNull();
		result.Should().HaveCount(3);
		result.Should().BeEquivalentTo(expectedNoteIds);
		await _aiService.Received(1).FindRelatedNotesAsync(
			embedding, userSubject, currentNoteId, topN, cancellationToken);
	}

	[Fact]
	public async Task FindRelatedNotesAsync_WithNullCurrentNoteId_ReturnsAllRelatedNotes()
	{
		// Given
		var embedding = new float[] { 0.1f, 0.2f, 0.3f };
		const string userSubject = "auth0|123456";
		Guid? currentNoteId = null;
		const int topN = 10;
		var expectedNoteIds = new List<Guid>
		{
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid()
		};
		var cancellationToken = CancellationToken.None;

		_aiService
			.FindRelatedNotesAsync(embedding, userSubject, currentNoteId, topN, cancellationToken)
			.Returns(expectedNoteIds);

		// When
		var result = await _aiService.FindRelatedNotesAsync(
			embedding, userSubject, currentNoteId, topN, cancellationToken);

		// Then
		result.Should().NotBeNull();
		result.Should().HaveCount(4);
	}

	[Fact]
	public async Task FindRelatedNotesAsync_WithNullTopN_UsesDefaultLimit()
	{
		// Given
		var embedding = new float[] { 0.1f, 0.2f, 0.3f };
		const string userSubject = "auth0|123456";
		var currentNoteId = Guid.NewGuid();
		int? topN = null;
		var expectedNoteIds = new List<Guid> { Guid.NewGuid() };
		var cancellationToken = CancellationToken.None;

		_aiService
			.FindRelatedNotesAsync(embedding, userSubject, currentNoteId, topN, cancellationToken)
			.Returns(expectedNoteIds);

		// When
		var result = await _aiService.FindRelatedNotesAsync(
			embedding, userSubject, currentNoteId, topN, cancellationToken);

		// Then
		result.Should().NotBeNull();
		await _aiService.Received(1).FindRelatedNotesAsync(
			embedding, userSubject, currentNoteId, topN, cancellationToken);
	}

	[Fact]
	public async Task FindRelatedNotesAsync_WithNoRelatedNotes_ReturnsEmptyList()
	{
		// Given
		var embedding = new float[] { 0.1f, 0.2f, 0.3f };
		const string userSubject = "auth0|123456";
		var currentNoteId = Guid.NewGuid();
		const int topN = 5;
		var expectedNoteIds = new List<Guid>();
		var cancellationToken = CancellationToken.None;

		_aiService
			.FindRelatedNotesAsync(embedding, userSubject, currentNoteId, topN, cancellationToken)
			.Returns(expectedNoteIds);

		// When
		var result = await _aiService.FindRelatedNotesAsync(
			embedding, userSubject, currentNoteId, topN, cancellationToken);

		// Then
		result.Should().NotBeNull();
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task FindRelatedNotesAsync_WithEmptyEmbedding_ReturnsEmptyList()
	{
		// Given
		var embedding = Array.Empty<float>();
		const string userSubject = "auth0|123456";
		var expectedNoteIds = new List<Guid>();

		_aiService
			.FindRelatedNotesAsync(embedding, userSubject, null, null, default)
			.Returns(expectedNoteIds);

		// When
		var result = await _aiService.FindRelatedNotesAsync(embedding, userSubject);

		// Then
		result.Should().NotBeNull();
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task FindRelatedNotesAsync_ResultsOrderedBySimilarity()
	{
		// Given
		var embedding = new float[] { 0.1f, 0.2f, 0.3f };
		const string userSubject = "auth0|123456";
		var noteId1 = Guid.NewGuid();
		var noteId2 = Guid.NewGuid();
		var noteId3 = Guid.NewGuid();
		var expectedNoteIds = new List<Guid> { noteId1, noteId2, noteId3 };

		_aiService
			.FindRelatedNotesAsync(embedding, userSubject, null, null, default)
			.Returns(expectedNoteIds);

		// When
		var result = await _aiService.FindRelatedNotesAsync(embedding, userSubject);

		// Then
		result.Should().ContainInOrder(noteId1, noteId2, noteId3);
	}

	[Fact]
	public async Task FindRelatedNotesAsync_ExcludesCurrentNote_WhenSpecified()
	{
		// Given
		var embedding = new float[] { 0.1f, 0.2f, 0.3f };
		const string userSubject = "auth0|123456";
		var currentNoteId = Guid.NewGuid();
		var otherNoteId = Guid.NewGuid();
		var expectedNoteIds = new List<Guid> { otherNoteId };

		_aiService
			.FindRelatedNotesAsync(embedding, userSubject, currentNoteId, null, default)
			.Returns(expectedNoteIds);

		// When
		var result = await _aiService.FindRelatedNotesAsync(embedding, userSubject, currentNoteId);

		// Then
		result.Should().NotContain(currentNoteId);
		result.Should().Contain(otherNoteId);
	}

	[Fact]
	public async Task FindRelatedNotesAsync_WithCancellationToken_HandlesCancellation()
	{
		// Given
		var embedding = new float[] { 0.1f, 0.2f, 0.3f };
		const string userSubject = "auth0|123456";
		var currentNoteId = Guid.NewGuid();
		const int topN = 5;
		var cancellationTokenSource = new CancellationTokenSource();
		cancellationTokenSource.Cancel();

		_aiService
			.FindRelatedNotesAsync(
				embedding, userSubject, currentNoteId, topN, cancellationTokenSource.Token)
			.ThrowsAsyncForAnyArgs(new OperationCanceledException());

		// When & Then
		await Assert.ThrowsAsync<OperationCanceledException>(() =>
			_aiService.FindRelatedNotesAsync(
				embedding, userSubject, currentNoteId, topN, cancellationTokenSource.Token));
	}

	[Fact]
	public async Task FindRelatedNotesAsync_WithDifferentUserSubjects_ReturnsOnlyUserNotes()
	{
		// Given
		var embedding = new float[] { 0.1f, 0.2f, 0.3f };
		const string userSubject = "auth0|user1";
		var expectedNoteIds = new List<Guid> { Guid.NewGuid() };

		_aiService
			.FindRelatedNotesAsync(embedding, userSubject, null, null, default)
			.Returns(expectedNoteIds);

		// When
		var result = await _aiService.FindRelatedNotesAsync(embedding, userSubject);

		// Then
		result.Should().NotBeNull();
		result.Should().HaveCount(1);
	}

	#endregion

}
