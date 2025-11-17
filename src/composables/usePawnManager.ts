import { ref, computed, type Ref, type ComputedRef } from 'vue'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'
// @ts-ignore
import { useAuthStore } from '../store/auth.js'
import { PawnService } from '../services/pawnService'
import type {
  PawnDto,
  PaymentCardDto,
  FormattedPawnDto,
  PawnPaymentForm,
  PawnStatistics,
  PawnStatus,
  PawnSortField,
  PawnStatusFilter,
} from '../types'

export function usePawnManager() {
  const toast = useToast()
  const confirm = useConfirm()
  const authStore = useAuthStore()

  // Data state
  const pawns = ref<PawnDto[]>([])
  const paymentCards = ref<PaymentCardDto[]>([])

  // UI state
  const isLoading = ref(false)
  const showPaymentModal = ref(false)
  const selectedPawn = ref<PawnDto | null>(null)
  const viewMode = ref<'active' | 'all'>('active')

  // Payment form state
  const paymentForm = ref<PawnPaymentForm>({
    cash: 0,
    card: 0,
    discount: 0,
    surcharge: 0,
    selectedCard: null,
    observation: '',
  })

  // Loading states
  const isSubmittingPayment = ref(false)
  const isLoadingCards = ref(false)

  // Helper: calculate surcharge for a card over a base amount
  const calculateCardSurcharge = (card: PaymentCardDto | null, base: number): number => {
    if (!card || base <= 0) return 0
    const percentage = Number((card as any).montoPorcentual || 0)
    const fixed = Number((card as any).montoFijo || 0)
    if (percentage > 0) return (base * percentage) / 100
    if (fixed > 0) return fixed
    return 0
  }

  // Computeds
  // Card surcharge: calculated over the pawn amount or card amount depending on the strategy.
  
// NUEVO CAMBIO:

const totalWithAdjustments = computed(() => {
  if (!selectedPawn.value) return 0
  
  const base = selectedPawn.value.monto
  const discount = paymentForm.value.discount || 0
  const additionalSurcharge = paymentForm.value.surcharge || 0
  
  // Si hay tarjeta seleccionada, calcular el interés sobre el monto base
  let cardInterest = 0
  if (paymentForm.value.selectedCard && paymentForm.value.card > 0) {
    const card = paymentForm.value.selectedCard
    const porcentaje = card.montoPorcentual || 0
    const montoFijo = card.montoFijo || 0
    
    if (porcentaje > 0) {
  cardInterest = ((base + additionalSurcharge) * porcentaje) / 100
} else if (montoFijo > 0) {
  cardInterest = montoFijo
}

  }
  
  // Total = base - descuento + recargo adicional + interés de tarjeta
  const total = base - discount + additionalSurcharge + cardInterest
  
  return total
})


  const totalPayment = computed(() => {
    return (paymentForm.value.cash || 0) + (paymentForm.value.card || 0)
  })

  const remainingBalance = computed(() => {
    return totalWithAdjustments.value - totalPayment.value
  })
// ========================================
// EXPLICACIÓN DE LA LÓGICA:
// ========================================
// Ejemplo: Empeño de $1,000 con tarjeta de 15%
//
// totalWithAdjustments = $1,000 (monto base sin ajustes)
// paymentForm.card = $1,150 (incluye $1,000 + $150 de interés)
// totalPayment = $1,150
// remainingBalance = $1,000 - $1,150 = -$150 ❌ INCORRECTO
//
// CORRECTO: Si el cliente paga con tarjeta, DEBE pagar más
// totalWithAdjustments = $1,150 (debe incluir el interés)
// paymentForm.card = $1,150
// totalPayment = $1,150
// remainingBalance = $1,150 - $1,150 = $0 ✅ CORRECTO
//


  const isPaymentValid = computed(() => {
    return Math.abs(remainingBalance.value) < 0.01
  })

  const formattedPawns = computed<FormattedPawnDto[]>(() => {
    return pawns.value.map((pawn) => ({
      ...pawn,
      formattedAmount: formatCurrency(pawn.monto),
      formattedDate: formatDate(pawn.fechaRegistro),
      status: getPawnStatus(pawn),
      daysOverdue: calculateDaysOverdue(pawn.fechaRegistro),
      montoFormateado: formatCurrency(pawn.monto),
      fechaFormateada: formatDate(pawn.fechaRegistro),
      diasTranscurridos: calculateDaysOverdue(pawn.fechaRegistro),
      statusColor: getStatusColor(getPawnStatus(pawn)),
      statusIcon: getStatusIcon(getPawnStatus(pawn)),
    }))
  })

  const searchTerm: Ref<string> = ref('')
  const statusFilter: Ref<PawnStatusFilter> = ref('all')
  const sortBy: Ref<PawnSortField> = ref('date')

  const filteredPawns = computed(() => {
    let filtered = formattedPawns.value.slice()

    if (viewMode.value === 'active') {
      filtered = filtered.filter((p) => p.estadoPago === 'Pendiente')
    }

    if (searchTerm.value) {
      const term = searchTerm.value.toLowerCase()
      filtered = filtered.filter(
        (pawn) =>
          (pawn.detalle || '').toLowerCase().includes(term) ||
          pawn.monto.toString().includes(term) ||
          pawn.empenoId.toString().includes(term)
      )
    }

    if (statusFilter.value !== 'all' && viewMode.value === 'active') {
      filtered = filtered.filter((pawn) => pawn.status === statusFilter.value)
    } else if (statusFilter.value !== 'all' && viewMode.value === 'all') {
      if (statusFilter.value === 'paid') filtered = filtered.filter((p) => p.estadoPago === 'Pagado')
      if (statusFilter.value === 'unpaid') filtered = filtered.filter((p) => p.estadoPago === 'Pendiente')
    }

    filtered.sort((a, b) => {
      switch (sortBy.value) {
        case 'amount':
          return b.monto - a.monto
        case 'status':
          if (viewMode.value === 'all' && a.estadoPago !== b.estadoPago) {
            return a.estadoPago === 'Pagado' ? -1 : 1
          }
          return a.status.localeCompare(b.status)
        case 'date':
        default:
          return new Date(b.fechaRegistro).getTime() - new Date(a.fechaRegistro).getTime()
      }
    })

    return filtered
  })

  const statistics = computed(() => {
    const relevant = viewMode.value === 'active' ? pawns.value.filter((p) => p.estadoPago === 'Pendiente') : pawns.value
    const total = relevant.length
    const totalAmount = relevant.reduce((s, p) => s + (p.monto || 0), 0)
    const overdue = viewMode.value === 'active' ? relevant.filter((p) => getPawnStatus(p) === 'overdue').length : pawns.value.filter((p) => p.estadoPago === 'Pagado').length
    const averageAmount = total > 0 ? totalAmount / total : 0
    return {
      total,
      totalAmount: formatCurrency(totalAmount),
      overdue,
      averageAmount: formatCurrency(averageAmount),
      overduePercentage: total > 0 ? Math.round((overdue / total) * 100) : 0,
    } as PawnStatistics
  })

  // Methods

  const openPaymentModal = (pawn: PawnDto) => {
    selectedPawn.value = pawn
    resetPaymentForm()
    showPaymentModal.value = true
    paymentForm.value.observation = `Pago de empeño correspondiente a la visita ${pawn.visitaId}`
    // preload payment cards if not loaded
    if (!paymentCards.value.length) fetchPaymentCards()
  }

  const closePaymentModal = () => {
    showPaymentModal.value = false
    selectedPawn.value = null
    resetPaymentForm()
  }

  const resetPaymentForm = () => {
    paymentForm.value = {
      cash: 0,
      card: 0,
      discount: 0,
      surcharge: 0,
      selectedCard: null,
      observation: selectedPawn.value ? `Pago de empeño correspondiente a la visita ${selectedPawn.value.visitaId}` : '',
    }
  }

  // When user selects a card from the UI this should be called (e.g. @change="handleCardSelection")
  const handleCardSelection = () => {
    const card = paymentForm.value.selectedCard
    // If user deselected card -> reset card-related fields and keep manual surcharge if any
    if (!card) {
      paymentForm.value.card = 0
      paymentForm.value.surcharge = 0
      showWarning('Tarjeta deseleccionada.')
      return
    }

    if (!selectedPawn.value) {
      showWarning('Seleccioná un empeño primero')
      return
    }

    const base = selectedPawn.value.monto || 0

    // Reset previous values to avoid accumulation
    paymentForm.value.card = 0
    paymentForm.value.cash = 0
    paymentForm.value.surcharge = 0

    // Calculate surcharge and assign
    const surcharge = calculateCardSurcharge(card, base)
    paymentForm.value.surcharge = surcharge
    paymentForm.value.card = base + surcharge

    showSuccess(
      `Tarjeta seleccionada (${(card as any).nombre}). Total: ${formatCurrency(paymentForm.value.card)}`
    )
  }

  // Auto-fill cash: full payment in cash, clear card/method
  const autoFillCash = () => {
  paymentForm.value.card = 0
  paymentForm.value.selectedCard = null
  paymentForm.value.surcharge = 0
  const total = selectedPawn.value?.monto || 0
  const adjusted = total - (paymentForm.value.discount || 0) + (paymentForm.value.surcharge || 0)
  paymentForm.value.cash = adjusted
  showSuccess(`Pago configurado: todo en efectivo ${formatCurrency(adjusted)}`)
}


  // Auto-fill card: compute surcharge based on pawn base and set card to original + surcharge
  // IMPORTANT: do NOT auto-assign the first card; require the user to select the card explicitly
  const autoFillCard = () => {
    if (isLoadingCards.value) {
      showWarning('Espera a que terminen de cargar las tarjetas')
      return
    }
    if (!selectedPawn.value) {
      showWarning('Seleccioná un empeño primero')
      return
    }

    const original = selectedPawn.value.monto || 0

    // Pago completo con tarjeta, sin aplicar recargo automático
    paymentForm.value.card = original
    paymentForm.value.cash = 0
    paymentForm.value.surcharge = 0

    // No obligamos a seleccionar tarjeta aquí
    paymentForm.value.selectedCard = null

    showSuccess(`Pago configurado: todo con tarjeta (${formatCurrency(paymentForm.value.card)} total)`)
  }


  // Split payment: keep deterministic calculation, surcharge applied over card portion's base amount
  const splitPayment = () => {
    if (isLoadingCards.value) {
      showWarning('Espera a que terminen de cargar las tarjetas')
      return
    }
    if (!selectedPawn.value) {
      showWarning('Seleccioná un empeño primero')
      return
    }
    if (!paymentCards.value.length) {
      showError('No hay tarjetas disponibles')
      return
    }

    if (!paymentForm.value.selectedCard) {
      showWarning('Seleccioná una tarjeta antes de continuar')
      return
    }

    const original = selectedPawn.value.monto || 0
    // deterministic split
    const half = Math.round(original / 2)

    // cash gets half (base)
    paymentForm.value.cash = half

    // surcharge must be computed only over the card base (half)
    const surchargeOnCard = calculateCardSurcharge(paymentForm.value.selectedCard as PaymentCardDto, half)
    paymentForm.value.surcharge = surchargeOnCard
    // card should cover remaining base (original - half) plus surcharge on the card portion
    paymentForm.value.card = (original - half) + surchargeOnCard

    showSuccess(`Pago dividido: ${formatCurrency(paymentForm.value.cash)} efectivo + ${formatCurrency(paymentForm.value.card)} tarjeta (Recargo: ${formatCurrency(surchargeOnCard)})`)
  }

  const fetchPaymentCards = async (): Promise<void> => {
    try {
      isLoadingCards.value = true
      const institucionID = authStore.institucionID
      if (!institucionID) {
        showError('No se encontró la institución')
        return
      }
      const response = await PawnService.getPaymentCards(institucionID)
      paymentCards.value = response?.data || []
    } catch (err) {
      console.error(err)
      showError('Error al cargar tarjetas de pago')
      paymentCards.value = []
    } finally {
      isLoadingCards.value = false
    }
  }

  const formatDate = (dateString: string) =>
    new Date(dateString).toLocaleDateString('es-CO', { year: 'numeric', month: 'short', day: '2-digit', hour: '2-digit', minute: '2-digit' })

  const getPawnStatus = (pawn: PawnDto): PawnStatus => {
    const days = calculateDaysOverdue(pawn.fechaRegistro)
    if (days > 30) return 'overdue'
    if (days > 15) return 'warning'
    return 'active'
  }

  const calculateDaysOverdue = (dateString: string) => {
    const pawnDate = new Date(dateString)
    const now = new Date()
    const diff = now.getTime() - pawnDate.getTime()
    return Math.floor(diff / (1000 * 60 * 60 * 24))
  }

  const getStatusColor = (status: PawnStatus) => {
    switch (status) {
      case 'overdue': return 'text-red-400'
      case 'warning': return 'text-yellow-400'
      case 'active': return 'text-green-400'
      default: return 'text-white'
    }
  }

  const getStatusIcon = (status: PawnStatus) => {
    switch (status) {
      case 'overdue': return 'pi-exclamation-triangle'
      case 'warning': return 'pi-clock'
      case 'active': return 'pi-check-circle'
      default: return 'pi-circle'
    }
  }

  // Toast helpers
  const showSuccess = (message: string) => toast.add({ severity: 'success', summary: 'Éxito', detail: message, life: 5000 })
  const showError = (message: string) => toast.add({ severity: 'error', summary: 'Error', detail: message, life: 5000 })
  const showWarning = (message: string) => toast.add({ severity: 'warn', summary: 'Advertencia', detail: message, life: 5000 })

  // Confirmation dialog wrapper used elsewhere
  const confirmPayment = (): Promise<boolean> => {
    return new Promise((resolve) => {
      confirm.require({
        message: `¿Confirmar el pago de ${formatCurrency(totalWithAdjustments.value)} para el empeño ${selectedPawn.value?.empenoId}?`,
        header: 'Confirmar Pago',
        icon: 'pi pi-question-circle',
        acceptLabel: 'Sí, confirmar',
        rejectLabel: 'Cancelar',
        acceptClass: 'p-button-success',
        accept: () => resolve(true),
        reject: () => resolve(false),
      })
    })
  }

  // Fetch pawns
  const fetchPawns = async (mode: 'active' | 'all' = viewMode.value): Promise<void> => {
    try {
      isLoading.value = true
      let response
      if (mode === 'all') response = await PawnService.getAllPawns()
      else response = await PawnService.getPawns()
      if (response && response.isSuccess && response.data) pawns.value = response.data
      else pawns.value = []
    } catch (err) {
      console.error(err)
      pawns.value = []
    } finally {
      isLoading.value = false
    }
  }

  const toggleViewMode = () => {
    viewMode.value = viewMode.value === 'active' ? 'all' : 'active'
    fetchPawns(viewMode.value)
  }

  const formatCurrency = (amount: number): string => {
    return new Intl.NumberFormat('es-AR', {
      style: 'currency',
      currency: 'ARS',
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(amount || 0)
  }

  // exposed: keep applyCardRecalculation for backward compatibility
  const applyCardRecalculation = () => {
    if (!selectedPawn.value || !paymentForm.value.selectedCard) return

    const pawnAmount = selectedPawn.value.monto || 0
    const card = paymentForm.value.selectedCard

    const surcharge = calculateCardSurcharge(card, pawnAmount)

    paymentForm.value.surcharge = surcharge
    paymentForm.value.card = pawnAmount + surcharge
    paymentForm.value.cash = 0
  }

  return {
    // Data state
    pawns,
    paymentCards,

    // UI state
    isLoading,
    showPaymentModal,
    selectedPawn,
    viewMode,

    // Payment form
    paymentForm,
    isSubmittingPayment,
    isLoadingCards,

    // Computed values
    totalWithAdjustments,
    totalPayment,
    remainingBalance,
    isPaymentValid,
    formattedPawns,
    filteredPawns,
    statistics,

    // Filters
    searchTerm,
    statusFilter,
    sortBy,

    // Methods
    openPaymentModal,
    closePaymentModal,
    resetPaymentForm,
    handleCardSelection,
    autoFillCash,
    autoFillCard,
    splitPayment,
    fetchPaymentCards,
    fetchPawns,
    toggleViewMode,
    formatCurrency,
    formatDate,
    getPawnStatus,
    calculateDaysOverdue,
    getStatusColor,
    getStatusIcon,
    showSuccess,
    showError,
    showWarning,
    confirmPayment,
    // utils
    applyCardRecalculation,
  }
}
