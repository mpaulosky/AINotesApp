namespace AINotesApp.Services.Ai;

/// <summary>
/// Configuration options for OpenAI API integration.
/// </summary>
public class AiServiceOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "OpenAI";

    /// <summary>
    /// Gets or sets the OpenAI API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the model to use for chat completions (summaries).
    /// </summary>
    public string ChatModel { get; set; } = "gpt-4o-mini";

    /// <summary>
    /// Gets or sets the model to use for embeddings.
    /// </summary>
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";

    /// <summary>
    /// Gets or sets the maximum number of tokens for summary generation.
    /// </summary>
    public int MaxSummaryTokens { get; set; } = 150;

    /// <summary>
    /// Gets or sets the number of related notes to return.
    /// </summary>
    public int RelatedNotesCount { get; set; } = 5;

    /// <summary>
    /// Gets or sets the similarity threshold for related notes (0.0 to 1.0).
    /// </summary>
    public double SimilarityThreshold { get; set; } = 0.7;
}
