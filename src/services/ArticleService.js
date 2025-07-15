/**
 * Article Service - API calls for article management
 * Handles CRUD operations for articles with V1 API support
 */

import axiosClient from '../axiosClient'

// Feature flag for V1 API (can be set to true to use new endpoints)
const USE_V1_API = true

export class ArticleService {
  /**
   * Get all articles
   * @param {number} institucionID - Institution ID (optional for V1 - uses JWT context)
   * @param {number} categoriaID - Optional category filter
   * @returns {Promise} API response
   */
  static async getArticles(institucionID = null, categoriaID = null) {
    const endpoint = USE_V1_API ? '/api/v1/articulos' : '/api/Articulos/GetArticulos'
    
    if (USE_V1_API) {
      const params = new URLSearchParams()
      if (categoriaID) params.append('categoriaId', categoriaID)
      const queryString = params.toString()
      return axiosClient.get(queryString ? `${endpoint}?${queryString}` : endpoint)
    } else {
      const url = categoriaID 
        ? `${endpoint}?InstitucionID=${institucionID}&categoriaID=${categoriaID}`
        : `${endpoint}?InstitucionID=${institucionID}`
      return axiosClient.get(url)
    }
  }

  /**
   * Get article by ID
   * @param {number} articuloId - Article ID
   * @returns {Promise} API response
   */
  static async getArticleById(articuloId) {
    const endpoint = USE_V1_API ? `/api/v1/articulos/${articuloId}` : `/api/Articulos/GetArticulo/${articuloId}`
    return axiosClient.get(endpoint)
  }

  /**
   * Create a new article (without image)
   * @param {Object} articleData - Article data
   * @param {number} institucionID - Institution ID (ignored in V1 - uses JWT context)
   * @returns {Promise} API response
   */
  static async createArticle(articleData, institucionID = null) {
    if (USE_V1_API) {
      const endpoint = '/api/v1/articulos'
      const payload = {
        nombreArticulo: articleData.nombre || articleData.nombreArticulo,
        precio: articleData.precio,
        categoriaId: articleData.categoriaID || articleData.categoriaId
      }
      return axiosClient.post(endpoint, payload)
    } else {
      const endpoint = '/api/Articulos/CreateArticuloWithImage'
      const formData = new FormData()
      formData.append('nombre', articleData.nombre)
      formData.append('precio', articleData.precio)
      formData.append('categoriaID', articleData.categoriaID)
      formData.append('imagen', articleData.imagen)
      return axiosClient.post(`${endpoint}?InstitucionID=${institucionID}`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      })
    }
  }

  /**
   * Create a new article with image
   * @param {Object} articleData - Article data
   * @param {number} institucionID - Institution ID (ignored in V1 - uses JWT context)
   * @returns {Promise} API response
   */
  static async createArticleWithImage(articleData, institucionID = null) {
    if (USE_V1_API) {
      const endpoint = '/api/v1/articulos/with-image'
      const formData = new FormData()
      formData.append('nombreArticulo', articleData.nombre || articleData.nombreArticulo)
      formData.append('precio', articleData.precio)
      formData.append('categoriaId', articleData.categoriaID || articleData.categoriaId)
      formData.append('imagen', articleData.imagen)
      return axiosClient.post(endpoint, formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      })
    } else {
      const endpoint = '/api/Articulos/CreateArticuloWithImage'
      const formData = new FormData()
      formData.append('nombre', articleData.nombre)
      formData.append('precio', articleData.precio)
      formData.append('categoriaID', articleData.categoriaID)
      formData.append('imagen', articleData.imagen)
      return axiosClient.post(`${endpoint}?InstitucionID=${institucionID}`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      })
    }
  }

  /**
   * Update article details
   * @param {number} articuloID - Article ID
   * @param {Object} articleData - Updated article data
   * @param {number} usuarioID - User ID (ignored in V1 - uses JWT context)
   * @returns {Promise} API response
   */
  static async updateArticle(articuloID, articleData, usuarioID = null) {
    if (USE_V1_API) {
      const endpoint = `/api/v1/articulos/${articuloID}`
      const payload = {
        nombreArticulo: articleData.nombre || articleData.nombreArticulo,
        precio: articleData.precio
      }
      // Only include categoriaId if it's provided
      if (articleData.categoriaID || articleData.categoriaId) {
        payload.categoriaId = articleData.categoriaID || articleData.categoriaId
      }
      return axiosClient.put(endpoint, payload)
    } else {
      const endpoint = '/api/Articulos/UpdateArticulo'
      const params = new URLSearchParams({
        id: articuloID,
        UsuarioID: usuarioID,
        nombre: articleData.nombre,
        precio: articleData.precio,
        categoriaID: articleData.categoriaID
      })
      return axiosClient.put(`${endpoint}?${params}`)
    }
  }

  /**
   * Update article image
   * @param {number} articuloID - Article ID
   * @param {File} imageFile - New image file
   * @returns {Promise} API response
   */
  static async updateArticleImage(articuloID, imageFile) {
    if (USE_V1_API) {
      const endpoint = `/api/v1/articulos/${articuloID}/image`
      const formData = new FormData()
      formData.append('imagen', imageFile)
      return axiosClient.patch(endpoint, formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      })
    } else {
      const endpoint = '/api/Articulos/UpdateArticuloImage'
      const formData = new FormData()
      formData.append('articuloID', articuloID)
      formData.append('nuevaImagen', imageFile)
      return axiosClient.put(endpoint, formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      })
    }
  }

  /**
   * Delete article (hard delete - Admin/Director only)
   * @param {number} articuloID - Article ID
   * @returns {Promise} API response
   */
  static async deleteArticle(articuloID) {
    if (USE_V1_API) {
      const endpoint = `/api/v1/articulos/${articuloID}`
      return axiosClient.delete(endpoint)
    } else {
      const endpoint = '/api/Articulos/AnularArticulo'
      return axiosClient.delete(`${endpoint}?id=${articuloID}&estado=true`)
    }
  }

  /**
   * Change article status (activate/deactivate)
   * @param {number} articuloID - Article ID
   * @param {boolean} anulado - True to deactivate, false to activate
   * @returns {Promise} API response
   */
  static async changeArticleStatus(articuloID, anulado) {
    if (USE_V1_API) {
      const endpoint = `/api/v1/articulos/${articuloID}/status`
      return axiosClient.patch(endpoint, { anulado })
    } else {
      const endpoint = '/api/Articulos/AnularArticulo'
      return axiosClient.delete(`${endpoint}?id=${articuloID}&estado=${anulado}`)
    }
  }

  /**
   * Get article image
   * @param {number} articuloID - Article ID
   * @returns {Promise} API response (returns image file)
   */
  static async getArticleImage(articuloID) {
    if (USE_V1_API) {
      const endpoint = `/api/v1/articulos/${articuloID}/image`
      return axiosClient.get(endpoint, { responseType: 'blob' })
    } else {
      // Legacy implementation
      throw new Error('Image endpoint not available in legacy API')
    }
  }

  /**
   * Get article image URL (for direct use in img src)
   * @param {number} articuloID - Article ID
   * @returns {string} Image URL
   */
  static getArticleImageUrl(articuloID) {
    if (USE_V1_API) {
      // Get base URL from axios client
      const baseURL = axiosClient.defaults.baseURL || ''
      return `${baseURL}/api/v1/articulos/${articuloID}/image`
    } else {
      return null
    }
  }

  /**
   * Get categories for articles
   * @param {number} institucionID - Institution ID (ignored in V1 - uses JWT context)
   * @returns {Promise} API response
   */
  static async getCategories(institucionID = null) {
    // Categories are handled by a separate service in V1
    // This is maintained for backward compatibility
    const endpoint = USE_V1_API ? '/api/v1/categorias' : '/api/CategoriaArticulos/GetCategorias'
    
    if (USE_V1_API) {
      return axiosClient.get(endpoint)
    } else {
      return axiosClient.get(`${endpoint}?InstitucionID=${institucionID}`)
    }
  }

  /**
   * Health check
   * @returns {Promise} API response
   */
  static async healthCheck() {
    if (USE_V1_API) {
      return axiosClient.get('/api/v1/articulos/health')
    } else {
      throw new Error('Health check not available in legacy API')
    }
  }

  /**
   * Search articles by term
   * @param {string} searchTerm - Search term
   * @param {number} institucionID - Institution ID (ignored in V1)
   * @param {Object} filters - Additional filters
   * @returns {Promise} API response
   */
  static async searchArticles(searchTerm, institucionID = null, filters = {}) {
    if (!USE_V1_API) {
      // For legacy API, use client-side filtering
      return this.getArticles(institucionID, filters.categoriaID)
    }
    
    const params = new URLSearchParams()
    if (searchTerm) params.append('q', searchTerm)
    if (filters.categoriaId) params.append('categoriaId', filters.categoriaId)
    
    const queryString = params.toString()
    const endpoint = '/api/v1/articulos'
    return axiosClient.get(queryString ? `${endpoint}?${queryString}` : endpoint)
  }

  /**
   * Get article statistics
   * @returns {Promise} API response
   */
  static async getStatistics() {
    if (!USE_V1_API) {
      throw new Error('Statistics are only available with V1 API')
    }
    
    return axiosClient.get('/api/v1/articulos/statistics')
  }
}

export default ArticleService