/**
 * Cierre Models - V1 API DTOs
 * TypeScript interfaces matching backend C# models
 */

/**
 * DTO for payment information
 */
export interface PagoDto {
  pagoId: number
  habitacionId?: number
  fecha?: Date | string
  montoEfectivo?: number
  montoTarjeta?: number
  montoBillVirt?: number
  montoDescuento?: number
  // Add other properties as needed
}

/**
 * DTO for basic closure information in the list
 */
export interface CierreBasicoDto {
  cierreId: number
  fechaHoraCierre?: Date | string
  estadoCierre: boolean
  totalIngresosEfectivo?: number
  totalIngresosBillVirt?: number
  totalIngresosTarjeta?: number
  montoInicialCaja?: number
  observaciones?: string
  institucionID: number
  pagos: PagoDto[]
}

/**
 * DTO for pending transactions (payments, cancellations)
 */
export interface TransaccionPendienteDto {
  pagoId: number
  habitacionId?: number
  tarjetaNombre?: string
  periodo: number
  categoriaNombre?: string
  fecha?: Date | string
  horaIngreso?: Date | string
  horaSalida?: Date | string
  montoAdicional: number
  totalConsumo: number
  montoEfectivo?: number
  montoTarjeta?: number
  montoBillVirt?: number
  montoDescuento?: number
  observacion?: string
  tipoHabitacion?: string
  tipoTransaccion?: string // "Habitación", "Empeño", "Anulación"
}

/**
 * DTO for expense details
 */
export interface EgresoDetalleDto {
  egresoId: number
  fecha?: Date | string
  montoEfectivo: number
  observacion?: string
  tipoEgresoNombre?: string
  cantidad: number
  precio: number
}

/**
 * Main DTO for closures and current transactions
 */
export interface CierresyActualDto {
  cierres: CierreBasicoDto[]
  transaccionesPendientes: TransaccionPendienteDto[]
  egresosPendientes: EgresoDetalleDto[]
}

/**
 * DTO for complete closure details
 */
export interface CierreDetalleCompletoDto {
  cierre: CierreBasicoDto
  pagos: TransaccionPendienteDto[]
  anulaciones: TransaccionPendienteDto[]
  egresos: EgresoDetalleDto[]
}

/**
 * DTO for detailed closure information
 */
export interface CajaDetalladaDto {
  cierreId: number
  fechaHoraCierre?: Date | string
  montoInicial: number
  montoFinal: number
  totalEfectivo: number
  totalTarjeta: number
  totalDescuentos: number
  observacion?: string
  usuarioNombre: string
  institucionNombre?: string
}

export interface Paginated {
  pageNumber: number
  pageSize: number
  totalRecords: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

/**
 * DTO for paginated closure response
 */
export interface CierresPaginatedDto {
  data: {
    cierres: CierreBasicoDto[]
    pagosSinCierre: TransaccionPendienteDto[]
  }
  pagination: Paginated
}

/**
 * Pagination parameters
 */
export interface PaginationParams {
  page: number
  pageSize: number
  startDate?: string
  endDate?: string
}

