using Azure.AI.OpenAI;
using GenSearch.Models;

namespace GenSearch.Services;

public class AzureOpenAIAnswerService : IGenerativeAnswerService
{
    private readonly OpenAIClient _client;
    private readonly string _deploymentName;
    private readonly ILogger<AzureOpenAIAnswerService> _logger;

    public AzureOpenAIAnswerService(IConfiguration config, ILogger<AzureOpenAIAnswerService> logger)
    {
        _client = new OpenAIClient(
            new Uri(config["AzureOpenAI:Endpoint"]!),
            new Azure.AzureKeyCredential(config["AzureOpenAI:ApiKey"]!));
        _deploymentName = config["AzureOpenAI:DeploymentName"]!;
        _logger = logger;
    }

    private string BuildSystemPrompt(IEnumerable<DocumentChunk> chunks)
    {
        var context = string.Join("\n\n", chunks.Select((c, i) =>
            $"[Source {i + 1}] {c.Title}\n{c.Content}"));

        return $"""
            You are a helpful assistant answering questions based on the provided sources.
            Always cite sources using [Source N] notation.
            Be concise, accurate, and grounded in the provided context.
            If the answer is not in the sources, say so clearly.

            SOURCES:
            {context}
            """;
    }

    public async Task<GeneratedAnswer> GenerateAnswerAsync(string query, IEnumerable<DocumentChunk> chunks)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var options = new ChatCompletionsOptions
        {
            DeploymentName = _deploymentName,
            MaxTokens = 1000,
            Temperature = 0.3f,
            Messages =
            {
                new ChatRequestSystemMessage(BuildSystemPrompt(chunks)),
                new ChatRequestUserMessage(query)
            }
        };

        var response = await _client.GetChatCompletionsAsync(options);
        sw.Stop();

        var content = response.Value.Choices[0].Message.Content;

        return new GeneratedAnswer
        {
            Text = content,
            FollowUpQuestions = GenerateFollowUps(query),
            LatencyMs = sw.ElapsedMilliseconds
        };
    }

    public async IAsyncEnumerable<string> StreamAnswerAsync(
        string query,
        IEnumerable<DocumentChunk> chunks,
        CancellationToken ct)
    {
        var options = new ChatCompletionsOptions
        {
            DeploymentName = _deploymentName,
            MaxTokens = 1000,
            Temperature = 0.3f,
            Messages =
            {
                new ChatRequestSystemMessage(BuildSystemPrompt(chunks)),
                new ChatRequestUserMessage(query)
            }
        };

        await foreach (var update in _client.GetChatCompletionsStreamingAsync(options, ct))
        {
            foreach (var choice in update.Choices)
            {
                if (choice.Delta?.Content is { } token && !string.IsNullOrEmpty(token))
                    yield return token;
            }
        }
    }

    private static List<string> GenerateFollowUps(string query) =>
        new()
        {
            $"Can you explain more about {query.Split(' ').FirstOrDefault()}?",
            "What are the key takeaways?",
            "Are there any limitations to consider?"
        };
}
