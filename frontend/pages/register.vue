<script setup lang="ts">
const auth = useAuth()
const name = ref('')
const email = ref('')
const password = ref('')
const error = ref('')
const busy = ref(false)

async function submit() {
  error.value = ''
  busy.value = true
  try {
    await auth.register(name.value, email.value, password.value)
    await navigateTo('/dashboard')
  } catch (e: any) {
    error.value = e?.statusCode === 429 ? 'Too many attempts. Try again in a minute.' : errorMessage(e)
  } finally {
    busy.value = false
  }
}
</script>

<template>
  <form class="max-w-sm mx-auto bg-white border rounded-lg p-6 space-y-3" @submit.prevent="submit">
    <h1 class="text-xl font-bold">Create account</h1>
    <input v-model="name" required minlength="2" placeholder="Name" autocomplete="name" class="w-full border rounded px-3 py-2">
    <input v-model="email" type="email" required placeholder="Email" autocomplete="email" class="w-full border rounded px-3 py-2">
    <input v-model="password" type="password" required minlength="10" placeholder="Password (min 10 characters)" autocomplete="new-password" class="w-full border rounded px-3 py-2">
    <p v-if="error" class="text-red-600 text-sm">{{ error }}</p>
    <button :disabled="busy" class="w-full bg-emerald-600 text-white rounded py-2 disabled:opacity-50">Sign up</button>
  </form>
</template>
