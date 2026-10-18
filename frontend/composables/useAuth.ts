import type { AuthResponse, User } from '~/types'

function decode(token: string | null | undefined): User | null {
  if (!token) return null
  try {
    const b64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')
    const p = JSON.parse(decodeURIComponent(escape(atob(b64))))
    return { id: Number(p.sub), name: p.name, email: p.email, role: p.role, createdAt: '' }
  } catch {
    return null
  }
}

// Tokens live in cookies so SSR and client agree. They are JS-readable (not httpOnly);
// keep the access token short-lived and avoid rendering untrusted HTML.
export const useAuth = () => {
  const opts = { sameSite: 'strict' as const, secure: !import.meta.dev, maxAge: 60 * 60 * 24 * 7 }
  const token = useCookie<string | null>('access_token', opts)
  const refreshToken = useCookie<string | null>('refresh_token', opts)

  const user = computed(() => decode(token.value))
  const isLoggedIn = computed(() => !!user.value)
  const isAdmin = computed(() => user.value?.role === 'Admin')

  const base = () => useRuntimeConfig().public.apiBase

  function store(r: AuthResponse) {
    token.value = r.accessToken
    refreshToken.value = r.refreshToken
  }

  async function login(email: string, password: string) {
    store(await $fetch<AuthResponse>('/auth/login', { baseURL: base(), method: 'POST', body: { email, password } }))
  }

  async function register(name: string, email: string, password: string) {
    store(await $fetch<AuthResponse>('/auth/register', { baseURL: base(), method: 'POST', body: { name, email, password } }))
  }

  async function refresh(): Promise<boolean> {
    if (!refreshToken.value) return false
    try {
      store(await $fetch<AuthResponse>('/auth/refresh', { baseURL: base(), method: 'POST', body: { refreshToken: refreshToken.value } }))
      return true
    } catch {
      token.value = null
      refreshToken.value = null
      return false
    }
  }

  async function logout() {
    const rt = refreshToken.value
    token.value = null
    refreshToken.value = null
    if (rt) await $fetch('/auth/logout', { baseURL: base(), method: 'POST', body: { refreshToken: rt } }).catch(() => {})
    await navigateTo('/')
  }

  return { token, user, isLoggedIn, isAdmin, login, register, refresh, logout }
}
