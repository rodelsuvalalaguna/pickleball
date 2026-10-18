<script setup lang="ts">
import type { Reservation } from '~/types'

const route = useRoute()
const auth = useAuth()
const { court: getCourt, create } = useReservations()
const { data: court } = await useAsyncData(`court-${route.params.id}`, () => getCourt(route.params.id as string))

const date = ref(todayStr())
const start = ref<string | null>(null)
const hours = ref(1)
const picker = ref<{ reload: () => Promise<void> } | null>(null)
const error = ref('')
const busy = ref(false)
const confirmation = ref<Reservation | null>(null)

const total = computed(() => (court.value ? court.value.hourlyRate * hours.value : 0))

async function book() {
  if (!auth.isLoggedIn.value) return navigateTo('/login')
  if (!start.value || !court.value) return
  error.value = ''
  busy.value = true
  try {
    confirmation.value = await create(court.value.id, start.value, addHours(start.value, hours.value))
  } catch (e: any) {
    error.value = errorMessage(e)
    start.value = null
    await picker.value?.reload()
  } finally {
    busy.value = false
  }
}
</script>

<template>
  <div v-if="court" class="max-w-2xl">
    <NuxtLink to="/courts" class="text-sm text-emerald-700">&larr; All courts</NuxtLink>
    <h1 class="text-2xl font-bold mt-2">{{ court.name }}</h1>
    <p class="text-slate-600">{{ court.location }} · {{ court.surfaceType }} · ${{ court.hourlyRate.toFixed(2) }}/hr</p>
    <p v-if="court.description" class="mt-1 text-slate-500">{{ court.description }}</p>

    <div v-if="confirmation" class="mt-6 bg-emerald-50 border border-emerald-300 rounded-lg p-5">
      <h2 class="font-semibold text-emerald-800 text-lg">Booking confirmed</h2>
      <p>{{ confirmation.courtName }} — {{ fmtDateTime(confirmation.startTime) }} to {{ fmtDateTime(confirmation.endTime) }}</p>
      <p>Total: ${{ confirmation.totalPrice.toFixed(2) }}</p>
      <NuxtLink to="/dashboard" class="inline-block mt-3 text-emerald-700 underline">View my bookings</NuxtLink>
    </div>

    <div v-else class="mt-6 bg-white border rounded-lg p-4 space-y-4">
      <div class="flex flex-wrap gap-3 items-end">
        <label class="text-sm">Date<br><input v-model="date" type="date" :min="todayStr()" class="border rounded px-3 py-2"></label>
        <label class="text-sm">Duration<br>
          <select v-model.number="hours" class="border rounded px-3 py-2">
            <option :value="1">1 hour</option><option :value="2">2 hours</option><option :value="3">3 hours</option>
          </select>
        </label>
      </div>

      <SlotPicker ref="picker" v-model:selected="start" :court-id="court.id" :date="date" />

      <p v-if="error" class="text-red-600 text-sm">{{ error }}</p>
      <div v-if="start" class="flex items-center justify-between border-t pt-3">
        <span>{{ fmtDateTime(start) }} · {{ hours }}h · <strong>${{ total.toFixed(2) }}</strong></span>
        <button :disabled="busy" class="bg-emerald-600 text-white rounded px-4 py-2 disabled:opacity-50" @click="book">
          {{ auth.isLoggedIn.value ? 'Confirm booking' : 'Log in to book' }}
        </button>
      </div>
    </div>
  </div>
</template>
