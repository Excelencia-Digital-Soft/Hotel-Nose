// Re-export all types for easier imports
export * from './api';
export * from './composables';

// Explicit re-exports for better IDE support and to avoid module resolution issues
export type { ApiResponse } from './common';
export type { 
  InventoryDto, 
  InventoryItem
} from './inventory';
export { 
  InventoryLocationType,
  InventoryMovementType 
} from './inventory';
export type { CategoriaDto } from './categoria';
export type { 
  PromocionDto, 
  PromocionCreateDto, 
  PromocionUpdateDto, 
  PromocionValidateDto,
  PromocionValidationResult 
} from './promocion';
export type { 
  RoomReservation, 
  Habitacion 
} from './habitacion';
export type { 
  ConsumoResponseDto, 
  ConsumoCreateDto, 
  ConsumoUpdateDto 
} from './consumo';