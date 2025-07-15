import axiosClient from '../axiosClient'

export class CategoryService {
  // Get all categories for an institution
  static async getCategories(institucionId) {
    try {
      const response = await axiosClient.get(`/api/v1/categories?institucionId=${institucionId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching categories:', error)
      throw error
    }
  }

  // Get category by ID
  static async getCategory(categoryId) {
    try {
      const response = await axiosClient.get(`/api/v1/categories/${categoryId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching category:', error)
      throw error
    }
  }

  // Create category
  static async createCategory(categoryData) {
    try {
      const response = await axiosClient.post('/api/v1/categories', {
        nombreCategoria: categoryData.nombreCategoria,
        precioNormal: categoryData.precioNormal,
        capacidadMaxima: categoryData.capacidadMaxima,
        porcentajeXPersona: categoryData.porcentajeXPersona,
        institucionId: categoryData.institucionId,
        usuarioId: categoryData.usuarioId
      })
      return response.data
    } catch (error) {
      console.error('Error creating category:', error)
      throw error
    }
  }

  // Update category
  static async updateCategory(categoryId, categoryData) {
    try {
      const response = await axiosClient.put(`/api/v1/categories/${categoryId}`, {
        nombreCategoria: categoryData.nombreCategoria,
        precioNormal: categoryData.precioNormal,
        capacidadMaxima: categoryData.capacidadMaxima,
        porcentajeXPersona: categoryData.porcentajeXPersona,
        usuarioId: categoryData.usuarioId
      })
      return response.data
    } catch (error) {
      console.error('Error updating category:', error)
      throw error
    }
  }

  // Delete category
  static async deleteCategory(categoryId) {
    try {
      const response = await axiosClient.delete(`/api/v1/categories/${categoryId}`)
      return response.data
    } catch (error) {
      console.error('Error deleting category:', error)
      throw error
    }
  }

  // Get category pricing
  static async getCategoryPricing(categoryId, personas = 1) {
    try {
      const response = await axiosClient.get(`/api/v1/categories/${categoryId}/pricing?personas=${personas}`)
      return response.data
    } catch (error) {
      console.error('Error fetching category pricing:', error)
      throw error
    }
  }

  // Get categories with room count
  static async getCategoriesWithRoomCount(institucionId) {
    try {
      const response = await axiosClient.get(`/api/v1/categories/stats?institucionId=${institucionId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching categories with room count:', error)
      throw error
    }
  }

  // Legacy methods for backward compatibility
  static async legacyGetCategories(institucionId) {
    try {
      const response = await axiosClient.get(`/api/Objetos/GetCategorias?InstitucionID=${institucionId}`)
      return response.data
    } catch (error) {
      console.error('Error fetching legacy categories:', error)
      throw error
    }
  }

  static async legacyCreateCategory(categoryData) {
    try {
      const response = await axiosClient.post(
        `/api/Objetos/CrearCategoria?nombreCategoria=${encodeURIComponent(categoryData.nombreCategoria)}&UsuarioID=${categoryData.usuarioId}&InstitucionID=${categoryData.institucionId}&precio=${categoryData.precioNormal}&capacidadmaxima=${categoryData.capacidadMaxima}&Porcentaje=${categoryData.porcentajeXPersona}`
      )
      return response.data
    } catch (error) {
      console.error('Error creating legacy category:', error)
      throw error
    }
  }

  static async legacyUpdateCategory(categoryId, categoryData) {
    try {
      const response = await axiosClient.put(
        `/api/Objetos/ActualizarCategoria?id=${categoryId}&UsuarioID=${categoryData.usuarioId}&nuevoNombre=${encodeURIComponent(categoryData.nombreCategoria)}&nuevaCapacidad=${categoryData.capacidadMaxima}&Precio=${categoryData.precioNormal}&Porcentaje=${categoryData.porcentajeXPersona}`
      )
      return response.data
    } catch (error) {
      console.error('Error updating legacy category:', error)
      throw error
    }
  }

  static async legacyDeleteCategory(categoryId) {
    try {
      const response = await axiosClient.delete(`/api/Objetos/AnularCategoria?id=${categoryId}&Estado=true`)
      return response.data
    } catch (error) {
      console.error('Error deleting legacy category:', error)
      throw error
    }
  }

  // Utility methods
  static calculatePriceWithPercentage(basePrice, percentage, extraPersons = 0) {
    const extraCost = (basePrice * percentage / 100) * extraPersons
    return basePrice + extraCost
  }

  static validateCategoryData(categoryData) {
    const errors = []

    if (!categoryData.nombreCategoria || categoryData.nombreCategoria.trim() === '') {
      errors.push('El nombre de la categoría es requerido')
    }

    if (categoryData.precioNormal === undefined || categoryData.precioNormal === null || categoryData.precioNormal < 0) {
      errors.push('El precio debe ser un número válido mayor o igual a 0')
    }

    if (categoryData.capacidadMaxima === undefined || categoryData.capacidadMaxima === null || categoryData.capacidadMaxima <= 0) {
      errors.push('La capacidad máxima debe ser un número entero mayor a 0')
    }

    if (categoryData.porcentajeXPersona === undefined || categoryData.porcentajeXPersona === null || 
        categoryData.porcentajeXPersona < 0 || categoryData.porcentajeXPersona > 100) {
      errors.push('El porcentaje por persona debe ser un número entre 0 y 100')
    }

    return {
      isValid: errors.length === 0,
      errors
    }
  }

  static formatCategoryForDisplay(category) {
    return {
      ...category,
      precioFormateado: new Intl.NumberFormat('es-CO', {
        style: 'currency',
        currency: 'COP',
        minimumFractionDigits: 0
      }).format(category.precioNormal),
      porcentajeFormateado: `${category.porcentajeXPersona}%`,
      capacidadFormateada: `${category.capacidadMaxima} persona${category.capacidadMaxima !== 1 ? 's' : ''}`
    }
  }
}