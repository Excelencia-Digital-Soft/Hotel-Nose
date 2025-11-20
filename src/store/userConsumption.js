import { defineStore } from 'pinia'
import { UserConsumptionService } from '../services/userConsumptionService'

/**
 * Pinia store for managing user consumption state
 * Handles caching, loading states, and data management for user consumption
 */
export const useUserConsumptionStore = defineStore('userConsumption', {
  state: () => ({
    // Personal consumption data
    myConsumption: [],
    mySummary: null,
    myConsumptionByService: [],
    
    // Admin data
    userConsumption: [],
    userSummary: null,
    allConsumption: [],
    
    // UI states
    loading: {
      myConsumption: false,
      mySummary: false,
      myConsumptionByService: false,
      userConsumption: false,
      userSummary: false,
      allConsumption: false,
      creating: false
    },
    
    // Error states
    errors: {
      myConsumption: null,
      mySummary: null,
      myConsumptionByService: null,
      userConsumption: null,
      userSummary: null,
      allConsumption: null,
      creating: null
    },
    
    // Current filters
    currentFilters: {
      startDate: null,
      endDate: null,
      selectedUserId: null
    },
    
    // Cache timestamps for data freshness
    lastFetchTimes: {
      myConsumption: null,
      mySummary: null,
      myConsumptionByService: null,
      userConsumption: null,
      userSummary: null,
      allConsumption: null
    },
    
    // Service health
    serviceHealth: null
  }),

  getters: {
    /**
     * Check if any data is currently loading
     */
    isAnyLoading: (state) => {
      return Object.values(state.loading).some(loading => loading)
    },

    /**
     * Get formatted personal consumption with currency formatting
     */
    formattedMyConsumption: (state) => {
      return state.myConsumption.map(consumption => ({
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
      }))
    },

    /**
     * Get total amount of personal consumption
     */
    totalMyConsumption: (state) => {
      return state.myConsumption.reduce((total, item) => total + (item.total || 0), 0)
    },

    /**
     * Get total formatted personal consumption
     */
    totalMyConsumptionFormatted: (state) => {
      const total = state.myConsumption.reduce((sum, item) => sum + (item.total || 0), 0)
      return new Intl.NumberFormat('es-PE', {
        style: 'currency',
        currency: 'PEN'
      }).format(total)
    },

    /**
     * Get consumption grouped by service type for charts
     */
    consumptionByServiceChart: (state) => {
      if (!state.myConsumptionByService?.length) return []
      
      return state.myConsumptionByService.map(service => ({
        label: service.serviceType,
        value: service.totalAmount,
        percentage: service.percentage,
        items: service.totalItems
      }))
    },

    /**
     * Get top consumed items from summary
     */
    topConsumedItems: (state) => {
      return state.mySummary?.topConsumedItems || []
    },

    /**
     * Get consumption statistics for dashboard
     */
    dashboardStats: (state) => {
      if (!state.mySummary) return null
      
      return {
        totalItems: state.mySummary.totalItems || 0,
        totalQuantity: state.mySummary.totalQuantity || 0,
        totalAmount: state.mySummary.totalAmount || 0,
        totalAmountFormatted: new Intl.NumberFormat('es-PE', {
          style: 'currency',
          currency: 'PEN'
        }).format(state.mySummary.totalAmount || 0),
        periodStart: state.mySummary.periodStart,
        periodEnd: state.mySummary.periodEnd,
        amountByType: state.mySummary.amountByType || {}
      }
    },

    /**
     * Check if data is fresh (less than 5 minutes old)
     */
    isDataFresh: (state) => {
      const freshThreshold = 5 * 60 * 1000 // 5 minutes
      const now = Date.now()
      
      return {
        myConsumption: state.lastFetchTimes.myConsumption && (now - state.lastFetchTimes.myConsumption) < freshThreshold,
        mySummary: state.lastFetchTimes.mySummary && (now - state.lastFetchTimes.mySummary) < freshThreshold,
        myConsumptionByService: state.lastFetchTimes.myConsumptionByService && (now - state.lastFetchTimes.myConsumptionByService) < freshThreshold,
        userConsumption: state.lastFetchTimes.userConsumption && (now - state.lastFetchTimes.userConsumption) < freshThreshold,
        userSummary: state.lastFetchTimes.userSummary && (now - state.lastFetchTimes.userSummary) < freshThreshold,
        allConsumption: state.lastFetchTimes.allConsumption && (now - state.lastFetchTimes.allConsumption) < freshThreshold
      }
    },

    /**
     * Get all consumption data (both personal and admin) for export
     */
    allConsumptionForExport: (state) => {
      return [...state.myConsumption, ...state.userConsumption, ...state.allConsumption]
    },

    /**
     * Get consumption grouped by date for charts
     */
    consumptionByDateChart: (state) => {
      const grouped = state.myConsumption.reduce((acc, item) => {
        const date = new Date(item.fechaConsumo).toLocaleDateString('es-PE')
        if (!acc[date]) {
          acc[date] = { date, total: 0, items: 0 }
        }
        acc[date].total += item.total
        acc[date].items += item.cantidad
        return acc
      }, {})
      
      return Object.values(grouped).sort((a, b) => new Date(a.date) - new Date(b.date))
    }
  },

  actions: {
    /**
     * Set current filters
     */
    setFilters(filters) {
      this.currentFilters = { ...this.currentFilters, ...filters }
    },

    /**
     * Clear all errors
     */
    clearErrors() {
      this.errors = {
        myConsumption: null,
        mySummary: null,
        myConsumptionByService: null,
        userConsumption: null,
        userSummary: null,
        allConsumption: null,
        creating: null
      }
    },

    /**
     * Clear all data
     */
    clearData() {
      this.myConsumption = []
      this.mySummary = null
      this.myConsumptionByService = []
      this.userConsumption = []
      this.userSummary = null
      this.allConsumption = []
      this.lastFetchTimes = {
        myConsumption: null,
        mySummary: null,
        myConsumptionByService: null,
        userConsumption: null,
        userSummary: null,
        allConsumption: null
      }
      this.clearErrors()
    },

    /**
     * Fetch personal consumption
     */
    async fetchMyConsumption(startDate = null, endDate = null, forceRefresh = false) {
      if (!forceRefresh && this.isDataFresh.myConsumption && this.myConsumption.length > 0) {
        return this.myConsumption
      }

      this.loading.myConsumption = true
      this.errors.myConsumption = null

      try {
        const response = await UserConsumptionService.getMyConsumption(startDate, endDate)
        
        if (response.isSuccess) {
          this.myConsumption = response.data || []
          this.lastFetchTimes.myConsumption = Date.now()
          this.setFilters({ startDate, endDate })
        } else {
          throw new Error('Error al obtener mi consumo')
        }

        return this.myConsumption
      } catch (error) {
        this.errors.myConsumption = error.message
        throw error
      } finally {
        this.loading.myConsumption = false
      }
    },

    /**
     * Fetch personal consumption summary
     */
    async fetchMySummary(startDate = null, endDate = null, forceRefresh = false) {
      if (!forceRefresh && this.isDataFresh.mySummary && this.mySummary) {
        return this.mySummary
      }

      this.loading.mySummary = true
      this.errors.mySummary = null

      try {
        const response = await UserConsumptionService.getMySummary(startDate, endDate)
        
        if (response.isSuccess) {
          this.mySummary = response.data
          this.lastFetchTimes.mySummary = Date.now()
          this.setFilters({ startDate, endDate })
        } else {
          throw new Error('Error al obtener resumen de consumo')
        }

        return this.mySummary
      } catch (error) {
        this.errors.mySummary = error.message
        throw error
      } finally {
        this.loading.mySummary = false
      }
    },

    /**
     * Fetch personal consumption by service type
     */
    async fetchMyConsumptionByService(startDate = null, endDate = null, forceRefresh = false) {
      if (!forceRefresh && this.isDataFresh.myConsumptionByService && this.myConsumptionByService.length > 0) {
        return this.myConsumptionByService
      }

      this.loading.myConsumptionByService = true
      this.errors.myConsumptionByService = null

      try {
        const response = await UserConsumptionService.getMyConsumptionByService(startDate, endDate)
        
        if (response.isSuccess) {
          this.myConsumptionByService = response.data || []
          this.lastFetchTimes.myConsumptionByService = Date.now()
          this.setFilters({ startDate, endDate })
        } else {
          throw new Error('Error al obtener consumo por servicio')
        }

        return this.myConsumptionByService
      } catch (error) {
        this.errors.myConsumptionByService = error.message
        throw error
      } finally {
        this.loading.myConsumptionByService = false
      }
    },

    /**
     * Fetch specific user consumption (Admin/Director only)
     */
    async fetchUserConsumption(userId, startDate = null, endDate = null, forceRefresh = false) {
      if (!forceRefresh && this.isDataFresh.userConsumption && this.userConsumption.length > 0 && this.currentFilters.selectedUserId === userId) {
        return this.userConsumption
      }

      this.loading.userConsumption = true
      this.errors.userConsumption = null

      try {
        const response = await UserConsumptionService.getUserConsumption(userId, startDate, endDate)
        
        if (response.isSuccess) {
          this.userConsumption = response.data || []
          this.lastFetchTimes.userConsumption = Date.now()
          this.setFilters({ startDate, endDate, selectedUserId: userId })
        } else {
          throw new Error('Error al obtener consumo del usuario')
        }

        return this.userConsumption
      } catch (error) {
        this.errors.userConsumption = error.message
        throw error
      } finally {
        this.loading.userConsumption = false
      }
    },

    /**
     * Fetch all consumption (Admin/Director only)
     */
    async fetchAllConsumption(startDate = null, endDate = null, forceRefresh = false) {
      if (!forceRefresh && this.isDataFresh.allConsumption && this.allConsumption.length > 0) {
        return this.allConsumption
      }

      this.loading.allConsumption = true
      this.errors.allConsumption = null

      try {
        const response = await UserConsumptionService.getAllConsumption(startDate, endDate)
        
        if (response.isSuccess) {
          this.allConsumption = response.data || []
          this.lastFetchTimes.allConsumption = Date.now()
          this.setFilters({ startDate, endDate })
        } else {
          throw new Error('Error al obtener todos los consumos')
        }

        return this.allConsumption
      } catch (error) {
        this.errors.allConsumption = error.message
        throw error
      } finally {
        this.loading.allConsumption = false
      }
    },

    /**
     * Create new consumption
     */
    async createConsumption(consumptionData) {
      this.loading.creating = true
      this.errors.creating = null

      try {
        const response = await UserConsumptionService.createConsumption(consumptionData)
        
        if (response.isSuccess) {
          // Add to local state
          this.myConsumption.unshift(response.data)
          
          // Refresh summary data to reflect new consumption
          if (this.mySummary) {
            await this.fetchMySummary(this.currentFilters.startDate, this.currentFilters.endDate, true)
          }
          
          return response.data
        } else {
          throw new Error('Error al crear consumo')
        }
      } catch (error) {
        this.errors.creating = error.message
        throw error
      } finally {
        this.loading.creating = false
      }
    },

    /**
     * Fetch all personal data at once
     */
    async fetchAllPersonalData(startDate = null, endDate = null, forceRefresh = false) {
      const promises = [
        this.fetchMyConsumption(startDate, endDate, forceRefresh),
        this.fetchMySummary(startDate, endDate, forceRefresh),
        this.fetchMyConsumptionByService(startDate, endDate, forceRefresh)
      ]

      try {
        await Promise.allSettled(promises)
      } catch (error) {
        console.error('Error fetching personal consumption data:', error)
        throw error
      }
    },

    /**
     * Refresh all data
     */
    async refreshAllData(startDate = null, endDate = null) {
      this.clearData()
      return await this.fetchAllPersonalData(startDate, endDate, true)
    },

    /**
     * Check service health
     */
    async checkServiceHealth() {
      try {
        const response = await UserConsumptionService.healthCheck()
        this.serviceHealth = response
        return response
      } catch (error) {
        this.serviceHealth = null
        throw error
      }
    }
  }
})