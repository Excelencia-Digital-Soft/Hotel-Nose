import { ref, computed } from 'vue'
import { inventoryService } from '../services/roomInventoryService'
import { InventoryLocationType } from '../types'

/**
 * Composable for Inventory Transfer Management (V1 API)
 * Handles single and batch transfers between locations
 * 
 * Features:
 * - Single room transfers
 * - Batch transfers to multiple rooms
 * - Floor-wide reloads
 * - Transfer validation
 * - Progress tracking
 * - Error handling
 */
export function useInventoryTransfers() {
  // State
  const loading = ref(false)
  const transferring = ref(false)
  const error = ref(null)
  const transfers = ref([])
  const pendingTransfers = ref([])
  const transferProgress = ref({
    total: 0,
    completed: 0,
    errors: 0,
    current: null
  })

  // Computed
  const transfersInProgress = computed(() => 
    transfers.value.filter(t => t.estado === 'EN_PROCESO')
  )

  const completedTransfers = computed(() =>
    transfers.value.filter(t => t.estado === 'COMPLETADO')
  )

  const failedTransfers = computed(() =>
    transfers.value.filter(t => t.estado === 'FALLIDO')
  )

  const transferStats = computed(() => ({
    total: transfers.value.length,
    inProgress: transfersInProgress.value.length,
    completed: completedTransfers.value.length,
    failed: failedTransfers.value.length,
    successRate: transfers.value.length > 0 
      ? (completedTransfers.value.length / transfers.value.length * 100).toFixed(1)
      : 0
  }))

  // Methods

  /**
   * 🔄 Single transfer between locations
   */
  const createSingleTransfer = async (transferData) => {
    try {
      transferring.value = true
      error.value = null

      // Validate transfer data
      if (!transferData.detalles || transferData.detalles.length === 0) {
        throw new Error('No se han especificado artículos para transferir')
      }

      // Add transfer to tracking
      const transferId = Date.now()
      const transfer = {
        id: transferId,
        tipo: 'SIMPLE',
        estado: 'EN_PROCESO',
        fechaInicio: new Date().toISOString(),
        ...transferData
      }
      transfers.value.push(transfer)

      const response = await inventoryService.createTransfer(transferData)
      
      if (response.isSuccess) {
        // Update transfer status
        transfer.estado = 'COMPLETADO'
        transfer.fechaCompletado = new Date().toISOString()
        transfer.resultado = response.data
        
        return response.data
      } else {
        transfer.estado = 'FALLIDO'
        transfer.error = response.message
        throw new Error(response.message || 'Error al crear transferencia')
      }
    } catch (err) {
      error.value = err.message
      throw err
    } finally {
      transferring.value = false
    }
  }

  /**
   * ⚡ Batch transfer to multiple rooms
   */
  const createBatchTransfer = async (batchData) => {
    try {
      transferring.value = true
      error.value = null

      // Initialize progress tracking
      transferProgress.value = {
        total: batchData.transferencias.length,
        completed: 0,
        errors: 0,
        current: null
      }

      // Validate batch data
      if (!batchData.transferencias || batchData.transferencias.length === 0) {
        throw new Error('No se han especificado transferencias')
      }

      // Add batch transfer to tracking
      const batchId = Date.now()
      const batchTransfer = {
        id: batchId,
        tipo: 'LOTE',
        estado: 'EN_PROCESO',
        fechaInicio: new Date().toISOString(),
        transferencias: batchData.transferencias.length,
        ...batchData
      }
      transfers.value.push(batchTransfer)

      const response = await inventoryService.createBatchTransfer(batchData)
      
      if (response.isSuccess) {
        batchTransfer.estado = 'COMPLETADO'
        batchTransfer.fechaCompletado = new Date().toISOString()
        batchTransfer.resultado = response.data
        
        transferProgress.value.completed = transferProgress.value.total
        
        return response.data
      } else {
        batchTransfer.estado = 'FALLIDO'
        batchTransfer.error = response.message
        throw new Error(response.message || 'Error al crear transferencias en lote')
      }
    } catch (err) {
      error.value = err.message
      transferProgress.value.errors++
      throw err
    } finally {
      transferring.value = false
      transferProgress.value = { total: 0, completed: 0, errors: 0, current: null }
    }
  }

  /**
   * 🏨 Reload stock for multiple rooms (floor/section)
   */
  const reloadMultipleRooms = async (roomIds, stockItems, options = {}) => {
    const {
      motivo = 'Recarga masiva de habitaciones',
      prioridad = 'Media',
      requireAprobacion = false
    } = options

    try {
      transferring.value = true
      error.value = null

      // Validate inputs
      if (!roomIds || roomIds.length === 0) {
        throw new Error('No se han especificado habitaciones')
      }

      if (!stockItems || stockItems.length === 0) {
        throw new Error('No se han especificado artículos')
      }

      // Validate stock availability for all items
      const validationPromises = stockItems.map(item =>
        inventoryService.validateStock({
          articuloId: item.articuloId,
          cantidad: item.cantidad * roomIds.length, // Total needed for all rooms
          locationType: InventoryLocationType.General
        })
      )

      const validations = await Promise.all(validationPromises)
      const invalidItems = validations.filter(v => !v.data.isValid)

      if (invalidItems.length > 0) {
        const errorMessage = invalidItems.map(v => 
          `${v.data.articuloId}: disponible ${v.data.availableQuantity}, requerido ${v.data.requestedQuantity}`
        ).join(', ')
        throw new Error(`Stock insuficiente: ${errorMessage}`)
      }

      // Create batch transfer
      const transferencias = roomIds.map(roomId => ({
        tipoUbicacionOrigen: InventoryLocationType.General,
        tipoUbicacionDestino: InventoryLocationType.Room,
        ubicacionIdDestino: roomId,
        prioridad,
        motivo: `${motivo} - Habitación ${roomId}`,
        requireAprobacion,
        detalles: stockItems.map(item => ({
          inventarioId: validations.find(v => v.data.articuloId === item.articuloId).data.inventoryId,
          cantidadSolicitada: item.cantidad
        }))
      }))

      const batchData = {
        procesamientoAtomico: false, // Continue even if some fail
        transferencias
      }

      return await createBatchTransfer(batchData)
    } catch (err) {
      error.value = err.message
      throw err
    }
  }

  /**
   * 🏢 Reload stock by floor
   */
  const reloadFloor = async (floor, stockItems, options = {}) => {
    try {
      // Note: This would require a rooms service to get rooms by floor
      // For now, this is a template implementation
      
      const motivo = `Recarga piso ${floor}`
      
      // Mock: Get rooms for floor (would need actual API call)
      const roomsForFloor = [] // await RoomService.getRoomsByFloor(floor)
      
      if (roomsForFloor.length === 0) {
        throw new Error(`No se encontraron habitaciones en el piso ${floor}`)
      }

      const roomIds = roomsForFloor.map(room => room.habitacionId)
      
      return await reloadMultipleRooms(roomIds, stockItems, {
        ...options,
        motivo
      })
    } catch (err) {
      error.value = err.message
      throw err
    }
  }

  /**
   * 🎯 Smart reload based on alerts
   */
  const reloadBasedOnAlerts = async (alerts, defaultQuantities = {}) => {
    try {
      transferring.value = true
      error.value = null

      // Group alerts by room
      const alertsByRoom = new Map()
      
      alerts.forEach(alert => {
        if (alert.locationType === InventoryLocationType.Room && alert.locationId) {
          if (!alertsByRoom.has(alert.locationId)) {
            alertsByRoom.set(alert.locationId, [])
          }
          alertsByRoom.get(alert.locationId).push(alert)
        }
      })

      if (alertsByRoom.size === 0) {
        throw new Error('No hay alertas de habitaciones para procesar')
      }

      // Create transfers for each room
      const transferencias = []
      
      for (const [roomId, roomAlerts] of alertsByRoom) {
        const detalles = roomAlerts.map(alert => {
          const quantity = defaultQuantities[alert.articuloId] || 
                          alert.umbralStockBajo || 10 // Default quantity
          
          return {
            inventarioId: alert.inventoryId,
            cantidadSolicitada: quantity
          }
        })

        transferencias.push({
          tipoUbicacionOrigen: InventoryLocationType.General,
          tipoUbicacionDestino: InventoryLocationType.Room,
          ubicacionIdDestino: roomId,
          prioridad: 'Alta',
          motivo: `Recarga automática por alertas - Habitación ${roomId}`,
          requireAprobacion: false,
          detalles
        })
      }

      const batchData = {
        procesamientoAtomico: false,
        transferencias
      }

      return await createBatchTransfer(batchData)
    } catch (err) {
      error.value = err.message
      throw err
    } finally {
      transferring.value = false
    }
  }

  /**
   * 📋 Get pending transfers
   */
  const fetchPendingTransfers = async () => {
    try {
      loading.value = true
      error.value = null

      const response = await inventoryService.getPendingTransfers()
      
      if (response.isSuccess) {
        pendingTransfers.value = response.data
        return response.data
      } else {
        throw new Error(response.message || 'Error al cargar transferencias pendientes')
      }
    } catch (err) {
      error.value = err.message
      throw err
    } finally {
      loading.value = false
    }
  }

  /**
   * ✅ Approve transfer
   */
  const approveTransfer = async (transferId, comments = '') => {
    try {
      transferring.value = true
      error.value = null

      const response = await inventoryService.approveTransfer(transferId, {
        comentarios: comments
      })
      
      if (response.isSuccess) {
        // Update local state
        const transfer = pendingTransfers.value.find(t => t.id === transferId)
        if (transfer) {
          transfer.estado = 'APROBADO'
          transfer.comentarios = comments
        }
        
        return response.data
      } else {
        throw new Error(response.message || 'Error al aprobar transferencia')
      }
    } catch (err) {
      error.value = err.message
      throw err
    } finally {
      transferring.value = false
    }
  }

  /**
   * 🔍 Validate transfer feasibility
   */
  const validateTransfer = async (transferData) => {
    try {
      const validations = []
      
      for (const detalle of transferData.detalles) {
        const validation = await inventoryService.validateStock({
          articuloId: detalle.articuloId || detalle.inventarioId, // Support both formats
          cantidad: detalle.cantidadSolicitada,
          locationType: transferData.tipoUbicacionOrigen
        })
        
        validations.push({
          ...validation.data,
          detalle
        })
      }

      const isValid = validations.every(v => v.isValid)
      const errors = validations.filter(v => !v.isValid)
      
      return {
        isValid,
        validations,
        errors,
        summary: {
          totalItems: validations.length,
          validItems: validations.filter(v => v.isValid).length,
          invalidItems: errors.length
        }
      }
    } catch (err) {
      error.value = err.message
      throw err
    }
  }

  /**
   * 📊 Get transfer statistics
   */
  const getTransferStatistics = () => {
    const last24h = new Date(Date.now() - 24 * 60 * 60 * 1000)
    const recent = transfers.value.filter(t => 
      new Date(t.fechaInicio) > last24h
    )

    return {
      all: transferStats.value,
      recent: {
        total: recent.length,
        completed: recent.filter(t => t.estado === 'COMPLETADO').length,
        failed: recent.filter(t => t.estado === 'FALLIDO').length
      }
    }
  }

  /**
   * 🧹 Clear transfer history
   */
  const clearTransferHistory = () => {
    transfers.value = []
    transferProgress.value = { total: 0, completed: 0, errors: 0, current: null }
  }

  return {
    // State
    loading,
    transferring,
    error,
    transfers,
    pendingTransfers,
    transferProgress,

    // Computed
    transfersInProgress,
    completedTransfers,
    failedTransfers,
    transferStats,

    // Methods
    createSingleTransfer,
    createBatchTransfer,
    reloadMultipleRooms,
    reloadFloor,
    reloadBasedOnAlerts,
    fetchPendingTransfers,
    approveTransfer,
    validateTransfer,
    getTransferStatistics,
    clearTransferHistory
  }
}

/**
 * Specialized composable for room-to-room transfers
 */
export function useRoomToRoomTransfers() {
  const transferSystem = useInventoryTransfers()

  const transferBetweenRooms = async (fromRoomId, toRoomId, items) => {
    const transferData = {
      tipoUbicacionOrigen: InventoryLocationType.Room,
      ubicacionIdOrigen: fromRoomId,
      tipoUbicacionDestino: InventoryLocationType.Room,
      ubicacionIdDestino: toRoomId,
      prioridad: 'Media',
      motivo: `Transferencia entre habitaciones ${fromRoomId} → ${toRoomId}`,
      requireAprobacion: false,
      detalles: items
    }

    return await transferSystem.createSingleTransfer(transferData)
  }

  return {
    ...transferSystem,
    transferBetweenRooms
  }
}

/**
 * Emergency reload composable
 */
export function useEmergencyReload() {
  const transferSystem = useInventoryTransfers()

  const emergencyReload = async (roomIds, priority = 'Critica') => {
    // Standard emergency items with default quantities
    const emergencyItems = [
      { articuloId: 1, cantidad: 6 }, // Towels
      { articuloId: 2, cantidad: 3 }, // Soap
      { articuloId: 3, cantidad: 2 }, // Shampoo
      { articuloId: 4, cantidad: 4 }  // Toilet paper
    ]

    return await transferSystem.reloadMultipleRooms(roomIds, emergencyItems, {
      motivo: 'Recarga de emergencia',
      prioridad: priority,
      requireAprobacion: false
    })
  }

  return {
    ...transferSystem,
    emergencyReload
  }
}