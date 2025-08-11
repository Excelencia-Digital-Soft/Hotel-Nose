<template>
  <div class="fixed inset-0 bg-black/70 backdrop-blur-sm flex items-center justify-center z-50 p-4">
    <div class="glass-modal max-w-2xl w-full max-h-[90vh] overflow-y-auto">
      <!-- Header -->
      <div class="flex items-center justify-between p-6 border-b border-white/10">
        <div>
          <h2 class="text-2xl font-bold text-white lexend-exa">Procesar Pago</h2>
          <p class="text-white/70 text-sm mt-1">
            Empeño #{{ pawn?.empeñoId }} - Visita #{{ pawn?.visitaId }}
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
              <label class="block text-white font-medium text-sm"> Descuento </label>
              <div class="relative">
                <span class="absolute left-3 top-1/2 transform -translate-y-1/2 text-white/70"
                  >$</span
                >
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
              <label class="block text-white font-medium text-sm"> Recargo </label>
              <div class="relative">
                <span class="absolute left-3 top-1/2 transform -translate-y-1/2 text-white/70"
                  >$</span
                >
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
              <span class="text-white font-bold text-xl">{{
                formatCurrency(totalWithAdjustments)
              }}</span>
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
              class="glass-button py-2 px-3 rounded-lg text-sm transition-all duration-300 hover:scale-105 active:scale-95"
            >
              <i class="pi pi-money-bill mr-1 text-green-400"></i>
              Todo Efectivo
            </button>
            <button
              @click="autoFillCard"
              :disabled="isLoadingCards || !paymentCards.length"
              class="glass-button py-2 px-3 rounded-lg text-sm transition-all duration-300 hover:scale-105 active:scale-95 disabled:opacity-50 disabled:cursor-not-allowed"
              :title="
                isLoadingCards
                  ? 'Cargando tarjetas...'
                  : !paymentCards.length
                    ? 'No hay tarjetas disponibles'
                    : 'Pagar todo con tarjeta'
              "
            >
              <i v-if="isLoadingCards" class="pi pi-spin pi-spinner mr-1 text-blue-400"></i>
              <i v-else class="pi pi-credit-card mr-1 text-blue-400"></i>
              Todo Tarjeta
            </button>
            <button
              @click="splitPayment"
              :disabled="isLoadingCards || !paymentCards.length"
              class="glass-button py-2 px-3 rounded-lg text-sm transition-all duration-300 hover:scale-105 active:scale-95 disabled:opacity-50 disabled:cursor-not-allowed"
              :title="
                isLoadingCards
                  ? 'Cargando tarjetas...'
                  : !paymentCards.length
                    ? 'No hay tarjetas disponibles'
                    : 'Dividir 50% efectivo / 50% tarjeta'
              "
            >
              <i v-if="isLoadingCards" class="pi pi-spin pi-spinner mr-1 text-purple-400"></i>
              <i v-else class="pi pi-percentage mr-1 text-purple-400"></i>
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
                <span class="absolute left-3 top-1/2 transform -translate-y-1/2 text-white/70"
                  >$</span
                >
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
                <span class="absolute left-3 top-1/2 transform -translate-y-1/2 text-white/70"
                  >$</span
                >
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
          <div v-if="paymentForm.card > 0" class="mt-4 space-y-3">
            <label class="text-white font-medium text-sm flex items-center">
              <i class="pi pi-credit-card mr-2 text-blue-400"></i>
              Seleccionar Tarjeta
              <span v-if="isLoadingCards" class="ml-2 text-xs text-white/50">
                <i class="pi pi-spin pi-spinner mr-1"></i>
                Cargando...
              </span>
            </label>

            <div class="relative">
              <select
                v-model="paymentForm.selectedCard"
                :disabled="isLoadingCards || !paymentCards.length"
                class="glass-input w-full pr-10 disabled:opacity-50 disabled:cursor-not-allowed"
                @change="handleCardSelection"
              >
                <option :value="null">
                  {{ isLoadingCards ? 'Cargando tarjetas...' : 'Selecciona una tarjeta' }}
                </option>
                <option v-for="card in paymentCards" :key="card.tarjetaId" :value="card">
                  💳 {{ card.nombre }}
                </option>
              </select>
              <div class="absolute inset-y-0 right-0 flex items-center pr-3 pointer-events-none">
                <i v-if="isLoadingCards" class="pi pi-spin pi-spinner text-blue-400 text-sm"></i>
                <i v-else class="pi pi-chevron-down text-white/70 text-sm"></i>
              </div>
            </div>

            <!-- Payment Cards Status -->
            <div
              v-if="!isLoadingCards && paymentCards.length === 0"
              class="p-3 bg-red-500/10 border border-red-400/30 rounded-lg"
            >
              <div class="flex items-center text-sm">
                <i class="pi pi-exclamation-triangle mr-2 text-red-400"></i>
                <span class="text-red-300">No hay tarjetas de pago disponibles</span>
              </div>
            </div>

            <!-- Card Info (No automatic surcharge) -->
            <div
              v-if="paymentForm.selectedCard"
              class="p-3 bg-blue-500/10 border border-blue-400/30 rounded-lg"
            >
              <div class="flex items-center text-sm">
                <i class="pi pi-credit-card mr-2 text-blue-400"></i>
                <span class="text-blue-300"
                  >Tarjeta seleccionada: {{ paymentForm.selectedCard.nombre }}</span
                >
              </div>
              <div class="mt-1 text-xs text-blue-300/70">
                Si aplica recargo, ingrésalo manualmente en el campo "Recargo"
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

          <div class="space-y-4">
            <!-- Payment Methods Breakdown -->
            <div class="space-y-3">
              <div class="flex justify-between items-center text-sm">
                <div class="flex items-center">
                  <i class="pi pi-money-bill mr-2 text-green-400"></i>
                  <span class="text-white/70">Efectivo:</span>
                </div>
                <span class="text-white font-medium">{{
                  formatCurrency(paymentForm.cash || 0)
                }}</span>
              </div>

              <div v-if="paymentForm.card > 0" class="flex justify-between items-center text-sm">
                <div class="flex items-center">
                  <i class="pi pi-credit-card mr-2 text-blue-400"></i>
                  <span class="text-white/70">Tarjeta:</span>
                </div>
                <span class="text-white font-medium">{{
                  formatCurrency(paymentForm.card || 0)
                }}</span>
              </div>
            </div>

            <hr class="border-white/20" />

            <!-- Totals Section -->
            <div class="space-y-3">
              <div class="flex justify-between items-center">
                <span class="text-white font-medium">Total Requerido:</span>
                <span class="text-white font-bold text-xl">{{
                  formatCurrency(totalWithAdjustments)
                }}</span>
              </div>

              <div class="flex justify-between items-center">
                <span class="text-white font-medium">Total Pagado:</span>
                <span class="text-white font-bold text-lg">{{ formatCurrency(totalPayment) }}</span>
              </div>

              <hr class="border-white/10" />

              <!-- Balance Status with enhanced visual feedback -->
              <div
                class="flex justify-between items-center p-3 rounded-lg"
                :class="{
                  'bg-green-500/10 border border-green-400/30': remainingBalance === 0,
                  'bg-red-500/10 border border-red-400/30': remainingBalance !== 0,
                }"
              >
                <div class="flex items-center">
                  <i
                    :class="{
                      'pi pi-check-circle text-green-400': remainingBalance === 0,
                      'pi pi-exclamation-circle text-red-400': remainingBalance !== 0,
                    }"
                    class="mr-2"
                  ></i>
                  <span
                    :class="{
                      'text-green-400': remainingBalance === 0,
                      'text-red-400': remainingBalance !== 0,
                    }"
                    class="font-medium"
                  >
                    {{
                      remainingBalance === 0
                        ? '✅ Pagado Completo'
                        : remainingBalance > 0
                          ? '⚠️ Falta por Pagar'
                          : '💰 Exceso de Pago'
                    }}
                    <span class="text-xs opacity-75 ml-2"> (Valid: {{ isPaymentValid }}) </span>
                  </span>
                </div>
                <span
                  :class="{
                    'text-green-400': remainingBalance === 0,
                    'text-red-400': remainingBalance !== 0,
                  }"
                  class="font-bold text-lg"
                >
                  {{ remainingBalance === 0 ? '✓' : formatCurrency(Math.abs(remainingBalance)) }}
                </span>
              </div>
            </div>
          </div>
        </div>

        <!-- Debug Info (Remove in production) -->
        <div class="glass-section-card bg-gray-800/30">
          <h3 class="text-sm text-gray-400 mb-2">🔍 Debug Info:</h3>
          <div class="text-xs text-gray-300 space-y-1">
            <div>Total Required: {{ formatCurrency(totalWithAdjustments) }}</div>
            <div>Total Payment: {{ formatCurrency(totalPayment) }}</div>
            <div>Remaining: {{ formatCurrency(remainingBalance) }} ({{ remainingBalance }})</div>
            <div>Is Valid: {{ isPaymentValid }} (abs: {{ Math.abs(remainingBalance) }})</div>
            <div>Cash: {{ paymentForm.cash }}, Card: {{ paymentForm.card }}</div>
            <div>Discount: {{ paymentForm.discount }}, Surcharge: {{ paymentForm.surcharge }}</div>
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
            :title="
              !isPaymentValid
                ? `Balance: ${formatCurrency(Math.abs(remainingBalance))} - ${remainingBalance > 0 ? 'Falta' : 'Exceso'}`
                : 'Confirmar pago'
            "
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

<script setup lang="ts">
  import { ref, computed, watch, onMounted } from 'vue'
  import { usePawnManager } from '../composables/usePawnManager'
  import { PawnService } from '../services/pawnService'
  import { useAuthStore } from '../store/auth.js'
  import type { PawnDto, PaymentCardDto, PayPawnRequest } from '../types'

  // Props and emits
  interface Props {
    pawn: PawnDto
    paymentCards: PaymentCardDto[]
  }

  const props = withDefaults(defineProps<Props>(), {
    paymentCards: () => [],
  })

  const emit = defineEmits<{
    close: []
    'payment-completed': []
  }>()

  // Auth store
  const authStore = useAuthStore()

  // Initialize composable
  const {
    paymentForm,
    isSubmittingPayment,
    isLoadingCards,
    totalWithAdjustments,
    cardSurcharge,
    totalPayment,
    remainingBalance,
    isPaymentValid,
    autoFillCash,
    autoFillCard,
    splitPayment,
    fetchPaymentCards,
    formatCurrency,
    formatDate,
    getStatusColor,
    showSuccess,
    showError,
    confirmPayment: confirmPaymentDialog,
    selectedPawn,
    paymentCards,
  } = usePawnManager()

  // Set initial values from props (ensure reactive updates)
  onMounted(() => {
    selectedPawn.value = props.pawn
    paymentCards.value = props.paymentCards
  })

  // Computed
  const getDaysStatus = (): 'overdue' | 'warning' | 'active' => {
    const days = calculateDays()
    if (days > 30) return 'overdue'
    if (days > 15) return 'warning'
    return 'active'
  }

  // Methods
  const calculateDays = (): number => {
    if (!props.pawn?.fechaRegistro) return 0
    return PawnService.calculateDaysFromDate(props.pawn.fechaRegistro)
  }

  const handleCardAmountChange = (): void => {
    // Auto-select first card if amount > 0 and no card selected
    if (
      paymentForm.value.card > 0 &&
      !paymentForm.value.selectedCard &&
      paymentCards.value.length > 0
    ) {
      paymentForm.value.selectedCard = paymentCards.value[0]
    }
  }

  const handleCardSelection = (): void => {
    // Recalculate surcharge when card changes
    if (paymentForm.value.selectedCard && paymentForm.value.card > 0) {
      // The composable will handle the calculation
    }
  }

  const confirmPayment = async (): Promise<void> => {
    console.log('🔄 Confirming payment...')
    console.log('💰 Payment validation:', {
      isValid: isPaymentValid.value,
      remainingBalance: remainingBalance.value,
      totalRequired: totalWithAdjustments.value,
      cardSurcharge: cardSurcharge.value,
      totalPayment: totalPayment.value,
      cash: paymentForm.value.cash,
      card: paymentForm.value.card,
    })

    if (!isPaymentValid.value) {
      showError(
        `El pago no está balanceado. Falta: ${formatCurrency(Math.abs(remainingBalance.value))}`
      )
      return
    }

    console.log('✅ Payment is valid, asking for confirmation...')
    const confirmed = await confirmPaymentDialog()
    if (!confirmed) {
      console.log('❌ Payment cancelled by user')
      return
    }

    try {
      isSubmittingPayment.value = true

      // Update pawn amount if there are adjustments
      if (paymentForm.value.discount !== 0 || paymentForm.value.surcharge !== 0) {
        const updateResponse = await PawnService.updatePawnAmount(
          props.pawn.empenoId,
          totalWithAdjustments.value
        )
        if (!updateResponse.isSuccess) {
          showError(updateResponse.message || 'Error al actualizar el monto del empeño')
          return
        }
      }

      // Process payment
      const paymentData: PayPawnRequest = {
        pawnId: props.pawn.empenoId,
        montoEfectivo: paymentForm.value.cash || 0,
        montoTarjeta: paymentForm.value.card || 0, // No automatic surcharge - user handles it manually
        observacion: paymentForm.value.observation,
        tarjetaId: paymentForm.value.selectedCard?.tarjetaId,
      }

      console.log('📤 Sending payment data:', paymentData)
      const response = await PawnService.payPawn(paymentData)
      console.log('📥 Payment response:', response)

      if (response.isSuccess) {
        console.log('✅ Payment successful!')
        showSuccess('Pago procesado exitosamente')
        emit('payment-completed')
      } else {
        console.log('❌ Payment failed:', response.message)
        showError(response.message || 'Error al procesar el pago')
      }
    } catch (error) {
      console.error('❌ Error processing payment:', error)
      showError('Error al procesar el pago')
    } finally {
      isSubmittingPayment.value = false
    }
  }

  // Initialize form data and ensure proper reactive setup
  onMounted(async () => {
    if (props.pawn) {
      // These should already be set above, but ensure they're reactive
      selectedPawn.value = props.pawn
      paymentCards.value = props.paymentCards
      // Set default observation
      paymentForm.value.observation = `Pago de empeño correspondiente a la visita ${props.pawn.visitaId}`

      // Load payment cards if not provided via props or if empty
      if (!paymentCards.value || paymentCards.value.length === 0) {
        console.log('🔄 Loading payment cards in modal...')
        await fetchPaymentCards()
      }
    }
  })

  // Watch for pawn changes to update totals
  watch(
    () => props.pawn,
    (newPawn) => {
      if (newPawn) {
        // Reset form when pawn changes
        console.log('newPawn', newPawn)
        paymentForm.value.cash = 0
        paymentForm.value.card = 0
        paymentForm.value.discount = 0
        paymentForm.value.surcharge = 0
        paymentForm.value.selectedCard = null
        paymentForm.value.observation = `Pago de empeño correspondiente a la visita ${newPawn.visitaId}`
        selectedPawn.value = newPawn
        paymentCards.value = props.paymentCards
      }
    },
    { immediate: true }
  )
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
