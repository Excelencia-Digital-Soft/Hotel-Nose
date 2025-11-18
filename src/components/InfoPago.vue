<template>
  <Teleport to="body">
    <Transition name="modal-outer" appear>
      <div class="fixed inset-0 bg-black/60 backdrop-blur-xl flex justify-center items-center z-[70] p-4">
        <Transition name="modal-inner">
          <div class="relative glass-container max-w-4xl w-full max-h-[90vh] overflow-y-auto">
            <!-- Header -->
            <div class="sticky top-0 glass-card p-6 mb-6 border-b border-white/10">
              <div class="flex items-center justify-between">
                <div class="flex items-center">
                  <div class="bg-gradient-to-r from-blue-400 to-purple-400 p-3 rounded-full mr-4">
                    <i class="pi pi-receipt text-white text-2xl"></i>
                  </div>
                  <div>
                    <h2 class="text-2xl font-bold text-white mb-1">📋 Detalle de Pago</h2>
                    <p class="text-gray-300">
                      {{ pago.pagoId === 0 ? '❌ Pago Anulado' : `Pago #${pago.pagoId}` }} | 
                      {{ formatFechaHora(pago.fecha) }}
                    </p>
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

            <!-- Content -->
            <div class="p-6 space-y-6">
              
              <!-- Información de Habitación -->
              <div class="glass-card p-6">
                <div class="flex items-center mb-4">
                  <i class="pi pi-home text-blue-400 text-xl mr-3"></i>
                  <h3 class="text-xl font-bold text-white">🏨 Información de Habitación</h3>
                </div>
                
                <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                  <div class="glass-card p-4">
                    <p class="text-gray-300 text-sm mb-1">Tipo de Habitación</p>
                    <p class="text-white font-semibold">
                      {{ pago.tipoHabitacion || 'N/A' }}
                    </p>
                  </div>
                  
                  <div class="glass-card p-4">
                    <p class="text-gray-300 text-sm mb-1">🚪 Hora Ingreso</p>
                    <p class="text-green-400 font-semibold">
                      {{ formatFechaHora(pago.horaIngreso) || '-' }}
                    </p>
                  </div>
                  
                  <div class="glass-card p-4">
                    <p class="text-gray-300 text-sm mb-1">🏃 Hora Salida</p>
                    <p class="text-red-400 font-semibold">
                      {{ formatFechaHora(pago.horaSalida) || '-' }}
                    </p>
                  </div>
                  
                  <div class="glass-card p-4">
                    <p class="text-gray-300 text-sm mb-1">⏱️ Tiempo Total</p>
                    <p class="text-purple-400 font-semibold">
                      {{ calculateDuration(pago.horaIngreso, pago.horaSalida) }}
                    </p>
                  </div>
                </div>
              </div>

              <!-- Resumen de Cobro -->
              <div class="glass-card p-6">
                <div class="flex items-center mb-4">
                  <i class="pi pi-calculator text-green-400 text-xl mr-3"></i>
                  <h3 class="text-xl font-bold text-white">💰 Resumen de Cobro</h3>
                </div>
                
                <div class="grid grid-cols-2 gap-6">
                  <!-- Left Column - Charges -->
                  <div class="space-y-4">
                    <div class="flex justify-between items-center py-2 border-b border-white/10">
                      <span class="text-gray-300 font-medium">Periodo:</span>
                      <span class="text-green-400 font-bold">${{ (pago.periodo || 0).toFixed(2) }}</span>
                    </div>
                    
                    <div class="flex justify-between items-center py-2 border-b border-white/10">
                      <span class="text-gray-300 font-medium">Adicional:</span>
                      <span class="text-green-400 font-bold">${{ (pago.montoAdicional || 0).toFixed(2) }}</span>
                    </div>
                    
                    <div class="flex justify-between items-center py-2 border-b border-white/10">
                      <span class="text-gray-300 font-medium">Consumos:</span>
                      <span class="text-green-400 font-bold">${{ (pago.totalConsumo || 0).toFixed(2) }}</span>
                    </div>
                  </div>

                  <!-- Right Column - Payments -->
                  <div class="space-y-4">
                    <div v-if="pago.montoDescuento > 0" class="flex justify-between items-center py-2 border-b border-white/10">
                      <span class="text-gray-300 font-medium">Descuento:</span>
                      <span class="text-red-400 font-bold">-${{ (pago.montoDescuento || 0).toFixed(2) }}</span>
                    </div>
                    
                    <div class="flex justify-between items-center py-3 bg-white/5 rounded-lg px-3">
                      <span class="text-white font-bold text-lg">Subtotal:</span>
                      <span class="text-blue-400 font-bold text-xl">
                        ${{ calculateSubtotal().toFixed(2) }}
                      </span>
                    </div>
                    
                    <div class="flex justify-between items-center py-3 bg-gradient-to-r from-green-500/20 to-blue-500/20 rounded-lg px-3">
                      <span class="text-white font-bold text-lg">💎 Total Pagado:</span>
                      <span class="text-white font-bold text-2xl">
                        ${{ calculateTotalPagado().toFixed(2) }}
                      </span>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Forma de Pago -->
              <div class="glass-card p-6">
                <div class="flex items-center mb-4">
                  <i class="pi pi-wallet text-purple-400 text-xl mr-3"></i>
                  <h3 class="text-xl font-bold text-white">💳 Forma de Pago</h3>
                </div>
                
                <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <!-- Efectivo -->
                  <div class="glass-card p-4">
                    <div class="flex items-center justify-between">
                      <div class="flex items-center">
                        <i class="pi pi-money-bill text-green-400 mr-3"></i>
                        <div>
                          <p class="text-white font-semibold">Efectivo</p>
                          <p class="text-gray-300 text-sm">Pago en efectivo</p>
                        </div>
                      </div>
                      <div class="text-right">
                        <p class="text-green-400 font-bold text-lg">
                          ${{ (pago.montoEfectivo || 0).toFixed(2) }}
                        </p>
                      </div>
                    </div>
                  </div>
                  
                  <!-- Tarjeta -->
                  <div class="glass-card p-4">
                    <div class="flex items-center justify-between">
                      <div class="flex items-center">
                        <i class="pi pi-credit-card text-blue-400 mr-3"></i>
                        <div>
                          <p class="text-white font-semibold">Tarjeta</p>
                          <p class="text-gray-300 text-sm">
                            {{ pago.tarjetaNombre || 'Sin especificar' }}
                          </p>
                        </div>
                      </div>
                      <div class="text-right">
                        <p class="text-blue-400 font-bold text-lg">
                          ${{ (pago.montoTarjeta || 0).toFixed(2) }}
                        </p>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Consumos Detallados -->
              <div class="glass-card p-6">
                <div class="flex items-center mb-4">
                  <i class="pi pi-shopping-cart text-orange-400 text-xl mr-3"></i>
                  <h3 class="text-xl font-bold text-white">🍽️ Consumos Detallados</h3>
                </div>
                
                <!-- Loading State -->
                <div v-if="loadingConsumos" class="text-center py-8">
                  <i class="pi pi-spinner pi-spin text-white text-2xl mb-4"></i>
                  <p class="text-gray-300">Cargando consumos...</p>
                </div>
                
                <!-- Consumos List -->
                <div v-else-if="consumos.length > 0" class="space-y-3">
                  <div 
                    v-for="(consumo, index) in consumos" 
                    :key="index"
                    class="flex items-center justify-between p-4 glass-card hover:bg-white/10 transition-all"
                  >
                    <div class="flex items-center">
                      <div class="bg-orange-500/20 p-2 rounded-full mr-3">
                        <i class="pi pi-shopping-bag text-orange-400"></i>
                      </div>
                      <div>
                        <p class="text-white font-semibold">{{ consumo.nombre }}</p>
                        <p class="text-gray-300 text-sm">
                          Cantidad: {{ consumo.cantidad }} | 
                          Precio unitario: ${{ consumo.precioUnitario.toFixed(2) }}
                        </p>
                      </div>
                    </div>
                    <div class="text-right">
                      <p class="text-orange-400 font-bold">
                        ${{ (consumo.cantidad * consumo.precioUnitario).toFixed(2) }}
                      </p>
                    </div>
                  </div>
                  
                  <!-- Total Consumos -->
                  <div class="border-t border-white/20 pt-3 mt-3">
                    <div class="flex justify-between items-center p-3 bg-orange-500/20 rounded-lg">
                      <span class="text-white font-bold">Total Consumos:</span>
                      <span class="text-orange-400 font-bold text-lg">
                        ${{ (pago.totalConsumo || 0).toFixed(2) }}
                      </span>
                    </div>
                  </div>
                </div>
                
                <!-- No Consumos -->
                <div v-else class="text-center py-8">
                  <i class="pi pi-info-circle text-gray-400 text-2xl mb-4"></i>
                  <p class="text-gray-400">No hay consumos registrados para este pago</p>
                </div>
              </div>

              <!-- Observaciones -->
              <div v-if="pago.observacion" class="glass-card p-6">
                <div class="flex items-center mb-4">
                  <i class="pi pi-comment text-yellow-400 text-xl mr-3"></i>
                  <h3 class="text-xl font-bold text-white">📝 Observaciones</h3>
                </div>
                
                <div class="glass-card p-4">
                  <p class="text-gray-300 leading-relaxed">
                    {{ pago.observacion }}
                  </p>
                </div>
              </div>

            </div>

            <!-- Footer Actions -->
            <div class="glass-card p-6 border-t border-white/10">
              <div class="flex justify-end space-x-3">
                <button 
                  @click="imprimirDetalle"
                  class="glass-button px-6 py-3 text-white hover:bg-white/20 transition-all flex items-center"
                >
                  <i class="pi pi-print mr-2"></i>
                  Imprimir Detalle
                </button>
                
                <button 
                  @click="emit('close')"
                  class="bg-gradient-to-r from-blue-400 to-blue-500 hover:from-blue-500 hover:to-blue-600 px-6 py-3 text-white font-semibold rounded-lg transition-all flex items-center"
                >
                  <i class="pi pi-check mr-2"></i>
                  Cerrar
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
import { ref, onMounted, computed } from 'vue'
import axiosClient from '../axiosClient'

// Props
const props = defineProps({
  pago: {
    type: Object,
    required: true
  }
})

// Emits
const emit = defineEmits(['close'])

// State
const consumos = ref([])
const loadingConsumos = ref(false)

// Computed
const calculateSubtotal = () => {
  const periodo = props.pago.periodo || 0
  const adicional = props.pago.montoAdicional || 0
  const consumo = props.pago.totalConsumo || 0
  const descuento = props.pago.montoDescuento || 0
  return periodo + adicional + consumo - descuento
}

const calculateTotalPagado = () => {
  const efectivo = props.pago.montoEfectivo || 0
  const tarjeta = props.pago.montoTarjeta || 0
  return efectivo + tarjeta
}

// Methods
const formatFechaHora = (fechaHora) => {
  if (!fechaHora) return '-'
  const date = new Date(fechaHora)
  return date.toLocaleString('es-ES', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const calculateDuration = (inicio, fin) => {
  if (!inicio || !fin) return '-'
  
  const inicioDate = new Date(inicio)
  const finDate = new Date(fin)
  const diffMs = finDate - inicioDate
  
  const hours = Math.floor(diffMs / (1000 * 60 * 60))
  const minutes = Math.floor((diffMs % (1000 * 60 * 60)) / (1000 * 60))
  
  if (hours >= 24) {
    const days = Math.floor(hours / 24)
    const remainingHours = hours % 24
    return `${days}d ${remainingHours}h ${minutes}m`
  }
  
  return `${hours}h ${minutes}m`
}

const fetchConsumos = async () => {
  if (!props.pago.pagoId || props.pago.pagoId === 0) return
  
  loadingConsumos.value = true
  try {
    // Intentar con API V1 primero, luego fallback a legacy
    let response
    try {
      response = await axiosClient.get(`/api/v1/consumos/pago/${props.pago.pagoId}`)
    } catch (v1Error) {
      // Fallback to legacy API
      response = await axiosClient.get(`/GetConsumosPago?pagoId=${props.pago.pagoId}`)
    }
    
    if (response.data && response.data.isSuccess) {
      consumos.value = response.data.data || []
    }
  } catch (error) {
    console.error('Error al obtener consumos del pago:', error)
    // No mostrar error al usuario, solo log
  } finally {
    loadingConsumos.value = false
  }
}

const imprimirDetalle = () => {
  window.print()
}

// Lifecycle
onMounted(() => {
  fetchConsumos()
})
</script>

<style scoped>
/* Glassmorphism components */
.glass-container {
  background-color: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(32px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 24px;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
}

.glass-card {
  background-color: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(16px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 12px;
}

.glass-button {
  background-color: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(8px);
  border: 1px solid rgba(255, 255, 255, 0.3);
  border-radius: 8px;
  transition: all 0.3s ease;
}

/* Modal animations */
.modal-outer-enter-active,
.modal-outer-leave-active {
  transition: opacity 0.3s ease;
}

.modal-outer-enter-from,
.modal-outer-leave-to {
  opacity: 0;
}

.modal-inner-enter-active,
.modal-inner-leave-active {
  transition: all 0.3s ease;
}

.modal-inner-enter-from,
.modal-inner-leave-to {
  opacity: 0;
  transform: scale(0.9);
}

/* Print styles */
@media print {
  * {
    -webkit-print-color-adjust: exact !important;
    print-color-adjust: exact !important;
  }

  body {
    margin: 0 !important;
    padding: 0 !important;
  }

  .glass-container {
    background: white !important;
    color: black !important;
    border: none !important;
    border-radius: 0 !important;
    box-shadow: none !important;
    backdrop-filter: none !important;
  }

  .glass-card {
    background: #f8f9fa !important;
    border: 1px solid #dee2e6 !important;
    color: black !important;
    backdrop-filter: none !important;
  }

  button {
    display: none !important;
  }

  .text-white { color: black !important; }
  .text-gray-300, .text-gray-400 { color: #6c757d !important; }
  .text-green-400 { color: #198754 !important; }
  .text-red-400 { color: #dc3545 !important; }
  .text-blue-400 { color: #0d6efd !important; }
  .text-purple-400 { color: #6f42c1 !important; }
  .text-orange-400 { color: #fd7e14 !important; }
  .text-yellow-400 { color: #ffc107 !important; }
}
</style>