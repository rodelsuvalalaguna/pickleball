// $fetch wrapper: adds the JWT, retries once after a refresh on 401.
export const useApi = () => {
  const baseURL = useRuntimeConfig().public.apiBase
  const auth = useAuth()

  return async function api<T>(path: string, opts: Record<string, any> = {}): Promise<T> {
    const call = () =>
      $fetch<T>(path, {
        baseURL,
        ...opts,
        headers: { ...(opts.headers || {}), ...(auth.token.value ? { Authorization: `Bearer ${auth.token.value}` } : {}) }
      } as any)
    try {
      return await call()
    } catch (e: any) {
      if (e?.statusCode === 401 && auth.token.value && (await auth.refresh())) return await call()
      throw e
    }
  }
}

export const errorMessage = (e: any): string =>
  e?.data?.detail || e?.data?.errors && Object.values(e.data.errors).flat().join(' ') || e?.statusMessage || 'Something went wrong.'
