<template>
  <div class="performance-panel glass-card p-4 w-80">
    <h3 class="text-white font-semibold mb-4 flex items-center gap-2">
      <i class="pi pi-cog" />
      Optimización de Rendimiento
    </h3>
    
    <!-- Current Performance Level -->
    <div class="mb-4">
      <label class="text-white text-sm block mb-2">Nivel Actual:</label>
      <div class="flex items-center gap-2 mb-2">
        <div 
          :class="[
            'w-3 h-3 rounded-full',
            levelColors[performanceLevel]
          ]"
        />
        <span class="text-white font-medium capitalize">
          {{ performanceLevel }}
        </span>
      </div>
    </div>

    <!-- Device Information -->
    <div class="mb-4" v-if="deviceInfo.memory || deviceInfo.cores">
      <label class="text-white text-sm block mb-2">Dispositivo:</label>
      <div class="text-gray-300 text-xs space-y-1">
        <div v-if="deviceInfo.memory">
          RAM: {{ deviceInfo.memory }}GB
        </div>
        <div v-if="deviceInfo.cores">
          CPU: {{ deviceInfo.cores }} cores
        </div>
        <div v-if="deviceInfo.connection">
          Red: {{ deviceInfo.connection.effectiveType }}
        </div>
      </div>
    </div>

    <!-- Performance Score -->
    <div class="mb-4">
      <label class="text-white text-sm block mb-2">Puntuación:</label>
      <div class="flex items-center gap-2">
        <div class="flex-1 bg-gray-700 rounded-full h-2">
          <div 
            class="h-2 rounded-full transition-all duration-300"
            :class="scoreColor"
            :style="{ width: `${score}%` }"
          />
        </div>
        <span class="text-white text-sm">{{ score }}/100</span>
      </div>
    </div>

    <!-- Manual Controls -->
    <div class="mb-4">
      <label class="text-white text-sm block mb-2">Control Manual:</label>
      <div class="grid grid-cols-2 gap-2">
        <button 
          v-for="level in Object.values(PERFORMANCE_LEVELS)"
          :key="level"
          @click="setPerformanceLevel(level)"
          :class="[
            'px-3 py-2 rounded-lg text-xs font-medium transition-all',
            performanceLevel === level 
              ? 'bg-primary-500 text-white' 
              : 'glass-button text-white/80 hover:text-white'
          ]"
        >
          {{ level.toUpperCase() }}
        </button>
      </div>
    </div>

    <!-- Performance Tips -->
    <div v-if="performanceTips[performanceLevel]" class="mb-4">
      <label class="text-white text-sm block mb-2">Consejos:</label>
      <p class="text-gray-300 text-xs">
        {{ performanceTips[performanceLevel] }}
      </p>
    </div>

    <!-- Quick Actions -->
    <div class="flex gap-2">
      <button 
        @click="enableHighPerformance"
        class="glass-button px-3 py-2 rounded-lg text-xs flex-1"
        :disabled="performanceLevel === 'high'"
      >
        Máximo
      </button>
      <button 
        @click="enableLowPerformance"
        class="glass-button px-3 py-2 rounded-lg text-xs flex-1"
        :disabled="performanceLevel === 'minimal'"
      >
        Mínimo
      </button>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { usePerformanceOptimization } from '../composables/usePerformanceOptimization.js'

const {
  performanceLevel,
  deviceInfo,
  setPerformanceLevel,
  enableHighPerformance,
  enableLowPerformance,
  getPerformanceInfo,
  PERFORMANCE_LEVELS
} = usePerformanceOptimization()

// Computed properties
const performanceInfo = computed(() => getPerformanceInfo())
const score = computed(() => performanceInfo.value.score)

const levelColors = {
  high: 'bg-green-500',
  medium: 'bg-yellow-500', 
  low: 'bg-orange-500',
  minimal: 'bg-red-500'
}

const scoreColor = computed(() => {
  if (score.value >= 80) return 'bg-green-500'
  if (score.value >= 60) return 'bg-yellow-500'
  if (score.value >= 40) return 'bg-orange-500'
  return 'bg-red-500'
})

const performanceTips = {
  high: 'Todos los efectos glassmorphism están activos. Tu dispositivo puede manejar la máxima calidad visual.',
  medium: 'Efectos glassmorphism moderados. Buen balance entre calidad y rendimiento.',
  low: 'Efectos simplificados para mejorar el rendimiento. Recomendado para dispositivos más antiguos.',
  minimal: 'Efectos deshabilitados. Máxima compatibilidad y velocidad para dispositivos muy antiguos.'
}
</script>

<style scoped>
.performance-panel {
  position: fixed;
  bottom: 20px;
  right: 20px;
  z-index: 1000;
  max-height: calc(100vh - 100px);
  overflow-y: auto;
}
</style>