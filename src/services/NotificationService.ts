/**
 * NotificationService - Comprehensive SignalR service for hotel management notifications
 *
 * Features:
 * - Type-safe SignalR connection management
 * - Automatic reconnection with exponential backoff
 * - Token-based authentication integration
 * - Event-driven notification handling
 * - Connection state monitoring
 * - Error handling and logging
 * - Memory leak prevention with proper cleanup
 */

import * as signalR from '@microsoft/signalr'
import type {
  NotificationServiceConfig,
  NotificationServiceState,
  ConnectionState,
  NotificationData,
  ClientNotification,
  NotificationEventCallbacks,
  NotificationSeverity,
  NotificationFilter,
  NotificationStats,
} from '../types/signalr'

/**
 * Default configuration for the notification service
 */
const DEFAULT_CONFIG: NotificationServiceConfig = {
  hubUrl: '/api/v1/notifications-hub',
  autoReconnect: true,
  reconnectDelays: [0, 2000, 5000, 10000, 30000],
  connectionTimeout: 30000,
  keepAliveInterval: 15000,
  maxNotifications: 500,
  defaultDismissTimeout: 300000, // 5 minutes
  enableLogging: import.meta.env.DEV,
}

/**
 * Initial connection state
 */
const INITIAL_CONNECTION_STATE: ConnectionState = {
  status: 'disconnected',
  reconnectAttempts: 0,
}

export class NotificationService {
  private static instance: NotificationService | null = null
  private state: NotificationServiceState
  private cleanup: (() => void)[] = []
  private callbackSubscribers: Map<string, NotificationEventCallbacks> = new Map()

  private constructor(config: Partial<NotificationServiceConfig> = {}) {
    this.state = {
      connection: null,
      connectionState: { ...INITIAL_CONNECTION_STATE },
      notifications: [],
      config: { ...DEFAULT_CONFIG, ...config },
      callbacks: {},
      nextClientId: 1,
    }

    this.log('NotificationService initialized with config:', this.state.config)
  }

  /**
   * Initialize and connect to the SignalR hub
   */
  async connect(accessToken: string, institutionId?: string): Promise<void> {
    if (this.state.connection && this.state.connectionState.status === 'connected') {
      this.log('Already connected to notification hub')
      return
    }

    // Prevent multiple concurrent connection attempts
    if (this.state.connectionState.status === 'connecting') {
      this.log('Connection attempt already in progress, waiting...')
      return
    }

    this.updateConnectionState({ status: 'connecting', lastAttempt: Date.now() })

    try {
      // Build the complete hub URL - match the working HTML test client
      const baseUrl = import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '') // Remove trailing slash
      const hubUrl = `${baseUrl}${this.state.config.hubUrl}`

      this.log('Connecting to SignalR hub:', hubUrl)

      this.state.connection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl, {
          accessTokenFactory: () => accessToken,
          // Force LongPolling to avoid WebSocket/SSE errors in development
          transport: signalR.HttpTransportType.LongPolling,
        })
        .withAutomaticReconnect(
          this.state.config.autoReconnect ? this.state.config.reconnectDelays : undefined
        )
        .configureLogging(
          this.state.config.enableLogging ? signalR.LogLevel.Information : signalR.LogLevel.Warning
        )
        .build()

      // Configure connection settings
      this.state.connection.serverTimeoutInMilliseconds = this.state.config.connectionTimeout
      this.state.connection.keepAliveIntervalInMilliseconds = this.state.config.keepAliveInterval

      // Set up event handlers
      this.setupEventHandlers()

      // Set up connection state monitoring
      this.setupConnectionStateHandlers()

      // Start the connection
      await this.state.connection.start()

      this.log('Successfully connected to SignalR hub')
      this.updateConnectionState({
        status: 'connected',
        error: undefined,
        reconnectAttempts: 0,
      })

      // Auto-subscribe to institution if provided (don't fail connection if subscription fails)
      if (institutionId) {
        try {
          await this.subscribeToInstitution(institutionId)
        } catch (subscriptionError) {
          this.log(
            'Warning: Auto-subscription failed, but connection maintained:',
            subscriptionError
          )
          // Don't throw error - connection is still valid
        }
      }
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown connection error'
      this.log('Failed to connect to SignalR hub:', errorMessage)

      this.updateConnectionState({
        status: 'error',
        error: errorMessage,
      })

      this.triggerCallback('onError', errorMessage)
      throw error
    }
  }

  /**
   * Disconnect from the SignalR hub
   */
  async disconnect(): Promise<void> {
    if (!this.state.connection) {
      return
    }

    this.log('Disconnecting from SignalR hub')

    try {
      // Clear all notifications with timeouts
      this.clearAllNotifications()

      // Stop the connection
      await this.state.connection.stop()

      this.log('Successfully disconnected from SignalR hub')
    } catch (error) {
      this.log('Error during disconnect:', error)
    } finally {
      // Clean up resources
      this.performCleanup()

      this.state.connection = null
      this.updateConnectionState({
        status: 'disconnected',
        subscribedInstitution: undefined,
        error: undefined,
      })
    }
  }

  /**
   * Subscribe to notifications for a specific institution
   */
  async subscribeToInstitution(institutionId: string): Promise<void> {
    if (!this.state.connection || this.state.connectionState.status !== 'connected') {
      throw new Error('Cannot subscribe: Not connected to notification hub')
    }

    this.log('Subscribing to institution notifications:', institutionId)

    try {
      await this.state.connection.invoke('SubscribeToInstitution', institutionId)

      this.updateConnectionState({
        subscribedInstitution: institutionId,
      })

      this.log('Successfully subscribed to institution:', institutionId)
    } catch (error) {
      const errorMessage = `Failed to subscribe to institution ${institutionId}: ${error}`
      this.log(errorMessage)
      this.triggerCallback('onError', errorMessage)
      throw error
    }
  }

  /**
   * Unsubscribe from institution notifications
   */
  async unsubscribeFromInstitution(institutionId: string): Promise<void> {
    if (!this.state.connection || this.state.connectionState.status !== 'connected') {
      return
    }

    this.log('Unsubscribing from institution notifications:', institutionId)

    try {
      await this.state.connection.invoke('UnsubscribeFromInstitution', institutionId)

      this.updateConnectionState({
        subscribedInstitution: undefined,
      })

      this.log('Successfully unsubscribed from institution:', institutionId)
    } catch (error) {
      this.log('Error unsubscribing from institution:', error)
    }
  }

  /**
   * Mark a notification as read
   */
  async markAsRead(notificationId: string): Promise<void> {
    const notification = this.state.notifications.find(
      (n) => n.id === notificationId || n.clientId === notificationId
    )

    if (!notification) {
      this.log('Notification not found for markAsRead:', notificationId)
      return
    }

    notification.isRead = true
    this.triggerCallback('onNotificationRead', notificationId)

    // Notify server if connection is available
    if (
      this.state.connection &&
      this.state.connectionState.status === 'connected' &&
      notification.id
    ) {
      try {
        await this.state.connection.invoke('MarkAsRead', notification.id)
        this.log('Marked notification as read on server:', notification.id)
      } catch (error) {
        this.log('Failed to mark notification as read on server:', error)
      }
    }
  }

  /**
   * Dismiss a notification
   */
  async dismissNotification(notificationId: string): Promise<void> {
    const notificationIndex = this.state.notifications.findIndex(
      (n) => n.id === notificationId || n.clientId === notificationId
    )

    if (notificationIndex === -1) {
      this.log('Notification not found for dismiss:', notificationId)
      return
    }

    const notification = this.state.notifications[notificationIndex]

    // Clear timeout if exists
    if (notification.timeoutId) {
      clearTimeout(notification.timeoutId)
    }

    // Remove from local notifications
    this.state.notifications.splice(notificationIndex, 1)

    this.triggerCallback('onNotificationDismissed', notificationId)

    // Notify server if connection is available
    if (
      this.state.connection &&
      this.state.connectionState.status === 'connected' &&
      notification.id
    ) {
      try {
        await this.state.connection.invoke('DismissNotification', notification.id)
        this.log('Dismissed notification on server:', notification.id)
      } catch (error) {
        this.log('Failed to dismiss notification on server:', error)
      }
    }
  }

  /**
   * Get notification history from server
   */
  async getNotificationHistory(): Promise<NotificationData[]> {
    if (!this.state.connection || this.state.connectionState.status !== 'connected') {
      throw new Error('Cannot get history: Not connected to notification hub')
    }

    try {
      const history = await this.state.connection.invoke('GetNotificationHistory')
      this.log('Retrieved notification history:', history?.length || 0, 'notifications')
      return history || []
    } catch (error) {
      this.log('Failed to get notification history:', error)
      throw error
    }
  }

  /**
   * Clear all notifications
   */
  clearAllNotifications(): void {
    // Clear all timeouts
    this.state.notifications.forEach((notification) => {
      if (notification.timeoutId) {
        clearTimeout(notification.timeoutId)
      }
    })

    this.state.notifications = []
    this.log('Cleared all notifications')
  }

  /**
   * Filter notifications based on criteria
   */
  filterNotifications(filter: NotificationFilter): ClientNotification[] {
    let filtered = [...this.state.notifications]

    if (filter.categories?.length) {
      filtered = filtered.filter((n) => filter.categories!.includes(n.category))
    }

    if (filter.severities?.length) {
      filtered = filtered.filter((n) => filter.severities!.includes(n.severity))
    }

    if (filter.readStatus && filter.readStatus !== 'all') {
      const isRead = filter.readStatus === 'read'
      filtered = filtered.filter((n) => n.isRead === isRead)
    }

    if (filter.roomId) {
      filtered = filtered.filter((n) => n.roomId === filter.roomId)
    }

    if (filter.dateRange) {
      const { from, to } = filter.dateRange
      filtered = filtered.filter((n) => {
        const notificationDate = new Date(n.receivedAt)
        return notificationDate >= from && notificationDate <= to
      })
    }

    if (filter.limit) {
      filtered = filtered.slice(0, filter.limit)
    }

    return filtered
  }

  /**
   * Get notification statistics
   */
  getStats(): NotificationStats {
    const notifications = this.state.notifications
    const unread = notifications.filter((n) => !n.isRead)

    const byCategory = notifications.reduce(
      (acc, n) => {
        acc[n.category] = (acc[n.category] || 0) + 1
        return acc
      },
      {} as Record<string, number>
    )

    const bySeverity = notifications.reduce(
      (acc, n) => {
        acc[n.severity] = (acc[n.severity] || 0) + 1
        return acc
      },
      {} as Record<string, number>
    )

    const lastReceived =
      notifications.length > 0 ? Math.max(...notifications.map((n) => n.receivedAt)) : undefined

    return {
      total: notifications.length,
      unread: unread.length,
      byCategory: byCategory as any,
      bySeverity: bySeverity as any,
      lastReceived,
    }
  }

  /**
   * Subscribe to events with a unique subscriber ID
   */
  subscribeToEvents(subscriberId: string, callbacks: NotificationEventCallbacks): void {
    this.callbackSubscribers.set(subscriberId, callbacks)
    this.log(`Subscribed callbacks for: ${subscriberId}`)
  }

  /**
   * Unsubscribe from events
   */
  unsubscribeFromEvents(subscriberId: string): void {
    this.callbackSubscribers.delete(subscriberId)
    this.log(`Unsubscribed callbacks for: ${subscriberId}`)
  }

  /**
   * Set event callbacks (legacy method - deprecated)
   */
  setCallbacks(callbacks: NotificationEventCallbacks): void {
    this.state.callbacks = { ...this.state.callbacks, ...callbacks }
    this.log('Updated event callbacks (legacy)')
  }

  /**
   * Get current connection state
   */
  getConnectionState(): ConnectionState {
    return { ...this.state.connectionState }
  }

  /**
   * Get all notifications
   */
  getNotifications(): ClientNotification[] {
    return [...this.state.notifications]
  }

  /**
   * Check if service is connected
   */
  isConnected(): boolean {
    return this.state.connectionState.status === 'connected'
  }

  /**
   * Setup SignalR event handlers
   */
  private setupEventHandlers(): void {
    if (!this.state.connection) return

    // Handle incoming notifications
    this.state.connection.on(
      'ReceiveNotification',
      (type: NotificationSeverity, message: string, data: NotificationData) => {
        this.log('Received notification:', { type, message, data })
        this.handleIncomingNotification(type, message, data)
      }
    )

    // Handle subscription confirmation
    this.state.connection.on('SubscriptionConfirmed', (message: string) => {
      this.log('Subscription confirmed:', message)
      this.triggerCallback('onSubscriptionConfirmed', message)
    })

    // Handle bulk notifications (for history/initial load)
    this.state.connection.on('BulkNotifications', (notifications: NotificationData[]) => {
      this.log('Received bulk notifications:', notifications.length)
      notifications.forEach((data) => {
        this.handleIncomingNotification(data.severity, data.message, data)
      })
    })

    // Handle connection status changes
    this.state.connection.on(
      'ConnectionStatusChanged',
      (status: 'connected' | 'disconnected' | 'reconnecting') => {
        this.log('Connection status changed:', status)
        this.updateConnectionState({ status })
      }
    )
  }

  /**
   * Setup connection state monitoring
   */
  private setupConnectionStateHandlers(): void {
    if (!this.state.connection) return

    this.state.connection.onclose((error) => {
      this.log('Connection closed:', error?.message || 'No error')
      this.updateConnectionState({
        status: 'disconnected',
        error: error?.message,
      })
    })

    this.state.connection.onreconnecting((error) => {
      this.log('Reconnecting:', error?.message || 'No error')
      this.updateConnectionState({
        status: 'reconnecting',
        reconnectAttempts: this.state.connectionState.reconnectAttempts + 1,
        error: error?.message,
      })
    })

    this.state.connection.onreconnected((connectionId) => {
      this.log('Reconnected with connection ID:', connectionId)
      this.updateConnectionState({
        status: 'connected',
        error: undefined,
        reconnectAttempts: 0,
      })

      // Re-subscribe to institution if we were subscribed
      if (this.state.connectionState.subscribedInstitution) {
        this.subscribeToInstitution(this.state.connectionState.subscribedInstitution).catch(
          (error) => this.log('Failed to re-subscribe after reconnection:', error)
        )
      }
    })
  }

  /**
   * Handle incoming notification from server
   */
  private handleIncomingNotification(
    type: NotificationSeverity,
    message: string,
    data: NotificationData
  ): void {
    const clientNotification: ClientNotification = {
      ...data,
      severity: type, // Use the type parameter as severity
      message: message, // Use the message parameter
      clientId: this.generateClientId(),
      receivedAt: Date.now(),
      isRead: false,
      isDismissed: false,
    }

    // Set up auto-dismiss timeout if specified
    if (this.state.config.defaultDismissTimeout > 0) {
      clientNotification.timeoutId = window.setTimeout(() => {
        this.dismissNotification(clientNotification.clientId)
      }, this.state.config.defaultDismissTimeout)
    }

    // Add to notifications array
    this.state.notifications.unshift(clientNotification)

    // Maintain max notifications limit
    if (this.state.notifications.length > this.state.config.maxNotifications) {
      const removed = this.state.notifications.splice(this.state.config.maxNotifications)
      // Clear timeouts for removed notifications
      removed.forEach((n) => {
        if (n.timeoutId) {
          clearTimeout(n.timeoutId)
        }
      })
    }

    // Trigger callback
    this.triggerCallback('onNotificationReceived', clientNotification)
  }

  /**
   * Update connection state and trigger callbacks
   */
  private updateConnectionState(updates: Partial<ConnectionState>): void {
    this.state.connectionState = { ...this.state.connectionState, ...updates }
    this.triggerCallback('onConnectionStateChange', this.state.connectionState)
  }

  /**
   * Trigger event callback safely for all subscribers
   */
  private triggerCallback<K extends keyof NotificationEventCallbacks>(
    event: K,
    ...args: Parameters<NonNullable<NotificationEventCallbacks[K]>>
  ): void {
    // Call legacy callback
    try {
      const callback = this.state.callbacks[event]
      if (callback) {
        ;(callback as any)(...args)
      }
    } catch (error) {
      this.log('Error in legacy event callback:', error)
    }

    // Call all subscriber callbacks
    this.callbackSubscribers.forEach((callbacks, subscriberId) => {
      try {
        const callback = callbacks[event]
        if (callback) {
          ;(callback as any)(...args)
        }
      } catch (error) {
        this.log(`Error in subscriber ${subscriberId} callback:`, error)
      }
    })
  }

  /**
   * Generate unique client ID
   */
  private generateClientId(): string {
    return `client_${Date.now()}_${this.state.nextClientId++}`
  }

  /**
   * Clean up resources and timeouts
   */
  private performCleanup(): void {
    // Clear all notification timeouts
    this.state.notifications.forEach((notification) => {
      if (notification.timeoutId) {
        clearTimeout(notification.timeoutId)
      }
    })

    // Run any additional cleanup functions
    this.cleanup.forEach((fn) => {
      try {
        fn()
      } catch (error) {
        this.log('Error during cleanup:', error)
      }
    })

    this.cleanup = []
  }

  /**
   * Conditional logging based on configuration
   */
  private log(message: string, ...args: any[]): void {
    if (this.state.config.enableLogging) {
      console.log(`[NotificationService] ${message}`, ...args)
    }
  }

  /**
   * Add cleanup function
   */
  addCleanup(fn: () => void): void {
    this.cleanup.push(fn)
  }

  /**
   * Get singleton instance of NotificationService
   */
  static getInstance(config?: Partial<NotificationServiceConfig>): NotificationService {
    if (!NotificationService.instance) {
      NotificationService.instance = new NotificationService(config)
    }
    return NotificationService.instance
  }

  /**
   * Initialize the singleton instance with a token
   */
  async initialize(accessToken: string, institutionId?: string): Promise<void> {
    return this.connect(accessToken, institutionId)
  }

  /**
   * Stop and reset the singleton instance
   */
  async stop(): Promise<void> {
    await this.disconnect()
    // Optionally reset the singleton instance
    // NotificationService.instance = null
  }
}

