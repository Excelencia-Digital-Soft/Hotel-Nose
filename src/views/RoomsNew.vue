<template>
  <div
    class="min-h-screen bg-gradient-to-br from-surface-950 via-surface-900 to-surface-950 relative overflow-hidden"
  >
    <!-- Decorative background effects -->
    <div class="absolute inset-0 overflow-hidden pointer-events-none">
      <div class="absolute top-1/4 -left-48 w-96 h-96 bg-primary-500/5 rounded-full blur-3xl"></div>
      <div
        class="absolute bottom-1/4 -right-48 w-96 h-96 bg-secondary-500/5 rounded-full blur-3xl"
      ></div>
      <div class="absolute top-3/4 left-1/3 w-64 h-64 bg-accent-500/3 rounded-full blur-2xl"></div>
    </div>

    <div class="relative z-10 p-6">
      <!-- Enhanced header with glassmorphism -->
      <div class="mb-8">
        <div :class="headerClasses">
          <div
            class="flex flex-col lg:flex-row justify-between items-start lg:items-center"
            :class="compactMode ? 'gap-3' : 'gap-6'"
          >
            <!-- Main title -->
            <div class="flex items-center" :class="compactMode ? 'gap-3' : 'gap-4'">
              <div
                class="bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 rounded-2xl flex items-center justify-center shadow-lg transition-all duration-300"
                :class="compactMode ? 'w-12 h-12' : 'w-16 h-16'"
              >
                <span
                  class="material-symbols-outlined text-white"
                  :class="compactMode ? 'text-xl' : 'text-2xl'"
                  >hotel</span
                >
              </div>
              <div v-if="!compactMode">
                <h1
                  class="text-4xl font-bold bg-gradient-to-r from-primary-300 via-secondary-300 to-accent-300 bg-clip-text text-transparent lexend-exa"
                >
                  Panel de Habitaciones
                </h1>
                <p class="text-gray-400 text-sm mt-1">Gestión completa de alojamiento</p>
              </div>
              <div v-else>
                <h1
                  class="text-2xl font-bold bg-gradient-to-r from-primary-300 via-secondary-300 to-accent-300 bg-clip-text text-transparent lexend-exa"
                >
                  Habitaciones
                </h1>
              </div>
            </div>

            <!-- Controls -->
            <div class="flex flex-wrap gap-3">
              <!-- Filters -->
              <RoomFilters
                v-model:search-term="searchTerm"
                v-model:selected-category="selectedCategory"
                v-model:show-only-occupied="showOnlyOccupied"
                :compact-mode="compactMode"
                :has-active-filters="hasActiveFilters"
                @clear-filters="clearFilters"
              />

              <!-- View controls -->
              <RoomViewControls
                :view-mode="viewMode"
                :compact-mode="compactMode"
                :is-refreshing="roomsStore.isRefreshing"
                @toggle-view-mode="toggleViewMode"
                @toggle-compact-mode="toggleCompactMode"
                @refresh="refreshRooms"
              />
            </div>
          </div>

          <!-- Statistics dashboard -->
          <RoomStats :stats="roomsStore.roomStats" :compact-mode="compactMode" />
        </div>
      </div>

      <!-- Error display -->
      <div v-if="roomsStore.hasErrors" class="mb-6">
        <div class="bg-red-500/10 border border-red-500/20 rounded-2xl p-4 backdrop-blur-md">
          <div class="flex items-center gap-3">
            <span class="material-symbols-outlined text-red-400">error</span>
            <div>
              <h3 class="text-red-300 font-medium">Error al cargar habitaciones</h3>
              <p class="text-red-200/70 text-sm">{{ roomsStore.currentError }}</p>
            </div>
            <button
              @click="roomsStore.clearErrors"
              class="ml-auto text-red-400 hover:text-red-300 transition-colors"
            >
              <span class="material-symbols-outlined">close</span>
            </button>
          </div>
        </div>
      </div>

      <!-- Loading state -->
      <div
        v-if="roomsStore.isLoading && !roomsStore.allRooms.length"
        class="flex items-center justify-center py-20"
      >
        <div class="text-center">
          <div
            class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-primary-400 mb-4"
          ></div>
          <p class="text-gray-400">Cargando habitaciones...</p>
        </div>
      </div>

      <!-- Main content -->
      <div v-else>
        <!-- Grid view -->
        <div v-if="viewMode === 'grid'" class="grid grid-cols-2 gap-6">
          <!-- Free & Maintenance rooms panel -->
          <div class="space-y-6">
            <div
              class="bg-white/5 backdrop-blur-xl border border-white/10 rounded-3xl p-6 shadow-2xl"
            >
              <div class="flex items-center gap-4 mb-6">
                <div
                  class="w-12 h-12 bg-gradient-to-r from-green-400 to-emerald-500 rounded-2xl flex items-center justify-center shadow-lg"
                >
                  <span class="material-symbols-outlined text-white">hotel_class</span>
                </div>
                <div>
                  <h2 class="text-2xl text-white lexend-exa font-bold">Habitaciones Libres</h2>
                  <p class="text-gray-400 text-sm">
                    {{ filteredFreeRooms.length }} disponibles
                    <span v-if="filteredMaintenanceRooms.length > 0" class="text-yellow-400">
                      • {{ filteredMaintenanceRooms.length }} en mantenimiento
                    </span>
                  </p>
                </div>
              </div>

              <div :class="freeRoomsGridClasses">
                <template
                  v-for="(room, index) in combinedFreeAndMaintenanceRooms"
                  :key="room.habitacionId"
                >
                  <!-- Separador visual para habitaciones en mantenimiento -->
                  <div
                    v-if="isFirstMaintenanceRoom(room, index)"
                    class="col-span-full flex items-center gap-3 py-2 my-2"
                  >
                    <div
                      class="flex-1 h-px bg-gradient-to-r from-transparent via-yellow-400/30 to-transparent"
                    ></div>
                    <span
                      class="text-yellow-400 text-xs font-medium px-2 py-1 bg-yellow-400/10 rounded-full border border-yellow-400/20"
                    >
                      En Mantenimiento
                    </span>
                    <div
                      class="flex-1 h-px bg-gradient-to-r from-transparent via-yellow-400/30 to-transparent"
                    ></div>
                  </div>

                  <RoomCard
                    :room="room"
                    :variant="compactMode ? 'compact' : 'default'"
                    @click="
                      room.estadoHabitacion === 'Mantenimiento'
                        ? openMaintenanceRoomModal(room)
                        : openFreeRoomModal(room)
                    "
                    @refresh-room="handleRefreshRoom"
                  />
                </template>
              </div>
            </div>
          </div>

          <!-- Occupied rooms panel -->
          <div class="space-y-6">
            <div
              class="bg-white/5 backdrop-blur-xl border border-white/10 rounded-3xl p-6 shadow-2xl"
            >
              <div class="flex items-center gap-4 mb-6">
                <div
                  class="w-12 h-12 bg-gradient-to-r from-red-400 to-rose-500 rounded-2xl flex items-center justify-center shadow-lg"
                >
                  <span class="material-symbols-outlined text-white">hotel</span>
                </div>
                <div>
                  <h2 class="text-2xl text-white lexend-exa font-bold">Habitaciones Ocupadas</h2>
                  <p class="text-gray-400 text-sm">{{ filteredOccupiedRooms.length }} en uso</p>
                </div>
              </div>

              <div :class="occupiedRoomsGridClasses">
                <RoomCard
                  v-for="room in filteredOccupiedRooms"
                  :key="room.habitacionId"
                  :room="room"
                  :variant="compactMode ? 'compact' : 'default'"
                  @click="openOccupiedRoomModal"
                  @refresh-room="handleRefreshRoom"
                />
              </div>
            </div>
          </div>
        </div>

        <!-- List view -->
        <div v-else class="space-y-6 grid grid-cols-2 gap-4 content-start">
          <!-- Free & Maintenance rooms list -->
          <div
            class="bg-white/5 backdrop-blur-xl border border-white/10 rounded-3xl overflow-hidden shadow-2xl"
          >
            <div
              class="p-6 bg-gradient-to-r from-green-600/20 to-emerald-600/20 border-b border-green-500/30"
            >
              <h3 class="text-white font-bold text-xl flex items-center gap-3">
                <span class="material-symbols-outlined">hotel_class</span>
                Habitaciones Libres ({{ filteredFreeRooms.length }})
                <span
                  v-if="filteredMaintenanceRooms.length > 0"
                  class="text-yellow-400 text-sm font-normal"
                >
                  • {{ filteredMaintenanceRooms.length }} en mantenimiento
                </span>
              </h3>
            </div>
            <div class="p-6">
              <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-3 gap-4">
                <template
                  v-for="(room, index) in combinedFreeAndMaintenanceRooms"
                  :key="room.habitacionId"
                >
                  <!-- Separador visual para habitaciones en mantenimiento -->
                  <div
                    v-if="isFirstMaintenanceRoom(room, index)"
                    class="col-span-full flex items-center gap-3 py-2 my-2"
                  >
                    <div
                      class="flex-1 h-px bg-gradient-to-r from-transparent via-yellow-400/30 to-transparent"
                    ></div>
                    <span
                      class="text-yellow-400 text-xs font-medium px-2 py-1 bg-yellow-400/10 rounded-full border border-yellow-400/20"
                    >
                      En Mantenimiento
                    </span>
                    <div
                      class="flex-1 h-px bg-gradient-to-r from-transparent via-yellow-400/30 to-transparent"
                    ></div>
                  </div>

                  <div
                    @click="
                      room.estadoHabitacion === 'Mantenimiento'
                        ? openMaintenanceRoomModal(room)
                        : openFreeRoomModal(room)
                    "
                    :class="[
                      'group flex items-center gap-4 p-4 backdrop-blur-md rounded-2xl transition-all duration-300 cursor-pointer border border-white/10 hover:scale-105',
                      room.estadoHabitacion === 'Mantenimiento'
                        ? 'bg-yellow-500/10 hover:bg-yellow-500/20 hover:border-yellow-400/50'
                        : 'bg-white/5 hover:bg-green-500/10 hover:border-green-400/50',
                    ]"
                  >
                    <div
                      :class="[
                        'w-3 h-3 rounded-full',
                        room.estadoHabitacion === 'Mantenimiento'
                          ? 'bg-yellow-400 animate-pulse'
                          : 'bg-green-400 animate-pulse',
                      ]"
                    ></div>
                    <div class="flex-1">
                      <span class="text-white font-medium block">{{
                        roomUtils.getRoomNumber(room.nombreHabitacion)
                      }}</span>
                      <span
                        :class="[
                          'text-sm',
                          room.estadoHabitacion === 'Mantenimiento'
                            ? 'text-yellow-400'
                            : 'text-gray-400',
                        ]"
                      >
                        {{ roomUtils.getCategoryFromName(room.nombreHabitacion) }}
                        <span v-if="room.estadoHabitacion === 'Mantenimiento'" class="ml-2"
                          >• Mantenimiento</span
                        >
                      </span>
                    </div>
                    <span
                      :class="[
                        'text-sm font-bold',
                        room.estadoHabitacion === 'Mantenimiento'
                          ? 'text-yellow-300'
                          : 'text-green-300',
                      ]"
                      >${{ room.precio }}/h</span
                    >
                  </div>
                </template>
              </div>
            </div>
          </div>

          <!-- Occupied rooms list -->
          <div
            class="bg-white/5 backdrop-blur-xl border border-white/10 rounded-3xl overflow-hidden shadow-2xl content-start"
          >
            <div
              class="p-6 bg-gradient-to-r from-red-600/20 to-rose-600/20 border-b border-red-500/30"
            >
              <h3 class="text-white font-bold text-xl flex items-center gap-3">
                <span class="material-symbols-outlined">hotel</span>
                Habitaciones Ocupadas ({{ filteredOccupiedRooms.length }})
              </h3>
            </div>
            <div class="p-6">
              <div class="space-y-3 overflow-y-auto custom-scrollbar">
                <div
                  v-for="room in filteredOccupiedRooms"
                  :key="room.habitacionId"
                  @click="openOccupiedRoomModal(room)"
                  class="group flex items-center gap-4 p-4 bg-white/5 backdrop-blur-md rounded-2xl hover:bg-white/10 transition-all duration-300 cursor-pointer border border-white/10 hover:border-gray-400/50 hover:scale-[1.02]"
                >
                  <div :class="roomUtils.getStatusIndicator(room)"></div>
                  <div class="flex-1">
                    <div class="flex items-center gap-3 mb-1">
                      <span class="text-white font-medium">{{
                        roomUtils.getRoomNumber(room.nombreHabitacion)
                      }}</span>
                      <span v-if="room.pedidosPendientes" class="text-red-400 animate-pulse">
                        <span class="material-symbols-outlined text-sm">notifications_active</span>
                      </span>
                    </div>
                    <div class="text-gray-400 text-sm">
                      {{ room.visita?.identificador || 'Sin cliente' }} •
                      {{ roomUtils.getCategoryFromName(room.nombreHabitacion) }}
                    </div>
                  </div>
                  <div class="text-right">
                    <div class="text-sm font-medium" :class="roomUtils.getTimeTextColor(room)">
                      {{ roomUtils.getTimeRemaining(room) }}
                    </div>
                    <div class="text-xs text-gray-500">{{ roomUtils.getStatusText(room) }}</div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Modals -->
    <ReserveRoom
      :room="selectedRoom"
      v-if="showOccupiedModal && selectedRoom"
      @close-modal="closeModals"
      @update-room="handleRoomUpdate"
      @update-tiempo="handleAddExtraTime"
      @room-checkout="handleRoomCheckout"
    />

    <ReserveRoomLibre
      :room="selectedRoom"
      v-if="showFreeModal && selectedRoom"
      @close-modal="closeModals"
      @room-reserved="handleRoomReservation"
    />
  </div>
</template>

<script setup>
  import { ref, computed, onMounted, onUnmounted, watchEffect } from 'vue'
  import { useRoomsStore } from '../store/modules/roomsStore'
  import { useRoomUtils } from '../composables/rooms/useRoomUtils'
  import { useRoomFilters } from '../composables/rooms/useRoomFilters'
  import { useRoomActions } from '../composables/rooms/useRoomActions'
  import { useRoomView } from '../composables/rooms/useRoomView'
  import { useRoomSignalR } from '../composables/rooms/useRoomSignalR'

  // Components
  import RoomCard from '../components/rooms/RoomCard.vue'
  import RoomFilters from '../components/rooms/RoomFilters.vue'
  import RoomStats from '../components/rooms/RoomStats.vue'
  import RoomViewControls from '../components/rooms/RoomViewControls.vue'
  import ReserveRoom from '../components/ReserveRoom.vue'
  import ReserveRoomLibre from '../components/ReserveRoomLibre.vue'

  // Store and composables
  const roomsStore = useRoomsStore()
  const roomUtils = useRoomUtils()

  // View management
  const { viewMode, compactMode, toggleViewMode, toggleCompactMode, headerClasses } = useRoomView()

  // Filtering
  const {
    searchTerm,
    selectedCategory,
    selectedMaintenanceType,
    showOnlyOccupied,
    showOnlyMaintenance,
    filteredFreeRooms,
    filteredMaintenanceRooms,
    filteredOccupiedRooms,
    hasActiveFilters,
    clearFilters,
  } = useRoomFilters(roomsStore)

  // Actions
  const {
    selectedRoom,
    showOccupiedModal,
    showFreeModal,
    openOccupiedRoomModal,
    openFreeRoomModal,
    closeModals,
    handleRoomReservation,
    handleRoomCheckout,
    handleAddExtraTime,
    handleRoomUpdate,
    refreshRooms,
  } = useRoomActions()

  // Handle individual room refresh
  const handleRefreshRoom = async (roomId) => {
    try {
      // Show loading toast
      import('../utils/toast').then(({ showInfoToast }) => {
        showInfoToast(`Actualizando habitación ${roomId}...`)
      })
      
      // Refresh all rooms (since there's no per-room refresh endpoint)
      const success = await refreshRooms()
      
      if (success) {
        import('../utils/toast').then(({ showSuccessToast }) => {
          showSuccessToast(`Habitación ${roomId} actualizada correctamente`)
        })
      } else {
        import('../utils/toast').then(({ showErrorToast }) => {
          showErrorToast(`Error al actualizar habitación ${roomId}`)
        })
      }
    } catch (error) {
      console.error('Error refreshing room:', error)
      import('../utils/toast').then(({ showErrorToast }) => {
        showErrorToast(`Error al actualizar habitación ${roomId}`)
      })
    }
  }

  // Real-time updates via SignalR
  const {
    isConnected: signalRConnected,
    connectionStatus,
    lastUpdateTimestamp,
    eventStats,
    activeUpdates,
    isRoomUpdating,
    requestRoomProgressUpdates,
  } = useRoomSignalR()

  // Maintenance room modal handling
  const showMaintenanceModal = ref(false)

  const openMaintenanceRoomModal = (room) => {
    selectedRoom.value = room
    showMaintenanceModal.value = true
    document.body.style.overflow = 'hidden'
  }

  const closeMaintenanceModal = () => {
    showMaintenanceModal.value = false
    selectedRoom.value = null
    document.body.style.overflow = 'auto'
  }

  // Real-time updates: SignalR automatically updates room status, progress, and reservations
  // Manual refresh is available through the refresh button in RoomViewControls
  // Connection status and event statistics are available for debugging

  // Development mode for debugging
  const isDevelopment = import.meta.env.DEV

  // Combined free and maintenance rooms (maintenance at the end)
  const combinedFreeAndMaintenanceRooms = computed(() => {
    const freeRooms = [...filteredFreeRooms.value]
    const maintenanceRooms = [...filteredMaintenanceRooms.value]

    // Sort free rooms by room number for better organization
    freeRooms.sort((a, b) => {
      const aNum = parseInt(roomUtils.getRoomNumber(a.nombreHabitacion))
      const bNum = parseInt(roomUtils.getRoomNumber(b.nombreHabitacion))
      return aNum - bNum
    })

    // Sort maintenance rooms by room number as well
    maintenanceRooms.sort((a, b) => {
      const aNum = parseInt(roomUtils.getRoomNumber(a.nombreHabitacion))
      const bNum = parseInt(roomUtils.getRoomNumber(b.nombreHabitacion))
      return aNum - bNum
    })

    // Combine free rooms first, then maintenance rooms at the end
    return [...freeRooms, ...maintenanceRooms]
  })

  // Helper to check if this is the first maintenance room in the combined list
  const isFirstMaintenanceRoom = (room, index) => {
    if (room.estadoHabitacion !== 'Mantenimiento') return false

    // Check if this is the first maintenance room in the list
    const beforeThis = combinedFreeAndMaintenanceRooms.value.slice(0, index)
    return !beforeThis.some((r) => r.estadoHabitacion === 'Mantenimiento')
  }

  // Grid classes for responsive layout
  const freeRoomsGridClasses = computed(() => {
    const baseClasses = 'grid gap-4 min-h-[100px] overflow-y-auto pr-2 custom-scrollbar'

    if (compactMode.value) {
      return `${baseClasses} grid-cols-2 lg:grid-cols-4 xl:grid-cols-5 2xl:grid-cols-6`
    }

    return `${baseClasses} grid-cols-2 lg:grid-cols-3 xl:grid-cols-3 2xl:grid-cols-3`
  })

  const maintenanceRoomsGridClasses = computed(() => {
    const baseClasses = 'grid gap-4 min-h-[100px] overflow-y-auto pr-2 custom-scrollbar'

    if (compactMode.value) {
      return `${baseClasses} grid-cols-2 lg:grid-cols-4 xl:grid-cols-5 2xl:grid-cols-6`
    }

    return `${baseClasses} grid-cols-2 lg:grid-cols-3 xl:grid-cols-3 2xl:grid-cols-3`
  })

  const occupiedRoomsGridClasses = computed(() => {
    const baseClasses = 'grid gap-4 min-h-[100px] overflow-y-auto pr-2 custom-scrollbar'

    if (compactMode.value) {
      return `${baseClasses} grid-cols-2 lg:grid-cols-4 xl:grid-cols-5 2xl:grid-cols-6`
    }

    return `${baseClasses} grid-cols-2 lg:grid-cols-3 xl:grid-cols-3 2xl:grid-cols-3`
  })

  // Lifecycle
  onMounted(async () => {
    // Load initial data using optimized parallel loading
    console.log('🏨 Starting optimized room loading...')
    const startTime = performance.now()

    await roomsStore.fetchRoomsConditional({ parallel: true })

    const endTime = performance.now()
    console.log(`🏨 Optimized loading completed in ${Math.round(endTime - startTime)}ms`)

    // Enable real-time progress updates for occupied rooms
    // Use connectionStatus instead of signalRConnected which might be undefined
    if (connectionStatus.value === 'connected') {
      roomsStore.occupiedRooms.forEach((room) => {
        if (room.visitaID) {
          requestRoomProgressUpdates(room.habitacionId, true)
        }
      })
      console.log('📊 [SignalR] Enabled progress updates for occupied rooms')
    } else {
      console.log('⏳ [SignalR] Waiting for connection to enable progress updates...')
      // Watch for connection and then enable updates
      const unwatch = watchEffect(() => {
        if (connectionStatus.value === 'connected') {
          roomsStore.occupiedRooms.forEach((room) => {
            if (room.visitaID) {
              requestRoomProgressUpdates(room.habitacionId, true)
            }
          })
          console.log(
            '📊 [SignalR] Connection established - Enabled progress updates for occupied rooms'
          )
          unwatch() // Stop watching once connected
        }
      })
    }
  })

  onUnmounted(() => {
    // SignalR cleanup is handled automatically by useRoomSignalR composable
    console.log('🏨 [RoomsNew] Component unmounting - SignalR events will be cleaned up')
  })
</script>

<style scoped>
  /* Custom scrollbar styling */
  .custom-scrollbar::-webkit-scrollbar {
    width: 8px;
  }

  .custom-scrollbar::-webkit-scrollbar-track {
    background: rgba(255, 255, 255, 0.05);
    border-radius: 10px;
  }

  .custom-scrollbar::-webkit-scrollbar-thumb {
    background: linear-gradient(45deg, rgba(168, 85, 247, 0.5), rgba(236, 72, 153, 0.5));
    border-radius: 10px;
    border: 2px solid transparent;
    background-clip: content-box;
  }

  .custom-scrollbar::-webkit-scrollbar-thumb:hover {
    background: linear-gradient(45deg, rgba(168, 85, 247, 0.8), rgba(236, 72, 153, 0.8));
    background-clip: content-box;
  }

  /* Enhanced glassmorphism effects */
  .backdrop-blur-xl {
    backdrop-filter: blur(20px);
  }

  .backdrop-blur-md {
    backdrop-filter: blur(12px);
  }

  /* Responsive adjustments */
  @media (max-width: 1024px) {
    .grid-cols-4 {
      grid-template-columns: repeat(3, minmax(0, 1fr));
    }
  }

  @media (max-width: 768px) {
    .grid-cols-4,
    .grid-cols-3 {
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    .text-4xl {
      font-size: 2rem;
    }

    .text-3xl {
      font-size: 1.5rem;
    }
  }

  @media (max-width: 640px) {
    .grid-cols-2 {
      grid-template-columns: repeat(1, minmax(0, 1fr));
    }
  }
</style>
