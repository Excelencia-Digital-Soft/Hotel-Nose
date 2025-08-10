import axiosClient from '../axiosClient'
import type { 
  ApiResponse, 
  InventoryDto, 
  InventoryCreateDto 
} from '../types'

export class InventarioService {
  private static readonly API_BASE = '/api/v1/inventory'

  /**
   * Get all inventory items for a specific room
   */
  static async getRoomInventory(roomId: number): Promise<ApiResponse<InventoryDto[]>> {
    try {
      const response = await axiosClient.get<ApiResponse<InventoryDto[]>>(
        `${this.API_BASE}/rooms/${roomId}`
      )
      return response.data
    } catch (error) {
      console.error('Error fetching room inventory:', error)
      throw error
    }
  }

  /**
   * Get combined inventory view (general + room inventories)
   */
  static async getCombinedInventory(roomId: number): Promise<ApiResponse<InventoryDto[]>> {
    try {
      const response = await axiosClient.get<ApiResponse<InventoryDto[]>>(
        `${this.API_BASE}/rooms/${roomId}/combined`
      )
      return response.data
    } catch (error) {
      console.error('Error fetching combined inventory:', error)
      throw error
    }
  }

  /**
   * Add inventory item to a room
   */
  static async addRoomInventory(
    roomId: number, 
    inventoryData: InventoryCreateDto
  ): Promise<ApiResponse<InventoryDto>> {
    try {
      const response = await axiosClient.post<ApiResponse<InventoryDto>>(
        `${this.API_BASE}/rooms/${roomId}`,
        inventoryData
      )
      return response.data
    } catch (error) {
      console.error('Error adding room inventory:', error)
      throw error
    }
  }

  /**
   * Adapter method to transform legacy inventory structure to V1 InventoryDto
   */
  static adaptLegacyToV1(legacyItem: any): InventoryDto {
    return {
      inventoryId: legacyItem.inventarioId,
      articuloId: legacyItem.articulo?.articuloId || legacyItem.articuloId,
      articuloNombre: legacyItem.articulo?.nombreArticulo || legacyItem.articuloNombre,
      articuloPrecio: legacyItem.articulo?.precio || legacyItem.articuloPrecio,
      articuloDescripcion: legacyItem.articulo?.descripcion || legacyItem.articuloDescripcion,
      cantidad: legacyItem.cantidad,
      locationType: 'Room',
      locationId: legacyItem.habitacionId || 0
    }
  }

  /**
   * Adapter method to transform V1 InventoryDto to component-expected structure
   */
  static adaptV1ToComponent(v1Item: InventoryDto): any {
    return {
      inventarioId: v1Item.inventoryId,
      cantidad: v1Item.cantidad,
      articulo: {
        articuloId: v1Item.articuloId,
        nombreArticulo: v1Item.articuloNombre,
        precio: v1Item.articuloPrecio,
        descripcion: v1Item.articuloDescripcion || '',
        imageUrl: v1Item.articuloImagenUrl || null // Will be populated separately with image service
      }
    }
  }
}

// Export as default for backward compatibility
export default InventarioService