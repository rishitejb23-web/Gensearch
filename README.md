# GenSearch — Full Stack Generative Answer Engine

A production-grade RAG-powered search engine with streaming generative answers.
Architecturally mirrors **Microsoft Copilot** and **Bing generative answers**.

## Stack
- **Backend**: C#/.NET 8, ASP.NET Core, Azure OpenAI GPT-4, pgvector (RAG)
- **Frontend**: Next.js 14, React, TypeScript, Tailwind CSS (SSR, edge caching)
- **Infra**: Azure AKS, OpenTelemetry, GitHub Actions CI/CD

## Features
- Real-time streaming answers via SSE (token-by-token, like Copilot)
- RAG pipeline: query → embedding → vector similarity → grounded generation
- Source attribution with numbered citations
- Follow-up question chips
- Sub-200ms TTFB via SSR + edge caching
- Core Web Vitals optimized

## Architecture
```
User Query
    │
    ▼
Next.js SSR (TTFB < 200ms)
    │
    ▼
C# ASP.NET Core API
    ├── Embedding Service (Azure OpenAI)
    ├── Vector Store (pgvector similarity search)
    └── Answer Service (GPT-4 streaming via SSE)
    │
    ▼
React frontend renders tokens as they stream
```

## Running locally
```bash
# Backend
cd backend
dotnet run

# Frontend
cd frontend
npm install && npm run dev
```

## Environment variables
```
AZURE_OPENAI_ENDPOINT=...
AZURE_OPENAI_KEY=...
AZURE_OPENAI_DEPLOYMENT=gpt-4
POSTGRES_CONNECTION=...
REDIS_CONNECTION=...
```
