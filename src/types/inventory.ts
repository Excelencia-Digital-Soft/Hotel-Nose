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

// Movement history DTO
export interface InventoryMovementDto {
  movementId: number;
  articuloId: number;
  articuloNombre: string;
  movementType: InventoryMovementType;
  cantidad: number;
  fromLocationId?: number;
  fromLocationName?: string;
  toLocationId?: number;
  toLocationName?: string;
  notes?: string;
  fechaMovimiento: string;
  userId?: string;
  userName?: string;
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

// Stock validation DTO
export interface StockValidationDto {
  isValid: boolean;
  errorMessage?: string;
  articuloId: number;
  requestedQuantity: number;
  availableQuantity: number;
  locationType: InventoryLocationType;
  locationId?: number;
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