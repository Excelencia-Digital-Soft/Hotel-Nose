<template>
  <div class="glass-card p-4 rounded-xl">
    <div class="flex items-center justify-between mb-4">
      <h3 class="text-lg font-semibold text-white">SignalR Status</h3>
      <div class="flex items-center space-x-2">
        <div 
          :class="[
            'w-3 h-3 rounded-full',
            connectionStatusClass
          ]"
        ></div>
        <span class="text-sm text-gray-300">{{ connectionStatusText }}</span>
      </div>
    </div>

    <div class="space-y-2">
      <div class="text-xs text-gray-400 space-y-1">
        <p v-if="connectionInfo">Connection ID: {{ connectionInfo.connectionId }}</p>
        <p v-if="connectionInfo?.institutionId">Institution: {{ connectionInfo.institutionId }}</p>
        <p class="text-xs">State: {{ connectionState }}</p>
      </div>

      <div class="flex space-x-2">
        <button 
          @click="testPing"
          :disabled="!isConnected"
          class="glass-button px-3 py-1 text-xs disabled:opacity-50"
        >
          Ping
        </button>
        <button 
          @click="clearNotifications"
          class="glass-button px-3 py-1 text-xs"
        >
          Clear
        </button>
      </div>
    </div>

    <!-- Recent Notifications -->
    <div v-if="recentNotifications.length > 0" class="mt-4">
      <h4 class="text-sm font-medium text-white mb-2">Recent Notifications</h4>
      <div class="space-y-1 max-h-32 overflow-y-auto">
        <div 
          v-for="notification in recentNotifications" 
          :key="notification.id || notification.timestamp?.getTime()"
          :class="[
            'p-2 rounded text-xs',
            getNotificationClass(notification.type)
          ]"
        >
          <div class="font-medium">{{ notification.message }}</div>
          <div class="text-xs opacity-75">
            {{ notification.timestamp?.toLocaleTimeString() }}
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useGlobalSignalR } from '../composables/useGlobalSignalR'
import type { NotificationTypeEnum } from '../types/signalr'

const {
  isConnected,
  isConnecting,
  isReconnecting,
  connectionState,
  connectionInfo,
  notifications,
  ping,
  clearNotifications,
  getRecentNotifications
} = useGlobalSignalR()

const recentNotifications = computed(() => getRecentNotifications(5))

const connectionStatusClass = computed(() => {
  if (isConnected.value) return 'bg-green-500'
  if (isConnecting.value || isReconnecting.value) return 'bg-yellow-500'
  return 'bg-red-500'
})

const connectionStatusText = computed(() => {
  if (isConnected.value) return 'Connected'
  if (isConnecting.value) return 'Connecting...'
  if (isReconnecting.value) return 'Reconnecting...'
  return 'Disconnected'
})

const testPing = async () => {
  try {
    await ping()
  } catch (error) {
    console.error('Ping failed:', error)
  }
}


const getNotificationClass = (type: NotificationTypeEnum) => {
  switch (type) {
    case 'success':
      return 'bg-green-500/20 border border-green-500/30 text-green-300'
    case 'error':
      return 'bg-red-500/20 border border-red-500/30 text-red-300'
    case 'warning':
      return 'bg-yellow-500/20 border border-yellow-500/30 text-yellow-300'
    case 'info':
    default:
      return 'bg-blue-500/20 border border-blue-500/30 text-blue-300'
  }
}
</script>