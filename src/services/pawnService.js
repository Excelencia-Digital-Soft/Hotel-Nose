import axiosClient from '../axiosClient'

export class PawnService {
  // Get all pawns for an institution
  static async getPawns(institucionId) {
    try {
      const response = await axiosClient.get(`/api/v1/pawns?institucionId=${institucionId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching pawns:', error)
      throw error
    }
  }

  // Get pawn by ID
  static async getPawn(pawnId) {
    try {
      const response = await axiosClient.get(`/api/v1/pawns/${pawnId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching pawn:', error)
      throw error
    }
  }

  // Create pawn
  static async createPawn(pawnData) {
    try {
      const response = await axiosClient.post('/api/v1/pawns', {
        detalle: pawnData.detalle,
        monto: pawnData.monto,
        visitaId: pawnData.visitaId,
        usuarioId: pawnData.usuarioId,
        observaciones: pawnData.observaciones
      })
      return response.data
    } catch (error) {
      console.error('Error creating pawn:', error)
      throw error
    }
  }

  // Update pawn amount
  static async updatePawnAmount(pawnId, newAmount) {
    try {
      const response = await axiosClient.put(`/api/v1/pawns/${pawnId}/amount`, {
        nuevoMonto: newAmount
      })
      return response.data
    } catch (error) {
      console.error('Error updating pawn amount:', error)
      throw error
    }
  }

  // Pay pawn
  static async payPawn(paymentData) {
    try {
      const response = await axiosClient.post('/api/v1/pawns/pay', {
        pawnId: paymentData.pawnId,
        montoEfectivo: paymentData.montoEfectivo,
        montoTarjeta: paymentData.montoTarjeta,
        observacion: paymentData.observacion,
        tarjetaId: paymentData.tarjetaId
      })
      return response.data
    } catch (error) {
      console.error('Error paying pawn:', error)
      throw error
    }
  }

  // Get pawn statistics
  static async getPawnStatistics(institucionId) {
    try {
      const response = await axiosClient.get(`/api/v1/pawns/statistics?institucionId=${institucionId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching pawn statistics:', error)
      throw error
    }
  }

  // Get overdue pawns
  static async getOverduePawns(institucionId, days = 30) {
    try {
      const response = await axiosClient.get(`/api/v1/pawns/overdue?institucionId=${institucionId}&days=${days}`)
      return response.data
    } catch (error) {
      console.error('Error fetching overdue pawns:', error)
      throw error
    }
  }

  // Get payment cards
  static async getPaymentCards(institucionId) {
    try {
      const response = await axiosClient.get(`/api/v1/payment-cards?institucionId=${institucionId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching payment cards:', error)
      throw error
    }
  }

  // Legacy methods for backward compatibility
  static async legacyGetPawns(institucionId) {
    try {
      const response = await axiosClient.get(`/api/Empeño/GetAllEmpenos?InstitucionID=${institucionId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching legacy pawns:', error)
      throw error
    }
  }

  static async legacyUpdatePawnAmount(pawnId, newAmount) {
    try {
      const response = await axiosClient.post(`/api/Empeño/ActualizarEmpeño`, {}, {
        params: {
          empeñoID: pawnId,
          nuevoMonto: newAmount
        }
      })
      return response.data
    } catch (error) {
      console.error('Error updating legacy pawn amount:', error)
      throw error
    }
  }

  static async legacyPayPawn(paymentData) {
    try {
      const response = await axiosClient.post(`/api/Empeño/PagarEmpeno`, {}, {
        params: {
          empeñoId: paymentData.pawnId,
          observacion: paymentData.observacion,
          montoEfectivo: paymentData.montoEfectivo,
          montoTarjeta: paymentData.montoTarjeta,
          TarjetaID: paymentData.tarjetaId
        }
      })
      return response.data
    } catch (error) {
      console.error('Error paying legacy pawn:', error)
      throw error
    }
  }

  static async legacyGetPaymentCards(institucionId) {
    try {
      const response = await axiosClient.get(`/GetTarjetas?InstitucionID=${institucionId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching legacy payment cards:', error)
      throw error
    }
  }

  // Utility methods
  static calculateDaysFromDate(dateString) {
    const pawnDate = new Date(dateString)
    const now = new Date()
    const diffTime = now - pawnDate
    return Math.floor(diffTime / (1000 * 60 * 60 * 24))
  }

  static calculateCardSurcharge(amount, percentage) {
    return (amount * percentage) / 100
  }

  static validatePaymentData(paymentData) {
    const errors = []

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
      errors
    }
  }

  static formatPawnForDisplay(pawn) {
    return {
      ...pawn,
      montoFormateado: new Intl.NumberFormat('es-CO', {
        style: 'currency',
        currency: 'COP',
        minimumFractionDigits: 0
      }).format(pawn.monto),
      fechaFormateada: new Date(pawn.fechaRegistro).toLocaleDateString('es-CO', {
        year: 'numeric',
        month: 'short',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
      }),
      diasTranscurridos: this.calculateDaysFromDate(pawn.fechaRegistro),
      estado: this.getPawnStatus(pawn)
    }
  }

  static getPawnStatus(pawn) {
    const days = this.calculateDaysFromDate(pawn.fechaRegistro)
    if (days > 30) return 'vencido'
    if (days > 15) return 'alerta'
    return 'activo'
  }

  static getStatusColor(status) {
    switch (status) {
      case 'vencido': return 'text-red-500'
      case 'alerta': return 'text-yellow-500'
      case 'activo': return 'text-green-500'
      default: return 'text-gray-500'
    }
  }

  static processPaymentCalculation(baseAmount, cardPercentage, cashAmount, cardAmount) {
    const cardSurcharge = this.calculateCardSurcharge(cardAmount, cardPercentage)
    const totalWithSurcharge = cashAmount + cardAmount + cardSurcharge
    
    return {
      cardSurcharge,
      totalPayment: cashAmount + cardAmount,
      totalWithSurcharge,
      remaining: baseAmount - totalWithSurcharge
    }
  }
}