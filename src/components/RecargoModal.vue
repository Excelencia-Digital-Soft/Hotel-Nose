<template>
  <Teleport to="body">
    <Transition name="modal-outer" appear>
      <div class="fixed inset-0 bg-black/60 backdrop-blur-xl flex justify-center items-center z-[70] p-4">
        <Transition name="modal-inner">
          <div class="relative glass-container max-w-md w-full">
            <!-- Header -->
            <div class="glass-card p-6 mb-4 border-b border-white/10">
              <div class="flex items-center justify-between">
                <div class="flex items-center">
                  <div class="bg-gradient-to-r from-orange-400 to-red-400 p-3 rounded-full mr-4">
                    <i class="pi pi-plus-circle text-white text-xl"></i>
                  </div>
                  <div>
                    <h2 class="text-xl font-bold text-white mb-1">📈 Recargo</h2>
                    <p class="text-gray-300 text-sm">Aplicar recargo adicional</p>
                  </div>
                </div>
                
                <button 
                  @click="emit('close')" 
                  class="glass-button p-3 text-white hover:text-red-300 hover:bg-red-500/20 transition-all rounded-full"
                >
                  <i class="pi pi-times text-lg"></i>
                </button>
              </div>
            </div>

            <!-- Form -->
            <div class="glass-card p-6 mb-4">
              <div class="space-y-4">
                <!-- Descripción -->
                <div>
                  <label class="flex items-center mb-3">
                    <i class="pi pi-file-edit text-orange-400 mr-2"></i>
                    <span class="text-white font-semibold">Motivo del Recargo</span>
                    <span class="text-red-400 ml-1">*</span>
                  </label>
                  <textarea
                    v-model="descripcion"
                    class="glass-input w-full px-4 py-3 h-20 resize-none"
                    :class="{ 'border-red-500/50': descripcionError }"
                    placeholder="Describe el motivo del recargo..."
                  ></textarea>
                  <p v-if="descripcionError" class="text-red-400 text-sm mt-1">
                    La descripción es obligatoria
                  </p>
                </div>

                <!-- Monto -->
                <div>
                  <label class="flex items-center mb-3">
                    <i class="pi pi-money-bill text-green-400 mr-2"></i>
                    <span class="text-white font-semibold">Monto del Recargo</span>
                    <span class="text-red-400 ml-1">*</span>
                  </label>
                  <input
                    type="number"
                    v-model.number="monto"
                    class="glass-input w-full px-4 py-3 text-lg font-bold text-center"
                    :class="{ 'border-red-500/50': montoError }"
                    placeholder="0.00"
                    step="0.01"
                    min="0"
                  />
                  <p v-if="montoError" class="text-red-400 text-sm mt-1">
                    El monto debe ser mayor que 0
                  </p>
                </div>
              </div>
            </div>

            <!-- Action Buttons -->
            <div class="glass-card p-6">
              <div class="flex gap-3">
                <button 
                  @click="emit('close')" 
                  class="flex-1 glass-button px-4 py-3 text-white hover:bg-red-500/20 transition-all rounded-lg"
                >
                  <i class="pi pi-times mr-2"></i>
                  Cancelar
                </button>
                <button 
                  @click="confirmRecargo" 
                  :disabled="!isFormValid"
                  class="flex-1 py-3 rounded-lg font-bold transition-all"
                  :class="isFormValid 
                    ? 'bg-gradient-to-r from-orange-400 to-red-400 hover:from-orange-500 hover:to-red-500 text-white shadow-lg' 
                    : 'glass-button text-gray-400 cursor-not-allowed'"
                >
                  <i class="pi pi-check mr-2"></i>
                  Aplicar Recargo
                </button>
              </div>
            </div>
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { ref, computed } from 'vue'

// Define emits
const emit = defineEmits(['close', 'confirm-recargo'])

// Reactive data
const descripcion = ref('')
const monto = ref(0)
const descripcionError = ref(false)
const montoError = ref(false)

// Computed
const isFormValid = computed(() => {
  return descripcion.value.trim().length > 0 && monto.value > 0
})

// Methods
const confirmRecargo = () => {
  // Reset errors
  descripcionError.value = false
  montoError.value = false

  // Validation
  let valid = true
  if (!descripcion.value.trim()) {
    descripcionError.value = true
    valid = false
  }
  if (!monto.value || monto.value <= 0) {
    montoError.value = true
    valid = false
  }

  // If valid, emit the values back to the parent
  if (valid) {
    emit('confirm-recargo', { 
      detalle: descripcion.value, 
      monto: monto.value 
    })
    
    // Reset form
    descripcion.value = ''
    monto.value = 0
  }
}
</script>

<style scoped>
.modal-outer-enter-active,
.modal-outer-leave-active {
  transition: opacity 0.3s cubic-bezier(0.52, 0.02, 0.19, 1.02);
}

.modal-outer-enter-from,
.modal-outer-leave-to {
  opacity: 0;
}

.modal-inner-enter-active {
  transition: all 0.3s cubic-bezier(0.52, 0.02, 0.19, 1.02) 0.10s;
}

.modal-inner-leave-active {
  transition: all 0.3s cubic-bezier(0.52, 0.02, 0.19, 1.02);
}

.modal-inner-enter-from {
  opacity: 0;
  transform: scale(0.8);
}

.modal-inner-leave-to {
  transform: scale(0.8);
}
</style>
