'use client'

import { useEffect, useState, useRef } from 'react'
import { Skeleton } from '@/components/ui/Skeleton'

interface AnswerCardProps {
  query: string
  initialAnswer?: string
  streaming?: boolean
}

export default function AnswerCard({ query, initialAnswer, streaming }: AnswerCardProps) {
  const [answer, setAnswer] = useState(initialAnswer ?? '')
  const [isStreaming, setIsStreaming] = useState(false)
  const abortRef = useRef<AbortController | null>(null)

  useEffect(() => {
    if (!streaming || !query) return

    // Stream answer tokens from SSE endpoint
    const controller = new AbortController()
    abortRef.current = controller
    setIsStreaming(true)
    setAnswer('')

    fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/search/stream`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ query }),
      signal: controller.signal
    }).then(async (res) => {
      const reader = res.body!.getReader()
      const decoder = new TextDecoder()

      while (true) {
        const { done, value } = await reader.read()
        if (done) break

        const chunk = decoder.decode(value)
        const lines = chunk.split('\n').filter(l => l.startsWith('data: '))

        for (const line of lines) {
          const token = line.replace('data: ', '').trim()
          if (token === '[DONE]') { setIsStreaming(false); return }
          setAnswer(prev => prev + token)
        }
      }
    }).catch(() => setIsStreaming(false))

    return () => controller.abort()
  }, [query, streaming])

  return (
    <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6">
      <div className="flex items-center gap-2 mb-4">
        <span className="text-xs font-semibold uppercase tracking-wide text-blue-600 bg-blue-50 px-2 py-1 rounded-full">
          AI Answer
        </span>
        {isStreaming && (
          <span className="w-2 h-2 rounded-full bg-blue-400 animate-pulse" />
        )}
      </div>

      <div className="prose prose-sm max-w-none text-gray-800 leading-relaxed">
        {answer || <Skeleton lines={4} />}
        {isStreaming && <span className="inline-block w-0.5 h-4 bg-gray-400 animate-blink ml-0.5" />}
      </div>
    </div>
  )
}

AnswerCard.Skeleton = function AnswerCardSkeleton() {
  return (
    <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6">
      <Skeleton lines={5} />
    </div>
  )
}
