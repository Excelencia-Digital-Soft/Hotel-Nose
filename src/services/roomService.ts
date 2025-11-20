import axiosClient from '../axiosClient';
import { 
  ApiResponse, 
  Habitacion, 
  HabitacionAvailabilityDto
} from '../types';

export class RoomService {
  // Get all rooms for an institution
  static async getRooms(institucionId: number): Promise<ApiResponse<Habitacion[]>> {
    try {
      const response = await axiosClient.get(`/api/v1/rooms?institucionId=${institucionId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching rooms:', error)
      throw error
    }
  }

  // Get room by ID
  static async getRoom(roomId) {
    try {
      const response = await axiosClient.get(`/api/v1/rooms/${roomId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching room:', error)
      throw error
    }
  }

  // Create room with images
  static async createRoom(roomData) {
    try {
      const formData = new FormData()
      formData.append('institucionId', roomData.institucionId)
      formData.append('nombreHabitacion', roomData.nombreHabitacion)
      formData.append('categoriaId', roomData.categoriaId)
      
      if (roomData.imagenes) {
        roomData.imagenes.forEach((file) => {
          formData.append('imagenes', file)
        })
      }

      const response = await axiosClient.post('/api/v1/rooms', formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      })
      return response.data
    } catch (error) {
      console.error('Error creating room:', error)
      throw error
    }
  }

  // Update room
  static async updateRoom(roomId, roomData) {
    try {
      const formData = new FormData()
      formData.append('nombreHabitacion', roomData.nombreHabitacion)
      formData.append('categoriaId', roomData.categoriaId)
      formData.append('usuarioId', roomData.usuarioId)
      
      if (roomData.nuevasImagenes) {
        roomData.nuevasImagenes.forEach((file) => {
          formData.append('nuevasImagenes', file)
        })
      }

      if (roomData.removedImageIds) {
        formData.append('removedImageIds', JSON.stringify(roomData.removedImageIds))
      }

      const response = await axiosClient.put(`/api/v1/rooms/${roomId}`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      })
      return response.data
    } catch (error) {
      console.error('Error updating room:', error)
      throw error
    }
  }

  // Delete room
  static async deleteRoom(roomId) {
    try {
      const response = await axiosClient.delete(`/api/v1/rooms/${roomId}`)
      return response.data
    } catch (error) {
      console.error('Error deleting room:', error)
      throw error
    }
  }

  // Get room categories
  static async getCategories(institucionId) {
    try {
      const response = await axiosClient.get(`/api/v1/rooms/categories?institucionId=${institucionId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching categories:', error)
      throw error
    }
  }

  // Get room images
  static async getRoomImages(roomId) {
    try {
      const response = await axiosClient.get(`/api/v1/rooms/${roomId}/images`)
      return response.data
    } catch (error) {
      console.error('Error fetching room images:', error)
      throw error
    }
  }

  // Delete room image
  static async deleteRoomImage(imageId) {
    try {
      const response = await axiosClient.delete(`/api/v1/rooms/images/${imageId}`)
      return response.data
    } catch (error) {
      console.error('Error deleting room image:', error)
      throw error
    }
  }

  // Get room characteristics
  static async getRoomCharacteristics(roomId) {
    try {
      const response = await axiosClient.get(`/api/v1/rooms/${roomId}/characteristics`)
      return response.data
    } catch (error) {
      console.error('Error fetching room characteristics:', error)
      throw error
    }
  }

  // Change room availability
  static async changeAvailability(roomId: number, disponible: boolean): Promise<ApiResponse> {
    try {
      const requestData: HabitacionAvailabilityDto = { disponible };
      const response = await axiosClient.patch(`/api/v1/habitaciones/${roomId}/availability`, requestData)
      return response.data
    } catch (error) {
      console.error('Error changing room availability:', error)
      throw error
    }
  }

  // Legacy methods for backward compatibility
  static async legacyGetRooms(institucionId) {
    try {
      const response = await axiosClient.get(`/GetHabitaciones?InstitucionID=${institucionId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching legacy rooms:', error)
      throw error
    }
  }

  static async legacyGetCategories(institucionId) {
    try {
      const response = await axiosClient.get(`/api/Objetos/GetCategorias?InstitucionID=${institucionId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching legacy categories:', error)
      throw error
    }
  }

  static async legacyCreateRoom(roomData) {
    try {
      const formData = new FormData()
      formData.append('institucionID', roomData.institucionId)
      formData.append('nombreHabitacion', roomData.nombreHabitacion)
      formData.append('categoriaID', roomData.categoriaId)
      
      if (roomData.imagenes) {
        roomData.imagenes.forEach((file) => {
          formData.append('imagenes', file)
        })
      }

      const response = await axiosClient.post('/CrearHabitacionConImagenes', formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      })
      return response.data
    } catch (error) {
      console.error('Error creating legacy room:', error)
      throw error
    }
  }

  static async legacyUpdateRoom(roomId, roomData) {
    try {
      const formData = new FormData()
      formData.append('id', roomId)
      formData.append('nuevoNombre', roomData.nombreHabitacion)
      formData.append('nuevaCategoria', roomData.categoriaId)
      formData.append('usuarioId', roomData.usuarioId)
      
      if (roomData.nuevasImagenes) {
        roomData.nuevasImagenes.forEach((file) => {
          formData.append('nuevasImagenes', file)
        })
      }

      const response = await axiosClient.put('/ActualizarHabitacion', formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      })
      return response.data
    } catch (error) {
      console.error('Error updating legacy room:', error)
      throw error
    }
  }

  static async legacyDeleteRoom(roomId) {
    try {
      const response = await axiosClient.delete(`/AnularHabitacion?idHabitacion=${roomId}&Estado=true`)
      return response.data
    } catch (error) {
      console.error('Error deleting legacy room:', error)
      throw error
    }
  }

  static async legacyDeleteRoomImage(imageId) {
    try {
      const response = await axiosClient.delete(`/EliminarImagenHabitacion?imagenId=${imageId}`)
      return response.data
    } catch (error) {
      console.error('Error deleting legacy room image:', error)
      throw error
    }
  }
}