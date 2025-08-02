import axiosClient from '../axiosClient'
import type {
  InventoryDto,
  InventoryCreateDto,
  InventoryUpdateDto,
  InventoryBatchUpdateDto,
  InventoryMovementDto,
  InventoryAlertDto,
  StockValidationDto,
  InventorySummaryDto,
  ApiResponse,
} from '../types'
import { InventoryLocationType } from '../types'

/**
 * Inventory Service V1 - Complete Implementation
 * Based on: Guía Completa: Administración de Inventario por Habitación - API V1
 * 
 * Endpoints:
 * - General inventory management
 * - Room-specific inventory
 * - Transfers and movements
 * - Alerts and notifications
 * - Batch operations
 * - Validation and reports
 */
export class InventoryService {
  private static readonly BASE_URL = '/api/v1/inventory'

  // 🏠 1. General Inventory Management

  /**
   * Get all inventory with optional filters
   * GET /api/v1/inventory
   */
  static async getAllInventory(params?: {
    locationType?: InventoryLocationType
    locationId?: number
  }): Promise<ApiResponse<InventoryDto[]>> {
    const response = await axiosClient.get(this.BASE_URL, { params })
    return response.data
  }

  /**
   * Get inventory for a specific room
   * GET /api/v1/inventory/rooms/{roomId}
   */
  static async getRoomInventory(roomId: number): Promise<ApiResponse<InventoryDto[]>> {
    const response = await axiosClient.get(`${this.BASE_URL}/rooms/${roomId}`)
    return response.data
  }

  /**
   * Get combined view (room + general available)
   * GET /api/v1/inventory/rooms/{roomId}/combined
   */
  static async getCombinedInventory(roomId: number): Promise<ApiResponse<InventoryDto[]>> {
    const response = await axiosClient.get(`${this.BASE_URL}/rooms/${roomId}/combined`)
    return response.data
  }

  /**
   * Get general inventory of the hotel
   * GET /api/v1/inventory/general
   */
  static async getGeneralInventory(): Promise<ApiResponse<InventoryDto[]>> {
    const response = await axiosClient.get(`${this.BASE_URL}/general`)
    return response.data
  }

  // 📦 2. CRUD Operations

  /**
   * Create new inventory item
   * POST /api/v1/inventory
   */
  static async createInventory(data: InventoryCreateDto): Promise<ApiResponse<InventoryDto>> {
    const response = await axiosClient.post(this.BASE_URL, data)
    return response.data
  }

  /**
   * Update inventory quantity
   * PUT /api/v1/inventory/{id}
   */
  static async updateInventory(id: number, data: InventoryUpdateDto): Promise<ApiResponse<InventoryDto>> {
    const response = await axiosClient.put(`${this.BASE_URL}/${id}`, data)
    return response.data
  }

  /**
   * Batch update inventory items
   * PUT /api/v1/inventory/batch
   */
  static async batchUpdateInventory(data: InventoryBatchUpdateDto): Promise<ApiResponse<InventoryDto[]>> {
    const response = await axiosClient.put(`${this.BASE_URL}/batch`, data)
    return response.data
  }

  // 🔄 3. Movements and Traceability

  /**
   * Register inventory movement
   * POST /api/v1/inventory/{id}/movements
   */
  static async registerMovement(inventoryId: number, data: {
    tipoMovimiento: string
    cantidadCambiada: number
    motivo: string
    numeroDocumento?: string
    tipoUbicacionOrigen?: InventoryLocationType
    tipoUbicacionDestino?: InventoryLocationType
    ubicacionIdDestino?: number
  }): Promise<ApiResponse<InventoryMovementDto>> {
    const response = await axiosClient.post(`${this.BASE_URL}/${inventoryId}/movements`, data)
    return response.data
  }

  /**
   * Get movement history for an item
   * GET /api/v1/inventory/{id}/movements
   */
  static async getMovementHistory(inventoryId: number): Promise<ApiResponse<InventoryMovementDto[]>> {
    const response = await axiosClient.get(`${this.BASE_URL}/${inventoryId}/movements`)
    return response.data
  }

  /**
   * Get complete audit of movements
   * GET /api/v1/inventory/movements/audit
   */
  static async getMovementAudit(params?: {
    fechaInicio?: string
    fechaFin?: string
    tipoMovimiento?: string
    articuloId?: number
    usuarioId?: string
    pagina?: number
    tamañoPagina?: number
  }): Promise<ApiResponse<InventoryMovementDto[]>> {
    const response = await axiosClient.get(`${this.BASE_URL}/movements/audit`, { params })
    return response.data
  }

  // 🚨 4. Alert System

  /**
   * Get active alerts
   * GET /api/v1/inventory/alerts/active
   */
  static async getActiveAlerts(params?: {
    tipoAlerta?: 'StockBajo' | 'StockCritico' | 'StockAgotado'
    severidad?: 'Baja' | 'Media' | 'Alta' | 'Critica'
    soloNoReconocidas?: boolean
    tipoUbicacion?: InventoryLocationType
  }): Promise<ApiResponse<InventoryAlertDto[]>> {
    const response = await axiosClient.get(`${this.BASE_URL}/alerts/active`, { params })
    return response.data
  }

  /**
   * Configure alerts for inventory item
   * POST /api/v1/inventory/alerts/configure
   */
  static async configureAlerts(data: {
    inventarioId: number
    umbralStockBajo: number
    umbralStockCritico: number
    alertasStockBajoActivas: boolean
    notificacionEmailActiva: boolean
  }): Promise<ApiResponse<any>> {
    const response = await axiosClient.post(`${this.BASE_URL}/alerts/configure`, data)
    return response.data
  }

  /**
   * Acknowledge alert
   * PUT /api/v1/inventory/alerts/{alertId}/acknowledge
   */
  static async acknowledgeAlert(alertId: number, data: {
    comentarios?: string
    resolverAlerta?: boolean
  }): Promise<ApiResponse<any>> {
    const response = await axiosClient.put(`${this.BASE_URL}/alerts/${alertId}/acknowledge`, data)
    return response.data
  }

  // ⚡ 5. Transfers (Recommended for Reloads)

  /**
   * Simple transfer between locations
   * POST /api/v1/inventory/transfer
   */
  static async createTransfer(data: {
    tipoUbicacionOrigen: InventoryLocationType
    tipoUbicacionDestino: InventoryLocationType
    ubicacionIdDestino?: number
    prioridad?: 'Baja' | 'Media' | 'Alta'
    motivo: string
    requireAprobacion?: boolean
    detalles: Array<{
      inventarioId: number
      cantidadSolicitada: number
    }>
  }): Promise<ApiResponse<any>> {
    const response = await axiosClient.post(`${this.BASE_URL}/transfer`, data)
    return response.data
  }

  /**
   * Batch transfers (Multiple rooms)
   * POST /api/v1/inventory/transfer/batch
   */
  static async createBatchTransfer(data: {
    procesamientoAtomico?: boolean
    transferencias: Array<{
      tipoUbicacionOrigen: InventoryLocationType
      tipoUbicacionDestino: InventoryLocationType
      ubicacionIdDestino?: number
      motivo: string
      detalles: Array<{
        inventarioId: number
        cantidadSolicitada: number
      }>
    }>
  }): Promise<ApiResponse<any>> {
    const response = await axiosClient.post(`${this.BASE_URL}/transfer/batch`, data)
    return response.data
  }

  /**
   * Get pending transfers
   * GET /api/v1/inventory/transfer/pending
   */
  static async getPendingTransfers(): Promise<ApiResponse<any[]>> {
    const response = await axiosClient.get(`${this.BASE_URL}/transfer/pending`)
    return response.data
  }

  /**
   * Approve transfer
   * PUT /api/v1/inventory/transfer/{id}/approve
   */
  static async approveTransfer(transferId: number, data: {
    comentarios?: string
  }): Promise<ApiResponse<any>> {
    const response = await axiosClient.put(`${this.BASE_URL}/transfer/${transferId}/approve`, data)
    return response.data
  }

  // 📊 Additional Utility Methods

  /**
   * Validate stock availability
   * GET /api/v1/inventory/validate-stock
   */
  static async validateStock(params: {
    articuloId: number
    cantidad: number
    locationType?: InventoryLocationType
  }): Promise<ApiResponse<StockValidationDto>> {
    const response = await axiosClient.get(`${this.BASE_URL}/validate-stock`, { params })
    return response.data
  }

  /**
   * Get inventory summary
   * GET /api/v1/inventory/summary
   */
  static async getInventorySummary(): Promise<ApiResponse<InventorySummaryDto[]>> {
    const response = await axiosClient.get(`${this.BASE_URL}/summary`)
    return response.data
  }

  // 🎯 Utility Functions for Frontend

  /**
   * Reload room stock - Common use case implementation
   */
  static async reloadRoomStock(roomId: number, articuloId: number, cantidad: number): Promise<ApiResponse<any>> {
    // 1. Validate stock availability
    const validation = await this.validateStock({
      articuloId,
      cantidad,
      locationType: InventoryLocationType.General
    })

    if (!validation.data?.isValid) {
      throw new Error(`Stock insuficiente. Disponible: ${validation.data?.availableQuantity || 0}`)
    }

    // 2. Create transfer
    return await this.createTransfer({
      tipoUbicacionOrigen: InventoryLocationType.General,
      tipoUbicacionDestino: InventoryLocationType.Room,
      ubicacionIdDestino: roomId,
      prioridad: 'Media',
      motivo: `Recarga habitación ${roomId}`,
      requireAprobacion: false,
      detalles: [{
        inventarioId: validation.data.inventoryId,
        cantidadSolicitada: cantidad
      }]
    })
  }

  /**
   * Quick stock check for room
   */
  static async checkRoomStock(roomId: number): Promise<{
    items: InventoryDto[]
    alerts: InventoryAlertDto[]
    needsReload: boolean
  }> {
    const [stockResponse, alertsResponse] = await Promise.all([
      this.getRoomInventory(roomId),
      this.getActiveAlerts({ tipoUbicacion: InventoryLocationType.Room })
    ])

    const roomAlerts = (alertsResponse.data || []).filter(
      (alert: any) => alert.ubicacionId === roomId
    )

    return {
      items: stockResponse.data || [],
      alerts: roomAlerts,
      needsReload: roomAlerts.length > 0
    }
  }

  // Static validation helpers
  static validateInventoryLimits(
    cantidad: number,
    cantidadMinima: number,
    cantidadMaxima: number
  ): boolean {
    return cantidad >= 0 && cantidadMinima >= 0 && cantidadMaxima >= cantidadMinima
  }

  static calculateStockStatus(
    cantidad: number,
    cantidadMinima: number = 5
  ): 'OK' | 'LOW_STOCK' | 'OUT_OF_STOCK' {
    if (cantidad === 0) return 'OUT_OF_STOCK'
    if (cantidad <= cantidadMinima) return 'LOW_STOCK'
    return 'OK'
  }
}

// Export both class and singleton for compatibility
export const inventoryService = InventoryService
export { InventoryService as RoomInventoryService } // Backward compatibility
