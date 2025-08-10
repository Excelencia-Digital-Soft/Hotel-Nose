import axiosClient from '../axiosClient'
import type { ApiResponse, RoomImage, CategoriaDto } from '../types'

// Room DTO interfaces for V1 API
interface HabitacionDto {
  habitacionId: number
  nombreHabitacion: string
  categoriaId: number
  estado: 'Disponible' | 'Ocupado' | 'Mantenimiento' | 'Limpieza'
  capacidadMaxima: number
  precioPorNoche: number
  descripcion?: string
  imagenes?: number[]
  caracteristicas?: RoomCharacteristicDto[]
  institucionId: number
  isActive: boolean
  createdAt?: string
  updatedAt?: string
}

interface RoomCharacteristicDto {
  id: number
  nombre: string
  valor: string
  habitacionId: number
}

interface CreateRoomDto {
  institucionId: number
  nombreHabitacion: string
  categoriaId: number
  capacidadMaxima?: number
  precioPorNoche?: number
  descripcion?: string
  imagenes?: File[]
  caracteristicas?: Omit<RoomCharacteristicDto, 'id' | 'habitacionId'>[]
}

interface UpdateRoomDto {
  nombreHabitacion: string
  categoriaId: number
  capacidadMaxima?: number
  precioPorNoche?: number
  descripcion?: string
  estado?: 'Disponible' | 'Ocupado' | 'Mantenimiento' | 'Limpieza'
  usuarioId: number
  nuevasImagenes?: File[]
  imagenesEliminadas?: number[]
  caracteristicas?: Omit<RoomCharacteristicDto, 'id' | 'habitacionId'>[]
}

interface AvailableRoomsParams {
  institucionID: number
  fechaInicio: string | Date
  fechaFin: string | Date
}

interface UpdateRoomStatusDto {
  estado: 'Disponible' | 'Ocupado' | 'Mantenimiento' | 'Limpieza'
}

class HabitacionService {
  /**
   * Get all rooms for an institution
   */
  static async getRooms(): Promise<ApiResponse<HabitacionDto[]>> {
    try {
      const response = await axiosClient.get<ApiResponse<HabitacionDto[]>>(`/api/v1/habitaciones`)
      return response.data
    } catch (error: any) {
      console.error('Error fetching rooms:', error)
      return {
        isSuccess: false,
        message: error.response?.data?.message || 'Error al obtener las habitaciones',
        data: undefined,
      }
    }
  }

  /**
   * Get room by ID
   */
  static async getRoomById(habitacionID: number): Promise<ApiResponse<HabitacionDto>> {
    try {
      const response = await axiosClient.get<ApiResponse<HabitacionDto>>(
        `/api/v1/habitaciones/${habitacionID}`
      )
      return response.data
    } catch (error: any) {
      console.error('Error fetching room:', error)
      return {
        isSuccess: false,
        message: error.response?.data?.message || 'Error al obtener la habitación',
        data: undefined,
      }
    }
  }

  /**
   * Get categories for an institution
   */
  static async getCategories(): Promise<ApiResponse<CategoriaDto[]>> {
    try {
      const response = await axiosClient.get<ApiResponse<CategoriaDto[]>>(
        `/api/v1/habitacion-categorias`
      )
      return response.data
    } catch (error: any) {
      console.error('Error fetching categories:', error)
      return {
        isSuccess: false,
        message: error.response?.data?.message || 'Error al obtener las categorías',
        data: undefined,
      }
    }
  }

  /**
   * Create a new room
   */
  static async createRoom(roomData: CreateRoomDto): Promise<ApiResponse<HabitacionDto>> {
    try {
      // If there are images, we need to use FormData
      if (roomData.imagenes && roomData.imagenes.length > 0) {
        const formData = new FormData()
        formData.append('institucionId', roomData.institucionId.toString())
        formData.append('nombreHabitacion', roomData.nombreHabitacion)
        formData.append('categoriaHabitacionId', roomData.categoriaId.toString())

        if (roomData.capacidadMaxima !== undefined) {
          formData.append('capacidadMaxima', roomData.capacidadMaxima.toString())
        }
        if (roomData.precioPorNoche !== undefined) {
          formData.append('precioPorNoche', roomData.precioPorNoche.toString())
        }
        if (roomData.descripcion) {
          formData.append('descripcion', roomData.descripcion)
        }

        roomData.imagenes.forEach((image, _) => {
          formData.append(`imagenes`, image)
        })

        if (roomData.caracteristicas) {
          formData.append('caracteristicas', JSON.stringify(roomData.caracteristicas))
        }

        const response = await axiosClient.post<ApiResponse<HabitacionDto>>(
          '/api/v1/habitaciones',
          formData,
          {
            headers: {
              'Content-Type': 'multipart/form-data',
            },
          }
        )
        return response.data
      } else {
        // No images, send as JSON
        const response = await axiosClient.post<ApiResponse<HabitacionDto>>(
          '/api/v1/habitaciones',
          roomData
        )
        return response.data
      }
    } catch (error: any) {
      console.error('Error creating room:', error)
      return {
        isSuccess: false,
        message: error.response?.data?.message || 'Error al crear la habitación',
        data: undefined,
      }
    }
  }

  /**
   * Update a room
   */
  static async updateRoom(
    habitacionID: number,
    roomData: UpdateRoomDto
  ): Promise<ApiResponse<HabitacionDto>> {
    try {
      // If there are new images, we need to use FormData
      if (roomData.nuevasImagenes && roomData.nuevasImagenes.length > 0) {
        const formData = new FormData()
        formData.append('nombreHabitacion', roomData.nombreHabitacion)
        formData.append('categoriaId', roomData.categoriaId.toString())
        formData.append('usuarioId', roomData.usuarioId.toString())

        if (roomData.capacidadMaxima !== undefined) {
          formData.append('capacidadMaxima', roomData.capacidadMaxima.toString())
        }
        if (roomData.precioPorNoche !== undefined) {
          formData.append('precioPorNoche', roomData.precioPorNoche.toString())
        }
        if (roomData.descripcion) {
          formData.append('descripcion', roomData.descripcion)
        }
        if (roomData.estado) {
          formData.append('estado', roomData.estado)
        }

        roomData.nuevasImagenes.forEach((image, _) => {
          formData.append(`nuevasImagenes`, image)
        })

        if (roomData.imagenesEliminadas && roomData.imagenesEliminadas.length > 0) {
          roomData.imagenesEliminadas.forEach((id, index) => {
            formData.append(`imagenesEliminadas[${index}]`, id.toString())
          })
        }

        if (roomData.caracteristicas) {
          formData.append('caracteristicas', JSON.stringify(roomData.caracteristicas))
        }

        const response = await axiosClient.put<ApiResponse<HabitacionDto>>(
          `/api/v1/habitaciones/${habitacionID}`,
          formData,
          {
            headers: {
              'Content-Type': 'multipart/form-data',
            },
          }
        )
        return response.data
      } else {
        // No new images, send as JSON
        const response = await axiosClient.put<ApiResponse<HabitacionDto>>(
          `/api/v1/habitaciones/${habitacionID}`,
          roomData
        )
        return response.data
      }
    } catch (error: any) {
      console.error('Error updating room:', error)
      return {
        isSuccess: false,
        message: error.response?.data?.message || 'Error al actualizar la habitación',
        data: undefined,
      }
    }
  }

  /**
   * Delete a room
   */
  static async deleteRoom(habitacionID: number): Promise<ApiResponse<void>> {
    try {
      const response = await axiosClient.delete<ApiResponse<void>>(
        `/api/v1/habitaciones/${habitacionID}`
      )
      return response.data
    } catch (error: any) {
      console.error('Error deleting room:', error)
      return {
        isSuccess: false,
        message: error.response?.data?.message || 'Error al eliminar la habitación',
        data: undefined,
      }
    }
  }

  /**
   * Get available rooms for reservations
   */
  static async getAvailableRooms(
    params: AvailableRoomsParams
  ): Promise<ApiResponse<HabitacionDto[]>> {
    try {
      const response = await axiosClient.get<ApiResponse<HabitacionDto[]>>(
        `/api/v1/habitaciones/disponibles`,
        {
          params: {
            institucionID: params.institucionID,
            fechaInicio: params.fechaInicio,
            fechaFin: params.fechaFin,
          },
        }
      )
      return response.data
    } catch (error: any) {
      console.error('Error fetching available rooms:', error)
      return {
        isSuccess: false,
        message: error.response?.data?.message || 'Error al obtener habitaciones disponibles',
        data: undefined,
      }
    }
  }

  /**
   * Update room status
   */
  static async updateRoomStatus(
    habitacionID: number,
    estado: UpdateRoomStatusDto['estado']
  ): Promise<ApiResponse<HabitacionDto>> {
    try {
      const response = await axiosClient.patch<ApiResponse<HabitacionDto>>(
        `/api/v1/habitaciones/${habitacionID}/estado`,
        { estado }
      )
      return response.data
    } catch (error: any) {
      console.error('Error updating room status:', error)
      return {
        isSuccess: false,
        message: error.response?.data?.message || 'Error al actualizar el estado de la habitación',
        data: undefined,
      }
    }
  }

  /**
   * Upload room image
   */
  static async uploadRoomImage(
    habitacionID: number,
    imageFile: File
  ): Promise<ApiResponse<RoomImage>> {
    try {
      const formData = new FormData()
      formData.append('imagen', imageFile)

      const response = await axiosClient.post<ApiResponse<RoomImage>>(
        `/api/v1/habitaciones/${habitacionID}/imagen`,
        formData,
        {
          headers: {
            'Content-Type': 'multipart/form-data',
          },
        }
      )
      return response.data
    } catch (error: any) {
      console.error('Error uploading room image:', error)
      return {
        isSuccess: false,
        message: error.response?.data?.message || 'Error al subir la imagen',
        data: undefined,
      }
    }
  }

  /**
   * Delete room image
   */
  static async deleteRoomImage(habitacionID: number, imagenID: number): Promise<ApiResponse<void>> {
    try {
      const response = await axiosClient.delete<ApiResponse<void>>(
        `/api/v1/habitaciones/${habitacionID}/imagen/${imagenID}`
      )
      return response.data
    } catch (error: any) {
      console.error('Error deleting room image:', error)
      return {
        isSuccess: false,
        message: error.response?.data?.message || 'Error al eliminar la imagen',
        data: undefined,
      }
    }
  }
}

export default HabitacionService
export type {
  HabitacionDto,
  CreateRoomDto,
  UpdateRoomDto,
  RoomCharacteristicDto,
  AvailableRoomsParams,
  UpdateRoomStatusDto,
}
