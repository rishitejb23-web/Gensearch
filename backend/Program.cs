using GenSearch.Services;
using GenSearch.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IEmbeddingService, AzureEmbeddingService>();
builder.Services.AddSingleton<IVectorStoreService, PgVectorStoreService>();
builder.Services.AddSingleton<IGenerativeAnswerService, AzureOpenAIAnswerService>();
builder.Services.AddSingleton<ISearchIndexService, SearchIndexService>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(builder.Configuration["Frontend:BaseUrl"]!)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter());

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowFrontend");
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();
