import type { Block, Court, Reservation, Slot } from '~/types'

export const useReservations = () => {
  const api = useApi()
  return {
    courts: (q: Record<string, any> = {}) => api<Court[]>('/courts', { query: q }),
    court: (id: number | string) => api<Court>(`/courts/${id}`),
    availability: (id: number | string, date: string) => api<Slot[]>(`/courts/${id}/availability`, { query: { date } }),
    mine: () => api<Reservation[]>('/reservations/mine'),
    create: (courtId: number, startTime: string, endTime: string) =>
      api<Reservation>('/reservations', { method: 'POST', body: { courtId, startTime, endTime } }),
    reschedule: (id: number, startTime: string, endTime: string) =>
      api<Reservation>(`/reservations/${id}/reschedule`, { method: 'PUT', body: { startTime, endTime } }),
    cancel: (id: number) => api<void>(`/reservations/${id}/cancel`, { method: 'POST' }),

    // admin
    adminCourts: () => api<Court[]>('/admin/courts'),
    saveCourt: (c: Partial<Court>) =>
      c.id ? api<Court>(`/admin/courts/${c.id}`, { method: 'PUT', body: c }) : api<Court>('/admin/courts', { method: 'POST', body: c }),
    deactivateCourt: (id: number) => api<void>(`/admin/courts/${id}`, { method: 'DELETE' }),
    adminReservations: () => api<Reservation[]>('/admin/reservations'),
    blocks: () => api<Block[]>('/admin/blocks'),
    addBlock: (b: { courtId: number; startTime: string; endTime: string; reason: string }) =>
      api<Block>('/admin/blocks', { method: 'POST', body: b }),
    deleteBlock: (id: number) => api<void>(`/admin/blocks/${id}`, { method: 'DELETE' }),
    users: () => api<any[]>('/admin/users'),
    setRole: (id: number, role: string) => api<void>(`/admin/users/${id}/role`, { method: 'PUT', body: { role } })
  }
}

export const fmtDateTime = (s: string) =>
  new Date(s).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })
const pad = (n: number) => String(n).padStart(2, '0')
// Court-local wall-clock strings (no timezone), matching what the API stores.
export const addHours = (s: string, h: number) => {
  const d = new Date(s)
  d.setHours(d.getHours() + h)
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:00:00`
}
export const todayStr = () => {
  const d = new Date()
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}
