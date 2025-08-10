import axiosClient from '../axiosClient'
import type {
  ApiResponse,
  CaracteristicaDto,
  CaracteristicaCreateDto,
  CaracteristicaUpdateDto,
  CaracteristicaRoomAssignDto,
  CaracteristicaWithImageDto,
} from '../types'

export class CaracteristicasService {
  private static readonly API_BASE = '/api/v1/caracteristicas'

  /**
   * Get all características
   */
  static async getCaracteristicas(): Promise<ApiResponse<CaracteristicaDto[]>> {
    try {
      const response = await axiosClient.get<ApiResponse<CaracteristicaDto[]>>(this.API_BASE)
      return response.data
    } catch (error) {
      console.error('Error fetching características:', error)
      throw error
    }
  }

  /**
   * Get característica by ID
   */
  static async getCaracteristica(
    caracteristicaId: number
  ): Promise<ApiResponse<CaracteristicaDto>> {
    try {
      const response = await axiosClient.get<ApiResponse<CaracteristicaDto>>(
        `${this.API_BASE}/${caracteristicaId}`
      )
      return response.data
    } catch (error) {
      console.error('Error fetching característica:', error)
      throw error
    }
  }

  /**
   * Create a new característica with optional icon upload
   */
  static async createCaracteristica(
    caracteristicaData: CaracteristicaCreateDto
  ): Promise<ApiResponse<CaracteristicaDto>> {
    try {
      const formData = this.buildFormData(caracteristicaData)

      const response = await axiosClient.post<ApiResponse<CaracteristicaDto>>(
        this.API_BASE,
        formData,
        {
          headers: { 'Content-Type': 'multipart/form-data' },
        }
      )
      return response.data
    } catch (error) {
      console.error('Error creating característica:', error)
      throw error
    }
  }

  /**
   * Update an existing característica
   */
  static async updateCaracteristica(
    caracteristicaId: number,
    caracteristicaData: CaracteristicaUpdateDto
  ): Promise<ApiResponse<CaracteristicaDto>> {
    try {
      const formData = this.buildFormData(caracteristicaData)

      const response = await axiosClient.put<ApiResponse<CaracteristicaDto>>(
        `${this.API_BASE}/${caracteristicaId}`,
        formData,
        {
          headers: { 'Content-Type': 'multipart/form-data' },
        }
      )
      return response.data
    } catch (error) {
      console.error('Error updating característica:', error)
      throw error
    }
  }

  /**
   * Delete a característica
   */
  static async deleteCaracteristica(caracteristicaId: number): Promise<ApiResponse<void>> {
    try {
      const response = await axiosClient.delete<ApiResponse<void>>(
        `${this.API_BASE}/${caracteristicaId}`
      )
      return response.data
    } catch (error) {
      console.error('Error deleting característica:', error)
      throw error
    }
  }

  /**
   * Get característica image as blob
   */
  static async getCaracteristicaImage(caracteristicaId: number): Promise<Blob> {
    try {
      const response = await axiosClient.get<Blob>(`${this.API_BASE}/${caracteristicaId}/image`, {
        responseType: 'blob',
      })
      return response.data
    } catch (error) {
      console.error('Error fetching característica image:', error)
      throw error
    }
  }

  /**
   * Get características associated with a room
   */
  static async getCaracteristicasByRoom(roomId: number): Promise<ApiResponse<CaracteristicaDto[]>> {
    try {
      const response = await axiosClient.get<ApiResponse<CaracteristicaDto[]>>(
        `${this.API_BASE}/room/${roomId}`
      )
      return response.data
    } catch (error) {
      console.error('Error fetching room características:', error)
      throw error
    }
  }

  /**
   * Assign características to a room
   */
  static async assignCaracteristicasToRoom(
    roomId: number,
    caracteristicaIds: number[]
  ): Promise<ApiResponse<void>> {
    try {
      const payload: CaracteristicaRoomAssignDto = { caracteristicaIds }

      const response = await axiosClient.post<ApiResponse<void>>(
        `${this.API_BASE}/room/${roomId}/assign`,
        payload
      )
      return response.data
    } catch (error) {
      console.error('Error assigning características to room:', error)
      throw error
    }
  }

  // ==================== Helper Methods ====================

  /**
   * Build FormData from característica data
   */
  private static buildFormData(
    caracteristicaData: CaracteristicaCreateDto | CaracteristicaUpdateDto
  ): FormData {
    const formData = new FormData()

    formData.append('nombre', caracteristicaData.nombre)
    formData.append('descripcion', caracteristicaData.descripcion || '')

    if (caracteristicaData.icono instanceof File) {
      formData.append('icono', caracteristicaData.icono)
    }

    return formData
  }

  /**
   * Process características with their images
   * Fetches and converts image blobs to URLs for display
   */
  static async processCaracteristicasWithImages(
    caracteristicas: CaracteristicaDto[]
  ): Promise<CaracteristicaWithImageDto[]> {
    try {
      const caracteristicasWithImages = await Promise.all(
        caracteristicas.map(async (caracteristica): Promise<CaracteristicaWithImageDto> => {
          const result: CaracteristicaWithImageDto = {
            ...caracteristica,
            icono: null,
          }

          if (caracteristica.icono && caracteristica.caracteristicaId) {
            try {
              const imageBlob = await this.getCaracteristicaImage(caracteristica.caracteristicaId)
              result.icono = URL.createObjectURL(imageBlob)
            } catch (error) {
              console.error(`Error loading image for ${caracteristica.nombre}:`, error)
            }
          }

          return result
        })
      )

      return caracteristicasWithImages
    } catch (error) {
      console.error('Error processing características with images:', error)
      throw error
    }
  }

  /**
   * Cleanup blob URLs to prevent memory leaks
   * Should be called when component unmounts or data changes
   */
  static cleanupBlobUrls(caracteristicas: CaracteristicaWithImageDto[] | null): void {
    if (!caracteristicas || !Array.isArray(caracteristicas)) {
      return
    }

    caracteristicas.forEach((caracteristica) => {
      if (
        caracteristica.icono &&
        typeof caracteristica.icono === 'string' &&
        caracteristica.icono.startsWith('blob:')
      ) {
        URL.revokeObjectURL(caracteristica.icono)
      }
    })
  }

  /**
   * Batch fetch características with their images
   * Optimized for loading multiple características at once
   */
  static async batchFetchWithImages(
    caracteristicaIds: number[]
  ): Promise<CaracteristicaWithImageDto[]> {
    try {
      // Fetch all características data in parallel
      const caracteristicasPromises = caracteristicaIds.map((id) => this.getCaracteristica(id))

      const caracteristicasResponses = await Promise.all(caracteristicasPromises)
      const caracteristicas = caracteristicasResponses
        .filter((response) => response?.data)
        .map((response) => response.data as CaracteristicaDto)

      // Process with images
      return await this.processCaracteristicasWithImages(caracteristicas)
    } catch (error) {
      console.error('Error batch fetching características:', error)
      throw error
    }
  }
}

// Export as default for backward compatibility
export default CaracteristicasService