import { ref, computed, onMounted, onUnmounted } from 'vue'
import { inventoryService } from '../services/roomInventoryService'
import { InventoryLocationType } from '../types'

/**
 * Composable for Inventory Alert Management (V1 API)
 * Handles low stock, critical stock, and out of stock alerts
 * 
 * Features:
 * - Real-time alert monitoring
 * - Alert acknowledgment
 * - Alert configuration
 * - Auto-refresh capability
 * - Filtering and sorting
 */
export function useInventoryAlerts(options = {}) {
  const {
    autoRefresh = true,
    refreshInterval = 30000, // 30 seconds
    filterByRoom = null,
    alertTypes = ['StockBajo', 'StockCritico', 'StockAgotado']
  } = options

  // State
  const loading = ref(false)
  const saving = ref(false)
  const error = ref(null)
  const alerts = ref([])
  const refreshTimer = ref(null)

  // Computed
  const activeAlerts = computed(() => 
    alerts.value.filter(alert => !alert.reconocida)
  )

  const criticalAlerts = computed(() =>
    activeAlerts.value.filter(alert => 
      alert.severidad === 'Critica' || alert.tipoAlerta === 'StockAgotado'
    )
  )

  const lowStockAlerts = computed(() =>
    activeAlerts.value.filter(alert => alert.tipoAlerta === 'StockBajo')
  )

  const outOfStockAlerts = computed(() =>
    activeAlerts.value.filter(alert => alert.tipoAlerta === 'StockAgotado')
  )

  const alertsByRoom = computed(() => {
    const roomMap = new Map()
    
    activeAlerts.value.forEach(alert => {
      if (alert.locationType === InventoryLocationType.Room && alert.locationId) {
        if (!roomMap.has(alert.locationId)) {
          roomMap.set(alert.locationId, {
            roomId: alert.locationId,
            roomName: alert.locationName || `Habitación ${alert.locationId}`,
            alerts: []
          })
        }
        roomMap.get(alert.locationId).alerts.push(alert)
      }
    })
    
    return Array.from(roomMap.values())
      .sort((a, b) => b.alerts.length - a.alerts.length) // Sort by alert count
  })

  const alertCounts = computed(() => ({
    total: activeAlerts.value.length,
    critical: criticalAlerts.value.length,
    lowStock: lowStockAlerts.value.length,
    outOfStock: outOfStockAlerts.value.length,
    roomsAffected: alertsByRoom.value.length
  }))

  // Methods

  /**
   * 📡 Fetch all active alerts
   */
  const fetchAlerts = async (filters = {}) => {
    try {
      loading.value = true
      error.value = null

      const params = {
        soloNoReconocidas: false,
        ...filters
      }

      // Filter by room if specified
      if (filterByRoom) {
        params.tipoUbicacion = InventoryLocationType.Room
        // Note: The API might need room filtering capability
      }

      const response = await inventoryService.getActiveAlerts(params)
      
      if (response.isSuccess) {
        // Filter by alert types
        alerts.value = response.data.filter(alert => 
          alertTypes.includes(alert.tipoAlerta)
        )

        // Additional client-side filtering by room if needed
        if (filterByRoom) {
          alerts.value = alerts.value.filter(alert => 
            alert.locationId === filterByRoom
          )
        }
      } else {
        throw new Error(response.message || 'Error al cargar alertas')
      }
    } catch (err) {
      error.value = err.message
      console.error('Error fetching alerts:', err)
    } finally {
      loading.value = false
    }
  }

  /**
   * ✅ Acknowledge an alert
   */
  const acknowledgeAlert = async (alertId, comments = '') => {
    try {
      saving.value = true
      error.value = null

      const response = await inventoryService.acknowledgeAlert(alertId, {
        comentarios: comments,
        resolverAlerta: true
      })
      
      if (response.isSuccess) {
        // Update local state
        const alert = alerts.value.find(a => a.alertId === alertId)
        if (alert) {
          alert.reconocida = true
          alert.fechaReconocimiento = new Date().toISOString()
          alert.comentarios = comments
        }
        return response.data
      } else {
        throw new Error(response.message || 'Error al reconocer alerta')
      }
    } catch (err) {
      error.value = err.message
      throw err
    } finally {
      saving.value = false
    }
  }

  /**
   * 🔄 Acknowledge multiple alerts
   */
  const acknowledgeMultipleAlerts = async (alertIds, comments = '') => {
    try {
      saving.value = true
      error.value = null

      const results = await Promise.allSettled(
        alertIds.map(alertId => acknowledgeAlert(alertId, comments))
      )

      const successful = results.filter(r => r.status === 'fulfilled').length
      const failed = results.filter(r => r.status === 'rejected').length

      if (failed > 0) {
        throw new Error(`${failed} alertas no pudieron ser reconocidas`)
      }

      return { successful, failed }
    } catch (err) {
      error.value = err.message
      throw err
    } finally {
      saving.value = false
    }
  }

  /**
   * ⚙️ Configure alert thresholds for inventory item
   */
  const configureAlerts = async (inventoryId, config) => {
    try {
      saving.value = true
      error.value = null

      const response = await inventoryService.configureAlerts({
        inventarioId: inventoryId,
        umbralStockBajo: config.lowStockThreshold || 5,
        umbralStockCritico: config.criticalStockThreshold || 1,
        alertasStockBajoActivas: config.enableLowStockAlerts !== false,
        notificacionEmailActiva: config.enableEmailNotifications !== false
      })
      
      if (response.isSuccess) {
        await fetchAlerts() // Refresh to get updated alerts
        return response.data
      } else {
        throw new Error(response.message || 'Error al configurar alertas')
      }
    } catch (err) {
      error.value = err.message
      throw err
    } finally {
      saving.value = false
    }
  }

  /**
   * 🎯 Get alerts for specific room
   */
  const getAlertsForRoom = (roomId) => {
    return alerts.value.filter(alert => 
      alert.locationType === InventoryLocationType.Room && 
      alert.locationId === roomId
    )
  }

  /**
   * 📊 Get alert summary by severity
   */
  const getAlertSummary = () => {
    const summary = {
      Baja: 0,
      Media: 0,
      Alta: 0,
      Critica: 0
    }

    activeAlerts.value.forEach(alert => {
      summary[alert.severidad]++
    })

    return summary
  }

  /**
   * 🔍 Filter alerts by criteria
   */
  const filterAlerts = (criteria) => {
    let filtered = [...alerts.value]

    if (criteria.severity) {
      filtered = filtered.filter(alert => alert.severidad === criteria.severity)
    }

    if (criteria.type) {
      filtered = filtered.filter(alert => alert.tipoAlerta === criteria.type)
    }

    if (criteria.acknowledged !== undefined) {
      filtered = filtered.filter(alert => alert.reconocida === criteria.acknowledged)
    }

    if (criteria.roomId) {
      filtered = filtered.filter(alert => alert.locationId === criteria.roomId)
    }

    if (criteria.search) {
      const searchTerm = criteria.search.toLowerCase()
      filtered = filtered.filter(alert => 
        alert.articuloNombre.toLowerCase().includes(searchTerm) ||
        alert.locationName?.toLowerCase().includes(searchTerm) ||
        alert.mensaje.toLowerCase().includes(searchTerm)
      )
    }

    return filtered
  }

  /**
   * 🚨 Get highest priority alerts
   */
  const getHighPriorityAlerts = (limit = 5) => {
    const priorityOrder = { 'Critica': 4, 'Alta': 3, 'Media': 2, 'Baja': 1 }
    
    return activeAlerts.value
      .sort((a, b) => {
        // Sort by severity first, then by creation date
        const severityDiff = priorityOrder[b.severidad] - priorityOrder[a.severidad]
        if (severityDiff !== 0) return severityDiff
        
        return new Date(b.fechaCreacion) - new Date(a.fechaCreacion)
      })
      .slice(0, limit)
  }

  /**
   * 🔄 Start auto-refresh
   */
  const startAutoRefresh = () => {
    if (refreshTimer.value) return // Already running

    refreshTimer.value = setInterval(() => {
      fetchAlerts()
    }, refreshInterval)
  }

  /**
   * ⏹️ Stop auto-refresh
   */
  const stopAutoRefresh = () => {
    if (refreshTimer.value) {
      clearInterval(refreshTimer.value)
      refreshTimer.value = null
    }
  }

  /**
   * 🔄 Manual refresh
   */
  const refresh = () => {
    return fetchAlerts()
  }

  // Lifecycle
  onMounted(() => {
    fetchAlerts()
    if (autoRefresh) {
      startAutoRefresh()
    }
  })

  onUnmounted(() => {
    stopAutoRefresh()
  })

  return {
    // State
    loading,
    saving,
    error,
    alerts,

    // Computed
    activeAlerts,
    criticalAlerts,
    lowStockAlerts,
    outOfStockAlerts,
    alertsByRoom,
    alertCounts,

    // Methods
    fetchAlerts,
    acknowledgeAlert,
    acknowledgeMultipleAlerts,
    configureAlerts,
    getAlertsForRoom,
    getAlertSummary,
    filterAlerts,
    getHighPriorityAlerts,
    startAutoRefresh,
    stopAutoRefresh,
    refresh
  }
}

/**
 * Specialized composable for room-specific alerts
 */
export function useRoomAlerts(roomId) {
  const alertSystem = useInventoryAlerts({
    filterByRoom: roomId,
    autoRefresh: true,
    refreshInterval: 15000 // More frequent for room-specific monitoring
  })

  const roomNeedsAttention = computed(() => 
    alertSystem.activeAlerts.value.length > 0
  )

  const roomAlertSummary = computed(() => ({
    hasAlerts: alertSystem.activeAlerts.value.length > 0,
    criticalCount: alertSystem.criticalAlerts.value.length,
    lowStockCount: alertSystem.lowStockAlerts.value.length,
    outOfStockCount: alertSystem.outOfStockAlerts.value.length,
    lastUpdate: new Date().toISOString()
  }))

  return {
    ...alertSystem,
    roomNeedsAttention,
    roomAlertSummary
  }
}

/**
 * Global alert monitoring composable
 */
export function useGlobalAlerts() {
  const alertSystem = useInventoryAlerts({
    autoRefresh: true,
    refreshInterval: 60000 // Less frequent for global monitoring
  })

  const globalAlertSummary = computed(() => ({
    totalAlerts: alertSystem.alertCounts.value.total,
    criticalAlerts: alertSystem.alertCounts.value.critical,
    roomsAffected: alertSystem.alertCounts.value.roomsAffected,
    topAlertedRooms: alertSystem.alertsByRoom.value.slice(0, 3),
    needsImmediateAttention: alertSystem.criticalAlerts.value.length > 0
  }))

  return {
    ...alertSystem,
    globalAlertSummary
  }
}