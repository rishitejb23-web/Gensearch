'use client'

import { useRouter } from 'next/navigation'
import { Source } from '@/types'

// ── SourcePanel ──────────────────────────────────────────────
interface SourcePanelProps {
  sources: Source[]
}

export function SourcePanel({ sources }: SourcePanelProps) {
  if (!sources.length) return null

  return (
    <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-5">
      <h3 className="text-xs font-semibold uppercase tracking-wide text-gray-400 mb-3">Sources</h3>
      <div className="space-y-3">
        {sources.map((src, i) => (
          <a
            key={i}
            href={src.url}
            target="_blank"
            rel="noopener noreferrer"
            className="flex items-start gap-3 group"
          >
            <span className="mt-0.5 flex-shrink-0 w-5 h-5 rounded-full bg-gray-100 text-gray-500
                             text-xs flex items-center justify-center font-medium group-hover:bg-blue-50
                             group-hover:text-blue-600 transition-colors">
              {i + 1}
            </span>
            <div>
              <p className="text-sm font-medium text-gray-800 group-hover:text-blue-600 transition-colors">
                {src.title}
              </p>
              <p className="text-xs text-gray-400 mt-0.5 line-clamp-2">{src.snippet}</p>
            </div>
          </a>
        ))}
      </div>
    </div>
  )
}

// ── FollowUpChips ────────────────────────────────────────────
interface FollowUpChipsProps {
  questions: string[]
}

export function FollowUpChips({ questions }: FollowUpChipsProps) {
  const router = useRouter()
  if (!questions.length) return null

  return (
    <div>
      <h3 className="text-xs font-semibold uppercase tracking-wide text-gray-400 mb-2">
        Follow-up
      </h3>
      <div className="flex flex-wrap gap-2">
        {questions.map((q, i) => (
          <button
            key={i}
            onClick={() => router.push(`/?q=${encodeURIComponent(q)}`)}
            className="text-sm px-4 py-2 rounded-full border border-gray-200 bg-white
                       text-gray-700 hover:border-blue-400 hover:text-blue-600
                       hover:bg-blue-50 transition-all"
          >
            {q}
          </button>
        ))}
      </div>
    </div>
  )
}

export default SourcePanel
