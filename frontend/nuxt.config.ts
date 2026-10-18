export default defineNuxtConfig({
  compatibilityDate: '2024-10-01',
  modules: ['@nuxtjs/tailwindcss'],
  typescript: { strict: true },
  runtimeConfig: {
    public: {
      // Override with NUXT_PUBLIC_API_BASE
      apiBase: 'https://localhost:61551/api'
    }
  },
  app: {
    head: {
      title: 'Pickleball Court Booking',
      meta: [{ name: 'viewport', content: 'width=device-width, initial-scale=1' }]
    }
  }
})
