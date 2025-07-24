import axiosClient from '../axiosClient'
import type {
  RoomInventoryDto,
  RoomInventoryCreateDto,
  RoomInventoryUpdateDto,
  RoomInventoryBulkUpdateDto,
  RoomInventorySummaryDto,
  InventoryAlertDto,
  RoomForInventoryDto,
  ArticleForRoomDto,
  InventoryMovementDto,
  RoomInventoryReportDto,
  InventoryTransferDto,
  RoomInventoryResponse,
  RoomInventoryListResponse,
  RoomInventorySummaryResponse,
  InventoryAlertsResponse,
  RoomForInventoryResponse,
  ArticleForRoomResponse,
  InventoryReportResponse,
  ApiResponse
} from '../types'

/**
 * Room Inventory Service
 * Handles all room inventory-related API calls
 * Base endpoint: /api/v1/room-inventory
 */
export class RoomInventoryService {
  private static readonly BASE_URL = '/api/v1/room-inventory'

  // Room Inventory CRUD Operations
  static async getAllInventories(habitacionId?: number): Promise<RoomInventoryListResponse> {
    const params = habitacionId ? { habitacionId } : undefined
    const response = await axiosClient.get(this.BASE_URL, { params })
    return response.data
  }

  static async getInventoryById(roomInventoryId: number): Promise<RoomInventoryResponse> {
    const response = await axiosClient.get(`${this.BASE_URL}/${roomInventoryId}`)
    return response.data
  }

  static async getInventoriesByRoom(habitacionId: number): Promise<RoomInventoryListResponse> {
    const response = await axiosClient.get(`${this.BASE_URL}/room/${habitacionId}`)
    return response.data
  }

  static async createInventory(data: RoomInventoryCreateDto): Promise<RoomInventoryResponse> {
    const response = await axiosClient.post(this.BASE_URL, data)
    return response.data
  }

  static async updateInventory(roomInventoryId: number, data: RoomInventoryUpdateDto): Promise<RoomInventoryResponse> {
    const response = await axiosClient.put(`${this.BASE_URL}/${roomInventoryId}`, data)
    return response.data
  }

  static async deleteInventory(roomInventoryId: number): Promise<ApiResponse> {
    const response = await axiosClient.delete(`${this.BASE_URL}/${roomInventoryId}`)
    return response.data
  }

  // Bulk Operations
  static async bulkUpdateInventories(data: RoomInventoryBulkUpdateDto): Promise<RoomInventoryListResponse> {
    const response = await axiosClient.put(`${this.BASE_URL}/bulk-update`, data)
    return response.data
  }

  static async initializeRoomInventory(habitacionId: number, articuloIds: number[]): Promise<RoomInventoryListResponse> {
    const payload = { habitacionId, articuloIds }
    const response = await axiosClient.post(`${this.BASE_URL}/initialize`, payload)
    return response.data
  }

  // Inventory Summary and Reports
  static async getRoomInventorySummary(habitacionId?: number): Promise<RoomInventorySummaryResponse> {
    const params = habitacionId ? { habitacionId } : undefined
    const response = await axiosClient.get(`${this.BASE_URL}/summary`, { params })
    return response.data
  }

  static async getInventoryReport(habitacionId: number): Promise<InventoryReportResponse> {
    const response = await axiosClient.get(`${this.BASE_URL}/report/${habitacionId}`)
    return response.data
  }

  static async getInventoryReportByDateRange(
    habitacionId: number, 
    fechaInicio: string, 
    fechaFin: string
  ): Promise<InventoryReportResponse> {
    const params = { fechaInicio, fechaFin }
    const response = await axiosClient.get(`${this.BASE_URL}/report/${habitacionId}/date-range`, { params })
    return response.data
  }

  // Inventory Alerts
  static async getInventoryAlerts(habitacionId?: number): Promise<InventoryAlertsResponse> {
    const params = habitacionId ? { habitacionId } : undefined
    const response = await axiosClient.get(`${this.BASE_URL}/alerts`, { params })
    return response.data
  }

  static async getLowStockAlerts(): Promise<InventoryAlertsResponse> {
    const response = await axiosClient.get(`${this.BASE_URL}/alerts/low-stock`)
    return response.data
  }

  static async getOutOfStockAlerts(): Promise<InventoryAlertsResponse> {
    const response = await axiosClient.get(`${this.BASE_URL}/alerts/out-of-stock`)
    return response.data
  }

  static async getCriticalAlerts(): Promise<InventoryAlertsResponse> {
    const response = await axiosClient.get(`${this.BASE_URL}/alerts/critical`)
    return response.data
  }

  // Room and Article Data
  static async getAvailableRooms(): Promise<RoomForInventoryResponse> {
    const response = await axiosClient.get(`${this.BASE_URL}/rooms/available`)
    return response.data
  }

  static async getRoomsWithInventory(): Promise<RoomForInventoryResponse> {
    const response = await axiosClient.get(`${this.BASE_URL}/rooms/with-inventory`)
    return response.data
  }

  static async getArticlesForRoom(habitacionId: number): Promise<ArticleForRoomResponse> {
    const response = await axiosClient.get(`${this.BASE_URL}/rooms/${habitacionId}/articles`)
    return response.data
  }

  static async getAvailableArticles(): Promise<ArticleForRoomResponse> {
    const response = await axiosClient.get(`${this.BASE_URL}/articles/available`)
    return response.data
  }

  // Inventory Movements
  static async getInventoryMovements(
    roomInventoryId: number,
    limit?: number
  ): Promise<ApiResponse<InventoryMovementDto[]>> {
    const params = limit ? { limit } : undefined
    const response = await axiosClient.get(`${this.BASE_URL}/${roomInventoryId}/movements`, { params })
    return response.data
  }

  static async addInventoryMovement(
    roomInventoryId: number,
    tipoMovimiento: 'ENTRADA' | 'SALIDA' | 'AJUSTE' | 'CONSUMO' | 'REPOSICION',
    cantidad: number,
    motivo: string
  ): Promise<ApiResponse<InventoryMovementDto>> {
    const payload = {
      tipoMovimiento,
      cantidad,
      motivo
    }
    const response = await axiosClient.post(`${this.BASE_URL}/${roomInventoryId}/movements`, payload)
    return response.data
  }

  // Inventory Adjustments
  static async adjustInventoryQuantity(
    roomInventoryId: number,
    nuevaCantidad: number,
    motivo: string
  ): Promise<RoomInventoryResponse> {
    const payload = {
      cantidad: nuevaCantidad,
      motivo
    }
    const response = await axiosClient.put(`${this.BASE_URL}/${roomInventoryId}/adjust`, payload)
    return response.data
  }

  static async increaseInventory(
    roomInventoryId: number,
    cantidad: number,
    motivo: string
  ): Promise<RoomInventoryResponse> {
    const payload = { cantidad, motivo }
    const response = await axiosClient.put(`${this.BASE_URL}/${roomInventoryId}/increase`, payload)
    return response.data
  }

  static async decreaseInventory(
    roomInventoryId: number,
    cantidad: number,
    motivo: string
  ): Promise<RoomInventoryResponse> {
    const payload = { cantidad, motivo }
    const response = await axiosClient.put(`${this.BASE_URL}/${roomInventoryId}/decrease`, payload)
    return response.data
  }

  // Inventory Transfers
  static async transferInventory(data: InventoryTransferDto): Promise<ApiResponse<{
    origen: RoomInventoryDto
    destino: RoomInventoryDto
  }>> {
    const response = await axiosClient.post(`${this.BASE_URL}/transfer`, data)
    return response.data
  }

  static async getTransferHistory(habitacionId?: number): Promise<ApiResponse<InventoryMovementDto[]>> {
    const params = habitacionId ? { habitacionId } : undefined
    const response = await axiosClient.get(`${this.BASE_URL}/transfers/history`, { params })
    return response.data
  }

  // Stock Management
  static async replenishStock(
    roomInventoryId: number,
    cantidadReponer: number,
    motivo?: string
  ): Promise<RoomInventoryResponse> {
    const payload = {
      cantidadReponer,
      motivo: motivo || 'Reposición de stock'
    }
    const response = await axiosClient.put(`${this.BASE_URL}/${roomInventoryId}/replenish`, payload)
    return response.data
  }

  static async setStockLimits(
    roomInventoryId: number,
    cantidadMinima: number,
    cantidadMaxima: number
  ): Promise<RoomInventoryResponse> {
    const payload = {
      cantidadMinima,
      cantidadMaxima
    }
    const response = await axiosClient.put(`${this.BASE_URL}/${roomInventoryId}/limits`, payload)
    return response.data
  }

  // Search and Filtering
  static async searchInventories(query: {
    habitacionId?: number
    articuloId?: number
    categoria?: string
    bajoStock?: boolean
    agotados?: boolean
    busqueda?: string
  }): Promise<RoomInventoryListResponse> {
    const response = await axiosClient.get(`${this.BASE_URL}/search`, { params: query })
    return response.data
  }

  static async getInventoriesByCategory(categoria: string): Promise<RoomInventoryListResponse> {
    const response = await axiosClient.get(`${this.BASE_URL}/category/${encodeURIComponent(categoria)}`)
    return response.data
  }

  // Statistics and Analytics
  static async getInventoryStatistics(): Promise<ApiResponse<{
    totalRoomsWithInventory: number
    totalArticlesInInventory: number
    totalValueInventory: number
    lowStockCount: number
    outOfStockCount: number
    averageStockLevel: number
    topCategories: Array<{ categoria: string; count: number; value: number }>
  }>> {
    const response = await axiosClient.get(`${this.BASE_URL}/statistics`)
    return response.data
  }

  static async getRoomInventoryStats(habitacionId: number): Promise<ApiResponse<{
    totalArticulos: number
    valorTotal: number
    articulosBajoStock: number
    articulosAgotados: number
    ultimaActualizacion: string
    categorias: Array<{ categoria: string; count: number; value: number }>
  }>> {
    const response = await axiosClient.get(`${this.BASE_URL}/statistics/room/${habitacionId}`)
    return response.data
  }

  // Validation and Utility Methods
  static validateInventoryLimits(cantidad: number, cantidadMinima: number, cantidadMaxima: number): boolean {
    return cantidad >= 0 && cantidadMinima >= 0 && cantidadMaxima >= cantidadMinima
  }

  static validateTransfer(
    origenCantidad: number,
    cantidadTransferir: number
  ): { valid: boolean; error?: string } {
    if (cantidadTransferir <= 0) {
      return { valid: false, error: 'La cantidad a transferir debe ser mayor a 0' }
    }
    if (cantidadTransferir > origenCantidad) {
      return { valid: false, error: 'No hay suficiente stock para realizar la transferencia' }
    }
    return { valid: true }
  }

  static calculateStockStatus(
    cantidad: number,
    cantidadMinima: number,
    cantidadMaxima: number
  ): 'OK' | 'LOW_STOCK' | 'OUT_OF_STOCK' | 'OVERSTOCKED' {
    if (cantidad === 0) return 'OUT_OF_STOCK'
    if (cantidad <= cantidadMinima) return 'LOW_STOCK'
    if (cantidad > cantidadMaxima) return 'OVERSTOCKED'
    return 'OK'
  }

  // Export/Import functionality
  static async exportRoomInventory(habitacionId: number, format: 'CSV' | 'EXCEL' = 'EXCEL'): Promise<Blob> {
    const response = await axiosClient.get(`${this.BASE_URL}/export/room/${habitacionId}`, {
      params: { format },
      responseType: 'blob'
    })
    return response.data
  }

  static async exportAllInventories(format: 'CSV' | 'EXCEL' = 'EXCEL'): Promise<Blob> {
    const response = await axiosClient.get(`${this.BASE_URL}/export/all`, {
      params: { format },
      responseType: 'blob'
    })
    return response.data
  }

  static async importInventory(file: File, habitacionId?: number): Promise<ApiResponse<{
    imported: number
    errors: Array<{ row: number; error: string }>
  }>> {
    const formData = new FormData()
    formData.append('file', file)
    if (habitacionId) {
      formData.append('habitacionId', habitacionId.toString())
    }

    const response = await axiosClient.post(`${this.BASE_URL}/import`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })
    return response.data
  }
}

// Export singleton pattern for consistency
export const roomInventoryService = RoomInventoryService