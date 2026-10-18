export interface User { id: number; name: string; email: string; role: string; createdAt: string }
export interface AuthResponse { accessToken: string; refreshToken: string; expiresAt: string; user: User }
export interface Court {
  id: number; name: string; location: string; surfaceType: string
  description?: string | null; hourlyRate: number; isActive: boolean
}
export interface Slot { start: string; end: string; available: boolean }
export interface Reservation {
  id: number; courtId: number; courtName: string; courtLocation: string
  userId: number; userName: string; startTime: string; endTime: string
  totalPrice: number; status: 'Confirmed' | 'Cancelled'; createdAt: string
}
export interface Block { id: number; courtId: number; courtName: string; startTime: string; endTime: string; reason: string }
