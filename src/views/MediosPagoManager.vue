<template>
  <div class="min-h-screen bg-gradient-to-br from-neutral-900 via-neutral-800 to-neutral-900 p-4 lg:p-6">
    <!-- Header Section -->
    <div class="glass-container mb-6 p-6 transform hover:scale-[1.02] transition-all duration-300">
      <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between space-y-4 lg:space-y-0">
        <div class="text-center lg:text-left">
          <div class="flex items-center justify-center lg:justify-start mb-2">
            <div class="bg-gradient-to-r from-primary-400 to-accent-400 p-3 rounded-full mr-3">
              <i class="pi pi-credit-card text-white text-2xl"></i>
            </div>
            <h1 class="text-3xl lg:text-4xl font-bold text-white">💳 Medios de Pago</h1>
          </div>
          <p class="text-gray-300 text-lg">Gestiona tarjetas y descuentos de manera fácil y eficiente 💰</p>
        </div>
        
        <!-- Quick Stats -->
        <div class="glass-card p-4">
          <div class="grid grid-cols-2 gap-4 text-center">
            <div class="transform hover:scale-110 transition-transform duration-200">
              <div class="bg-gradient-to-r from-primary-400 to-primary-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
                <span class="font-bold text-lg">{{ tarjetas?.length || 0 }}</span>
              </div>
              <p class="text-xs text-gray-300">Tarjetas</p>
            </div>
            <div class="transform hover:scale-110 transition-transform duration-200">
              <div class="bg-gradient-to-r from-secondary-400 to-secondary-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2">
                <span class="font-bold text-xs">{{ averagePercentage }}%</span>
              </div>
              <p class="text-xs text-gray-300">Promedio</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Create Card Button (Expandable) -->
    <div class="mb-6">
      <div 
        class="glass-container cursor-pointer transform transition-all duration-500 hover:scale-[1.02]"
        :class="{ 'p-4': !showCreateForm, 'p-6': showCreateForm }"
        @click="!showCreateForm && toggleCreateForm()"
      >
        <!-- Collapsed State -->
        <div v-if="!showCreateForm" class="flex items-center justify-center py-4">
          <div class="bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 p-4 rounded-full mr-4 animate-pulse">
            <i class="pi pi-plus text-white text-2xl"></i>
          </div>
          <div class="text-center">
            <h2 class="text-2xl font-bold text-white mb-1">{{ isEditMode ? '✏️ Editando Tarjeta' : '💳 Crear Nueva Tarjeta' }}</h2>
            <p class="text-gray-300">{{ isEditMode ? '¡Actualiza la información de la tarjeta!' : '¡Haz clic aquí para agregar un nuevo medio de pago!' }}</p>
          </div>
          <i class="pi pi-chevron-down text-white text-xl ml-4 animate-bounce"></i>
        </div>

        <!-- Expanded Form -->
        <div v-if="showCreateForm" class="space-y-6">
          <!-- Form Header -->
          <div class="flex items-center justify-between">
            <div class="flex items-center">
              <div class="bg-gradient-to-r from-primary-400 to-accent-400 p-3 rounded-full mr-3">
                <i :class="isEditMode ? 'pi pi-pencil' : 'pi pi-plus'" class="text-white text-xl"></i>
              </div>
              <h2 class="text-2xl font-bold text-white">
                {{ isEditMode ? '✏️ Editar Tarjeta' : '💳 Crear Nueva Tarjeta' }}
              </h2>
            </div>
            <div class="flex space-x-2">
             <button
  v-if="isEditMode"
  @click.stop="cancelEdit"
  class="glass-button px-4 py-2 text-white hover:text-red-300 transform hover:scale-105 transition-all"
>
                <i class="pi pi-times mr-2"></i>
                Cancelar
              </button>
              <button
  @click.stop="resetFormAndCollapse"
  class="glass-button px-4 py-2 text-white hover:text-gray-300 transform hover:scale-105 transition-all"
  title="Limpiar formulario y cerrar"
>
  <i class="pi pi-chevron-up mr-1"></i>
  <i class="pi pi-refresh text-xs"></i>
</button>
            </div>
          </div>

          <!-- Friendly Form -->
          <form @submit.prevent="handleSubmit" class="grid grid-cols-1 lg:grid-cols-2 gap-8">
            <!-- Left Column - Form Fields -->
            <!-- Name Field -->
<div class="glass-card p-4 hover:bg-white/15 transition-all duration-300">
  <label class="flex items-center text-white font-semibold mb-3">
    <i class="pi pi-tag text-primary-400 mr-2"></i>
    ¿Cómo se llama la tarjeta? *
  </label>

  <div class="flex items-center gap-3">
    <!-- Ícono fuera del input, fijo y alineado -->
    <div class="w-12 h-12 flex items-center justify-center">
      <i class="pi pi-credit-card text-accent-400 text-xl" aria-hidden="true"></i>
    </div>

    <!-- Input ocupa el resto -->
    <div class="flex-1">
      <input
        v-model="formData.nombre"
        type="text"
        class="glass-input w-full px-4 py-3 text-lg"
        placeholder="Ej: Visa, MasterCard 💳"
        :class="{ 'border-red-500 shake': isDuplicateName }"
        required
      />

      <div v-if="isDuplicateName" class="mt-2 p-2 bg-red-500/20 rounded-lg border border-red-500/30">
        <p class="text-red-300 text-sm flex items-center">
          <i class="pi pi-exclamation-triangle mr-2"></i>
          ¡Ups! Ya tienes una tarjeta con este nombre
        </p>
      </div>

      <div v-if="formData.nombre && !isDuplicateName" class="mt-2 p-2 bg-green-500/20 rounded-lg border border-green-500/30">
        <p class="text-green-300 text-sm flex items-center">
          <i class="pi pi-check mr-2"></i>
          ¡Perfecto! Este nombre está disponible
        </p>
      </div>
    </div>
  </div>
</div>
<!-- Percentage Field -->
<div class="glass-card p-4 hover:bg-white/15 transition-all duration-300">
  <label class="flex items-center text-white font-semibold mb-3">
    <i class="pi pi-percentage text-secondary-400 mr-2"></i>
    ¿Qué porcentaje de recargo tiene? *
  </label>

  <div class="flex items-center gap-3">
    <!-- Ícono fijo a la izquierda -->
    <div class="w-12 h-12 flex items-center justify-center">
      <i class="pi pi-chart-line text-green-400 text-xl animate-pulse"></i>
    </div>

    <!-- Input con símbolo % a la derecha -->
    <div class="flex-1 relative">
      <input
        v-model.number="formData.montoPorcentual"
        type="number"
        step="0.01"
        min="0"
        max="100"
        class="glass-input w-full px-4 py-3 text-lg pr-10"
        placeholder="0.00"
        required
      />
      <span class="absolute right-3 top-1/2 -translate-y-1/2 text-accent-400 font-bold text-lg"></span>
    </div>
  </div>

  <div class="mt-2 p-2 bg-blue-500/20 rounded-lg border border-blue-500/30">
    <p class="text-blue-300 text-sm flex items-center">
      <i class="pi pi-info-circle mr-2"></i>
      El porcentaje debe estar entre 0% y 100%
    </p>
  </div>
</div>
            <!-- Right Column - Preview Card -->
            <div class="space-y-6">
              <!-- Card Preview -->
              <div class="glass-card p-4 hover:bg-white/15 transition-all duration-300">
                <label class="flex items-center text-white font-semibold mb-3">
                  <i class="pi pi-eye text-accent-400 mr-2"></i>
                  👀 Vista previa de la tarjeta
                </label>
                
                <!-- Card Design -->
                <div class="relative">
                  <div class="bg-gradient-to-br from-primary-400 via-secondary-400 to-accent-400 p-6 rounded-xl text-white shadow-2xl transform hover:scale-105 transition-all duration-300">
                    <div class="flex justify-between items-start mb-4">
                      <div>
                        <h3 class="text-xl font-bold">{{ formData.nombre || 'Nombre de la tarjeta' }}</h3>
                        <p class="text-sm opacity-80">Medio de Pago</p>
                      </div>
                      <i class="pi pi-credit-card text-3xl opacity-80"></i>
                    </div>
                    
                    <div class="mt-6">
                      <p class="text-sm opacity-80">Recargo aplicado</p>
                      <p class="text-3xl font-bold">{{ formData.montoPorcentual || 0 }}</p>
                    </div>
                    
                    <div class="mt-4 flex justify-between items-center">
                      <span class="text-xs opacity-60">Hotel Management</span>
                      <div class="flex space-x-1">
                        <div class="w-2 h-2 bg-white rounded-full opacity-60"></div>
                        <div class="w-2 h-2 bg-white rounded-full opacity-40"></div>
                        <div class="w-2 h-2 bg-white rounded-full opacity-20"></div>
                      </div>
                    </div>
                  </div>
                </div>
                
                <div class="mt-3 p-3 bg-purple-500/20 rounded-lg border border-purple-500/30">
                  <p class="text-purple-300 text-sm flex items-center">
                    <i class="pi pi-sparkles mr-2"></i>
                    Esta es una vista previa de cómo se verá tu tarjeta ✨
                  </p>
                </div>
              </div>

              <!-- Submit Button -->
              <button
                type="submit"
                :disabled="!isFormValid || isSubmitting"
                class="w-full bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 
                       hover:from-primary-500 hover:via-secondary-500 hover:to-accent-500 
                       disabled:opacity-50 disabled:cursor-not-allowed
                       text-white font-bold py-4 px-6 rounded-xl text-lg
                       transition-all duration-300 transform hover:scale-105 
                       shadow-lg hover:shadow-xl"
              >
                <div class="flex items-center justify-center">
                  <i v-if="isSubmitting" class="pi pi-spinner pi-spin mr-3 text-xl"></i>
                  <i v-else :class="isEditMode ? 'pi pi-check' : 'pi pi-sparkles'" class="mr-3 text-xl"></i>
                  <span>
                    {{ isSubmitting ? '✨ Procesando...' : (isEditMode ? '🎉 ¡Actualizar Tarjeta!' : '🚀 ¡Crear Tarjeta!') }}
                  </span>
                </div>
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Cards Grid -->
    <div class="glass-container p-6">
      <div class="flex items-center justify-between mb-6">
        <div class="flex items-center">
          <i class="pi pi-list text-primary-400 text-xl mr-2"></i>
          <h3 class="text-xl font-bold text-white">
            {{ tarjetas.length > 0 ? `💳 ${tarjetas.length} tarjetas configuradas` : '💳 Medios de Pago' }}
          </h3>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="isLoading" class="text-center py-16">
        <div class="glass-card max-w-md mx-auto p-8">
          <div class="bg-gradient-to-r from-primary-400 to-accent-400 p-6 rounded-full mx-auto w-24 h-24 flex items-center justify-center mb-4">
            <i class="pi pi-spinner pi-spin text-white text-4xl"></i>
          </div>
          <h3 class="text-xl text-white font-bold mb-2">🔄 Cargando tarjetas...</h3>
          <p class="text-gray-300">Obteniendo la configuración actual</p>
        </div>
      </div>

      <!-- Cards Grid -->
      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div
          v-for="tarjeta in tarjetas"
          :key="tarjeta.tarjetaID"
          class="glass-card p-4 hover:bg-white/15 transition-all duration-300 group transform hover:scale-105"
        >
          <!-- Card Design -->
          <div class="relative mb-4">
            <div class="bg-gradient-to-br from-blue-400 via-purple-400 to-pink-400 p-6 rounded-xl text-white shadow-lg group-hover:shadow-xl transition-all duration-300">
              <div class="flex justify-between items-start mb-4">
                <div>
                  <h3 class="text-lg font-bold">{{ tarjeta.nombre }}</h3>
                  <p class="text-xs opacity-80">Medio de Pago</p>
                </div>
                <i class="pi pi-credit-card text-2xl opacity-80"></i>
              </div>
              
              <div class="mt-4">
                <p class="text-xs opacity-80">Recargo aplicado</p>
                <p class="text-2xl font-bold">{{ tarjeta.montoPorcentual }}%</p>
              </div>
              
              <div class="mt-4 flex justify-between items-center">
                <span class="text-xs opacity-60">Activa</span>
                <div class="flex space-x-1">
                  <div class="w-1.5 h-1.5 bg-white rounded-full opacity-60"></div>
                  <div class="w-1.5 h-1.5 bg-white rounded-full opacity-40"></div>
                  <div class="w-1.5 h-1.5 bg-white rounded-full opacity-20"></div>
                </div>
              </div>
            </div>
          </div>

          <!-- Card Info -->
          <div class="mb-4">
            <div class="flex items-center justify-between">
              <span class="text-gray-300 text-sm">Recargo:</span>
              <span class="text-accent-400 font-bold">{{ tarjeta.montoPorcentual }}%</span>
            </div>
          </div>

          <!-- Actions -->
          <div class="flex space-x-2 opacity-0 group-hover:opacity-100 transition-all duration-300">
            <button
              @click="startEdit(tarjeta)"
              class="flex-1 glass-button py-2 text-white hover:text-blue-300 transform hover:scale-105 transition-all"
            >
              <i class="pi pi-pencil mr-1"></i>
              ✏️ Editar
            </button>
            <button
              @click="handleDelete(tarjeta)"
              class="flex-1 glass-button py-2 text-white hover:text-red-300 transform hover:scale-105 transition-all"
            >
              <i class="pi pi-trash mr-1"></i>
              🗑️ Eliminar
            </button>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div v-if="!isLoading && tarjetas.length === 0" class="text-center py-16">
        <div class="glass-card max-w-md mx-auto p-8">
          <div class="mb-6">
            <div class="bg-gradient-to-r from-primary-400 to-accent-400 p-6 rounded-full mx-auto w-24 h-24 flex items-center justify-center mb-4">
              <i class="pi pi-credit-card text-white text-4xl"></i>
            </div>
            <h3 class="text-2xl text-white font-bold mb-2">💳 ¡No hay tarjetas configuradas!</h3>
            <p class="text-gray-300 mb-6">¡Es hora de agregar tu primera tarjeta para gestionar los medios de pago!</p>
          </div>
          
          <button
            @click="showCreateForm = true"
            class="bg-gradient-to-r from-primary-400 to-accent-400 hover:from-primary-500 hover:to-accent-500 
                   text-white font-bold py-3 px-6 rounded-lg transform hover:scale-105 transition-all duration-300"
          >
            <i class="pi pi-plus mr-2"></i>
            🚀 ¡Crear mi primera tarjeta!
          </button>
        </div>
      </div>
    </div>

    <!-- Cash Discount Section (Hidden for now, can be enabled) -->
    <div v-if="false" class="glass-container p-6 mt-6">
      <div class="flex items-center mb-4">
        <i class="pi pi-money-bill text-green-400 text-xl mr-2"></i>
        <h3 class="text-xl font-bold text-white">💵 Descuento en Efectivo</h3>
      </div>
      
      <div class="glass-card p-4 max-w-md">
        <div class="flex items-center justify-between mb-4">
          <span class="text-white font-semibold">Descuento aplicado:</span>
          <span class="text-green-400 font-bold text-xl">{{ descuentoEfectivo?.montoPorcentual || 0 }}%</span>
        </div>
        
        <button
          @click="editOrSaveEfectivo"
          class="w-full glass-button py-3 text-white hover:bg-white/20 transform hover:scale-105 transition-all"
        >
          <i :class="editingEfectivo ? 'pi pi-check' : 'pi pi-pencil'" class="mr-2"></i>
          {{ editingEfectivo ? 'Guardar' : 'Editar' }} Descuento
        </button>
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
const tarjetas = ref([])
const isLoading = ref(true)
const isSubmitting = ref(false)
const showCreateForm = ref(false)
const isEditMode = ref(false)

// Form data
const formData = ref({
  tarjetaID: null,
  nombre: '',
  montoPorcentual: null
})

// Cash discount (hidden for now)
const descuentoEfectivo = ref(null)
const editingEfectivo = ref(false)
const editingEfectivoMonto = ref('')

// Computed
const averagePercentage = computed(() => {
  if (!tarjetas.value || tarjetas.value.length === 0) return 0
  const total = tarjetas.value.reduce((sum, tarjeta) => sum + (tarjeta.montoPorcentual || 0), 0)
  return (total / tarjetas.value.length).toFixed(1)
})

const isDuplicateName = computed(() => {
  if (!tarjetas.value || !formData.value.nombre) return false
  
  return tarjetas.value.some((tarjeta) =>
    tarjeta.nombre.toLowerCase() === formData.value.nombre.toLowerCase() &&
    tarjeta.tarjetaID !== formData.value.tarjetaID
  )
})

const isFormValid = computed(() => {
  return formData.value.nombre.trim() !== '' &&
         formData.value.montoPorcentual !== null &&
         formData.value.montoPorcentual !== '' &&
         formData.value.montoPorcentual >= 0 &&
         formData.value.montoPorcentual <= 100 &&
         !isDuplicateName.value
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

const toggleCreateForm = () => {
  showCreateForm.value = !showCreateForm.value
  if (!showCreateForm.value && isEditMode.value) {
    cancelEdit()
  }
}

// Nueva función para resetear formulario y colapsar
const resetFormAndCollapse = () => {
  // Limpiar todos los campos del formulario
  formData.value = {
    tarjetaID: null,
    nombre: '',
    montoPorcentual: null
  }
  
  // Resetear estados del formulario
  isEditMode.value = false
  isSubmitting.value = false
  
  // Contraer/ocultar el formulario
  showCreateForm.value = false
  
  // Mostrar mensaje de confirmación
  toast.add({
    severity: 'info',
    summary: 'Formulario limpiado',
    detail: 'Formulario contraído y datos limpiados',
    life: 3000
  })
}

const resetForm = () => {
  formData.value = {
    tarjetaID: null,
    nombre: '',
    montoPorcentual: null
  }
  isEditMode.value = false
}

const startEdit = (tarjeta) => {
  isEditMode.value = true
  formData.value = {
    tarjetaID: tarjeta.tarjetaID,
    nombre: tarjeta.nombre,
    montoPorcentual: tarjeta.montoPorcentual
  }
  showCreateForm.value = true
}

const cancelEdit = () => {
  resetForm()
  showCreateForm.value = false
}

const handleSubmit = async () => {
  if (!isFormValid.value) return
  
  isSubmitting.value = true
  
  try {
    if (isEditMode.value) {
      // Update existing card
      await axiosClient.put(
        `/UpdateTarjeta?id=${formData.value.tarjetaID}&Nombre=${encodeURIComponent(formData.value.nombre)}&Monto=${formData.value.montoPorcentual}`
      )
      showSuccess('Tarjeta actualizada exitosamente!')
    } else {
      // Create new card
      await axiosClient.post(
        `/CrearRecargoTarjeta?Nombre=${encodeURIComponent(formData.value.nombre)}&Monto=${formData.value.montoPorcentual}&InstitucionID=${authStore.institucionID}`
      )
      showSuccess('Nueva tarjeta creada con éxito!')
    }
    
    resetForm()
    showCreateForm.value = false
    await fetchTarjetas()
    
  } catch (error) {
    console.error('Error:', error)
    showError(isEditMode.value ? 'Error al actualizar la tarjeta' : 'Error al crear la tarjeta')
  } finally {
    isSubmitting.value = false
  }
}

const handleDelete = async (tarjeta) => {
  const confirmed = await new Promise((resolve) => {
    confirm.require({
      message: `¿Estás seguro de eliminar la tarjeta "${tarjeta.nombre}"?`,
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
    // CORREGIDO: Usar el endpoint correcto y eliminar del array local
    const response = await axiosClient.delete(`/DeleteTarjeta/${tarjeta.tarjetaID}`)
    
    if (response.status === 200 || response.status === 204) {
      // Eliminar de la lista local inmediatamente
      tarjetas.value = tarjetas.value.filter(t => t.tarjetaID !== tarjeta.tarjetaID)
      showSuccess('Tarjeta eliminada correctamente')
    } else {
      throw new Error('Error en la respuesta del servidor')
    }
  } catch (error) {
    console.error('Error deleting card:', error)
    showError('Error al eliminar la tarjeta')
    // Recargar la lista en caso de error para sincronizar
    await fetchTarjetas()
  }
}

const fetchTarjetas = async () => {
  if (!authStore.institucionID) {
    showError('No se pudo obtener la información de la institución')
    return
  }

  try {
    isLoading.value = true
    const response = await axiosClient.get(`/GetTarjetas?InstitucionID=${authStore.institucionID}`)
    
    if (response.data?.ok) {
      tarjetas.value = response.data.data || []
    } else {
      console.error('Error fetching tarjetas:', response.data?.message)
      showError('Error al cargar las tarjetas')
    }
  } catch (error) {
    console.error('Error fetching tarjetas:', error)
    showError('Error al cargar las tarjetas')
  } finally {
    isLoading.value = false
  }
}

const fetchDescuentoEfectivo = async () => {
  try {
    const response = await axiosClient.get('/GetDescuentoEfectivo?institucionID=0')
    if (response.data) {
      descuentoEfectivo.value = response.data.data
    } else {
      descuentoEfectivo.value = null
    }
  } catch (error) {
    console.error('Error fetching descuento en efectivo:', error)
  }
}

const editOrSaveEfectivo = async () => {
  if (editingEfectivo.value) {
    // Save logic for cash discount
    if (editingEfectivoMonto.value < 0 || editingEfectivoMonto.value > 100) {
      showError('El porcentaje debe estar entre 0 y 100')
      return
    }

    try {
      if (descuentoEfectivo.value) {
        await axiosClient.put(
          `/UpdateDescuento?id=${descuentoEfectivo.value.descuentoID}&Monto=${editingEfectivoMonto.value}`
        )
      } else {
        await axiosClient.post(`/CrearDescuentoEfectivo?Monto=${editingEfectivoMonto.value}`)
      }
      await fetchDescuentoEfectivo()
      showSuccess('Descuento en efectivo actualizado')
    } catch (error) {
      console.error('Error updating/creating descuento en efectivo:', error)
      showError('Error al actualizar el descuento')
    }
    editingEfectivo.value = false
  } else {
    editingEfectivo.value = true
    editingEfectivoMonto.value = descuentoEfectivo.value ? descuentoEfectivo.value.montoPorcentual : 0
  }
}

// Lifecycle
onMounted(async () => {
  await Promise.all([
    fetchTarjetas(),
    fetchDescuentoEfectivo()
  ])
})
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