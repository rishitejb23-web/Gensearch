// app/page.tsx  — SSR entry point
import { Suspense } from 'react'
import SearchBar from '@/components/SearchBar'
import AnswerCard from '@/components/AnswerCard'
import SourcePanel from '@/components/SourcePanel'
import FollowUpChips from '@/components/FollowUpChips'
import { SearchResponse } from '@/types'

interface PageProps {
  searchParams: { q?: string }
}

// Server-side fetch for initial query (SSR for sub-200ms TTFB)
async function fetchAnswer(query: string): Promise<SearchResponse | null> {
  if (!query) return null
  try {
    const res = await fetch(`${process.env.API_BASE_URL}/api/search/query`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ query }),
      next: { revalidate: 60 } // edge cache for repeated queries
    })
    return res.ok ? res.json() : null
  } catch {
    return null
  }
}

export default async function HomePage({ searchParams }: PageProps) {
  const query = searchParams.q ?? ''
  const initialResult = await fetchAnswer(query)

  return (
    <main className="min-h-screen bg-gray-50">
      <div className="max-w-3xl mx-auto px-4 py-12">

        {/* Logo */}
        <div className="text-center mb-10">
          <h1 className="text-4xl font-bold text-navy-800 tracking-tight">GenSearch</h1>
          <p className="text-gray-500 mt-1 text-sm">AI-powered answers, grounded in sources</p>
        </div>

        {/* Search input */}
        <SearchBar initialQuery={query} />

        {/* Results */}
        {query && (
          <Suspense fallback={<AnswerCard.Skeleton />}>
            <div className="mt-8 space-y-6">
              <AnswerCard
                query={query}
                initialAnswer={initialResult?.answer}
                streaming={!initialResult}
              />
              {initialResult && (
                <>
                  <SourcePanel sources={initialResult.sources} />
                  <FollowUpChips questions={initialResult.followUpQuestions} />
                </>
              )}
            </div>
          </Suspense>
        )}
      </div>
    </main>
  )
}
