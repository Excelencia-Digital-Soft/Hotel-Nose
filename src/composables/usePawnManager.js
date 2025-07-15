import { ref, computed } from 'vue'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'

export function usePawnManager() {
  const toast = useToast()
  const confirm = useConfirm()

  // Data state
  const pawns = ref([])
  const paymentCards = ref([])

  // UI state
  const isLoading = ref(false)
  const showPaymentModal = ref(false)
  const selectedPawn = ref(null)

  // Payment form state
  const paymentForm = ref({
    cash: 0,
    card: 0,
    discount: 0,
    surcharge: 0,
    selectedCard: null,
    observation: ''
  })

  // Loading states
  const isSubmittingPayment = ref(false)
  const isLoadingCards = ref(false)

  // Computed values
  const totalWithAdjustments = computed(() => {
    if (!selectedPawn.value) return 0
    return selectedPawn.value.monto - paymentForm.value.discount + paymentForm.value.surcharge
  })

  const cardSurcharge = computed(() => {
    if (!paymentForm.value.selectedCard || !paymentForm.value.card) return 0
    const percentage = paymentForm.value.selectedCard.montoPorcentual || 0
    return (paymentForm.value.card * percentage) / 100
  })

  const totalPayment = computed(() => {
    return paymentForm.value.cash + paymentForm.value.card
  })

  const remainingBalance = computed(() => {
    return totalWithAdjustments.value + cardSurcharge.value - totalPayment.value
  })

  const isPaymentValid = computed(() => {
    return Math.abs(remainingBalance.value) < 0.01 // Allow for small floating point differences
  })

  const formattedPawns = computed(() => {
    return pawns.value.map(pawn => ({
      ...pawn,
      formattedAmount: formatCurrency(pawn.monto),
      formattedDate: formatDate(pawn.fechaRegistro),
      status: getPawnStatus(pawn),
      daysOverdue: calculateDaysOverdue(pawn.fechaRegistro)
    }))
  })

  // Filters and search
  const searchTerm = ref('')
  const statusFilter = ref('all') // all, active, overdue
  const sortBy = ref('date') // date, amount, status

  const filteredPawns = computed(() => {
    let filtered = formattedPawns.value

    // Search filter
    if (searchTerm.value) {
      const term = searchTerm.value.toLowerCase()
      filtered = filtered.filter(pawn => 
        (pawn.detalle || '').toLowerCase().includes(term) ||
        pawn.monto.toString().includes(term) ||
        pawn.empeñoID.toString().includes(term)
      )
    }

    // Status filter
    if (statusFilter.value !== 'all') {
      filtered = filtered.filter(pawn => pawn.status === statusFilter.value)
    }

    // Sort
    filtered.sort((a, b) => {
      switch (sortBy.value) {
        case 'amount':
          return b.monto - a.monto
        case 'status':
          return a.status.localeCompare(b.status)
        case 'date':
        default:
          return new Date(b.fechaRegistro) - new Date(a.fechaRegistro)
      }
    })

    return filtered
  })

  // Statistics
  const statistics = computed(() => {
    const total = pawns.value.length
    const totalAmount = pawns.value.reduce((sum, pawn) => sum + pawn.monto, 0)
    const overdue = pawns.value.filter(pawn => getPawnStatus(pawn) === 'overdue').length
    const averageAmount = total > 0 ? totalAmount / total : 0

    return {
      total,
      totalAmount: formatCurrency(totalAmount),
      overdue,
      averageAmount: formatCurrency(averageAmount),
      overduePercentage: total > 0 ? Math.round((overdue / total) * 100) : 0
    }
  })

  // Payment modal methods
  const openPaymentModal = (pawn) => {
    selectedPawn.value = pawn
    resetPaymentForm()
    showPaymentModal.value = true
    
    // Set default observation
    paymentForm.value.observation = `Pago de empeño correspondiente a la visita ${pawn.visitaID}`
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
      observation: selectedPawn.value ? `Pago de empeño correspondiente a la visita ${selectedPawn.value.visitaID}` : ''
    }
  }

  // Card selection handling
  const handleCardSelection = () => {
    if (paymentForm.value.selectedCard && paymentForm.value.card > 0) {
      // Recalculate card amount with surcharge
      const baseAmount = paymentForm.value.card
      const percentage = paymentForm.value.selectedCard.montoPorcentual || 0
      const surcharge = (baseAmount * percentage) / 100
      
      // Update the card amount to include surcharge for display
      // But keep the original amount for calculation
    }
  }

  // Auto-fill payment methods
  const autoFillCash = () => {
    paymentForm.value.cash = totalWithAdjustments.value
    paymentForm.value.card = 0
    paymentForm.value.selectedCard = null
  }

  const autoFillCard = () => {
    if (!paymentCards.value.length) {
      showError('No hay tarjetas disponibles')
      return
    }
    
    paymentForm.value.cash = 0
    paymentForm.value.card = totalWithAdjustments.value
    
    // Select the first available card if none selected
    if (!paymentForm.value.selectedCard && paymentCards.value.length > 0) {
      paymentForm.value.selectedCard = paymentCards.value[0]
    }
  }

  const splitPayment = () => {
    const total = totalWithAdjustments.value
    paymentForm.value.cash = Math.round(total / 2)
    paymentForm.value.card = total - paymentForm.value.cash
    
    // Select the first available card if none selected
    if (!paymentForm.value.selectedCard && paymentCards.value.length > 0) {
      paymentForm.value.selectedCard = paymentCards.value[0]
    }
  }

  // Utility methods
  const formatCurrency = (amount) => {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP',
      minimumFractionDigits: 0
    }).format(amount)
  }

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString('es-CO', {
      year: 'numeric',
      month: 'short',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    })
  }

  const getPawnStatus = (pawn) => {
    const daysOverdue = calculateDaysOverdue(pawn.fechaRegistro)
    if (daysOverdue > 30) return 'overdue'
    if (daysOverdue > 15) return 'warning'
    return 'active'
  }

  const calculateDaysOverdue = (dateString) => {
    const pawnDate = new Date(dateString)
    const now = new Date()
    const diffTime = now - pawnDate
    return Math.floor(diffTime / (1000 * 60 * 60 * 24))
  }

  const getStatusColor = (status) => {
    switch (status) {
      case 'overdue': return 'text-red-400'
      case 'warning': return 'text-yellow-400'
      case 'active': return 'text-green-400'
      default: return 'text-white'
    }
  }

  const getStatusIcon = (status) => {
    switch (status) {
      case 'overdue': return 'pi-exclamation-triangle'
      case 'warning': return 'pi-clock'
      case 'active': return 'pi-check-circle'
      default: return 'pi-circle'
    }
  }

  // Toast messages
  const showSuccess = (message) => {
    toast.add({
      severity: 'success',
      summary: 'Éxito',
      detail: message,
      life: 5000
    })
  }

  const showError = (message) => {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: message,
      life: 5000
    })
  }

  const showWarning = (message) => {
    toast.add({
      severity: 'warn',
      summary: 'Advertencia',
      detail: message,
      life: 5000
    })
  }

  // Confirmation dialogs
  const confirmPayment = () => {
    return new Promise((resolve) => {
      confirm.require({
        message: `¿Confirmar el pago de ${formatCurrency(totalWithAdjustments.value)} para el empeño ${selectedPawn.value?.empeñoID}?`,
        header: 'Confirmar Pago',
        icon: 'pi pi-question-circle',
        acceptLabel: 'Sí, confirmar',
        rejectLabel: 'Cancelar',
        acceptClass: 'p-button-success',
        accept: () => resolve(true),
        reject: () => resolve(false)
      })
    })
  }

  return {
    // Data state
    pawns,
    paymentCards,
    
    // UI state
    isLoading,
    showPaymentModal,
    selectedPawn,
    
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
    formatCurrency,
    formatDate,
    getPawnStatus,
    calculateDaysOverdue,
    getStatusColor,
    getStatusIcon,
    showSuccess,
    showError,
    showWarning,
    confirmPayment
  }
}