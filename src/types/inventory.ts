// Inventory interfaces based on V1 API DTOs

// Enums
export enum InventoryLocationType {
  General = 0,    // Institution-wide general inventory
  Room = 1,       // Room-specific inventory
  Warehouse = 2   // Warehouse inventory (future use)
}

export enum InventoryMovementType {
  Addition = 0,     // Adding inventory
  Removal = 1,      // Removing inventory
  Transfer = 2,     // Transfer between locations
  Consumption = 3,  // Consumption/usage
  Adjustment = 4    // Manual adjustment
}

// Main inventory DTO
export interface InventoryDto {
  inventoryId: number;
  articuloId: number;
  articuloNombre: string;
  articuloDescripcion?: string;
  articuloPrecio: number;
  cantidad: number;
  locationType: InventoryLocationType;
  locationId?: number;
  locationName?: string;
  institucionId: number;
  fechaRegistro: string;
  userId?: string;
  userName?: string;
  isActive: boolean;
}

// Create inventory DTO
export interface InventoryCreateDto {
  articuloId: number;
  cantidad: number;
  locationType: InventoryLocationType;
  locationId?: number;
  notes?: string;
}

// Update inventory DTO
export interface InventoryUpdateDto {
  cantidad: number;
  notes?: string;
}

// Batch update DTOs
export interface InventoryBatchItem {
  inventoryId: number;
  cantidad: number;
}

export interface InventoryBatchUpdateDto {
  items: InventoryBatchItem[];
}

// Transfer DTO
export interface InventoryTransferDto {
  articuloId: number;
  cantidad: number;
  fromLocationType: InventoryLocationType;
  fromLocationId?: number;
  toLocationType: InventoryLocationType;
  toLocationId?: number;
  notes?: string;
}

// Movement history DTO (V1)
export interface InventoryMovementDto {
  movementId: number;
  inventoryId: number;
  articuloId: number;
  articuloNombre: string;
  tipoMovimiento: string;
  cantidadCambiada: number;
  cantidadAnterior: number;
  cantidadNueva: number;
  motivo: string;
  numeroDocumento?: string;
  tipoUbicacionOrigen?: InventoryLocationType;
  tipoUbicacionDestino?: InventoryLocationType;
  ubicacionIdOrigen?: number;
  ubicacionIdDestino?: number;
  fechaMovimiento: string;
  usuarioId?: string;
  usuarioNombre?: string;
  institucionId: number;
}

// Summary DTOs
export interface InventoryItemSummaryDto {
  articuloId: number;
  articuloNombre: string;
  cantidad: number;
  precioUnitario: number;
  valorTotal: number;
  ultimaActualizacion: string;
}

export interface InventorySummaryDto {
  locationType: InventoryLocationType;
  locationId?: number;
  locationName?: string;
  totalItems: number;
  uniqueArticles: number;
  totalValue: number;
  items: InventoryItemSummaryDto[];
}

// Stock validation DTO (V1)
export interface StockValidationDto {
  isValid: boolean;
  errorMessage?: string;
  articuloId: number;
  inventoryId: number;
  requestedQuantity: number;
  availableQuantity: number;
  locationType: InventoryLocationType;
  locationId?: number;
}

// Alert DTOs (V1)
export interface InventoryAlertDto {
  alertId: number;
  inventoryId: number;
  articuloId: number;
  articuloNombre: string;
  tipoAlerta: 'StockBajo' | 'StockCritico' | 'StockAgotado';
  severidad: 'Baja' | 'Media' | 'Alta' | 'Critica';
  cantidadActual: number;
  umbralStockBajo?: number;
  umbralStockCritico?: number;
  locationType: InventoryLocationType;
  locationId?: number;
  locationName?: string;
  mensaje: string;
  fechaCreacion: string;
  reconocida: boolean;
  fechaReconocimiento?: string;
  comentarios?: string;
  usuarioId?: string;
  usuarioNombre?: string;
  institucionId: number;
}

// Filter interfaces for component usage
export interface InventoryFilter {
  locationType?: InventoryLocationType;
  locationId?: number;
  articuloId?: number;
  hasStock?: boolean;
  isActive?: boolean;
}

// Component-friendly inventory item (for UI usage)
export interface InventoryItem {
  articuloId: number;
  nombreArticulo: string;
  precio: number;
  cantidad: number;
  maximo?: number;
  categoriaID?: number;
  descripcion?: string;
  imageUrl?: string;
}