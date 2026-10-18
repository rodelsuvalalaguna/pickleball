<script setup lang="ts">
const auth = useAuth()
const email = ref('')
const password = ref('')
const error = ref('')
const busy = ref(false)

async function submit() {
  error.value = ''
  busy.value = true
  try {
    await auth.login(email.value, password.value)
    await navigateTo(auth.isAdmin.value ? '/admin' : '/dashboard')
  } catch (e: any) {
    error.value = e?.statusCode === 429 ? 'Too many attempts. Try again in a minute.' : errorMessage(e)
  } finally {
    busy.value = false
  }
}
</script>

<template>
  <form class="max-w-sm mx-auto bg-white border rounded-lg p-6 space-y-3" @submit.prevent="submit">
    <h1 class="text-xl font-bold">Log in</h1>
    <input v-model="email" type="email" required placeholder="Email" autocomplete="email" class="w-full border rounded px-3 py-2">
    <input v-model="password" type="password" required placeholder="Password" autocomplete="current-password" class="w-full border rounded px-3 py-2">
    <p v-if="error" class="text-red-600 text-sm">{{ error }}</p>
    <button :disabled="busy" class="w-full bg-emerald-600 text-white rounded py-2 disabled:opacity-50">Log in</button>
    <p class="text-sm text-center">No account? <NuxtLink to="/register" class="text-emerald-700 underline">Sign up</NuxtLink></p>
  </form>
</template>
