<script setup lang="ts">
import type { Court } from '~/types'
defineProps<{ court: Court }>()
</script>

<template>
  <div class="bg-white rounded-lg shadow-sm border p-4 flex flex-col gap-2">
    <div class="flex justify-between items-start">
      <h3 class="font-semibold text-lg">{{ court.name }}</h3>
      <span class="text-emerald-700 font-medium">${{ court.hourlyRate.toFixed(2) }}/hr</span>
    </div>
    <p class="text-sm text-slate-600">{{ court.location }}</p>
    <p class="text-xs"><span class="bg-emerald-100 text-emerald-800 rounded px-2 py-0.5">{{ court.surfaceType }}</span></p>
    <p v-if="court.description" class="text-sm text-slate-500">{{ court.description }}</p>
    <NuxtLink :to="`/courts/${court.id}`" class="mt-auto bg-emerald-600 hover:bg-emerald-700 text-white text-center rounded py-2">
      View &amp; book
    </NuxtLink>
  </div>
</template>
