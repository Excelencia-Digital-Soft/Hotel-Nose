import axiosClient from '../axiosClient'

export class CaracteristicasService {
  // Get all características
  static async getCaracteristicas() {
    try {
      const response = await axiosClient.get('/api/v1/caracteristicas')
      return response.data
    } catch (error) {
      console.error('Error fetching características:', error)
      throw error
    }
  }

  // Get característica by ID
  static async getCaracteristica(caracteristicaId) {
    try {
      const response = await axiosClient.get(`/api/v1/caracteristicas/${caracteristicaId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching característica:', error)
      throw error
    }
  }

  // Create característica
  static async createCaracteristica(caracteristicaData) {
    try {
      const formData = new FormData()
      formData.append('nombre', caracteristicaData.nombre)
      formData.append('descripcion', caracteristicaData.descripcion || '')
      
      if (caracteristicaData.icono) {
        formData.append('icono', caracteristicaData.icono)
      }

      const response = await axiosClient.post('/api/v1/caracteristicas', formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      })
      return response.data
    } catch (error) {
      console.error('Error creating característica:', error)
      throw error
    }
  }

  // Update característica
  static async updateCaracteristica(caracteristicaId, caracteristicaData) {
    try {
      const formData = new FormData()
      formData.append('nombre', caracteristicaData.nombre)
      formData.append('descripcion', caracteristicaData.descripcion || '')
      
      if (caracteristicaData.icono instanceof File) {
        formData.append('icono', caracteristicaData.icono)
      }

      const response = await axiosClient.put(`/api/v1/caracteristicas/${caracteristicaId}`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      })
      return response.data
    } catch (error) {
      console.error('Error updating característica:', error)
      throw error
    }
  }

  // Delete característica
  static async deleteCaracteristica(caracteristicaId) {
    try {
      const response = await axiosClient.delete(`/api/v1/caracteristicas/${caracteristicaId}`)
      return response.data
    } catch (error) {
      console.error('Error deleting característica:', error)
      throw error
    }
  }

  // Get característica image
  static async getCaracteristicaImage(caracteristicaId) {
    try {
      const response = await axiosClient.get(`/api/v1/caracteristicas/${caracteristicaId}/image`, {
        responseType: 'blob'
      })
      return response.data
    } catch (error) {
      console.error('Error fetching característica image:', error)
      throw error
    }
  }

  // Get características by room
  static async getCaracteristicasByRoom(roomId) {
    try {
      const response = await axiosClient.get(`/api/v1/caracteristicas/room/${roomId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching room características:', error)
      throw error
    }
  }

  // Assign características to room
  static async assignCaracteristicasToRoom(roomId, caracteristicaIds) {
    try {
      const response = await axiosClient.post(`/api/v1/caracteristicas/room/${roomId}/assign`, {
        caracteristicaIds
      })
      return response.data
    } catch (error) {
      console.error('Error assigning características to room:', error)
      throw error
    }
  }

  // Legacy methods for backward compatibility
  static async legacyGetCaracteristicas() {
    try {
      const response = await axiosClient.get('/api/Caracteristicas/GetCaracteristicas')
      return response.data
    } catch (error) {
      console.error('Error fetching legacy características:', error)
      throw error
    }
  }

  static async legacyCreateCaracteristica(caracteristicaData) {
    try {
      const formData = new FormData()
      formData.append('nombre', caracteristicaData.nombre)
      formData.append('descripcion', caracteristicaData.descripcion || '')
      
      if (caracteristicaData.icono) {
        formData.append('icono', caracteristicaData.icono)
      }

      const response = await axiosClient.post('/api/Caracteristicas/CrearCaracteristica', formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      })
      return response.data
    } catch (error) {
      console.error('Error creating legacy característica:', error)
      throw error
    }
  }

  static async legacyUpdateCaracteristica(caracteristicaId, caracteristicaData) {
    try {
      const formData = new FormData()
      formData.append('nombre', caracteristicaData.nombre)
      formData.append('descripcion', caracteristicaData.descripcion || '')
      
      if (caracteristicaData.icono instanceof File) {
        formData.append('icono', caracteristicaData.icono)
      }

      const response = await axiosClient.put(
        `/api/Caracteristicas/ActualizarCaracteristica/${caracteristicaId}`,
        formData,
        {
          headers: { 'Content-Type': 'multipart/form-data' }
        }
      )
      return response.data
    } catch (error) {
      console.error('Error updating legacy característica:', error)
      throw error
    }
  }

  static async legacyDeleteCaracteristica(caracteristicaId) {
    try {
      const response = await axiosClient.delete(`/api/Caracteristicas/EliminarCaracteristica/${caracteristicaId}`)
      return response.data
    } catch (error) {
      console.error('Error deleting legacy característica:', error)
      throw error
    }
  }

  static async legacyGetCaracteristicaImage(caracteristicaId) {
    try {
      const response = await axiosClient.get(`/api/Caracteristicas/GetImage/${caracteristicaId}`, {
        responseType: 'blob'
      })
      return response.data
    } catch (error) {
      console.error('Error fetching legacy característica image:', error)
      throw error
    }
  }

  // Helper method to process características with images
  static async processCaracteristicasWithImages(caracteristicas, useV1Api = false) {
    try {
      const caracteristicasWithImages = await Promise.all(
        caracteristicas.map(async (caracteristica) => {
          if (caracteristica.icono) {
            try {
              let imageBlob
              if (useV1Api) {
                imageBlob = await this.getCaracteristicaImage(caracteristica.caracteristicaId)
              } else {
                imageBlob = await this.legacyGetCaracteristicaImage(caracteristica.caracteristicaId)
              }
              caracteristica.icono = URL.createObjectURL(imageBlob)
            } catch (error) {
              console.error(`Error loading image for ${caracteristica.nombre}:`, error)
              caracteristica.icono = null
            }
          }
          return caracteristica
        })
      )
      return caracteristicasWithImages
    } catch (error) {
      console.error('Error processing características with images:', error)
      throw error
    }
  }

  // Cleanup blob URLs to prevent memory leaks
  static cleanupBlobUrls(caracteristicas) {
    if (caracteristicas && Array.isArray(caracteristicas)) {
      caracteristicas.forEach(caracteristica => {
        if (caracteristica.icono && caracteristica.icono.startsWith('blob:')) {
          URL.revokeObjectURL(caracteristica.icono)
        }
      })
    }
  }
}