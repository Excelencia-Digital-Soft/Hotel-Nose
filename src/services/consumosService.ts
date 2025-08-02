import axiosClient from '../axiosClient'
import type {
  ApiResponse,
  ConsumoCreateDto,
  ConsumoUpdateDto,
  ConsumoResponseDto,
  ConsumoSummaryDto,
} from '../types'

export class ConsumosService {
  // Get consumos by visita ID
  static async getConsumosByVisita(visitaId: number): Promise<ApiResponse<ConsumoResponseDto[]>> {
    try {
      const response = await axiosClient.get(`/api/v1/consumos/visita/${visitaId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching consumos:', error)
      throw error
    }
  }

  // Get consumos summary
  static async getConsumosSummary(visitaId: number): Promise<ApiResponse<ConsumoSummaryDto>> {
    try {
      const response = await axiosClient.get(`/api/v1/consumos/visita/${visitaId}/summary`)
      return response.data
    } catch (error) {
      console.error('Error fetching consumos summary:', error)
      throw error
    }
  }

  // Add general consumos
  static async addGeneralConsumos(
    visitaId: number,
    habitacionId: number,
    consumos: any[]
  ): Promise<ApiResponse<ConsumoResponseDto[]>> {
    try {
      // Map component format to DTO format
      const items: ConsumoCreateDto[] = consumos.map((item) => ({
        articuloId: item.articuloId,
        cantidad: item.cantidad,
        precioUnitario: item.precio, // Changed from 'precio' to 'precioUnitario'
        esHabitacion: false, // For general consumos
      }))

      // Use query parameters for habitacionId and visitaId
      const url = `/api/v1/consumos/general?habitacionId=${habitacionId}&visitaId=${visitaId}`

      console.log('Sending consumos to V1 API:', url, items)
      // Send items array directly as body
      const response = await axiosClient.post(url, items)
      return response.data
    } catch (error) {
      console.error('Error adding general consumos:', error)
      throw error
    }
  }

  // Add room consumos
  static async addRoomConsumos(
    visitaId: number,
    habitacionId: number,
    consumos: any[]
  ): Promise<ApiResponse<ConsumoResponseDto[]>> {
    try {
      // Map component format to DTO format
      const items: ConsumoCreateDto[] = consumos.map((item) => ({
        articuloId: item.articuloId,
        cantidad: item.cantidad,
        precioUnitario: item.precio, // Changed from 'precio' to 'precioUnitario'
        esHabitacion: true, // For room consumos
      }))

      // Use query parameters for habitacionId and visitaId
      const url = `/api/v1/consumos/room?habitacionId=${habitacionId}&visitaId=${visitaId}`

      console.log('Sending room consumos to V1 API:', url, items)
      // Send items array directly as body
      const response = await axiosClient.post(url, items)
      return response.data
    } catch (error) {
      console.error('Error adding room consumos:', error)
      throw error
    }
  }

  // Update consumo quantity
  static async updateConsumoQuantity(
    consumoId: number,
    cantidad: number
  ): Promise<ApiResponse<ConsumoResponseDto>> {
    try {
      const updateData: ConsumoUpdateDto = { cantidad }
      const response = await axiosClient.put(`/api/v1/consumos/${consumoId}`, updateData)
      return response.data
    } catch (error) {
      console.error('Error updating consumo quantity:', error)
      throw error
    }
  }

  // Delete/Cancel consumo
  static async cancelConsumo(consumoId: number): Promise<ApiResponse<void>> {
    try {
      const response = await axiosClient.delete(`/api/v1/consumos/${consumoId}`)
      return response.data
    } catch (error) {
      console.error('Error canceling consumo:', error)
      throw error
    }
  }

  // Legacy methods for backward compatibility
  static async getLegacyConsumos(visitaId: number): Promise<any> {
    try {
      const response = await axiosClient.get(`/GetConsumosVisita?VisitaID=${visitaId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching legacy consumos:', error)
      throw error
    }
  }

  static async addLegacyGeneralConsumos(
    habitacionId: number,
    visitaId: number,
    selectedItems: any[]
  ): Promise<any> {
    try {
      const response = await axiosClient.post(
        `/ConsumoGeneral?habitacionId=${habitacionId}&visitaId=${visitaId}`,
        selectedItems
      )
      return response.data
    } catch (error) {
      console.error('Error adding legacy general consumos:', error)
      throw error
    }
  }

  static async addLegacyRoomConsumos(
    habitacionId: number,
    visitaId: number,
    selectedItems: any[]
  ): Promise<any> {
    try {
      const response = await axiosClient.post(
        `/ConsumoHabitacion?habitacionId=${habitacionId}&visitaId=${visitaId}`,
        selectedItems
      )
      return response.data
    } catch (error) {
      console.error('Error adding legacy room consumos:', error)
      throw error
    }
  }

  static async updateLegacyConsumo(consumoId: number, cantidad: number): Promise<any> {
    try {
      const response = await axiosClient.put(
        `/UpdateConsumo?idConsumo=${consumoId}&Cantidad=${cantidad}`
      )
      return response.data
    } catch (error) {
      console.error('Error updating legacy consumo:', error)
      throw error
    }
  }

  static async cancelLegacyConsumo(consumoId: number): Promise<any> {
    try {
      const response = await axiosClient.delete(`/AnularConsumo?idConsumo=${consumoId}`)
      return response.data
    } catch (error) {
      console.error('Error canceling legacy consumo:', error)
      throw error
    }
  }
}

