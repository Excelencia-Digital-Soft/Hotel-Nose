import axiosClient from '../axiosClient'

export class PromocionesService {
  // Get promociones by categoria
  static async getPromocionesByCategoria(categoriaId) {
    try {
      const response = await axiosClient.get(`/api/v1/promociones/categoria/${categoriaId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching promociones by categoria:', error)
      throw error
    }
  }

  // Get active promociones
  static async getActivePromociones() {
    try {
      const response = await axiosClient.get('/api/v1/promociones/active')
      return response.data
    } catch (error) {
      console.error('Error fetching active promociones:', error)
      throw error
    }
  }

  // Get promocion by ID
  static async getPromocion(promocionId) {
    try {
      const response = await axiosClient.get(`/api/v1/promociones/${promocionId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching promocion:', error)
      throw error
    }
  }

  // Create promocion
  static async createPromocion(promocionData) {
    try {
      const response = await axiosClient.post('/api/v1/promociones', promocionData)
      return response.data
    } catch (error) {
      console.error('Error creating promocion:', error)
      throw error
    }
  }

  // Update promocion
  static async updatePromocion(promocionId, promocionData) {
    try {
      const response = await axiosClient.put(`/api/v1/promociones/${promocionId}`, promocionData)
      return response.data
    } catch (error) {
      console.error('Error updating promocion:', error)
      throw error
    }
  }

  // Delete promocion
  static async deletePromocion(promocionId) {
    try {
      const response = await axiosClient.delete(`/api/v1/promociones/${promocionId}`)
      return response.data
    } catch (error) {
      console.error('Error deleting promocion:', error)
      throw error
    }
  }

  // Validate promocion
  static async validatePromocion(promocionId, validationData) {
    try {
      const response = await axiosClient.post(`/api/v1/promociones/${promocionId}/validate`, validationData)
      return response.data
    } catch (error) {
      console.error('Error validating promocion:', error)
      throw error
    }
  }

  // Legacy methods for backward compatibility
  static async getLegacyPromocionesByCategoria(categoriaId) {
    try {
      const response = await axiosClient.get(`/api/Promociones/GetPromocionesCategoria?categoriaID=${categoriaId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching legacy promociones by categoria:', error)
      throw error
    }
  }
}