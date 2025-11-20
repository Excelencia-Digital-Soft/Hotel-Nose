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

            <!-- Surcharge Manual (solo para recargos adicionales que NO sean de tarjeta) -->
            <div class="space-y-2">
              <label class="block text-white font-medium text-sm"> 
                Recargo Adicional
                <span class="text-xs text-white/50 ml-1">(ej: daños, extras)</span>
              </label>
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
                  title="Recargos por daños u otros conceptos. El interés de la tarjeta se calculará sobre el total incluyendo este recargo."
                />
              </div>
            </div>
          </div>

          <!-- Adjusted Total -->
          <div class="mt-4 p-4 bg-primary-500/10 border border-primary-400/30 rounded-xl">
            <div class="flex justify-between items-center">
              <span class="text-white font-medium">Monto Base Ajustado:</span>
              <span class="text-white font-bold text-xl">{{
                formatCurrency(totalWithAdjustments)
              }}</span>
            </div>
            <div v-if="paymentForm.selectedCard" class="text-xs text-white/50 mt-1">
              (El interés de la tarjeta se agrega automáticamente al monto de tarjeta)
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
                <span class="text-xs text-white/50 ml-1">(automático)</span>
              </label>
              <div class="relative">
                <span class="absolute left-3 top-1/2 transform -translate-y-1/2 text-white/70"
                  >$</span
                >
                <input
                  v-model.number="paymentForm.card"
                  type="number"
                  min="0"
                  placeholder="Selecciona una tarjeta"
                  class="glass-input w-full pl-8 bg-white/5"
                  readonly
                  title="El monto se calcula automáticamente: Monto base + Interés de la tarjeta"
                />
              </div>
            </div>
          </div>

          <!-- Card Selection -->
          <div v-if="paymentForm.card > 0 || paymentCards.length > 0" class="mt-4 space-y-3">
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

            <!-- Card Info -->
            <div
              v-if="paymentForm.selectedCard"
              class="p-3 bg-blue-500/10 border border-blue-400/30 rounded-lg"
            >
              <div class="flex items-center justify-between text-sm mb-1">
                <div class="flex items-center">
                  <i class="pi pi-credit-card mr-2 text-blue-400"></i>
                  <span class="text-blue-300">{{ paymentForm.selectedCard.nombre }}</span>
                </div>
                <span class="text-blue-300 font-bold">
                  {{ formatCurrency(paymentForm.card) }}
                </span>
              </div>
              <div class="text-xs text-blue-300/70">
                Incluye monto base + interés de {{ paymentForm.selectedCard.montoPorcentual }}%
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
            <!-- Base amounts breakdown -->
            <div class="space-y-2 pb-3 border-b border-white/10">
              <div class="flex justify-between items-center text-sm">
                <span class="text-white/70">Monto Original del Empeño:</span>
                <span class="text-white font-medium">{{ formatCurrency(pawn?.monto || 0) }}</span>
              </div>
              
              <div v-if="paymentForm.discount > 0" class="flex justify-between items-center text-sm">
                <span class="text-green-400">(-) Descuento:</span>
                <span class="text-green-400 font-medium">{{ formatCurrency(paymentForm.discount) }}</span>
              </div>
              
              <div v-if="paymentForm.surcharge > 0" class="flex justify-between items-center text-sm">
                <span class="text-yellow-400">(+) Recargo Adicional:</span>
                <span class="text-yellow-400 font-medium">{{ formatCurrency(paymentForm.surcharge) }}</span>
              </div>

              <!-- Subtotal antes del interés -->
              <div v-if="paymentForm.discount > 0 || paymentForm.surcharge > 0" class="flex justify-between items-center text-sm pt-2 border-t border-white/10">
                <span class="text-white/90 font-medium">Subtotal:</span>
                <span class="text-white font-medium">{{ formatCurrency(subtotalWithoutInterest) }}</span>
              </div>

              <div v-if="paymentForm.selectedCard" class="flex justify-between items-center text-sm">
                <span class="text-blue-400">(+) Interés por Tarjeta ({{ paymentForm.selectedCard.montoPorcentual }}%):</span>
                <span class="text-blue-400 font-medium">{{ formatCurrency(currentCardInterest) }}</span>
              </div>
            </div>

            <!-- Payment Methods Breakdown -->
            <div class="space-y-3">
              <div v-if="paymentForm.cash > 0" class="flex justify-between items-center text-sm">
                <div class="flex items-center">
                  <i class="pi pi-money-bill mr-2 text-green-400"></i>
                  <span class="text-white/70">Pago en Efectivo:</span>
                </div>
                <span class="text-white font-medium">{{
                  formatCurrency(paymentForm.cash || 0)
                }}</span>
              </div>

              <div v-if="paymentForm.card > 0" class="flex justify-between items-center text-sm">
                <div class="flex items-center">
                  <i class="pi pi-credit-card mr-2 text-blue-400"></i>
                  <span class="text-white/70">Pago con Tarjeta:</span>
                </div>
                <span class="text-white font-medium">{{
                  formatCurrency(paymentForm.card || 0)
                }}</span>
              </div>
            </div>

            <hr class="border-white/20" />

            <!-- Totals Section -->
            <div class="space-y-3">
              <div class="flex justify-between items-center p-3 bg-blue-500/10 rounded-lg">
                <span class="text-white font-bold">Total a Pagar:</span>
                <span class="text-blue-300 font-bold text-2xl">{{
                  formatCurrency(totalWithAdjustments)
                }}</span>
              </div>

              <div class="flex justify-between items-center">
                <span class="text-white/70 font-medium">Total Pagado:</span>
                <span class="text-white font-bold text-lg">{{ formatCurrency(totalPayment) }}</span>
              </div>

              <hr class="border-white/10" />

              <!-- Balance Status with enhanced visual feedback -->
              <div
                class="flex justify-between items-center p-3 rounded-lg"
                :class="{
                  'bg-green-500/10 border border-green-400/30': Math.abs(remainingBalance) < 0.01,
                  'bg-red-500/10 border border-red-400/30': Math.abs(remainingBalance) >= 0.01,
                }"
              >
                <div class="flex items-center">
                  <i
                    :class="{
                      'pi pi-check-circle text-green-400': Math.abs(remainingBalance) < 0.01,
                      'pi pi-exclamation-circle text-red-400': Math.abs(remainingBalance) >= 0.01,
                    }"
                    class="mr-2"
                  ></i>
                  <span
                    :class="{
                      'text-green-400': Math.abs(remainingBalance) < 0.01,
                      'text-red-400': Math.abs(remainingBalance) >= 0.01,
                    }"
                    class="font-medium"
                  >
                    {{
                      Math.abs(remainingBalance) < 0.01
                        ? '✅ Pago Completo'
                        : remainingBalance > 0
                          ? '⚠️ Falta por Pagar'
                          : '💰 Exceso de Pago'
                    }}
                  </span>
                </div>
                <span
                  :class="{
                    'text-green-400': Math.abs(remainingBalance) < 0.01,
                    'text-red-400': Math.abs(remainingBalance) >= 0.01,
                  }"
                  class="font-bold text-lg"
                >
                  {{ Math.abs(remainingBalance) < 0.01 ? '✓' : formatCurrency(Math.abs(remainingBalance)) }}
                </span>
              </div>
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

  // Mantener el monto original del empeño como referencia
  const originalPawnAmount = ref(0)

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
    autoFillCash: autoFillCashComposable,
    autoFillCard: autoFillCardComposable,
    splitPayment: splitPaymentComposable,
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

  // Sobrescribir las funciones de auto-fill para considerar recargos adicionales
  const autoFillCash = () => {
    paymentForm.value.card = 0
    paymentForm.value.selectedCard = null
    
    const base = originalPawnAmount.value
    const discount = paymentForm.value.discount || 0
    const additionalSurcharge = paymentForm.value.surcharge || 0
    const adjusted = base - discount + additionalSurcharge
    
    paymentForm.value.cash = adjusted
    showSuccess(`Pago configurado: todo en efectivo ${formatCurrency(adjusted)}`)
  }

  const autoFillCard = () => {
    if (isLoadingCards.value) {
      showError('Espera a que terminen de cargar las tarjetas')
      return
    }
    if (!paymentCards.value || paymentCards.value.length === 0) {
      showError('No hay tarjetas de pago disponibles')
      return
    }

    // Limpiar efectivo
    paymentForm.value.cash = 0
    
    // Si ya hay una tarjeta seleccionada, recalcular
    if (paymentForm.value.selectedCard) {
      handleCardSelection()
    } else {
      // Si no hay tarjeta, mostrar el monto ajustado sin interés aún
      const base = originalPawnAmount.value
      const discount = paymentForm.value.discount || 0
      const additionalSurcharge = paymentForm.value.surcharge || 0
      const adjusted = base - discount + additionalSurcharge
      
      paymentForm.value.card = adjusted
      showSuccess(`Selecciona una tarjeta para calcular el interés sobre ${formatCurrency(adjusted)}`)
    }
  }

  const splitPayment = () => {
    if (isLoadingCards.value) {
      showError('Espera a que terminen de cargar las tarjetas')
      return
    }
    if (!paymentCards.value || paymentCards.value.length === 0) {
      showError('No hay tarjetas de pago disponibles')
      return
    }
    if (!paymentForm.value.selectedCard) {
      showError('Selecciona una tarjeta antes de dividir el pago')
      return
    }

    const base = originalPawnAmount.value
    const discount = paymentForm.value.discount || 0
    const additionalSurcharge = paymentForm.value.surcharge || 0
    const adjusted = base - discount + additionalSurcharge
    
    // Dividir el monto ajustado
    const half = Math.round(adjusted / 2)
    
    // Efectivo recibe la mitad
    paymentForm.value.cash = half
    
    // Para la tarjeta: calcular interés sobre la otra mitad
    const cardBase = adjusted - half
    const card = paymentForm.value.selectedCard
    const porcentaje = card.montoPorcentual || 0
    const montoFijo = card.montoFijo || 0
    
    let interes = 0
    if (porcentaje > 0) {
      interes = (cardBase * porcentaje) / 100
    } else if (montoFijo > 0) {
      interes = montoFijo
    }
    
    paymentForm.value.card = cardBase + interes
    
    showSuccess(`Pago dividido: ${formatCurrency(half)} efectivo + ${formatCurrency(paymentForm.value.card)} tarjeta (incluye ${formatCurrency(interes)} de interés)`)
  }

  // Computed para calcular el interés actual de la tarjeta
  const currentCardInterest = computed(() => {
    if (!paymentForm.value.selectedCard) return 0
    
    const base = originalPawnAmount.value
    const discount = paymentForm.value.discount || 0
    const additionalSurcharge = paymentForm.value.surcharge || 0
    const adjustedBase = base - discount + additionalSurcharge
    
    const card = paymentForm.value.selectedCard
    const porcentaje = card.montoPorcentual || 0
    const montoFijo = card.montoFijo || 0
    
    if (porcentaje > 0) {
      return (adjustedBase * porcentaje) / 100
    } else if (montoFijo > 0) {
      return montoFijo
    }
    
    return 0
  })

  // Computed para el subtotal sin interés
  const subtotalWithoutInterest = computed(() => {
    const base = originalPawnAmount.value
    const discount = paymentForm.value.discount || 0
    const additionalSurcharge = paymentForm.value.surcharge || 0
    return base - discount + additionalSurcharge
  })

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
    if (!paymentForm.value.card || paymentForm.value.card <= 0) {
      // Si el monto de tarjeta queda vacío → limpiar selección
      paymentForm.value.selectedCard = null
    }
  }

  const handleCardSelection = () => {
    const selectedCard = paymentForm.value.selectedCard
    
    // Si no hay tarjeta seleccionada, limpiar valores
    if (!selectedCard) {
      paymentForm.value.card = 0
      paymentForm.value.cash = 0
      showError('No se seleccionó ninguna tarjeta')
      return
    }

    // Base ajustada: monto original - descuento + recargo adicional
    const baseAmount = originalPawnAmount.value
    const discount = paymentForm.value.discount || 0
    const additionalSurcharge = paymentForm.value.surcharge || 0
    
    const adjustedBase = baseAmount - discount + additionalSurcharge
    
    if (adjustedBase <= 0) {
      showError('El monto ajustado no es válido')
      return
    }

    // Calcular el interés sobre la BASE AJUSTADA (incluye recargos adicionales)
    const porcentaje = selectedCard.montoPorcentual || 0
    const montoFijo = selectedCard.montoFijo || 0
    
    let interes = 0
    if (porcentaje > 0) {
      interes = (adjustedBase * porcentaje) / 100
    } else if (montoFijo > 0) {
      interes = montoFijo
    }

    console.log('📊 Cálculo de interés por medio de pago:', {
      baseAmount,
      discount,
      additionalSurcharge,
      adjustedBase,
      porcentaje,
      montoFijo,
      interes,
      totalCard: adjustedBase + interes,
      tarjeta: selectedCard.nombre
    })

    // El monto de tarjeta es: base ajustada + interés
    paymentForm.value.card = adjustedBase + interes
    
    // Limpiar efectivo cuando se selecciona pago total con tarjeta
    paymentForm.value.cash = 0

    const breakdown = []
    breakdown.push(`Monto base: ${formatCurrency(baseAmount)}`)
    if (discount > 0) breakdown.push(`Descuento: -${formatCurrency(discount)}`)
    if (additionalSurcharge > 0) breakdown.push(`Recargo adicional: +${formatCurrency(additionalSurcharge)}`)
    breakdown.push(`Subtotal: ${formatCurrency(adjustedBase)}`)
    breakdown.push(`Interés ${porcentaje}%: +${formatCurrency(interes)}`)
    
    showSuccess(
      `Tarjeta "${selectedCard.nombre}" - ${breakdown.join(', ')} = Total: ${formatCurrency(paymentForm.value.card)}`
    )
  }

  const confirmPayment = async (): Promise<void> => {
    console.log('🔄 Confirming payment...')
    console.log('💰 Payment validation:', {
      isValid: isPaymentValid.value,
      remainingBalance: remainingBalance.value,
      totalRequired: totalWithAdjustments.value,
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
        montoTarjeta: paymentForm.value.card || 0,
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
      // Guardar el monto original del empeño
      originalPawnAmount.value = props.pawn.monto || 0
      
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
        // Actualizar el monto original cuando cambia el empeño
        originalPawnAmount.value = newPawn.monto || 0
        
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

  // Watch para recalcular el monto de tarjeta cuando cambian descuento o recargo adicional
  watch(
    () => [paymentForm.value.discount, paymentForm.value.surcharge],
    () => {
      // Si hay una tarjeta seleccionada, recalcular
      if (paymentForm.value.selectedCard) {
        console.log('🔄 Recalculando por cambio en descuento/recargo')
        handleCardSelection()
      }
    }
  )

  // Watch para cuando cambia la tarjeta seleccionada
  watch(
    () => paymentForm.value.selectedCard,
    (newCard, oldCard) => {
      if (newCard && newCard !== oldCard) {
        console.log('🔄 Tarjeta cambiada, recalculando...')
        handleCardSelection()
      }
    }
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