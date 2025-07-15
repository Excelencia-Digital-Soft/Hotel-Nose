import axiosClient from '../axiosClient'

export class UserConsumptionService {
  // My personal consumption
  static async getMyConsumption(startDate = null, endDate = null) {
    try {
      const params = new URLSearchParams()
      if (startDate) params.append('startDate', startDate)
      if (endDate) params.append('endDate', endDate)
      
      const response = await axiosClient.get(`/api/v1/user-consumption/my-consumption?${params}`)
      return response.data
    } catch (error) {
      console.error('Error fetching my consumption:', error)
      throw error
    }
  }

  // My consumption summary
  static async getMySummary(startDate = null, endDate = null) {
    try {
      const params = new URLSearchParams()
      if (startDate) params.append('startDate', startDate)
      if (endDate) params.append('endDate', endDate)
      
      const response = await axiosClient.get(`/api/v1/user-consumption/my-summary?${params}`)
      return response.data
    } catch (error) {
      console.error('Error fetching my consumption summary:', error)
      throw error
    }
  }

  // My consumption by service type
  static async getMyConsumptionByService(startDate = null, endDate = null) {
    try {
      const params = new URLSearchParams()
      if (startDate) params.append('startDate', startDate)
      if (endDate) params.append('endDate', endDate)
      
      const response = await axiosClient.get(`/api/v1/user-consumption/my-consumption/by-service?${params}`)
      return response.data
    } catch (error) {
      console.error('Error fetching my consumption by service:', error)
      throw error
    }
  }

  // Specific user consumption (Admin/Director only)
  static async getUserConsumption(userId, startDate = null, endDate = null) {
    try {
      const params = new URLSearchParams()
      if (startDate) params.append('startDate', startDate)
      if (endDate) params.append('endDate', endDate)
      
      const response = await axiosClient.get(`/api/v1/user-consumption/user/${userId}?${params}`)
      return response.data
    } catch (error) {
      console.error('Error fetching user consumption:', error)
      throw error
    }
  }

  // Specific user summary (Admin/Director only)
  static async getUserSummary(userId, startDate = null, endDate = null) {
    try {
      const params = new URLSearchParams()
      if (startDate) params.append('startDate', startDate)
      if (endDate) params.append('endDate', endDate)
      
      const response = await axiosClient.get(`/api/v1/user-consumption/user/${userId}/summary?${params}`)
      return response.data
    } catch (error) {
      console.error('Error fetching user summary:', error)
      throw error
    }
  }

  // All users consumption (Admin/Director only)
  static async getAllConsumption(startDate = null, endDate = null) {
    try {
      const params = new URLSearchParams()
      if (startDate) params.append('startDate', startDate)
      if (endDate) params.append('endDate', endDate)
      
      const response = await axiosClient.get(`/api/v1/user-consumption/all?${params}`)
      return response.data
    } catch (error) {
      console.error('Error fetching all consumption:', error)
      throw error
    }
  }

  // Register new consumption
  static async createConsumption(consumptionData) {
    try {
      const response = await axiosClient.post('/api/v1/user-consumption/', consumptionData)
      return response.data
    } catch (error) {
      console.error('Error creating consumption:', error)
      throw error
    }
  }

  // Register new consumption for another user (Admin only)
  static async createConsumptionForUser(consumptionData) {
    try {
      const response = await axiosClient.post('/api/v1/user-consumption/admin/create-for-user', consumptionData)
      return response.data
    } catch (error) {
      console.error('Error creating consumption for user:', error)
      throw error
    }
  }

  // Health check
  static async healthCheck() {
    try {
      const response = await axiosClient.get('/api/v1/user-consumption/health')
      return response.data
    } catch (error) {
      console.error('Error checking health:', error)
      throw error
    }
  }

  // Utility method to build date filters
  static buildDateFilter(startDate, endDate) {
    const filter = {}
    if (startDate) filter.startDate = startDate
    if (endDate) filter.endDate = endDate
    return filter
  }

  // Format consumption data for display
  static formatConsumptionData(consumption) {
    return {
      ...consumption,
      fechaConsumo: new Date(consumption.fechaConsumo),
      totalFormatted: new Intl.NumberFormat('es-PE', {
        style: 'currency',
        currency: 'PEN'
      }).format(consumption.total),
      precioUnitarioFormatted: new Intl.NumberFormat('es-PE', {
        style: 'currency',
        currency: 'PEN'
      }).format(consumption.precioUnitario)
    }
  }
}