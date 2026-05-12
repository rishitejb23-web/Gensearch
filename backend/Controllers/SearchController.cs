using Microsoft.AspNetCore.Mvc;
using GenSearch.Models;
using GenSearch.Services;
using System.Runtime.CompilerServices;

namespace GenSearch.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStoreService _vectorStore;
    private readonly IGenerativeAnswerService _answerService;
    private readonly ILogger<SearchController> _logger;

    public SearchController(
        IEmbeddingService embeddingService,
        IVectorStoreService vectorStore,
        IGenerativeAnswerService answerService,
        ILogger<SearchController> logger)
    {
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
        _answerService = answerService;
        _logger = logger;
    }

    /// <summary>
    /// Non-streaming search — returns full answer with sources
    /// </summary>
    [HttpPost("query")]
    public async Task<ActionResult<SearchResponse>> Query([FromBody] SearchRequest request)
    {
        _logger.LogInformation("Query received: {Query}", request.Query);

        var embedding = await _embeddingService.GetEmbeddingAsync(request.Query);
        var chunks = await _vectorStore.SimilaritySearchAsync(embedding, topK: 5);
        var answer = await _answerService.GenerateAnswerAsync(request.Query, chunks);

        return Ok(new SearchResponse
        {
            Answer = answer.Text,
            Sources = chunks.Select(c => new SourceReference
            {
                Title = c.Title,
                Url = c.Url,
                Snippet = c.Content[..Math.Min(200, c.Content.Length)]
            }).ToList(),
            FollowUpQuestions = answer.FollowUpQuestions,
            LatencyMs = answer.LatencyMs
        });
    }

    /// <summary>
    /// Streaming search — returns answer tokens as SSE stream (mirrors Bing/Copilot streaming)
    /// </summary>
    [HttpPost("stream")]
    public async IAsyncEnumerable<string> Stream(
        [FromBody] SearchRequest request,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var embedding = await _embeddingService.GetEmbeddingAsync(request.Query);
        var chunks = await _vectorStore.SimilaritySearchAsync(embedding, topK: 5);

        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");

        await foreach (var token in _answerService.StreamAnswerAsync(request.Query, chunks, ct))
        {
            yield return $"data: {token}\n\n";
        }

        yield return "data: [DONE]\n\n";
    }
}
