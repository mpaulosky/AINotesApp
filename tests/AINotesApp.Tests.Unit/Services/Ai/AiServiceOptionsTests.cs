// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     AiServiceOptionsTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;

using AINotesApp.Services.Ai;

using FluentAssertions;

namespace AINotesApp.Tests.Unit.Services.Ai;

/// <summary>
///   Unit tests for AiServiceOptions configuration class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AiServiceOptionsTests
{

	[Fact]
	public void Constructor_InitializesWithDefaultValues()
	{
		// Given & When
		var options = new AiServiceOptions();

		// Then
		options.ApiKey.Should().Be(string.Empty);
		options.ChatModel.Should().Be("gpt-4o-mini");
		options.EmbeddingModel.Should().Be("text-embedding-3-small");
		options.MaxSummaryTokens.Should().Be(150);
		options.RelatedNotesCount.Should().Be(5);
		options.SimilarityThreshold.Should().Be(0.7);
	}

	[Fact]
	public void SectionName_ShouldHaveCorrectValue()
	{
		// Given & When
		var sectionName = AiServiceOptions.SectionName;

		// Then
		sectionName.Should().Be("OpenAI");
	}

	[Fact]
	public void ApiKey_CanBeSetAndRetrieved()
	{
		// Given
		var options = new AiServiceOptions();
		var testApiKey = "test-api-key-12345";

		// When
		options.ApiKey = testApiKey;

		// Then
		options.ApiKey.Should().Be(testApiKey);
	}

	[Fact]
	public void ApiKey_CanBeSetToEmpty()
	{
		// Given
		var options = new AiServiceOptions { ApiKey = "some-key" };

		// When
		options.ApiKey = string.Empty;

		// Then
		options.ApiKey.Should().Be(string.Empty);
	}

	[Fact]
	public void ChatModel_CanBeSetAndRetrieved()
	{
		// Given
		var options = new AiServiceOptions();
		var testModel = "gpt-4-turbo";

		// When
		options.ChatModel = testModel;

		// Then
		options.ChatModel.Should().Be(testModel);
	}

	[Fact]
	public void ChatModel_CanBeSetToEmpty()
	{
		// Given
		var options = new AiServiceOptions();

		// When
		options.ChatModel = string.Empty;

		// Then
		options.ChatModel.Should().Be(string.Empty);
	}

	[Fact]
	public void EmbeddingModel_CanBeSetAndRetrieved()
	{
		// Given
		var options = new AiServiceOptions();
		var testModel = "text-embedding-3-large";

		// When
		options.EmbeddingModel = testModel;

		// Then
		options.EmbeddingModel.Should().Be(testModel);
	}

	[Fact]
	public void EmbeddingModel_CanBeSetToEmpty()
	{
		// Given
		var options = new AiServiceOptions();

		// When
		options.EmbeddingModel = string.Empty;

		// Then
		options.EmbeddingModel.Should().Be(string.Empty);
	}

	[Fact]
	public void MaxSummaryTokens_CanBeSetAndRetrieved()
	{
		// Given
		var options = new AiServiceOptions();
		var testTokens = 300;

		// When
		options.MaxSummaryTokens = testTokens;

		// Then
		options.MaxSummaryTokens.Should().Be(testTokens);
	}

	[Fact]
	public void MaxSummaryTokens_CanBeSetToZero()
	{
		// Given
		var options = new AiServiceOptions();

		// When
		options.MaxSummaryTokens = 0;

		// Then
		options.MaxSummaryTokens.Should().Be(0);
	}

	[Fact]
	public void MaxSummaryTokens_CanBeSetToNegativeValue()
	{
		// Given
		var options = new AiServiceOptions();

		// When
		options.MaxSummaryTokens = -100;

		// Then
		options.MaxSummaryTokens.Should().Be(-100);
	}

	[Fact]
	public void RelatedNotesCount_CanBeSetAndRetrieved()
	{
		// Given
		var options = new AiServiceOptions();
		var testCount = 10;

		// When
		options.RelatedNotesCount = testCount;

		// Then
		options.RelatedNotesCount.Should().Be(testCount);
	}

	[Fact]
	public void RelatedNotesCount_CanBeSetToZero()
	{
		// Given
		var options = new AiServiceOptions();

		// When
		options.RelatedNotesCount = 0;

		// Then
		options.RelatedNotesCount.Should().Be(0);
	}

	[Fact]
	public void RelatedNotesCount_CanBeSetToNegativeValue()
	{
		// Given
		var options = new AiServiceOptions();

		// When
		options.RelatedNotesCount = -5;

		// Then
		options.RelatedNotesCount.Should().Be(-5);
	}

	[Fact]
	public void SimilarityThreshold_CanBeSetAndRetrieved()
	{
		// Given
		var options = new AiServiceOptions();
		var testThreshold = 0.85;

		// When
		options.SimilarityThreshold = testThreshold;

		// Then
		options.SimilarityThreshold.Should().Be(testThreshold);
	}

	[Fact]
	public void SimilarityThreshold_CanBeSetToZero()
	{
		// Given
		var options = new AiServiceOptions();

		// When
		options.SimilarityThreshold = 0.0;

		// Then
		options.SimilarityThreshold.Should().Be(0.0);
	}

	[Fact]
	public void SimilarityThreshold_CanBeSetToOne()
	{
		// Given
		var options = new AiServiceOptions();

		// When
		options.SimilarityThreshold = 1.0;

		// Then
		options.SimilarityThreshold.Should().Be(1.0);
	}

	[Fact]
	public void SimilarityThreshold_CanBeSetToNegativeValue()
	{
		// Given
		var options = new AiServiceOptions();

		// When
		options.SimilarityThreshold = -0.5;

		// Then
		options.SimilarityThreshold.Should().Be(-0.5);
	}

	[Fact]
	public void SimilarityThreshold_CanBeSetToValueGreaterThanOne()
	{
		// Given
		var options = new AiServiceOptions();

		// When
		options.SimilarityThreshold = 1.5;

		// Then
		options.SimilarityThreshold.Should().Be(1.5);
	}

	[Fact]
	public void AllProperties_CanBeSetViaObjectInitializer()
	{
		// Given & When
		var options = new AiServiceOptions
		{
			ApiKey = "custom-api-key",
			ChatModel = "gpt-4",
			EmbeddingModel = "ada-002",
			MaxSummaryTokens = 500,
			RelatedNotesCount = 10,
			SimilarityThreshold = 0.8
		};

		// Then
		options.ApiKey.Should().Be("custom-api-key");
		options.ChatModel.Should().Be("gpt-4");
		options.EmbeddingModel.Should().Be("ada-002");
		options.MaxSummaryTokens.Should().Be(500);
		options.RelatedNotesCount.Should().Be(10);
		options.SimilarityThreshold.Should().Be(0.8);
	}

	[Fact]
	public void Properties_CanBeModifiedAfterInstantiation()
	{
		// Given
		var options = new AiServiceOptions
		{
			ApiKey = "initial-key",
			MaxSummaryTokens = 100
		};

		// When
		options.ApiKey = "modified-key";
		options.MaxSummaryTokens = 200;

		// Then
		options.ApiKey.Should().Be("modified-key");
		options.MaxSummaryTokens.Should().Be(200);
	}

	[Fact]
	public void ChatModel_DefaultValue_IsGpt4oMini()
	{
		// Given & When
		var options = new AiServiceOptions();

		// Then
		options.ChatModel.Should().Be("gpt-4o-mini");
	}

	[Fact]
	public void EmbeddingModel_DefaultValue_IsTextEmbedding3Small()
	{
		// Given & When
		var options = new AiServiceOptions();

		// Then
		options.EmbeddingModel.Should().Be("text-embedding-3-small");
	}

	[Theory]
	[InlineData(50)]
	[InlineData(100)]
	[InlineData(150)]
	[InlineData(500)]
	[InlineData(1000)]
	public void MaxSummaryTokens_AcceptsVariousPositiveValues(int tokens)
	{
		// Given
		var options = new AiServiceOptions();

		// When
		options.MaxSummaryTokens = tokens;

		// Then
		options.MaxSummaryTokens.Should().Be(tokens);
	}

	[Theory]
	[InlineData(1)]
	[InlineData(3)]
	[InlineData(5)]
	[InlineData(10)]
	[InlineData(20)]
	public void RelatedNotesCount_AcceptsVariousPositiveValues(int count)
	{
		// Given
		var options = new AiServiceOptions();

		// When
		options.RelatedNotesCount = count;

		// Then
		options.RelatedNotesCount.Should().Be(count);
	}

	[Theory]
	[InlineData(0.0)]
	[InlineData(0.3)]
	[InlineData(0.5)]
	[InlineData(0.7)]
	[InlineData(0.9)]
	[InlineData(1.0)]
	public void SimilarityThreshold_AcceptsVariousValidValues(double threshold)
	{
		// Given
		var options = new AiServiceOptions();

		// When
		options.SimilarityThreshold = threshold;

		// Then
		options.SimilarityThreshold.Should().Be(threshold);
	}

}
