/**
 * useRoomSignalR - Composable for real-time room updates via SignalR
 * 
 * This composable handles real-time updates for room status, occupancy, 
 * time progress, and other room-related events through SignalR.
 */

import { ref, onMounted, onUnmounted, computed } from 'vue'
import { useSignalRStore } from '../../store/signalr'
import { useRoomsStore } from '../../store/modules/roomsStore'

// Types for SignalR room events
interface RoomStatusChangedPayload {
  roomId: number
  status: 'libre' | 'ocupada' | 'mantenimiento' | 'limpieza'
  visitaId?: number
  timestamp: string
  usuarioId?: number
}

interface RoomProgressUpdatedPayload {
  roomId: number
  visitaId: number
  startTime: string
  currentTime: string
  progressPercentage: number
  timeElapsed: string
  estimatedEndTime?: string
}

interface RoomReservationChangedPayload {
  roomId: number
  reservaId?: number
  visitaId?: number
  action: 'created' | 'updated' | 'cancelled' | 'finalized'
  timestamp: string
}

interface RoomMaintenanceChangedPayload {
  roomId: number
  maintenanceType: string
  status: 'started' | 'completed' | 'cancelled'
  description?: string
  timestamp: string
}

export function useRoomSignalR() {
  // Stores
  const signalRStore = useSignalRStore()
  const roomsStore = useRoomsStore()
  
  // Reactive state
  const isConnected = computed(() => signalRStore.isConnected)
  const connectionStatus = computed(() => signalRStore.connectionState)
  const lastUpdateTimestamp = ref<Date | null>(null)
  const activeUpdates = ref<Set<number>>(new Set()) // Track rooms being updated
  
  // Event counters for debugging
  const eventStats = ref({
    statusChanges: 0,
    progressUpdates: 0,
    reservationChanges: 0,
    maintenanceChanges: 0
  })

  /**
   * Handle room status changes (libre -> ocupada, etc.)
   */
  const handleRoomStatusChanged = (payload: RoomStatusChangedPayload) => {
    
    try {
      activeUpdates.value.add(payload.roomId)
      eventStats.value.statusChanges++
      lastUpdateTimestamp.value = new Date()

      // Update room in store
      roomsStore.updateRoomStatus({
        roomId: payload.roomId,
        status: payload.status,
        visitaId: payload.visitaId,
        timestamp: payload.timestamp
      })

      // Remove from active updates after a delay to show visual feedback
      setTimeout(() => {
        activeUpdates.value.delete(payload.roomId)
      }, 2000)

    } catch (error) {
      console.error('❌ [SignalR] Error handling room status change:', error)
    }
  }

  /**
   * Handle room time progress updates
   */
  const handleRoomProgressUpdated = (payload: RoomProgressUpdatedPayload) => {
    
    try {
      // ⚠️ BACKEND BUG WORKAROUND: Filter out historical reservations
      // Backend is sending completed reservations from months ago (Dec 2024, Jan 2025)
      // which causes extreme negative time calculations
      const startTime = new Date(payload.startTime)
      const now = new Date()
      const hoursDifference = (now.getTime() - startTime.getTime()) / (1000 * 60 * 60)
      
      // Ignore updates older than 48 hours (likely historical data)
      if (hoursDifference > 48) {
        console.warn(`⚠️ [SignalR] Ignoring historical room progress update from ${startTime.toLocaleDateString()} for room ${payload.roomId} (${Math.round(hoursDifference)} hours ago)`)
        return
      }
      
      eventStats.value.progressUpdates++
      lastUpdateTimestamp.value = new Date()

      // Update room progress in store
      roomsStore.updateRoomProgress({
        roomId: payload.roomId,
        visitaId: payload.visitaId,
        progressPercentage: payload.progressPercentage,
        timeElapsed: payload.timeElapsed,
        startTime: payload.startTime,
        estimatedEndTime: payload.estimatedEndTime
      })

    } catch (error) {
      console.error('❌ [SignalR] Error handling room progress update:', error)
    }
  }

  /**
   * Handle reservation changes
   */
  const handleRoomReservationChanged = (payload: RoomReservationChangedPayload) => {
    
    try {
      activeUpdates.value.add(payload.roomId)
      eventStats.value.reservationChanges++
      lastUpdateTimestamp.value = new Date()

      // Update reservation in store
      roomsStore.updateRoomReservation({
        roomId: payload.roomId,
        reservaId: payload.reservaId,
        visitaId: payload.visitaId,
        action: payload.action,
        timestamp: payload.timestamp
      })

      // Remove from active updates
      setTimeout(() => {
        activeUpdates.value.delete(payload.roomId)
      }, 2000)

    } catch (error) {
      console.error('❌ [SignalR] Error handling room reservation change:', error)
    }
  }

  /**
   * Handle maintenance status changes
   */
  const handleRoomMaintenanceChanged = (payload: RoomMaintenanceChangedPayload) => {
    
    try {
      activeUpdates.value.add(payload.roomId)
      eventStats.value.maintenanceChanges++
      lastUpdateTimestamp.value = new Date()

      // Update maintenance status in store
      roomsStore.updateRoomMaintenance({
        roomId: payload.roomId,
        maintenanceType: payload.maintenanceType,
        status: payload.status,
        description: payload.description,
        timestamp: payload.timestamp
      })

      // Remove from active updates
      setTimeout(() => {
        activeUpdates.value.delete(payload.roomId)
      }, 1500)

    } catch (error) {
      console.error('❌ [SignalR] Error handling room maintenance change:', error)
    }
  }

  /**
   * Subscribe to room-specific SignalR events
   */
  const subscribeToRoomEvents = () => {
    
    // Check connection state directly
    if (signalRStore.connectionState !== 'connected') {
      return false
    }

    try {
      
      // Subscribe to room status changes
      signalRStore.on('RoomStatusChanged', handleRoomStatusChanged)
      
      // Subscribe to room progress updates  
      signalRStore.on('RoomProgressUpdated', handleRoomProgressUpdated)
      
      // Subscribe to reservation changes
      signalRStore.on('RoomReservationChanged', handleRoomReservationChanged)
      
      // Subscribe to maintenance changes
      signalRStore.on('RoomMaintenanceChanged', handleRoomMaintenanceChanged)

      return true
      
    } catch (error) {
      console.error('❌ [SignalR] Error subscribing to room events:', error)
      return false
    }
  }

  /**
   * Unsubscribe from room-specific SignalR events
   */
  const unsubscribeFromRoomEvents = () => {
    try {
      signalRStore.off('RoomStatusChanged', handleRoomStatusChanged)
      signalRStore.off('RoomProgressUpdated', handleRoomProgressUpdated)
      signalRStore.off('RoomReservationChanged', handleRoomReservationChanged)
      signalRStore.off('RoomMaintenanceChanged', handleRoomMaintenanceChanged)
      
      
    } catch (error) {
      console.error('❌ [SignalR] Error unsubscribing from room events:', error)
    }
  }

  /**
   * Request real-time progress updates for a specific room
   */
  const requestRoomProgressUpdates = async (roomId: number, enable: boolean = true) => {
    if (signalRStore.connectionState !== 'connected') {
      return false
    }

    try {
      await signalRStore.invoke('SubscribeToRoomProgress', roomId, enable)
      return true
      
    } catch (error) {
      console.error('❌ [SignalR] Error requesting room progress updates:', error)
      return false
    }
  }

  /**
   * Join room group for targeted updates
   */
  const joinRoomGroup = async (roomId: number) => {
    if (signalRStore.connectionState !== 'connected') {
      return false
    }

    try {
      await signalRStore.invoke('JoinRoomGroup', roomId)
      return true
      
    } catch (error) {
      console.error('❌ [SignalR] Error joining room group:', error)
      return false
    }
  }

  /**
   * Leave room group
   */
  const leaveRoomGroup = async (roomId: number) => {
    if (signalRStore.connectionState !== 'connected') return false

    try {
      await signalRStore.invoke('LeaveRoomGroup', roomId)
      return true
      
    } catch (error) {
      console.error('❌ [SignalR] Error leaving room group:', error)
      return false
    }
  }

  /**
   * Check if a room is currently being updated
   */
  const isRoomUpdating = (roomId: number): boolean => {
    return activeUpdates.value.has(roomId)
  }

  // Lifecycle management
  onMounted(() => {
    
    // Expose handler for testing
    if (typeof window !== 'undefined') {
      (window as any).signalRTestHandler = handleRoomStatusChanged
    }
    
    // Subscribe immediately if already connected
    if (signalRStore.connectionState === 'connected') {
      const success = subscribeToRoomEvents()
    } else {
    }
    
    // Watch for connection changes
    const unwatch = signalRStore.$subscribe((mutation, state) => {
      
      // Check connection state directly since isConnected might be a getter
      if (state.connectionState === 'connected') {
        const success = subscribeToRoomEvents()
      } else if (state.connectionState === 'disconnected' || state.connectionState === 'reconnecting') {
      }
    })
    
    // Store unwatch function for cleanup
    return () => unwatch()
  })

  onUnmounted(() => {
    unsubscribeFromRoomEvents()
    activeUpdates.value.clear()
  })

  // Return reactive interface
  return {
    // Connection state
    isConnected,
    connectionStatus,
    lastUpdateTimestamp,
    eventStats,
    
    // Room update tracking
    activeUpdates,
    isRoomUpdating,
    
    // Event management
    subscribeToRoomEvents,
    unsubscribeFromRoomEvents,
    
    // Room-specific operations
    requestRoomProgressUpdates,
    joinRoomGroup,
    leaveRoomGroup
  }
}