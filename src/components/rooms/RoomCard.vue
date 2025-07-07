<template>
  <div 
    :class="cardClasses"
    @click="$emit('click', room)"
  >
    <!-- Pending orders indicator -->
    <div 
      v-if="room.pedidosPendientes" 
      class="absolute -top-1 -right-1 z-20"
    >
      <div class="bg-red-500 text-white p-2 rounded-full animate-bounce shadow-lg">
        <span class="material-symbols-outlined text-sm">notifications_active</span>
      </div>
    </div>

    <!-- Category badge -->
    <div class="absolute top-3 right-3 z-10">
      <span :class="categoryBadgeClasses">
        {{ roomUtils.getCategoryFromName(room.nombreHabitacion) }}
      </span>
    </div>

    <!-- Status indicator -->
    <div class="absolute top-3 left-3 z-10">
      <div :class="roomUtils.getStatusIndicator(room)"></div>
    </div>

    <!-- Hover effect -->
    <div class="absolute inset-0 bg-gradient-to-r from-transparent via-white/10 to-transparent -translate-x-full group-hover:translate-x-full transition-transform duration-1000"></div>
    
    <div class="relative p-5 h-full flex flex-col">
      <!-- Room icon -->
      <div :class="iconContainerClasses">
        <span class="material-symbols-outlined text-white text-xl">
          {{ roomUtils.getRoomStatus(room) === 'maintenance' ? roomUtils.getMaintenanceIcon(room) : roomUtils.getRoomIcon(room.nombreHabitacion) }}
        </span>
      </div>

      <!-- Room number and type -->
      <h3 class="text-white font-bold mb-2" :class="titleClasses">
        {{ roomUtils.getRoomNumber(room.nombreHabitacion) }}
      </h3>
      
      <div class="text-sm text-gray-300 mb-3" v-if="!isCompact">
        {{ roomUtils.getRoomType(room.nombreHabitacion) }}
      </div>

      <!-- Free room price -->
      <div v-if="roomUtils.getRoomStatus(room) === 'free' && !isCompact" class="mt-auto pt-4 border-t border-white/10">
        <div class="flex items-center justify-between">
          <div class="text-gray-400 text-sm">Precio/hora</div>
          <div class="text-green-300 font-bold text-lg">${{ room.precio }}</div>
        </div>
      </div>

      <!-- Maintenance room details -->
      <div v-if="roomUtils.getRoomStatus(room) === 'maintenance' && !isCompact" class="mt-auto pt-4 border-t border-yellow-500/20">
        <div class="space-y-2">
          <div class="flex items-center gap-2 text-yellow-300 text-sm">
            <span class="material-symbols-outlined text-xs">{{ roomUtils.getMaintenanceIcon(room) }}</span>
            <span class="font-medium">{{ roomUtils.getMaintenanceType(room) }}</span>
          </div>
          <div class="text-gray-400 text-xs">
            En mantenimiento
          </div>
        </div>
      </div>

      <!-- Occupied room details -->
      <div v-if="!room.disponible" class="space-y-2 mb-4 flex-1">
        <!-- Customer info or "Sin reserva" -->
        <div v-if="!isCompact" class="flex items-center gap-2 text-gray-300 text-sm">
          <span class="material-symbols-outlined text-xs">person</span>
          <span class="truncate">{{ room.visita?.nombreCompleto || room.visita?.identificador || 'Sin reserva' }}</span>
        </div>
        
        <!-- Time remaining -->
        <div class="flex items-center gap-2 text-sm" :class="roomUtils.getTimeTextColor(room)">
          <span class="material-symbols-outlined text-xs">schedule</span>
          <span class="font-medium">{{ roomUtils.getTimeRemaining(room) }}</span>
        </div>
      </div>

      <!-- Progress bar for occupied rooms -->
      <div v-if="!room.disponible && !isCompact" class="mb-4">
        <div class="w-full bg-black/30 rounded-full h-2 overflow-hidden">
          <div 
            class="h-2 rounded-full transition-all duration-1000 ease-out" 
            :class="roomUtils.getProgressBarColor(room)"
            :style="{ width: roomUtils.getTimeProgress(room) + '%' }"
          ></div>
        </div>
        <div class="flex justify-between text-xs text-gray-400 mt-1">
          <span>{{ roomUtils.getStatusText(room) }}</span>
          <span>{{ Math.round(roomUtils.getTimeProgress(room)) }}%</span>
        </div>
      </div>

      <!-- Action button -->
      <div class="opacity-0 group-hover:opacity-100 transition-opacity duration-300" v-if="!isCompact">
        <div :class="actionButtonClasses">
          <span class="material-symbols-outlined text-sm">{{ actionIcon }}</span>
          <span>{{ actionLabel }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue';
import { useRoomUtils } from '../../composables/rooms/useRoomUtils';

const props = defineProps({
  room: {
    type: Object,
    required: true
  },
  variant: {
    type: String,
    default: 'default', // 'default' | 'compact'
    validator: (value) => ['default', 'compact'].includes(value)
  }
});

defineEmits(['click']);

const roomUtils = useRoomUtils();

// Computed properties
const isCompact = computed(() => props.variant === 'compact');

const cardClasses = computed(() => {
  const baseClasses = 'group relative overflow-hidden rounded-2xl transition-all duration-500 cursor-pointer backdrop-blur-md hover:scale-105 hover:shadow-2xl';
  
  const roomStatus = roomUtils.getRoomStatus(props.room);
  
  switch (roomStatus) {
    case 'maintenance':
      return `${baseClasses} bg-gradient-to-br from-yellow-900/20 to-yellow-800/20 border border-yellow-500/50 hover:border-yellow-400/70 hover:shadow-yellow-500/25`;
    case 'free':
      return `${baseClasses} bg-gradient-to-br from-white/10 to-white/5 border border-white/20 hover:border-green-400/50 hover:shadow-green-500/25`;
    case 'occupied':
    default:
      return `${baseClasses} ${roomUtils.getOccupiedRoomStyles(props.room)}`;
  }
});

const titleClasses = computed(() => {
  if (isCompact.value) return 'text-lg';
  
  const roomStatus = roomUtils.getRoomStatus(props.room);
  switch (roomStatus) {
    case 'maintenance':
      return 'text-lg group-hover:text-yellow-300 transition-colors duration-300';
    case 'free':
      return 'text-lg group-hover:text-green-300 transition-colors duration-300';
    case 'occupied':
    default:
      return 'text-lg';
  }
});

const iconContainerClasses = computed(() => {
  const baseClasses = 'mb-4 rounded-xl flex items-center justify-center border group-hover:scale-110 transition-transform duration-300';
  const sizeClasses = isCompact.value ? 'w-10 h-10' : 'w-12 h-12';
  
  const roomStatus = roomUtils.getRoomStatus(props.room);
  
  switch (roomStatus) {
    case 'maintenance':
      return `${baseClasses} ${sizeClasses} bg-gradient-to-r from-yellow-400/20 to-amber-500/20 border-yellow-400/30`;
    case 'free':
      return `${baseClasses} ${sizeClasses} bg-gradient-to-r from-green-400/20 to-emerald-500/20 border-green-400/30`;
    case 'occupied':
    default:
      return `${baseClasses} ${sizeClasses} bg-black/20 backdrop-blur-sm border-white/20`;
  }
});

const categoryBadgeClasses = computed(() => {
  const baseClasses = 'px-2 py-1 rounded-lg text-xs font-medium backdrop-blur-sm';
  
  const roomStatus = roomUtils.getRoomStatus(props.room);
  
  switch (roomStatus) {
    case 'maintenance':
      return `${baseClasses} bg-yellow-500/20 border border-yellow-400/30 text-yellow-300`;
    case 'free':
      return `${baseClasses} bg-green-500/20 border border-green-400/30 text-green-300`;
    case 'occupied':
    default:
      return `${baseClasses} bg-black/30 border border-white/20 text-white`;
  }
});

const actionIcon = computed(() => {
  const roomStatus = roomUtils.getRoomStatus(props.room);
  
  switch (roomStatus) {
    case 'maintenance':
      return 'task_alt';
    case 'free':
      return 'add_circle';
    case 'occupied':
    default:
      return 'manage_accounts';
  }
});

const actionLabel = computed(() => {
  const roomStatus = roomUtils.getRoomStatus(props.room);
  
  switch (roomStatus) {
    case 'maintenance':
      return 'Finalizar';
    case 'free':
      return 'Ocupar';
    case 'occupied':
    default:
      return 'Gestionar';
  }
});

const actionButtonClasses = computed(() => {
  const baseClasses = 'flex items-center justify-center gap-2 text-sm font-medium py-2 rounded-xl border transition-colors';
  
  const roomStatus = roomUtils.getRoomStatus(props.room);
  
  switch (roomStatus) {
    case 'maintenance':
      return `${baseClasses} text-yellow-300 bg-yellow-500/10 border-yellow-400/30 hover:bg-yellow-500/20`;
    case 'free':
      return `${baseClasses} text-green-300 bg-green-500/10 border-green-400/30 hover:bg-green-500/20`;
    case 'occupied':
    default:
      return `${baseClasses} text-white bg-black/20 border-white/20 hover:bg-black/30`;
  }
});
</script>