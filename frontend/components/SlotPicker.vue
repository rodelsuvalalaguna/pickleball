<script setup lang="ts">
import type { Slot } from '~/types'

const props = defineProps<{ courtId: number | string; date: string }>()
const selected = defineModel<string | null>('selected', { default: null })

const { availability } = useReservations()
const slots = ref<Slot[]>([])
const loading = ref(false)

async function load() {
  if (!props.date) return
  try {
    slots.value = await availability(props.courtId, props.date)
  } catch {
    slots.value = []
  }
}

let timer: ReturnType<typeof setInterval>
onMounted(async () => {
  loading.value = true
  await load()
  loading.value = false
  timer = setInterval(load, 15000) // near-real-time refresh
})
onBeforeUnmount(() => clearInterval(timer))
watch(() => props.date, async () => { selected.value = null; await load() })

const hour = (s: string) => new Date(s).toLocaleTimeString(undefined, { hour: 'numeric', minute: '2-digit' })
defineExpose({ reload: load })
</script>

<template>
  <div>
    <p v-if="loading" class="text-sm text-slate-500">Loading availability...</p>
    <div class="grid grid-cols-3 sm:grid-cols-5 gap-2">
      <button
        v-for="s in slots" :key="s.start" type="button" :disabled="!s.available"
        class="rounded border py-2 text-sm"
        :class="selected === s.start ? 'bg-emerald-600 text-white border-emerald-600'
          : s.available ? 'bg-white hover:bg-emerald-50' : 'bg-slate-100 text-slate-400 line-through cursor-not-allowed'"
        @click="selected = s.start"
      >
        {{ hour(s.start) }}
      </button>
    </div>
  </div>
</template>
