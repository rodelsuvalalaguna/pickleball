<script setup lang="ts">
import type { Reservation } from '~/types'
definePageMeta({ middleware: 'auth' })

const { mine, cancel, reschedule } = useReservations()
const items = ref<Reservation[]>([])
const error = ref('')

// Reschedule form state
const editing = ref<Reservation | null>(null)
const newDate = ref(todayStr())
const newStart = ref<string | null>(null)

const load = async () => { items.value = await mine() }
onMounted(load)

const isFuture = (r: Reservation) => r.status === 'Confirmed' && new Date(r.startTime) > new Date()

async function doCancel(r: Reservation) {
  if (!confirm('Cancel this reservation?')) return
  error.value = ''
  try { await cancel(r.id); await load() } catch (e: any) { error.value = errorMessage(e) }
}

function startEdit(r: Reservation) {
  editing.value = r
  newDate.value = r.startTime.slice(0, 10)
  newStart.value = null
}

async function doReschedule() {
  if (!editing.value || !newStart.value) return
  const hrs = (new Date(editing.value.endTime).getTime() - new Date(editing.value.startTime).getTime()) / 3600000
  error.value = ''
  try {
    await reschedule(editing.value.id, newStart.value, addHours(newStart.value, hrs))
    editing.value = null
    await load()
  } catch (e: any) { error.value = errorMessage(e) }
}
</script>

<template>
  <div>
    <h1 class="text-2xl font-bold mb-4">My bookings</h1>
    <p v-if="error" class="text-red-600 mb-3">{{ error }}</p>
    <p v-if="!items.length" class="text-slate-500">No reservations yet. <NuxtLink to="/courts" class="text-emerald-700 underline">Book a court</NuxtLink></p>

    <ul class="space-y-3">
      <li v-for="r in items" :key="r.id" class="bg-white border rounded-lg p-4">
        <div class="flex flex-wrap justify-between gap-2">
          <div>
            <p class="font-medium">{{ r.courtName }} <span class="text-slate-500 text-sm">({{ r.courtLocation }})</span></p>
            <p class="text-sm">{{ fmtDateTime(r.startTime) }} – {{ fmtDateTime(r.endTime) }} · ${{ r.totalPrice.toFixed(2) }}</p>
            <span class="text-xs rounded px-2 py-0.5" :class="r.status === 'Confirmed' ? 'bg-emerald-100 text-emerald-800' : 'bg-slate-200 text-slate-600'">{{ r.status }}</span>
          </div>
          <div v-if="isFuture(r)" class="flex gap-2 items-start">
            <button class="border rounded px-3 py-1 text-sm" @click="startEdit(r)">Reschedule</button>
            <button class="border border-red-300 text-red-600 rounded px-3 py-1 text-sm" @click="doCancel(r)">Cancel</button>
          </div>
        </div>

        <div v-if="editing?.id === r.id" class="mt-4 border-t pt-4 space-y-3">
          <input v-model="newDate" type="date" :min="todayStr()" class="border rounded px-3 py-2">
          <SlotPicker v-model:selected="newStart" :court-id="r.courtId" :date="newDate" />
          <div class="flex gap-2">
            <button :disabled="!newStart" class="bg-emerald-600 text-white rounded px-4 py-2 disabled:opacity-50" @click="doReschedule">Save new time</button>
            <button class="border rounded px-4 py-2" @click="editing = null">Close</button>
          </div>
        </div>
      </li>
    </ul>
  </div>
</template>
