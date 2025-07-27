<template>
  <!-- Modal container with glassmorphism -->
  <div class="fixed inset-0 z-50 flex items-center justify-center p-4">
    <!-- Backdrop with blur -->
    <div class="absolute inset-0 bg-black/60 backdrop-blur-xl" @click="closeModal"></div>
    
    <!-- Modal content with animations -->
    <div 
      class="relative w-full max-w-7xl h-[90vh] glass-container p-0 transform transition-all duration-500 scale-100 opacity-100 flex flex-col"
      :class="{ 'scale-95 opacity-0': isClosing }"
      ref="prueba" 
      id="testCierre"
    >
      <!-- Header Section -->
      <div class="glass-header p-6 border-b border-white/20 flex-shrink-0">
        <div class="flex items-center justify-between">
          <div class="flex items-center space-x-4">
            <div class="bg-gradient-to-r from-primary-400 to-accent-400 p-3 rounded-full">
              <i class="pi pi-money-bill text-white text-2xl"></i>
            </div>
            <div>
              <h2 class="text-3xl font-bold text-white">💰 Lista de Pagos</h2>
              <p class="text-gray-300 text-sm mt-1">
                {{ esAbierto ? '🟢 Sesión Actual' : `📚 Cierre #${idcierre}` }}
              </p>
            </div>
          </div>
          
          <!-- Close button -->
          <button 
            @click="closeModal"
            class="glass-button p-3 hover:bg-white/20 transform hover:scale-110 transition-all group"
          >
            <i class="pi pi-times text-white text-xl group-hover:text-red-400"></i>
          </button>
        </div>
      </div>

      <!-- Content Section with scroll -->
      <div class="overflow-y-auto flex-1 p-6">
        <!-- Print Header (only visible when printing) -->
        <div class="print-header hidden">
          <h1>📊 Lista de Pagos - {{ esAbierto ? 'Sesión Actual' : `Cierre #${idcierre}` }}</h1>
          <p>Fecha de impresión: {{ getCurrentDateTime() }}</p>
        </div>
        
        <!-- Summary Cards -->
        <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
          <div class="glass-card p-4 transform hover:scale-105 transition-all">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-gray-300 text-sm">💵 Efectivo Total</p>
                <p class="text-2xl font-bold text-green-400">{{ calculateEfectivo().toFixed(2) }}</p>
              </div>
              <i class="pi pi-wallet text-green-400 text-2xl"></i>
            </div>
          </div>
          
          <div class="glass-card p-4 transform hover:scale-105 transition-all">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-gray-300 text-sm">💳 Tarjeta Total</p>
                <p class="text-2xl font-bold text-blue-400">{{ calculateTarjeta().toFixed(2) }}</p>
              </div>
              <i class="pi pi-credit-card text-blue-400 text-2xl"></i>
            </div>
          </div>
          
          <div class="glass-card p-4 transform hover:scale-105 transition-all">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-gray-300 text-sm">🎯 Descuentos</p>
                <p class="text-2xl font-bold text-red-400">{{ calculateDescuento().toFixed(2) }}</p>
              </div>
              <i class="pi pi-percentage text-red-400 text-2xl"></i>
            </div>
          </div>
          
          <div class="glass-card p-4 transform hover:scale-105 transition-all">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-gray-300 text-sm">💎 Total General</p>
                <p class="text-2xl font-bold text-white">{{ calculateTotal().toFixed(2) }}</p>
              </div>
              <i class="pi pi-chart-line text-white text-2xl"></i>
            </div>
          </div>
        </div>

        <!-- Main Table with glassmorphism -->
        <div class="glass-card overflow-hidden" ref="cierreCajaRef" id="cierre-caja-content">
          <!-- Table wrapper for horizontal scroll on mobile -->
          <div class="overflow-x-auto">
            <table class="w-full min-w-[1400px]">
              <!-- Headers -->
              <thead>
                <tr class="bg-white/10 backdrop-blur-sm">
                  <th class="glass-table-header text-left" colspan="2">🏨 Tipo Habitación</th>
                  <th class="glass-table-header">💰 Pago</th>
                  <th class="glass-table-header text-right">⏱️ Periodo</th>
                  <th class="glass-table-header text-right">➕ Adicional</th>
                  <th class="glass-table-header text-right">🍽️ Consumo</th>
                  <th class="glass-table-header">🚪 Ingreso</th>
                  <th class="glass-table-header">🏃 Salida</th>
                  <th class="glass-table-header text-right">💵 Efectivo</th>
                  <th class="glass-table-header text-right">💳 Tarjeta</th>
                  <th class="glass-table-header text-right">🏦 Tarjeta Usada</th>
                  <th class="glass-table-header text-right">🎯 Descuento</th>
                  <th class="glass-table-header text-right">💎 Total</th>
                  <th class="glass-table-header">📝 Observación</th>
                </tr>
              </thead>
              
              <!-- Body -->
              <tbody>
                <!-- Pagos -->
                <template v-for="(pago, index) in listaPagos" :key="`pago-${pago.pagoId}`">
                  <tr 
                    v-if="pago.tipoHabitacion !== null"
                    class="glass-table-row group"
                    :class="{ 'bg-red-500/20': pago.pagoId === 0 }"
                  >
                    <td class="glass-table-cell font-semibold" colspan="2">
                      <span v-if="pago.pagoId === 0" class="text-red-400">
                        ❌ {{ pago.tipoHabitacion }} ANULADA
                      </span>
                      <span v-else class="text-white">
                        {{ pago.tipoHabitacion }}
                      </span>
                    </td>
                    <td class="glass-table-cell">
                      <button 
                        @click="openInfoModal(pago)"
                        class="text-blue-400 hover:text-blue-300 hover:underline transition-colors"
                      >
                        Pago #{{ pago.pagoId }}
                        <div class="text-gray-400 text-xs">{{ formatFechaHora(pago.fecha) }}</div>
                      </button>
                    </td>
                    <td class="glass-table-cell text-right text-green-400 font-semibold">
                      {{ pago.periodo || '-' }}
                    </td>
                    <td class="glass-table-cell text-right text-green-400 font-semibold">
                      {{ pago.montoAdicional || '-' }}
                    </td>
                    <td class="glass-table-cell text-right text-green-400 font-semibold">
                      {{ pago.totalConsumo || '-' }}
                    </td>
                    <td class="glass-table-cell text-gray-300 text-sm">
                      {{ formatFechaHora(pago.horaIngreso) }}
                    </td>
                    <td class="glass-table-cell text-gray-300 text-sm">
                      {{ formatFechaHora(pago.horaSalida) }}
                    </td>
                    <td class="glass-table-cell text-right text-green-400 font-semibold">
                      {{ pago.montoEfectivo || '-' }}
                    </td>
                    <td class="glass-table-cell text-right text-blue-400 font-semibold">
                      {{ pago.montoTarjeta || '-' }}
                    </td>
                    <td class="glass-table-cell text-right text-blue-400">
                      {{ pago.tarjetaNombre || '-' }}
                    </td>
                    <td class="glass-table-cell text-right text-red-400 font-semibold">
                      {{ pago.montoDescuento || '-' }}
                    </td>
                    <td class="glass-table-cell text-right text-white font-bold">
                      {{ (pago.montoEfectivo + pago.montoTarjeta).toFixed(2) }}
                    </td>
                    <td class="glass-table-cell text-gray-300 text-sm">
                      {{ pago.observacion || '-' }}
                    </td>
                  </tr>
                  
                  <!-- Pago Empeño -->
                  <tr 
                    v-if="pago.tipoHabitacion == null && pago.pagoId != 0"
                    class="glass-table-row group bg-purple-500/10"
                  >
                    <td class="glass-table-cell font-semibold text-purple-400" colspan="2">
                      💍 PAGO EMPEÑO
                    </td>
                    <td class="glass-table-cell">
                      <button 
                        @click="openInfoModal(pago)"
                        class="text-blue-400 hover:text-blue-300 hover:underline transition-colors"
                      >
                        Pago #{{ pago.pagoId }}
                        <div class="text-gray-400 text-xs">{{ formatFechaHora(pago.fecha) }}</div>
                      </button>
                    </td>
                    <!-- Rest of cells similar to above -->
                    <td class="glass-table-cell text-right text-green-400 font-semibold">
                      {{ pago.periodo || '-' }}
                    </td>
                    <td class="glass-table-cell text-right text-green-400 font-semibold">
                      {{ pago.montoAdicional || '-' }}
                    </td>
                    <td class="glass-table-cell text-right text-green-400 font-semibold">
                      {{ pago.totalConsumo || '-' }}
                    </td>
                    <td class="glass-table-cell text-gray-300 text-sm">
                      {{ formatFechaHora(pago.horaIngreso) }}
                    </td>
                    <td class="glass-table-cell text-gray-300 text-sm">
                      {{ formatFechaHora(pago.horaSalida) }}
                    </td>
                    <td class="glass-table-cell text-right text-green-400 font-semibold">
                      {{ pago.montoEfectivo || '-' }}
                    </td>
                    <td class="glass-table-cell text-right text-blue-400 font-semibold">
                      {{ pago.montoTarjeta || '-' }}
                    </td>
                    <td class="glass-table-cell text-right text-blue-400">
                      {{ pago.tarjetaNombre || '-' }}
                    </td>
                    <td class="glass-table-cell text-right text-red-400 font-semibold">
                      {{ pago.montoDescuento || '-' }}
                    </td>
                    <td class="glass-table-cell text-right text-white font-bold">
                      {{ (pago.montoEfectivo + pago.montoTarjeta).toFixed(2) }}
                    </td>
                    <td class="glass-table-cell text-gray-300 text-sm">
                      {{ pago.observacion || '-' }}
                    </td>
                  </tr>
                </template>
                
                <!-- Egresos -->
                <template v-for="(egreso, index) in egresos" :key="`egreso-${index}`">
                  <tr class="glass-table-row bg-red-500/10">
                    <td class="glass-table-cell font-semibold text-red-400" colspan="2">
                      💸 EGRESO
                    </td>
                    <td class="glass-table-cell">
                      <span class="text-red-400">
                        Egreso #{{ index + 1 }}
                        <div class="text-gray-400 text-xs">{{ formatFechaHora(egreso.fecha) }}</div>
                      </span>
                    </td>
                    <td class="glass-table-cell" colspan="5"></td>
                    <td class="glass-table-cell text-right text-red-400 font-semibold">
                      {{ egreso.montoEfectivo || '-' }}
                    </td>
                    <td class="glass-table-cell text-right text-red-400 font-semibold">
                      {{ egreso.montoTarjeta || '-' }}
                    </td>
                    <td class="glass-table-cell"></td>
                    <td class="glass-table-cell text-right text-red-400 font-semibold">
                      {{ egreso.montoDescuento || '-' }}
                    </td>
                    <td class="glass-table-cell text-right text-red-400 font-bold">
                      {{ (egreso.montoEfectivo + egreso.montoTarjeta).toFixed(2) }}
                    </td>
                    <td class="glass-table-cell text-gray-300 text-sm">
                      {{ egreso.observacion || '-' }}
                    </td>
                  </tr>
                </template>
              </tbody>
              
              <!-- Footer Totals -->
              <tfoot>
                <!-- First row: Categories -->
                <tr class="bg-gradient-to-r from-primary-400/20 to-accent-400/20 backdrop-blur-sm totals-row-1">
                  <td class="glass-table-footer" colspan="3">
                    <div class="space-y-1">
                      <p class="font-bold text-white text-sm">🏨 Habitaciones</p>
                      <div v-for="(item, index) in formatCategorias()" :key="index" class="text-xs text-gray-300">
                        {{ item.categoria }}: {{ item.count }}
                      </div>
                    </div>
                  </td>
                  <td class="glass-table-footer text-center">
                    <p class="text-xs text-gray-400 mb-1">Periodo</p>
                    <p class="font-bold text-green-400">{{ calculatePeriodo() }}</p>
                  </td>
                  <td class="glass-table-footer text-center">
                    <p class="text-xs text-gray-400 mb-1">Adicional</p>
                    <p class="font-bold text-green-400">{{ calculateAdicional().toFixed(2) }}</p>
                  </td>
                  <td class="glass-table-footer text-center">
                    <p class="text-xs text-gray-400 mb-1">Consumo</p>
                    <p class="font-bold text-green-400">{{ calculateConsumo().toFixed(2) }}</p>
                  </td>
                  <td class="glass-table-footer" colspan="2"></td>
                  <td class="glass-table-footer text-center">
                    <p class="text-xs text-gray-400 mb-1">Efectivo</p>
                    <p class="font-bold text-green-400">{{ calculateEfectivo().toFixed(2) }}</p>
                  </td>
                  <td class="glass-table-footer text-center">
                    <p class="text-xs text-gray-400 mb-1">Tarjeta</p>
                    <p class="font-bold text-blue-400">{{ calculateTarjeta().toFixed(2) }}</p>
                  </td>
                  <td class="glass-table-footer"></td>
                  <td class="glass-table-footer text-center">
                    <p class="text-xs text-gray-400 mb-1">Descuentos</p>
                    <p class="font-bold text-red-400">{{ calculateDescuento().toFixed(2) }}</p>
                  </td>
                  <td class="glass-table-footer text-center">
                    <p class="text-xs text-gray-400 mb-1">TOTAL GENERAL</p>
                    <p class="font-bold text-white text-lg">{{ calculateTotal().toFixed(2) }}</p>
                  </td>
                  <td class="glass-table-footer"></td>
                </tr>
              </tfoot>
            </table>
          </div>
        </div>
      </div>

      <!-- Action Buttons -->
      <div class="glass-header border-t border-white/20 flex-shrink-0">
        <div class="p-6 space-y-3">
          <!-- "Cerrar Caja" Button (only shown if esAbierto is true) -->
          <button 
            v-if="esAbierto" 
            @click="showCerrarCajaModal = true"
            class="w-full bg-gradient-to-r from-red-500 to-red-600 hover:from-red-600 hover:to-red-700 
                   text-white font-bold py-4 rounded-xl transform hover:scale-[1.02] transition-all 
                   shadow-lg hover:shadow-red-500/50 flex items-center justify-center space-x-2"
          >
            <i class="pi pi-lock text-xl"></i>
            <span class="text-lg">🔒 Cerrar Caja</span>
          </button>
          
          <!-- Print Button -->
          <button 
            @click="imprimirModal"
            class="w-full glass-button py-4 text-white font-bold hover:bg-white/20 
                   transform hover:scale-[1.02] transition-all flex items-center justify-center space-x-2"
          >
            <i class="pi pi-print text-xl"></i>
            <span class="text-lg">🖨️ Imprimir</span>
          </button>
        </div>
      </div>
    </div>
  </div>

  <!-- Modals -->
  <Transition name="modal-fade">
    <InfoPago v-if="showInfoModal" :pago="selectedPago" @close="closeInfoModal" />
  </Transition>
  
  <Transition name="modal-fade">
    <CerrarCajaModal v-if="showCerrarCajaModal" @close="closeCerrarCajaModal" @refresh="refreshPage" />
  </Transition>
</template>

<script setup>
import { ref, onMounted, onBeforeMount } from 'vue';
import InfoPago from './InfoPago.vue';
import axiosClient from '../axiosClient';
import CerrarCajaModal from './CerrarCajaModal.vue';

const props = defineProps({
  selectedPagos: Array,
  selectedEgresos: Array,
  esAbierto: Boolean,
  idcierre: Number
});

const emit = defineEmits(['close-modal', 'imprimir-modal']);

// State
const cierreCajaRef = ref(null);
const showInfoModal = ref(false);
const selectedPago = ref(null);
const showCerrarCajaModal = ref(false);
const listaPagos = ref([]);
const egresos = ref([]);
const isClosing = ref(false);

// Lifecycle
onBeforeMount(() => {
  if (props.selectedPagos.length > 0) {
    listaPagos.value = props.selectedPagos;
  }
  if (props.idcierre > 0) {
    fetchDetalleCierre();
  } else {
    egresos.value = props.selectedEgresos;
  }
});

onMounted(() => {
  if (props.selectedPagos.length > 0) {
    listaPagos.value = props.selectedPagos;
  }
  listaPagos.value = [...listaPagos.value, ...egresos.value];
});

// Methods
const closeModal = () => {
  isClosing.value = true;
  setTimeout(() => {
    emit('close-modal');
  }, 300);
};

const imprimirModal = () => {
  if (cierreCajaRef.value) {
    emit('imprimir-modal', cierreCajaRef.value);
  }
};

const fetchDetalleCierre = () => {
  axiosClient.get(`/api/Caja/GetDetalleCierre?idCierre=${props.idcierre}`)
    .then(({ data }) => {
      listaPagos.value = data.data.pagos;
      egresos.value = data.data.egresos;
    })
    .catch(error => {
      console.error('Error al obtener detalle cierres:', error);
    });
};

const openInfoModal = (pago) => {
  selectedPago.value = pago;
  showInfoModal.value = true;
};

const closeInfoModal = () => {
  showInfoModal.value = false;
  selectedPago.value = null;
};

const closeCerrarCajaModal = () => {
  showCerrarCajaModal.value = false;
};

const refreshPage = () => {
  window.location.reload();
};

const formatFechaHora = (fechaHora) => {
  if (!fechaHora) return '-';
  const date = new Date(fechaHora);
  return date.toLocaleString('es-ES', {
    day: '2-digit',
    month: 'short',
    hour: '2-digit',
    minute: '2-digit'
  });
};

const getCurrentDateTime = () => {
  return new Date().toLocaleString('es-ES', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });
};

// Calculations
const calculateTotal = () => {
  return listaPagos.value.reduce((total, item) => {
    if (item.tipoHabitacion === null && item.pagoId === 0) {
      return total - (item.montoEfectivo);
    } else {
      return total + item.montoEfectivo + item.montoTarjeta + (item.montoBillVirt || 0);
    }
  }, 0);
};

const calculateEfectivo = () => {
  return listaPagos.value.reduce((total, item) => {
    if (item.tipoHabitacion === null && item.pagoId === 0) {
      return total - item.montoEfectivo;
    } else {
      return total + item.montoEfectivo;
    }
  }, 0);
};

const calculateDescuento = () => {
  return listaPagos.value.reduce((total, item) => {
    return total + (item.montoDescuento || 0);
  }, 0);
};

const calculateTarjeta = () => {
  return listaPagos.value.reduce((total, item) => {
    return total + (item.montoTarjeta || 0);
  }, 0);
};

const calculateConsumo = () => {
  return listaPagos.value.reduce((total, item) => {
    return total + (item.totalConsumo || 0);
  }, 0);
};

const calculatePeriodo = () => {
  return listaPagos.value.reduce((total, item) => {
    return total + (item.periodo || 0);
  }, 0);
};

const countCategorias = () => {
  return listaPagos.value.reduce((counts, item) => {
    if (item.categoriaNombre) {
      counts[item.categoriaNombre] = (counts[item.categoriaNombre] || 0) + 1;
    }
    return counts;
  }, {});
};

const formatCategorias = () => {
  const categorias = countCategorias();
  return Object.entries(categorias).map(([categoria, count]) => ({ categoria, count }));
};

const calculateAdicional = () => {
  return listaPagos.value.reduce((total, item) => {
    return total + (item.montoAdicional || 0);
  }, 0);
};
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

.glass-header {
  background-color: rgba(255, 255, 255, 0.05);
  backdrop-filter: blur(16px);
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

/* Table styles */
.glass-table-header {
  padding: 16px;
  color: white;
  font-weight: bold;
  font-size: 14px;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  border-bottom: 1px solid rgba(255, 255, 255, 0.2);
}

.glass-table-row {
  transition: all 0.2s ease;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.glass-table-row:hover {
  background-color: rgba(255, 255, 255, 0.05);
}

.glass-table-cell {
  padding: 12px;
  color: rgba(255, 255, 255, 0.9);
}

.glass-table-footer {
  padding: 16px;
  font-weight: bold;
}

/* Animations */
.modal-fade-enter-active,
.modal-fade-leave-active {
  transition: opacity 0.3s ease;
}

.modal-fade-enter-from,
.modal-fade-leave-to {
  opacity: 0;
}

/* Custom scrollbar */
::-webkit-scrollbar {
  width: 10px;
  height: 10px;
}

::-webkit-scrollbar-track {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 5px;
}

::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.3);
  border-radius: 5px;
}

::-webkit-scrollbar-thumb:hover {
  background: rgba(255, 255, 255, 0.5);
}

/* Print styles */
@media print {
  * {
    -webkit-print-color-adjust: exact !important;
    color-adjust: exact !important;
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
    width: 100% !important;
    height: auto !important;
    max-width: none !important;
    margin: 0 !important;
    padding: 0 !important;
    overflow: visible !important;
  }

  /* Hide header and footer sections */
  .glass-header {
    display: none !important;
  }

  /* Hide buttons and interactive elements */
  button,
  .glass-header,
  .fixed,
  .absolute {
    display: none !important;
  }

  /* Show only content section */
  .overflow-y-auto.flex-1 {
    overflow: visible !important;
    height: auto !important;
    flex: none !important;
    padding: 5mm !important;
  }

  /* Summary cards for print */
  [class*="grid-cols-2"] {
    display: grid !important;
    grid-template-columns: repeat(4, 1fr) !important;
    gap: 5mm !important;
    margin-bottom: 8mm !important;
    break-inside: avoid !important;
  }

  .glass-card {
    background: #f8f9fa !important;
    border: 1px solid #dee2e6 !important;
    border-radius: 4px !important;
    padding: 3mm !important;
    color: black !important;
    backdrop-filter: none !important;
    box-shadow: none !important;
    break-inside: avoid !important;
  }

  /* Table styles for print */
  .glass-card.overflow-hidden {
    overflow: visible !important;
    background: white !important;
    border: 1px solid black !important;
    border-radius: 0 !important;
  }

  .overflow-x-auto {
    overflow: visible !important;
  }

  table {
    width: 100% !important;
    min-width: auto !important;
    border-collapse: collapse !important;
    font-size: 7px !important;
    line-height: 1.2 !important;
    table-layout: fixed !important;
  }

  .glass-table-header {
    background: #f8f9fa !important;
    color: black !important;
    font-weight: bold !important;
    padding: 2mm !important;
    border: 1px solid black !important;
    font-size: 6px !important;
    text-align: center !important;
    word-wrap: break-word !important;
    overflow-wrap: break-word !important;
  }

  .glass-table-cell {
    color: black !important;
    padding: 1.5mm !important;
    border: 1px solid black !important;
    font-size: 6px !important;
    word-wrap: break-word !important;
    overflow-wrap: break-word !important;
    max-width: 0 !important;
  }

  .glass-table-row {
    background: white !important;
    border: none !important;
    break-inside: avoid !important;
    page-break-inside: avoid !important;
  }

  .glass-table-row:nth-child(even) {
    background: #f8f9fa !important;
  }

  .glass-table-footer {
    background: #e9ecef !important;
    color: black !important;
    font-weight: bold !important;
    padding: 2mm !important;
    border: 1px solid black !important;
    font-size: 6px !important;
    vertical-align: top !important;
    line-height: 1.1 !important;
  }

  /* Improved totals section for print */
  .totals-row-1 .glass-table-footer {
    padding: 1.5mm !important;
    background: #f8f9fa !important;
    border: 1px solid #333 !important;
    height: auto !important;
    min-height: 8mm !important;
  }

  .totals-row-1 .glass-table-footer p {
    margin: 0 !important;
    padding: 0 !important;
    line-height: 1.1 !important;
  }

  .totals-row-1 .glass-table-footer .text-xs {
    font-size: 5px !important;
    color: #666 !important;
    font-weight: normal !important;
  }

  .totals-row-1 .glass-table-footer .font-bold {
    font-size: 7px !important;
    font-weight: bold !important;
    margin-top: 0.5mm !important;
  }

  .totals-row-1 .glass-table-footer .text-lg {
    font-size: 8px !important;
    font-weight: bold !important;
  }

  /* Categories section */
  .totals-row-1 td:first-child {
    text-align: left !important;
    padding-left: 2mm !important;
  }

  .totals-row-1 td:first-child .space-y-1 > * {
    margin-bottom: 0.5mm !important;
  }

  .totals-row-1 td:first-child .text-sm {
    font-size: 6px !important;
    color: black !important;
    font-weight: bold !important;
  }

  .totals-row-1 td:first-child .text-xs {
    font-size: 5px !important;
    color: #666 !important;
  }

  /* Specific column widths for better fit */
  th:nth-child(1), td:nth-child(1) { width: 12% !important; } /* Tipo Habitación */
  th:nth-child(2), td:nth-child(2) { width: 8% !important; }  /* Pago */
  th:nth-child(3), td:nth-child(3) { width: 6% !important; }  /* Periodo */
  th:nth-child(4), td:nth-child(4) { width: 6% !important; }  /* Adicional */
  th:nth-child(5), td:nth-child(5) { width: 6% !important; }  /* Consumo */
  th:nth-child(6), td:nth-child(6) { width: 8% !important; }  /* Ingreso */
  th:nth-child(7), td:nth-child(7) { width: 8% !important; }  /* Salida */
  th:nth-child(8), td:nth-child(8) { width: 6% !important; }  /* Efectivo */
  th:nth-child(9), td:nth-child(9) { width: 6% !important; }  /* Tarjeta */
  th:nth-child(10), td:nth-child(10) { width: 8% !important; } /* Tarjeta Usada */
  th:nth-child(11), td:nth-child(11) { width: 6% !important; } /* Descuento */
  th:nth-child(12), td:nth-child(12) { width: 6% !important; } /* Total */
  th:nth-child(13), td:nth-child(13) { width: 14% !important; } /* Observación */

  /* Color coding for print */
  .text-green-400, .text-green-600 {
    color: #198754 !important;
  }

  .text-red-400, .text-red-600 {
    color: #dc3545 !important;
  }

  .text-blue-400, .text-blue-600 {
    color: #0d6efd !important;
  }

  .text-purple-400 {
    color: #6f42c1 !important;
  }

  .text-yellow-400 {
    color: #ffc107 !important;
  }

  .text-white {
    color: black !important;
  }

  .text-gray-300, .text-gray-400 {
    color: #6c757d !important;
  }

  /* Background colors for special rows */
  [class*="bg-red-500"] {
    background: #ffebee !important;
  }

  [class*="bg-purple-500"] {
    background: #f3e5f5 !important;
  }

  /* Page setup */
  @page {
    size: A4 landscape;
    margin: 10mm 5mm;
  }

  /* Force page breaks */
  .page-break {
    page-break-before: always !important;
  }

  /* Prevent page breaks within important elements */
  .glass-card,
  tbody tr,
  .space-y-2 {
    break-inside: avoid !important;
    page-break-inside: avoid !important;
  }

  /* Print header */
  .print-header {
    display: block !important;
    text-align: center !important;
    margin-bottom: 5mm !important;
    padding-bottom: 3mm !important;
    border-bottom: 2px solid black !important;
  }

  .print-header h1 {
    font-size: 14px !important;
    font-weight: bold !important;
    margin: 0 0 2mm 0 !important;
    color: black !important;
  }

  .print-header p {
    font-size: 10px !important;
    margin: 0 !important;
    color: #666 !important;
  }
}
</style>