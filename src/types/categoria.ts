// Category interface
export interface Category {
  id: number;
  nombre: string;
  descripcion?: string;
  institucionId: number;
}

// CategoriaDto for API responses
export interface CategoriaDto {
  categoriaId: number;
  nombreCategoria: string;
  descripcion?: string;
  capacidadMaxima: number;
  precioNormal: number;
  porcentajeXPersona: number;
  institucionId: number;
  isActive: boolean;
}