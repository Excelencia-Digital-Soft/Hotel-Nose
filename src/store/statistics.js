import { defineStore } from 'pinia'
import { StatisticsService } from '../services/StatisticsService'

/**
 * Pinia store for managing statistics state
 * Handles caching, loading states, and data management for all statistics
 */
export const useStatisticsStore = defineStore('statistics', {
  state: () => ({
    // Data
    roomRanking: [],
    roomRevenue: [],
    categoryOccupancy: [],
    roomConsumption: [],

    // Loading states
    isLoadingRoomRanking: false,
    isLoadingRoomRevenue: false,
    isLoadingCategoryOccupancy: false,
    isLoadingRoomConsumption: false,

    // Current date range
    currentDateRange: null,

    // Cache timestamps for data freshness
    lastFetchTimes: {
      roomRanking: null,
      roomRevenue: null,
      categoryOccupancy: null,
      roomConsumption: null
    },

    // Error states
    errors: {
      roomRanking: null,
      roomRevenue: null,
      categoryOccupancy: null,
      roomConsumption: null
    }
  }),

  getters: {
    /**
     * Check if any statistics are currently loading
     */
    isAnyLoading: (state) => {
      return state.isLoadingRoomRanking ||
        state.isLoadingRoomRevenue ||
        state.isLoadingCategoryOccupancy ||
        state.isLoadingRoomConsumption
    },

    /**
     * Get total number of rooms from ranking data
     */
    totalRooms: (state) => {
      return state.roomRanking?.length || 0
    },

    /**
     * Get total revenue from all rooms
     */
    totalRevenue: (state) => {
      return state.roomRevenue?.reduce((total, room) => total + (room.totalIngresos || 0), 0) || 0
    },

    /**
     * Get total reservations from ranking data
     */
    totalReservations: (state) => {
      return state.roomRanking?.reduce((total, room) => total + (room.totalReservas || 0), 0) || 0
    },

    /**
     * Get average occupancy rate
     */
    averageOccupancy: (state) => {
      if (!state.categoryOccupancy?.length) return 0
      const totalRate = state.categoryOccupancy.reduce((total, category) => total + (category.tasaOcupacion || 0), 0)
      return totalRate / state.categoryOccupancy.length
    },

    /**
     * Get total consumption amount
     */
    totalConsumption: (state) => {
      return state.roomConsumption?.reduce((total, room) => total + (room.totalConsumos || 0), 0) || 0
    },

    /**
     * Get top performing rooms by reservations
     */
    topRoomsByReservations: (state) => {
      if (!state.roomRanking?.length) return []
      return [...state.roomRanking]
        .sort((a, b) => (b.totalReservas || 0) - (a.totalReservas || 0))
        .slice(0, 5)
    },

    /**
     * Get top revenue generating rooms
     */
    topRoomsByRevenue: (state) => {
      if (!state.roomRevenue?.length) return []
      return [...state.roomRevenue]
        .sort((a, b) => (b.totalIngresos || 0) - (a.totalIngresos || 0))
        .slice(0, 5)
    },

    /**
     * Get categories with highest occupancy
     */
    topCategoriesByOccupancy: (state) => {
      if (!state.categoryOccupancy?.length) return []
      return [...state.categoryOccupancy]
        .sort((a, b) => (b.tasaOcupacion || 0) - (a.tasaOcupacion || 0))
    },

    /**
     * Get rooms with highest consumption
     */
    topRoomsByConsumption: (state) => {
      if (!state.roomConsumption?.length) return []
      return [...state.roomConsumption]
        .sort((a, b) => (b.totalConsumos || 0) - (a.totalConsumos || 0))
        .slice(0, 5)
    },

    /**
     * Check if data is fresh (less than 5 minutes old)
     */
    isDataFresh: (state) => {
      const freshThreshold = 5 * 60 * 1000 // 5 minutes
      const now = Date.now()

      return {
        roomRanking: state.lastFetchTimes.roomRanking && (now - state.lastFetchTimes.roomRanking) < freshThreshold,
        roomRevenue: state.lastFetchTimes.roomRevenue && (now - state.lastFetchTimes.roomRevenue) < freshThreshold,
        categoryOccupancy: state.lastFetchTimes.categoryOccupancy && (now - state.lastFetchTimes.categoryOccupancy) < freshThreshold,
        roomConsumption: state.lastFetchTimes.roomConsumption && (now - state.lastFetchTimes.roomConsumption) < freshThreshold
      }
    },

    /**
     * Get summary statistics for dashboard
     */
    dashboardSummary: (state) => {
      const totalRooms = state.roomRanking?.length || 0
      const totalRevenue = state.roomRevenue?.reduce((total, room) => total + (room.totalIngresos || 0), 0) || 0
      const totalReservations = state.roomRanking?.reduce((total, room) => total + (room.totalReservas || 0), 0) || 0
      const totalConsumption = state.roomConsumption?.reduce((total, room) => total + (room.totalConsumos || 0), 0) || 0

      let averageOccupancy = 0
      if (state.categoryOccupancy?.length > 0) {
        const totalRate = state.categoryOccupancy.reduce((total, category) => total + (category.tasaOcupacion || 0), 0)
        averageOccupancy = totalRate / state.categoryOccupancy.length
      }

      return {
        totalRooms,
        totalRevenue,
        totalReservations,
        averageOccupancy,
        totalConsumption,
        dateRange: state.currentDateRange ? StatisticsService.formatDateRangeDisplay(state.currentDateRange) : null
      }
    }
  },

  actions: {
    /**
     * Set the current date range
     */
    setDateRange(dateRange) {
      this.currentDateRange = dateRange
    },

    /**
     * Clear all errors
     */
    clearErrors() {
      this.errors = {
        roomRanking: null,
        roomRevenue: null,
        categoryOccupancy: null,
        roomConsumption: null
      }
    },

    /**
     * Clear all data
     */
    clearData() {
      this.roomRanking = []
      this.roomRevenue = []
      this.categoryOccupancy = []
      this.roomConsumption = []
      this.lastFetchTimes = {
        roomRanking: null,
        roomRevenue: null,
        categoryOccupancy: null,
        roomConsumption: null
      }
      this.clearErrors()
    },

    /**
     * Fetch room ranking statistics
     */
    async fetchRoomRanking(dateRange, forceRefresh = false) {
      // Check if data is fresh and we don't need to force refresh
      if (!forceRefresh && this.isDataFresh.roomRanking && this.roomRanking.length > 0) {
        return this.roomRanking
      }

      this.isLoadingRoomRanking = true
      this.errors.roomRanking = null

      try {
        const response = await StatisticsService.getRoomRanking(dateRange)

        if (response.isSuccess) {
          this.roomRanking = response.data || []
          this.lastFetchTimes.roomRanking = Date.now()
          this.setDateRange(dateRange)
        } else {
          throw new Error(response.message || 'Error al obtener ranking de habitaciones')
        }

        return this.roomRanking
      } catch (error) {
        this.errors.roomRanking = error.message
        throw error
      } finally {
        this.isLoadingRoomRanking = false
      }
    },

    /**
     * Fetch room revenue statistics
     */
    async fetchRoomRevenue(dateRange, forceRefresh = false) {
      if (!forceRefresh && this.isDataFresh.roomRevenue && this.roomRevenue.length > 0) {
        return this.roomRevenue
      }

      this.isLoadingRoomRevenue = true
      this.errors.roomRevenue = null

      try {
        const response = await StatisticsService.getRoomRevenue(dateRange)

        if (response.isSuccess) {
          this.roomRevenue = response.data || []
          this.lastFetchTimes.roomRevenue = Date.now()
          this.setDateRange(dateRange)
        } else {
          throw new Error(response.message || 'Error al obtener ingresos por habitación')
        }

        return this.roomRevenue
      } catch (error) {
        this.errors.roomRevenue = error.message
        throw error
      } finally {
        this.isLoadingRoomRevenue = false
      }
    },

    /**
     * Fetch category occupancy statistics
     */
    async fetchCategoryOccupancy(dateRange, forceRefresh = false) {
      if (!forceRefresh && this.isDataFresh.categoryOccupancy && this.categoryOccupancy.length > 0) {
        return this.categoryOccupancy
      }

      this.isLoadingCategoryOccupancy = true
      this.errors.categoryOccupancy = null

      try {
        const response = await StatisticsService.getCategoryOccupancy(dateRange)

        if (response.isSuccess) {
          this.categoryOccupancy = response.data || []
          this.lastFetchTimes.categoryOccupancy = Date.now()
          this.setDateRange(dateRange)
        } else {
          throw new Error(response.message || 'Error al obtener ocupación por categoría')
        }

        return this.categoryOccupancy
      } catch (error) {
        this.errors.categoryOccupancy = error.message
        throw error
      } finally {
        this.isLoadingCategoryOccupancy = false
      }
    },

    /**
     * Fetch room consumption statistics
     */
    async fetchRoomConsumption(dateRange, forceRefresh = false) {
      if (!forceRefresh && this.isDataFresh.roomConsumption && this.roomConsumption.length > 0) {
        return this.roomConsumption
      }

      this.isLoadingRoomConsumption = true
      this.errors.roomConsumption = null

      try {
        const response = await StatisticsService.getRoomConsumption(dateRange)

        if (response.isSuccess) {
          this.roomConsumption = response.data || []
          this.lastFetchTimes.roomConsumption = Date.now()
          this.setDateRange(dateRange)
        } else {
          throw new Error(response.message || 'Error al obtener consumo por habitación')
        }

        return this.roomConsumption
      } catch (error) {
        this.errors.roomConsumption = error.message
        throw error
      } finally {
        this.isLoadingRoomConsumption = false
      }
    },

    /**
     * Fetch all statistics at once
     */
    async fetchAllStatistics(dateRange, forceRefresh = false) {
      const promises = [
        this.fetchRoomRanking(dateRange, forceRefresh),
        this.fetchRoomRevenue(dateRange, forceRefresh),
        this.fetchCategoryOccupancy(dateRange, forceRefresh),
        this.fetchRoomConsumption(dateRange, forceRefresh)
      ]

      try {
        await Promise.allSettled(promises)
      } catch (error) {
        console.error('Error fetching statistics:', error)
        throw error
      }
    },

    /**
     * Refresh all data
     */
    async refreshAllData(dateRange) {
      this.clearData()
      return await this.fetchAllStatistics(dateRange, true)
    }
  }
})
