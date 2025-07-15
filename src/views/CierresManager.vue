<template>
  <div class="min-h-screen bg-gradient-to-br from-neutral-900 via-neutral-800 to-neutral-900 p-4 lg:p-6">
    <!-- Header Section -->
    <div class="glass-container mb-6 p-6 transform hover:scale-[1.02] transition-all duration-300">
      <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between space-y-4 lg:space-y-0">
        <div class="text-center lg:text-left">
          <div class="flex items-center justify-center lg:justify-start mb-2">
            <div class="bg-gradient-to-r from-primary-400 to-accent-400 p-3 rounded-full mr-3">
              <i class="pi pi-calculator text-white text-2xl"></i>
            </div>
            <h1 class="text-3xl lg:text-4xl font-bold text-white">🧮 Gestión de Cierres</h1>
          </div>
          <p class="text-gray-300 text-lg">Controla y revisa todos los cierres de caja de manera fácil 💰</p>
        </div>
        
        <!-- Quick Stats -->
        <div class="glass-card p-4">
          <div class="grid grid-cols-2 gap-4 text-center">
            <div class="transform hover:scale-110 transition-transform duration-200">
              <div class="bg-gradient-to-r from-primary-400 to-primary-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
                <span class="font-bold text-lg">{{ cierres?.length || 0 }}</span>
              </div>
              <p class="text-xs text-gray-300">Cierres</p>
            </div>
            <div class="transform hover:scale-110 transition-transform duration-200">
              <div class="bg-gradient-to-r from-secondary-400 to-secondary-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
                <i class="pi pi-clock text-white text-lg"></i>
              </div>
              <p class="text-xs text-gray-300">Histórico</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Current Session Section -->
    <div v-if="pagosSinCierres" class="glass-container mb-6 p-6">
      <div class="flex items-center mb-4">
        <i class="pi pi-clock text-green-400 text-xl mr-2"></i>
        <h3 class="text-xl font-bold text-white">🟢 Sesión Actual</h3>
      </div>
      
      <div 
        @click="abrirPagosSinCierre"
        class="glass-card p-6 cursor-pointer hover:bg-white/15 transition-all duration-300 group transform hover:scale-[1.02]"
      >
        <div class="flex items-center justify-between">
          <div class="flex items-center">
            <div class="bg-gradient-to-r from-green-400 to-emerald-500 p-4 rounded-full mr-4 group-hover:scale-110 transition-transform">
              <i class="pi pi-play text-white text-2xl"></i>
            </div>
            <div>
              <h4 class="text-xl font-bold text-white mb-1">💸 Cierre Actual</h4>
              <p class="text-gray-300">Sesión activa - Haz clic para ver detalles</p>
            </div>
          </div>
          <div class="flex items-center space-x-2">
            <div class="bg-green-500/20 px-3 py-1 rounded-full border border-green-500/30">
              <span class="text-green-400 text-sm font-semibold flex items-center">
                <i class="pi pi-circle-fill mr-1 animate-pulse"></i>
                Activo
              </span>
            </div>
            <i class="pi pi-chevron-right text-gray-400 group-hover:text-white transition-colors"></i>
          </div>
        </div>
      </div>
    </div>

    <!-- Historical Closures Section -->
    <div v-if="authStore.auth && (authStore.auth.rol === 1 || authStore.auth.rol === 2)" class="glass-container p-6">
      <div class="flex items-center justify-between mb-6">
        <div class="flex items-center">
          <i class="pi pi-history text-primary-400 text-xl mr-2"></i>
          <h3 class="text-xl font-bold text-white">
            {{ cierres.length > 0 ? `📚 ${cierres.length} cierres históricos` : '📚 Cierres Históricos' }}
          </h3>
        </div>
        
        <button
          @click="fetchCierres"
          :disabled="isLoading"
          class="glass-button px-4 py-2 text-white hover:bg-white/20 transform hover:scale-105 transition-all disabled:opacity-50"
        >
          <i :class="isLoading ? 'pi pi-spinner pi-spin' : 'pi pi-refresh'" class="mr-2"></i>
          Actualizar
        </button>
      </div>

      <!-- Loading State -->
      <div v-if="isLoading" class="text-center py-16">
        <div class="glass-card max-w-md mx-auto p-8">
          <div class="bg-gradient-to-r from-primary-400 to-accent-400 p-6 rounded-full mx-auto w-24 h-24 flex items-center justify-center mb-4">
            <i class="pi pi-spinner pi-spin text-white text-4xl"></i>
          </div>
          <h3 class="text-xl text-white font-bold mb-2">🔄 Cargando cierres...</h3>
          <p class="text-gray-300">Obteniendo el historial de cierres</p>
        </div>
      </div>

      <!-- Closures Grid -->
      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div
          v-for="(cierre, index) in cierres"
          :key="cierre.cierreId"
          @click="fetchPagosByCierre(cierre.cierreId)"
          class="glass-card p-6 cursor-pointer hover:bg-white/15 transition-all duration-300 group transform hover:scale-105"
        >
          <!-- Closure Header -->
          <div class="flex items-start justify-between mb-4">
            <div class="flex items-center">
              <div class="bg-gradient-to-r from-blue-400 to-purple-500 p-3 rounded-full mr-3 group-hover:scale-110 transition-transform">
                <i class="pi pi-file text-white text-xl"></i>
              </div>
              <div>
                <h4 class="text-white font-bold text-lg">
                  {{ cierre.cierreId === 0 ? '🟢 Cierre Actual' : `📄 Cierre #${cierre.cierreId}` }}
                </h4>
                <p class="text-gray-300 text-sm">{{ formatFechaHora(cierre.fechaHoraCierre) }}</p>
              </div>
            </div>
            <i class="pi pi-eye text-gray-400 group-hover:text-white transition-colors"></i>
          </div>

          <!-- Closure Info -->
          <div class="space-y-3">
            <div class="glass-card p-3 bg-white/5">
              <div class="flex items-center justify-between">
                <span class="text-gray-300 text-sm flex items-center">
                  <i class="pi pi-calendar mr-2 text-blue-400"></i>
                  Fecha de cierre:
                </span>
                <span class="text-blue-400 font-semibold text-sm">{{ formatFecha(cierre.fechaHoraCierre) }}</span>
              </div>
            </div>
            
            <div class="glass-card p-3 bg-white/5">
              <div class="flex items-center justify-between">
                <span class="text-gray-300 text-sm flex items-center">
                  <i class="pi pi-clock mr-2 text-purple-400"></i>
                  Hora:
                </span>
                <span class="text-purple-400 font-semibold text-sm">{{ formatHora(cierre.fechaHoraCierre) }}</span>
              </div>
            </div>
          </div>

          <!-- Hover Actions -->
          <div class="mt-4 opacity-0 group-hover:opacity-100 transition-all duration-300">
            <button
              @click.stop="fetchPagosByCierre(cierre.cierreId)"
              class="w-full glass-button py-3 text-white hover:text-blue-300 transform hover:scale-105 transition-all"
            >
              <i class="pi pi-eye mr-2"></i>
              👁️ Ver Detalles
            </button>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div v-if="!isLoading && cierres.length === 0" class="text-center py-16">
        <div class="glass-card max-w-md mx-auto p-8">
          <div class="mb-6">
            <div class="bg-gradient-to-r from-primary-400 to-accent-400 p-6 rounded-full mx-auto w-24 h-24 flex items-center justify-center mb-4">
              <i class="pi pi-file text-white text-4xl"></i>
            </div>
            <h3 class="text-2xl text-white font-bold mb-2">📚 ¡No hay cierres históricos!</h3>
            <p class="text-gray-300 mb-6">Aún no se han realizado cierres de caja en el sistema</p>
          </div>
          
          <button
            @click="fetchCierres"
            class="bg-gradient-to-r from-primary-400 to-accent-400 hover:from-primary-500 hover:to-accent-500 
                   text-white font-bold py-3 px-6 rounded-lg transform hover:scale-105 transition-all duration-300"
          >
            <i class="pi pi-refresh mr-2"></i>
            🔄 Actualizar Lista
          </button>
        </div>
      </div>
    </div>

    <!-- Access Denied for Regular Users -->
    <div v-else class="glass-container p-6">
      <div class="text-center py-16">
        <div class="glass-card max-w-md mx-auto p-8">
          <div class="mb-6">
            <div class="bg-gradient-to-r from-red-400 to-red-500 p-6 rounded-full mx-auto w-24 h-24 flex items-center justify-center mb-4">
              <i class="pi pi-lock text-white text-4xl"></i>
            </div>
            <h3 class="text-2xl text-white font-bold mb-2">🔒 Acceso Restringido</h3>
            <p class="text-gray-300 mb-6">Solo los administradores pueden ver el historial de cierres</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Enhanced Modals -->
    <div v-if="showPagosModal" class="fixed inset-0 bg-black/60 backdrop-blur-xl z-50 flex items-center justify-center p-4">
      <ModalCierre
        :selectedPagos="pagos"
        :idcierre="selectedIdCierre"
        :selectedEgresos="egresosSinCierres"
        :esAbierto="false"
        @imprimir-modal="ImprimirModal"
        @close-modal="togglePagosModal"
      />
    </div>
    
    <div v-if="showPagosSinCierreModal" class="fixed inset-0 bg-black/60 backdrop-blur-xl z-50 flex items-center justify-center p-4">
      <ModalCierre
        :selectedPagos="pagosSinCierres"
        :selectedEgresos="egresosSinCierres"
        :esAbierto="true"
        @imprimir-modal="ImprimirModal"
        @close-modal="togglePagosSinCierreModal"
      />
    </div>

    <!-- Toast for notifications -->
    <Toast />
    <ConfirmDialog />
  </div>
</template>

<script setup>
import { ref, onBeforeMount } from 'vue'
import { useToast } from 'primevue/usetoast'
import axiosClient from '../axiosClient'
import ModalCierre from '../components/ModalCierre.vue'
import { useAuthStore } from '../store/auth'
import Toast from 'primevue/toast'
import ConfirmDialog from 'primevue/confirmdialog'

// Composables
const toast = useToast()
const authStore = useAuthStore()

// State
const cierres = ref([])
const showPagosModal = ref(false)
const showPagosSinCierreModal = ref(false)
const pagos = ref([])
const egresos = ref([])
const pagosSinCierres = ref([])
const egresosSinCierres = ref([])
const selectedIdCierre = ref()
const isLoading = ref(false)
const InstitucionID = ref(null)

// Methods
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

const formatFechaHora = (fechaHora) => {
  if (!fechaHora) return 'Sin fecha'
  const date = new Date(fechaHora)
  return date.toLocaleString('es-ES', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const formatFecha = (fechaHora) => {
  if (!fechaHora) return 'Sin fecha'
  const date = new Date(fechaHora)
  return date.toLocaleDateString('es-ES', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })
}

const formatHora = (fechaHora) => {
  if (!fechaHora) return 'Sin hora'
  const date = new Date(fechaHora)
  return date.toLocaleTimeString('es-ES', {
    hour: '2-digit',
    minute: '2-digit'
  })
}

const getDatosLogin = () => {
  InstitucionID.value = authStore.institucionID
}

const togglePagosModal = () => {
  showPagosModal.value = !showPagosModal.value
}

const togglePagosSinCierreModal = () => {
  showPagosSinCierreModal.value = !showPagosSinCierreModal.value
}

// Fetch all cierres with their associated pagos when the view is loaded
const fetchCierres = async () => {
  if (!InstitucionID.value) {
    showError('❌ No se pudo obtener la información de la institución')
    return
  }

  try {
    isLoading.value = true
    const response = await axiosClient.get(`/api/Caja/GetCierresyActual?InstitucionID=${InstitucionID.value}`)
    
    if (response.data?.ok) {
      cierres.value = response.data.data?.cierres || []
      pagosSinCierres.value = response.data.data?.pagosSinCierre || []
      egresosSinCierres.value = response.data.data?.egresos || []
      
      if (cierres.value.length > 0) {
        showSuccess(`📚 ${cierres.value.length} cierres cargados correctamente`)
      }
    } else {
      console.error('Error fetching cierres:', response.data?.message)
      showError('❌ Error al cargar los cierres')
    }
  } catch (error) {
    console.error('Error fetching cierres:', error)
    showError('❌ Error al cargar los cierres')
  } finally {
    isLoading.value = false
  }
}

// Fetch pagos by CierreId when a Cierre is clicked
const fetchPagosByCierre = (cierreId) => {
  selectedIdCierre.value = cierreId
  showSuccess(`👁️ Abriendo detalles del cierre #${cierreId}`)
  togglePagosModal()
}

const abrirPagosSinCierre = () => {
  pagos.value = pagosSinCierres.value
  egresos.value = egresosSinCierres.value
  showSuccess('🟢 Abriendo sesión actual')
  togglePagosSinCierreModal()
}

const ImprimirModal = (cierreCajaRef) => {
  try {
    // Get the modal content
    const printContent = cierreCajaRef.innerHTML

    // Clone styles from document
    const styles = Array.from(document.styleSheets)
      .map((styleSheet) => {
        try {
          return Array.from(styleSheet.cssRules)
            .map((rule) => rule.cssText)
            .join("\n")
        } catch (error) {
          return "" // Ignore cross-origin styles
        }
      })
      .join("\n")

    // Open a new window
    const printWindow = window.open("", "_blank")

    printWindow.document.write(`
      <html>
        <head>
          <title>Imprimir Cierre de Caja</title>
          <style>
            ${styles}  
          </style>
        </head>
        <body>
          <div id="cierre-caja-content">${printContent}</div>
          <script>
            window.onload = function() {
              window.print();
              window.onafterprint = function() { 
                window.close(); 
              };
             };
          <\/script> 
        </body>
      </html>
    `)

    printWindow.document.close()
    showSuccess('🖨️ Preparando impresión...')
  } catch (error) {
    console.error('Error printing:', error)
    showError('❌ Error al preparar la impresión')
  }
}

onBeforeMount(() => {
  getDatosLogin()
  fetchCierres()
})
</script>

<style scoped>
/* Glass effect styles */
.glass-container {
  @apply bg-white/5 backdrop-blur-2xl border border-white/20 rounded-3xl;
}

.glass-card {
  @apply bg-white/10 backdrop-blur-md border border-white/20 rounded-xl;
}

.glass-button {
  @apply bg-white/10 hover:bg-white/20 backdrop-blur-sm border border-white/30 rounded-lg transition-all;
}

.glass-input {
  @apply bg-white/10 backdrop-blur-sm border border-white/30 rounded-lg text-white placeholder-gray-300;
}

.glass-input:focus {
  @apply ring-2 ring-primary-400 border-primary-400 outline-none;
}

/* Custom animations */
@keyframes shake {
  0%, 100% { transform: translateX(0); }
  25% { transform: translateX(-5px); }
  75% { transform: translateX(5px); }
}

.shake {
  animation: shake 0.5s ease-in-out;
}

/* Hover animations */
.animate-pulse {
  animation: pulse 2s cubic-bezier(0.4, 0, 0.6, 1) infinite;
}

.animate-bounce {
  animation: bounce 1s infinite;
}

/* Custom scrollbar */
::-webkit-scrollbar {
  width: 8px;
}

::-webkit-scrollbar-track {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 4px;
}

::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.3);
  border-radius: 4px;
}

::-webkit-scrollbar-thumb:hover {
  background: rgba(255, 255, 255, 0.5);
}

/* Ensure the modal content is not truncated */
#cierre-caja-content {
  overflow: visible !important;
}

/* Print-specific styles */
@media print {
  #cierre-caja-content {
    overflow: visible !important;
    height: auto !important;
  }
  #cierre-caja-content * {
    display: grid !important; /* Ensure all elements are visible */
    grid-template-columns: repeat(14, minmax(0, 1fr)) !important;
  }

  body * {
    visibility: hidden;
  }
  #print-container,
  #print-container * {
    visibility: visible;
  }
  #print-container {
    position: absolute;
    left: 0;
    top: 0;
    width: 100%;
    height: auto !important;
    overflow: visible !important;
  }

  ol, ul {
    page-break-inside: avoid;
  }
  li {
    page-break-inside: avoid;
    page-break-after: auto;
  }
}
</style>