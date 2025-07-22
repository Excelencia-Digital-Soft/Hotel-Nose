import axiosClient from '../axiosClient'
import { 
  ApiResponse, 
  PromocionDto, 
  PromocionCreateDto, 
  PromocionUpdateDto, 
  PromocionValidateDto,
  PromocionValidationResult 
} from '../types'

export class PromocionesService {
  // Get promociones by categoria
  static async getPromocionesByCategoria(categoriaId: number): Promise<ApiResponse<PromocionDto[]>> {
    try {
      const response = await axiosClient.get(`/api/v1/promociones/categoria/${categoriaId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching promociones by categoria:', error)
      throw error
    }
  }

  // Get active promociones
  static async getActivePromociones(): Promise<ApiResponse<PromocionDto[]>> {
    try {
      const response = await axiosClient.get('/api/v1/promociones/active')
      return response.data
    } catch (error) {
      console.error('Error fetching active promociones:', error)
      throw error
    }
  }

  // Get promocion by ID
  static async getPromocion(promocionId: number): Promise<ApiResponse<PromocionDto>> {
    try {
      const response = await axiosClient.get(`/api/v1/promociones/${promocionId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching promocion:', error)
      throw error
    }
  }

  // Create promocion
  static async createPromocion(promocionData: PromocionCreateDto): Promise<ApiResponse<PromocionDto>> {
    try {
      const response = await axiosClient.post('/api/v1/promociones', promocionData)
      return response.data
    } catch (error) {
      console.error('Error creating promocion:', error)
      throw error
    }
  }

  // Update promocion
  static async updatePromocion(promocionId: number, promocionData: PromocionUpdateDto): Promise<ApiResponse<PromocionDto>> {
    try {
      const response = await axiosClient.put(`/api/v1/promociones/${promocionId}`, promocionData)
      return response.data
    } catch (error) {
      console.error('Error updating promocion:', error)
      throw error
    }
  }

  // Delete promocion
  static async deletePromocion(promocionId: number): Promise<ApiResponse<void>> {
    try {
      const response = await axiosClient.delete(`/api/v1/promociones/${promocionId}`)
      return response.data
    } catch (error) {
      console.error('Error deleting promocion:', error)
      throw error
    }
  }

  // Validate promocion
  static async validatePromocion(promocionId: number, validationData: PromocionValidateDto): Promise<ApiResponse<PromocionValidationResult>> {
    try {
      const response = await axiosClient.post(`/api/v1/promociones/${promocionId}/validate`, validationData)
      return response.data
    } catch (error) {
      console.error('Error validating promocion:', error)
      throw error
    }
  }

}