import { type ApiResponse } from './common'
import { InventoryLocationType } from './inventory'

// Unified inventory DTO (V1)
export interface InventoryDto {
  inventoryId: number
  articuloId: number
  articuloNombre: string
  articuloDescripcion?: string
  articuloPrecio: number
  articuloImagenUrl?: string
  cantidad: number
  locationType: InventoryLocationType
  locationId?: number
  locationName?: string
  institucionId: number
  fechaRegistro: string
  userId?: string
  userName?: string
  isActive: boolean
}

// Legacy room inventory interface (for backwards compatibility)
export interface RoomInventoryDto extends InventoryDto {
  // Additional room-specific fields
  roomInventoryId: number // maps to inventoryId
  habitacionId: number // maps to locationId
  cantidadMinima: number
  cantidadMaxima: number
  fechaActualizacion: string // maps to fechaRegistro
  activo: boolean // maps to isActive
  // Related data
  habitacion?: {
    numero: string
    nombre: string
    categoriaId: number
    categoria?: {
      nombre: string
      descripcion: string
    }
  }
  articulo?: {
    nombreArticulo: string
    precio: number
    categoria: string
    descripcion: string
    unidadMedida: string
  }
}

// Inventory creation DTO (V1)
export interface InventoryCreateDto {
  articuloId: number
  cantidad: number
  locationType: InventoryLocationType
  locationId?: number // Required if locationType = Room
  notes?: string // Max 200 characters
}

// Legacy room inventory creation (for backwards compatibility)
export interface RoomInventoryCreateDto extends InventoryCreateDto {
  habitacionId: number // maps to locationId
  cantidadMinima: number
  cantidadMaxima: number
}

// Inventory update DTO (V1)
export interface InventoryUpdateDto {
  cantidad: number // >= 0
  notes?: string
}

// Legacy room inventory update (for backwards compatibility)
export interface RoomInventoryUpdateDto extends InventoryUpdateDto {
  cantidadMinima?: number
  cantidadMaxima?: number
  activo?: boolean
}

// Batch update DTO (V1)
export interface InventoryBatchUpdateDto {
  items: Array<{
    inventoryId: number
    cantidad: number
  }>
}

// Legacy bulk update (for backwards compatibility)
export interface RoomInventoryBulkUpdateDto {
  updates: Array<{
    roomInventoryId: number
    cantidad?: number
    cantidadMinima?: number
    cantidadMaxima?: number
  }>
}

// Room inventory summary
export interface RoomInventorySummaryDto {
  habitacionId: number
  habitacionNumero: string
  habitacionNombre: string
  totalArticulos: number
  articulosBajoStock: number
  articulosAgotados: number
  valorTotalInventario: number
  fechaUltimaActualizacion: string
}

// Inventory alerts
export interface InventoryAlertDto {
  roomInventoryId: number
  habitacionId: number
  habitacionNumero: string
  articuloId: number
  articuloNombre: string
  tipoAlerta: 'LOW_STOCK' | 'OUT_OF_STOCK' | 'OVERSTOCKED'
  cantidadActual: number
  cantidadMinima: number
  cantidadMaxima: number
  mensaje: string
  prioridad: 'LOW' | 'MEDIUM' | 'HIGH' | 'CRITICAL'
}

// Room selection for inventory
export interface RoomForInventoryDto {
  habitacionId: number
  numero: string
  nombre: string
  categoriaId: number
  categoriaNombre: string
  estado: string
  tieneInventario: boolean
}

// Article for room inventory
export interface ArticleForRoomDto {
  articuloId: number
  nombreArticulo: string
  precio: number
  categoria: string
  descripcion: string
  unidadMedida: string
  stockGeneral: number
  enInventarioHabitacion: boolean
  cantidadEnHabitacion?: number
}

// Inventory movement tracking
export interface InventoryMovementDto {
  movementId: number
  roomInventoryId: number
  tipoMovimiento: 'ENTRADA' | 'SALIDA' | 'AJUSTE' | 'CONSUMO' | 'REPOSICION'
  cantidadAnterior: number
  cantidadNueva: number
  motivo: string
  usuarioId: number
  usuario: string
  fechaMovimiento: string
}

// Room inventory report
export interface RoomInventoryReportDto {
  habitacionId: number
  habitacionInfo: {
    numero: string
    nombre: string
    categoria: string
  }
  inventario: RoomInventoryDto[]
  resumen: {
    totalArticulos: number
    valorTotal: number
    alertas: InventoryAlertDto[]
  }
  movimientos: InventoryMovementDto[]
}

// Inventory transfer between rooms
export interface InventoryTransferDto {
  articuloId: number
  habitacionOrigenId: number
  habitacionDestinoId: number
  cantidad: number
  motivo: string
}

// API Response types (V1)
export type InventoryResponse = ApiResponse<InventoryDto>
export type InventoryListResponse = ApiResponse<InventoryDto[]>
export type RoomInventoryResponse = ApiResponse<RoomInventoryDto>
export type RoomInventoryListResponse = ApiResponse<RoomInventoryDto[]>
export type RoomInventorySummaryResponse = ApiResponse<RoomInventorySummaryDto[]>
export type InventoryAlertsResponse = ApiResponse<InventoryAlertDto[]>
export type RoomForInventoryResponse = ApiResponse<RoomForInventoryDto[]>
export type ArticleForRoomResponse = ApiResponse<ArticleForRoomDto[]>
export type InventoryReportResponse = ApiResponse<RoomInventoryReportDto>

// Stock validation response
export interface StockValidationItem {
  articuloId: number
  disponible: boolean
  cantidadDisponible: number
  cantidadRequerida: number
  ubicaciones: Array<{
    locationType: InventoryLocationType
    locationId?: number
    cantidad: number
  }>
}

// Summary response
export interface InventorySummaryItem {
  locationType: InventoryLocationType
  locationId?: number
  locationName: string
  totalItems: number
  totalQuantity: number
  totalValue: number
}

// UI State interfaces (enhanced for V1)
export interface RoomInventoryState {
  inventories: InventoryDto[] // Unified inventory items
  roomInventories: RoomInventoryDto[] // Legacy room items
  generalInventory: InventoryDto[] // General inventory items
  combinedInventory: InventoryDto[] // Combined view
  selectedRoom: RoomForInventoryDto | null
  availableRooms: RoomForInventoryDto[]
  availableArticles: ArticleForRoomDto[]
  alerts: InventoryAlertDto[]
  loading: boolean
  saving: boolean
  error: string | null
  viewMode: 'room' | 'general' | 'combined'
}

// Filter and search options
export interface InventoryFilters {
  habitacionId?: number
  categoria?: string
  bajoStock?: boolean
  agotados?: boolean
  alertas?: boolean
  busqueda?: string
}

// Form interfaces (V1)
export interface InventoryFormData {
  articuloId: number | null
  cantidad: number
  locationType: InventoryLocationType
  locationId: number | null
  notes?: string
}

// Legacy form interface
export interface RoomInventoryFormData extends InventoryFormData {
  habitacionId: number | null // maps to locationId
  cantidadMinima: number
  cantidadMaxima: number
}

export interface InventoryAdjustmentForm {
  roomInventoryId: number
  nuevaCantidad: number
  motivo: string
  tipoMovimiento: 'AJUSTE' | 'ENTRADA' | 'SALIDA'
}

// Validation rules
export interface RoomInventoryValidation {
  cantidad: {
    required: boolean
    min: number
    max: number
  }
  cantidadMinima: {
    required: boolean
    min: number
  }
  cantidadMaxima: {
    required: boolean
    min: number
  }
  habitacion: {
    required: boolean
  }
  articulo: {
    required: boolean
  }
}
