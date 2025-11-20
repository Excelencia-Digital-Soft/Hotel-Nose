import axiosClient from '../axiosClient'
import type {
  PawnDto,
  CreatePawnRequest,
  UpdatePawnAmountRequest,
  PayPawnRequest,
  GetPawnsResponse,
  GetPawnResponse,
  CreatePawnResponse,
  UpdatePawnResponse,
  PayPawnResponse,
  GetPaymentCardsResponse,
  FormattedPawnDto,
  PaymentValidationResult,
  PaymentCalculationResult,
  PawnStatus,
} from '../types'

export class PawnService {
  // Base API endpoints
  private static readonly BASE_URL = '/api/v1'
  private static readonly EMPENOS_ENDPOINT = `${this.BASE_URL}/empenos`

  // Generic HTTP request wrapper with error handling
  private static async makeRequest<T>(
    method: 'GET' | 'POST' | 'PUT' | 'DELETE',
    endpoint: string,
    data?: any,
    errorMessage?: string
  ): Promise<T> {
    try {
      const config = data ? { data } : {}
      const response = await axiosClient.request<T>({
        method: method.toLowerCase() as any,
        url: endpoint,
        ...config,
      })
      return response.data
    } catch (error) {
      console.error(errorMessage || `Error with ${method} ${endpoint}:`, error)
      throw error
    }
  }

  // Helper methods for building URLs
  private static buildEmpenos(path = '', params?: Record<string, any>): string {
    let url = `${this.EMPENOS_ENDPOINT}${path}`
    if (params) {
      const searchParams = new URLSearchParams()
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined) searchParams.append(key, String(value))
      })
      url += `?${searchParams.toString()}`
    }
    return url
  }

  // === Core API Methods ===

  /**
   * Gets all unpaid empeños for the current institution
   */
  static async getPawns(): Promise<GetPawnsResponse> {
    return this.makeRequest<GetPawnsResponse>(
      'GET',
      this.buildEmpenos(''),
      undefined,
      'Error fetching pawns'
    )
  }

  /**
   * Get pawn by ID
   */
  static async getPawn(pawnId: number): Promise<GetPawnResponse> {
    return this.makeRequest<GetPawnResponse>(
      'GET',
      this.buildEmpenos(`/${pawnId}`),
      undefined,
      'Error fetching pawn'
    )
  }

  /**
   * Create new pawn
   */
  static async createPawn(pawnData: CreatePawnRequest): Promise<CreatePawnResponse> {
    const payload = {
      detalle: pawnData.detalle,
      monto: pawnData.monto,
      visitaId: pawnData.visitaId,
      usuarioId: pawnData.usuarioId,
      observaciones: pawnData.observaciones,
    }

    return this.makeRequest<CreatePawnResponse>(
      'POST',
      this.buildEmpenos(),
      payload,
      'Error creating pawn'
    )
  }

  /**
   * Update pawn amount
   */
  static async updatePawnAmount(pawnId: number, newAmount: number): Promise<UpdatePawnResponse> {
    const payload: UpdatePawnAmountRequest = { nuevoMonto: newAmount }

    return this.makeRequest<UpdatePawnResponse>(
      'PUT',
      this.buildEmpenos(`/${pawnId}`),
      payload,
      'Error updating pawn amount'
    )
  }

  /**
   * Pay pawn
   */
  static async payPawn(paymentData: PayPawnRequest): Promise<PayPawnResponse> {
    const payload = {
      montoEfectivo: paymentData.montoEfectivo,
      montoTarjeta: paymentData.montoTarjeta,
      observacion: paymentData.observacion,
      tarjetaId: paymentData.tarjetaId,
    }

    return this.makeRequest<PayPawnResponse>(
      'POST',
      this.buildEmpenos(`/${paymentData.pawnId}/payment`),
      payload,
      'Error paying pawn'
    )
  }

  /**
   * Get all empeños (paid and unpaid)
   */
  static async getAllPawns(): Promise<GetPawnsResponse> {
    return this.makeRequest<GetPawnsResponse>(
      'GET',
      this.buildEmpenos('/all'),
      undefined,
      'Error fetching all pawns'
    )
  }

  /**
   * Get empeños by visit ID
   */
  static async getPawnsByVisita(visitaId: number): Promise<GetPawnsResponse> {
    return this.makeRequest<GetPawnsResponse>(
      'GET',
      this.buildEmpenos(`/by-visita/${visitaId}`),
      undefined,
      'Error fetching pawns by visita'
    )
  }

  /**
   * Validate visit for pawn operations
   */
  static async validateVisita(visitaId: number): Promise<GetPawnResponse> {
    return this.makeRequest<GetPawnResponse>(
      'GET',
      this.buildEmpenos(`/validate-visita/${visitaId}`),
      undefined,
      'Error validating visita'
    )
  }

  /**
   * Get payment cards for current institution (legacy endpoint)
   */
  static async getPaymentCards(institucionID: number): Promise<GetPaymentCardsResponse> {
    return this.makeRequest<GetPaymentCardsResponse>(
      'GET',
      `/GetTarjetas?InstitucionID=${institucionID}`,
      undefined,
      'Error fetching payment cards'
    )
  }

  /**
   * Check service health
   */
  static async checkHealth(): Promise<{ isSuccess: boolean; message: string }> {
    return this.makeRequest<{ isSuccess: boolean; message: string }>(
      'GET',
      this.buildEmpenos('/health'),
      undefined,
      'Error checking service health'
    )
  }

  // === Utility Methods ===

  /**
   * Calculate days from date string
   */
  static calculateDaysFromDate(dateString: string): number {
    const pawnDate = new Date(dateString)
    const now = new Date()
    const diffTime = now.getTime() - pawnDate.getTime()
    return Math.floor(diffTime / (1000 * 60 * 60 * 24))
  }

  /**
   * Calculate card surcharge
   */
  static calculateCardSurcharge(amount: number, percentage: number): number {
    return (amount * percentage) / 100
  }

  /**
   * Validate payment data
   */
  static validatePaymentData(paymentData: PayPawnRequest): PaymentValidationResult {
    const errors: string[] = []

    if (!paymentData.pawnId) {
      errors.push('ID del empeño es requerido')
    }

    if (paymentData.montoEfectivo < 0 || paymentData.montoTarjeta < 0) {
      errors.push('Los montos no pueden ser negativos')
    }

    if (paymentData.montoEfectivo === 0 && paymentData.montoTarjeta === 0) {
      errors.push('Debe especificar al menos un método de pago')
    }

    if (paymentData.montoTarjeta > 0 && !paymentData.tarjetaId) {
      errors.push('Debe seleccionar una tarjeta para pagos con tarjeta')
    }

    return {
      isValid: errors.length === 0,
      errors,
    }
  }

  /**
   * Get pawn status based on registration date
   */
  static getPawnStatus(pawn: PawnDto): PawnStatus {
    const days = this.calculateDaysFromDate(pawn.fechaRegistro)
    if (days > 30) return 'overdue'
    if (days > 15) return 'warning'
    return 'active'
  }

  /**
   * Get status color class for UI
   */
  static getStatusColor(status: PawnStatus): string {
    switch (status) {
      case 'overdue':
        return 'text-red-400'
      case 'warning':
        return 'text-yellow-400'
      case 'active':
        return 'text-green-400'
      default:
        return 'text-white'
    }
  }

  /**
   * Get status icon class for UI
   */
  static getStatusIcon(status: PawnStatus): string {
    switch (status) {
      case 'overdue':
        return 'pi-exclamation-triangle'
      case 'warning':
        return 'pi-clock'
      case 'active':
        return 'pi-check-circle'
      default:
        return 'pi-circle'
    }
  }

  /**
   * Get status text for display
   */
  static getStatusText(status: PawnStatus): string {
    switch (status) {
      case 'overdue':
        return 'Vencido'
      case 'warning':
        return 'En Alerta'
      case 'active':
        return 'Activo'
      default:
        return 'Desconocido'
    }
  }

  /**
   * Format currency for display (Colombian Pesos)
   */
  static formatCurrency(amount: number): string {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP',
      minimumFractionDigits: 0,
    }).format(amount)
  }

  /**
   * Format date for display (Colombian locale)
   */
  static formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('es-CO', {
      year: 'numeric',
      month: 'short',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
    })
  }

  /**
   * Format pawn for display with all computed properties
   */
  static formatPawnForDisplay(pawn: PawnDto): FormattedPawnDto {
    const daysTranscurridos = this.calculateDaysFromDate(pawn.fechaRegistro)
    const status = this.getPawnStatus(pawn)
    const formattedAmount = this.formatCurrency(pawn.monto)
    const formattedDate = this.formatDate(pawn.fechaRegistro)

    return {
      ...pawn,
      // Legacy format properties (for backward compatibility)
      montoFormateado: formattedAmount,
      fechaFormateada: formattedDate,
      diasTranscurridos: daysTranscurridos,
      // New format properties
      formattedAmount,
      formattedDate,
      daysOverdue: daysTranscurridos,
      status,
      statusColor: this.getStatusColor(status),
      statusIcon: this.getStatusIcon(status),
    }
  }

  /**
   * Process payment calculation with surcharges
   */
  static processPaymentCalculation(
    baseAmount: number,
    cardPercentage: number,
    cashAmount: number,
    cardAmount: number
  ): PaymentCalculationResult {
    const cardSurcharge = this.calculateCardSurcharge(cardAmount, cardPercentage)
    const totalWithSurcharge = cashAmount + cardAmount + cardSurcharge

    return {
      cardSurcharge,
      totalPayment: cashAmount + cardAmount,
      totalWithSurcharge,
      remaining: baseAmount - totalWithSurcharge,
    }
  }
}

