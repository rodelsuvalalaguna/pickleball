export default defineNuxtRouteMiddleware(() => {
  if (!useAuth().isLoggedIn.value) return navigateTo('/login')
})
