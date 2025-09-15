<template>
  <div class="flex flex-col space-y-3 p-4 min-w-[280px] bg-neutral-900/95 backdrop-blur-xl rounded-xl border border-white/20">
    <!-- Glass effect overlay -->
    <div class="absolute inset-0 bg-gradient-to-br from-primary-400/10 via-secondary-400/5 to-accent-400/10 rounded-xl pointer-events-none"></div>
    
    <!-- Content -->
    <div class="relative z-10">
      <h4 class="text-sm font-semibold text-white border-b border-white/20 pb-2 flex items-center gap-2">
        <span class="material-symbols-outlined text-accent-400 text-base">schedule</span>
        Extender tiempo de ocupación
      </h4>
    
      <!-- Hours Input -->
      <div class="flex flex-col space-y-1 bg-white/5 backdrop-blur-sm rounded-lg p-3 border border-white/10">
        <label class="text-xs font-medium text-white/80 flex items-center gap-1">
          <span class="material-symbols-outlined text-primary-400 text-sm">timer</span>
          Horas
        </label>
        <InputNumber 
          v-model="hours" 
          :min="0" 
          :max="99" 
          showButtons 
          buttonLayout="horizontal"
          class="w-full glass-input-number"
          :pt="{
            root: { class: 'flex items-center' },
            input: { class: 'bg-white/10 text-white border-0 text-center font-semibold text-lg w-20 mx-2' },
            buttonGroup: { class: 'flex gap-1' },
            incrementButton: { class: 'bg-gradient-to-r from-primary-400/20 to-secondary-400/20 hover:from-primary-400/30 hover:to-secondary-400/30 text-white border border-white/20 rounded-lg p-1' },
            decrementButton: { class: 'bg-gradient-to-r from-primary-400/20 to-secondary-400/20 hover:from-primary-400/30 hover:to-secondary-400/30 text-white border border-white/20 rounded-lg p-1' }
          }"
        />
      </div>
      
      <!-- Minutes Input -->
      <div class="flex flex-col space-y-1 bg-white/5 backdrop-blur-sm rounded-lg p-3 border border-white/10">
        <label class="text-xs font-medium text-white/80 flex items-center gap-1">
          <span class="material-symbols-outlined text-secondary-400 text-sm">schedule</span>
          Minutos
        </label>
        <InputNumber 
          v-model="minutes" 
          :min="0" 
          :max="59" 
          showButtons 
          buttonLayout="horizontal"
          class="w-full glass-input-number"
          :pt="{
            root: { class: 'flex items-center' },
            input: { class: 'bg-white/10 text-white border-0 text-center font-semibold text-lg w-20 mx-2' },
            buttonGroup: { class: 'flex gap-1' },
            incrementButton: { class: 'bg-gradient-to-r from-primary-400/20 to-secondary-400/20 hover:from-primary-400/30 hover:to-secondary-400/30 text-white border border-white/20 rounded-lg p-1' },
            decrementButton: { class: 'bg-gradient-to-r from-primary-400/20 to-secondary-400/20 hover:from-primary-400/30 hover:to-secondary-400/30 text-white border border-white/20 rounded-lg p-1' }
          }"
        />
      </div>
    
      <!-- Action Buttons -->
      <div class="flex gap-2 pt-3 mt-2 border-t border-white/20">
        <button 
          @click="handleCancel"
          type="button"
          class="flex-1 px-3 py-2 text-xs font-medium text-white/80 bg-white/10 hover:bg-white/20 backdrop-blur-sm border border-white/20 rounded-lg transition-all duration-200 flex items-center justify-center gap-1"
        >
          <span class="text-sm font-bold">×</span>
          Cancelar
        </button>
        <button 
          @click="handleConfirm"
          type="button"
          :disabled="!isValid"
          class="flex-1 px-3 py-2 text-xs font-medium text-white bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 hover:from-primary-500 hover:via-secondary-500 hover:to-accent-500 backdrop-blur-sm border border-white/30 rounded-lg transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-1 shadow-lg"
        >
          <span class="material-symbols-outlined text-sm">add_circle</span>
          Agregar tiempo
        </button>
      </div>

      <!-- Helper text -->
      <div v-if="!isValid" class="flex items-center justify-center gap-1 text-xs text-amber-300/80 bg-amber-500/10 backdrop-blur-sm border border-amber-500/20 rounded-lg p-2 mt-2">
        <span class="material-symbols-outlined text-sm">info</span>
        Ingrese al menos 1 minuto
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import InputNumber from 'primevue/inputnumber'

const emit = defineEmits(['confirm', 'cancel'])

const hours = ref(0)
const minutes = ref(0)

const isValid = computed(() => {
  return hours.value > 0 || minutes.value > 0
})

const handleConfirm = () => {
  if (isValid.value) {
    emit('confirm', { hours: hours.value, minutes: minutes.value })
    // Reset values
    hours.value = 0
    minutes.value = 0
  }
}

const handleCancel = () => {
  // Reset values
  hours.value = 0
  minutes.value = 0
  emit('cancel')
}
</script>