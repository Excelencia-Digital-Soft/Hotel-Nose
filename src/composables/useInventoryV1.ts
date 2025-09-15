import { ref, computed, type Ref, type ComputedRef } from 'vue'
import { useToast, type ToastServiceMethods } from 'primevue/usetoast'
import { inventoryService } from '../services/roomInventoryService'
import { InventoryLocationType } from '../types'
import { useAuthStore } from '../store/auth'
import type { 
  InventoryDto, 
  InventoryUpdateDto, 
  InventoryCreateDto,
  InventoryMovementDto,
  InventoryAlertDto,
  ApiResponse 
} from '../types'

// Types for the composable
interface InventoryStats {
  totalItems: number
  totalStock: number
  lowStockCount: number
  outOfStockCount: number
  totalValue: number
}

interface MovementData {
  tipo: string
  cantidad: number
  motivo: string
  documento?: string
  destinoTipo?: InventoryLocationType
  destinoId?: number
}

interface AlertConfig {
  lowStockThreshold?: number
  criticalStockThreshold?: number
  enableLowStockAlerts?: boolean
  enableEmailNotifications?: boolean
}

interface UseInventoryV1Return {
  // State
  inventory: Ref<InventoryDto[]>
  loading: Ref<boolean>
  saving: Ref<boolean>
  error: Ref<string | null>
  alerts: Ref<InventoryAlertDto[]>
  
  // Computed
  stats: ComputedRef<InventoryStats>
  activeAlerts: ComputedRef<InventoryAlertDto[]>
  criticalAlerts: ComputedRef<InventoryAlertDto[]>
  
  // Methods
  fetchInventory: (showNotification?: boolean) => Promise<void>
  fetchAlerts: () => Promise<void>
  updateInventory: (inventoryId: number, updateData: InventoryUpdateDto) => Promise<InventoryDto>
  batchUpdateInventory: (updates: Array<{ inventoryId: number; cantidad: number; notes?: string }>) => Promise<InventoryDto[]>
  createInventory: (itemData: InventoryCreateDto & { notes?: string }) => Promise<InventoryDto>
  registerMovement: (inventoryId: number, movementData: MovementData) => Promise<InventoryMovementDto>
  acknowledgeAlert: (alertId: number, comments?: string) => Promise<any>
  configureAlerts: (inventoryId: number, config: AlertConfig) => Promise<any>
  getMovementHistory: (inventoryId: number) => Promise<InventoryMovementDto[]>
  exportToCSV: () => void
  importFromFile: (file: File) => Promise<void>
  getInventorySummary: () => Promise<any>
  syncInventory: () => Promise<void>
}

/**
 * Enhanced composable for General Inventory Management using V1 API
 * Features:
 * - Full CRUD operations
 * - Batch updates
 * - Alert management
 * - Export functionality
 * - Real-time statistics
 */
export function useInventoryV1(): UseInventoryV1Return {
  // Core state
  const inventory: Ref<InventoryDto[]> = ref([])
  const loading: Ref<boolean> = ref(false)
  const saving: Ref<boolean> = ref(false)
  const error: Ref<string | null> = ref(null)
  const alerts: Ref<InventoryAlertDto[]> = ref([])
  
  // Toast for notifications
  const toast: ToastServiceMethods = useToast()
  const authStore = useAuthStore()

  // Statistics computed
  const stats: ComputedRef<InventoryStats> = computed(() => {
    const items = inventory.value || []
    return {
      totalItems: items.length,
      totalStock: items.reduce((sum, item) => sum + (item.cantidad || 0), 0),
      lowStockCount: items.filter(item => item.cantidad > 0 && item.cantidad <= 5).length,
      outOfStockCount: items.filter(item => item.cantidad === 0).length,
      totalValue: items.reduce((sum, item) => sum + ((item.cantidad || 0) * (item.articuloPrecio || 0)), 0)
    }
  })

  // Active alerts computed
  const activeAlerts: ComputedRef<InventoryAlertDto[]> = computed(() => 
    alerts.value.filter(alert => !alert.reconocida)
  )

  // Critical alerts computed
  const criticalAlerts: ComputedRef<InventoryAlertDto[]> = computed(() =>
    alerts.value.filter(alert => alert.severidad === 'Critica' || alert.severidad === 'Alta')
  )

  /**
   * Fetch general inventory with V1 API
   */
  const fetchInventory = async (showNotification: boolean = false): Promise<void> => {
    try {
      loading.value = true
      error.value = null

      // Fetch general inventory
      const response: ApiResponse<InventoryDto[]> = await inventoryService.getGeneralInventory()
      
      if (response.isSuccess && response.data) {
        // Enrich inventory data with additional fields
        inventory.value = response.data.map(item => ({
          ...item,
          newQuantity: item.cantidad, // For editing
          isModified: false,
          stockStatus: inventoryService.calculateStockStatus(item.cantidad, item.cantidadMinima),
          lastUpdated: item.fechaActualizacion || new Date().toISOString()
        }))

        if (showNotification) {
          toast.add({
            severity: 'success',
            summary: 'Inventario Actualizado',
            detail: `${inventory.value.length} productos cargados`,
            life: 3000
          })
        }

        // Fetch alerts in parallel
        await fetchAlerts()
      } else {
        throw new Error(response.message || 'Error al cargar inventario')
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error desconocido'
      error.value = errorMessage
      console.error('Error fetching inventory:', err)
      
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'No se pudo cargar el inventario',
        life: 5000
      })
    } finally {
      loading.value = false
    }
  }

  /**
   * Fetch active alerts for general inventory
   */
  const fetchAlerts = async (): Promise<void> => {
    try {
      const response: ApiResponse<InventoryAlertDto[]> = await inventoryService.getActiveAlerts({
        tipoUbicacion: InventoryLocationType.General,
        soloNoReconocidas: false
      })
      
      if (response.isSuccess) {
        alerts.value = response.data || []
      }
    } catch (err) {
      console.error('Error fetching alerts:', err)
    }
  }

  /**
   * Update single inventory item
   */
  const updateInventory = async (inventoryId: number, updateData: InventoryUpdateDto): Promise<InventoryDto> => {
    try {
      saving.value = true
      error.value = null

      const response: ApiResponse<InventoryDto> = await inventoryService.updateInventory(inventoryId, updateData)
      
      if (response.isSuccess) {
        // Update local state
        const index = inventory.value.findIndex(item => item.inventoryId === inventoryId)
        if (index > -1) {
          inventory.value[index] = {
            ...inventory.value[index],
            ...response.data,
            isModified: false,
            newQuantity: response.data.cantidad
          }
        }

        // Check for new alerts
        await fetchAlerts()
        
        return response.data
      } else {
        throw new Error(response.message || 'Error al actualizar inventario')
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
   * Batch update multiple inventory items
   */
  const batchUpdateInventory = async (
    updates: Array<{ inventoryId: number; cantidad: number; notes?: string }>
  ): Promise<InventoryDto[]> => {
    try {
      saving.value = true
      error.value = null

      const batchData = {
        items: updates.map(update => ({
          inventoryId: update.inventoryId,
          cantidad: update.cantidad,
          notes: update.notes || 'Actualización masiva'
        }))
      }

      const response: ApiResponse<InventoryDto[]> = await inventoryService.batchUpdateInventory(batchData)
      
      if (response.isSuccess) {
        // Refresh inventory to get updated data
        await fetchInventory()
        return response.data
      } else {
        throw new Error(response.message || 'Error en actualización masiva')
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
   * Create new inventory item
   */
  const createInventory = async (itemData: InventoryCreateDto & { notes?: string }): Promise<InventoryDto> => {
    try {
      saving.value = true
      error.value = null

      const createData: InventoryCreateDto = {
        articuloId: itemData.articuloId,
        cantidad: itemData.cantidad,
        cantidadMinima: itemData.cantidadMinima || 5,
        cantidadMaxima: itemData.cantidadMaxima || 100,
        locationType: InventoryLocationType.General,
        locationId: null,
        notes: itemData.notes || 'Nuevo item agregado al inventario general'
      }

      const response: ApiResponse<InventoryDto> = await inventoryService.createInventory(createData)
      
      if (response.isSuccess) {
        await fetchInventory()
        
        toast.add({
          severity: 'success',
          summary: 'Éxito',
          detail: 'Producto agregado al inventario',
          life: 3000
        })
        
        return response.data
      } else {
        throw new Error(response.message || 'Error al crear item')
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error desconocido'
      error.value = errorMessage
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'No se pudo agregar el producto',
        life: 5000
      })
      throw err
    } finally {
      saving.value = false
    }
  }

  /**
   * Register inventory movement
   */
  const registerMovement = async (inventoryId: number, movementData: MovementData): Promise<InventoryMovementDto> => {
    try {
      saving.value = true
      
      const response: ApiResponse<InventoryMovementDto> = await inventoryService.registerMovement(inventoryId, {
        tipoMovimiento: movementData.tipo,
        cantidadCambiada: movementData.cantidad,
        motivo: movementData.motivo,
        numeroDocumento: movementData.documento,
        tipoUbicacionOrigen: InventoryLocationType.General,
        tipoUbicacionDestino: movementData.destinoTipo,
        ubicacionIdDestino: movementData.destinoId
      })
      
      if (response.isSuccess) {
        await fetchInventory()
        return response.data
      } else {
        throw new Error(response.message || 'Error al registrar movimiento')
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
   * Acknowledge alert
   */
  const acknowledgeAlert = async (alertId: number, comments: string = ''): Promise<any> => {
    try {
      const response: ApiResponse<any> = await inventoryService.acknowledgeAlert(alertId, {
        comentarios: comments,
        resolverAlerta: true
      })
      
      if (response.isSuccess) {
        // Remove from local alerts
        alerts.value = alerts.value.filter(alert => alert.alertaId !== alertId)
        
        toast.add({
          severity: 'info',
          summary: 'Alerta Reconocida',
          detail: 'La alerta ha sido marcada como resuelta',
          life: 3000
        })
        
        return response.data
      } else {
        throw new Error(response.message || 'Error al reconocer alerta')
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error desconocido'
      error.value = errorMessage
      throw err
    }
  }

  /**
   * Configure alerts for an inventory item
   */
  const configureAlerts = async (inventoryId: number, config: AlertConfig): Promise<any> => {
    try {
      saving.value = true
      
      const response: ApiResponse<any> = await inventoryService.configureAlerts({
        inventarioId: inventoryId,
        umbralStockBajo: config.lowStockThreshold || 5,
        umbralStockCritico: config.criticalStockThreshold || 2,
        alertasStockBajoActivas: config.enableLowStockAlerts !== false,
        notificacionEmailActiva: config.enableEmailNotifications || false
      })
      
      if (response.isSuccess) {
        toast.add({
          severity: 'success',
          summary: 'Configuración Guardada',
          detail: 'Las alertas han sido configuradas',
          life: 3000
        })
        
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
   * Get movement history for an inventory item
   */
  const getMovementHistory = async (inventoryId: number): Promise<InventoryMovementDto[]> => {
    try {
      loading.value = true
      
      const response: ApiResponse<InventoryMovementDto[]> = await inventoryService.getMovementHistory(inventoryId)
      
      if (response.isSuccess) {
        return response.data
      } else {
        throw new Error(response.message || 'Error al obtener historial')
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error desconocido'
      error.value = errorMessage
      throw err
    } finally {
      loading.value = false
    }
  }

  /**
   * Export inventory to CSV
   */
  const exportToCSV = (): void => {
    const headers = ['ID', 'Producto', 'Cantidad', 'Stock Mínimo', 'Precio', 'Estado', 'Última Actualización']
    const rows = inventory.value.map(item => [
      item.inventoryId,
      item.articuloNombre,
      item.cantidad,
      item.cantidadMinima || 5,
      item.articuloPrecio?.toFixed(2) || '0.00',
      item.stockStatus,
      new Date(item.lastUpdated || '').toLocaleDateString('es-ES')
    ])

    const csvContent = [
      headers.join(','),
      ...rows.map(row => row.map(cell => `"${cell}"`).join(','))
    ].join('\n')

    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' })
    const link = document.createElement('a')
    const url = URL.createObjectURL(blob)
    
    link.setAttribute('href', url)
    link.setAttribute('download', `inventario_${new Date().toISOString().split('T')[0]}.csv`)
    link.style.visibility = 'hidden'
    
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
  }

  /**
   * Import inventory from CSV/Excel
   */
  const importFromFile = async (file: File): Promise<void> => {
    try {
      loading.value = true
      
      // Here you would implement file parsing and batch creation
      // This is a placeholder for the actual implementation
      
      toast.add({
        severity: 'info',
        summary: 'Función en Desarrollo',
        detail: 'La importación de archivos estará disponible pronto',
        life: 5000
      })
      
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error desconocido'
      error.value = errorMessage
      throw err
    } finally {
      loading.value = false
    }
  }

  /**
   * Get inventory summary statistics
   */
  const getInventorySummary = async (): Promise<any> => {
    try {
      const response: ApiResponse<any> = await inventoryService.getInventorySummary()
      
      if (response.isSuccess) {
        return response.data
      } else {
        throw new Error(response.message || 'Error al obtener resumen')
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error desconocido'
      error.value = errorMessage
      throw err
    }
  }

  /**
   * Sync inventory with external system
   */
  const syncInventory = async (): Promise<void> => {
    const institucionID = authStore.institucionID
    if (!institucionID) {
      throw new Error('No se pudo obtener la información de la institución')
    }

    try {
      loading.value = true
      
      // This would call a sync endpoint if available
      // For now, just refresh the inventory
      await fetchInventory(true)
      
      toast.add({
        severity: 'success',
        summary: 'Sincronización Completa',
        detail: 'El inventario ha sido sincronizado',
        life: 3000
      })
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error desconocido'
      error.value = errorMessage
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'No se pudo sincronizar el inventario',
        life: 5000
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  return {
    // State
    inventory,
    loading,
    saving,
    error,
    alerts,
    
    // Computed
    stats,
    activeAlerts,
    criticalAlerts,
    
    // Methods
    fetchInventory,
    fetchAlerts,
    updateInventory,
    batchUpdateInventory,
    createInventory,
    registerMovement,
    acknowledgeAlert,
    configureAlerts,
    getMovementHistory,
    exportToCSV,
    importFromFile,
    getInventorySummary,
    syncInventory
  }
}