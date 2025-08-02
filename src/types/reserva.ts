// Reserva interfaces
export interface Reserva {
  ReservaID: number
  HabitacionID: number
  VisitaID?: number
  fechaInicio: string
  fechaFin?: string
  estado: string
  promocionId?: number
}

// Comprehensive cancel request
export interface ComprehensiveCancelRequest {
  reason: string
}

export interface ReservaExtensionDto {
  additionalHours: number | null
  additionalMinutes: number | null
}

// V1 Reserva DTOs
export interface GuestDto {
  patenteVehiculo: string | null
  numeroTelefono: string | null
  identificador: string | null
}

export interface CreateReservaRequestDto {
  habitacionId: number
  promocionId: number
  fechaInicio: string
  fechaFin: string
  totalHoras: number
  totalMinutos: number
  esReserva: boolean
  guest: GuestDto
}

export interface ReservaResponseDto {
  reservaId: number
  habitacionId: number
  visitaId: number
  fechaInicio: string
  fechaFin: string
  totalHoras: number
  totalMinutos: number
  promocionId: number
  promocionNombre: string
  promocionTarifa: number
  pausaHoras: number
  pausaMinutos: number
  esReserva: boolean
  activo: boolean
  createdAt: string
  updatedAt: string
}
