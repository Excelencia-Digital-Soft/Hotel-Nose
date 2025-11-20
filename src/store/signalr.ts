import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import * as signalR from '@microsoft/signalr'
import type {
  SignalRConfig,
  SignalRNotification,
  NotificationTypeEnum,
  ConnectionStateEnum,
  ConnectionInfo,
  SignalREventHandlers,
  NotificationData,
} from '../types/signalr'

export const useSignalRStore = defineStore('signalr', () => {
  // State
  const connection = ref<signalR.HubConnection | null>(null)
  const connectionState = ref<ConnectionStateEnum>('disconnected')
  const connectionInfo = ref<ConnectionInfo | null>(null)
  const notifications = ref<SignalRNotification[]>([])
  const config = ref<SignalRConfig | null>(null)
  const eventHandlers = ref<SignalREventHandlers | null>(null)

  // Computed
  const isConnected = computed(() => connectionState.value === 'connected')
  const isConnecting = computed(() => connectionState.value === 'connecting')
  const isReconnecting = computed(() => connectionState.value === 'reconnecting')

  /**
   * Initialize SignalR connection with config
   */
  const initialize = async (initialConfig: SignalRConfig, handlers?: SignalREventHandlers): Promise<void> => {
    // Store config and handlers for reconnection
    config.value = initialConfig
    eventHandlers.value = handlers || null

    // Don't initialize if already connected or connecting
    if (isConnected.value || isConnecting.value) {
      console.log('SignalR already connected or connecting')
      return
    }

    await connect()
  }

  /**
   * Connect to SignalR Hub using stored config
   */
  const connect = async (): Promise<void> => {
    if (!config.value) {
      throw new Error('SignalR config not set. Call initialize() first.')
    }

    try {
      if (!config.value.accessToken) {
        throw new Error('Access token is required')
      }

      connectionState.value = 'connecting'
      addNotification('info', '🔌 Attempting to connect to SignalR Hub...')

      // 🔍 Debug token information
      const tokenDebug = config.value.accessToken.substring(0, 20) + '...'
      console.log(`🔐 [SignalR] Using token: ${tokenDebug}`)
      console.log(`🌐 [SignalR] Server URL: ${config.value.serverUrl}`)

      // Create connection with optimized configuration to avoid API blocking
      connection.value = new signalR.HubConnectionBuilder()
        .withUrl(config.value.serverUrl, {
          // Token factory for authentication with enhanced debugging
          accessTokenFactory: () => {
            const token = config.value!.accessToken
            const tokenPreview = token ? token.substring(0, 20) + '...' : 'No token'
            console.log(`🔐 [SignalR] AccessTokenFactory called - Token: ${tokenPreview}`)
            
            // Validate token format (JWT should have 3 parts)
            if (token) {
              const tokenParts = token.split('.')
              if (tokenParts.length !== 3) {
                console.error('🔐 [SignalR] Invalid JWT format - expected 3 parts, got:', tokenParts.length)
              } else {
                console.log('🔐 [SignalR] Token format validated - JWT has 3 parts')
              }
            }
            
            return token
          },
          
          // ✅ CRITICAL: Use WebSockets + SSE to avoid API blocking
          // DO NOT use LongPolling as it can block HTTP requests
          transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents,
          
          // Skip negotiation for better performance (false = negotiate first)
          skipNegotiation: false,
          
          // Additional headers if needed
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${config.value.accessToken}`
          }
        })
        .withAutomaticReconnect(config.value.automaticReconnect || [0, 2000, 10000, 30000])
        .configureLogging(config.value.logging ? signalR.LogLevel.Information : signalR.LogLevel.Error)
        .build()

      // Setup event handlers
      setupEventHandlers()

      // Start connection
      await connection.value.start()

      connectionState.value = 'connected'
      connectionInfo.value = {
        connectionId: connection.value.connectionId || '',
        state: 'connected',
        serverUrl: config.value.serverUrl,
      }

      // Log transport information for debugging
      const transport = (connection.value as any).transport?.name || 'Unknown'
      console.log(`🚀 [SignalR] Connected using transport: ${transport}`)
      
      addNotification(
        'success',
        `✅ Connected successfully! Connection ID: ${connection.value.connectionId} (${transport})`
      )

      // Auto-subscribe to institution if user is authenticated
      await autoSubscribeToInstitution()

    } catch (error) {
      connectionState.value = 'disconnected'
      const errorMessage = error instanceof Error ? error.message : 'Unknown error'
      addNotification('error', `❌ Connection failed: ${errorMessage}`)
      console.error('SignalR Connection error:', error)
      throw error
    }
  }

  /**
   * Disconnect from SignalR Hub
   */
  const disconnect = async (): Promise<void> => {
    if (connection.value) {
      try {
        connectionState.value = 'disconnecting'
        await connection.value.stop()
        connectionState.value = 'disconnected'
        connectionInfo.value = null
        addNotification('warning', '🔌 Disconnected from SignalR Hub')
      } catch (error) {
        const errorMessage = error instanceof Error ? error.message : 'Unknown error'
        addNotification('error', `❌ Error disconnecting: ${errorMessage}`)
        throw error
      }
    }
  }

  /**
   * Setup event handlers for SignalR connection
   */
  const setupEventHandlers = (): void => {
    if (!connection.value) return

    // Handle incoming notifications
    connection.value.on(
      'ReceiveNotification',
      (type: string, message: string, data?: NotificationData) => {
        const notificationType = mapNotificationType(type)
        addNotification(
          notificationType,
          `📢 NOTIFICATION [${type.toUpperCase()}]: ${message}`,
          data
        )

        // Show browser notification if supported
        showBrowserNotification(type, message)

        // Call custom handler if provided
        eventHandlers.value?.onReceiveNotification?.(type, message, data)
      }
    )

    // Handle subscription confirmations
    connection.value.on('SubscriptionConfirmed', (message: string) => {
      addNotification('success', `✅ SUBSCRIPTION: ${message}`)
      eventHandlers.value?.onSubscriptionConfirmed?.(message)
    })

    // Handle forced disconnection from server
    connection.value.on('forcedisconnect', (reason?: string) => {
      addNotification('warning', `⚠️ Server forced disconnection: ${reason || 'Unknown reason'}`)
      eventHandlers.value?.onForcedDisconnect?.(reason)
      // Automatically attempt to reconnect after forced disconnect
      setTimeout(() => {
        if (connectionState.value === 'disconnected') {
          connect().catch(console.error)
        }
      }, 2000)
    })

    // ✅ ROOM-SPECIFIC EVENTS
    // Handle room status changes (libre -> ocupada -> mantenimiento, etc.)
    connection.value.on('RoomStatusChanged', (data: any) => {
      console.log('🏨 [SignalR] RoomStatusChanged EVENT RECEIVED:', JSON.stringify(data, null, 2))
      console.log('🔍 [DEBUG] Browser window:', window.location.href)
      console.log('🔍 [DEBUG] Timestamp:', new Date().toISOString())
      addNotification('info', `🏨 Room ${data.roomId} status changed to: ${data.status}`)
      
      // Show visual toast for important changes
      import('../utils/toast.ts').then(({ showSignalRToast }) => {
        showSignalRToast('info', `Habitación ${data.roomId} cambió a ${data.status}`, data)
      })
      
      // Update rooms store with new status
      handleRoomStatusChange(data)
    })

    // Handle room reservation changes (created, finalized, cancelled)
    connection.value.on('RoomReservationChanged', (data: any) => {
      console.log('📝 [SignalR] RoomReservationChanged EVENT RECEIVED:', JSON.stringify(data, null, 2))
      addNotification('info', `📝 Room ${data.roomId} reservation ${data.action}`)
      
      // Show visual toast for reservation changes
      import('../utils/toast.ts').then(({ showSignalRToast }) => {
        const message = data.action === 'created' ? 'Nueva reserva creada' : 
                       data.action === 'finalized' ? 'Reserva finalizada' : 
                       'Cambio en reserva'
        showSignalRToast('info', `Habitación ${data.roomId}: ${message}`, data)
      })
      
      // Update rooms store with reservation change
      handleRoomReservationChange(data)
    })

    // Handle room progress updates (time elapsed, percentage, etc.)
    connection.value.on('RoomProgressUpdated', (data: any) => {
      // Only log every 10th update to avoid spam
      const shouldLog = Math.random() < 0.1
      if (shouldLog) {
        console.log('📊 [SignalR] RoomProgressUpdated (sampled):', data)
      }
      
      // Update rooms store with progress data
      handleRoomProgressUpdate(data)
    })

    // Handle room maintenance changes
    connection.value.on('RoomMaintenanceChanged', (data: any) => {
      console.log('🔧 [SignalR] RoomMaintenanceChanged EVENT RECEIVED:', JSON.stringify(data, null, 2))
      addNotification('warning', `🔧 Room ${data.roomId} maintenance ${data.status}`)
      
      // Show visual toast for maintenance changes
      import('../utils/toast.ts').then(({ showSignalRToast }) => {
        showSignalRToast('warning', `Habitación ${data.roomId}: Mantenimiento ${data.status}`, data)
      })
      
      // Update rooms store with maintenance info
      handleRoomMaintenanceChange(data)
    })

    // Handle connection lifecycle
    connection.value.onreconnecting((error?: Error) => {
      connectionState.value = 'reconnecting'
      const errorMsg = error ? ` ${error.message}` : ''
      addNotification('warning', `🔄 Connection lost. Attempting to reconnect...${errorMsg}`)
      eventHandlers.value?.onReconnecting?.(error)
    })

    connection.value.onreconnected((connectionId?: string) => {
      connectionState.value = 'connected'
      if (connectionInfo.value) {
        connectionInfo.value.connectionId = connectionId || ''
      }
      
      // Log transport information after reconnection
      const transport = (connection.value as any)?.transport?.name || 'Unknown'
      console.log(`🔄 [SignalR] Reconnected using transport: ${transport}`)
      
      addNotification('success', `🔄 Reconnected with connection ID: ${connectionId} (${transport})`)
      
      // Re-subscribe to institution after reconnection
      autoSubscribeToInstitution()
      
      eventHandlers.value?.onReconnected?.(connectionId || '')
    })

    connection.value.onclose((error?: Error) => {
      connectionState.value = 'disconnected'
      connectionInfo.value = null

      if (error) {
        addNotification('error', `❌ Connection closed with error: ${error.message}`)
      } else {
        addNotification('warning', '🔌 Connection closed')
      }
      eventHandlers.value?.onClose?.(error)
    })
  }

  /**
   * Auto-subscribe to institution based on user authentication
   */
  const autoSubscribeToInstitution = async (): Promise<void> => {
    try {
      // Import auth store dynamically to avoid circular dependencies
      const { useAuthStore } = await import('./auth')
      const authStore = useAuthStore()
      
      console.log('🏢 [SignalR] Auto-subscribing to institution...')
      console.log('🏢 [SignalR] Auth state:', {
        isAuthenticated: authStore.isAuthenticated,
        user: authStore.user,
        institutionId: authStore.user?.institutionId || authStore.institucionID
      })
      
      // Try multiple sources for institution ID
      const institutionId = authStore.user?.institutionId || authStore.institucionID || authStore.user?.InstitutionId
      
      if (authStore.isAuthenticated && institutionId) {
        console.log(`🏢 [SignalR] Subscribing to institution ID: ${institutionId}`)
        await subscribeToInstitution(institutionId)
      } else {
        console.warn('🏢 [SignalR] Cannot auto-subscribe - missing institution ID or not authenticated')
      }
    } catch (error) {
      console.warn('Could not auto-subscribe to institution:', error)
    }
  }

  /**
   * Subscribe to institution notifications
   */
  const subscribeToInstitution = async (institutionId: number): Promise<void> => {
    if (!isConnected.value || !connection.value) {
      throw new Error('Not connected to SignalR Hub')
    }

    if (!institutionId || institutionId <= 0) {
      throw new Error('Please provide a valid Institution ID')
    }

    try {
      addNotification('info', `🏢 Subscribing to institution ${institutionId}...`)
      await connection.value.invoke('SubscribeToInstitution', institutionId)

      if (connectionInfo.value) {
        connectionInfo.value.institutionId = institutionId
      }
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error'
      addNotification('error', `❌ Failed to subscribe to institution: ${errorMessage}`)
      throw error
    }
  }

  /**
   * Test connection with ping
   */
  const ping = async (): Promise<void> => {
    if (!isConnected.value || !connection.value) {
      throw new Error('Not connected to SignalR Hub')
    }

    try {
      addNotification('info', '🏓 Sending ping...')
      await connection.value.invoke('Ping')
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error'
      addNotification('error', `❌ Ping failed: ${errorMessage}`)
      throw error
    }
  }

  /**
   * Get connection information
   */
  const getConnectionInfo = async (): Promise<void> => {
    if (!isConnected.value || !connection.value) {
      throw new Error('Not connected to SignalR Hub')
    }

    try {
      addNotification('info', 'ℹ️ Requesting connection info...')
      await connection.value.invoke('GetConnectionInfo')
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error'
      addNotification('error', `❌ Failed to get connection info: ${errorMessage}`)
      throw error
    }
  }

  /**
   * Clear all notifications
   */
  const clearNotifications = (): void => {
    notifications.value = []
  }

  /**
   * Add notification to the list
   */
  const addNotification = (
    type: NotificationTypeEnum | string,
    message: string,
    data?: NotificationData
  ): void => {
    const notification: SignalRNotification = {
      type: typeof type === 'string' ? mapNotificationType(type) : type,
      message,
      data,
      timestamp: new Date(),
      id: `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`, // Unique ID
    }

    notifications.value.unshift(notification)

    // Keep only last 100 notifications to prevent memory issues
    if (notifications.value.length > 100) {
      notifications.value = notifications.value.slice(0, 100)
    }

    console.log(`[${notification.timestamp.toLocaleTimeString()}] ${message}`)
  }

  /**
   * Map string notification type to enum
   */
  const mapNotificationType = (type: string): NotificationTypeEnum => {
    switch (type.toLowerCase()) {
      case 'success':
        return 'success'
      case 'error':
        return 'error'
      case 'warning':
        return 'warning'
      case 'info':
      default:
        return 'info'
    }
  }

  /**
   * Show browser notification if supported
   */
  const showBrowserNotification = (type: string, message: string): void => {
    if ('Notification' in window && Notification.permission === 'granted') {
      const icon = getIconForNotificationType(type)
      new Notification(`Hotel Notification - ${type}`, {
        body: message,
        icon: `data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"><text y=".9em" font-size="90">${icon}</text></svg>`,
      })
    }
  }

  /**
   * Get emoji icon for notification type
   */
  const getIconForNotificationType = (type: string): string => {
    switch (type.toLowerCase()) {
      case 'success':
        return '✅'
      case 'error':
        return '❌'
      case 'warning':
        return '⚠️'
      case 'info':
      default:
        return 'ℹ️'
    }
  }

  /**
   * Request browser notification permission
   */
  const requestNotificationPermission = async (): Promise<NotificationPermission> => {
    if ('Notification' in window && Notification.permission === 'default') {
      const permission = await Notification.requestPermission()
      addNotification('info', `Browser notification permission: ${permission}`)
      return permission
    }
    return Notification.permission
  }

  /**
   * Update access token (useful when token expires)
   */
  const updateAccessToken = (newToken: string): void => {
    if (config.value) {
      config.value.accessToken = newToken
    }
  }

  /**
   * ROOM EVENT HANDLERS
   * These functions handle real-time room events from SignalR
   */
  
  const handleRoomStatusChange = (data: any): void => {
    console.log('🔧 [SignalR] handleRoomStatusChange called with:', data)
    try {
      // Import rooms store dynamically to avoid circular dependencies
      import('../store/modules/roomsStore').then(({ useRoomsStore }) => {
        const roomsStore = useRoomsStore()
        console.log('🔧 [SignalR] Calling roomsStore.updateRoomStatus with:', data)
        roomsStore.updateRoomStatus(data)
        console.log('🔧 [SignalR] roomsStore.updateRoomStatus completed')
      })
    } catch (error) {
      console.error('❌ [SignalR] Error handling room status change:', error)
    }
  }

  const handleRoomReservationChange = (data: any): void => {
    try {
      import('../store/modules/roomsStore').then(({ useRoomsStore }) => {
        const roomsStore = useRoomsStore()
        roomsStore.updateRoomReservation(data)
      })
    } catch (error) {
      console.error('Error handling room reservation change:', error)
    }
  }

  const handleRoomProgressUpdate = (data: any): void => {
    try {
      import('../store/modules/roomsStore').then(({ useRoomsStore }) => {
        const roomsStore = useRoomsStore()
        roomsStore.updateRoomProgress(data)
      })
    } catch (error) {
      console.error('Error handling room progress update:', error)
    }
  }

  const handleRoomMaintenanceChange = (data: any): void => {
    try {
      import('../store/modules/roomsStore').then(({ useRoomsStore }) => {
        const roomsStore = useRoomsStore()
        roomsStore.updateRoomMaintenance(data)
      })
    } catch (error) {
      console.error('Error handling room maintenance change:', error)
    }
  }

  /**
   * Subscribe to SignalR event
   */
  const on = (eventName: string, callback: (...args: any[]) => void): void => {
    if (!connection.value) {
      console.warn(`Cannot subscribe to ${eventName} - not connected`)
      return
    }

    connection.value.on(eventName, callback)
    console.log(`📡 [SignalR] Subscribed to event: ${eventName}`)
  }

  /**
   * Unsubscribe from SignalR event
   */
  const off = (eventName: string, callback?: (...args: any[]) => void): void => {
    if (!connection.value) {
      console.warn(`Cannot unsubscribe from ${eventName} - not connected`)
      return
    }

    if (callback) {
      connection.value.off(eventName, callback)
    } else {
      connection.value.off(eventName)
    }
    console.log(`📡 [SignalR] Unsubscribed from event: ${eventName}`)
  }

  /**
   * Invoke SignalR hub method
   */
  const invoke = async (methodName: string, ...args: any[]): Promise<any> => {
    if (!isConnected.value || !connection.value) {
      throw new Error(`Cannot invoke ${methodName} - not connected to SignalR Hub`)
    }

    try {
      console.log(`📡 [SignalR] Invoking method: ${methodName}`, args)
      const result = await connection.value.invoke(methodName, ...args)
      console.log(`📡 [SignalR] Method ${methodName} completed successfully`)
      return result
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error'
      console.error(`❌ [SignalR] Method ${methodName} failed:`, errorMessage)
      addNotification('error', `❌ Method ${methodName} failed: ${errorMessage}`)
      throw error
    }
  }


  // Auto-request notification permission
  if (typeof window !== 'undefined') {
    requestNotificationPermission()
  }

  return {
    // State
    connection,
    connectionState,
    connectionInfo,
    notifications,
    config,
    
    // Computed
    isConnected,
    isConnecting,
    isReconnecting,
    
    // Methods
    initialize,
    connect,
    disconnect,
    subscribeToInstitution,
    ping,
    getConnectionInfo,
    clearNotifications,
    updateAccessToken,
    
    // Event handling
    on,
    off,
    invoke,
  }
})