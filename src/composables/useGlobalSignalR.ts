import { toRef } from 'vue'
import { useSignalRStore } from '../store/signalr'
import type { SignalRConfig, SignalREventHandlers } from '../types/signalr'

/**
 * Global SignalR composable that uses the singleton store
 * This ensures only one connection per app instance
 */
export function useGlobalSignalR() {
  const signalRStore = useSignalRStore()

  /**
   * Initialize SignalR if not already connected
   * Safe to call multiple times - will only connect once
   */
  const ensureConnection = async (config: SignalRConfig, handlers?: SignalREventHandlers): Promise<void> => {
    await signalRStore.initialize(config, handlers)
  }

  /**
   * Get latest notifications (reactive)
   */
  const getRecentNotifications = (limit: number = 10) => {
    return signalRStore.notifications.slice(0, limit)
  }

  /**
   * Subscribe to notifications with custom handler
   */
  const onNotification = (callback: (type: string, message: string, data?: any) => void) => {
    // This would typically use a watcher or event system
    // For now, we can add to the store's event handlers
    return {
      unsubscribe: () => {
        // Cleanup logic if needed
      }
    }
  }

  return {
    // Store state (reactive) - use toRef to maintain reactivity
    isConnected: toRef(signalRStore, 'isConnected'),
    isConnecting: toRef(signalRStore, 'isConnecting'),
    isReconnecting: toRef(signalRStore, 'isReconnecting'),
    connectionState: toRef(signalRStore, 'connectionState'),
    connectionInfo: toRef(signalRStore, 'connectionInfo'),
    notifications: toRef(signalRStore, 'notifications'),
    
    // Methods
    ensureConnection,
    disconnect: signalRStore.disconnect,
    subscribeToInstitution: signalRStore.subscribeToInstitution,
    ping: signalRStore.ping,
    getConnectionInfo: signalRStore.getConnectionInfo,
    clearNotifications: signalRStore.clearNotifications,
    updateAccessToken: signalRStore.updateAccessToken,
    
    // Utility methods
    getRecentNotifications,
    onNotification,
  }
}