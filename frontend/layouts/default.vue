<script setup lang="ts">
const auth = useAuth()
const open = ref(false)
</script>

<template>
  <div class="min-h-screen flex flex-col bg-slate-50 text-slate-800">
    <header class="bg-emerald-700 text-white">
      <nav class="max-w-6xl mx-auto px-4 py-3 flex items-center justify-between">
        <NuxtLink to="/" class="font-bold text-lg">Pickleball Courts</NuxtLink>
        <button class="sm:hidden" aria-label="Menu" @click="open = !open">Menu</button>
        <div :class="[open ? 'flex' : 'hidden', 'sm:flex']" class="absolute sm:static top-12 left-0 right-0 bg-emerald-700 flex-col sm:flex-row gap-3 sm:gap-5 p-4 sm:p-0 items-start sm:items-center z-10" @click="open = false">
          <NuxtLink to="/courts">Courts</NuxtLink>
          <template v-if="auth.isLoggedIn.value">
            <NuxtLink to="/dashboard">My bookings</NuxtLink>
            <NuxtLink v-if="auth.isAdmin.value" to="/admin">Admin</NuxtLink>
            <button @click="auth.logout()">Log out ({{ auth.user.value?.name }})</button>
          </template>
          <template v-else>
            <NuxtLink to="/login">Log in</NuxtLink>
            <NuxtLink to="/register" class="bg-white text-emerald-700 rounded px-3 py-1 font-medium">Sign up</NuxtLink>
          </template>
        </div>
      </nav>
    </header>
    <main class="flex-1 max-w-6xl w-full mx-auto px-4 py-6">
      <slot />
    </main>
    <footer class="text-center text-sm text-slate-500 py-4">Pickleball Court Booking</footer>
  </div>
</template>
