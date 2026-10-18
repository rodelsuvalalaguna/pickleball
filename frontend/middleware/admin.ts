export default defineNuxtRouteMiddleware(() => {
  const auth = useAuth()
  if (!auth.isLoggedIn.value) return navigateTo('/login')
  if (!auth.isAdmin.value) return navigateTo('/')
})
