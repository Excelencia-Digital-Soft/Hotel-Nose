<template>
  <div class="min-h-screen bg-gradient-to-br from-slate-900 via-purple-900 to-slate-900 p-6">
    <!-- Glass Container -->
    <div class="max-w-7xl mx-auto">
      <!-- Header -->
      <div class="glass-card mb-8">
        <h1 class="text-3xl font-bold text-white lexend-exa mb-2">Gestión de Habitaciones</h1>
        <p class="text-white/70">Crear y administrar habitaciones del hotel</p>
      </div>

      <!-- Main Content Grid -->
      <div class="grid lg:grid-cols-2 gap-8 mb-8">
        <!-- Room Form Section -->
        <div class="glass-card">
          <div class="flex items-center justify-between mb-6">
            <h2 class="text-xl font-bold text-white lexend-exa">
              {{ isUpdateMode ? 'Actualizar Habitación' : 'Crear Habitación' }}
            </h2>
            <div v-if="isUpdateMode" class="text-sm text-white/70">
              ID: {{ roomToUpdate?.habitacionId }}
            </div>
          </div>

          <form @submit.prevent="submitForm" class="space-y-6">
            <!-- Room Name -->
            <div class="space-y-2">
              <label class="block text-white font-medium text-sm" for="roomName">
                Nombre de la Habitación
              </label>
              <input
                v-model="formData.roomName"
                id="roomName"
                type="text"
                placeholder="Ej: Suite Presidencial 101"
                class="glass-input w-full"
                required
              />
            </div>

            <!-- Category Selection -->
            <div class="space-y-3">
              <label class="block text-white font-medium text-sm"> Categoría de Habitación </label>
              <div class="grid grid-cols-2 gap-3">
                <button
                  v-for="categoria in categorias"
                  :key="categoria.categoriaId"
                  @click.prevent="handleCategorySelect(categoria.categoriaId)"
                  :class="{
                    'glass-button-active': formData.selectedCategory === categoria.categoriaId,
                    'glass-button': formData.selectedCategory !== categoria.categoriaId,
                  }"
                  class="py-3 px-4 rounded-xl transition-all duration-300 hover:scale-105"
                >
                  {{ categoria.nombreCategoria }}
                </button>
              </div>
            </div>

            <!-- Image Upload -->
            <div class="space-y-3">
              <label class="block text-white font-medium text-sm">
                Imágenes de la Habitación
              </label>
              <div class="glass-upload-area">
                <input
                  type="file"
                  multiple
                  accept="image/*"
                  @change="handleImageUpload"
                  class="hidden"
                  ref="fileInput"
                  id="imageUpload"
                />
                <label for="imageUpload" class="cursor-pointer block">
                  <div class="text-center py-6">
                    <i class="pi pi-cloud-upload text-3xl text-white/70 mb-3 block"></i>
                    <p class="text-white/90 font-medium">Haz clic para subir imágenes</p>
                    <p class="text-white/60 text-sm mt-1">PNG, JPG hasta 10MB cada una</p>
                  </div>
                </label>
              </div>
            </div>

            <!-- Image Previews -->
            <div v-if="formData.imagePreviews.length" class="space-y-3">
              <label class="block text-white font-medium text-sm">
                Vista Previa ({{ formData.imagePreviews.length }} imagen{{
                  formData.imagePreviews.length > 1 ? 'es' : ''
                }})
              </label>
              <div class="grid grid-cols-3 gap-4">
                <div
                  v-for="(image, index) in formData.imagePreviews"
                  :key="index"
                  class="relative group"
                >
                  <img
                    :src="image.url"
                    class="w-full h-24 object-cover rounded-xl border border-white/20"
                  />
                  <button
                    @click.prevent="removeImage(index, image.id)"
                    class="absolute -top-2 -right-2 bg-red-500/90 hover:bg-red-600 text-white rounded-full w-6 h-6 flex items-center justify-center text-xs opacity-0 group-hover:opacity-100 transition-opacity duration-200"
                  >
                    ✕
                  </button>
                </div>
              </div>
            </div>

            <!-- Action Buttons -->
            <div class="flex gap-3 pt-4">
              <button
                type="submit"
                :disabled="!isFormValid || isSubmitting"
                class="flex-1 glass-button-primary py-3 px-6 rounded-xl font-medium transition-all duration-300 hover:scale-105 disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100"
              >
                <i v-if="isSubmitting" class="pi pi-spin pi-spinner mr-2"></i>
                {{ isUpdateMode ? 'Actualizar Habitación' : 'Crear Habitación' }}
              </button>
              <button
                v-if="isUpdateMode"
                @click.prevent="resetFormAndClear"
                type="button"
                class="glass-button py-3 px-6 rounded-xl font-medium transition-all duration-300 hover:scale-105"
              >
                Cancelar
              </button>
            </div>
          </form>
        </div>

        <!-- Characteristics Section -->
        <div class="glass-card">
          <h3 class="text-lg font-bold text-white lexend-exa mb-4">
            Características de la Habitación
          </h3>
          <CaracteristicasComponent
            :trigger="triggerSignal"
            :idHabitacion="habitacionID"
            :listaCaracteristicas="listaCaracteristicas"
            @caracteristicasActualizadas="onCaracteristicasActualizadas"
          />
        </div>
      </div>

      <!-- Rooms List -->
      <div class="glass-card">
        <div class="flex items-center justify-between mb-6">
          <h2 class="text-xl text-white lexend-exa font-bold">
            Habitaciones Registradas ({{ habitacionesFiltradas.length }})
          </h2>
          <div class="text-sm text-white/70">
            Total: {{ habitacionesFiltradas.length }} de {{ habitaciones.length }} habitacion{{
              habitaciones.length !== 1 ? 'es' : ''
            }}
          </div>
        </div>

        <!-- Search Input -->
        <div class="mb-6">
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <i class="pi pi-search text-white/40"></i>
            </div>
            <input
              v-model="searchQuery"
              type="text"
              placeholder="Buscar habitaciones por nombre..."
              class="glass-input w-full pl-10 pr-12"
            />
            <button
              v-if="searchQuery"
              @click="clearSearch"
              class="absolute inset-y-0 right-0 pr-3 flex items-center text-white/40 hover:text-white/70 transition-colors"
            >
              <i class="pi pi-times"></i>
            </button>
          </div>
          <p
            v-if="searchQuery && habitacionesFiltradas.length === 0"
            class="text-white/50 text-sm mt-2"
          >
            No se encontraron habitaciones que coincidan con "{{ searchQuery }}"
          </p>
        </div>

        <div v-if="habitaciones.length === 0" class="text-center py-12">
          <i class="pi pi-home text-4xl text-white/50 mb-4 block"></i>
          <p class="text-white/70 text-lg">No hay habitaciones registradas</p>
          <p class="text-white/50 text-sm">Crea la primera habitación usando el formulario</p>
        </div>

        <div
          v-else-if="searchQuery && habitacionesFiltradas.length === 0"
          class="text-center py-12"
        >
          <i class="pi pi-search text-4xl text-white/50 mb-4 block"></i>
          <p class="text-white/70 text-lg">No se encontraron habitaciones</p>
          <p class="text-white/50 text-sm">Intenta con otros términos de búsqueda</p>
        </div>

        <div v-else class="grid lg:grid-cols-2 xl:grid-cols-3 gap-4">
          <div
            v-for="habitacion in habitacionesFiltradas"
            :key="habitacion.habitacionId"
            class="glass-room-card group"
          >
            <div class="p-6">
              <div class="flex items-start justify-between mb-4">
                <div>
                  <h3 class="text-lg font-semibold text-white mb-1">
                    {{ habitacion.nombreHabitacion }}
                  </h3>
                  <p class="text-white/60 text-sm">ID: {{ habitacion.habitacionId }}</p>
                </div>
                <span class="glass-badge" v-if="habitacion.categoriaId">
                  {{ getCategoryName(habitacion.categoriaId) }}
                </span>
              </div>

              <!-- Room Image Preview -->
              <div v-if="habitacion.imagenes && habitacion.imagenes.length > 0" class="mb-4">
                <div class="w-full h-32 bg-white/10 rounded-lg overflow-hidden">
                  <img
                    :src="getFirstImageUrl(habitacion.imagenes)"
                    class="w-full h-full object-cover"
                    :alt="habitacion.nombreHabitacion"
                  />
                </div>
              </div>

              <div class="grid grid-cols-3 gap-2">
                <button
                  @click="startUpdateRoom(habitacion)"
                  class="glass-action-button text-blue-400 hover:text-blue-300"
                >
                  <i class="pi pi-pencil text-sm"></i>
                  <span class="text-xs mt-1">Editar</span>
                </button>
                <button
                  @click="showInventoryRoom(habitacion)"
                  class="glass-action-button text-green-400 hover:text-green-300"
                >
                  <i class="pi pi-list text-sm"></i>
                  <span class="text-xs mt-1">Inventario</span>
                </button>
                <button
                  @click="openDeleteRoom(habitacion)"
                  class="glass-action-button text-red-400 hover:text-red-300"
                >
                  <i class="pi pi-trash text-sm"></i>
                  <span class="text-xs mt-1">Eliminar</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Delete Confirmation Modal -->
    <div
      v-if="showDeleteModal"
      class="fixed inset-0 flex items-center justify-center bg-black/70 backdrop-blur-sm z-50"
    >
      <div class="glass-modal max-w-md w-full mx-4">
        <div class="p-6 text-center">
          <div
            class="w-16 h-16 bg-red-500/20 rounded-full flex items-center justify-center mx-auto mb-4"
          >
            <i class="pi pi-exclamation-triangle text-2xl text-red-400"></i>
          </div>
          <h3 class="text-xl font-bold text-white mb-2">Confirmar Eliminación</h3>
          <p class="text-white/70 mb-6">
            ¿Estás seguro de eliminar la habitación
            <span class="font-semibold text-white">{{ roomToDelete?.nombreHabitacion }}</span
            >? Esta acción no se puede deshacer.
          </p>
          <div class="flex gap-3">
            <button
              @click="deleteRoom(roomToDelete.habitacionId)"
              class="flex-1 bg-red-500/90 hover:bg-red-600 text-white py-3 px-4 rounded-xl font-medium transition-all duration-300"
            >
              Eliminar
            </button>
            <button
              @click="openDeleteRoom"
              class="flex-1 glass-button py-3 px-4 rounded-xl font-medium transition-all duration-300"
            >
              Cancelar
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Inventory Modal -->
    <div
      v-if="showInventoryModal"
      class="fixed inset-0 flex items-center justify-center bg-black/70 backdrop-blur-sm z-50"
    >
      <ModalInventory :room="roomToInventory" @close-modal="showInventoryRoom" />
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted, computed, watch } from 'vue'
  import axiosClient from '../axiosClient'
  import ModalInventory from '../components/ModalInventory.vue'
  import { useAuthStore } from '../store/auth.js'
  import { fetchImagesAndIds } from '../services/imageService'
  import CaracteristicasComponent from '../components/CaracteristicasComponent.vue'
  import { useRoomCreate } from '../composables/useRoomCreate'
  import { useToast } from 'primevue/usetoast'
  import habitacionService from '../services/habitacionService'

  // Stores and composables
  const authStore = useAuthStore()
  const toast = useToast()
  const {
    formData,
    isUpdateMode,
    roomToUpdate,
    removedImageIds,
    isSubmitting,
    isLoadingImages,
    isFormValid,
    initializeUpdateMode,
    resetForm,
    handleImagePreview,
    removeImage,
    selectCategory,
    showSuccess,
    showError,
  } = useRoomCreate()

  // Local reactive data
  const habitaciones = ref([])
  const categorias = ref([])
  const showDeleteModal = ref(false)
  const showInventoryModal = ref(false)
  const roomToDelete = ref(null)
  const roomToInventory = ref(null)
  const triggerSignal = ref(false)
  const habitacionID = ref(null)
  const listaCaracteristicas = ref(null)
  const fileInput = ref(null)
  const searchQuery = ref('')

  // Computed properties
  const habitacionesFiltradas = computed(() => {
    if (!searchQuery.value.trim()) {
      return habitaciones.value
    }

    const query = searchQuery.value.toLowerCase()
    return habitaciones.value.filter((habitacion) =>
      habitacion.nombreHabitacion.toLowerCase().includes(query)
    )
  })

  // Modal handlers
  const openDeleteRoom = (room) => {
    if (room) {
      roomToDelete.value = room
    }
    showDeleteModal.value = !showDeleteModal.value
  }

  const showInventoryRoom = (room) => {
    roomToInventory.value = room
    showInventoryModal.value = !showInventoryModal.value
  }

  // Helper methods for template
  const getCategoryName = (categoriaId) => {
    const categoria = categorias.value.find((c) => c.categoriaId === categoriaId)
    return categoria ? categoria.nombreCategoria : 'Sin categoría'
  }

  const getFirstImageUrl = (imagenes) => {
    return imagenes && imagenes.length > 0 ? imagenes[0] : '/placeholder-room.jpg'
  }

  // Search functionality
  const clearSearch = () => {
    searchQuery.value = ''
  }
  const triggerChildFunction = (habID) => {
    habitacionID.value = habID
    triggerSignal.value = !triggerSignal.value
  }
  // Fetch rooms and categories
  const fetchHabitaciones = async () => {
    const institucionID = authStore.institucionID
    if (!institucionID) {
      console.warn('InstitucionID is not available. Please ensure the user is logged in.')
      return
    }

    try {
      const response = await habitacionService.getRooms()
      if (response && response.isSuccess) {
        habitaciones.value = response.data || []
        // need mapper categoriaHabitacionId to categoriaId
        habitaciones.value = habitaciones.value.map((habitacion) => ({
          ...habitacion,
          categoriaId: habitacion.categoriaHabitacionId,
        }))
      } else {
        console.error('Failed to fetch rooms:', response?.message)
        showError(response?.message || 'Error al cargar las habitaciones')
      }
    } catch (error) {
      console.error('Error fetching rooms:', error)
      showError('Error al cargar las habitaciones')
    }
  }

  const fetchCategorias = async () => {
    const institucionID = authStore.institucionID
    if (!institucionID) {
      console.warn('InstitucionID is not available. Please ensure the user is logged in.')
      return
    }

    try {
      const response = await habitacionService.getCategories()
      if (response && response.isSuccess) {
        categorias.value = response.data || []
        await fetchHabitaciones()
      } else {
        console.error('Failed to fetch categories:', response?.message)
        showError(response?.message || 'Error al cargar las categorías')
      }
    } catch (error) {
      console.error('Error fetching categories:', error)
      showError('Error al cargar las categorías')
    }
  }

  // Image handling
  const handleImageUpload = (event) => {
    const files = event.target.files
    if (files && files.length > 0) {
      handleImagePreview(files)
    }
  }
  const startUpdateRoom = async (room) => {
    listaCaracteristicas.value = room.caracteristicas
    habitacionID.value = room.habitacionId

    // Initialize update mode with composable
    initializeUpdateMode(room)

    // Load images for the room
    const imagenIds = room.imagenes || []
    if (imagenIds.length > 0) {
      try {
        isLoadingImages.value = true
        const imagenes = await fetchImagesAndIds(imagenIds)
        formData.value.imagePreviews = imagenes
      } catch (error) {
        console.error('Error fetching images:', error)
        showError('Error al cargar las imágenes de la habitación')
      } finally {
        isLoadingImages.value = false
      }
    }
  }

  // Submit form (create or update)
  const submitForm = async () => {
    const institucionID = authStore.institucionID
    if (!institucionID) {
      showError('No se pudo obtener el ID de la institución')
      return
    }

    if (!isFormValid.value) {
      showError('Por favor completa todos los campos requeridos')
      return
    }

    isSubmitting.value = true

    try {
      if (isUpdateMode.value) {
        await updateRoom(roomToUpdate.value.habitacionId)
      } else {
        await createRoom()
      }
    } catch (error) {
      console.error('Error in form submission:', error)
    } finally {
      isSubmitting.value = false
    }
  }

  // Create a new room
  const createRoom = async () => {
    const roomData = {
      institucionId: authStore.institucionID,
      nombreHabitacion: formData.value.roomName,
      categoriaHabitacionId: formData.value.selectedCategory,
      imagenes: formData.value.imageFiles,
    }

    try {
      const response = await habitacionService.createRoom(roomData)

      if (response && response.isSuccess) {
        const newRoomId = response.data?.habitacionId || response.data?.id
        if (newRoomId) {
          triggerChildFunction(newRoomId)
        } else {
          // If no room ID, reset immediately since no características to save
          resetFormAndClear()
        }
        showSuccess(response.message || 'Habitación creada exitosamente')
        await fetchHabitaciones()
      } else {
        showError(response?.message || 'Error al crear la habitación')
      }
    } catch (error) {
      console.error('Error al crear habitación:', error)
      showError(error.response?.data?.message || 'Hubo un error al crear la habitación')
    }
  }

  // Update a room
  const updateRoom = async (habitacionID) => {
    const updateData = {
      nombreHabitacion: formData.value.roomName,
      categoriaId: formData.value.selectedCategory,
      usuarioId: 0,
      nuevasImagenes: formData.value.imageFiles,
      imagenesEliminadas: removedImageIds.value,
    }

    try {
      const response = await habitacionService.updateRoom(habitacionID, updateData)

      if (response && response.isSuccess) {
        showSuccess(response.message || 'Habitación actualizada exitosamente')
        triggerChildFunction(habitacionID)
        // Note: resetFormAndClear() will be called after características are saved
        await fetchHabitaciones()
      } else {
        showError(response?.message || 'Error al actualizar la habitación')
      }
    } catch (error) {
      console.error('Error updating room:', error)
      showError(error.response?.data?.message || 'Error al actualizar la habitación')
    }
  }
  // Enhanced reset form
  const resetFormAndClear = () => {
    resetForm()
    listaCaracteristicas.value = null
    habitacionID.value = null
    if (fileInput.value) {
      fileInput.value.value = ''
    }
  }

  // Handle características updated event
  const onCaracteristicasActualizadas = () => {
    // Reset form only after características have been successfully saved
    resetFormAndClear()
  }

  // Delete a room
  const deleteRoom = async (idHabitacion) => {
    try {
      const response = await habitacionService.deleteRoom(idHabitacion)

      if (response && response.isSuccess) {
        showSuccess(response.message || 'Habitación eliminada correctamente')
        showDeleteModal.value = false
        roomToDelete.value = null
      } else {
        showError(response?.message || 'Error al eliminar la habitación')
      }
    } catch (error) {
      console.error('Error deleting room:', error)
      showError(error.response?.data?.message || 'Error al eliminar la habitación')
    }
  }

  // Override the composable selectCategory to handle local state
  const handleCategorySelect = (categoriaId) => {
    selectCategory(categoriaId)
  }

  // Fetch rooms and categories on component mount
  onMounted(async () => {
    await fetchCategorias()
    await fetchHabitaciones()
  })
</script>

<style scoped>
  /* Add custom styles if needed */
</style>
