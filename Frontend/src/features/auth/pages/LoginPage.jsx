import { useEffect, useState } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { FlowDeskLogo } from '../../../components/brand/FlowDeskLogo.jsx'
import { Button } from '../../../components/ui/Button.jsx'
import { Input } from '../../../components/ui/Input.jsx'
import { clearError, login, selectAuthStatus } from '../authSlice.js'

export function LoginPage() {
  const dispatch = useDispatch()
  const navigate = useNavigate()
  const location = useLocation()
  const status = useSelector(selectAuthStatus)

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [fieldErrors, setFieldErrors] = useState({})

  useEffect(() => {
    return () => {
      dispatch(clearError())
    }
  }, [dispatch])

  const from =
    (location.state && location.state.from) === '/login'
      ? '/'
      : location.state?.from || '/'

  async function handleSubmit(e) {
    e.preventDefault()
    const next = {}
    if (!email.trim()) next.email = 'Email is required'
    if (!password) next.password = 'Password is required'
    setFieldErrors(next)
    if (Object.keys(next).length) return

    const action = await dispatch(login({ email: email.trim(), password }))
    if (login.fulfilled.match(action)) {
      toast.success('Signed in successfully')
      navigate(from, { replace: true })
    } else if (login.rejected.match(action)) {
      toast.error(action.payload ?? 'Sign in failed')
    }
  }

  const loading = status === 'loading'

  return (
    <div className="flex min-h-screen flex-col bg-gray-50 font-sans">
      <header className="sticky top-0 z-50 border-b border-gray-200 bg-white/95 shadow-sm backdrop-blur-md transition-shadow duration-200">
        <div className="mx-auto flex max-w-7xl items-center justify-between gap-4 p-4 md:p-6">
          <Link
            to="/"
            className="inline-flex shrink-0 rounded-md outline-offset-2 transition-opacity duration-200 hover:opacity-90 focus-visible:outline focus-visible:outline-2 focus-visible:outline-emerald-600"
          >
            <FlowDeskLogo />
          </Link>
          <span className="text-sm text-gray-600">Secure access</span>
        </div>
      </header>

      <div className="relative flex flex-1 flex-col md:grid md:min-h-0 md:grid-cols-2 md:flex-none">
        <aside className="relative hidden overflow-hidden bg-gradient-to-br from-zinc-900 via-neutral-800 to-stone-950 md:flex md:flex-col md:justify-between">
          <div
            className="pointer-events-none absolute -right-24 -top-24 h-72 w-72 rounded-full bg-white/10 blur-3xl transition-opacity duration-500"
            aria-hidden
          />
          <div
            className="pointer-events-none absolute -bottom-20 -left-16 h-64 w-64 rounded-full bg-emerald-500/15 blur-3xl"
            aria-hidden
          />

          <div className="relative z-10 flex flex-1 flex-col justify-center p-10 lg:p-14">
            <p className="text-xs font-semibold uppercase tracking-widest text-zinc-400">
              Operations workspace
            </p>
            <h2 className="mt-3 max-w-md text-3xl font-semibold tracking-tight text-white">
              Calm, fast access to the tools your team relies on.
            </h2>
            <p className="mt-4 max-w-sm text-sm leading-relaxed text-zinc-300">
              Sign in once, stay in flow. Role-based permissions keep every view
              intentional and auditable.
            </p>
          </div>

          <div className="relative z-10 border-t border-white/10 p-10 text-xs text-zinc-400 lg:p-14">
            Encrypted session · Organization policies apply
          </div>
        </aside>

        <main className="relative flex flex-1 items-center justify-center px-4 py-10 sm:px-6 md:py-12 md:pl-8 md:pr-10 lg:pl-12 lg:pr-16">
          <div className="absolute inset-0 -z-10 bg-gradient-to-b from-emerald-50/80 via-gray-50 to-gray-50 md:hidden" />

          <div className="w-full max-w-md space-y-8">
            <div className="text-center md:text-left">
              <h1 className="text-3xl font-semibold tracking-tight text-gray-900">
                Welcome back
              </h1>
              <p className="mt-2 text-sm text-gray-600">
                Sign in with your work email to continue
              </p>
            </div>

            <div className="rounded-xl border border-gray-200 bg-white p-6 shadow-sm transition-all duration-200 hover:shadow-md sm:p-8">
              <form
                className="flex flex-col gap-6"
                onSubmit={handleSubmit}
                noValidate
              >
                <Input
                  id="email"
                  name="email"
                  type="email"
                  autoComplete="email"
                  label="Email"
                  placeholder="you@company.com"
                  value={email}
                  onChange={(ev) => setEmail(ev.target.value)}
                  error={fieldErrors.email}
                  disabled={loading}
                />
                <Input
                  id="password"
                  name="password"
                  type="password"
                  autoComplete="current-password"
                  label="Password"
                  placeholder="••••••••"
                  value={password}
                  onChange={(ev) => setPassword(ev.target.value)}
                  error={fieldErrors.password}
                  disabled={loading}
                />

                <div className="flex flex-col gap-3 pt-1">
                  <Button
                    type="submit"
                    className="w-full"
                    loading={loading}
                    disabled={loading}
                  >
                    {loading ? 'Signing in' : 'Sign in'}
                  </Button>
                  <p className="text-center text-xs leading-relaxed text-gray-500 md:text-left">
                    By continuing you agree to your organization&apos;s access
                    policies.
                  </p>
                </div>
              </form>
            </div>

            <p className="text-center text-sm text-gray-600 md:text-left">
              Need an account?{' '}
              <Link
                to="/"
                className="font-medium text-emerald-700 underline-offset-4 transition-colors duration-200 hover:text-emerald-800 hover:underline"
              >
                Contact your administrator
              </Link>
            </p>
          </div>
        </main>
      </div>
    </div>
  )
}
