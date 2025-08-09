<template>
  <Teleport to="body">
    <Transition name="modal-outer" appear>
      <div class="fixed inset-0 bg-black/60 backdrop-blur-xl flex justify-center items-center z-[60] p-4">
        <Transition name="modal-inner">
          <div class="relative glass-container max-w-2xl w-full max-h-[90vh] overflow-y-auto">
            <!-- Header -->
            <div class="sticky top-0 glass-card p-6 mb-6 border-b border-white/10">
              <div class="flex items-center justify-between">
                <div class="flex items-center">
                  <div class="bg-gradient-to-r from-green-400 to-blue-400 p-3 rounded-full mr-4">
                    <i class="pi pi-credit-card text-white text-2xl"></i>
                  </div>
                  <div>
                    <h2 class="text-2xl font-bold text-white mb-1">💳 Procesar Pago</h2>
                    <p class="text-gray-300">Gestiona el pago de la habitación</p>
                  </div>
                </div>
                
                <button 
                  @click="emit('close')" 
                  class="glass-button p-3 text-white hover:text-red-300 hover:bg-red-500/20 transition-all rounded-full"
                >
                  <i class="pi pi-times text-xl"></i>
                </button>
              </div>
            </div>

            <!-- Payment Summary -->
            <div class="glass-card p-6 mb-6">
              <div class="flex items-center mb-4">
                <i class="pi pi-calculator text-blue-400 text-xl mr-3"></i>
                <h3 class="text-xl font-bold text-white">📊 Resumen de Cobro</h3>
              </div>
              
              <div class="grid grid-cols-2 gap-4">
                <!-- Left Column - Charges -->
                <div class="space-y-3">
                  <div class="flex justify-between items-center py-2 border-b border-white/10">
                    <span class="text-gray-300 font-medium">Periodo:</span>
                    <span class="text-white font-bold">${{ paymentData.periodo.toFixed(2) }}</span>
                  </div>
                  
                  <div class="flex justify-between items-center py-2 border-b border-white/10">
                    <span class="text-gray-300 font-medium">Consumos:</span>
                    <span class="text-emerald-400 font-bold">${{ paymentData.consumo.toFixed(2) }}</span>
                  </div>
                  
                  <div class="flex justify-between items-center py-2 border-b border-white/10">
                    <span class="text-gray-300 font-medium">Adicional:</span>
                    <span class="text-white font-bold">${{ paymentData.adicional.toFixed(2) }}</span>
                  </div>
                  
                  <div v-if="paymentData.recargoMonto > 0" class="flex justify-between items-center py-2 border-b border-white/10">
                    <span class="text-gray-300 font-medium">Recargo:</span>
                    <span class="text-red-400 font-bold">+${{ paymentData.recargoMonto.toFixed(2) }}</span>
                  </div>
                </div>

                <!-- Right Column - Payments -->
                <div class="space-y-3">
                  <div v-if="paymentData.descuento > 0" class="flex justify-between items-center py-2 border-b border-white/10">
                    <span class="text-gray-300 font-medium">Descuento:</span>
                    <span class="text-green-400 font-bold">-${{ paymentData.descuento.toFixed(2) }}</span>
                  </div>
                  
                  <div v-if="paymentData.empenoMonto > 0" class="flex justify-between items-center py-2 border-b border-white/10">
                    <span class="text-gray-300 font-medium">Empeño:</span>
                    <span class="text-green-400 font-bold">-${{ paymentData.empenoMonto.toFixed(2) }}</span>
                  </div>
                  
                  <div class="flex justify-between items-center py-3 bg-white/5 rounded-lg px-3">
                    <span class="text-white font-bold text-lg">Total:</span>
                    <span class="text-blue-400 font-bold text-xl">${{ calculatedTotal.toFixed(2) }}</span>
                  </div>
                  
                  <div class="flex justify-between items-center py-3 rounded-lg px-3" 
                       :class="faltaPorPagar > 0 ? 'bg-red-500/20' : 'bg-green-500/20'">
                    <span class="font-bold" :class="faltaPorPagar > 0 ? 'text-red-300' : 'text-green-300'">
                      {{ faltaPorPagar > 0 ? 'Falta por pagar:' : 'Pago completo:' }}
                    </span>
                    <span class="font-bold text-xl" :class="faltaPorPagar > 0 ? 'text-red-400' : 'text-green-400'">
                      ${{ Math.abs(faltaPorPagar).toFixed(2) }}
                    </span>
                  </div>
                </div>
              </div>
            </div>

            <!-- Payment Methods -->
            <div class="glass-card p-6 mb-6">
              <div class="flex items-center mb-4">
                <i class="pi pi-wallet text-green-400 text-xl mr-3"></i>
                <h3 class="text-xl font-bold text-white">💰 Métodos de Pago</h3>
              </div>
              
              <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <!-- Cash Payment -->
                <div class="glass-card p-4">
                  <label class="flex items-center mb-3">
                    <i class="pi pi-money-bill text-green-400 mr-2"></i>
                    <span class="text-white font-semibold">Efectivo</span>
                  </label>
                  <input
                    type="number"
                    v-model.number="paymentData.efectivo"
                    class="glass-input w-full px-4 py-3 text-lg font-bold text-center"
                    placeholder="0.00"
                    step="0.01"
                    min="0"
                  />
                </div>

                <!-- Card Payment -->
                <div class="glass-card p-4">
                  <label class="flex items-center mb-3">
                    <i class="pi pi-credit-card text-blue-400 mr-2"></i>
                    <span class="text-white font-semibold">Tarjeta</span>
                    <span v-if="paymentData.selectedTarjeta && paymentData.porcentajeRecargo > 0" 
                          class="ml-2 px-2 py-1 bg-orange-500/20 text-orange-300 rounded-full text-xs">
                      +{{ paymentData.porcentajeRecargo }}%
                    </span>
                  </label>
                  
                  <select
                    v-model="paymentData.selectedTarjeta"
                    class="glass-input w-full px-4 py-3 mb-3"
                  >
                    <option :value="null">Seleccionar tarjeta</option>
                    <option v-for="tarjeta in tarjetas" :key="tarjeta.tarjetaID" :value="tarjeta">
                      {{ tarjeta.nombre }} {{ tarjeta.montoPorcentual > 0 ? `(+${tarjeta.montoPorcentual}%)` : '' }}
                    </option>
                  </select>
                  
                  <input
                    type="number"
                    v-model.number="paymentData.tarjeta"
                    class="glass-input w-full px-4 py-3 text-lg font-bold text-center"
                    placeholder="0.00"
                    step="0.01"
                    min="0"
                    :disabled="paymentData.selectedTarjeta !== null"
                    :class="{ 'opacity-50 cursor-not-allowed': paymentData.selectedTarjeta !== null }"
                  />
                </div>

                <!-- Discount -->
                <div class="glass-card p-4">
                  <label class="flex items-center mb-3">
                    <i class="pi pi-percentage text-yellow-400 mr-2"></i>
                    <span class="text-white font-semibold">Descuento</span>
                  </label>
                  <input
                    type="number"
                    v-model.number="paymentData.descuento"
                    class="glass-input w-full px-4 py-3 text-lg font-bold text-center"
                    placeholder="0.00"
                    step="0.01"
                    min="0"
                  />
                  <p v-if="paymentData.descuento > 0" class="text-yellow-400 text-xs mt-1">
                    ⚠️ Comentario obligatorio
                  </p>
                </div>

                <!-- Comment -->
                <div class="glass-card p-4">
                  <label class="flex items-center mb-3">
                    <i class="pi pi-comment text-purple-400 mr-2"></i>
                    <span class="text-white font-semibold">Comentario</span>
                    <span v-if="paymentData.descuento > 0" class="text-red-400 ml-1">*</span>
                  </label>
                  <textarea
                    v-model="paymentData.comentario"
                    class="glass-input w-full px-4 py-3 h-20 resize-none"
                    :class="{ 'border-red-500/50': paymentData.descuento > 0 && !paymentData.comentario }"
                    placeholder="Escribe un comentario..."
                  ></textarea>
                </div>
              </div>
            </div>

            <!-- Action Buttons -->
            <div class="glass-card p-6 mb-6">
              <div class="flex items-center mb-4">
                <i class="pi pi-cog text-orange-400 text-xl mr-3"></i>
                <h3 class="text-xl font-bold text-white">⚙️ Acciones Adicionales</h3>
              </div>
              
              <div class="flex flex-wrap gap-3 mb-4">
                <!-- Timer Controls -->
                <button 
                  v-if="!paymentData.pausa" 
                  @click="handlePauseTimer"
                  :disabled="isProcessing"
                  class="glass-button px-4 py-2 text-white hover:bg-yellow-500/20 disabled:opacity-50 transition-all"
                >
                  <i class="pi pi-pause mr-2"></i>
                  Pausar Timer
                </button>
                
                <button 
                  v-else
                  @click="handleRecalculateTimer"
                  :disabled="isProcessing"
                  class="glass-button px-4 py-2 text-white hover:bg-green-500/20 disabled:opacity-50 transition-all"
                >
                  <i class="pi pi-play mr-2"></i>
                  Recalcular Timer
                </button>

                <!-- Additional Options -->
                <button 
                  @click="paymentData.showEmpenoModal = true"
                  :disabled="isProcessing"
                  class="glass-button px-4 py-2 text-white hover:bg-blue-500/20 disabled:opacity-50 transition-all"
                >
                  <i class="pi pi-bookmark mr-2"></i>
                  Empeño
                </button>
                
                <button 
                  @click="paymentData.showRecargoModal = true"
                  :disabled="isProcessing"
                  class="glass-button px-4 py-2 text-white hover:bg-orange-500/20 disabled:opacity-50 transition-all"
                >
                  <i class="pi pi-plus-circle mr-2"></i>
                  Recargo
                </button>
              </div>

              <!-- Maintenance Checkbox -->
              <div class="glass-card p-4 mb-4">
                <label class="flex items-center cursor-pointer group">
                  <div class="relative">
                    <input
                      type="checkbox"
                      v-model="paymentData.enviarAMantenimiento"
                      class="sr-only"
                    />
                    <div 
                      class="w-6 h-6 border-2 border-white/30 rounded-lg transition-all duration-300 group-hover:border-yellow-400/50"
                      :class="paymentData.enviarAMantenimiento 
                        ? 'bg-gradient-to-r from-yellow-400 to-orange-500 border-yellow-400' 
                        : 'bg-white/5'"
                    >
                      <i 
                        v-if="paymentData.enviarAMantenimiento"
                        class="pi pi-check text-white text-sm absolute top-0 left-0 w-full h-full flex items-center justify-center"
                      ></i>
                    </div>
                  </div>
                  <div class="ml-3 flex items-center">
                    <i class="pi pi-wrench text-yellow-400 mr-2"></i>
                    <div>
                      <span class="text-white font-medium">Enviar a Mantenimiento</span>
                      <p class="text-gray-300 text-sm">La habitación será marcada para mantenimiento después del checkout</p>
                    </div>
                  </div>
                </label>
              </div>

              <!-- Main Action Button -->
              <button 
                @click="handleConfirmPayment"
                :disabled="!isPaymentValid || isProcessing"
                class="w-full py-4 rounded-xl font-bold text-lg transition-all transform hover:scale-105"
                :class="isPaymentValid && !isProcessing 
                  ? 'bg-gradient-to-r from-green-400 to-green-500 hover:from-green-500 hover:to-green-600 text-white shadow-lg' 
                  : 'glass-button text-gray-400 cursor-not-allowed'"
              >
                <i :class="isProcessing ? 'pi pi-spinner pi-spin' : 'pi pi-check'" class="mr-2"></i>
                {{ isProcessing ? 'Procesando...' : 'Confirmar Pago' }}
              </button>
              
              <!-- Validation Messages -->
              <div v-if="!isPaymentValid" class="mt-3 p-3 bg-red-500/20 border border-red-500/30 rounded-lg">
                <div class="flex items-center text-red-300">
                  <i class="pi pi-exclamation-triangle mr-2"></i>
                  <div class="text-sm">
                    <p v-if="faltaPorPagar !== 0">El pago debe estar completo (falta: ${{ faltaPorPagar.toFixed(2) }})</p>
                    <p v-if="paymentData.descuento > 0 && !paymentData.comentario">El comentario es obligatorio con descuento</p>
                  </div>
                </div>
              </div>
            </div>

            <!-- Loading Overlay -->
            <div v-if="isProcessing" class="absolute inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center rounded-3xl">
              <div class="glass-card p-6 text-center">
                <div class="bg-gradient-to-r from-blue-400 to-purple-400 p-4 rounded-full mx-auto mb-4 w-16 h-16 flex items-center justify-center">
                  <i class="pi pi-spinner pi-spin text-white text-2xl"></i>
                </div>
                <p class="text-white font-semibold">Procesando pago...</p>
                <p class="text-gray-300 text-sm">Por favor espera</p>
              </div>
            </div>
          </div>
        </Transition>
      </div>
    </Transition>

    <!-- Child Modals -->
    <RecargoModal 
      v-if="paymentData.showRecargoModal" 
      @close="paymentData.showRecargoModal = false" 
      @confirm-recargo="confirmarRecargo" 
    />
    
    <EmpenoModal 
      v-if="paymentData.showEmpenoModal" 
      @close="paymentData.showEmpenoModal = false" 
      @confirm-empeno="confirmarEmpeno" 
    />

    <!-- Toast for notifications -->
    <Toast />
  </Teleport>
</template>

<script setup>
import { computed } from 'vue'
import { useToast } from 'primevue/usetoast'
import { usePaymentConsumer } from '../composables/usePaymentProvider'
import EmpenoModal from './EmpenoModal.vue'
import RecargoModal from './RecargoModal.vue'
import Toast from 'primevue/toast'

// Define emits
const emit = defineEmits(['close', 'confirm-payment'])

// Use payment provider (no props needed!)
const {
  paymentData,
  tarjetas,
  loading,
  calculatedTotal,
  faltaPorPagar,
  isPaymentValid,
  confirmarEmpeno,
  confirmarRecargo,
  crearMovimientoAdicional,
  pagarVisita,
  finalizarReserva,
  pausarTimer,
  recalcularTimer
} = usePaymentConsumer()

const toast = useToast()

// Computed
const isProcessing = computed(() => paymentData.value.isProcessing)

// Methods
const handlePauseTimer = async () => {
  const success = await pausarTimer()
  if (success) {
    emit('confirm-payment', { paused: true })
  }
}

const handleRecalculateTimer = async () => {
  const success = await recalcularTimer()
  if (success) {
    emit('confirm-payment', { recalculated: true })
  }
}

const handleConfirmPayment = async () => {
  // Step 1: Create additional movement
  const movementSuccess = await crearMovimientoAdicional()
  if (!movementSuccess) return

  // Step 2: Process payment
  const paymentSuccess = await pagarVisita()
  if (!paymentSuccess) return

  // Step 3: Finalize reservation
  const finalizeSuccess = await finalizarReserva()
  if (finalizeSuccess) {
    emit('confirm-payment', { 
      habitacionId: paymentData.value.habitacionId,
      completed: true 
    })
  }
}
</script>

<style>
/* Add your styles here */
</style>
