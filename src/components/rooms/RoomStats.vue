<template>
  <div :class="containerClasses">
    <!-- Free rooms stat -->
    <div class="group relative">
      <div class="absolute inset-0 bg-gradient-to-r from-green-500/20 to-emerald-500/20 rounded-2xl blur opacity-75 group-hover:opacity-100 transition-opacity duration-300"></div>
      <div class="relative bg-white/5 backdrop-blur-md border border-green-500/20 rounded-2xl hover:bg-white/10 transition-all duration-300" :class="statCardClasses">
        <div class="flex items-center" :class="statContentClasses">
          <div :class="statIconClasses('green')">
            <span class="material-symbols-outlined text-green-300">hotel_class</span>
          </div>
          <div>
            <p :class="statNumberClasses" class="text-green-300">{{ stats.free }}</p>
            <p :class="statLabelClasses" class="text-green-200/70">{{ freeRoomsLabel }}</p>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Occupied rooms stat -->
    <div class="group relative">
      <div class="absolute inset-0 bg-gradient-to-r from-red-500/20 to-rose-500/20 rounded-2xl blur opacity-75 group-hover:opacity-100 transition-opacity duration-300"></div>
      <div class="relative bg-white/5 backdrop-blur-md border border-red-500/20 rounded-2xl hover:bg-white/10 transition-all duration-300" :class="statCardClasses">
        <div class="flex items-center" :class="statContentClasses">
          <div :class="statIconClasses('red')">
            <span class="material-symbols-outlined text-red-300">hotel</span>
          </div>
          <div>
            <p :class="statNumberClasses" class="text-red-300">{{ stats.occupied }}</p>
            <p :class="statLabelClasses" class="text-red-200/70">{{ occupiedRoomsLabel }}</p>
          </div>
        </div>
      </div>
    </div>
    
    <!-- About to expire stat -->
    <div class="group relative">
      <div class="absolute inset-0 bg-gradient-to-r from-yellow-500/20 to-amber-500/20 rounded-2xl blur opacity-75 group-hover:opacity-100 transition-opacity duration-300"></div>
      <div class="relative bg-white/5 backdrop-blur-md border border-yellow-500/20 rounded-2xl hover:bg-white/10 transition-all duration-300" :class="statCardClasses">
        <div class="flex items-center" :class="statContentClasses">
          <div :class="statIconClasses('yellow')">
            <span class="material-symbols-outlined text-yellow-300">schedule</span>
          </div>
          <div>
            <p :class="statNumberClasses" class="text-yellow-300">{{ stats.aboutToExpire }}</p>
            <p :class="statLabelClasses" class="text-yellow-200/70">{{ aboutToExpireLabel }}</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Occupancy rate stat (only in normal mode) -->
    <div v-if="!compactMode" class="group relative">
      <div class="absolute inset-0 bg-gradient-to-r from-blue-500/20 to-cyan-500/20 rounded-2xl blur opacity-75 group-hover:opacity-100 transition-opacity duration-300"></div>
      <div class="relative bg-white/5 backdrop-blur-md border border-blue-500/20 rounded-2xl hover:bg-white/10 transition-all duration-300" :class="statCardClasses">
        <div class="flex items-center" :class="statContentClasses">
          <div :class="statIconClasses('blue')">
            <span class="material-symbols-outlined text-blue-300">analytics</span>
          </div>
          <div>
            <p :class="statNumberClasses" class="text-blue-300">{{ stats.occupancyRate }}%</p>
            <p :class="statLabelClasses" class="text-blue-200/70">Ocupación</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Total rooms stat (only in normal mode) -->
    <div v-if="!compactMode" class="group relative">
      <div class="absolute inset-0 bg-gradient-to-r from-purple-500/20 to-violet-500/20 rounded-2xl blur opacity-75 group-hover:opacity-100 transition-opacity duration-300"></div>
      <div class="relative bg-white/5 backdrop-blur-md border border-purple-500/20 rounded-2xl hover:bg-white/10 transition-all duration-300" :class="statCardClasses">
        <div class="flex items-center" :class="statContentClasses">
          <div :class="statIconClasses('purple')">
            <span class="material-symbols-outlined text-purple-300">domain</span>
          </div>
          <div>
            <p :class="statNumberClasses" class="text-purple-300">{{ stats.total }}</p>
            <p :class="statLabelClasses" class="text-purple-200/70">Total</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue';

const props = defineProps({
  stats: {
    type: Object,
    required: true,
    default: () => ({
      total: 0,
      free: 0,
      occupied: 0,
      aboutToExpire: 0,
      occupancyRate: 0
    })
  },
  compactMode: {
    type: Boolean,
    default: false
  }
});

// Computed classes
const containerClasses = computed(() => {
  if (props.compactMode) {
    return 'flex flex-wrap gap-3 mt-4';
  }
  return 'grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4 mt-8';
});

const statCardClasses = computed(() => {
  return props.compactMode ? 'px-4 py-2' : 'p-6';
});

const statContentClasses = computed(() => {
  return props.compactMode ? 'gap-3' : 'gap-4';
});

const statIconClasses = (color) => {
  const sizeClasses = props.compactMode ? 'w-8 h-8' : 'w-12 h-12';
  const baseClasses = `${sizeClasses} backdrop-blur-md rounded-xl flex items-center justify-center border`;
  
  const colorClasses = {
    green: 'bg-green-500/20 border-green-500/30',
    red: 'bg-red-500/20 border-red-500/30',
    yellow: 'bg-yellow-500/20 border-yellow-500/30',
    blue: 'bg-blue-500/20 border-blue-500/30',
    purple: 'bg-purple-500/20 border-purple-500/30'
  };
  
  return `${baseClasses} ${colorClasses[color]}`;
};

const statNumberClasses = computed(() => {
  return props.compactMode ? 'text-xl font-bold' : 'text-3xl font-bold';
});

const statLabelClasses = computed(() => {
  return props.compactMode ? 'text-xs ml-1' : 'text-sm font-medium';
});

// Computed labels
const freeRoomsLabel = computed(() => {
  return props.compactMode ? 'Libres' : 'Hab. Libres';
});

const occupiedRoomsLabel = computed(() => {
  return props.compactMode ? 'Ocupadas' : 'Hab. Ocupadas';
});

const aboutToExpireLabel = computed(() => {
  return props.compactMode ? 'Por Vencer' : 'Por Vencer';
});
</script>
