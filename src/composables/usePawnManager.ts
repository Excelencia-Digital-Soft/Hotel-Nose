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

interface UsePawnManagerReturn {
  // Data state
  pawns: Ref<PawnDto[]>
  paymentCards: Ref<PaymentCardDto[]>

  // UI state
  isLoading: Ref<boolean>
  showPaymentModal: Ref<boolean>
  selectedPawn: Ref<PawnDto | null>
  viewMode: Ref<'active' | 'all'>

  // Payment form
  paymentForm: Ref<PawnPaymentForm>
  isSubmittingPayment: Ref<boolean>
  isLoadingCards: Ref<boolean>

  // Computed values
  totalWithAdjustments: ComputedRef<number>
  cardSurcharge: ComputedRef<number>
  totalPayment: ComputedRef<number>
  remainingBalance: ComputedRef<number>
  isPaymentValid: ComputedRef<boolean>
  formattedPawns: ComputedRef<FormattedPawnDto[]>
  filteredPawns: ComputedRef<FormattedPawnDto[]>
  statistics: ComputedRef<PawnStatistics>

  // Filters
  searchTerm: Ref<string>
  statusFilter: Ref<PawnStatusFilter>
  sortBy: Ref<PawnSortField>

  // Methods
  openPaymentModal: (pawn: PawnDto) => void
  closePaymentModal: () => void
  resetPaymentForm: () => void
  handleCardSelection: () => void
  autoFillCash: () => void
  autoFillCard: () => void
  splitPayment: () => void
  fetchPaymentCards: () => Promise<void>
  fetchPawns: (mode?: 'active' | 'all') => Promise<void>
  toggleViewMode: () => void
  formatCurrency: (amount: number) => string
  formatDate: (dateString: string) => string
  getPawnStatus: (pawn: PawnDto) => PawnStatus
  calculateDaysOverdue: (dateString: string) => number
  getStatusColor: (status: PawnStatus) => string
  getStatusIcon: (status: PawnStatus) => string
  showSuccess: (message: string) => void
  showError: (message: string) => void
  showWarning: (message: string) => void
  confirmPayment: () => Promise<boolean>
}

export function usePawnManager(): UsePawnManagerReturn {
  const toast = useToast()
  const confirm = useConfirm()
  const authStore = useAuthStore()

  // Data state
  const pawns: Ref<PawnDto[]> = ref([])
  const paymentCards: Ref<PaymentCardDto[]> = ref([])

  // UI state
  const isLoading: Ref<boolean> = ref(false)
  const showPaymentModal: Ref<boolean> = ref(false)
  const selectedPawn: Ref<PawnDto | null> = ref(null)
  const viewMode: Ref<'active' | 'all'> = ref('active')

  // Payment form state
  const paymentForm: Ref<PawnPaymentForm> = ref({
    cash: 0,
    card: 0,
    discount: 0,
    surcharge: 0,
    selectedCard: null,
    observation: '',
  })

  // Loading states
  const isSubmittingPayment: Ref<boolean> = ref(false)
  const isLoadingCards: Ref<boolean> = ref(false)

  // Computed values
  const totalWithAdjustments: ComputedRef<number> = computed(() => {
    if (!selectedPawn.value) return 0
    return selectedPawn.value.monto - paymentForm.value.discount + paymentForm.value.surcharge
  })

  const cardSurcharge: ComputedRef<number> = computed(() => {
    // No automatic card surcharge - user will input manually in surcharge field
    return 0
  })

  const totalPayment: ComputedRef<number> = computed(() => {
    return paymentForm.value.cash + paymentForm.value.card
  })

  const remainingBalance: ComputedRef<number> = computed(() => {
    // Balance = Total with adjustments (includes manual surcharge) - total payment
    return totalWithAdjustments.value - totalPayment.value
  })

  const isPaymentValid: ComputedRef<boolean> = computed(() => {
    const absBalance = Math.abs(remainingBalance.value)
    console.log('💰 Payment validation:', {
      remainingBalance: remainingBalance.value,
      absBalance,
      isValid: absBalance < 1, // Increased tolerance to 1 peso
    })
    return absBalance < 1 // Increased tolerance for debugging
  })

  const formattedPawns: ComputedRef<FormattedPawnDto[]> = computed(() => {
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

  // Filters and search
  const searchTerm: Ref<string> = ref('')
  const statusFilter: Ref<PawnStatusFilter> = ref('all')
  const sortBy: Ref<PawnSortField> = ref('date')

  const filteredPawns: ComputedRef<FormattedPawnDto[]> = computed(() => {
    let filtered = formattedPawns.value

    // Filter by view mode - in 'active' mode, only show unpaid pawns
    if (viewMode.value === 'active') {
      const beforeFilter = filtered.length
      filtered = filtered.filter((pawn) => pawn.estadoPago === 'Pendiente')
      console.log(`✅ Active mode filter: ${beforeFilter} → ${filtered.length} pawns`)
    }
    // In 'all' mode, show all pawns (both paid and unpaid)

    // Search filter
    if (searchTerm.value) {
      const term = searchTerm.value.toLowerCase()
      filtered = filtered.filter(
        (pawn) =>
          (pawn.detalle || '').toLowerCase().includes(term) ||
          pawn.monto.toString().includes(term) ||
          pawn.empenoId.toString().includes(term)
      )
    }

    // Status filter - only apply time-based status filtering in active mode
    if (statusFilter.value !== 'all' && viewMode.value === 'active') {
      filtered = filtered.filter((pawn) => pawn.status === statusFilter.value)
    } else if (statusFilter.value !== 'all' && viewMode.value === 'all') {
      // In 'all' mode, allow filtering by payment status
      if (statusFilter.value === 'paid') {
        filtered = filtered.filter((pawn) => pawn.estadoPago === 'Pagado')
      } else if (statusFilter.value === 'unpaid') {
        filtered = filtered.filter((pawn) => pawn.estadoPago === 'Pendiente')
      }
    }

    // Sort
    filtered.sort((a, b) => {
      switch (sortBy.value) {
        case 'amount':
          return b.monto - a.monto
        case 'status':
          // In 'all' mode, sort by payment status first, then by time status
          if (viewMode.value === 'all') {
            if (a.estadoPago !== b.estadoPago) {
              return a.estadoPago === 'Pagado' ? -1 : 1
            }
          }
          return a.status.localeCompare(b.status)
        case 'date':
        default:
          return new Date(b.fechaRegistro).getTime() - new Date(a.fechaRegistro).getTime()
      }
    })

    return filtered
  })

  // Statistics
  const statistics: ComputedRef<PawnStatistics> = computed(() => {
    // Calculate based on the filtered view
    const relevantPawns =
      viewMode.value === 'active'
        ? pawns.value.filter((pawn) => pawn.estadoPago === 'Pendiente')
        : pawns.value

    const total = relevantPawns.length
    const totalAmount = relevantPawns.reduce((sum, pawn) => sum + pawn.monto, 0)

    let overdue = 0
    if (viewMode.value === 'active') {
      overdue = relevantPawns.filter((pawn) => getPawnStatus(pawn) === 'overdue').length
    } else {
      overdue = pawns.value.filter((pawn) => pawn.estadoPago === 'Pagado').length
    }

    const averageAmount = total > 0 ? totalAmount / total : 0

    return {
      total,
      totalAmount: formatCurrency(totalAmount),
      overdue,
      averageAmount: formatCurrency(averageAmount),
      overduePercentage: total > 0 ? Math.round((overdue / total) * 100) : 0,
    }
  })

  // Payment modal methods
  const openPaymentModal = (pawn: PawnDto): void => {
    selectedPawn.value = pawn
    resetPaymentForm()
    showPaymentModal.value = true

    // Set default observation
    paymentForm.value.observation = `Pago de empeño correspondiente a la visita ${pawn.visitaId}`
  }

  const closePaymentModal = (): void => {
    showPaymentModal.value = false
    selectedPawn.value = null
    resetPaymentForm()
  }

  const resetPaymentForm = (): void => {
    paymentForm.value = {
      cash: 0,
      card: 0,
      discount: 0,
      surcharge: 0,
      selectedCard: null,
      observation: selectedPawn.value
        ? `Pago de empeño correspondiente a la visita ${selectedPawn.value.visitaId}`
        : '',
    }
  }

  // Card selection handling
  const handleCardSelection = (): void => {
    if (paymentForm.value.selectedCard && paymentForm.value.card > 0) {
      // Recalculate card amount with surcharge
      //const baseAmount = paymentForm.value.card
      //const percentage = paymentForm.value.selectedCard.montoPorcentual || 0
      //const surcharge = (baseAmount * percentage) / 100
      // Update the card amount to include surcharge for display
      // But keep the original amount for calculation
    }
  }

  // Auto-fill payment methods
  const autoFillCash = (): void => {
    paymentForm.value.cash = totalWithAdjustments.value
    paymentForm.value.card = 0
    paymentForm.value.selectedCard = null
    showSuccess(`Pago configurado: todo en efectivo ${formatCurrency(totalWithAdjustments.value)}`)
  }

  const autoFillCard = (): void => {
    if (isLoadingCards.value) {
      showWarning('Espera a que terminen de cargar las tarjetas')
      return
    }

    if (!paymentCards.value.length) {
      showError('No hay tarjetas disponibles')
      return
    }

    // Select the first available card
    if (!paymentForm.value.selectedCard && paymentCards.value.length > 0) {
      paymentForm.value.selectedCard = paymentCards.value[0]
    }

    // Simply set all amount to card - user will add surcharge manually if needed
    paymentForm.value.cash = 0
    paymentForm.value.card = totalWithAdjustments.value

    showSuccess(`Pago configurado: todo con tarjeta ${paymentForm.value.selectedCard?.nombre}`)
  }

  const splitPayment = (): void => {
    if (isLoadingCards.value) {
      showWarning('Espera a que terminen de cargar las tarjetas')
      return
    }

    if (!paymentCards.value.length) {
      showError('No hay tarjetas disponibles')
      return
    }

    // Select the first available card
    if (!paymentForm.value.selectedCard && paymentCards.value.length > 0) {
      paymentForm.value.selectedCard = paymentCards.value[0]
    }

    // Simple 50/50 split - user will add surcharge manually if needed
    const total = totalWithAdjustments.value
    paymentForm.value.cash = Math.round(total / 2)
    paymentForm.value.card = total - paymentForm.value.cash

    showSuccess(
      `Pago dividido: ${formatCurrency(paymentForm.value.cash)} efectivo + ${formatCurrency(paymentForm.value.card)} tarjeta`
    )
  }

  // Fetch payment cards
  const fetchPaymentCards = async (): Promise<void> => {
    try {
      isLoadingCards.value = true

      const institucionID = authStore.institucionID
      console.log('🔍 Fetching payment cards for institucionID:', institucionID)

      if (!institucionID) {
        showError('No se encontró la institución')
        return
      }

      const response = await PawnService.getPaymentCards(institucionID)
      console.log('📋 Payment cards response:', response)

      if (response && response.isSuccess && response.data) {
        paymentCards.value = response.data
        console.log('✅ Payment cards loaded:', response.data.length, 'cards')
      } else if (response && response.data) {
        // Try legacy response format
        paymentCards.value = response.data
        console.log('✅ Payment cards loaded (legacy format):', response.data.length, 'cards')
      } else {
        paymentCards.value = []
        console.warn('⚠️ No payment cards found')
        showError(response?.message || 'No se encontraron tarjetas de pago')
      }
    } catch (error) {
      console.error('❌ Error fetching payment cards:', error)
      showError('Error al cargar tarjetas de pago')
      paymentCards.value = []
    } finally {
      isLoadingCards.value = false
    }
  }

  // Utility methods
  const formatCurrency = (amount: number): string => {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP',
      minimumFractionDigits: 0,
    }).format(amount)
  }

  const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleDateString('es-CO', {
      year: 'numeric',
      month: 'short',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
    })
  }

  const getPawnStatus = (pawn: PawnDto): PawnStatus => {
    const daysOverdue = calculateDaysOverdue(pawn.fechaRegistro)
    if (daysOverdue > 30) return 'overdue'
    if (daysOverdue > 15) return 'warning'
    return 'active'
  }

  const calculateDaysOverdue = (dateString: string): number => {
    const pawnDate = new Date(dateString)
    const now = new Date()
    const diffTime = now.getTime() - pawnDate.getTime()
    return Math.floor(diffTime / (1000 * 60 * 60 * 24))
  }

  const getStatusColor = (status: PawnStatus): string => {
    switch (status) {
      case 'overdue':
        return 'text-red-400'
      case 'warning':
        return 'text-yellow-400'
      case 'active':
        return 'text-green-400'
      default:
        return 'text-white'
    }
  }

  const getStatusIcon = (status: PawnStatus): string => {
    switch (status) {
      case 'overdue':
        return 'pi-exclamation-triangle'
      case 'warning':
        return 'pi-clock'
      case 'active':
        return 'pi-check-circle'
      default:
        return 'pi-circle'
    }
  }

  // Toast messages
  const showSuccess = (message: string): void => {
    toast.add({
      severity: 'success',
      summary: 'Éxito',
      detail: message,
      life: 5000,
    })
  }

  const showError = (message: string): void => {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: message,
      life: 5000,
    })
  }

  const showWarning = (message: string): void => {
    toast.add({
      severity: 'warn',
      summary: 'Advertencia',
      detail: message,
      life: 5000,
    })
  }

  // Confirmation dialogs
  const confirmPayment = (): Promise<boolean> => {
    console.log('🤔 Showing confirmation dialog...', selectedPawn.value)
    return new Promise((resolve) => {
      confirm.require({
        message: `¿Confirmar el pago de ${formatCurrency(totalWithAdjustments.value)} para el empeño ${selectedPawn.value?.empenoId}?`,
        header: 'Confirmar Pago',
        icon: 'pi pi-question-circle',
        acceptLabel: 'Sí, confirmar',
        rejectLabel: 'Cancelar',
        acceptClass: 'p-button-success',
        accept: () => {
          console.log('✅ User confirmed payment')
          resolve(true)
        },
        reject: () => {
          console.log('❌ User rejected payment')
          resolve(false)
        },
      })
    })
  }

  // Fetch pawns method
  const fetchPawns = async (mode: 'active' | 'all' = viewMode.value): Promise<void> => {
    try {
      isLoading.value = true

      let response
      if (mode === 'all') {
        console.log('🔄 Fetching ALL pawns...')
        response = await PawnService.getAllPawns()
      } else {
        console.log('🔄 Fetching ACTIVE pawns...')
        response = await PawnService.getPawns()
      }

      if (response && response.isSuccess && response.data) {
        console.log(`📦 Received ${response.data.length} pawns for mode '${mode}':`, response.data)

        pawns.value = response.data
      } else {
        pawns.value = []
        console.warn('No pawns data received:', response)
      }
    } catch (error) {
      console.error('Error fetching pawns:', error)
      pawns.value = []
    } finally {
      isLoading.value = false
    }
  }

  // Toggle view mode
  const toggleViewMode = (): void => {
    viewMode.value = viewMode.value === 'active' ? 'all' : 'active'
    // Automatically fetch data for the new mode
    fetchPawns(viewMode.value)
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
    cardSurcharge,
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
  }
}
