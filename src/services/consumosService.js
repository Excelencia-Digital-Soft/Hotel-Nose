import axiosClient from '../axiosClient'

export class ConsumosService {
  // Get consumos by visita ID
  static async getConsumosByVisita(visitaId) {
    try {
      const response = await axiosClient.get(`/api/v1/consumos/visita/${visitaId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching consumos:', error)
      throw error
    }
  }

  // Get consumos summary
  static async getConsumosSummary(visitaId) {
    try {
      const response = await axiosClient.get(`/api/v1/consumos/visita/${visitaId}/summary`)
      return response.data
    } catch (error) {
      console.error('Error fetching consumos summary:', error)
      throw error
    }
  }

  // Add general consumos
  static async addGeneralConsumos(visitaId, habitacionId, consumos) {
    try {
      const payload = {
        visitaId,
        habitacionId,
        consumos
      }
      const response = await axiosClient.post('/api/v1/consumos/general', payload)
      return response.data
    } catch (error) {
      console.error('Error adding general consumos:', error)
      throw error
    }
  }

  // Add room consumos
  static async addRoomConsumos(visitaId, habitacionId, consumos) {
    try {
      const payload = {
        visitaId,
        habitacionId,
        consumos
      }
      const response = await axiosClient.post('/api/v1/consumos/room', payload)
      return response.data
    } catch (error) {
      console.error('Error adding room consumos:', error)
      throw error
    }
  }

  // Update consumo quantity
  static async updateConsumoQuantity(consumoId, cantidad) {
    try {
      const response = await axiosClient.put(`/api/v1/consumos/${consumoId}`, { cantidad })
      return response.data
    } catch (error) {
      console.error('Error updating consumo quantity:', error)
      throw error
    }
  }

  // Delete/Cancel consumo
  static async cancelConsumo(consumoId) {
    try {
      const response = await axiosClient.delete(`/api/v1/consumos/${consumoId}`)
      return response.data
    } catch (error) {
      console.error('Error canceling consumo:', error)
      throw error
    }
  }

  // Legacy methods for backward compatibility
  static async getLegacyConsumos(visitaId) {
    try {
      const response = await axiosClient.get(`/GetConsumosVisita?VisitaID=${visitaId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching legacy consumos:', error)
      throw error
    }
  }

  static async addLegacyGeneralConsumos(habitacionId, visitaId, selectedItems) {
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

  static async addLegacyRoomConsumos(habitacionId, visitaId, selectedItems) {
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

  static async updateLegacyConsumo(consumoId, cantidad) {
    try {
      const response = await axiosClient.put(`/UpdateConsumo?idConsumo=${consumoId}&Cantidad=${cantidad}`)
      return response.data
    } catch (error) {
      console.error('Error updating legacy consumo:', error)
      throw error
    }
  }

  static async cancelLegacyConsumo(consumoId) {
    try {
      const response = await axiosClient.delete(`/AnularConsumo?idConsumo=${consumoId}`)
      return response.data
    } catch (error) {
      console.error('Error canceling legacy consumo:', error)
      throw error
    }
  }
}