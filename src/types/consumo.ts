// Consumo interfaces
export interface Consumo {
  id: number;
  visitaId: number;
  habitacionId: number;
  articulo: string;
  cantidad: number;
  precio: number;
  total: number;
  fecha: string;
  estado: string;
}

// V1 Consumo DTOs
export interface ConsumoCreateDto {
  articuloId: number;
  cantidad: number;
  precioUnitario: number;
  esHabitacion: boolean;
}

export interface ConsumoUpdateDto {
  cantidad: number;
}

export interface ConsumoResponseDto {
  consumoId: number;
  visitaId: number;
  habitacionId: number;
  articuloId: number;
  articuloNombre: string;
  cantidad: number;
  precio: number;
  total: number;
  fecha: string;
  estado: string;
  usuarioId?: string;
  usuarioNombre?: string;
}

export interface ConsumoSummaryDto {
  visitaId: number;
  habitacionId: number;
  totalItems: number;
  totalAmount: number;
  consumos: ConsumoResponseDto[];
}