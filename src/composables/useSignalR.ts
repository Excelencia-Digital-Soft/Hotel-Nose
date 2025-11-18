import { ref, computed, onUnmounted, type Ref } from 'vue'
import * as signalR from '@microsoft/signalr'
import type {
  SignalRConfig,
  SignalRNotification,
  NotificationTypeEnum,
  ConnectionStateEnum,
  ConnectionInfo,
  SignalREventHandlers,
  SignalRComposableReturn,
  NotificationData,
} from '../types/signalr'

export function useSignalR(): SignalRComposableReturn {
  // State
  const connection = ref<signalR.HubConnection | null>(null)
  const connectionState = ref<ConnectionStateEnum>('disconnected' as ConnectionStateEnum)
  const connectionInfo = ref<ConnectionInfo | null>(null)
  const notifications = ref<SignalRNotification[]>([])

  // Computed
  const isConnected = computed(() => connectionState.value === 'connected')

  /**
   * Connect to SignalR Hub
   */
  const connect = async (config: SignalRConfig): Promise<void> => {
    try {
      if (!config.accessToken) {
        throw new Error('Access token is required')
      }

      connectionState.value = 'connecting' as ConnectionStateEnum
      addNotification('info', '🔌 Attempting to connect to SignalR Hub...')

      // Create connection
      connection.value = new signalR.HubConnectionBuilder()
        .withUrl(config.serverUrl, {
          accessTokenFactory: () => config.accessToken,
        })
        .withAutomaticReconnect(config.automaticReconnect || [0, 2000, 5000, 10000, 30000])
        .configureLogging(config.logging ? signalR.LogLevel.Information : signalR.LogLevel.Error)
        .build()

      // Setup default event handlers
      setupEventHandlers()

      // Start connection
      await connection.value.start()

      connectionState.value = 'connected' as ConnectionStateEnum
      connectionInfo.value = {
        connectionId: connection.value.connectionId || '',
        state: 'connected' as ConnectionStateEnum,
        serverUrl: config.serverUrl,
      }

      addNotification(
        'success',
        `✅ Connected successfully! Connection ID: ${connection.value.connectionId}`
      )
    } catch (error) {
      connectionState.value = 'disconnected' as ConnectionStateEnum
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
        connectionState.value = 'disconnecting' as ConnectionStateEnum

        await connection.value.stop()

        connectionState.value = 'disconnected' as ConnectionStateEnum
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
   * Setup default event handlers for SignalR connection
   */
  const setupEventHandlers = (customHandlers?: SignalREventHandlers): void => {
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
        customHandlers?.onReceiveNotification?.(type, message, data)
      }
    )

    // Handle subscription confirmations
    connection.value.on('SubscriptionConfirmed', (message: string) => {
      addNotification('success', `✅ SUBSCRIPTION: ${message}`)
      customHandlers?.onSubscriptionConfirmed?.(message)
    })

    // Handle connection lifecycle
    connection.value.onreconnecting((error?: Error) => {
      connectionState.value = 'reconnecting' as ConnectionStateEnum
      const errorMsg = error ? ` ${error.message}` : ''
      addNotification('warning', `🔄 Connection lost. Attempting to reconnect...${errorMsg}`)
      customHandlers?.onReconnecting?.(error)
    })

    connection.value.onreconnected((connectionId?: string) => {
      connectionState.value = 'connected' as ConnectionStateEnum
      if (connectionInfo.value) {
        connectionInfo.value.connectionId = connectionId || ''
      }
      addNotification('success', `🔄 Reconnected with connection ID: ${connectionId}`)
      customHandlers?.onReconnected?.(connectionId || '')
    })

    connection.value.onclose((error?: Error) => {
      connectionState.value = 'disconnected' as ConnectionStateEnum
      connectionInfo.value = null

      if (error) {
        addNotification('error', `❌ Connection closed with error: ${error.message}`)
      } else {
        addNotification('warning', '🔌 Connection closed')
      }
      customHandlers?.onClose?.(error)
    })
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
    }

    notifications.value.unshift(notification)

    // Keep only last 100 notifications to prevent memory issues
    if (notifications.value.length > 100) {
      notifications.value = notifications.value.slice(0, 100)
    }

    console.log(`[${notification.timestamp!.toLocaleTimeString()}] ${message}`)
  }

  /**
   * Map string notification type to enum
   */
  const mapNotificationType = (type: string): NotificationTypeEnum => {
    switch (type.toLowerCase()) {
      case 'success':
        return 'success' as NotificationTypeEnum
      case 'error':
        return 'error' as NotificationTypeEnum
      case 'warning':
        return 'warning' as NotificationTypeEnum
      case 'info':
      default:
        return 'info' as NotificationTypeEnum
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

  // Auto-request notification permission
  if (typeof window !== 'undefined') {
    requestNotificationPermission()
  }

  // Cleanup on unmount
  onUnmounted(async () => {
    if (connection.value && connectionState.value !== 'disconnected') {
      await disconnect()
    }
  })

  return {
    // State
    connection: connection as Ref<any>,
    isConnected,
    connectionState,
    connectionInfo,
    notifications,

    // Methods
    connect,
    disconnect,
    subscribeToInstitution,
    ping,
    getConnectionInfo,
    clearNotifications,
    setupEventHandlers,
  }
}

