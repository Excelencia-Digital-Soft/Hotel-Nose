import axiosClient from '../axiosClient'
import type { ApiResponse, CategoriaDto } from '../types'

// Category DTO interfaces for V1 API
interface CategoryCreateDto {
  nombreCategoria: string
  descripcion?: string
  capacidadMaxima: number
  precioNormal: number
  porcentajeXPersona: number
  institucionId: number
  usuarioId: number
}

interface CategoryUpdateDto {
  nombreCategoria: string
  descripcion?: string
  capacidadMaxima: number
  precioNormal: number
  porcentajeXPersona: number
  usuarioId: number
}

interface CategoryValidationResult {
  isValid: boolean
  errors: string[]
}

interface CategoryDisplayFormat extends CategoriaDto {
  precioFormateado: string
  porcentajeFormateado: string
  capacidadFormateada: string
}

class CategoryService {
  /**
   * Get all categories for an institution
   */
  static async getCategories(): Promise<ApiResponse<CategoriaDto[]>> {
    try {
      const response = await axiosClient.get<ApiResponse<CategoriaDto[]>>(
        '/api/v1/habitacion-categorias'
      )
      return response.data
    } catch (error: any) {
      console.error('Error fetching categories:', error)
      return {
        isSuccess: false,
        message: error.response?.data?.message || 'Error al obtener las categorías',
        data: undefined,
      }
    }
  }

  /**
   * Get category by ID
   */
  static async getCategory(categoryId: number): Promise<ApiResponse<CategoriaDto>> {
    try {
      const response = await axiosClient.get<ApiResponse<CategoriaDto>>(
        `/api/v1/habitacion-categorias/${categoryId}`
      )
      return response.data
    } catch (error: any) {
      console.error('Error fetching category:', error)
      return {
        isSuccess: false,
        message: error.response?.data?.message || 'Error al obtener la categoría',
        data: undefined,
      }
    }
  }

  /**
   * Create category
   */
  static async createCategory(categoryData: CategoryCreateDto): Promise<ApiResponse<CategoriaDto>> {
    try {
      const response = await axiosClient.post<ApiResponse<CategoriaDto>>(
        '/api/v1/habitacion-categorias',
        {
          nombreCategoria: categoryData.nombreCategoria,
          descripcion: categoryData.descripcion,
          precioNormal: categoryData.precioNormal,
          capacidadMaxima: categoryData.capacidadMaxima,
          porcentajeXPersona: categoryData.porcentajeXPersona,
          institucionId: categoryData.institucionId,
          usuarioId: categoryData.usuarioId,
        }
      )
      return response.data
    } catch (error: any) {
      console.error('Error creating category:', error)
      return {
        isSuccess: false,
        message: error.response?.data?.message || 'Error al crear la categoría',
        data: undefined,
      }
    }
  }

  /**
   * Update category
   */
  static async updateCategory(
    categoryId: number,
    categoryData: CategoryUpdateDto
  ): Promise<ApiResponse<CategoriaDto>> {
    try {
      const response = await axiosClient.put<ApiResponse<CategoriaDto>>(
        `/api/v1/habitacion-categorias/${categoryId}`,
        {
          nombreCategoria: categoryData.nombreCategoria,
          descripcion: categoryData.descripcion,
          precioNormal: categoryData.precioNormal,
          capacidadMaxima: categoryData.capacidadMaxima,
          porcentajeXPersona: categoryData.porcentajeXPersona,
          usuarioId: categoryData.usuarioId,
        }
      )
      return response.data
    } catch (error: any) {
      console.error('Error updating category:', error)
      return {
        isSuccess: false,
        message: error.response?.data?.message || 'Error al actualizar la categoría',
        data: undefined,
      }
    }
  }

  /**
   * Delete category
   */
  static async deleteCategory(categoryId: number): Promise<ApiResponse<void>> {
    try {
      const response = await axiosClient.delete<ApiResponse<void>>(
        `/api/v1/habitacion-categorias/${categoryId}`
      )
      return response.data
    } catch (error: any) {
      console.error('Error deleting category:', error)
      return {
        isSuccess: false,
        message: error.response?.data?.message || 'Error al eliminar la categoría',
        data: undefined,
      }
    }
  }

  // Utility methods
  /**
   * Calculate price with percentage for extra persons
   */
  static calculatePriceWithPercentage(
    basePrice: number,
    percentage: number,
    extraPersons: number = 0
  ): number {
    const extraCost = ((basePrice * percentage) / 100) * extraPersons
    return basePrice + extraCost
  }

  /**
   * Validate category data
   */
  static validateCategoryData(categoryData: Partial<CategoryCreateDto>): CategoryValidationResult {
    const errors: string[] = []

    if (!categoryData.nombreCategoria || categoryData.nombreCategoria.trim() === '') {
      errors.push('El nombre de la categoría es requerido')
    }

    if (
      categoryData.precioNormal === undefined ||
      categoryData.precioNormal === null ||
      categoryData.precioNormal < 0
    ) {
      errors.push('El precio debe ser un número válido mayor o igual a 0')
    }

    if (
      categoryData.capacidadMaxima === undefined ||
      categoryData.capacidadMaxima === null ||
      categoryData.capacidadMaxima <= 0
    ) {
      errors.push('La capacidad máxima debe ser un número entero mayor a 0')
    }

    if (
      categoryData.porcentajeXPersona === undefined ||
      categoryData.porcentajeXPersona === null ||
      categoryData.porcentajeXPersona < 0 ||
      categoryData.porcentajeXPersona > 100
    ) {
      errors.push('El porcentaje por persona debe ser un número entre 0 y 100')
    }

    return {
      isValid: errors.length === 0,
      errors,
    }
  }

  /**
   * Format category for display with localized currency and text
   */
  static formatCategoryForDisplay(category: CategoriaDto): CategoryDisplayFormat {
    return {
      ...category,
      precioFormateado: new Intl.NumberFormat('es-CO', {
        style: 'currency',
        currency: 'COP',
        minimumFractionDigits: 0,
      }).format(category.precioNormal),
      porcentajeFormateado: `${category.porcentajeXPersona}%`,
      capacidadFormateada: `${category.capacidadMaxima} persona${category.capacidadMaxima !== 1 ? 's' : ''}`,
    }
  }

  /**
   * Health check endpoint
   */
  static async healthCheck(): Promise<ApiResponse<{ status: string; timestamp: string }>> {
    try {
      const response = await axiosClient.get<ApiResponse<{ status: string; timestamp: string }>>(
        '/api/v1/habitacion-categorias/health'
      )
      return response.data
    } catch (error: any) {
      console.error('Error in category service health check:', error)
      return {
        isSuccess: false,
        message: error.response?.data?.message || 'Error en el servicio de categorías',
        data: undefined,
      }
    }
  }
}

export default CategoryService

// Export types for external use
export type {
  CategoryCreateDto,
  CategoryUpdateDto,
  CategoryValidationResult,
  CategoryDisplayFormat,
}

