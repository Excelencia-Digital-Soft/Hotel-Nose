import type { Ref, ComputedRef } from 'vue'

export interface SignalRConfig {
  serverUrl: string
  accessToken: string
  automaticReconnect?: number[]
  logging?: boolean
}

export interface NotificationData {
  [key: string]: any
}

export interface SignalRNotification {
  id?: string
  type: NotificationTypeEnum
  message: string
  data?: NotificationData
  timestamp?: Date
}

export enum NotificationTypeEnum {
  Success = 'success',
  Error = 'error',
  Warning = 'warning',
  Info = 'info'
}

export enum ConnectionStateEnum {
  Disconnected = 'disconnected',
  Connecting = 'connecting',
  Connected = 'connected',
  Reconnecting = 'reconnecting',
  Disconnecting = 'disconnecting'
}

export interface ConnectionInfo {
  connectionId: string
  state: ConnectionStateEnum
  serverUrl: string
  institutionId?: number
}

export interface SignalREventHandlers {
  onReceiveNotification?: (type: string, message: string, data?: NotificationData) => void
  onSubscriptionConfirmed?: (message: string) => void
  onReconnecting?: (error?: Error) => void
  onReconnected?: (connectionId: string) => void
  onClose?: (error?: Error) => void
}

export interface SignalRMethodInvocations {
  subscribeToInstitution: (institutionId: number) => Promise<void>
  ping: () => Promise<void>
  getConnectionInfo: () => Promise<void>
}

export interface SignalRComposableReturn {
  // State
  connection: Ref<any>
  isConnected: ComputedRef<boolean>
  connectionState: Ref<ConnectionStateEnum>
  connectionInfo: Ref<ConnectionInfo | null>
  notifications: Ref<SignalRNotification[]>
  
  // Methods
  connect: (config: SignalRConfig) => Promise<void>
  disconnect: () => Promise<void>
  subscribeToInstitution: (institutionId: number) => Promise<void>
  ping: () => Promise<void>
  getConnectionInfo: () => Promise<void>
  clearNotifications: () => void
  
  // Event setup
  setupEventHandlers: (handlers?: SignalREventHandlers) => void
}