import { ref, computed, onMounted, reactive } from 'vue'
import { roomInventoryService as RoomInventoryService } from '../services/roomInventoryService'
import type {
  RoomInventoryDto,
  RoomInventoryCreateDto,
  RoomInventoryUpdateDto,
  RoomInventorySummaryDto,
  InventoryAlertDto,
  RoomForInventoryDto,
  ArticleForRoomDto,
  InventoryMovementDto,
  RoomInventoryState,
  InventoryFilters,
  RoomInventoryFormData,
  InventoryAdjustmentForm,
  InventoryTransferDto
} from '../types'

/**
 * Room Inventory management composable
 * Handles room inventory state and operations
 */
export function useRoomInventory(initialRoomId?: number) {
  // State
  const state = reactive<RoomInventoryState>({
    inventories: [],
    selectedRoom: null,
    availableRooms: [],
    availableArticles: [],
    alerts: [],
    loading: false,
    saving: false,
    error: null
  })

  // Additional state
  const summaries = ref<RoomInventorySummaryDto[]>([])
  const movements = ref<InventoryMovementDto[]>([])
  const filters = ref<InventoryFilters>({})

  // Form state
  const inventoryForm = ref<RoomInventoryFormData>({
    habitacionId: initialRoomId || null,
    articuloId: null,
    cantidad: 0,
    cantidadMinima: 1,
    cantidadMaxima: 100
  })

  const adjustmentForm = ref<InventoryAdjustmentForm>({
    roomInventoryId: 0,
    nuevaCantidad: 0,
    motivo: '',
    tipoMovimiento: 'AJUSTE'
  })

  const transferForm = ref<InventoryTransferDto>({
    articuloId: 0,
    habitacionOrigenId: 0,
    habitacionDestinoId: 0,
    cantidad: 0,
    motivo: ''
  })

  // Computed properties
  const filteredInventories = computed(() => {
    let result = state.inventories

    if (filters.value.habitacionId) {
      result = result.filter(inv => inv.habitacionId === filters.value.habitacionId)
    }

    if (filters.value.categoria) {
      result = result.filter(inv => 
        inv.articulo?.categoria.toLowerCase().includes(filters.value.categoria!.toLowerCase())
      )
    }

    if (filters.value.bajoStock) {
      result = result.filter(inv => inv.cantidad <= inv.cantidadMinima)
    }

    if (filters.value.agotados) {
      result = result.filter(inv => inv.cantidad === 0)
    }

    if (filters.value.alertas) {
      const alertInventoryIds = state.alerts.map(alert => alert.roomInventoryId)
      result = result.filter(inv => alertInventoryIds.includes(inv.roomInventoryId))
    }

    if (filters.value.busqueda) {
      const search = filters.value.busqueda.toLowerCase()
      result = result.filter(inv =>
        inv.articulo?.nombreArticulo.toLowerCase().includes(search) ||
        inv.habitacion?.nombre.toLowerCase().includes(search) ||
        inv.habitacion?.numero.includes(search)
      )
    }

    return result
  })

  const inventoriesByRoom = computed(() => {
    const grouped: Record<number, RoomInventoryDto[]> = {}
    state.inventories.forEach(inventory => {
      if (!grouped[inventory.habitacionId]) {
        grouped[inventory.habitacionId] = []
      }
      grouped[inventory.habitacionId].push(inventory)
    })
    return grouped
  })

  const lowStockInventories = computed(() => {
    return state.inventories.filter(inv => inv.cantidad <= inv.cantidadMinima && inv.cantidad > 0)
  })

  const outOfStockInventories = computed(() => {
    return state.inventories.filter(inv => inv.cantidad === 0)
  })

  const totalInventoryValue = computed(() => {
    return state.inventories.reduce((total, inv) => {
      return total + (inv.cantidad * (inv.articulo?.precio || 0))
    }, 0)
  })

  const criticalAlerts = computed(() => {
    return state.alerts.filter(alert => alert.prioridad === 'CRITICAL' || alert.prioridad === 'HIGH')
  })

  // Methods
  const loadInventories = async (habitacionId?: number) => {
    state.loading = true
    state.error = null

    try {
      const response = await RoomInventoryService.getAllInventories(habitacionId)
      if (response.isSuccess && response.data) {
        state.inventories = Array.isArray(response.data) ? response.data : []
      } else {
        state.error = response.message || 'Error loading inventories'
        state.inventories = []
      }
    } catch (error) {
      console.error('Error loading inventories:', error)
      state.error = 'Error de conexión al cargar inventarios'
      state.inventories = []
    } finally {
      state.loading = false
    }
  }

  const loadRoomInventories = async (habitacionId: number) => {
    state.loading = true
    state.error = null

    try {
      const response = await RoomInventoryService.getInventoriesByRoom(habitacionId)
      if (response.isSuccess && response.data) {
        state.inventories = Array.isArray(response.data) ? response.data : []
        // Update selected room if needed
        if (!state.selectedRoom || state.selectedRoom.habitacionId !== habitacionId) {
          const room = state.availableRooms.find(r => r.habitacionId === habitacionId)
          if (room) {
            state.selectedRoom = room
          }
        }
      } else {
        state.error = response.message || 'Error loading room inventories'
        state.inventories = []
      }
    } catch (error) {
      console.error('Error loading room inventories:', error)
      state.error = 'Error de conexión al cargar inventarios de la habitación'
      state.inventories = []
    } finally {
      state.loading = false
    }
  }

  const loadAvailableRooms = async () => {
    try {
      const response = await RoomInventoryService.getAvailableRooms()
      if (response.isSuccess && response.data) {
        state.availableRooms = Array.isArray(response.data) ? response.data : []
      }
    } catch (error) {
      console.error('Error loading available rooms:', error)
    }
  }

  const loadAvailableArticles = async () => {
    try {
      const response = await RoomInventoryService.getAvailableArticles()
      if (response.isSuccess && response.data) {
        state.availableArticles = Array.isArray(response.data) ? response.data : []
      }
    } catch (error) {
      console.error('Error loading available articles:', error)
    }
  }

  const loadArticlesForRoom = async (habitacionId: number) => {
    try {
      const response = await RoomInventoryService.getArticlesForRoom(habitacionId)
      if (response.isSuccess && response.data) {
        state.availableArticles = Array.isArray(response.data) ? response.data : []
      }
    } catch (error) {
      console.error('Error loading articles for room:', error)
    }
  }

  const loadInventoryAlerts = async (habitacionId?: number) => {
    try {
      const response = await RoomInventoryService.getInventoryAlerts(habitacionId)
      if (response.isSuccess && response.data) {
        state.alerts = Array.isArray(response.data) ? response.data : []
      }
    } catch (error) {
      console.error('Error loading inventory alerts:', error)
    }
  }

  const loadInventorySummaries = async (habitacionId?: number) => {
    try {
      const response = await RoomInventoryService.getRoomInventorySummary(habitacionId)
      if (response.isSuccess && response.data) {
        summaries.value = Array.isArray(response.data) ? response.data : []
      }
    } catch (error) {
      console.error('Error loading inventory summaries:', error)
    }
  }

  // CRUD Operations
  const createInventory = async (data: RoomInventoryCreateDto): Promise<boolean> => {
    state.saving = true
    state.error = null

    try {
      const response = await RoomInventoryService.createInventory(data)
      if (response.isSuccess && response.data) {
        state.inventories.push(response.data)
        return true
      } else {
        state.error = response.message || 'Error creating inventory'
        return false
      }
    } catch (error) {
      console.error('Error creating inventory:', error)
      state.error = 'Error de conexión al crear inventario'
      return false
    } finally {
      state.saving = false
    }
  }

  const updateInventory = async (roomInventoryId: number, data: RoomInventoryUpdateDto): Promise<boolean> => {
    state.saving = true
    state.error = null

    try {
      const response = await RoomInventoryService.updateInventory(roomInventoryId, data)
      if (response.isSuccess && response.data) {
        const index = state.inventories.findIndex(inv => inv.roomInventoryId === roomInventoryId)
        if (index !== -1) {
          state.inventories[index] = response.data
        }
        return true
      } else {
        state.error = response.message || 'Error updating inventory'
        return false
      }
    } catch (error) {
      console.error('Error updating inventory:', error)
      state.error = 'Error de conexión al actualizar inventario'
      return false
    } finally {
      state.saving = false
    }
  }

  const deleteInventory = async (roomInventoryId: number): Promise<boolean> => {
    state.saving = true
    state.error = null

    try {
      const response = await RoomInventoryService.deleteInventory(roomInventoryId)
      if (response.isSuccess) {
        state.inventories = state.inventories.filter(inv => inv.roomInventoryId !== roomInventoryId)
        return true
      } else {
        state.error = response.message || 'Error deleting inventory'
        return false
      }
    } catch (error) {
      console.error('Error deleting inventory:', error)
      state.error = 'Error de conexión al eliminar inventario'
      return false
    } finally {
      state.saving = false
    }
  }

  // Inventory Adjustments
  const adjustInventoryQuantity = async (
    roomInventoryId: number,
    nuevaCantidad: number,
    motivo: string
  ): Promise<boolean> => {
    state.saving = true
    state.error = null

    try {
      const response = await RoomInventoryService.adjustInventoryQuantity(roomInventoryId, nuevaCantidad, motivo)
      if (response.isSuccess && response.data) {
        const index = state.inventories.findIndex(inv => inv.roomInventoryId === roomInventoryId)
        if (index !== -1) {
          state.inventories[index] = response.data
        }
        return true
      } else {
        state.error = response.message || 'Error adjusting inventory'
        return false
      }
    } catch (error) {
      console.error('Error adjusting inventory:', error)
      state.error = 'Error de conexión al ajustar inventario'
      return false
    } finally {
      state.saving = false
    }
  }

  const increaseInventory = async (
    roomInventoryId: number,
    cantidad: number,
    motivo: string
  ): Promise<boolean> => {
    state.saving = true
    state.error = null

    try {
      const response = await RoomInventoryService.increaseInventory(roomInventoryId, cantidad, motivo)
      if (response.isSuccess && response.data) {
        const index = state.inventories.findIndex(inv => inv.roomInventoryId === roomInventoryId)
        if (index !== -1) {
          state.inventories[index] = response.data
        }
        return true
      } else {
        state.error = response.message || 'Error increasing inventory'
        return false
      }
    } catch (error) {
      console.error('Error increasing inventory:', error)
      state.error = 'Error de conexión al aumentar inventario'
      return false
    } finally {
      state.saving = false
    }
  }

  const decreaseInventory = async (
    roomInventoryId: number,
    cantidad: number,
    motivo: string
  ): Promise<boolean> => {
    state.saving = true
    state.error = null

    try {
      const response = await RoomInventoryService.decreaseInventory(roomInventoryId, cantidad, motivo)
      if (response.isSuccess && response.data) {
        const index = state.inventories.findIndex(inv => inv.roomInventoryId === roomInventoryId)
        if (index !== -1) {
          state.inventories[index] = response.data
        }
        return true
      } else {
        state.error = response.message || 'Error decreasing inventory'
        return false
      }
    } catch (error) {
      console.error('Error decreasing inventory:', error)
      state.error = 'Error de conexión al disminuir inventario'
      return false
    } finally {
      state.saving = false
    }
  }

  // Inventory Transfers
  const transferInventory = async (data: InventoryTransferDto): Promise<boolean> => {
    state.saving = true
    state.error = null

    try {
      const response = await RoomInventoryService.transferInventory(data)
      if (response.isSuccess && response.data) {
        // Update both origin and destination inventories
        const origenIndex = state.inventories.findIndex(inv => 
          inv.habitacionId === data.habitacionOrigenId && inv.articuloId === data.articuloId
        )
        const destinoIndex = state.inventories.findIndex(inv => 
          inv.habitacionId === data.habitacionDestinoId && inv.articuloId === data.articuloId
        )

        if (origenIndex !== -1) {
          state.inventories[origenIndex] = response.data.origen
        }
        if (destinoIndex !== -1) {
          state.inventories[destinoIndex] = response.data.destino
        } else {
          // Add new inventory item if it didn't exist in destination
          state.inventories.push(response.data.destino)
        }

        return true
      } else {
        state.error = response.message || 'Error transferring inventory'
        return false
      }
    } catch (error) {
      console.error('Error transferring inventory:', error)
      state.error = 'Error de conexión al transferir inventario'
      return false
    } finally {
      state.saving = false
    }
  }

  // Utility methods
  const setSelectedRoom = (room: RoomForInventoryDto | null) => {
    state.selectedRoom = room
    if (room) {
      loadRoomInventories(room.habitacionId)
      loadArticlesForRoom(room.habitacionId)
    }
  }

  const setFilters = (newFilters: Partial<InventoryFilters>) => {
    filters.value = { ...filters.value, ...newFilters }
  }

  const clearFilters = () => {
    filters.value = {}
  }

  const getInventoryStatus = (inventory: RoomInventoryDto): 'OK' | 'LOW_STOCK' | 'OUT_OF_STOCK' | 'OVERSTOCKED' => {
    return RoomInventoryService.calculateStockStatus(
      inventory.cantidad,
      inventory.cantidadMinima,
      inventory.cantidadMaxima
    )
  }

  const refreshData = async () => {
    await Promise.all([
      loadInventories(),
      loadAvailableRooms(),
      loadAvailableArticles(),
      loadInventoryAlerts(),
      loadInventorySummaries()
    ])
  }

  const clearError = () => {
    state.error = null
  }

  // Initialize data on mount
  onMounted(() => {
    refreshData()
    if (initialRoomId) {
      loadRoomInventories(initialRoomId)
    }
  })

  return {
    // State
    inventories: computed(() => state.inventories),
    selectedRoom: computed(() => state.selectedRoom),
    availableRooms: computed(() => state.availableRooms),
    availableArticles: computed(() => state.availableArticles),
    alerts: computed(() => state.alerts),
    loading: computed(() => state.loading),
    saving: computed(() => state.saving),
    error: computed(() => state.error),

    // Additional state
    summaries,
    movements,
    filters,

    // Form state
    inventoryForm,
    adjustmentForm,
    transferForm,

    // Computed
    filteredInventories,
    inventoriesByRoom,
    lowStockInventories,
    outOfStockInventories,
    totalInventoryValue,
    criticalAlerts,

    // Methods
    loadInventories,
    loadRoomInventories,
    loadAvailableRooms,
    loadAvailableArticles,
    loadArticlesForRoom,
    loadInventoryAlerts,
    loadInventorySummaries,

    // CRUD operations
    createInventory,
    updateInventory,
    deleteInventory,

    // Inventory adjustments
    adjustInventoryQuantity,
    increaseInventory,
    decreaseInventory,

    // Transfers
    transferInventory,

    // Utility methods
    setSelectedRoom,
    setFilters,
    clearFilters,
    getInventoryStatus,
    refreshData,
    clearError
  }
}