import { ref, computed, watch } from 'vue'
import { useToast } from 'primevue/usetoast'
import { useStatisticsStore } from '../store/statistics'
import { StatisticsService } from '../services/StatisticsService'
import { useAuthStore } from '../store/auth'

/**
 * Composable for managing statistics data and state
 * Provides reactive state management and helper functions for statistics components
 */
export function useStatistics() {
  // Stores
  const statisticsStore = useStatisticsStore()
  const authStore = useAuthStore()
  const toast = useToast()

  // Reactive state
  const selectedDateRange = ref(null)
  const selectedPredefinedRange = ref('lastMonth')
  const isRefreshing = ref(false)

  // Initialize default date range
  const initializeDateRange = () => {
    if (!selectedDateRange.value && authStore.institucionID) {
      selectedDateRange.value = StatisticsService.getDefaultDateRange(authStore.institucionID)
    }
  }

  // Computed properties
  const predefinedRanges = computed(() => {
    if (!authStore.institucionID) return {}
    return StatisticsService.getPredefinedRanges(authStore.institucionID)
  })

  const currentDateRangeDisplay = computed(() => {
    if (!selectedDateRange.value) return ''
    return StatisticsService.formatDateRangeDisplay(selectedDateRange.value)
  })

  const isValidDateRange = computed(() => {
    if (!selectedDateRange.value) return false
    const validation = StatisticsService.validateDateRange(selectedDateRange.value)
    return validation.isValid
  })

  const dateRangeErrors = computed(() => {
    if (!selectedDateRange.value) return []
    const validation = StatisticsService.validateDateRange(selectedDateRange.value)
    return validation.errors
  })

  // Store getters as computed properties for reactivity
  const roomRanking = computed(() => statisticsStore.roomRanking)
  const roomRevenue = computed(() => statisticsStore.roomRevenue)
  const categoryOccupancy = computed(() => statisticsStore.categoryOccupancy)
  const roomConsumption = computed(() => statisticsStore.roomConsumption)

  const isLoadingAny = computed(() => statisticsStore.isAnyLoading)
  const isLoadingRoomRanking = computed(() => statisticsStore.isLoadingRoomRanking)
  const isLoadingRoomRevenue = computed(() => statisticsStore.isLoadingRoomRevenue)
  const isLoadingCategoryOccupancy = computed(() => statisticsStore.isLoadingCategoryOccupancy)
  const isLoadingRoomConsumption = computed(() => statisticsStore.isLoadingRoomConsumption)

  const dashboardSummary = computed(() => statisticsStore.dashboardSummary)
  const topRoomsByReservations = computed(() => statisticsStore.topRoomsByReservations)
  const topRoomsByRevenue = computed(() => statisticsStore.topRoomsByRevenue)
  const topCategoriesByOccupancy = computed(() => statisticsStore.topCategoriesByOccupancy)
  const topRoomsByConsumption = computed(() => statisticsStore.topRoomsByConsumption)

  const errors = computed(() => statisticsStore.errors)

  // Methods
  const showSuccess = (message) => {
    toast.add({
      severity: 'success',
      summary: 'Éxito',
      detail: message,
      life: 5000
    })
  }

  const showError = (message) => {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: message,
      life: 5000
    })
  }

  const showInfo = (message) => {
    toast.add({
      severity: 'info',
      summary: 'Información',
      detail: message,
      life: 5000
    })
  }

  /**
   * Set predefined date range
   */
  const setPredefinedRange = (rangeKey) => {
    const ranges = predefinedRanges.value
    if (ranges[rangeKey]) {
      selectedPredefinedRange.value = rangeKey
      selectedDateRange.value = {
        fechaInicio: ranges[rangeKey].fechaInicio,
        fechaFin: ranges[rangeKey].fechaFin,
        institucionID: ranges[rangeKey].institucionID
      }
    }
  }

  /**
   * Set custom date range
   */
  const setCustomDateRange = (startDate, endDate) => {
    selectedPredefinedRange.value = 'custom'
    selectedDateRange.value = {
      fechaInicio: startDate,
      fechaFin: endDate,
      institucionID: authStore.institucionID
    }
  }

  /**
   * Fetch room ranking data
   */
  const fetchRoomRanking = async (forceRefresh = false) => {
    if (!isValidDateRange.value) {
      showError('Rango de fechas no válido')
      return
    }

    try {
      await statisticsStore.fetchRoomRanking(selectedDateRange.value, forceRefresh)
      if (forceRefresh) {
        showSuccess('📊 Ranking de habitaciones actualizado')
      }
    } catch (error) {
      showError(`❌ ${error.message}`)
    }
  }

  /**
   * Fetch room revenue data
   */
  const fetchRoomRevenue = async (forceRefresh = false) => {
    if (!isValidDateRange.value) {
      showError('Rango de fechas no válido')
      return
    }

    try {
      await statisticsStore.fetchRoomRevenue(selectedDateRange.value, forceRefresh)
      if (forceRefresh) {
        showSuccess('💰 Ingresos por habitación actualizados')
      }
    } catch (error) {
      showError(`❌ ${error.message}`)
    }
  }

  /**
   * Fetch category occupancy data
   */
  const fetchCategoryOccupancy = async (forceRefresh = false) => {
    if (!isValidDateRange.value) {
      showError('Rango de fechas no válido')
      return
    }

    try {
      await statisticsStore.fetchCategoryOccupancy(selectedDateRange.value, forceRefresh)
      if (forceRefresh) {
        showSuccess('📈 Ocupación por categoría actualizada')
      }
    } catch (error) {
      showError(`❌ ${error.message}`)
    }
  }

  /**
   * Fetch room consumption data
   */
  const fetchRoomConsumption = async (forceRefresh = false) => {
    if (!isValidDateRange.value) {
      showError('Rango de fechas no válido')
      return
    }

    try {
      await statisticsStore.fetchRoomConsumption(selectedDateRange.value, forceRefresh)
      if (forceRefresh) {
        showSuccess('🍽️ Consumo por habitación actualizado')
      }
    } catch (error) {
      showError(`❌ ${error.message}`)
    }
  }

  /**
   * Fetch all statistics
   */
  const fetchAllStatistics = async (forceRefresh = false) => {
    if (!isValidDateRange.value) {
      showError('Rango de fechas no válido')
      return
    }

    console.log('🔄 [useStatistics] Fetching all statistics with:', {
      dateRange: selectedDateRange.value,
      forceRefresh,
      isValid: isValidDateRange.value
    })

    try {
      await statisticsStore.fetchAllStatistics(selectedDateRange.value, forceRefresh)
      if (forceRefresh) {
        showSuccess('📊 Todas las estadísticas actualizadas')
      }
    } catch (error) {
      console.error('❌ [useStatistics] Error in fetchAllStatistics:', error)
      showError(`❌ Error al cargar estadísticas: ${error.message}`)
    }
  }

  /**
   * Refresh all data
   */
  const refreshAllData = async () => {
    if (!isValidDateRange.value) {
      showError('Rango de fechas no válido')
      return
    }

    isRefreshing.value = true
    try {
      await statisticsStore.refreshAllData(selectedDateRange.value)
      showSuccess('🔄 Todas las estadísticas han sido actualizadas')
    } catch (error) {
      showError(`❌ Error al actualizar estadísticas: ${error.message}`)
    } finally {
      isRefreshing.value = false
    }
  }

  /**
   * Clear all data
   */
  const clearData = () => {
    statisticsStore.clearData()
    showInfo('🗑️ Datos de estadísticas limpiados')
  }

  /**
   * Format currency for display
   */
  const formatCurrency = (amount) => {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    }).format(amount || 0)
  }

  /**
   * Format percentage for display
   */
  const formatPercentage = (value) => {
    return new Intl.NumberFormat('es-ES', {
      style: 'percent',
      minimumFractionDigits: 1,
      maximumFractionDigits: 1
    }).format((value || 0) / 100)
  }

  /**
   * Format number with thousand separators
   */
  const formatNumber = (value) => {
    return new Intl.NumberFormat('es-ES').format(value || 0)
  }

  /**
   * Get chart colors for consistent theming
   */
  const getChartColors = () => {
    return {
      primary: ['#818cf8', '#6366f1', '#4f46e5', '#4338ca'],
      secondary: ['#a78bfa', '#8b5cf6', '#7c3aed', '#6d28d9'],
      accent: ['#f472b6', '#ec4899', '#db2777', '#be185d'],
      success: ['#34d399', '#10b981', '#059669', '#047857'],
      warning: ['#fbbf24', '#f59e0b', '#d97706', '#b45309'],
      error: ['#f87171', '#ef4444', '#dc2626', '#b91c1c'],
      info: ['#60a5fa', '#3b82f6', '#2563eb', '#1d4ed8']
    }
  }

  // Watch for date range changes and auto-fetch data
  watch(selectedDateRange, async (newRange) => {
    if (newRange && isValidDateRange.value) {
      await fetchAllStatistics()
    }
  }, { deep: true })

  // Initialize on composable creation
  initializeDateRange()

  return {
    // Reactive state
    selectedDateRange,
    selectedPredefinedRange,
    isRefreshing,

    // Computed properties
    predefinedRanges,
    currentDateRangeDisplay,
    isValidDateRange,
    dateRangeErrors,

    // Store data
    roomRanking,
    roomRevenue,
    categoryOccupancy,
    roomConsumption,

    // Loading states
    isLoadingAny,
    isLoadingRoomRanking,
    isLoadingRoomRevenue,
    isLoadingCategoryOccupancy,
    isLoadingRoomConsumption,

    // Computed statistics
    dashboardSummary,
    topRoomsByReservations,
    topRoomsByRevenue,
    topCategoriesByOccupancy,
    topRoomsByConsumption,

    // Errors
    errors,

    // Methods
    setPredefinedRange,
    setCustomDateRange,
    fetchRoomRanking,
    fetchRoomRevenue,
    fetchCategoryOccupancy,
    fetchRoomConsumption,
    fetchAllStatistics,
    refreshAllData,
    clearData,

    // Formatting helpers
    formatCurrency,
    formatPercentage,
    formatNumber,
    getChartColors,

    // Toast helpers
    showSuccess,
    showError,
    showInfo
  }
}