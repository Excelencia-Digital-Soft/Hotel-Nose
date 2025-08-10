import { ref, computed, provide, inject, watch } from 'vue'
import { useToast } from 'primevue/usetoast'
import { useAuthStore } from '../store/auth.js'
import axiosClient from '../axiosClient'

/**
 * Payment Provider Composable
 * Manages payment data and eliminates prop drilling using provide/inject pattern
 * 
 * Features:
 * - Centralized payment state management
 * - Automatic calculations
 * - Card and payment method management
 * - Pawn and surcharge handling
 * - API integration for payment processing
 */

// Injection keys
export const PAYMENT_PROVIDER_KEY = Symbol('paymentProvider')

/**
 * Payment Provider - Use in parent component
 */
export function usePaymentProvider(initialData = {}) {
  const toast = useToast()
  const authStore = useAuthStore()

  // Core payment data
  const paymentData = ref({
    // Basic amounts
    periodo: initialData.periodo || 0,
    consumo: initialData.consumo || 0,
    adicional: initialData.adicional || 0,
    total: initialData.total || 0,
    
    // Payment details
    visitaId: initialData.visitaId || null,
    habitacionId: initialData.habitacionId || null,
    pausa: initialData.pausa || false,
    
    // User inputs
    descuento: 0,
    efectivo: 0,
    tarjeta: 0,
    comentario: '',
    
    // Additional charges
    empenoMonto: 0,
    empenoDetalle: '',
    recargoMonto: 0,
    recargoDetalle: '',
    
    // Card processing
    selectedTarjeta: null,
    extraTarjeta: 0,
    porcentajeRecargo: 0,
    
    // UI state
    isProcessing: false,
    showEmpenoModal: false,
    showRecargoModal: false,
    
    // Maintenance flag
    enviarAMantenimiento: false
  })

  // Available payment methods
  const tarjetas = ref([])
  const loading = ref(false)
  const error = ref(null)

  // Computed values
  const calculatedTotal = computed(() => {
    const data = paymentData.value
    return data.periodo + data.consumo + data.adicional + data.extraTarjeta + data.recargoMonto
  })

  const faltaPorPagar = computed(() => {
    const data = paymentData.value
    const descuentoValue = data.descuento || 0
    const efectivoValue = data.efectivo || 0
    const tarjetaValue = data.tarjeta || 0
    const empenoMontoValue = data.empenoMonto || 0

    // Calculate total to pay (consistent with calculatedTotal)
    const totalToPay = data.periodo + data.consumo + data.adicional + data.extraTarjeta + data.recargoMonto
    
    // Calculate total paid
    const totalPaid = descuentoValue + efectivoValue + tarjetaValue + empenoMontoValue

    return totalToPay - totalPaid
  })

  const isPaymentValid = computed(() => {
    const data = paymentData.value
    return faltaPorPagar.value === 0 && !(data.descuento > 0 && !data.comentario.trim())
  })

  const paymentSummary = computed(() => ({
    subtotal: paymentData.value.periodo + paymentData.value.consumo + paymentData.value.adicional,
    descuento: paymentData.value.descuento,
    recargo: paymentData.value.recargoMonto,
    empeno: paymentData.value.empenoMonto,
    total: calculatedTotal.value,
    faltaPorPagar: faltaPorPagar.value,
    pagado: calculatedTotal.value - faltaPorPagar.value
  }))

  // Methods
  const updatePaymentData = (updates) => {
    Object.assign(paymentData.value, updates)
  }

  const resetPaymentData = () => {
    const currentBasics = {
      periodo: paymentData.value.periodo,
      consumo: paymentData.value.consumo,
      adicional: paymentData.value.adicional,
      visitaId: paymentData.value.visitaId,
      habitacionId: paymentData.value.habitacionId,
      pausa: paymentData.value.pausa
    }
    
    paymentData.value = {
      ...currentBasics,
      descuento: 0,
      efectivo: 0,
      tarjeta: 0,
      comentario: '',
      empenoMonto: 0,
      empenoDetalle: '',
      recargoMonto: 0,
      recargoDetalle: '',
      selectedTarjeta: null,
      extraTarjeta: 0,
      porcentajeRecargo: 0,
      isProcessing: false,
      showEmpenoModal: false,
      showRecargoModal: false,
      enviarAMantenimiento: false
    }
  }

  const fetchTarjetas = async () => {
    try {
      loading.value = true
      error.value = null
      
      const institucionID = authStore.institucionID
      if (!institucionID) {
        throw new Error('No se encontró la institución')
      }

      const response = await axiosClient.get(`/GetTarjetas?InstitucionID=${institucionID}`)
      
      if (response.data && response.data.data) {
        tarjetas.value = response.data.data
      } else {
        tarjetas.value = []
      }
    } catch (err) {
      error.value = err.message
      console.error('Error fetching tarjetas:', err)
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Error al cargar tarjetas de pago',
        life: 5000
      })
    } finally {
      loading.value = false
    }
  }

  const updateRecargo = () => {
    const data = paymentData.value
    
    if (data.selectedTarjeta && data.selectedTarjeta.montoPorcentual > 0) {
      // Calculate the amount available to pay with card (remaining amount)
      const baseAmounts = data.periodo + data.consumo + data.adicional + data.recargoMonto
      const currentPayments = data.descuento + data.efectivo + data.empenoMonto
      const availableForCard = Math.max(0, baseAmounts - currentPayments)
      
      data.porcentajeRecargo = data.selectedTarjeta.montoPorcentual
      
      // Calculate card surcharge based on the available amount
      const surchargeRate = data.selectedTarjeta.montoPorcentual / 100
      data.extraTarjeta = Number((availableForCard * surchargeRate).toFixed(2))
      data.tarjeta = Number((availableForCard * (1 + surchargeRate)).toFixed(2))
    } else {
      data.tarjeta = 0
      data.extraTarjeta = 0
      data.porcentajeRecargo = 0
    }
  }

  const confirmarEmpeno = ({ monto, detalle }) => {
    const data = paymentData.value
    data.empenoMonto = monto
    data.empenoDetalle = detalle
    data.comentario = `Empeño de ${detalle} por un valor de $${monto.toFixed(2)}. ` + data.comentario
    data.showEmpenoModal = false
    
    toast.add({
      severity: 'success',
      summary: 'Empeño Agregado',
      detail: `Empeño de $${monto.toFixed(2)} agregado correctamente`,
      life: 3000
    })
  }

  const confirmarRecargo = (recargo) => {
    const data = paymentData.value
    data.recargoMonto = recargo.monto
    data.recargoDetalle = recargo.detalle
    data.comentario += `Recargo por ${recargo.detalle} con un valor de $${recargo.monto.toFixed(2)}. `
    data.showRecargoModal = false
    
    toast.add({
      severity: 'success',
      summary: 'Recargo Agregado',
      detail: `Recargo de $${recargo.monto.toFixed(2)} agregado correctamente`,
      life: 3000
    })
  }

  const crearMovimientoAdicional = async () => {
    const data = paymentData.value
    
    if (data.descuento > 0 && !data.comentario.trim()) {
      toast.add({
        severity: 'warn',
        summary: 'Comentario Requerido',
        detail: 'El comentario es obligatorio cuando se aplica un descuento',
        life: 5000
      })
      return false
    }

    if (data.isProcessing) return false

    try {
      data.isProcessing = true

      // Create pawn if exists
      if (data.empenoMonto > 0) {
        const institucionID = authStore.institucionID
        await axiosClient.post(
          `api/Empeño/AddEmpeno?institucionID=${institucionID}&visitaID=${data.visitaId}&detalle=${data.empenoDetalle}&monto=${data.empenoMonto}`
        )
      }

      // Create additional movement
      await axiosClient.post(
        `/MovimientoHabitacion?totalFacturado=${data.adicional}&habitacionId=${data.habitacionId}&visitaId=${data.visitaId}&comentario=${encodeURIComponent(data.comentario)}`
      )

      return true
    } catch (err) {
      error.value = err.message
      console.error('Error al crear movimiento adicional:', err)
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Error al procesar el movimiento adicional',
        life: 5000
      })
      return false
    } finally {
      data.isProcessing = false
    }
  }

  const pagarVisita = async () => {
    const data = paymentData.value
    
    try {
      data.isProcessing = true

      // Helper function to safely convert to number
      const safeNumber = (value) => {
        if (value === '' || value === null || value === undefined) return 0
        const num = Number(value)
        return isNaN(num) ? 0 : num
      }

      // Prepare payment data
      const paymentInfo = {
        visitaId: data.visitaId,
        montoDescuento: safeNumber(data.descuento),
        montoEfectivo: safeNumber(data.efectivo),
        montoTarjeta: safeNumber(data.tarjeta),
        montoBillVirt: 0,
        adicional: safeNumber(data.adicional),
        medioPagoId: 1,
        comentario: encodeURIComponent(data.comentario || ''),
        montoRecargo: safeNumber(data.recargoMonto),
        descripcionRecargo: encodeURIComponent(data.recargoDetalle || ''),
      }
      
      const tarjetaSeleccionada = data.selectedTarjeta?.tarjetaID || 0
      
      // Build URL with or without surcharge
      const baseParams = `visitaId=${paymentInfo.visitaId}&montoDescuento=${paymentInfo.montoDescuento}&montoEfectivo=${paymentInfo.montoEfectivo}&montoTarjeta=${paymentInfo.montoTarjeta}&montoBillVirt=${paymentInfo.montoBillVirt}&adicional=${paymentInfo.adicional}&medioPagoId=${paymentInfo.medioPagoId}&comentario=${paymentInfo.comentario}&tarjetaID=${tarjetaSeleccionada}`
      
      const url = paymentInfo.montoRecargo > 0
        ? `/api/Pago/PagarVisita?${baseParams}&montoRecargo=${paymentInfo.montoRecargo}&descripcionRecargo=${paymentInfo.descripcionRecargo}`
        : `/api/Pago/PagarVisita?${baseParams}`

      await axiosClient.post(url)
      
      toast.add({
        severity: 'success',
        summary: 'Pago Procesado',
        detail: 'El pago se ha procesado correctamente',
        life: 3000
      })
      
      return true
    } catch (err) {
      error.value = err.message
      console.error('Error al realizar el pago:', err)
      toast.add({
        severity: 'error',
        summary: 'Error en el Pago',
        detail: 'Error al procesar el pago',
        life: 5000
      })
      return false
    } finally {
      data.isProcessing = false
    }
  }

  const finalizarReserva = async () => {
    const data = paymentData.value
    
    try {
      // First, finalize the reservation
      await axiosClient.put(`/FinalizarReserva?idHabitacion=${data.habitacionId}`)
      
      // If maintenance is requested, set the room to maintenance mode
      if (data.enviarAMantenimiento) {
        await enviarHabitacionAMantenimiento()
      }
      
      toast.add({
        severity: 'success',
        summary: 'Reserva Finalizada',
        detail: data.enviarAMantenimiento 
          ? 'La reserva se ha finalizado correctamente y la habitación fue enviada a mantenimiento'
          : 'La reserva se ha finalizado correctamente',
        life: 4000
      })
      
      return true
    } catch (err) {
      error.value = err.message
      console.error('Error al finalizar reserva:', err)
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Error al finalizar la reserva',
        life: 5000
      })
      return false
    }
  }

  const enviarHabitacionAMantenimiento = async () => {
    const data = paymentData.value
    
    try {
      // Try V1 API first (if it exists)
      try {
        await axiosClient.put(`/api/v1/habitaciones/${data.habitacionId}/maintenance`, {
          maintenanceType: 'mantenimiento',
          description: 'Habitación marcada para mantenimiento durante checkout',
          startedBy: authStore.user?.username || 'Usuario'
        })
        console.log('✅ Room set to maintenance using V1 API')
        return true
      } catch (v1Error) {
        console.log('V1 API not available, trying legacy approach...')
      }
      
      // Fallback: Try generic room state endpoint
      try {
        await axiosClient.put(`/SetEstadoHabitacion?habitacionId=${data.habitacionId}&estado=mantenimiento`)
        console.log('✅ Room set to maintenance using legacy API')
        return true
      } catch (legacyError) {
        console.log('Legacy API not available, trying alternative...')
      }
      
      // Alternative: Use room update endpoint if available
      await axiosClient.put(`/UpdateRoomStatus`, {
        habitacionId: data.habitacionId,
        estado: 'mantenimiento',
        descripcion: 'Habitación enviada a mantenimiento después del checkout'
      })
      
      console.log('✅ Room set to maintenance using alternative API')
      return true
      
    } catch (err) {
      console.error('❌ Error setting room to maintenance:', err)
      
      // Show user a warning but don't fail the entire checkout process
      toast.add({
        severity: 'warn',
        summary: 'Advertencia',
        detail: 'La habitación fue liberada pero no se pudo marcar para mantenimiento automáticamente. Por favor, márquela manualmente.',
        life: 8000
      })
      
      // Don't throw error - the payment was successful, just the maintenance flag failed
      return false
    }
  }

  const pausarTimer = async () => {
    const data = paymentData.value
    
    try {
      await axiosClient.put(`/PausarOcupacion?visitaId=${data.visitaId}`)
      
      toast.add({
        severity: 'info',
        summary: 'Timer Pausado',
        detail: 'El timer de ocupación ha sido pausado',
        life: 3000
      })
      
      return true
    } catch (err) {
      error.value = err.message
      console.error('Error al pausar timer:', err)
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Error al pausar el timer',
        life: 5000
      })
      return false
    }
  }

  const recalcularTimer = async () => {
    const data = paymentData.value
    
    try {
      await axiosClient.put(`/RecalcularOcupacion?visitaId=${data.visitaId}`)
      
      toast.add({
        severity: 'info',
        summary: 'Timer Recalculado',
        detail: 'El timer de ocupación ha sido recalculado',
        life: 3000
      })
      
      return true
    } catch (err) {
      error.value = err.message
      console.error('Error al recalcular timer:', err)
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Error al recalcular el timer',
        life: 5000
      })
      return false
    }
  }

  // Watchers
  watch(() => paymentData.value.selectedTarjeta, updateRecargo, { immediate: true })
  watch(() => [paymentData.value.efectivo, paymentData.value.descuento, paymentData.value.empenoMonto], updateRecargo, { deep: true })
  
  // Auto-fetch tarjetas on creation
  fetchTarjetas()

  // Provider object
  const provider = {
    // State
    paymentData,
    tarjetas,
    loading,
    error,
    
    // Computed
    calculatedTotal,
    faltaPorPagar,
    isPaymentValid,
    paymentSummary,
    
    // Methods
    updatePaymentData,
    resetPaymentData,
    fetchTarjetas,
    updateRecargo,
    confirmarEmpeno,
    confirmarRecargo,
    crearMovimientoAdicional,
    pagarVisita,
    finalizarReserva,
    enviarHabitacionAMantenimiento,
    pausarTimer,
    recalcularTimer
  }

  // Provide the payment data to children
  provide(PAYMENT_PROVIDER_KEY, provider)
  
  return provider
}

/**
 * Payment Consumer - Use in child components
 */
export function usePaymentConsumer() {
  const paymentProvider = inject(PAYMENT_PROVIDER_KEY)
  
  if (!paymentProvider) {
    throw new Error('usePaymentConsumer must be used within a PaymentProvider')
  }
  
  return paymentProvider
}

/**
 * Standalone Payment Hook - For components that need payment logic without provider
 */
export function usePayment(initialData = {}) {
  const provider = usePaymentProvider(initialData)
  
  // Return the provider methods for standalone use
  return provider
}