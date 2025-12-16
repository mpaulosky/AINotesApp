// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     MockAiServiceHelper.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Integration
// =======================================================

using System.Diagnostics.CodeAnalysis;

using AINotesApp.Services;

using NSubstitute;

namespace AINotesApp.Tests.Integration.Helpers;

/// <summary>
///   Helper class for creating mock AI services with standard test behaviors.
/// </summary>
[ExcludeFromCodeCoverage]
public static class MockAiServiceHelper
{

	/// <summary>
	///   Creates a mock AI service with default test responses.
	/// </summary>
	/// <returns>A configured mock IAiService with standard test data.</returns>
	public static IAiService CreateMockAiService()
	{
		var aiService = Substitute.For<IAiService>();

		aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns("Test summary");

		aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns("test,tag");

		aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns([0.1f, 0.2f, 0.3f, 0.4f, 0.5f]);

		return aiService;
	}

	/// <summary>
	///   Creates a mock AI service with custom responses.
	/// </summary>
	/// <param name="summary">Custom AI summary to return.</param>
	/// <param name="tags">Custom tags to return.</param>
	/// <param name="embedding">Custom embedding array to return.</param>
	/// <returns>A configured mock IAiService with custom test data.</returns>
	public static IAiService CreateMockAiService(string summary, string tags, float[] embedding)
	{
		var aiService = Substitute.For<IAiService>();

		aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns(summary);

		aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns(tags);

		aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns(embedding);

		return aiService;
	}

	/// <summary>
	///   Creates a mock AI service that returns dynamic responses based on input.
	/// </summary>
	/// <param name="summaryFunc">Function to generate summary from content.</param>
	/// <param name="tagsFunc">Function to generate tags from title and content.</param>
	/// <param name="embeddingFunc">Function to generate embedding from content.</param>
	/// <returns>A configured mock IAiService with dynamic test behavior.</returns>
	public static IAiService CreateDynamicMockAiService(
			Func<string, string>? summaryFunc = null,
			Func<string, string, string>? tagsFunc = null,
			Func<string, float[]>? embeddingFunc = null)
	{
		var aiService = Substitute.For<IAiService>();

		if (summaryFunc != null)
		{
			aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
					.Returns(ci => summaryFunc(ci.ArgAt<string>(0)));
		}
		else
		{
			aiService.GenerateSummaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
					.Returns("Test summary");
		}

		if (tagsFunc != null)
		{
			aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
					.Returns(ci => tagsFunc(ci.ArgAt<string>(0), ci.ArgAt<string>(1)));
		}
		else
		{
			aiService.GenerateTagsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
					.Returns("test,tag");
		}

		if (embeddingFunc != null)
		{
			aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
					.Returns(ci => embeddingFunc(ci.ArgAt<string>(0)));
		}
		else
		{
			aiService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
					.Returns([0.1f, 0.2f, 0.3f, 0.4f, 0.5f]);
		}

		return aiService;
	}

}
