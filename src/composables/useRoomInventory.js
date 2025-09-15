import { ref, computed, watch } from 'vue'
import { inventoryService } from '../services/roomInventoryService'
import { InventoryLocationType } from '../types'

/**
 * Composable for Room Inventory Management (V1 API)
 * Based on: Guía Completa: Administración de Inventario por Habitación - API V1
 * 
 * Features:
 * - Room-specific inventory management
 * - General inventory viewing
 * - Stock transfers and reloads
 * - Alert monitoring
 * - Real-time stock validation
 */
export function useRoomInventory(roomId = null) {
  // State
  const loading = ref(false)
  const saving = ref(false)
  const error = ref(null)
  const roomInventory = ref([])
  const generalInventory = ref([])
  const combinedInventory = ref([])
  const alerts = ref([])
  const selectedRoom = ref(roomId)

  // Computed
  const hasLowStock = computed(() => 
    alerts.value.some(alert => 
      alert.tipoAlerta === 'StockBajo' || alert.tipoAlerta === 'StockCritico'
    )
  )

  const hasOutOfStock = computed(() =>
    alerts.value.some(alert => alert.tipoAlerta === 'StockAgotado')
  )

  const needsAttention = computed(() => 
    alerts.value.filter(alert => !alert.reconocida).length
  )

  const stockSummary = computed(() => ({
    totalItems: roomInventory.value.length,
    totalQuantity: roomInventory.value.reduce((sum, item) => sum + item.cantidad, 0),
    lowStockItems: roomInventory.value.filter(item => item.cantidad <= 5 && item.cantidad > 0).length,
    outOfStockItems: roomInventory.value.filter(item => item.cantidad === 0).length
  }))

  // Methods

  /**
   * 🔄 Load room inventory data
   */
  const fetchRoomInventory = async (forceRefresh = false) => {
    if (!selectedRoom.value) {
      error.value = 'No se ha seleccionado una habitación'
      return
    }

    try {
      loading.value = true
      error.value = null

      const response = await inventoryService.getRoomInventory(selectedRoom.value)
      
      if (response.isSuccess) {
        roomInventory.value = response.data.map(item => ({
          ...item,
          stockStatus: inventoryService.calculateStockStatus(item.cantidad)
        }))
      } else {
        throw new Error(response.message || 'Error al cargar inventario')
      }
    } catch (err) {
      error.value = err.message
      console.error('Error fetching room inventory:', err)
    } finally {
      loading.value = false
    }
  }

  /**
   * 📦 Load general inventory (hotel-wide)
   */
  const fetchGeneralInventory = async () => {
    try {
      loading.value = true
      error.value = null

      const response = await inventoryService.getGeneralInventory()
      
      if (response.isSuccess) {
        generalInventory.value = response.data
      } else {
        throw new Error(response.message || 'Error al cargar inventario general')
      }
    } catch (err) {
      error.value = err.message
      console.error('Error fetching general inventory:', err)
    } finally {
      loading.value = false
    }
  }

  /**
   * 🔗 Load combined view (room + general available)
   */
  const fetchCombinedInventory = async () => {
    if (!selectedRoom.value) return

    try {
      loading.value = true
      error.value = null

      const response = await inventoryService.getCombinedInventory(selectedRoom.value)
      
      if (response.isSuccess) {
        combinedInventory.value = response.data
      } else {
        throw new Error(response.message || 'Error al cargar vista combinada')
      }
    } catch (err) {
      error.value = err.message
      console.error('Error fetching combined inventory:', err)
    } finally {
      loading.value = false
    }
  }

  /**
   * 🚨 Load alerts for the room
   */
  const fetchAlerts = async () => {
    try {
      const response = await inventoryService.getActiveAlerts({
        tipoUbicacion: InventoryLocationType.Room,
        soloNoReconocidas: false
      })
      
      if (response.isSuccess) {
        // Filter alerts for the selected room
        alerts.value = selectedRoom.value 
          ? response.data.filter(alert => alert.locationId === selectedRoom.value)
          : response.data
      }
    } catch (err) {
      console.error('Error fetching alerts:', err)
    }
  }

  /**
   * ➕ Add inventory item to room
   */
  const addInventoryItem = async (itemData) => {
    if (!selectedRoom.value) {
      throw new Error('No se ha seleccionado una habitación')
    }

    try {
      saving.value = true
      error.value = null

      const createData = {
        articuloId: itemData.articuloId,
        cantidad: itemData.cantidad,
        locationType: InventoryLocationType.Room,
        locationId: selectedRoom.value,
        notes: itemData.notes || `Agregado a habitación ${selectedRoom.value}`
      }

      const response = await inventoryService.createInventory(createData)
      
      if (response.isSuccess) {
        await fetchRoomInventory()
        return response.data
      } else {
        throw new Error(response.message || 'Error al agregar item')
      }
    } catch (err) {
      error.value = err.message
      throw err
    } finally {
      saving.value = false
    }
  }

  /**
   * ✏️ Update inventory quantity
   */
  const updateInventoryQuantity = async (inventoryId, newQuantity, notes = '') => {
    try {
      saving.value = true
      error.value = null

      const updateData = {
        cantidad: newQuantity,
        notes: notes || `Actualización de cantidad a ${newQuantity}`
      }

      const response = await inventoryService.updateInventory(inventoryId, updateData)
      
      if (response.isSuccess) {
        await fetchRoomInventory()
        return response.data
      } else {
        throw new Error(response.message || 'Error al actualizar cantidad')
      }
    } catch (err) {
      error.value = err.message
      throw err
    } finally {
      saving.value = false
    }
  }

  /**
   * 🔄 Reload room stock from general inventory
   */
  const reloadRoomStock = async (articuloId, cantidad) => {
    if (!selectedRoom.value) {
      throw new Error('No se ha seleccionado una habitación')
    }

    try {
      saving.value = true
      error.value = null

      const response = await inventoryService.reloadRoomStock(
        selectedRoom.value,
        articuloId,
        cantidad
      )
      
      if (response.isSuccess) {
        await fetchRoomInventory()
        await fetchAlerts() // Refresh alerts after stock change
        return response.data
      } else {
        throw new Error(response.message || 'Error al recargar stock')
      }
    } catch (err) {
      error.value = err.message
      throw err
    } finally {
      saving.value = false
    }
  }

  /**
   * ⚡ Create transfer between locations
   */
  const createTransfer = async (transferData) => {
    try {
      saving.value = true
      error.value = null

      const response = await inventoryService.createTransfer(transferData)
      
      if (response.isSuccess) {
        await fetchRoomInventory()
        await fetchAlerts()
        return response.data
      } else {
        throw new Error(response.message || 'Error al crear transferencia')
      }
    } catch (err) {
      error.value = err.message
      throw err
    } finally {
      saving.value = false
    }
  }

  /**
   * ✅ Acknowledge alert
   */
  const acknowledgeAlert = async (alertId, comments = '') => {
    try {
      saving.value = true
      
      const response = await inventoryService.acknowledgeAlert(alertId, {
        comentarios: comments,
        resolverAlerta: true
      })
      
      if (response.isSuccess) {
        await fetchAlerts()
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
   * 🔍 Quick stock check for room
   */
  const checkRoomStock = async (roomIdToCheck = null) => {
    const targetRoomId = roomIdToCheck || selectedRoom.value
    if (!targetRoomId) {
      throw new Error('No se ha especificado una habitación')
    }

    try {
      const response = await inventoryService.checkRoomStock(targetRoomId)
      return response
    } catch (err) {
      error.value = err.message
      throw err
    }
  }

  /**
   * 📊 Get stock summary with alerts
   */
  const getStockSummary = async () => {
    if (!selectedRoom.value) return null

    try {
      const [inventoryResponse, alertsResponse] = await Promise.all([
        inventoryService.getRoomInventory(selectedRoom.value),
        inventoryService.getActiveAlerts({
          tipoUbicacion: InventoryLocationType.Room
        })
      ])

      const roomAlerts = alertsResponse.isSuccess 
        ? alertsResponse.data.filter(alert => alert.locationId === selectedRoom.value)
        : []

      return {
        inventory: inventoryResponse.isSuccess ? inventoryResponse.data : [],
        alerts: roomAlerts,
        summary: {
          totalItems: inventoryResponse.data?.length || 0,
          alertsCount: roomAlerts.length,
          lowStockCount: roomAlerts.filter(a => a.tipoAlerta === 'StockBajo').length,
          outOfStockCount: roomAlerts.filter(a => a.tipoAlerta === 'StockAgotado').length
        }
      }
    } catch (err) {
      error.value = err.message
      throw err
    }
  }

  /**
   * 🏠 Change selected room
   */
  const changeRoom = (newRoomId) => {
    selectedRoom.value = newRoomId
    if (newRoomId) {
      fetchRoomInventory()
      fetchAlerts()
    }
  }

  /**
   * 🔄 Refresh all data
   */
  const refreshAll = async () => {
    if (selectedRoom.value) {
      await Promise.all([
        fetchRoomInventory(),
        fetchAlerts()
      ])
    }
  }

  // Watch for room changes
  watch(selectedRoom, (newRoomId) => {
    if (newRoomId) {
      fetchRoomInventory()
      fetchAlerts()
    }
  })

  // Auto-load data if room is provided
  if (roomId) {
    fetchRoomInventory()
    fetchAlerts()
  }

  return {
    // State
    loading,
    saving,
    error,
    roomInventory,
    generalInventory,
    combinedInventory,
    alerts,
    selectedRoom,

    // Computed
    hasLowStock,
    hasOutOfStock,
    needsAttention,
    stockSummary,

    // Methods
    fetchRoomInventory,
    fetchGeneralInventory,
    fetchCombinedInventory,
    fetchAlerts,
    addInventoryItem,
    updateInventoryQuantity,
    reloadRoomStock,
    createTransfer,
    acknowledgeAlert,
    checkRoomStock,
    getStockSummary,
    changeRoom,
    refreshAll
  }
}

/**
 * Specialized composable for managing general inventory
 */
export function useGeneralInventory() {
  const loading = ref(false)
  const saving = ref(false)
  const error = ref(null)
  const inventory = ref([])

  const fetchInventory = async () => {
    try {
      loading.value = true
      error.value = null

      const response = await inventoryService.getGeneralInventory()
      
      if (response.isSuccess) {
        inventory.value = response.data.map(item => ({
          ...item,
          stockStatus: inventoryService.calculateStockStatus(item.cantidad)
        }))
      } else {
        throw new Error(response.message || 'Error al cargar inventario general')
      }
    } catch (err) {
      error.value = err.message
      console.error('Error fetching general inventory:', err)
    } finally {
      loading.value = false
    }
  }

  const batchUpdateInventory = async (updates) => {
    try {
      saving.value = true
      error.value = null

      const response = await inventoryService.batchUpdateInventory({ items: updates })
      
      if (response.isSuccess) {
        await fetchInventory()
        return response.data
      } else {
        throw new Error(response.message || 'Error al actualizar inventario')
      }
    } catch (err) {
      error.value = err.message
      throw err
    } finally {
      saving.value = false
    }
  }

  return {
    loading,
    saving,
    error,
    inventory,
    fetchInventory,
    batchUpdateInventory
  }
}