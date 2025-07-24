import { type ApiResponse } from './common'

/**
 * Room Inventory TypeScript interfaces
 * For managing inventory per room
 */

// Base room inventory interface
export interface RoomInventoryDto {
  roomInventoryId: number
  habitacionId: number
  articuloId: number
  cantidad: number
  cantidadMinima: number
  cantidadMaxima: number
  fechaActualizacion: string
  activo: boolean
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

// Room inventory creation interface
export interface RoomInventoryCreateDto {
  habitacionId: number
  articuloId: number
  cantidad: number
  cantidadMinima: number
  cantidadMaxima: number
}

// Room inventory update interface
export interface RoomInventoryUpdateDto {
  cantidad?: number
  cantidadMinima?: number
  cantidadMaxima?: number
  activo?: boolean
}

// Bulk update interface
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

// API Response types
export type RoomInventoryResponse = ApiResponse<RoomInventoryDto>
export type RoomInventoryListResponse = ApiResponse<RoomInventoryDto[]>
export type RoomInventorySummaryResponse = ApiResponse<RoomInventorySummaryDto[]>
export type InventoryAlertsResponse = ApiResponse<InventoryAlertDto[]>
export type RoomForInventoryResponse = ApiResponse<RoomForInventoryDto[]>
export type ArticleForRoomResponse = ApiResponse<ArticleForRoomDto[]>
export type InventoryReportResponse = ApiResponse<RoomInventoryReportDto>

// UI State interfaces
export interface RoomInventoryState {
  inventories: RoomInventoryDto[]
  selectedRoom: RoomForInventoryDto | null
  availableRooms: RoomForInventoryDto[]
  availableArticles: ArticleForRoomDto[]
  alerts: InventoryAlertDto[]
  loading: boolean
  saving: boolean
  error: string | null
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

// Form interfaces
export interface RoomInventoryFormData {
  habitacionId: number | null
  articuloId: number | null
  cantidad: number
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

