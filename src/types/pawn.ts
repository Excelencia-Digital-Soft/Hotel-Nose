/**
 * Pawn Management TypeScript Interfaces
 * Defines types for pawn operations, payments, and statistics
 */

import type { ApiResponse } from './common'

// Core Pawn Interfaces
export interface PawnDto {
  empeñoId: number
  empenoId: number
  detalle: string
  monto: number
  fechaRegistro: string
  visitaId: number
  institucionId: number
  observaciones?: string
  estado: 'activo' | 'pagado' | 'vencido'
  estadoPago: 'Pendiente' | 'Pagado'
  fechaPago?: string
  montoPagado?: number
  pagoId?: number
}

export interface PaymentCardDto {
  tarjetaId: number
  nombre: string
  montoPorcentual: number
  institucionId: number
  activa: boolean
}

export interface PawnStatisticsDto {
  totalPawns: number
  totalAmount: number
  averageAmount: number
  overduePawns: number
  activePawns: number
  paidPawns: number
  overdueAmount: number
  overduePercentage: number
}

// Request/Response Interfaces for V1 API
export interface CreatePawnRequest {
  detalle: string
  monto: number
  visitaId: number
  usuarioId: number
  observaciones?: string
}

export interface UpdatePawnAmountRequest {
  nuevoMonto: number
}

export interface PayPawnRequest {
  pawnId: number
  montoEfectivo: number
  montoTarjeta: number
  observacion?: string
  tarjetaId?: number
}

export interface PawnPaymentDto {
  pawnId: number
  montoEfectivo: number
  montoTarjeta: number
  montoTotal: number
  cardSurcharge: number
  totalWithSurcharge: number
  observacion?: string
  tarjetaId?: number
  fechaPago: string
  usuarioID: number
}

// Display and UI Interfaces
export interface FormattedPawnDto extends PawnDto {
  formattedAmount: string
  formattedDate: string
  status: PawnStatus
  daysOverdue: number
  montoFormateado: string
  fechaFormateada: string
  diasTranscurridos: number
  statusColor: string
  statusIcon: string
  estadoPago: 'Pendiente' | 'Pagado'
}

export type PawnStatus = 'active' | 'warning' | 'overdue'

export interface PawnPaymentForm {
  cash: number
  card: number
  discount: number
  surcharge: number
  selectedCard: PaymentCardDto | null
  observation: string
}

export interface PawnStatistics {
  total: number
  totalAmount: string
  overdue: number
  averageAmount: string
  overduePercentage: number
}

// Filter and Sort Interfaces
export interface PawnFilters {
  searchTerm: string
  statusFilter: 'all' | 'active' | 'warning' | 'overdue' | 'paid' | 'unpaid'
  sortBy: 'date' | 'amount' | 'status'
}

export interface PaymentCalculationResult {
  cardSurcharge: number
  totalPayment: number
  totalWithSurcharge: number
  remaining: number
}

// Validation Interfaces
export interface PaymentValidationResult {
  isValid: boolean
  errors: string[]
}

// API Response Types for V1 endpoints
export type GetPawnsResponse = ApiResponse<PawnDto[]>
export type GetPawnResponse = ApiResponse<PawnDto>
export type CreatePawnResponse = ApiResponse<PawnDto>
export type UpdatePawnResponse = ApiResponse<PawnDto>
export type PayPawnResponse = ApiResponse<PawnPaymentDto>
export type GetPawnStatisticsResponse = ApiResponse<PawnStatisticsDto>
export type GetOverduePawnsResponse = ApiResponse<PawnDto[]>
export type GetPaymentCardsResponse = ApiResponse<PaymentCardDto[]>

// Utility Types
export type PawnSortField = 'date' | 'amount' | 'status'
export type PawnStatusFilter = 'all' | 'active' | 'warning' | 'overdue' | 'paid' | 'unpaid'
