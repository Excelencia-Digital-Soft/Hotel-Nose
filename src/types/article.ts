// Article interfaces based on V1 API DTOs

// Main article DTO
export interface ArticleDto {
  articuloId: number;
  nombreArticulo: string;
  precio: number;
  categoriaId: number;
  imagenUrl?: string;
  fechaCreacion: string;
  fechaActualizacion?: string;
  anulado: boolean;
  institucionId: number;
  usuarioCreacionId?: string;
  usuarioActualizacionId?: string;
}

// Create article DTO
export interface ArticleCreateDto {
  nombreArticulo: string;
  precio: number;
  categoriaId: number;
}

// Create article with image DTO
export interface ArticleCreateWithImageDto {
  nombreArticulo: string;
  precio: number;
  categoriaId: number;
  imagen: File;
}

// Update article DTO
export interface ArticleUpdateDto {
  nombreArticulo?: string;
  precio?: number;
  categoriaId?: number;
}

// Article status update DTO
export interface ArticleStatusDto {
  anulado: boolean;
}

// Article search parameters
export interface ArticleSearchParams {
  q?: string;
  categoriaId?: number;
  anulado?: boolean;
  fechaDesde?: string;
  fechaHasta?: string;
  limite?: number;
  pagina?: number;
}

// Article statistics DTO
export interface ArticleStatisticsDto {
  totalArticulos: number;
  articulosActivos: number;
  articulosInactivos: number;
  valorTotal: number;
  valorPromedio: number;
  categorias: {
    categoriaId: number;
    nombreCategoria: string;
    cantidadArticulos: number;
    valorTotal: number;
  }[];
  ultimaActualizacion: string;
}

// Form data interface for frontend use
export interface ArticleFormData {
  articuloId?: number;
  name: string;
  price: string;
  categoryId: number | null;
  image: File | null;
}

// Frontend article display interface
export interface ArticleDisplay {
  articuloId: number;
  nombreArticulo: string;
  precio: number;
  categoriaID?: number; // Legacy support
  categoriaId?: number;
  categoria?: string;
  imagenUrl?: string;
  imagen?: string; // Legacy support
  imagenAPI?: string; // Legacy support
  fechaCreacion?: string;
  anulado?: boolean;
}