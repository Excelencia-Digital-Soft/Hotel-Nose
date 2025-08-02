import axiosClient from '../axiosClient'
import type {
  ApiResponse,
  ComprehensiveCancelRequest,
  CreateReservaRequestDto,
  ReservaResponseDto,
} from '../types'
import type { ReservaExtensionDto } from '../types/reserva'

export class ReservasService {
  // Get reserva by ID
  static async getReserva(reservaId: number) {
    try {
      const response = await axiosClient.get(`/api/v1/reservas/${reservaId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching reserva:', error)
      throw error
    }
  }

  // Get active reservas
  static async getActiveReservas() {
    try {
      const response = await axiosClient.get('/api/v1/reservas/active')
      return response.data
    } catch (error) {
      console.error('Error fetching active reservas:', error)
      throw error
    }
  }

  // Finalize reserva
  static async finalizeReserva(reservaData: number) {
    try {
      const response = await axiosClient.post('/api/v1/reservas/finalize', reservaData)
      return response.data
    } catch (error) {
      console.error('Error finalizing reserva:', error)
      throw error
    }
  }

  // Pause ocupacion
  static async pauseOcupacion(visitaId: number) {
    try {
      const response = await axiosClient.post(`/api/v1/reservas/${visitaId}/pause`)
      return response.data
    } catch (error) {
      console.error('Error pausing ocupacion:', error)
      throw error
    }
  }

  // Resume ocupacion
  static async resumeOcupacion(visitaId: number) {
    try {
      const response = await axiosClient.post(`/api/v1/reservas/${visitaId}/resume`)
      return response.data
    } catch (error) {
      console.error('Error resuming ocupacion:', error)
      throw error
    }
  }

  // Update promotion
  static async updatePromotion(
    reservaId: number,
    promocionId: number | null
  ): Promise<ApiResponse> {
    try {
      const response = await axiosClient.put(`/api/v1/reservas/${reservaId}/promotion`, {
        promocionId,
      })
      return response.data
    } catch (error) {
      console.error('Error updating promotion:', error)
      throw error
    }
  }

  // Extend time
  static async extendTime(reservaId: number, extensionData: ReservaExtensionDto) {
    try {
      const response = await axiosClient.put(`/api/v1/reservas/${reservaId}/extend`, extensionData)
      return response.data
    } catch (error) {
      console.error('Error extending time:', error)
      throw error
    }
  }

  // Cancel reserva
  static async cancelReserva(reservaId: number) {
    try {
      const response = await axiosClient.delete(`/api/v1/reservas/${reservaId}`)
      return response.data
    } catch (error) {
      console.error('Error canceling reserva:', error)
      throw error
    }
  }

  // Comprehensive cancel (cancels reserva and all associated consumos)
  static async comprehensiveCancel(reservaId: number, motivo: string): Promise<ApiResponse> {
    try {
      const requestData: ComprehensiveCancelRequest = { reason: motivo }
      const response = await axiosClient.post(
        `/api/v1/reservas/${reservaId}/comprehensive-cancel`,
        requestData
      )
      return response.data
    } catch (error) {
      console.error('Error in comprehensive cancel:', error)
      throw error
    }
  }

  // Create new reserva (V1)
  static async createReserva(
    reservaData: CreateReservaRequestDto
  ): Promise<ApiResponse<ReservaResponseDto>> {
    try {
      const response = await axiosClient.post('/api/v1/reservas', reservaData)
      return response.data
    } catch (error) {
      console.error('Error creating reserva:', error)
      throw error
    }
  }

  // Legacy methods for backward compatibility
  static async legacyCreateReserva(institucionId: number, usuarioId: number, reservaData: any) {
    try {
      const response = await axiosClient.post(
        `/ReservarHabitacion?InstitucionID=${institucionId}&UsuarioID=${usuarioId}`,
        reservaData
      )
      return response.data
    } catch (error) {
      console.error('Error creating legacy reserva:', error)
      throw error
    }
  }

  static async legacyFinalizeReserva(habitacionId: number) {
    try {
      const response = await axiosClient.put(`/FinalizarReserva?idHabitacion=${habitacionId}`)
      return response.data
    } catch (error) {
      console.error('Error finalizing legacy reserva:', error)
      throw error
    }
  }

  static async legacyPauseOcupacion(visitaId: number) {
    try {
      const response = await axiosClient.put(`/PausarOcupacion?visitaId=${visitaId}`)
      return response.data
    } catch (error) {
      console.error('Error pausing legacy ocupacion:', error)
      throw error
    }
  }

  static async legacyUpdatePromotion(reservaId: number, promocionId: number) {
    try {
      const response = await axiosClient.put('/ActualizarReservaPromocion', null, {
        params: {
          reservaId,
          promocionId,
        },
      })
      return response.data
    } catch (error) {
      console.error('Error updating legacy promotion:', error)
      throw error
    }
  }
}

