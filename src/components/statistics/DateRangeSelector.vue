<template>
  <div class="glass-card p-6">
    <div class="flex items-center mb-4">
      <i class="pi pi-calendar text-primary-400 text-xl mr-2"></i>
      <h3 class="text-xl font-bold text-white">📅 Selector de Período</h3>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <!-- Quick Presets -->
      <div>
        <label class="block text-white font-semibold mb-3">
          <i class="pi pi-clock text-accent-400 mr-2"></i>
          Períodos rápidos
        </label>
        
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
          <button
            v-for="(preset, key) in quickPresets"
            :key="key"
            @click="selectPreset(key)"
            class="glass-button p-3 text-white hover:bg-white/20 transform hover:scale-105 transition-all text-left"
            :class="{ 'bg-primary-500/50 border-primary-400': selectedPreset === key }"
          >
            <div class="flex items-center">
              <i :class="preset.icon" class="text-primary-400 mr-3"></i>
              <div>
                <p class="font-semibold text-sm">{{ preset.label }}</p>
                <p class="text-gray-300 text-xs">{{ preset.description }}</p>
              </div>
            </div>
          </button>
        </div>
      </div>

      <!-- Custom Range -->
      <div>
        <label class="block text-white font-semibold mb-3">
          <i class="pi pi-calendar text-accent-400 mr-2"></i>
          Período personalizado
        </label>
        
        <div class="space-y-4">
          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="block text-gray-300 text-sm mb-2">Fecha inicio</label>
              <div class="relative">
                <input
                  v-model="customRange.start"
                  type="date"
                  class="glass-input w-full px-3 py-2 pr-10"
                  :max="maxStartDate"
                  @change="validateAndEmit"
                />
                <i class="pi pi-calendar absolute right-3 top-3 text-gray-400"></i>
              </div>
            </div>
            
            <div>
              <label class="block text-gray-300 text-sm mb-2">Fecha fin</label>
              <div class="relative">
                <input
                  v-model="customRange.end"
                  type="date"
                  class="glass-input w-full px-3 py-2 pr-10"
                  :min="customRange.start"
                  :max="today"
                  @change="validateAndEmit"
                />
                <i class="pi pi-calendar absolute right-3 top-3 text-gray-400"></i>
              </div>
            </div>
          </div>

          <!-- Date Range Display -->
          <div v-if="isCustomRangeValid" class="glass-card p-3 bg-green-500/20 border-green-500/30">
            <div class="flex items-center">
              <i class="pi pi-check-circle text-green-400 mr-2"></i>
              <div>
                <p class="text-green-300 font-semibold text-sm">Período seleccionado:</p>
                <p class="text-white text-sm">{{ formatRangeDisplay }}</p>
                <p class="text-gray-300 text-xs">{{ rangeDurationText }}</p>
              </div>
            </div>
          </div>

          <!-- Validation Errors -->
          <div v-if="validationErrors.length > 0" class="glass-card p-3 bg-red-500/20 border-red-500/30">
            <div class="flex items-start">
              <i class="pi pi-exclamation-triangle text-red-400 mr-2 mt-0.5"></i>
              <div>
                <p class="text-red-300 font-semibold text-sm">Errores de validación:</p>
                <ul class="text-red-300 text-sm mt-1 space-y-1">
                  <li v-for="error in validationErrors" :key="error">• {{ error }}</li>
                </ul>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Action Buttons -->
    <div class="flex items-center justify-between mt-6">
      <div class="flex items-center space-x-3">
        <button
          @click="applyRange"
          :disabled="!isValidRange"
          class="bg-gradient-to-r from-primary-400 to-accent-400 
                 hover:from-primary-500 hover:to-accent-500 
                 disabled:opacity-50 disabled:cursor-not-allowed
                 text-white font-bold py-3 px-6 rounded-lg 
                 transition-all duration-300 transform hover:scale-105"
        >
          <i class="pi pi-check mr-2"></i>
          ✅ Aplicar Período
        </button>
        
        <button
          @click="resetToDefault"
          class="glass-button py-3 px-4 text-white hover:text-blue-300 transform hover:scale-105 transition-all"
        >
          <i class="pi pi-refresh mr-2"></i>
          🔄 Restablecer
        </button>
      </div>

      <!-- Current Selection Info -->
      <div v-if="currentRange" class="glass-card px-3 py-2">
        <p class="text-accent-400 text-sm font-semibold">
          <i class="pi pi-info-circle mr-1"></i>
          {{ currentRangeText }}
        </p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'

// Props
const props = defineProps({
  modelValue: {
    type: Object,
    default: null
  },
  institucionID: {
    type: Number,
    required: true
  }
})

// Emits
const emit = defineEmits(['update:modelValue', 'apply'])

// Reactive state
const selectedPreset = ref('lastMonth')
const customRange = ref({
  start: '',
  end: ''
})

// Get today's date
const today = computed(() => {
  return new Date().toISOString().split('T')[0]
})

const maxStartDate = computed(() => {
  return today.value
})

// Quick presets configuration
const quickPresets = {
  today: {
    label: 'Hoy',
    description: 'Solo hoy',
    icon: 'pi pi-sun',
    getDates: () => {
      const today = new Date()
      return {
        start: today.toISOString().split('T')[0],
        end: today.toISOString().split('T')[0]
      }
    }
  },
  yesterday: {
    label: 'Ayer',
    description: 'Solo ayer',
    icon: 'pi pi-moon',
    getDates: () => {
      const yesterday = new Date()
      yesterday.setDate(yesterday.getDate() - 1)
      return {
        start: yesterday.toISOString().split('T')[0],
        end: yesterday.toISOString().split('T')[0]
      }
    }
  },
  lastWeek: {
    label: 'Última semana',
    description: 'Últimos 7 días',
    icon: 'pi pi-calendar',
    getDates: () => {
      const end = new Date()
      const start = new Date()
      start.setDate(start.getDate() - 7)
      return {
        start: start.toISOString().split('T')[0],
        end: end.toISOString().split('T')[0]
      }
    }
  },
  lastMonth: {
    label: 'Último mes',
    description: 'Últimos 30 días',
    icon: 'pi pi-calendar-plus',
    getDates: () => {
      const end = new Date()
      const start = new Date()
      start.setDate(start.getDate() - 30)
      return {
        start: start.toISOString().split('T')[0],
        end: end.toISOString().split('T')[0]
      }
    }
  },
  last3Months: {
    label: 'Últimos 3 meses',
    description: 'Últimos 90 días',
    icon: 'pi pi-calendar-times',
    getDates: () => {
      const end = new Date()
      const start = new Date()
      start.setDate(start.getDate() - 90)
      return {
        start: start.toISOString().split('T')[0],
        end: end.toISOString().split('T')[0]
      }
    }
  },
  thisMonth: {
    label: 'Este mes',
    description: 'Desde inicio de mes',
    icon: 'pi pi-forward',
    getDates: () => {
      const end = new Date()
      const start = new Date(end.getFullYear(), end.getMonth(), 1)
      return {
        start: start.toISOString().split('T')[0],
        end: end.toISOString().split('T')[0]
      }
    }
  },
  thisYear: {
    label: 'Este año',
    description: 'Desde enero',
    icon: 'pi pi-star',
    getDates: () => {
      const end = new Date()
      const start = new Date(end.getFullYear(), 0, 1)
      return {
        start: start.toISOString().split('T')[0],
        end: end.toISOString().split('T')[0]
      }
    }
  },
  custom: {
    label: 'Personalizado',
    description: 'Seleccionar fechas',
    icon: 'pi pi-cog',
    getDates: () => customRange.value
  }
}

// Computed properties
const currentRange = computed(() => {
  if (selectedPreset.value === 'custom') {
    return customRange.value
  }
  return quickPresets[selectedPreset.value]?.getDates() || null
})

const isCustomRangeValid = computed(() => {
  return customRange.value.start && 
         customRange.value.end && 
         customRange.value.start <= customRange.value.end
})

const validationErrors = computed(() => {
  const errors = []
  
  if (selectedPreset.value === 'custom') {
    if (!customRange.value.start) {
      errors.push('Fecha de inicio es requerida')
    }
    
    if (!customRange.value.end) {
      errors.push('Fecha de fin es requerida')
    }
    
    if (customRange.value.start && customRange.value.end) {
      const start = new Date(customRange.value.start)
      const end = new Date(customRange.value.end)
      
      if (start > end) {
        errors.push('La fecha de inicio debe ser anterior a la fecha de fin')
      }
      
      if (end > new Date()) {
        errors.push('La fecha de fin no puede ser futura')
      }
      
      // Check if range is too large (more than 1 year)
      const diffTime = Math.abs(end - start)
      const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24))
      
      if (diffDays > 365) {
        errors.push('El período no puede ser mayor a 1 año')
      }
    }
  }
  
  return errors
})

const isValidRange = computed(() => {
  if (selectedPreset.value === 'custom') {
    return isCustomRangeValid.value && validationErrors.value.length === 0
  }
  return true
})

const formatRangeDisplay = computed(() => {
  if (!currentRange.value) return ''
  
  const start = new Date(currentRange.value.start)
  const end = new Date(currentRange.value.end)
  
  const formatter = new Intl.DateTimeFormat('es-ES', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })
  
  if (currentRange.value.start === currentRange.value.end) {
    return formatter.format(start)
  }
  
  return `${formatter.format(start)} - ${formatter.format(end)}`
})

const rangeDurationText = computed(() => {
  if (!currentRange.value || !currentRange.value.start || !currentRange.value.end) return ''
  
  const start = new Date(currentRange.value.start)
  const end = new Date(currentRange.value.end)
  const diffTime = Math.abs(end - start)
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1
  
  if (diffDays === 1) {
    return '1 día'
  } else if (diffDays <= 7) {
    return `${diffDays} días`
  } else if (diffDays <= 30) {
    const weeks = Math.floor(diffDays / 7)
    const remainingDays = diffDays % 7
    if (remainingDays === 0) {
      return `${weeks} ${weeks === 1 ? 'semana' : 'semanas'}`
    }
    return `${weeks} ${weeks === 1 ? 'semana' : 'semanas'} y ${remainingDays} ${remainingDays === 1 ? 'día' : 'días'}`
  } else {
    const months = Math.floor(diffDays / 30)
    const remainingDays = diffDays % 30
    if (remainingDays === 0) {
      return `${months} ${months === 1 ? 'mes' : 'meses'}`
    }
    return `Aproximadamente ${months} ${months === 1 ? 'mes' : 'meses'}`
  }
})

const currentRangeText = computed(() => {
  if (selectedPreset.value === 'custom') {
    return `Período personalizado: ${formatRangeDisplay.value}`
  }
  return `${quickPresets[selectedPreset.value]?.label}: ${formatRangeDisplay.value}`
})

// Methods
const selectPreset = (presetKey) => {
  selectedPreset.value = presetKey
  
  if (presetKey !== 'custom') {
    const dates = quickPresets[presetKey].getDates()
    customRange.value = { ...dates }
    validateAndEmit()
  }
}

const validateAndEmit = () => {
  if (isValidRange.value && currentRange.value) {
    const rangeData = {
      fechaInicio: currentRange.value.start,
      fechaFin: currentRange.value.end,
      institucionID: props.institucionID
    }
    
    emit('update:modelValue', rangeData)
  }
}

const applyRange = () => {
  if (isValidRange.value && currentRange.value) {
    const rangeData = {
      fechaInicio: currentRange.value.start,
      fechaFin: currentRange.value.end,
      institucionID: props.institucionID
    }
    
    emit('apply', rangeData)
  }
}

const resetToDefault = () => {
  selectedPreset.value = 'lastMonth'
  selectPreset('lastMonth')
}

// Watchers
watch(() => props.modelValue, (newValue) => {
  if (newValue && newValue.fechaInicio && newValue.fechaFin) {
    customRange.value = {
      start: newValue.fechaInicio,
      end: newValue.fechaFin
    }
    
    // Try to match with a preset
    let matchedPreset = null
    for (const [key, preset] of Object.entries(quickPresets)) {
      if (key !== 'custom') {
        const presetDates = preset.getDates()
        if (presetDates.start === newValue.fechaInicio && presetDates.end === newValue.fechaFin) {
          matchedPreset = key
          break
        }
      }
    }
    
    selectedPreset.value = matchedPreset || 'custom'
  }
}, { immediate: true })

// Initialize with default
onMounted(() => {
  if (!props.modelValue) {
    selectPreset('lastMonth')
  }
})
</script>

<style scoped>
.glass-card {
  @apply bg-white/10 backdrop-blur-md border border-white/20 rounded-xl;
}

.glass-button {
  @apply bg-white/10 hover:bg-white/20 backdrop-blur-sm border border-white/30 rounded-lg transition-all;
}

.glass-input {
  @apply bg-white/10 backdrop-blur-sm border border-white/30 rounded-lg text-white placeholder-gray-300;
}

.glass-input:focus {
  @apply ring-2 ring-primary-400 border-primary-400 outline-none;
}

/* Custom date input styles */
input[type="date"]::-webkit-calendar-picker-indicator {
  filter: invert(1);
  cursor: pointer;
}

input[type="date"]::-webkit-datetime-edit {
  color: white;
}
</style>