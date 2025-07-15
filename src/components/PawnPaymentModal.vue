<template>
  <div class="fixed inset-0 bg-black/70 backdrop-blur-sm flex items-center justify-center z-50 p-4">
    <div class="glass-modal max-w-2xl w-full max-h-[90vh] overflow-y-auto">
      <!-- Header -->
      <div class="flex items-center justify-between p-6 border-b border-white/10">
        <div>
          <h2 class="text-2xl font-bold text-white lexend-exa">
            Procesar Pago
          </h2>
          <p class="text-white/70 text-sm mt-1">
            Empeño #{{ pawn?.empeñoID }} - Visita #{{ pawn?.visitaID }}
          </p>
        </div>
        <button 
          @click="$emit('close')"
          class="glass-button w-10 h-10 rounded-xl flex items-center justify-center text-white/70 hover:text-white transition-colors"
        >
          <i class="pi pi-times"></i>
        </button>
      </div>

      <!-- Content -->
      <div class="p-6 space-y-6">
        <!-- Pawn Details -->
        <div class="glass-section-card">
          <h3 class="text-lg font-semibold text-white mb-4 flex items-center">
            <i class="pi pi-info-circle mr-2 text-blue-400"></i>
            Detalles del Empeño
          </h3>
          <div class="grid grid-cols-2 gap-4">
            <div>
              <span class="text-white/70 text-sm">Descripción:</span>
              <p class="text-white font-medium">{{ pawn?.detalle || 'Sin descripción' }}</p>
            </div>
            <div>
              <span class="text-white/70 text-sm">Monto Original:</span>
              <p class="text-white font-bold text-lg">{{ formatCurrency(pawn?.monto || 0) }}</p>
            </div>
            <div>
              <span class="text-white/70 text-sm">Fecha de Registro:</span>
              <p class="text-white font-medium">{{ formatDate(pawn?.fechaRegistro) }}</p>
            </div>
            <div>
              <span class="text-white/70 text-sm">Días Transcurridos:</span>
              <p :class="['font-medium', getStatusColor(getDaysStatus())]">
                {{ calculateDays() }} días
              </p>
            </div>
          </div>
        </div>

        <!-- Payment Adjustments -->
        <div class="glass-section-card">
          <h3 class="text-lg font-semibold text-white mb-4 flex items-center">
            <i class="pi pi-cog mr-2 text-purple-400"></i>
            Ajustes al Monto
          </h3>
          <div class="grid grid-cols-2 gap-4">
            <!-- Discount -->
            <div class="space-y-2">
              <label class="block text-white font-medium text-sm">
                Descuento
              </label>
              <div class="relative">
                <span class="absolute left-3 top-1/2 transform -translate-y-1/2 text-white/70">$</span>
                <input 
                  v-model.number="paymentForm.discount"
                  type="number" 
                  min="0"
                  :max="pawn?.monto || 0"
                  placeholder="0.00"
                  class="glass-input w-full pl-8"
                />
              </div>
            </div>
            
            <!-- Surcharge -->
            <div class="space-y-2">
              <label class="block text-white font-medium text-sm">
                Recargo
              </label>
              <div class="relative">
                <span class="absolute left-3 top-1/2 transform -translate-y-1/2 text-white/70">$</span>
                <input 
                  v-model.number="paymentForm.surcharge"
                  type="number" 
                  min="0"
                  placeholder="0.00"
                  class="glass-input w-full pl-8"
                />
              </div>
            </div>
          </div>
          
          <!-- Adjusted Total -->
          <div class="mt-4 p-4 bg-primary-500/10 border border-primary-400/30 rounded-xl">
            <div class="flex justify-between items-center">
              <span class="text-white font-medium">Total con Ajustes:</span>
              <span class="text-white font-bold text-xl">{{ formatCurrency(totalWithAdjustments) }}</span>
            </div>
          </div>
        </div>

        <!-- Payment Methods -->
        <div class="glass-section-card">
          <h3 class="text-lg font-semibold text-white mb-4 flex items-center">
            <i class="pi pi-credit-card mr-2 text-green-400"></i>
            Métodos de Pago
          </h3>
          
          <!-- Quick Fill Buttons -->
          <div class="grid grid-cols-3 gap-2 mb-4">
            <button 
              @click="autoFillCash"
              class="glass-button py-2 px-3 rounded-lg text-sm transition-all duration-300 hover:scale-105"
            >
              <i class="pi pi-money-bill mr-1"></i>
              Todo Efectivo
            </button>
            <button 
              @click="autoFillCard"
              :disabled="!paymentCards.length"
              class="glass-button py-2 px-3 rounded-lg text-sm transition-all duration-300 hover:scale-105 disabled:opacity-50"
            >
              <i class="pi pi-credit-card mr-1"></i>
              Todo Tarjeta
            </button>
            <button 
              @click="splitPayment"
              :disabled="!paymentCards.length"
              class="glass-button py-2 px-3 rounded-lg text-sm transition-all duration-300 hover:scale-105 disabled:opacity-50"
            >
              <i class="pi pi-percentage mr-1"></i>
              Dividir 50/50
            </button>
          </div>
          
          <div class="grid grid-cols-2 gap-6">
            <!-- Cash Payment -->
            <div class="space-y-3">
              <label class="block text-white font-medium text-sm">
                <i class="pi pi-money-bill mr-2 text-green-400"></i>
                Efectivo
              </label>
              <div class="relative">
                <span class="absolute left-3 top-1/2 transform -translate-y-1/2 text-white/70">$</span>
                <input 
                  v-model.number="paymentForm.cash"
                  type="number" 
                  min="0"
                  :max="totalWithAdjustments"
                  placeholder="0.00"
                  class="glass-input w-full pl-8"
                />
              </div>
            </div>
            
            <!-- Card Payment -->
            <div class="space-y-3">
              <label class="block text-white font-medium text-sm">
                <i class="pi pi-credit-card mr-2 text-blue-400"></i>
                Tarjeta
              </label>
              <div class="relative">
                <span class="absolute left-3 top-1/2 transform -translate-y-1/2 text-white/70">$</span>
                <input 
                  v-model.number="paymentForm.card"
                  type="number" 
                  min="0"
                  placeholder="0.00"
                  class="glass-input w-full pl-8"
                  @input="handleCardAmountChange"
                />
              </div>
            </div>
          </div>
          
          <!-- Card Selection -->
          <div v-if="paymentForm.card > 0" class="mt-4 space-y-2">
            <label class="block text-white font-medium text-sm">
              Seleccionar Tarjeta
            </label>
            <select 
              v-model="paymentForm.selectedCard" 
              class="glass-input w-full"
              @change="handleCardSelection"
            >
              <option :value="null">Selecciona una tarjeta</option>
              <option 
                v-for="card in paymentCards" 
                :key="card.tarjetaID" 
                :value="card"
              >
                {{ card.nombre }} ({{ card.montoPorcentual }}% recargo)
              </option>
            </select>
            
            <!-- Card Surcharge Info -->
            <div v-if="paymentForm.selectedCard && cardSurcharge > 0" class="p-3 bg-yellow-500/10 border border-yellow-400/30 rounded-lg">
              <div class="flex justify-between items-center text-sm">
                <span class="text-yellow-300">Recargo por Tarjeta ({{ paymentForm.selectedCard.montoPorcentual }}%):</span>
                <span class="text-yellow-300 font-bold">{{ formatCurrency(cardSurcharge) }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Payment Summary -->
        <div class="glass-section-card">
          <h3 class="text-lg font-semibold text-white mb-4 flex items-center">
            <i class="pi pi-calculator mr-2 text-orange-400"></i>
            Resumen del Pago
          </h3>
          
          <div class="space-y-3">
            <div class="flex justify-between items-center text-sm">
              <span class="text-white/70">Efectivo:</span>
              <span class="text-white font-medium">{{ formatCurrency(paymentForm.cash || 0) }}</span>
            </div>
            <div class="flex justify-between items-center text-sm">
              <span class="text-white/70">Tarjeta:</span>
              <span class="text-white font-medium">{{ formatCurrency(paymentForm.card || 0) }}</span>
            </div>
            <div v-if="cardSurcharge > 0" class="flex justify-between items-center text-sm">
              <span class="text-yellow-300">Recargo Tarjeta:</span>
              <span class="text-yellow-300 font-medium">{{ formatCurrency(cardSurcharge) }}</span>
            </div>
            <hr class="border-white/20">
            <div class="flex justify-between items-center">
              <span class="text-white font-medium">Total a Pagar:</span>
              <span class="text-white font-bold text-lg">{{ formatCurrency(totalPayment + cardSurcharge) }}</span>
            </div>
            <div class="flex justify-between items-center">
              <span class="text-white font-medium">Total Requerido:</span>
              <span class="text-white font-bold text-lg">{{ formatCurrency(totalWithAdjustments) }}</span>
            </div>
            <div class="flex justify-between items-center">
              <span :class="['font-medium', remainingBalance === 0 ? 'text-green-400' : 'text-red-400']">
                {{ remainingBalance === 0 ? 'Pagado Completo' : remainingBalance > 0 ? 'Falta por Pagar' : 'Exceso' }}:
              </span>
              <span :class="['font-bold text-lg', remainingBalance === 0 ? 'text-green-400' : 'text-red-400']">
                {{ formatCurrency(Math.abs(remainingBalance)) }}
              </span>
            </div>
          </div>
        </div>

        <!-- Observation -->
        <div class="glass-section-card">
          <h3 class="text-lg font-semibold text-white mb-4 flex items-center">
            <i class="pi pi-comment mr-2 text-cyan-400"></i>
            Observaciones
          </h3>
          <textarea 
            v-model="paymentForm.observation"
            placeholder="Observaciones adicionales sobre el pago..."
            rows="3"
            class="glass-input w-full resize-none"
          ></textarea>
        </div>
      </div>

      <!-- Footer -->
      <div class="flex items-center justify-between p-6 border-t border-white/10">
        <div class="text-sm text-white/70">
          <i class="pi pi-info-circle mr-1"></i>
          Verifica los datos antes de confirmar
        </div>
        <div class="flex gap-3">
          <button 
            @click="$emit('close')"
            class="glass-button py-3 px-6 rounded-xl font-medium transition-all duration-300 hover:scale-105"
          >
            Cancelar
          </button>
          <button 
            @click="confirmPayment"
            :disabled="!isPaymentValid || isSubmittingPayment"
            class="glass-button-primary py-3 px-6 rounded-xl font-medium transition-all duration-300 hover:scale-105 disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100"
          >
            <i v-if="isSubmittingPayment" class="pi pi-spin pi-spinner mr-2"></i>
            <i v-else class="pi pi-check mr-2"></i>
            Confirmar Pago
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { usePawnManager } from '../composables/usePawnManager.js'
import { PawnService } from '../services/pawnService.js'
import { useAuthStore } from '../store/auth.js'

// Feature flag for V1 API
const USE_V1_API = false

// Props and emits
const props = defineProps({
  pawn: { type: Object, required: true },
  paymentCards: { type: Array, default: () => [] }
})

const emit = defineEmits(['close', 'payment-completed'])

// Auth store
const authStore = useAuthStore()

// Composable
const {
  paymentForm,
  isSubmittingPayment,
  totalWithAdjustments,
  cardSurcharge,
  totalPayment,
  remainingBalance,
  isPaymentValid,
  autoFillCash,
  autoFillCard,
  splitPayment,
  formatCurrency,
  formatDate,
  getStatusColor,
  showSuccess,
  showError,
  confirmPayment: confirmPaymentDialog
} = usePawnManager()

// Local reactive data
const paymentCards = ref(props.paymentCards)

// Computed
const getDaysStatus = () => {
  const days = calculateDays()
  if (days > 30) return 'overdue'
  if (days > 15) return 'warning'
  return 'active'
}

// Methods
const calculateDays = () => {
  if (!props.pawn?.fechaRegistro) return 0
  return PawnService.calculateDaysFromDate(props.pawn.fechaRegistro)
}

const handleCardAmountChange = () => {
  // Auto-select first card if amount > 0 and no card selected
  if (paymentForm.value.card > 0 && !paymentForm.value.selectedCard && paymentCards.value.length > 0) {
    paymentForm.value.selectedCard = paymentCards.value[0]
  }
}

const handleCardSelection = () => {
  // Recalculate surcharge when card changes
  if (paymentForm.value.selectedCard && paymentForm.value.card > 0) {
    // The composable will handle the calculation
  }
}

const confirmPayment = async () => {
  if (!isPaymentValid.value) {
    showError('El pago no está balanceado. Verifica los montos.')
    return
  }

  const confirmed = await confirmPaymentDialog()
  if (!confirmed) return

  try {
    isSubmittingPayment.value = true

    // Update pawn amount if there are adjustments
    if (paymentForm.value.discount !== 0 || paymentForm.value.surcharge !== 0) {
      if (USE_V1_API) {
        await PawnService.updatePawnAmount(props.pawn.empeñoID, totalWithAdjustments.value)
      } else {
        await PawnService.legacyUpdatePawnAmount(props.pawn.empeñoID, totalWithAdjustments.value)
      }
    }

    // Process payment
    const paymentData = {
      pawnId: props.pawn.empeñoID,
      montoEfectivo: paymentForm.value.cash || 0,
      montoTarjeta: paymentForm.value.card || 0,
      observacion: paymentForm.value.observation,
      tarjetaId: paymentForm.value.selectedCard?.tarjetaID || 0
    }

    let response
    if (USE_V1_API) {
      response = await PawnService.payPawn(paymentData)
    } else {
      response = await PawnService.legacyPayPawn(paymentData)
    }

    if (response) {
      emit('payment-completed')
    } else {
      showError('Error al procesar el pago')
    }
  } catch (error) {
    console.error('Error processing payment:', error)
    showError('Error al procesar el pago')
  } finally {
    isSubmittingPayment.value = false
  }
}

// Initialize form data
onMounted(() => {
  if (props.pawn) {
    paymentForm.value.observation = `Pago de empeño correspondiente a la visita ${props.pawn.visitaID}`
  }
})

// Watch for pawn changes to update totals
watch(() => props.pawn, (newPawn) => {
  if (newPawn) {
    // Reset form when pawn changes
    paymentForm.value.cash = 0
    paymentForm.value.card = 0
    paymentForm.value.discount = 0
    paymentForm.value.surcharge = 0
    paymentForm.value.selectedCard = null
    paymentForm.value.observation = `Pago de empeño correspondiente a la visita ${newPawn.visitaID}`
  }
}, { immediate: true })
</script>

<style scoped>
.glass-modal {
  @apply bg-neutral-900/95 backdrop-blur-xl rounded-2xl border border-white/20 shadow-2xl;
  background: linear-gradient(135deg, rgba(255, 255, 255, 0.1), rgba(255, 255, 255, 0.05));
  backdrop-filter: blur(20px);
}

.glass-section-card {
  @apply bg-white/5 backdrop-blur-sm border border-white/20 rounded-xl p-4;
  background: rgba(255, 255, 255, 0.05);
  backdrop-filter: blur(10px);
}
</style>