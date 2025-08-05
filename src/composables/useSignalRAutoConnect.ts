import { watch, onUnmounted } from 'vue'
import { useAuthStore } from '@/store/auth'
import { NotificationService } from '@/services/NotificationService'

/**
 * Auto-connects SignalR when user is authenticated
 * Use this in your root App.vue or main layout component
 */
export function useSignalRAutoConnect() {
  const authStore = useAuthStore()
  const notificationService = NotificationService.getInstance()

  // Watch for authentication changes
  const stopWatcher = watch(
    () => ({
      isAuthenticated: authStore.isAuthenticated,
      token: authStore.token,
      institucionID: authStore.institucionID
    }),
    async (newState, oldState) => {
      // User just logged in
      if (newState.isAuthenticated && newState.token && newState.institucionID) {
        if (!oldState?.isAuthenticated) {
          console.log('🔌 Auto-connecting SignalR after login')
          try {
            await notificationService.initialize(newState.token)
            console.log('✅ SignalR connected successfully')
          } catch (error) {
            console.error('❌ Failed to auto-connect SignalR:', error)
          }
        }
      }
      
      // User logged out
      if (!newState.isAuthenticated && oldState?.isAuthenticated) {
        console.log('🔌 Auto-disconnecting SignalR after logout')
        try {
          await notificationService.stop()
          console.log('✅ SignalR disconnected successfully')
        } catch (error) {
          console.error('❌ Failed to disconnect SignalR:', error)
        }
      }
    },
    { immediate: true } // Check immediately on mount
  )

  // Cleanup on component unmount
  onUnmounted(() => {
    stopWatcher()
  })

  return {
    notificationService,
    isConnected: () => notificationService.isConnected
  }
}