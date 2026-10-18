<script setup lang="ts">
import type { Court } from '~/types'

const { courts } = useReservations()
const filters = reactive({ location: '', surfaceType: '', date: '', hour: '' })
const list = ref<Court[]>([])
const loading = ref(false)

async function search() {
  loading.value = true
  const q: Record<string, any> = {}
  if (filters.location) q.location = filters.location
  if (filters.surfaceType) q.surfaceType = filters.surfaceType
  if (filters.date && filters.hour !== '') { q.date = filters.date; q.hour = filters.hour }
  try { list.value = await courts(q) } finally { loading.value = false }
}
onMounted(search)
</script>

<template>
  <div>
    <h1 class="text-2xl font-bold mb-4">Courts</h1>
    <form class="grid gap-3 sm:grid-cols-5 bg-white border rounded-lg p-4 mb-6" @submit.prevent="search">
      <input v-model="filters.location" placeholder="Location" class="border rounded px-3 py-2">
      <select v-model="filters.surfaceType" class="border rounded px-3 py-2">
        <option value="">Any surface</option>
        <option>Hard</option><option>Acrylic</option><option>Cushioned</option>
      </select>
      <input v-model="filters.date" type="date" :min="todayStr()" class="border rounded px-3 py-2">
      <select v-model="filters.hour" class="border rounded px-3 py-2">
        <option value="">Any time</option>
        <option v-for="h in 15" :key="h" :value="h + 6">{{ h + 6 }}:00</option>
      </select>
      <button class="bg-emerald-600 text-white rounded px-3 py-2">Search</button>
    </form>

    <p v-if="loading">Loading...</p>
    <p v-else-if="!list.length" class="text-slate-500">No courts match your filters.</p>
    <div class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
      <CourtCard v-for="c in list" :key="c.id" :court="c" />
    </div>
  </div>
</template>
