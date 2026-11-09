import '@tanstack/react-query'
import type { ErrorApi } from './problema'

declare module '@tanstack/react-query' {
  interface Register { defaultError: ErrorApi }
}
