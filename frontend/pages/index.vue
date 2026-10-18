<script setup lang="ts">
const { courts } = useReservations()
const { data } = await useAsyncData('home-courts', () => courts())
</script>

<template>
  <div>
    <section class="rounded-xl bg-emerald-600 text-white p-8 sm:p-12 mb-8">
      <h1 class="text-3xl sm:text-4xl font-bold">Book your pickleball court online</h1>
      <p class="mt-2 text-emerald-50 max-w-xl">Pick a court, choose an open time and play. Live availability, instant confirmation.</p>
      <NuxtLink to="/courts" class="inline-block mt-5 bg-white text-emerald-700 font-medium rounded px-5 py-2">Find a court</NuxtLink>
    </section>

    <h2 class="text-xl font-semibold mb-3">Our courts</h2>
    <p v-if="!data?.length" class="text-slate-500">No courts available right now.</p>
    <div class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
      <CourtCard v-for="c in data" :key="c.id" :court="c" />
    </div>
  </div>
</template>
