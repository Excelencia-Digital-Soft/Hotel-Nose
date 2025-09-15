/**
 * Article Service V1 - TypeScript Implementation
 * Handles CRUD operations for articles using only V1 API endpoints
 */

import axiosClient from '../axiosClient'
import type {
  ApiResponse,
  ArticleDto,
  ArticleCreateDto,
  ArticleCreateWithImageDto,
  ArticleUpdateDto,
  ArticleStatusDto,
  ArticleSearchParams,
  ArticleStatisticsDto,
} from '../types'

export class ArticleService {
  private static readonly BASE_URL = '/api/v1/articulos'

  /**
   * Get all articles with optional filters
   * GET /api/v1/articulos
   */
  static async getArticles(categoriaId?: number | null): Promise<ApiResponse<ArticleDto[]>> {
    const params = new URLSearchParams()
    if (categoriaId) {
      params.append('categoriaId', categoriaId.toString())
    }

    const queryString = params.toString()
    const url = queryString ? `${this.BASE_URL}?${queryString}` : this.BASE_URL

    const response = await axiosClient.get(url)
    return response.data
  }

  /**
   * Get article by ID
   * GET /api/v1/articulos/{articuloId}
   */
  static async getArticleById(articuloId: number): Promise<ApiResponse<ArticleDto>> {
    const response = await axiosClient.get(`${this.BASE_URL}/${articuloId}`)
    return response.data
  }

  /**
   * Create a new article (without image)
   * POST /api/v1/articulos
   */
  static async createArticle(articleData: ArticleCreateDto): Promise<ApiResponse<ArticleDto>> {
    const payload: ArticleCreateDto = {
      nombreArticulo: articleData.nombreArticulo,
      precio: articleData.precio,
      categoriaId: articleData.categoriaId,
    }

    const response = await axiosClient.post(this.BASE_URL, payload)
    return response.data
  }

  /**
   * Create a new article with image
   * POST /api/v1/articulos/with-image
   */
  static async createArticleWithImage(
    articleData: ArticleCreateWithImageDto
  ): Promise<ApiResponse<ArticleDto>> {
    const formData = new FormData()
    formData.append('nombreArticulo', articleData.nombreArticulo)
    formData.append('precio', articleData.precio.toString())
    formData.append('categoriaId', articleData.categoriaId.toString())
    formData.append('imagen', articleData.imagen)

    const response = await axiosClient.post(`${this.BASE_URL}/with-image`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
    return response.data
  }

  /**
   * Update article details
   * PUT /api/v1/articulos/{articuloId}
   */
  static async updateArticle(
    articuloId: number,
    articleData: ArticleUpdateDto
  ): Promise<ApiResponse<ArticleDto>> {
    const payload: ArticleUpdateDto = {}

    if (articleData.nombreArticulo !== undefined) {
      payload.nombreArticulo = articleData.nombreArticulo
    }
    if (articleData.precio !== undefined) {
      payload.precio = articleData.precio
    }
    if (articleData.categoriaId !== undefined) {
      payload.categoriaId = articleData.categoriaId
    }

    const response = await axiosClient.put(`${this.BASE_URL}/${articuloId}`, payload)
    return response.data
  }

  /**
   * Update article image
   * PATCH /api/v1/articulos/{articuloId}/image
   */
  static async updateArticleImage(
    articuloId: number,
    imageFile: File
  ): Promise<ApiResponse<ArticleDto>> {
    const formData = new FormData()
    formData.append('imagen', imageFile)

    const response = await axiosClient.patch(`${this.BASE_URL}/${articuloId}/image`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
    return response.data
  }

  /**
   * Delete article (hard delete - Admin/Director only)
   * DELETE /api/v1/articulos/{articuloId}
   */
  static async deleteArticle(articuloId: number): Promise<ApiResponse<void>> {
    const response = await axiosClient.delete(`${this.BASE_URL}/${articuloId}`)
    return response.data
  }

  /**
   * Change article status (activate/deactivate)
   * PATCH /api/v1/articulos/{articuloId}/status
   */
  static async changeArticleStatus(
    articuloId: number,
    anulado: boolean
  ): Promise<ApiResponse<ArticleDto>> {
    const payload: ArticleStatusDto = { anulado }
    const response = await axiosClient.patch(`${this.BASE_URL}/${articuloId}/status`, payload)
    return response.data
  }

  /**
   * Get categories for articles
   * GET /api/v1/categorias
   */
  static async getCategories(): Promise<ApiResponse<any[]>> {
    const response = await axiosClient.get('/api/v1/categorias')
    return response.data
  }

  /**
   * Health check
   * GET /api/v1/articulos/health
   */
  static async healthCheck(): Promise<ApiResponse<{ status: string; timestamp: string }>> {
    const response = await axiosClient.get(`${this.BASE_URL}/health`)
    return response.data
  }

  /**
   * Search articles by term and filters
   * GET /api/v1/articulos with query parameters
   */
  static async searchArticles(params: ArticleSearchParams): Promise<ApiResponse<ArticleDto[]>> {
    const searchParams = new URLSearchParams()

    if (params.q) searchParams.append('q', params.q)
    if (params.categoriaId) searchParams.append('categoriaId', params.categoriaId.toString())
    if (params.anulado !== undefined) searchParams.append('anulado', params.anulado.toString())
    if (params.fechaDesde) searchParams.append('fechaDesde', params.fechaDesde)
    if (params.fechaHasta) searchParams.append('fechaHasta', params.fechaHasta)
    if (params.limite) searchParams.append('limite', params.limite.toString())
    if (params.pagina) searchParams.append('pagina', params.pagina.toString())

    const queryString = searchParams.toString()
    const url = queryString ? `${this.BASE_URL}?${queryString}` : this.BASE_URL

    const response = await axiosClient.get(url)
    return response.data
  }

  /**
   * Get article statistics
   * GET /api/v1/articulos/statistics
   */
  static async getStatistics(): Promise<ApiResponse<ArticleStatisticsDto>> {
    const response = await axiosClient.get(`${this.BASE_URL}/statistics`)
    return response.data
  }

  /**
   * Batch delete articles
   * DELETE /api/v1/articulos/batch
   */
  static async batchDeleteArticles(
    articuloIds: number[]
  ): Promise<ApiResponse<{ deleted: number; failed: number }>> {
    const response = await axiosClient.delete(`${this.BASE_URL}/batch`, {
      data: { articuloIds },
    })
    return response.data
  }

  /**
   * Batch update article status
   * PATCH /api/v1/articulos/batch/status
   */
  static async batchUpdateStatus(
    articuloIds: number[],
    anulado: boolean
  ): Promise<ApiResponse<{ updated: number; failed: number }>> {
    const response = await axiosClient.patch(`${this.BASE_URL}/batch/status`, {
      articuloIds,
      anulado,
    })
    return response.data
  }

  /**
   * Validate article data before creation/update
   */
  static validateArticleData(data: Partial<ArticleCreateDto>): {
    isValid: boolean
    errors: string[]
  } {
    const errors: string[] = []

    if (!data.nombreArticulo || data.nombreArticulo.trim().length === 0) {
      errors.push('El nombre del artículo es requerido')
    }

    if (data.nombreArticulo && data.nombreArticulo.length > 100) {
      errors.push('El nombre del artículo no puede exceder 100 caracteres')
    }

    if (!data.precio || data.precio <= 0) {
      errors.push('El precio debe ser mayor a 0')
    }

    if (!data.categoriaId) {
      errors.push('La categoría es requerida')
    }

    return {
      isValid: errors.length === 0,
      errors,
    }
  }

  /**
   * Format article data from form to API format
   */
  static formatArticleForAPI(formData: {
    nombre?: string
    nombreArticulo?: string
    price?: string
    precio?: number
    categoryId?: number
    categoriaId?: number
    categoriaID?: number
  }): ArticleCreateDto {
    return {
      nombreArticulo: formData.nombre || formData.nombreArticulo || '',
      precio:
        typeof formData.precio === 'number' ? formData.precio : parseFloat(formData.price || '0'),
      categoriaId: formData.categoriaId || formData.categoryId || formData.categoriaID || 0,
    }
  }
}

export default ArticleService

