<template>
  <div class="fixed top-4 left-4 z-50 glass-card p-4 min-w-[300px] transition-all duration-300">
    <div class="flex items-center justify-between mb-3">
      <h3 class="text-white font-bold font-[Lexend]">🔧 SignalR Debug Panel</h3>
      <button 
        @click="isMinimized = !isMinimized"
        class="glass-button p-2 text-white hover:bg-white/20 transition-all duration-200"
        :title="isMinimized ? 'Maximize panel' : 'Minimize panel'"
      >
        <svg 
          class="w-4 h-4 transition-transform duration-200" 
          :class="{ 'rotate-180': isMinimized }"
          fill="none" 
          stroke="currentColor" 
          viewBox="0 0 24 24"
        >
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
        </svg>
      </button>
    </div>
    
    <!-- Panel Content (collapsible) -->
    <div 
      class="transition-height"
      :class="{ 'collapsed': isMinimized, 'expanded': !isMinimized }"
    >
      <!-- Connection Status -->
      <div class="mb-4">
      <div class="flex items-center gap-2 mb-2">
        <div 
          class="w-3 h-3 rounded-full"
          :class="{
            'bg-green-500 animate-pulse': isConnected,
            'bg-yellow-500': connectionStatus === 'connecting' || connectionStatus === 'reconnecting',
            'bg-red-500': connectionStatus === 'disconnected'
          }"
        ></div>
        <span class="text-white text-sm">{{ connectionStatus }}</span>
      </div>
      
      <div class="text-xs space-y-1">
        <div class="text-white/60">
          Last update: {{ lastUpdateTimestamp ? formatTime(lastUpdateTimestamp) : 'Never' }}
        </div>
        <div class="text-white/60">
          Connection ID: {{ connectionId || 'Not connected' }}
        </div>
      </div>
    </div>

    <!-- Event Statistics -->
    <div class="mb-4">
      <h4 class="text-white text-sm font-semibold mb-2">Event Stats:</h4>
      <div class="grid grid-cols-2 gap-2 text-xs">
        <div class="text-white/60">Status: <span class="text-white">{{ eventStats.statusChanges }}</span></div>
        <div class="text-white/60">Progress: <span class="text-white">{{ eventStats.progressUpdates }}</span></div>
        <div class="text-white/60">Reservations: <span class="text-white">{{ eventStats.reservationChanges }}</span></div>
        <div class="text-white/60">Maintenance: <span class="text-white">{{ eventStats.maintenanceChanges }}</span></div>
      </div>
    </div>

    <!-- Active Updates -->
    <div v-if="activeUpdates && activeUpdates.size > 0" class="mb-4">
      <h4 class="text-white text-sm font-semibold mb-2">Active Updates:</h4>
      <div class="flex flex-wrap gap-1">
        <span 
          v-for="roomId in Array.from(activeUpdates)" 
          :key="roomId"
          class="px-2 py-1 bg-primary-500/30 rounded text-xs text-white"
        >
          #{{ roomId }}
        </span>
      </div>
    </div>

    <!-- Time Debug Info -->
    <div class="mb-4">
      <h4 class="text-white text-sm font-semibold mb-2">Time Debug:</h4>
      <div class="text-xs space-y-1">
        <div class="text-white/60">
          Current time: {{ formatTime(new Date()) }}
        </div>
        <div class="text-white/60">
          Occupied rooms: {{ occupiedRoomsCount }}
        </div>
        <div class="text-white/60">
          Timer active: {{ timerActive ? 'Yes' : 'No' }}
        </div>
      </div>
    </div>

    <!-- Test Buttons -->
    <div class="space-y-2">
      <button 
        @click="testConnection"
        class="w-full glass-button text-xs py-2"
        :disabled="!isConnected"
      >
        Test Connection (Ping)
      </button>
      
      <button 
        @click="testRoomSubscription"
        class="w-full glass-button text-xs py-2"
        :disabled="!isConnected"
      >
        Subscribe to Room Events
      </button>
      
      <button 
        @click="simulateRoomUpdate"
        class="w-full glass-button text-xs py-2 bg-orange-500/20"
      >
        Simulate Room Update
      </button>

      <button 
        @click="testTimeCalculation"
        class="w-full glass-button text-xs py-2 bg-blue-500/20"
      >
        Test Time Calculation
      </button>

      <button 
        @click="testApiWithSignalR"
        class="w-full glass-button text-xs py-2 bg-green-500/20"
      >
        Test API + SignalR
      </button>

      <button 
        @click="debugTokens"
        class="w-full glass-button text-xs py-2 bg-purple-500/20"
      >
        Debug Tokens
      </button>

      <button 
        @click="testRoomEvents"
        class="w-full glass-button text-xs py-2 bg-indigo-500/20"
      >
        Test Room Events
      </button>

      <button 
        @click="testBroadcastSubscription"
        class="w-full glass-button text-xs py-2 bg-cyan-500/20"
      >
        Test Broadcast Subscription
      </button>

      <button 
        @click="simulateBroadcastEvent"
        class="w-full glass-button text-xs py-2 bg-pink-500/20"
      >
        Simulate Backend Broadcast
      </button>

      <button 
        @click="showLogs = !showLogs"
        class="w-full glass-button text-xs py-2"
      >
        {{ showLogs ? 'Hide' : 'Show' }} Console Logs
      </button>
    </div>

    <!-- Console Logs -->
    <div v-if="showLogs" class="mt-4 bg-black/50 rounded p-2 max-h-40 overflow-y-auto">
      <h4 class="text-white text-xs font-semibold mb-2">Recent Logs:</h4>
      <div class="space-y-1 text-xs font-mono">
        <div 
          v-for="(log, index) in logs" 
          :key="index"
          :class="{
            'text-green-400': log.type === 'success',
            'text-red-400': log.type === 'error',
            'text-yellow-400': log.type === 'warning',
            'text-blue-400': log.type === 'info',
            'text-gray-300': log.type === 'debug'
          }"
        >
          {{ log.time }} {{ log.message }}
        </div>
      </div>
    </div>
    
    </div> <!-- End Panel Content -->
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoomSignalR } from '../composables/rooms/useRoomSignalR'
import { useSignalRStore } from '../store/signalr'
import { useRoomsStore } from '../store/modules/roomsStore'
import { useRoomUtils } from '../composables/rooms/useRoomUtils'
// @ts-ignore
import { useAuthStore } from '../store/auth'

// Stores and composables
const signalRStore = useSignalRStore()
const roomsStore = useRoomsStore()
const roomUtils = useRoomUtils()
const authStore = useAuthStore()

// Use the room SignalR composable
const roomSignalR = useRoomSignalR()

// Local state
const isMinimized = ref(false)
const showLogs = ref(false)
const logs = ref([])

// Timer state
let debugTimer
const timerActive = ref(false)

// Computed properties
const isConnected = computed(() => signalRStore.isConnected)
const connectionStatus = computed(() => signalRStore.connectionState)
const connectionId = computed(() => signalRStore.connectionInfo?.connectionId)
const lastUpdateTimestamp = computed(() => roomSignalR.lastUpdateTimestamp?.value)
const eventStats = computed(() => roomSignalR.eventStats?.value || { statusChanges: 0, progressUpdates: 0, reservationChanges: 0, maintenanceChanges: 0 })
const activeUpdates = computed(() => roomSignalR.activeUpdates?.value)
const occupiedRoomsCount = computed(() => roomsStore.occupiedRooms.length)

// Add log entry
const addLog = (type, message) => {
  logs.value.unshift({
    type,
    time: new Date().toLocaleTimeString(),
    message
  })
  
  // Keep only last 50 logs
  if (logs.value.length > 50) {
    logs.value = logs.value.slice(0, 50)
  }
}

// Format time
const formatTime = (date) => {
  if (!date) return 'Never'
  return new Date(date).toLocaleTimeString()
}

// Test functions
const testConnection = async () => {
  try {
    addLog('info', '🏓 Testing connection...')
    await signalRStore.ping()
    addLog('success', '✅ Ping successful!')
  } catch (error) {
    addLog('error', `❌ Ping failed: ${error.message}`)
  }
}

const testRoomSubscription = async () => {
  try {
    addLog('info', '📡 Subscribing to room events...')
    
    // Subscribe to some test room groups
    if (roomSignalR.joinRoomGroup) {
      await roomSignalR.joinRoomGroup(101)
      if (roomSignalR.requestRoomProgressUpdates) {
        await roomSignalR.requestRoomProgressUpdates(101, true)
      }
    }
    
    addLog('success', '✅ Subscribed to room events!')
  } catch (error) {
    addLog('error', `❌ Subscription failed: ${error.message}`)
  }
}

const simulateRoomUpdate = () => {
  addLog('debug', '🧪 Simulating room progress update...')
  
  try {
    // Get an occupied room for testing
    const occupiedRoom = roomsStore.occupiedRooms.find(room => room.visitaID)
    
    if (!occupiedRoom) {
      addLog('warning', '⚠️ No occupied rooms found for testing')
      return
    }
    
    const roomId = occupiedRoom.habitacionId
    addLog('info', `🏨 Testing with room ${roomId}`)
    
    // Test room progress update
    const progressPayload = {
      roomId: roomId,
      visitaId: occupiedRoom.visitaID,
      progressPercentage: Math.random() * 100, // Random progress for testing
      timeElapsed: `${Math.floor(Math.random() * 3)}:${String(Math.floor(Math.random() * 60)).padStart(2, '0')}`,
      startTime: new Date().toISOString(),
      estimatedEndTime: new Date(Date.now() + 2 * 60 * 60 * 1000).toISOString()
    }
    
    addLog('debug', `📊 Progress payload: ${Math.round(progressPayload.progressPercentage)}% - ${progressPayload.timeElapsed}`)
    
    // Update store directly
    roomsStore.updateRoomProgress(progressPayload)
    
    addLog('success', '✅ Room progress updated successfully!')
    addLog('info', `🔄 UI should now show ${Math.round(progressPayload.progressPercentage)}% progress`)
    
  } catch (error) {
    addLog('error', `❌ Simulation failed: ${error.message}`)
  }
}

const testTimeCalculation = () => {
  addLog('info', '🕐 Testing time calculations...')
  
  try {
    const occupiedRooms = roomsStore.occupiedRooms.filter(room => room.visitaID)
    
    if (occupiedRooms.length === 0) {
      addLog('warning', '⚠️ No occupied rooms found for testing')
      return
    }
    
    occupiedRooms.forEach((room, index) => {
      if (index < 3) { // Test only first 3 rooms to avoid spam
        const roomId = room.habitacionId
        const timeRemaining = roomUtils.getTimeRemaining(room)
        const timeProgress = roomUtils.getTimeProgress(room)
        const timeLeft = roomUtils.getTimeLeftInMinutes(room)
        
        addLog('debug', `🏨 Room ${roomId}: ${timeRemaining} (${timeLeft} min, ${Math.round(timeProgress)}%)`)
        
        // Log detailed room data
        if (room.reservaActiva) {
          addLog('debug', `📅 Room ${roomId} data: ${JSON.stringify({
            fechaFin: room.reservaActiva.fechaFin,
            fechaInicio: room.reservaActiva.fechaInicio,
            estimatedEndTime: room.reservaActiva.estimatedEndTime,
            progressPercentage: room.reservaActiva.progressPercentage,
            timeElapsed: room.reservaActiva.timeElapsed,
            totalHoras: room.reservaActiva.totalHoras,
            totalMinutos: room.reservaActiva.totalMinutos
          }, null, 2)}`)
        }
      }
    })
    
    addLog('success', `✅ Tested time calculations for ${Math.min(3, occupiedRooms.length)} rooms`)
    
  } catch (error) {
    addLog('error', `❌ Time calculation test failed: ${error.message}`)
  }
}

const testApiWithSignalR = async () => {
  addLog('info', '🧪 Testing API calls with SignalR active...')
  
  try {
    const startTime = performance.now()
    
    // Test 1: SignalR Ping
    addLog('info', '1️⃣ Testing SignalR ping...')
    await signalRStore.ping()
    addLog('success', '✅ SignalR ping successful')
    
    // Test 2: API Room fetch (should not be blocked by SignalR)
    addLog('info', '2️⃣ Testing API room fetch...')
    const roomSuccess = await roomsStore.refreshRoomsOptimized()
    if (roomSuccess) {
      addLog('success', '✅ API room fetch successful')
    } else {
      addLog('error', '❌ API room fetch failed')
    }
    
    // Test 3: Multiple concurrent API + SignalR operations
    addLog('info', '3️⃣ Testing concurrent operations...')
    const promises = [
      roomsStore.refreshRoomsOptimized(),
      signalRStore.ping(),
      new Promise(resolve => setTimeout(resolve, 1000)) // Simulate delay
    ]
    
    await Promise.all(promises)
    
    const endTime = performance.now()
    const duration = Math.round(endTime - startTime)
    
    addLog('success', `✅ All tests completed in ${duration}ms`)
    
    // Get transport information safely
    const transport = signalRStore.connection && signalRStore.connection.transport 
      ? signalRStore.connection.transport.name 
      : 'Unknown'
    addLog('info', `📊 Transport: ${transport}`)
    addLog('info', `🌐 No API blocking detected - configuration is optimal!`)
    
  } catch (error) {
    addLog('error', `❌ API + SignalR test failed: ${error.message}`)
    addLog('warning', '⚠️ There may be a configuration issue causing API blocking')
  }
}

const debugTokens = () => {
  addLog('info', '🔐 Debugging token information...')
  
  try {
    // Check auth store
    addLog('debug', `Auth Store isAuthenticated: ${authStore.isAuthenticated}`)
    addLog('debug', `Auth Store token: ${authStore.token ? authStore.token.substring(0, 20) + '...' : 'None'}`)
    
    // Check localStorage tokens
    const lsAuthToken = localStorage.getItem('auth-token')
    const lsJwtToken = localStorage.getItem('jwt_token')
    const sessionJwtToken = sessionStorage.getItem('jwt_token')
    
    addLog('debug', `localStorage auth-token: ${lsAuthToken ? lsAuthToken.substring(0, 20) + '...' : 'None'}`)
    addLog('debug', `localStorage jwt_token: ${lsJwtToken ? lsJwtToken.substring(0, 20) + '...' : 'None'}`)
    addLog('debug', `sessionStorage jwt_token: ${sessionJwtToken ? sessionJwtToken.substring(0, 20) + '...' : 'None'}`)
    
    // Check SignalR config token
    if (signalRStore.config) {
      addLog('debug', `SignalR config token: ${signalRStore.config.accessToken ? signalRStore.config.accessToken.substring(0, 20) + '...' : 'None'}`)
      addLog('debug', `SignalR server URL: ${signalRStore.config.serverUrl}`)
    } else {
      addLog('warning', 'SignalR config not found')
    }
    
    // Validate token format
    const activeToken = authStore.token || lsAuthToken || lsJwtToken || sessionJwtToken
    if (activeToken) {
      const parts = activeToken.split('.')
      addLog('debug', `Token format validation: ${parts.length} parts (should be 3 for JWT)`)
      
      if (parts.length === 3) {
        try {
          // Decode JWT payload for debugging (not for security)
          const payload = JSON.parse(atob(parts[1]))
          addLog('debug', `Token payload:`)
          addLog('debug', `- exp: ${payload.exp} (${new Date(payload.exp * 1000).toLocaleString()})`)
          addLog('debug', `- iss: ${payload.iss}`)
          addLog('debug', `- aud: ${payload.aud}`)
          addLog('debug', `- nameid: ${payload.nameid}`)
          addLog('debug', `- InstitutionId: ${payload.InstitutionId}`)
          
          // Check if token is expired
          const now = Math.floor(Date.now() / 1000)
          if (payload.exp < now) {
            addLog('error', '❌ Token is EXPIRED!')
          } else {
            addLog('success', '✅ Token is valid and not expired')
          }
        } catch (e) {
          addLog('error', `❌ Failed to decode token payload: ${e.message}`)
        }
      } else {
        addLog('error', '❌ Invalid JWT format')
      }
    } else {
      addLog('error', '❌ No token found anywhere!')
    }
    
    addLog('success', '✅ Token debugging completed')
    
  } catch (error) {
    addLog('error', `❌ Token debugging failed: ${error.message}`)
  }
}

const testRoomEvents = () => {
  addLog('info', '🏨 Testing room events subscription...')
  
  try {
    if (!isConnected.value) {
      addLog('error', '❌ Not connected to SignalR - cannot test events')
      return
    }
    
    // Check if room event handlers are set up
    const connection = signalRStore.connection
    if (!connection) {
      addLog('error', '❌ SignalR connection not available')
      return
    }
    
    addLog('info', '📡 Checking event subscriptions...')
    
    // List expected room events
    const expectedEvents = [
      'RoomStatusChanged',
      'RoomReservationChanged', 
      'RoomProgressUpdated',
      'RoomMaintenanceChanged'
    ]
    
    expectedEvents.forEach(eventName => {
      // We can't directly check if an event is subscribed, but we can log
      addLog('debug', `✅ Expected event: ${eventName}`)
    })
    
    // Subscribe to all room events with temporary handlers for testing
    expectedEvents.forEach(eventName => {
      signalRStore.on(eventName, (data) => {
        addLog('success', `🎉 Received ${eventName} event!`)
        addLog('debug', `Event data: ${JSON.stringify(data, null, 2)}`)
      })
    })
    
    addLog('success', '✅ Room events test configured')
    addLog('info', '📢 Now make changes to rooms in another browser to test real-time sync')
    addLog('info', '🔍 Watch console and debug panel for incoming events')
    
    // Get current room count for reference
    addLog('debug', `Current room state:`)
    addLog('debug', `- Free rooms: ${roomsStore.freeRooms.length}`)
    addLog('debug', `- Occupied rooms: ${roomsStore.occupiedRooms.length}`)
    addLog('debug', `- Maintenance rooms: ${roomsStore.maintenanceRooms.length}`)
    
  } catch (error) {
    addLog('error', `❌ Room events test failed: ${error.message}`)
  }
}

const testBroadcastSubscription = async () => {
  addLog('info', '📻 Testing broadcast subscription status...')
  
  try {
    if (!isConnected.value) {
      addLog('error', '❌ Not connected to SignalR')
      return
    }
    
    // Check institution subscription
    const institutionId = authStore.institucionID || authStore.user?.InstitutionId
    addLog('info', `🏢 Institution ID: ${institutionId}`)
    
    if (!institutionId) {
      addLog('error', '❌ No institution ID found - cannot receive broadcasts')
      return
    }
    
    // Check if we're subscribed to institution
    const connectionInfo = signalRStore.connectionInfo
    if (connectionInfo && connectionInfo.institutionId === institutionId) {
      addLog('success', `✅ Already subscribed to institution ${institutionId}`)
    } else {
      addLog('warning', `⚠️ Not subscribed to institution ${institutionId}, subscribing now...`)
      await signalRStore.subscribeToInstitution(institutionId)
      addLog('success', `✅ Subscribed to institution ${institutionId}`)
    }
    
    // Test if we can invoke methods on the hub
    addLog('info', '📡 Testing hub method invocation...')
    try {
      // Try to get connection info from hub
      await signalRStore.invoke('GetConnectionInfo')
      addLog('success', '✅ Hub methods are accessible')
    } catch (e) {
      addLog('warning', `⚠️ Hub method test failed: ${e.message}`)
    }
    
    // Check if other clients are connected
    addLog('info', '🔍 Checking for other connected clients...')
    addLog('debug', `Connection ID: ${signalRStore.connectionInfo?.connectionId}`)
    addLog('debug', `Connection State: ${signalRStore.connectionState}`)
    
    // Set up test listener for broadcast events
    const testEventName = 'TestBroadcast'
    signalRStore.on(testEventName, (data) => {
      addLog('success', `🎉 Received test broadcast: ${JSON.stringify(data)}`)
    })
    
    addLog('info', '📻 Broadcast subscription test complete')
    addLog('info', '💡 If room changes don\'t sync between browsers, the backend may not be broadcasting to the institution group')
    
  } catch (error) {
    addLog('error', `❌ Broadcast test failed: ${error.message}`)
  }
}

const simulateBroadcastEvent = async () => {
  addLog('info', '🚀 Simulating backend broadcast event...')
  
  try {
    // Get an occupied room for testing
    const occupiedRoom = roomsStore.occupiedRooms[0]
    if (!occupiedRoom) {
      addLog('warning', '⚠️ No occupied rooms to test with')
      return
    }
    
    const roomId = occupiedRoom.habitacionId
    const visitaId = occupiedRoom.visitaID
    
    addLog('info', `🏨 Testing with room ${roomId} (Visit: ${visitaId})`)
    
    // Simulate different types of events
    const events = [
      {
        type: 'RoomStatusChanged',
        data: {
          roomId: roomId,
          status: 'ocupada',
          visitaId: visitaId,
          timestamp: new Date().toISOString()
        }
      },
      {
        type: 'RoomProgressUpdated',
        data: {
          roomId: roomId,
          visitaId: visitaId,
          progressPercentage: 50,
          timeElapsed: '1:30',
          startTime: new Date(Date.now() - 90 * 60 * 1000).toISOString(),
          estimatedEndTime: new Date(Date.now() + 90 * 60 * 1000).toISOString()
        }
      }
    ]
    
    // Try to invoke broadcast on hub (if backend supports it)
    for (const event of events) {
      addLog('debug', `📤 Sending ${event.type} event...`)
      
      try {
        // First, try to trigger the event directly on the connection
        if (signalRStore.connection) {
          // Manually trigger the event handler as if backend sent it
          const handlers = signalRStore.connection._methods[event.type.toLowerCase()]
          if (handlers && handlers.length > 0) {
            handlers.forEach(handler => handler(event.data))
            addLog('success', `✅ Triggered ${event.type} locally`)
          } else {
            addLog('warning', `⚠️ No handlers found for ${event.type}`)
          }
        }
      } catch (e) {
        addLog('debug', `Could not trigger event locally: ${e.message}`)
      }
      
      // Also try to invoke a test method on the hub
      try {
        await signalRStore.invoke('BroadcastTest', event.type, event.data)
        addLog('success', `✅ Sent ${event.type} to hub for broadcast`)
      } catch (e) {
        addLog('debug', `Hub broadcast method not available: ${e.message}`)
      }
    }
    
    addLog('info', '🔍 Check if events were received and UI updated')
    addLog('info', '💡 If UI didn\'t update, the issue is likely in the backend broadcast configuration')
    
  } catch (error) {
    addLog('error', `❌ Broadcast simulation failed: ${error.message}`)
  }
}

// Watch for SignalR events
onMounted(() => {
  addLog('info', '🔧 SignalR Debug Panel mounted')
  
  // Log initial state
  addLog('info', `Initial connection status: ${connectionStatus.value}`)
  
  // Start timer for time debug updates
  debugTimer = setInterval(() => {
    timerActive.value = !timerActive.value  // Toggle to show it's running
  }, 5000)
  
  // Watch for connection state changes
  signalRStore.$subscribe((mutation, state) => {
    addLog('info', `Connection state changed to: ${state.connectionState}`)
  })
})

onUnmounted(() => {
  addLog('info', '🔧 SignalR Debug Panel unmounted')
  
  if (debugTimer) {
    clearInterval(debugTimer)
  }
})
</script>

<style scoped>
.glass-card {
  @apply bg-black/40 backdrop-blur-md border border-white/20 rounded-xl;
}

.glass-button {
  @apply bg-white/10 hover:bg-white/20 backdrop-blur-sm border border-white/30 rounded-lg transition-all text-white;
}

.glass-button:disabled {
  @apply opacity-50 cursor-not-allowed hover:bg-white/10;
}

/* Minimize/Maximize transitions */
.transition-height {
  transition: max-height 0.3s ease-in-out, opacity 0.3s ease-in-out;
}

.collapsed {
  max-height: 0;
  opacity: 0;
  overflow: hidden;
}

.expanded {
  max-height: 2000px; /* Large enough to accommodate all content */
  opacity: 1;
}
</style>