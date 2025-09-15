import axiosClient from '../axiosClient'

/**
 * Statistics Service - Handles all statistics-related API calls
 * Uses V1 API endpoints for comprehensive hotel analytics
 */
export class StatisticsService {
  /**
   * Get room usage ranking statistics
   * @param {Object} dateRange - Date range and institution filter
   * @param {string} dateRange.fechaInicio - Start date (ISO string)
   * @param {string} dateRange.fechaFin - End date (ISO string)
   * @param {number} dateRange.institucionID - Institution ID
   * @returns {Promise<Object>} Room ranking data
   */
  static async getRoomRanking(dateRange) {
    try {
      console.log('📊 [StatisticsService] Fetching room ranking with:', dateRange)
      const response = await axiosClient.post('/api/v1/statistics/room-ranking', {
        fechaInicio: dateRange.fechaInicio,
        fechaFin: dateRange.fechaFin,
        institucionID: dateRange.institucionID
      })
      console.log('📊 [StatisticsService] Room ranking response:', response.data)
      return response.data || { isSuccess: false, data: [] }
    } catch (error) {
      console.error('❌ [StatisticsService] Error fetching room ranking:', error)
      console.error('❌ [StatisticsService] Error details:', {
        status: error.response?.status,
        statusText: error.response?.statusText,
        data: error.response?.data,
        url: error.config?.url
      })
      throw new Error(error.response?.data?.message || 'Error al obtener el ranking de habitaciones')
    }
  }

  /**
   * Get room revenue statistics
   * @param {Object} dateRange - Date range and institution filter
   * @returns {Promise<Object>} Room revenue data with detailed breakdown
   */
  static async getRoomRevenue(dateRange) {
    try {
      const response = await axiosClient.post('/api/v1/statistics/room-revenue', {
        fechaInicio: dateRange.fechaInicio,
        fechaFin: dateRange.fechaFin,
        institucionID: dateRange.institucionID
      })
      console.log('📊 [StatisticsService] Room revenue response:', response.data)
      return response.data || { isSuccess: false, data: [] }
    } catch (error) {
      console.error('Error fetching room revenue:', error)
      throw new Error(error.response?.data?.message || 'Error al obtener los ingresos por habitación')
    }
  }

  /**
   * Get category occupancy rate statistics
   * @param {Object} dateRange - Date range and institution filter
   * @returns {Promise<Object>} Category occupancy data
   */
  static async getCategoryOccupancy(dateRange) {
    try {
      const response = await axiosClient.post('/api/v1/statistics/category-occupancy', {
        fechaInicio: dateRange.fechaInicio,
        fechaFin: dateRange.fechaFin,
        institucionID: dateRange.institucionID
      })
      console.log('📊 [StatisticsService] Category occupancy response:', response.data)
      return response.data || { isSuccess: false, data: [] }
    } catch (error) {
      console.error('Error fetching category occupancy:', error)
      throw new Error(error.response?.data?.message || 'Error al obtener la ocupación por categoría')
    }
  }

  /**
   * Get room consumption statistics
   * @param {Object} dateRange - Date range and institution filter
   * @returns {Promise<Object>} Room consumption data with detailed breakdown
   */
  static async getRoomConsumption(dateRange) {
    try {
      const response = await axiosClient.post('/api/v1/statistics/room-consumption', {
        fechaInicio: dateRange.fechaInicio,
        fechaFin: dateRange.fechaFin,
        institucionID: dateRange.institucionID
      })
      console.log('📊 [StatisticsService] Room consumption response:', response.data)
      return response.data || { isSuccess: false, data: [] }
    } catch (error) {
      console.error('Error fetching room consumption:', error)
      throw new Error(error.response?.data?.message || 'Error al obtener el consumo por habitación')
    }
  }

  /**
   * Validate date range
   * @param {Object} dateRange - Date range to validate
   * @returns {Object} Validation result
   */
  static validateDateRange(dateRange) {
    const errors = []

    if (!dateRange.fechaInicio) {
      errors.push('La fecha de inicio es requerida')
    }

    if (!dateRange.fechaFin) {
      errors.push('La fecha de fin es requerida')
    }

    if (!dateRange.institucionID || dateRange.institucionID <= 0) {
      errors.push('ID de institución válido es requerido')
    }

    if (dateRange.fechaInicio && dateRange.fechaFin) {
      const startDate = new Date(dateRange.fechaInicio)
      const endDate = new Date(dateRange.fechaFin)

      if (startDate > endDate) {
        errors.push('La fecha de inicio debe ser menor o igual a la fecha de fin')
      }

      // Check if date range is not too large (e.g., more than 1 year)
      const diffTime = Math.abs(endDate - startDate)
      const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24))

      if (diffDays > 365) {
        errors.push('El rango de fechas no puede ser mayor a 1 año')
      }
    }

    return {
      isValid: errors.length === 0,
      errors
    }
  }

  /**
   * Format date range for display
   * @param {Object} dateRange - Date range to format
   * @returns {string} Formatted date range string
   */
  static formatDateRangeDisplay(dateRange) {
    if (!dateRange.fechaInicio || !dateRange.fechaFin) {
      return 'Rango de fechas no válido'
    }

    const startDate = new Date(dateRange.fechaInicio)
    const endDate = new Date(dateRange.fechaFin)

    const formatter = new Intl.DateTimeFormat('es-ES', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    })

    return `${formatter.format(startDate)} - ${formatter.format(endDate)}`
  }

  /**
   * Get default date range (last 30 days)
   * @param {number} institucionID - Institution ID
   * @returns {Object} Default date range
   */
  static getDefaultDateRange(institucionID) {
    const endDate = new Date()
    const startDate = new Date()
    startDate.setDate(startDate.getDate() - 30)

    return {
      fechaInicio: startDate.toISOString().split('T')[0],
      fechaFin: endDate.toISOString().split('T')[0],
      institucionID
    }
  }

  /**
   * Get predefined date ranges
   * @param {number} institucionID - Institution ID
   * @returns {Object} Predefined date ranges
   */
  static getPredefinedRanges(institucionID) {
    const today = new Date()
    const yesterday = new Date(today)
    yesterday.setDate(yesterday.getDate() - 1)

    const lastWeek = new Date(today)
    lastWeek.setDate(lastWeek.getDate() - 7)

    const lastMonth = new Date(today)
    lastMonth.setMonth(lastMonth.getMonth() - 1)

    const last3Months = new Date(today)
    last3Months.setMonth(last3Months.getMonth() - 3)

    const startOfMonth = new Date(today.getFullYear(), today.getMonth(), 1)
    const startOfYear = new Date(today.getFullYear(), 0, 1)

    return {
      today: {
        label: 'Hoy',
        fechaInicio: today.toISOString().split('T')[0],
        fechaFin: today.toISOString().split('T')[0],
        institucionID
      },
      yesterday: {
        label: 'Ayer',
        fechaInicio: yesterday.toISOString().split('T')[0],
        fechaFin: yesterday.toISOString().split('T')[0],
        institucionID
      },
      lastWeek: {
        label: 'Últimos 7 días',
        fechaInicio: lastWeek.toISOString().split('T')[0],
        fechaFin: today.toISOString().split('T')[0],
        institucionID
      },
      lastMonth: {
        label: 'Últimos 30 días',
        fechaInicio: lastMonth.toISOString().split('T')[0],
        fechaFin: today.toISOString().split('T')[0],
        institucionID
      },
      last3Months: {
        label: 'Últimos 3 meses',
        fechaInicio: last3Months.toISOString().split('T')[0],
        fechaFin: today.toISOString().split('T')[0],
        institucionID
      },
      thisMonth: {
        label: 'Este mes',
        fechaInicio: startOfMonth.toISOString().split('T')[0],
        fechaFin: today.toISOString().split('T')[0],
        institucionID
      },
      thisYear: {
        label: 'Este año',
        fechaInicio: startOfYear.toISOString().split('T')[0],
        fechaFin: today.toISOString().split('T')[0],
        institucionID
      }
    }
  }
}
