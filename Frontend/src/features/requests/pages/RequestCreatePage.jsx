import { useEffect, useState } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { Link, useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { RequestForm } from '../components/RequestModal.jsx'
import { fetchActiveCategoriesApi } from '../requestApi.js'
import {
  createRequest,
  selectRequestMutationLoading,
} from '../requestSlice.js'

export function RequestCreatePage() {
  const dispatch = useDispatch()
  const navigate = useNavigate()
  const mutating = useSelector(selectRequestMutationLoading)

  const [categories, setCategories] = useState([])
  const [serverError, setServerError] = useState(null)

  useEffect(() => {
    let cancelled = false
    fetchActiveCategoriesApi()
      .then((list) => {
        if (!cancelled) setCategories(list)
      })
      .catch(() => {
        if (!cancelled) setCategories([])
      })
    return () => {
      cancelled = true
    }
  }, [])

  async function handleSubmit(values) {
    setServerError(null)
    try {
      const created = await dispatch(createRequest(values)).unwrap()
      toast.success('Request created')
      const id = created?.requestId
      navigate(id ? `/requests/${id}` : '/requests')
    } catch (e) {
      setServerError(String(e))
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-4">
        <Link
          to="/requests"
          className="inline-flex min-h-[44px] items-center justify-center rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm font-semibold text-gray-800 shadow-sm hover:bg-gray-50"
        >
          Back to requests
        </Link>
      </div>

      <div className="mx-auto max-w-xl rounded-xl border border-gray-200 bg-white p-4 shadow-sm">
        <h2 className="text-lg font-semibold text-gray-900">Create request</h2>
        <div className="mt-4">
          <RequestForm
            categories={categories}
            saving={mutating}
            serverError={serverError}
            onSubmit={handleSubmit}
            submitLabel="Create request"
          />
        </div>
      </div>
    </div>
  )
}
