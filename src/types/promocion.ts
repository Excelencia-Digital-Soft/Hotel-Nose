// Promocion interfaces based on V1 API DTOs

// Main promocion DTO
export interface PromocionDto {
  promocionId: number;
  nombrePromocion: string;
  descripcion?: string;
  tarifa: number;
  duracionHoras: number;
  duracionMinutos: number;
  categoriaId: number;
  categoriaNombre?: string;
  isActive: boolean;
  institucionId: number;
  fechaInicio?: string;
  fechaFin?: string;
  createdAt?: string;
  updatedAt?: string;
}

// Create promocion DTO
export interface PromocionCreateDto {
  nombrePromocion: string;
  descripcion?: string;
  tarifa: number;
  duracionHoras: number;
  duracionMinutos: number;
  categoriaId: number;
  isActive: boolean;
  fechaInicio?: string;
  fechaFin?: string;
}

// Update promocion DTO
export interface PromocionUpdateDto {
  nombrePromocion?: string;
  descripcion?: string;
  tarifa?: number;
  duracionHoras?: number;
  duracionMinutos?: number;
  categoriaId?: number;
  isActive?: boolean;
  fechaInicio?: string;
  fechaFin?: string;
}

// Validation DTO
export interface PromocionValidateDto {
  promocionId: number;
  habitacionId: number;
  fechaReserva: string;
}

// Validation response
export interface PromocionValidationResult {
  isValid: boolean;
  message?: string;
  errorCode?: string;
}