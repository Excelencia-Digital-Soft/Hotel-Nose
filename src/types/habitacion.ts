// Habitacion/Room interfaces
export interface Habitacion {
  HabitacionID: number;
  ReservaID?: number;
  VisitaID?: number;
  NombreHabitacion?: string;
  nombreHabitacion?: string;
  Identificador?: string;
  categoriaId?: number;
  estado?: string;
  disponible?: boolean;
  pedidosPendientes?: boolean;
}

// Extended room interface for ReserveRoom component
export interface RoomReservation extends Habitacion {
  TotalHoras: number;
  TotalMinutos: number;
  Precio: number;
  Patente?: string;
  CompaniaString?: string;
  Pausa: number;
  NombrePromocion?: string;
  PromocionId?: number;
}

// Room availability DTO
export interface HabitacionAvailabilityDto {
  disponible: boolean;
}

// Room service interfaces
export interface RoomCreateData {
  institucionId: number;
  nombreHabitacion: string;
  categoriaId: number;
  imagenes?: File[];
}

export interface RoomUpdateData {
  nombreHabitacion: string;
  categoriaId: number;
  usuarioId: number;
  nuevasImagenes?: File[];
  removedImageIds?: number[];
}

// Room image interface
export interface RoomImage {
  id: number;
  url: string;
  habitacionId: number;
  orden?: number;
}

// Room characteristic interface
export interface RoomCharacteristic {
  id: number;
  nombre: string;
  valor: string;
  habitacionId: number;
}