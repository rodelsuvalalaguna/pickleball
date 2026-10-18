<script setup lang="ts">
import type { Block, Court, Reservation } from '~/types'
definePageMeta({ middleware: 'admin' })

const r = useReservations()
const tab = ref<'courts' | 'bookings' | 'blocks' | 'users'>('courts')
const error = ref('')

const courts = ref<Court[]>([])
const bookings = ref<Reservation[]>([])
const blocks = ref<Block[]>([])
const users = ref<any[]>([])

const emptyCourt = (): Partial<Court> => ({ name: '', location: '', surfaceType: 'Hard', description: '', hourlyRate: 20, isActive: true })
const court = ref<Partial<Court>>(emptyCourt())
const block = reactive({ courtId: 0, startTime: '', endTime: '', reason: '' })

async function run(fn: () => Promise<unknown>) {
  error.value = ''
  try { await fn() } catch (e: any) { error.value = errorMessage(e) }
}

const loadCourts = async () => { courts.value = await r.adminCourts() }
const loadAll = () => run(async () => {
  await loadCourts()
  ;[bookings.value, blocks.value, users.value] = await Promise.all([r.adminReservations(), r.blocks(), r.users()])
})
onMounted(loadAll)

const saveCourt = () => run(async () => { await r.saveCourt(court.value); court.value = emptyCourt(); await loadCourts() })
const deactivate = (id: number) => run(async () => { await r.deactivateCourt(id); await loadCourts() })
const cancelBooking = (id: number) => run(async () => { await r.cancel(id); bookings.value = await r.adminReservations() })
const addBlock = () => run(async () => {
  await r.addBlock({ ...block, startTime: block.startTime + ':00', endTime: block.endTime + ':00' })
  blocks.value = await r.blocks()
})
const delBlock = (id: number) => run(async () => { await r.deleteBlock(id); blocks.value = await r.blocks() })
const setRole = (id: number, role: string) => run(async () => { await r.setRole(id, role); users.value = await r.users() })
</script>

<template>
  <div>
    <h1 class="text-2xl font-bold mb-4">Admin</h1>
    <div class="flex gap-2 mb-4 overflow-x-auto">
      <button v-for="t in ['courts', 'bookings', 'blocks', 'users']" :key="t" class="px-4 py-2 rounded capitalize"
        :class="tab === t ? 'bg-emerald-600 text-white' : 'bg-white border'" @click="tab = t as any">{{ t }}</button>
    </div>
    <p v-if="error" class="text-red-600 mb-3">{{ error }}</p>

    <!-- Courts -->
    <section v-if="tab === 'courts'" class="space-y-4">
      <form class="bg-white border rounded-lg p-4 grid gap-3 sm:grid-cols-3" @submit.prevent="saveCourt">
        <input v-model="court.name" required placeholder="Name" class="border rounded px-3 py-2">
        <input v-model="court.location" required placeholder="Location" class="border rounded px-3 py-2">
        <input v-model="court.surfaceType" required placeholder="Surface" class="border rounded px-3 py-2">
        <input v-model.number="court.hourlyRate" type="number" step="0.01" min="0.01" required placeholder="Hourly rate" class="border rounded px-3 py-2">
        <input v-model="court.description" placeholder="Description" class="border rounded px-3 py-2">
        <label class="flex items-center gap-2"><input v-model="court.isActive" type="checkbox"> Active</label>
        <button class="bg-emerald-600 text-white rounded px-4 py-2 sm:col-span-3">{{ court.id ? 'Update court' : 'Add court' }}</button>
      </form>
      <div class="overflow-x-auto bg-white border rounded-lg">
        <table class="w-full text-sm">
          <thead class="text-left bg-slate-100"><tr><th class="p-2">Name</th><th>Location</th><th>Surface</th><th>Rate</th><th>Status</th><th /></tr></thead>
          <tbody>
            <tr v-for="c in courts" :key="c.id" class="border-t">
              <td class="p-2">{{ c.name }}</td><td>{{ c.location }}</td><td>{{ c.surfaceType }}</td>
              <td>${{ c.hourlyRate.toFixed(2) }}</td><td>{{ c.isActive ? 'Active' : 'Inactive' }}</td>
              <td class="space-x-2 p-2">
                <button class="text-emerald-700" @click="court = { ...c }">Edit</button>
                <button v-if="c.isActive" class="text-red-600" @click="deactivate(c.id)">Deactivate</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <!-- Bookings -->
    <section v-if="tab === 'bookings'" class="overflow-x-auto bg-white border rounded-lg">
      <table class="w-full text-sm">
        <thead class="text-left bg-slate-100"><tr><th class="p-2">Court</th><th>User</th><th>Start</th><th>End</th><th>Status</th><th /></tr></thead>
        <tbody>
          <tr v-for="b in bookings" :key="b.id" class="border-t">
            <td class="p-2">{{ b.courtName }}</td><td>{{ b.userName }}</td>
            <td>{{ fmtDateTime(b.startTime) }}</td><td>{{ fmtDateTime(b.endTime) }}</td><td>{{ b.status }}</td>
            <td class="p-2"><button v-if="b.status === 'Confirmed'" class="text-red-600" @click="cancelBooking(b.id)">Cancel</button></td>
          </tr>
        </tbody>
      </table>
    </section>

    <!-- Maintenance blocks -->
    <section v-if="tab === 'blocks'" class="space-y-4">
      <form class="bg-white border rounded-lg p-4 grid gap-3 sm:grid-cols-5" @submit.prevent="addBlock">
        <select v-model.number="block.courtId" required class="border rounded px-3 py-2">
          <option :value="0" disabled>Court</option>
          <option v-for="c in courts" :key="c.id" :value="c.id">{{ c.name }}</option>
        </select>
        <input v-model="block.startTime" type="datetime-local" required class="border rounded px-3 py-2">
        <input v-model="block.endTime" type="datetime-local" required class="border rounded px-3 py-2">
        <input v-model="block.reason" required placeholder="Reason" class="border rounded px-3 py-2">
        <button class="bg-emerald-600 text-white rounded px-4 py-2">Block time</button>
      </form>
      <ul class="space-y-2">
        <li v-for="b in blocks" :key="b.id" class="bg-white border rounded p-3 flex justify-between gap-2">
          <span>{{ b.courtName }} · {{ fmtDateTime(b.startTime) }} – {{ fmtDateTime(b.endTime) }} · {{ b.reason }}</span>
          <button class="text-red-600" @click="delBlock(b.id)">Remove</button>
        </li>
      </ul>
    </section>

    <!-- Users -->
    <section v-if="tab === 'users'" class="overflow-x-auto bg-white border rounded-lg">
      <table class="w-full text-sm">
        <thead class="text-left bg-slate-100"><tr><th class="p-2">Name</th><th>Email</th><th>Role</th><th /></tr></thead>
        <tbody>
          <tr v-for="u in users" :key="u.id" class="border-t">
            <td class="p-2">{{ u.name }}</td><td>{{ u.email }}</td><td>{{ u.role }}</td>
            <td class="p-2">
              <button class="text-emerald-700" @click="setRole(u.id, u.role === 'Admin' ? 'User' : 'Admin')">
                Make {{ u.role === 'Admin' ? 'User' : 'Admin' }}
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </section>
  </div>
</template>
