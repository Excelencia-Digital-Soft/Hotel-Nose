/**
 * SignalR Notification Service Types
 *
 * Comprehensive TypeScript interfaces for the hotel management notification system
 * Built for the /api/v1/notifications endpoint with proper type safety
 */

import { HubConnection } from '@microsoft/signalr'

/**
 * Notification severity levels
 */
export type NotificationSeverity = 'info' | 'warning' | 'error' | 'success'

/**
 * Hotel-specific notification categories
 */
export type NotificationCategory =
  | 'room_status' // Room occupancy, cleaning, maintenance
  | 'reservation' // New bookings, cancellations, modifications
  | 'payment' // Payment confirmations, failures
  | 'inventory' // Stock alerts, low inventory
  | 'system' // System maintenance, updates
  | 'consumption' // Room service, minibar consumption
  | 'checkout' // Guest checkout, billing
  | 'maintenance' // Equipment issues, repair requests
  | 'alert' // Emergency, security alerts
  | 'general' // General notifications

/**
 * Core notification data structure received from server
 */
export interface NotificationData {
  /** Unique notification identifier */
  id?: string
  /** Notification category for filtering/routing */
  category: NotificationCategory
  /** Notification severity level */
  severity: NotificationSeverity
  /** Primary notification message */
  title: string
  /** Detailed notification description */
  message: string
  /** Associated room ID if applicable */
  roomId?: number
  /** Associated reservation ID if applicable */
  reservationId?: number
  /** Associated guest ID if applicable */
  guestId?: number
  /** Institution/hotel ID */
  institutionId?: number
  /** Additional metadata */
  metadata?: Record<string, any>
  /** Server timestamp */
  timestamp?: string
  /** Notification expiry time */
  expiresAt?: string
  /** Whether notification requires user action */
  requiresAction?: boolean
  /** Action URL or route */
  actionUrl?: string
  /** Action button text */
  actionText?: string
}

/**
 * Client-side notification with local state
 */
export interface ClientNotification extends NotificationData {
  /** Local client ID for tracking */
  clientId: string
  /** Local timestamp when received */
  receivedAt: number
  /** Whether notification has been read */
  isRead: boolean
  /** Whether notification is dismissed */
  isDismissed: boolean
  /** Auto-dismiss timeout ID */
  timeoutId?: number
}

/**
 * Server-to-client SignalR events
 */
export interface SignalRServerEvents {
  /** Main notification event */
  ReceiveNotification: (type: NotificationSeverity, message: string, data: NotificationData) => void
  /** Subscription confirmation */
  SubscriptionConfirmed: (message: string) => void
  /** Connection status updates */
  ConnectionStatusChanged: (status: 'connected' | 'disconnected' | 'reconnecting') => void
  /** Bulk notifications for initial load */
  BulkNotifications: (notifications: NotificationData[]) => void
}

/**
 * Client-to-server SignalR methods
 */
export interface SignalRClientMethods {
  /** Subscribe to institution notifications */
  SubscribeToInstitution: (institutionId: string) => Promise<void>
  /** Unsubscribe from institution */
  UnsubscribeFromInstitution: (institutionId: string) => Promise<void>
  /** Mark notification as read */
  MarkAsRead: (notificationId: string) => Promise<void>
  /** Dismiss notification */
  DismissNotification: (notificationId: string) => Promise<void>
  /** Request notification history */
  GetNotificationHistory: () => Promise<NotificationData[]>
}

/**
 * Connection configuration options
 */
export interface NotificationServiceConfig {
  /** SignalR hub endpoint */
  hubUrl: string
  /** Automatic reconnection enabled */
  autoReconnect: boolean
  /** Reconnection delay intervals (ms) */
  reconnectDelays: number[]
  /** Connection timeout (ms) */
  connectionTimeout: number
  /** Keep-alive interval (ms) */
  keepAliveInterval: number
  /** Max notification retention count */
  maxNotifications: number
  /** Default auto-dismiss timeout (ms) */
  defaultDismissTimeout: number
  /** Enable debug logging */
  enableLogging: boolean
}

/**
 * Connection state information
 */
export interface ConnectionState {
  /** Current connection status */
  status: 'disconnected' | 'connecting' | 'connected' | 'reconnecting' | 'error'
  /** Connection error message if any */
  error?: string
  /** Last connection attempt timestamp */
  lastAttempt?: number
  /** Number of reconnection attempts */
  reconnectAttempts: number
  /** Currently subscribed institution ID */
  subscribedInstitution?: string
}

/**
 * Notification filter criteria
 */
export interface NotificationFilter {
  /** Filter by categories */
  categories?: NotificationCategory[]
  /** Filter by severity levels */
  severities?: NotificationSeverity[]
  /** Filter by read status */
  readStatus?: 'read' | 'unread' | 'all'
  /** Filter by room ID */
  roomId?: number
  /** Filter by date range */
  dateRange?: {
    from: Date
    to: Date
  }
  /** Maximum number of notifications */
  limit?: number
}

/**
 * Notification statistics
 */
export interface NotificationStats {
  /** Total notification count */
  total: number
  /** Unread notification count */
  unread: number
  /** Count by category */
  byCategory: Record<NotificationCategory, number>
  /** Count by severity */
  bySeverity: Record<NotificationSeverity, number>
  /** Most recent notification timestamp */
  lastReceived?: number
}

/**
 * Event callback types for composable
 */
export interface NotificationEventCallbacks {
  /** Called when new notification is received */
  onNotificationReceived?: (notification: ClientNotification) => void
  /** Called when notification is read */
  onNotificationRead?: (notificationId: string) => void
  /** Called when notification is dismissed */
  onNotificationDismissed?: (notificationId: string) => void
  /** Called on connection state change */
  onConnectionStateChange?: (state: ConnectionState) => void
  /** Called on subscription confirmation */
  onSubscriptionConfirmed?: (message: string) => void
  /** Called on error */
  onError?: (error: string) => void
}

/**
 * Service state interface
 */
export interface NotificationServiceState {
  /** SignalR hub connection */
  connection: HubConnection | null
  /** Current connection state */
  connectionState: ConnectionState
  /** All received notifications */
  notifications: ClientNotification[]
  /** Service configuration */
  config: NotificationServiceConfig
  /** Event callbacks */
  callbacks: NotificationEventCallbacks
  /** Next client notification ID */
  nextClientId: number
}

/**
 * Composable return interface
 */
export interface UseNotificationsReturn {
  // State
  /** Reactive notifications array */
  notifications: Readonly<Ref<ClientNotification[]>>
  /** Reactive connection state */
  connectionState: Readonly<Ref<ConnectionState>>
  /** Reactive notification statistics */
  stats: Readonly<Ref<NotificationStats>>

  // Computed
  /** Unread notifications */
  unreadNotifications: Readonly<Ref<ClientNotification[]>>
  /** Notifications by category */
  notificationsByCategory: Readonly<Ref<Record<NotificationCategory, ClientNotification[]>>>
  /** Connection status boolean */
  isConnected: Readonly<Ref<boolean>>

  // Actions
  /** Initialize and connect to notification service */
  connect: () => Promise<void>
  /** Disconnect from notification service */
  disconnect: () => Promise<void>
  /** Mark notification as read */
  markAsRead: (notificationId: string) => Promise<void>
  /** Dismiss notification */
  dismissNotification: (notificationId: string) => Promise<void>
  /** Clear all notifications */
  clearAllNotifications: () => void
  /** Filter notifications */
  filterNotifications: (filter: NotificationFilter) => ClientNotification[]
  /** Subscribe to institution */
  subscribeToInstitution: (institutionId: string) => Promise<void>
  /** Get notification history */
  getHistory: () => Promise<void>

  // Event handlers
  /** Set notification received callback */
  onNotificationReceived: (callback: (notification: ClientNotification) => void) => void
  /** Set connection state change callback */
  onConnectionStateChange: (callback: (state: ConnectionState) => void) => void
}

/**
 * Vue 3 Ref import for composable return types
 */
import type { Ref } from 'vue'

