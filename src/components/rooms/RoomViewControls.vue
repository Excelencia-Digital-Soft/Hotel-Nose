<template>
  <div class="flex flex-wrap gap-3">
    <!-- View mode toggle -->
    <button 
      @click="$emit('toggleViewMode')"
      class="relative group bg-gradient-to-r from-primary-500 to-secondary-500 hover:from-primary-400 hover:to-secondary-400 rounded-2xl flex items-center transition-all duration-300 shadow-lg hover:shadow-primary-500/25 hover:scale-105" 
      :class="buttonClasses"
    >
      <span class="material-symbols-outlined text-white" :class="iconClasses">{{ viewModeIcon }}</span>
      <span v-if="!compactMode" class="text-white font-medium">{{ viewModeLabel }}</span>
    </button>
    
    <!-- Compact mode toggle -->
    <button 
      @click="$emit('toggleCompactMode')"
      class="relative group bg-gradient-to-r from-purple-500 to-violet-500 hover:from-purple-400 hover:to-violet-400 rounded-2xl flex items-center transition-all duration-300 shadow-lg hover:shadow-purple-500/25 hover:scale-105" 
      :class="buttonClasses"
    >
      <span class="material-symbols-outlined text-white" :class="iconClasses">{{ compactModeIcon }}</span>
      <span v-if="!compactMode" class="text-white font-medium">{{ compactModeLabel }}</span>
    </button>

    <!-- Refresh button -->
    <button 
      @click="$emit('refresh')"
      :disabled="isRefreshing"
      class="relative group bg-gradient-to-r from-green-500 to-emerald-500 hover:from-green-400 hover:to-emerald-400 disabled:from-gray-500 disabled:to-gray-600 disabled:cursor-not-allowed rounded-2xl flex items-center transition-all duration-300 shadow-lg hover:shadow-green-500/25 hover:scale-105 disabled:hover:scale-100" 
      :class="buttonClasses"
    >
      <span 
        class="material-symbols-outlined text-white transition-transform duration-300" 
        :class="[iconClasses, { 'animate-spin': isRefreshing }]"
      >
        refresh
      </span>
      <span v-if="!compactMode" class="text-white font-medium">
        {{ isRefreshing ? 'Actualizando...' : 'Actualizar' }}
      </span>
    </button>

    <!-- SignalR status indicator -->
    <div 
      class="relative group rounded-2xl flex items-center transition-all duration-300 shadow-lg" 
      :class="[buttonClasses, signalRStatusClasses]"
      :title="signalRStatusTitle"
    >
      <span 
        class="material-symbols-outlined text-white" 
        :class="[iconClasses, signalRIconClasses]"
      >
        {{ signalRIcon }}
      </span>
      <span v-if="!compactMode" class="text-white font-medium">
        {{ signalRLabel }}
      </span>
    </div>

    <!-- Full screen toggle (optional) -->
    <button 
      v-if="showFullscreenToggle"
      @click="$emit('toggleFullscreen')"
      class="relative group bg-gradient-to-r from-gray-500 to-gray-600 hover:from-gray-400 hover:to-gray-500 rounded-2xl flex items-center transition-all duration-300 shadow-lg hover:shadow-gray-500/25 hover:scale-105" 
      :class="buttonClasses"
    >
      <span class="material-symbols-outlined text-white" :class="iconClasses">
        {{ isFullscreen ? 'fullscreen_exit' : 'fullscreen' }}
      </span>
      <span v-if="!compactMode" class="text-white font-medium">
        {{ isFullscreen ? 'Salir' : 'Pantalla completa' }}
      </span>
    </button>
  </div>
</template>

<script setup>
import { computed } from 'vue';
import { useSignalRStore } from '../../store/signalr';

// SignalR store
const signalRStore = useSignalRStore();

const props = defineProps({
  // View state
  viewMode: {
    type: String,
    default: 'grid'
  },
  compactMode: {
    type: Boolean,
    default: false
  },
  
  // Refresh state
  isRefreshing: {
    type: Boolean,
    default: false
  },
  
  // Fullscreen state
  isFullscreen: {
    type: Boolean,
    default: false
  },
  showFullscreenToggle: {
    type: Boolean,
    default: false
  }
});

defineEmits([
  'toggleViewMode',
  'toggleCompactMode',
  'refresh',
  'toggleFullscreen'
]);

// Computed classes
const buttonClasses = computed(() => {
  return props.compactMode ? 'px-4 py-2 gap-2' : 'px-6 py-3 gap-3';
});

const iconClasses = computed(() => {
  return props.compactMode ? 'text-lg' : '';
});


// Computed labels and icons
const viewModeIcon = computed(() => {
  return props.viewMode === 'grid' ? 'view_list' : 'grid_view';
});

const viewModeLabel = computed(() => {
  return props.viewMode === 'grid' ? 'Lista' : 'Grid';
});

const compactModeIcon = computed(() => {
  return props.compactMode ? 'unfold_more' : 'unfold_less';
});

const compactModeLabel = computed(() => {
  return props.compactMode ? 'Expandir' : 'Compacto';
});

// SignalR status computed properties
const signalRStatusClasses = computed(() => {
  const baseClasses = 'bg-gradient-to-r';
  
  switch (signalRStore.connectionState) {
    case 'connected':
      return `${baseClasses} from-green-600 to-emerald-600`;
    case 'connecting':
    case 'reconnecting':
      return `${baseClasses} from-yellow-600 to-amber-600`;
    case 'disconnected':
    case 'disconnecting':
    default:
      return `${baseClasses} from-red-600 to-red-700`;
  }
});

const signalRIcon = computed(() => {
  switch (signalRStore.connectionState) {
    case 'connected':
      return 'wifi';
    case 'connecting':
    case 'reconnecting':
      return 'wifi_tethering';
    case 'disconnected':
    case 'disconnecting':
    default:
      return 'wifi_off';
  }
});

const signalRIconClasses = computed(() => {
  const baseClasses = '';
  
  switch (signalRStore.connectionState) {
    case 'connected':
      return `${baseClasses} animate-pulse`;
    case 'connecting':
    case 'reconnecting':
      return `${baseClasses} animate-spin`;
    case 'disconnected':
    case 'disconnecting':
    default:
      return baseClasses;
  }
});

const signalRLabel = computed(() => {
  switch (signalRStore.connectionState) {
    case 'connected':
      return 'Tiempo Real';
    case 'connecting':
      return 'Conectando...';
    case 'reconnecting':
      return 'Reconectando...';
    case 'disconnecting':
      return 'Desconectando...';
    case 'disconnected':
    default:
      return 'Desconectado';
  }
});

const signalRStatusTitle = computed(() => {
  switch (signalRStore.connectionState) {
    case 'connected':
      return 'Actualizaciones en tiempo real activas via SignalR';
    case 'connecting':
      return 'Conectando a SignalR...';
    case 'reconnecting':
      return 'Reconectando a SignalR...';
    case 'disconnecting':
      return 'Desconectando de SignalR...';
    case 'disconnected':
    default:
      return 'Sin conexión SignalR - Las actualizaciones en tiempo real no están disponibles';
  }
});
</script>