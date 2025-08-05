import { ref, computed, onMounted, onUnmounted, type Ref, type ComputedRef } from 'vue'
import { inventoryService } from '../services/roomInventoryService'
import { InventoryLocationType } from '../types'
import type { InventoryAlertDto, ApiResponse } from '../types'

// Types for the composable
interface AlertOptions {
  autoRefresh?: boolean
  refreshInterval?: number
  filterByRoom?: number | null
  alertTypes?: AlertType[]
}

interface AlertFilterCriteria {
  severity?: AlertSeverity
  type?: AlertType
  acknowledged?: boolean
  roomId?: number
  search?: string
}

// API parameter interfaces for better type safety
interface FetchAlertsParams {
  soloNoReconocidas?: boolean
  tipoUbicacion?: InventoryLocationType
  ubicacionId?: number
  tipoAlerta?: AlertType[]
  severidad?: AlertSeverity[]
  articuloId?: number
  fechaDesde?: string
  fechaHasta?: string
  limite?: number
  pagina?: number
}

interface AcknowledgeAlertParams {
  comentarios?: string
  resolverAlerta?: boolean
  fechaReconocimiento?: string
}

interface AlertConfigurationParams {
  inventarioId: number
  umbralStockBajo: number
  umbralStockCritico: number
  alertasStockBajoActivas: boolean
  notificacionEmailActiva: boolean
}

interface AlertConfig {
  lowStockThreshold?: number
  criticalStockThreshold?: number
  enableLowStockAlerts?: boolean
  enableEmailNotifications?: boolean
}

interface RoomAlertGroup {
  roomId: number
  roomName: string
  alerts: InventoryAlertDto[]
}

interface AlertCounts {
  total: number
  critical: number
  lowStock: number
  outOfStock: number
  roomsAffected: number
}

interface AlertSummary {
  Baja: number
  Media: number
  Alta: number
  Critica: number
}

interface RoomAlertSummary {
  hasAlerts: boolean
  criticalCount: number
  lowStockCount: number
  outOfStockCount: number
  lastUpdate: string
}

interface GlobalAlertSummary {
  totalAlerts: number
  criticalAlerts: number
  roomsAffected: number
  topAlertedRooms: RoomAlertGroup[]
  needsImmediateAttention: boolean
}

interface BatchAcknowledgeResult {
  successful: number
  failed: number
}

type AlertType = 'StockBajo' | 'StockCritico' | 'StockAgotado'
type AlertSeverity = 'Baja' | 'Media' | 'Alta' | 'Critica'

interface UseInventoryAlertsReturn {
  // State
  loading: Ref<boolean>
  saving: Ref<boolean>
  error: Ref<string | null>
  alerts: Ref<InventoryAlertDto[]>
  
  // Computed
  activeAlerts: ComputedRef<InventoryAlertDto[]>
  criticalAlerts: ComputedRef<InventoryAlertDto[]>
  lowStockAlerts: ComputedRef<InventoryAlertDto[]>
  outOfStockAlerts: ComputedRef<InventoryAlertDto[]>
  alertsByRoom: ComputedRef<RoomAlertGroup[]>
  alertCounts: ComputedRef<AlertCounts>
  
  // Methods
  fetchAlerts: (filters?: Partial<FetchAlertsParams>) => Promise<void>
  acknowledgeAlert: (alertId: number, comments?: string) => Promise<any>
  acknowledgeMultipleAlerts: (alertIds: number[], comments?: string) => Promise<BatchAcknowledgeResult>
  configureAlerts: (inventoryId: number, config: AlertConfig) => Promise<any>
  getAlertsForRoom: (roomId: number) => InventoryAlertDto[]
  getAlertSummary: () => AlertSummary
  filterAlerts: (criteria: AlertFilterCriteria) => InventoryAlertDto[]
  getHighPriorityAlerts: (limit?: number) => InventoryAlertDto[]
  startAutoRefresh: () => void
  stopAutoRefresh: () => void
  refresh: () => Promise<void>
}

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
export function useInventoryAlerts(options: AlertOptions = {}): UseInventoryAlertsReturn {
  const {
    autoRefresh = true,
    refreshInterval = 30000, // 30 seconds
    filterByRoom = null,
    alertTypes = ['StockBajo', 'StockCritico', 'StockAgotado']
  } = options

  // State
  const loading: Ref<boolean> = ref(false)
  const saving: Ref<boolean> = ref(false)
  const error: Ref<string | null> = ref(null)
  const alerts: Ref<InventoryAlertDto[]> = ref([])
  const refreshTimer: Ref<ReturnType<typeof setInterval> | null> = ref(null)

  // Computed
  const activeAlerts: ComputedRef<InventoryAlertDto[]> = computed(() => 
    alerts.value.filter(alert => !alert.reconocida)
  )

  const criticalAlerts: ComputedRef<InventoryAlertDto[]> = computed(() =>
    activeAlerts.value.filter(alert => 
      alert.severidad === 'Critica' || alert.tipoAlerta === 'StockAgotado'
    )
  )

  const lowStockAlerts: ComputedRef<InventoryAlertDto[]> = computed(() =>
    activeAlerts.value.filter(alert => alert.tipoAlerta === 'StockBajo')
  )

  const outOfStockAlerts: ComputedRef<InventoryAlertDto[]> = computed(() =>
    activeAlerts.value.filter(alert => alert.tipoAlerta === 'StockAgotado')
  )

  const alertsByRoom: ComputedRef<RoomAlertGroup[]> = computed(() => {
    const roomMap = new Map<number, RoomAlertGroup>()
    
    activeAlerts.value.forEach(alert => {
      if (alert.tipoUbicacion === InventoryLocationType.Room && alert.ubicacionId) {
        if (!roomMap.has(alert.ubicacionId)) {
          roomMap.set(alert.ubicacionId, {
            roomId: alert.ubicacionId,
            roomName: alert.ubicacionNombre || `Habitación ${alert.ubicacionId}`,
            alerts: []
          })
        }
        roomMap.get(alert.ubicacionId)!.alerts.push(alert)
      }
    })
    
    return Array.from(roomMap.values())
      .sort((a, b) => b.alerts.length - a.alerts.length) // Sort by alert count
  })

  const alertCounts: ComputedRef<AlertCounts> = computed(() => ({
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
  const fetchAlerts = async (filters: Partial<FetchAlertsParams> = {}): Promise<void> => {
    try {
      loading.value = true
      error.value = null

      const params: FetchAlertsParams = {
        soloNoReconocidas: false,
        ...filters
      }

      // Filter by room if specified
      if (filterByRoom) {
        params.tipoUbicacion = InventoryLocationType.Room
        // Note: The API might need room filtering capability
      }

      const response: ApiResponse<InventoryAlertDto[]> = await inventoryService.getActiveAlerts(params as Record<string, any>)
      
      if (response.isSuccess && response.data) {
        // Ensure response.data is an array
        const alertsData = Array.isArray(response.data) ? response.data : []
        
        // Filter by alert types
        alerts.value = alertsData.filter(alert => 
          alertTypes.includes(alert.tipoAlerta as AlertType)
        )

        // Additional client-side filtering by room if needed
        if (filterByRoom) {
          alerts.value = alerts.value.filter(alert => 
            alert.ubicacionId === filterByRoom
          )
        }
      } else {
        // Handle case where response is successful but data is null/undefined
        if (response.isSuccess && !response.data) {
          alerts.value = []
        } else {
          throw new Error(response.message || 'Error al cargar alertas')
        }
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error desconocido'
      error.value = errorMessage
      console.error('Error fetching alerts:', err)
    } finally {
      loading.value = false
    }
  }

  /**
   * ✅ Acknowledge an alert
   */
  const acknowledgeAlert = async (alertId: number, comments: string = ''): Promise<any> => {
    try {
      saving.value = true
      error.value = null

      const params: AcknowledgeAlertParams = {
        comentarios: comments,
        resolverAlerta: true
      }

      const response: ApiResponse<any> = await inventoryService.acknowledgeAlert(alertId, params)
      
      if (response.isSuccess) {
        // Update local state
        const alert = alerts.value.find(a => a.alertaId === alertId)
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
      const errorMessage = err instanceof Error ? err.message : 'Error desconocido'
      error.value = errorMessage
      throw err
    } finally {
      saving.value = false
    }
  }

  /**
   * 🔄 Acknowledge multiple alerts
   */
  const acknowledgeMultipleAlerts = async (
    alertIds: number[], 
    comments: string = ''
  ): Promise<BatchAcknowledgeResult> => {
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
      const errorMessage = err instanceof Error ? err.message : 'Error desconocido'
      error.value = errorMessage
      throw err
    } finally {
      saving.value = false
    }
  }

  /**
   * ⚙️ Configure alert thresholds for inventory item
   */
  const configureAlerts = async (inventoryId: number, config: AlertConfig): Promise<any> => {
    try {
      saving.value = true
      error.value = null

      const params: AlertConfigurationParams = {
        inventarioId: inventoryId,
        umbralStockBajo: config.lowStockThreshold || 5,
        umbralStockCritico: config.criticalStockThreshold || 1,
        alertasStockBajoActivas: config.enableLowStockAlerts !== false,
        notificacionEmailActiva: config.enableEmailNotifications !== false
      }

      const response: ApiResponse<any> = await inventoryService.configureAlerts(params)
      
      if (response.isSuccess) {
        await fetchAlerts() // Refresh to get updated alerts
        return response.data
      } else {
        throw new Error(response.message || 'Error al configurar alertas')
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error desconocido'
      error.value = errorMessage
      throw err
    } finally {
      saving.value = false
    }
  }

  /**
   * 🎯 Get alerts for specific room
   */
  const getAlertsForRoom = (roomId: number): InventoryAlertDto[] => {
    return alerts.value.filter(alert => 
      alert.tipoUbicacion === InventoryLocationType.Room && 
      alert.ubicacionId === roomId
    )
  }

  /**
   * 📊 Get alert summary by severity
   */
  const getAlertSummary = (): AlertSummary => {
    const summary: AlertSummary = {
      Baja: 0,
      Media: 0,
      Alta: 0,
      Critica: 0
    }

    activeAlerts.value.forEach(alert => {
      if (alert.severidad in summary) {
        summary[alert.severidad as AlertSeverity]++
      }
    })

    return summary
  }

  /**
   * 🔍 Filter alerts by criteria
   */
  const filterAlerts = (criteria: AlertFilterCriteria): InventoryAlertDto[] => {
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
      filtered = filtered.filter(alert => alert.ubicacionId === criteria.roomId)
    }

    if (criteria.search) {
      const searchTerm = criteria.search.toLowerCase()
      filtered = filtered.filter(alert => 
        alert.articuloNombre.toLowerCase().includes(searchTerm) ||
        alert.ubicacionNombre?.toLowerCase().includes(searchTerm) ||
        alert.mensaje.toLowerCase().includes(searchTerm)
      )
    }

    return filtered
  }

  /**
   * 🚨 Get highest priority alerts
   */
  const getHighPriorityAlerts = (limit: number = 5): InventoryAlertDto[] => {
    const priorityOrder: Record<AlertSeverity, number> = { 
      'Critica': 4, 'Alta': 3, 'Media': 2, 'Baja': 1 
    }
    
    return activeAlerts.value
      .sort((a, b) => {
        // Sort by severity first, then by creation date
        const severityDiff = priorityOrder[b.severidad as AlertSeverity] - priorityOrder[a.severidad as AlertSeverity]
        if (severityDiff !== 0) return severityDiff
        
        return new Date(b.fechaCreacion).getTime() - new Date(a.fechaCreacion).getTime()
      })
      .slice(0, limit)
  }

  /**
   * 🔄 Start auto-refresh
   */
  const startAutoRefresh = (): void => {
    if (refreshTimer.value) return // Already running

    refreshTimer.value = setInterval(() => {
      fetchAlerts()
    }, refreshInterval)
  }

  /**
   * ⏹️ Stop auto-refresh
   */
  const stopAutoRefresh = (): void => {
    if (refreshTimer.value) {
      clearInterval(refreshTimer.value)
      refreshTimer.value = null
    }
  }

  /**
   * 🔄 Manual refresh
   */
  const refresh = (): Promise<void> => {
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
export function useRoomAlerts(roomId: number) {
  const alertSystem = useInventoryAlerts({
    filterByRoom: roomId,
    autoRefresh: true,
    refreshInterval: 15000 // More frequent for room-specific monitoring
  })

  const roomNeedsAttention: ComputedRef<boolean> = computed(() => 
    alertSystem.activeAlerts.value.length > 0
  )

  const roomAlertSummary: ComputedRef<RoomAlertSummary> = computed(() => ({
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

  const globalAlertSummary: ComputedRef<GlobalAlertSummary> = computed(() => ({
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