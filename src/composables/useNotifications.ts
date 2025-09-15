/**
 * useNotifications - Vue 3 Composable for SignalR Notification Service
 * 
 * Features:
 * - Reactive notification state management
 * - Automatic connection handling with auth store integration
 * - Type-safe notification operations
 * - Computed properties for filtering and statistics
 * - Lifecycle management with proper cleanup
 * - Error handling and retry logic
 * - Compatible with existing glassmorphism UI system
 */

import { ref, computed, onMounted, onUnmounted, watch, readonly } from 'vue'
import type { Ref } from 'vue'
import { useAuthStore } from '../store/auth'
import { NotificationService } from '../services/NotificationService'
import type {
  ClientNotification,
  ConnectionState,
  NotificationFilter,
  NotificationStats,
  NotificationCategory,
  NotificationSeverity,
  UseNotificationsReturn,
  NotificationEventCallbacks,
  NotificationServiceConfig
} from '../types/signalr'

/**
 * Vue 3 composable for notification management
 * 
 * @param config Optional service configuration
 * @param autoConnect Whether to auto-connect when auth state changes (default: true)
 *                    NOTE: Only use autoConnect=true in ONE place! Use useSignalRAutoConnect() instead.
 * @returns Reactive notification state and operations
 */
export function useNotifications(
  config: Partial<NotificationServiceConfig> = {},
  autoConnect: boolean = true
): UseNotificationsReturn {
  // Generate unique subscriber ID for this composable instance
  const subscriberId = `useNotifications_${Date.now()}_${Math.random().toString(36).substr(2, 9)}_${performance.now()}`
  
  // Reactive state
  const notifications = ref<ClientNotification[]>([])
  const connectionState = ref<ConnectionState>({
    status: 'disconnected',
    reconnectAttempts: 0
  })

  // Get auth store
  const authStore = useAuthStore()

  // Use singleton instance - never create multiple instances
  const service = NotificationService.getInstance(config)

  // Computed properties
  const unreadNotifications = computed(() => 
    notifications.value.filter(n => !n.isRead && !n.isDismissed)
  )

  const notificationsByCategory = computed(() => {
    const grouped: Record<NotificationCategory, ClientNotification[]> = {
      room_status: [],
      reservation: [],
      payment: [],
      inventory: [],
      system: [],
      consumption: [],
      checkout: [],
      maintenance: [],
      alert: [],
      general: []
    }

    notifications.value.forEach(notification => {
      if (grouped[notification.category]) {
        grouped[notification.category].push(notification)
      }
    })

    return grouped
  })

  const isConnected = computed(() => 
    connectionState.value.status === 'connected'
  )

  const stats = computed<NotificationStats>(() => 
    service.getStats()
  )

  // Event callbacks
  let eventCallbacks: NotificationEventCallbacks = {}

  /**
   * Connect to the notification service
   */
  const connect = async (): Promise<void> => {
    if (!authStore.isLoggedIn || isConnected.value) {
      return
    }

    try {
      await service.connect(
        authStore.token, 
        authStore.institucionID?.toString()
      )
      
      // Sync local state with service
      syncStateWithService()
    } catch (error) {
      console.error('[useNotifications] Connection failed:', error)
      throw error
    }
  }

  /**
   * Disconnect from the notification service
   */
  const disconnect = async (): Promise<void> => {
    try {
      await service.disconnect()
      
      // Clear local state
      notifications.value = []
      connectionState.value = {
        status: 'disconnected',
        reconnectAttempts: 0
      }
    } catch (error) {
      console.error('[useNotifications] Disconnect failed:', error)
      throw error
    }
  }

  /**
   * Mark notification as read
   */
  const markAsRead = async (notificationId: string): Promise<void> => {
    try {
      await service.markAsRead(notificationId)
      
      // Update local state
      const notification = notifications.value.find(n => 
        n.id === notificationId || n.clientId === notificationId
      )
      if (notification) {
        notification.isRead = true
      }
    } catch (error) {
      console.error('[useNotifications] Mark as read failed:', error)
      throw error
    }
  }

  /**
   * Dismiss notification
   */
  const dismissNotification = async (notificationId: string): Promise<void> => {
    try {
      await service.dismissNotification(notificationId)
      
      // Remove from local state
      const index = notifications.value.findIndex(n => 
        n.id === notificationId || n.clientId === notificationId
      )
      if (index !== -1) {
        notifications.value.splice(index, 1)
      }
    } catch (error) {
      console.error('[useNotifications] Dismiss notification failed:', error)
      throw error
    }
  }

  /**
   * Clear all notifications
   */
  const clearAllNotifications = (): void => {
    service.clearAllNotifications()
    notifications.value = []
  }

  /**
   * Filter notifications based on criteria
   */
  const filterNotifications = (filter: NotificationFilter): ClientNotification[] => {
    return service.filterNotifications(filter)
  }

  /**
   * Subscribe to institution notifications
   */
  const subscribeToInstitution = async (institutionId: string): Promise<void> => {
    try {
      await service.subscribeToInstitution(institutionId)
    } catch (error) {
      console.error('[useNotifications] Subscribe to institution failed:', error)
      throw error
    }
  }

  /**
   * Get notification history from server
   */
  const getHistory = async (): Promise<void> => {
    try {
      const history = await service.getNotificationHistory()
      
      // Convert history to client notifications and merge with existing
      const clientNotifications = history.map(data => ({
        ...data,
        clientId: `history_${data.id || Date.now()}_${Math.random()}`,
        receivedAt: data.timestamp ? new Date(data.timestamp).getTime() : Date.now(),
        isRead: true, // Assume history items are read
        isDismissed: false
      }))
      
      // Merge with existing notifications, avoiding duplicates
      const existingIds = new Set(notifications.value.map(n => n.id).filter(Boolean))
      const newNotifications = clientNotifications.filter(n => !existingIds.has(n.id))
      
      notifications.value = [...notifications.value, ...newNotifications]
    } catch (error) {
      console.error('[useNotifications] Get history failed:', error)
      throw error
    }
  }

  /**
   * Set notification received callback
   */
  const onNotificationReceived = (callback: (notification: ClientNotification) => void): void => {
    eventCallbacks.onNotificationReceived = callback
    // Don't call updateServiceCallbacks() here - it's called once in onMounted
  }

  /**
   * Set connection state change callback
   */
  const onConnectionStateChange = (callback: (state: ConnectionState) => void): void => {
    eventCallbacks.onConnectionStateChange = callback
    // Don't call updateServiceCallbacks() here - it's called once in onMounted
  }

  /**
   * Sync local reactive state with service state
   */
  const syncStateWithService = (): void => {
    notifications.value = service.getNotifications()
    connectionState.value = service.getConnectionState()
  }

  /**
   * Update service callbacks using new subscription system (called only once)
   */
  const updateServiceCallbacks = (): void => {
    const callbacks: NotificationEventCallbacks = {
      onNotificationReceived: (notification) => {
        // Update local state
        const existingIndex = notifications.value.findIndex(n => n.clientId === notification.clientId)
        if (existingIndex === -1) {
          notifications.value.unshift(notification)
        }
        
        // Call user callback (always get the latest one)
        eventCallbacks.onNotificationReceived?.(notification)
      },
      
      onNotificationRead: (notificationId) => {
        // Update local state
        const notification = notifications.value.find(n => 
          n.id === notificationId || n.clientId === notificationId
        )
        if (notification) {
          notification.isRead = true
        }
      },
      
      onNotificationDismissed: (notificationId) => {
        // Remove from local state
        const index = notifications.value.findIndex(n => 
          n.id === notificationId || n.clientId === notificationId
        )
        if (index !== -1) {
          notifications.value.splice(index, 1)
        }
      },
      
      onConnectionStateChange: (state) => {
        // Update local state
        connectionState.value = state
        
        // Call user callback (always get the latest one)
        eventCallbacks.onConnectionStateChange?.(state)
      },
      
      onError: (error) => {
        console.error('[useNotifications] Service error:', error)
        
        // Update connection state on error
        connectionState.value = {
          ...connectionState.value,
          status: 'error',
          error
        }
      }
    }

    service.subscribeToEvents(subscriberId, callbacks)
  }

  // Auto-connect logic
  if (autoConnect) {
    // Watch auth state changes
    watch(
      () => [authStore.isLoggedIn, authStore.institucionID],
      async ([isLoggedIn, institutionId]) => {
        if (isLoggedIn && institutionId) {
          try {
            await connect()
          } catch (error) {
            console.error('[useNotifications] Auto-connect failed:', error)
          }
        } else if (!isLoggedIn) {
          await disconnect()
        }
      },
      { immediate: true }
    )
  }

  // Setup service callbacks on mount
  onMounted(() => {
    console.log(`[useNotifications] Mounting instance: ${subscriberId}`)
    updateServiceCallbacks()
    syncStateWithService()
  })

  // Cleanup on unmount
  onUnmounted(() => {
    console.log(`[useNotifications] Unmounting instance: ${subscriberId}`)
    // Unsubscribe this instance's callbacks
    service.unsubscribeFromEvents(subscriberId)
  })

  // Return reactive interface
  return {
    // State
    notifications: readonly(notifications),
    connectionState: readonly(connectionState),
    stats: readonly(stats),
    
    // Computed
    unreadNotifications: readonly(unreadNotifications),
    notificationsByCategory: readonly(notificationsByCategory),
    isConnected: readonly(isConnected),
    
    // Actions
    connect,
    disconnect,
    markAsRead,
    dismissNotification,
    clearAllNotifications,
    filterNotifications,
    subscribeToInstitution,
    getHistory,
    
    // Event handlers
    onNotificationReceived,
    onConnectionStateChange
  }
}

/**
 * Helper composable for specific notification categories
 */
export function useNotificationsByCategory(category: NotificationCategory) {
  const { notifications, ...rest } = useNotifications(undefined, false) // Don't auto-connect
  
  const categoryNotifications = computed(() =>
    notifications.value.filter(n => n.category === category)
  )
  
  return {
    notifications: readonly(categoryNotifications),
    ...rest
  }
}

/**
 * Helper composable for room-specific notifications
 */
export function useRoomNotifications(roomId: Ref<number | undefined>) {
  const { notifications, ...rest } = useNotifications(undefined, false) // Don't auto-connect
  
  const roomNotifications = computed(() =>
    notifications.value.filter(n => n.roomId === roomId.value)
  )
  
  return {
    notifications: readonly(roomNotifications),
    ...rest
  }
}

/**
 * Helper composable for notification filtering with glassmorphism toast integration
 */
export function useNotificationToasts() {
  const { onNotificationReceived } = useNotifications(undefined, false) // Don't auto-connect here
  
  // Setup automatic toast display for new notifications
  onNotificationReceived((notification) => {
    // Import toast utility dynamically to avoid circular dependencies
    import('../utils/toast.js').then(({ showToast }) => {
      const toastType = notification.severity === 'error' ? 'error' :
                       notification.severity === 'warning' ? 'warn' :
                       notification.severity === 'success' ? 'success' : 'info'
      
      showToast(toastType, notification.title || notification.message, {
        life: notification.severity === 'error' ? 10000 : 5000,
        closable: true
      })
    }).catch(error => {
      console.error('[useNotificationToasts] Failed to show toast:', error)
    })
  })
}

/**
 * Helper to get notification service instance (for advanced usage)
 */
export function getNotificationService(): NotificationService {
  return NotificationService.getInstance()
}