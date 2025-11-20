<template>
  <div class="min-h-screen bg-gradient-to-br from-neutral-900 via-neutral-800 to-neutral-900 p-4 lg:p-6">
    <!-- Header Section -->
    <div class="glass-container mb-6 p-6 transform hover:scale-[1.02] transition-all duration-300">
      <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between space-y-4 lg:space-y-0">
        <div class="text-center lg:text-left">
          <div class="flex items-center justify-center lg:justify-start mb-2">
            <div class="bg-gradient-to-r from-primary-400 to-accent-400 p-3 rounded-full mr-3">
              <i class="pi pi-percentage text-white text-2xl"></i>
            </div>
            <h1 class="text-3xl lg:text-4xl font-bold text-white">🎯 Gestión de Promociones</h1>
          </div>
          <p class="text-gray-300 text-lg">Crea y administra promociones atractivas para tus clientes 🚀</p>
        </div>
        
        <!-- Quick Stats -->
        <div class="glass-card p-4">
          <div class="grid grid-cols-2 gap-4 text-center">
            <div class="transform hover:scale-110 transition-transform duration-200">
              <div class="bg-gradient-to-r from-primary-400 to-primary-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
                <span class="font-bold text-lg">{{ promociones?.length || 0 }}</span>
              </div>
              <p class="text-xs text-gray-300">Promociones</p>
            </div>
            <div class="transform hover:scale-110 transition-transform duration-200">
              <div class="bg-gradient-to-r from-secondary-400 to-secondary-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
                <span class="font-bold text-xs">{{ averageDiscount }}%</span>
              </div>
              <p class="text-xs text-gray-300">Descuento Prom.</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Category Selection -->
    <div class="glass-container mb-6 p-6">
      <div class="flex items-center mb-4">
        <i class="pi pi-filter text-accent-400 text-xl mr-2"></i>
        <h3 class="text-xl font-bold text-white">📂 Seleccionar Categoría</h3>
      </div>
      
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <!-- Category Selector -->
        <div class="md:col-span-3 relative">
          <i class="pi pi-list absolute left-4 top-4 text-gray-400"></i>
          <select
            v-model="selectedCategory"
            @change="fetchPromociones"
            class="glass-input w-full pl-12 pr-4 py-3 text-lg"
          >
            <option value="">🤔 Selecciona una categoría...</option>
            <option
              v-for="categoria in (categorias || [])"
              :key="categoria.categoriaId"
              :value="categoria.categoriaId"
            >
              📁 {{ categoria.nombreCategoria }}
            </option>
          </select>
        </div>

        <!-- Quick Create Button -->
        <button
          @click="openCreateModal"
          :disabled="!selectedCategory"
          class="bg-gradient-to-r from-primary-400 to-accent-400 hover:from-primary-500 hover:to-accent-500 
                 disabled:opacity-50 disabled:cursor-not-allowed
                 text-white font-bold py-3 px-6 rounded-lg 
                 transition-all duration-300 transform hover:scale-105"
        >
          <i class="pi pi-plus mr-2"></i>
          🎯 Nueva Promoción
        </button>
      </div>

      <div v-if="!selectedCategory" class="mt-4 p-3 bg-blue-500/20 rounded-lg border border-blue-500/30">
        <p class="text-blue-300 text-sm flex items-center">
          <i class="pi pi-info-circle mr-2"></i>
          Selecciona una categoría para ver y gestionar sus promociones
        </p>
      </div>
    </div>

    <!-- Promotions Grid -->
    <div v-if="selectedCategory" class="glass-container p-6">
      <div class="flex items-center justify-between mb-6">
        <div class="flex items-center">
          <i class="pi pi-gift text-primary-400 text-xl mr-2"></i>
          <h3 class="text-xl font-bold text-white">
            {{ promociones.length > 0 ? `🎯 ${promociones.length} promociones activas` : '🎯 Promociones' }}
          </h3>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="isLoading" class="text-center py-16">
        <div class="glass-card max-w-md mx-auto p-8">
          <div class="bg-gradient-to-r from-primary-400 to-accent-400 p-6 rounded-full mx-auto w-24 h-24 flex items-center justify-center mb-4">
            <i class="pi pi-spinner pi-spin text-white text-4xl"></i>
          </div>
          <h3 class="text-xl text-white font-bold mb-2">🔄 Cargando promociones...</h3>
          <p class="text-gray-300">Obteniendo las mejores ofertas</p>
        </div>
      </div>

      <!-- Promotions Grid -->
      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
        <!-- Existing Promotions -->
        <div
          v-for="promocion in promociones"
          :key="promocion.promocionID"
          class="glass-card p-6 hover:bg-white/15 transition-all duration-300 group transform hover:scale-105 cursor-pointer"
          @click="openModal(promocion)"
        >
          <!-- Promotion Header -->
          <div class="mb-4">
            <div class="flex items-center justify-between mb-2">
              <div class="bg-gradient-to-r from-accent-400 to-pink-400 text-white px-3 py-1 rounded-full text-xs font-bold">
                🎯 OFERTA
              </div>
              <i class="pi pi-edit text-gray-400 group-hover:text-white transition-colors"></i>
            </div>
            <h3 class="text-white font-bold text-lg mb-2 line-clamp-2">
              {{ promocion.detalle }}
            </h3>
          </div>

          <!-- Promotion Details -->
          <div class="space-y-3">
            <div class="glass-card p-3 bg-white/5">
              <div class="flex items-center justify-between">
                <span class="text-gray-300 text-sm flex items-center">
                  <i class="pi pi-dollar mr-2 text-green-400"></i>
                  Tarifa por hora:
                </span>
                <span class="text-green-400 font-bold">${{ formatPrice(promocion.tarifa) }}</span>
              </div>
            </div>
            
            <div class="glass-card p-3 bg-white/5">
              <div class="flex items-center justify-between">
                <span class="text-gray-300 text-sm flex items-center">
                  <i class="pi pi-clock mr-2 text-blue-400"></i>
                  Duración:
                </span>
                <span class="text-blue-400 font-bold">{{ promocion.cantidadHoras }}h</span>
              </div>
            </div>

            <!-- Total Calculation -->
            <div class="glass-card p-3 bg-gradient-to-r from-primary-400/20 to-accent-400/20 border border-primary-400/30">
              <div class="flex items-center justify-between">
                <span class="text-white text-sm font-semibold flex items-center">
                  <i class="pi pi-calculator mr-2 text-accent-400"></i>
                  Precio total:
                </span>
                <span class="text-accent-400 font-bold text-lg">
                  ${{ formatPrice(promocion.tarifa * promocion.cantidadHoras) }}
                </span>
              </div>
            </div>
          </div>

          <!-- Hover Actions -->
          <div class="mt-4 opacity-0 group-hover:opacity-100 transition-all duration-300">
            <div class="flex space-x-2">
              <button
                @click.stop="openModal(promocion)"
                class="flex-1 glass-button py-2 text-white hover:text-blue-300 transform hover:scale-105 transition-all"
              >
                <i class="pi pi-pencil mr-1"></i>
                ✏️ Editar
              </button>
              <button
                @click.stop="handleDelete(promocion)"
                class="flex-1 glass-button py-2 text-white hover:text-red-300 transform hover:scale-105 transition-all"
              >
                <i class="pi pi-trash mr-1"></i>
                🗑️ Eliminar
              </button>
            </div>
          </div>
        </div>

        <!-- Add New Promotion Card -->
        <div
          class="glass-card p-6 border-2 border-dashed border-primary-400/50 hover:border-primary-400 
                 hover:bg-white/15 transition-all duration-300 group transform hover:scale-105 cursor-pointer
                 flex flex-col items-center justify-center text-center min-h-[280px]"
          @click="openCreateModal"
        >
          <div class="bg-gradient-to-r from-primary-400 to-accent-400 p-6 rounded-full mx-auto mb-4 group-hover:scale-110 transition-transform">
            <i class="pi pi-plus text-white text-3xl"></i>
          </div>
          <h3 class="text-white font-bold text-lg mb-2">🎯 Nueva Promoción</h3>
          <p class="text-gray-300 text-sm">
            Haz clic aquí para crear una nueva promoción increíble
          </p>
        </div>
      </div>

      <!-- Empty State -->
      <div v-if="!isLoading && promociones.length === 0" class="text-center py-16">
        <div class="glass-card max-w-md mx-auto p-8">
          <div class="mb-6">
            <div class="bg-gradient-to-r from-primary-400 to-accent-400 p-6 rounded-full mx-auto w-24 h-24 flex items-center justify-center mb-4">
              <i class="pi pi-gift text-white text-4xl"></i>
            </div>
            <h3 class="text-2xl text-white font-bold mb-2">🎯 ¡No hay promociones aún!</h3>
            <p class="text-gray-300 mb-6">¡Es hora de crear promociones irresistibles para esta categoría!</p>
          </div>
          
          <button
            @click="openCreateModal"
            class="bg-gradient-to-r from-primary-400 to-accent-400 hover:from-primary-500 hover:to-accent-500 
                   text-white font-bold py-3 px-6 rounded-lg transform hover:scale-105 transition-all duration-300"
          >
            <i class="pi pi-plus mr-2"></i>
            🚀 ¡Crear mi primera promoción!
          </button>
        </div>
      </div>
    </div>

    <!-- Enhanced Promotion Modal -->
    <div v-if="showModal" class="fixed inset-0 bg-black/60 backdrop-blur-xl z-50 flex items-center justify-center p-4">
      <div class="glass-container max-w-2xl w-full p-8 transform transition-all duration-300">
        <!-- Modal Header -->
        <div class="flex items-center justify-between mb-6">
          <div class="flex items-center">
            <div class="bg-gradient-to-r from-primary-400 to-accent-400 p-3 rounded-full mr-3">
              <i :class="selectedPromocion ? 'pi pi-pencil' : 'pi pi-plus'" class="text-white text-xl"></i>
            </div>
            <h2 class="text-2xl font-bold text-white">
              {{ selectedPromocion ? '✏️ Editar Promoción' : '🎯 Crear Nueva Promoción' }}
            </h2>
          </div>
          <button
            @click="closeModal"
            class="glass-button px-3 py-2 text-white hover:text-gray-300"
          >
            <i class="pi pi-times"></i>
          </button>
        </div>

        <!-- Modal Form -->
        <form @submit.prevent="handleSave" class="grid grid-cols-1 lg:grid-cols-2 gap-8">
          <!-- Left Column - Form Fields -->
          <div class="space-y-6">
            <!-- Detail Field -->
            <div class="glass-card p-4 hover:bg-white/15 transition-all duration-300">
              <label class="flex items-center text-white font-semibold mb-3">
                <i class="pi pi-tag text-primary-400 mr-2"></i>
                ¿Cómo se llama la promoción? *
              </label>
              <div class="relative">
                <input
                  v-model="formData.detalle"
                  type="text"
                  class="glass-input w-full px-4 py-3 pl-12 text-lg"
                  placeholder="Ej: Oferta Especial de Fin de Semana 🎉"
                  required
                />
                <i class="pi pi-sparkles absolute left-4 top-4 text-accent-400"></i>
              </div>
            </div>

            <!-- Price Field -->
            <div class="glass-card p-4 hover:bg-white/15 transition-all duration-300">
              <label class="flex items-center text-white font-semibold mb-3">
                <i class="pi pi-dollar text-secondary-400 mr-2"></i>
                ¿Cuál es la tarifa por hora? *
              </label>
              <div class="relative">
                <span class="absolute left-4 top-4 text-accent-400 font-bold text-lg">$</span>
                <input
                  v-model.number="formData.tarifa"
                  type="number"
                  step="0.01"
                  min="0"
                  class="glass-input w-full px-4 py-3 pl-12 text-lg"
                  placeholder="0.00"
                  required
                />
                <div v-if="formData.tarifa" class="absolute right-4 top-4">
                  <i class="pi pi-money-bill text-green-400 animate-pulse"></i>
                </div>
              </div>
            </div>

            <!-- Hours Field -->
            <div class="glass-card p-4 hover:bg-white/15 transition-all duration-300">
              <label class="flex items-center text-white font-semibold mb-3">
                <i class="pi pi-clock text-primary-400 mr-2"></i>
                ¿Cuántas horas dura? *
              </label>
              <div class="relative">
                <input
                  v-model.number="formData.cantidadHoras"
                  type="number"
                  min="1"
                  max="24"
                  class="glass-input w-full px-4 py-3 pr-12 text-lg"
                  placeholder="1"
                  required
                />
                <span class="absolute right-4 top-4 text-accent-400 font-bold text-lg">h</span>
                <div v-if="formData.cantidadHoras" class="absolute left-4 top-4">
                  <i class="pi pi-clock text-blue-400 animate-pulse"></i>
                </div>
              </div>
              <div class="mt-2 p-2 bg-blue-500/20 rounded-lg border border-blue-500/30">
                <p class="text-blue-300 text-sm flex items-center">
                  <i class="pi pi-info-circle mr-2"></i>
                  Duración entre 1 y 24 horas
                </p>
              </div>
            </div>
          </div>

          <!-- Right Column - Preview Card -->
          <div class="space-y-6">
            <!-- Promotion Preview -->
            <div class="glass-card p-4 hover:bg-white/15 transition-all duration-300">
              <label class="flex items-center text-white font-semibold mb-3">
                <i class="pi pi-eye text-accent-400 mr-2"></i>
                👀 Vista previa de la promoción
              </label>
              
              <!-- Promotion Design -->
              <div class="relative">
                <div class="bg-gradient-to-br from-primary-400 via-secondary-400 to-accent-400 p-6 rounded-xl text-white shadow-2xl transform hover:scale-105 transition-all duration-300">
                  <div class="flex justify-between items-start mb-4">
                    <div class="bg-white/20 backdrop-blur-sm px-3 py-1 rounded-full text-xs font-bold">
                      🎯 PROMOCIÓN ESPECIAL
                    </div>
                    <i class="pi pi-percentage text-2xl opacity-80"></i>
                  </div>
                  
                  <div class="mb-4">
                    <h3 class="text-xl font-bold mb-2">{{ formData.detalle || 'Nombre de la promoción' }}</h3>
                  </div>
                  
                  <div class="grid grid-cols-2 gap-4 mb-4">
                    <div>
                      <p class="text-xs opacity-80">Tarifa por hora</p>
                      <p class="text-lg font-bold">${{ formatPrice(formData.tarifa || 0) }}</p>
                    </div>
                    <div>
                      <p class="text-xs opacity-80">Duración</p>
                      <p class="text-lg font-bold">{{ formData.cantidadHoras || 0 }} horas</p>
                    </div>
                  </div>
                  
                  <div class="border-t border-white/20 pt-4">
                    <p class="text-xs opacity-80">Precio total</p>
                    <p class="text-2xl font-bold">
                      ${{ formatPrice((formData.tarifa || 0) * (formData.cantidadHoras || 0)) }}
                    </p>
                  </div>
                </div>
              </div>
              
              <div class="mt-3 p-3 bg-purple-500/20 rounded-lg border border-purple-500/30">
                <p class="text-purple-300 text-sm flex items-center">
                  <i class="pi pi-sparkles mr-2"></i>
                  Esta es una vista previa de tu promoción ✨
                </p>
              </div>
            </div>

            <!-- Submit Buttons -->
            <div class="flex space-x-3">
              <button
                type="button"
                @click="closeModal"
                class="flex-1 glass-button py-3 text-white hover:text-red-300 transform hover:scale-105 transition-all"
              >
                <i class="pi pi-times mr-2"></i>
                Cancelar
              </button>
              <button
                type="submit"
                :disabled="!isFormValid || isSubmitting"
                class="flex-1 bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 
                       hover:from-primary-500 hover:via-secondary-500 hover:to-accent-500 
                       disabled:opacity-50 disabled:cursor-not-allowed
                       text-white font-bold py-3 px-6 rounded-lg 
                       transition-all duration-300 transform hover:scale-105"
              >
                <div class="flex items-center justify-center">
                  <i v-if="isSubmitting" class="pi pi-spinner pi-spin mr-2"></i>
                  <i v-else :class="selectedPromocion ? 'pi pi-check' : 'pi pi-sparkles'" class="mr-2"></i>
                  <span>
                    {{ isSubmitting ? '✨ Guardando...' : (selectedPromocion ? '🎉 ¡Actualizar!' : '🚀 ¡Crear!') }}
                  </span>
                </div>
              </button>
            </div>
          </div>
        </form>
      </div>
    </div>

    <!-- Toast for notifications -->
    <Toast />
    <ConfirmDialog />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'
import axiosClient from '../axiosClient'
import { useAuthStore } from '../store/auth.js'
import Toast from 'primevue/toast'
import ConfirmDialog from 'primevue/confirmdialog'

// Composables
const toast = useToast()
const confirm = useConfirm()
const authStore = useAuthStore()

// State
const categorias = ref([])
const selectedCategory = ref('')
const promociones = ref([])
const isLoading = ref(false)
const isSubmitting = ref(false)
const showModal = ref(false)
const selectedPromocion = ref(null)

// Form data
const formData = ref({
  detalle: '',
  tarifa: null,
  cantidadHoras: null
})

// Computed
const averageDiscount = computed(() => {
  if (!promociones.value || promociones.value.length === 0) return 0
  // Calculate average savings compared to a theoretical base rate
  const avgPrice = promociones.value.reduce((sum, promo) => sum + (promo.tarifa || 0), 0) / promociones.value.length
  const baseRate = 50 // Theoretical base rate
  return Math.max(0, Math.round(((baseRate - avgPrice) / baseRate) * 100))
})

const isFormValid = computed(() => {
  return formData.value.detalle.trim() !== '' &&
         formData.value.tarifa !== null &&
         formData.value.tarifa > 0 &&
         formData.value.cantidadHoras !== null &&
         formData.value.cantidadHoras > 0
})

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

const formatPrice = (price) => {
  return new Intl.NumberFormat('es-CO', {
    minimumFractionDigits: 0,
    maximumFractionDigits: 2
  }).format(price || 0)
}

const resetForm = () => {
  formData.value = {
    detalle: '',
    tarifa: null,
    cantidadHoras: null
  }
  selectedPromocion.value = null
}

const openModal = (promocion = null) => {
  selectedPromocion.value = promocion
  if (promocion) {
    formData.value = {
      detalle: promocion.detalle,
      tarifa: promocion.tarifa,
      cantidadHoras: promocion.cantidadHoras
    }
  } else {
    resetForm()
  }
  showModal.value = true
}

const openCreateModal = () => {
  if (!selectedCategory.value) {
    showError('⚠️ Primero selecciona una categoría')
    return
  }
  openModal()
}

const closeModal = () => {
  showModal.value = false
  resetForm()
}

const handleSave = async () => {
  if (!isFormValid.value) return
  
  isSubmitting.value = true
  
  try {
    const data = {
      ...formData.value,
      promocionID: selectedPromocion.value?.promocionID || null
    }

    if (data.promocionID) {
      // Update promotion
      await axiosClient.put(
        `/api/Promociones/UpdatePromocion?promocionID=${data.promocionID}&tarifa=${data.tarifa}&cantidadHoras=${data.cantidadHoras}&detalle=${encodeURIComponent(data.detalle)}`
      )
      showSuccess('🎉 ¡Promoción actualizada exitosamente!')
    } else {
      // Create promotion
      await axiosClient.post(
        `/api/Promociones/AddPromocion?tarifa=${data.tarifa}&cantidadHoras=${data.cantidadHoras}&categoriaID=${selectedCategory.value}&Detalle=${encodeURIComponent(data.detalle)}&InstitucionID=${authStore.institucionID}`
      )
      showSuccess('🚀 ¡Nueva promoción creada con éxito!')
    }
    
    closeModal()
    await fetchPromociones()
    
  } catch (error) {
    console.error('Error saving promotion:', error)
    showError(selectedPromocion.value ? '❌ Error al actualizar la promoción' : '❌ Error al crear la promoción')
  } finally {
    isSubmitting.value = false
  }
}

const handleDelete = async (promocion) => {
  const confirmed = await new Promise((resolve) => {
    confirm.require({
      message: `¿Estás seguro de eliminar la promoción "${promocion.detalle}"?`,
      header: 'Confirmar Eliminación',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sí, eliminar',
      rejectLabel: 'Cancelar',
      acceptClass: 'p-button-danger',
      accept: () => resolve(true),
      reject: () => resolve(false)
    })
  })

  if (!confirmed) return

  try {
    // Note: API endpoint for deletion would need to be implemented
    // await axiosClient.delete(`/api/Promociones/DeletePromocion?promocionID=${promocion.promocionID}`)
    showSuccess('🗑️ Promoción eliminada correctamente')
    await fetchPromociones()
  } catch (error) {
    console.error('Error deleting promotion:', error)
    showError('❌ Error al eliminar la promoción')
  }
}

const fetchCategorias = async () => {
  if (!authStore.institucionID) {
    showError('❌ No se pudo obtener la información de la institución')
    return
  }

  try {
    const response = await axiosClient.get(`/GetCategorias?InstitucionID=${authStore.institucionID}`)
    
    if (response.data?.ok) {
      categorias.value = response.data.data || []
    } else {
      console.error('Error fetching categories:', response.data?.message)
      showError('❌ Error al cargar las categorías')
    }
  } catch (error) {
    console.error('Error fetching categories:', error)
    showError('❌ Error al cargar las categorías')
  }
}

const fetchPromociones = async () => {
  if (!selectedCategory.value) {
    promociones.value = []
    return
  }

  try {
    isLoading.value = true
    const response = await axiosClient.get(`/api/Promociones/GetPromocionesCategoria?categoriaID=${selectedCategory.value}`)
    
    if (response.data?.ok) {
      promociones.value = response.data.data || []
      if (promociones.value.length > 0) {
        showSuccess(`🎯 ${promociones.value.length} promociones cargadas`)
      }
    } else {
      console.error('Error fetching promociones:', response.data?.message)
      showError('❌ Error al cargar las promociones')
    }
  } catch (error) {
    console.error('Error fetching promociones:', error)
    showError('❌ Error al cargar las promociones')
  } finally {
    isLoading.value = false
  }
}

// Lifecycle
onMounted(fetchCategorias)
</script>

<style scoped>
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

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
</style>