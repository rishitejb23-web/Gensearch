namespace GenSearch.Models;

public record SearchRequest(string Query, int TopK = 5);

public record SearchResponse
{
    public string Answer { get; init; } = string.Empty;
    public List<SourceReference> Sources { get; init; } = new();
    public List<string> FollowUpQuestions { get; init; } = new();
    public long LatencyMs { get; init; }
}

public record SourceReference
{
    public string Title { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string Snippet { get; init; } = string.Empty;
}

public record DocumentChunk
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public float[] Embedding { get; init; } = Array.Empty<float>();
    public float Score { get; init; }
}

public record GeneratedAnswer
{
    public string Text { get; init; } = string.Empty;
    public List<string> FollowUpQuestions { get; init; } = new();
    public long LatencyMs { get; init; }
}
