import { Toaster } from 'sonner'
import { AppRouter } from './app/router.jsx'

function App() {
  return (
    <>
      <AppRouter />
      <Toaster richColors closeButton position="top-right" />
    </>
  )
}

export default App
