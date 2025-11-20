<template>
  <div class="min-h-screen bg-gradient-to-br from-slate-900 via-purple-900 to-slate-900 p-6">
    <!-- Glass Container -->
    <div class="max-w-6xl mx-auto">
      <!-- Header -->
      <div class="glass-card mb-8">
        <h1 class="text-3xl font-bold text-white lexend-exa mb-2">Gestor de Características</h1>
        <p class="text-white/70">Crear y administrar características para las habitaciones</p>
      </div>

      <!-- Main Content Grid -->
      <div class="grid lg:grid-cols-2 gap-8 mb-8 relative z-20">
        <!-- Form Section -->
        <div class="glass-card relative z-30">
          <div class="flex items-center justify-between mb-6">
            <h2 class="text-xl font-bold text-white lexend-exa">
              {{ isEditMode ? 'Editar Característica' : 'Nueva Característica' }}
            </h2>
            <div v-if="isEditMode" class="text-sm text-white/70">
              ID: {{ formData.caracteristicaId }}
            </div>
          </div>

          <form @submit.prevent="submitForm" class="space-y-6">
            <!-- Name Input -->
            <div class="space-y-2">
              <label class="block text-white font-medium text-sm">
                Nombre de la Característica *
              </label>
              <input
                v-model="formData.nombre"
                type="text"
                placeholder="Ej: WiFi, Aire Acondicionado, TV"
                class="glass-input w-full"
                :class="{ 'border-red-400': isNameDuplicate && formData.nombre }"
                required
              />
              <p v-if="isNameDuplicate && formData.nombre" class="text-red-400 text-sm">
                Ya existe una característica con este nombre
              </p>
            </div>

            <!-- Description Input -->
            <div class="space-y-2">
              <label class="block text-white font-medium text-sm"> Descripción </label>
              <textarea
                v-model="formData.descripcion"
                placeholder="Descripción opcional de la característica"
                rows="3"
                class="glass-input w-full resize-none"
              ></textarea>
            </div>

            <!-- Icon Upload -->
            <div class="space-y-3">
              <label class="block text-white font-medium text-sm">
                Ícono de la Característica
              </label>
              <div class="glass-upload-area">
                <input
                  type="file"
                  accept="image/*"
                  @change="handleImageUploadEvent"
                  class="hidden"
                  ref="fileInput"
                  id="iconUpload"
                />
                <label for="iconUpload" class="cursor-pointer block">
                  <div class="text-center py-4">
                    <i class="pi pi-image text-2xl text-white/70 mb-2 block"></i>
                    <p class="text-white/90 font-medium text-sm">Subir ícono</p>
                    <p class="text-white/60 text-xs mt-1">PNG, JPG, SVG hasta 5MB</p>
                  </div>
                </label>
              </div>
            </div>

            <!-- Action Buttons -->
            <div class="flex gap-3 pt-4">
              <button
                type="submit"
                :disabled="!isFormValid || isSubmitting || isNameDuplicate"
                class="flex-1 glass-button-primary py-3 px-6 rounded-xl font-medium transition-all duration-300 hover:scale-105 disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100"
              >
                <i v-if="isSubmitting" class="pi pi-spin pi-spinner mr-2"></i>
                {{ isEditMode ? 'Actualizar Característica' : 'Crear Característica' }}
              </button>
              <button
                v-if="isEditMode"
                @click.prevent="cancelEdit"
                type="button"
                class="glass-button py-3 px-6 rounded-xl font-medium transition-all duration-300 hover:scale-105"
              >
                Cancelar
              </button>
            </div>
          </form>
        </div>

        <!-- Image Preview Section -->
        <div class="glass-card relative z-20">
          <h3 class="text-lg font-bold text-white lexend-exa mb-4">Vista Previa</h3>
          <div class="aspect-square w-full max-w-sm mx-auto">
            <div class="glass-image-preview">
              <img
                :src="imagePreview"
                alt="Vista previa del ícono"
                class="w-full h-full object-contain rounded-xl"
              />
            </div>
          </div>
          <div v-if="formData.nombre" class="mt-4 text-center">
            <div class="glass-badge inline-block">
              <i class="pi pi-tag mr-2"></i>
              {{ formData.nombre }}
            </div>
          </div>
        </div>
      </div>

      <!-- Características List -->
      <div class="glass-card relative z-10">
        <div class="flex items-center justify-between mb-6">
          <h2 class="text-xl text-white lexend-exa font-bold">
            Características Registradas ({{ caracteristicas.length }})
          </h2>
          <div class="text-sm text-white/70">
            Total: {{ caracteristicas.length }} característica{{
              caracteristicas.length !== 1 ? 's' : ''
            }}
          </div>
        </div>

        <div v-if="caracteristicas.length === 0" class="text-center py-12">
          <i class="pi pi-star text-4xl text-white/50 mb-4 block"></i>
          <p class="text-white/70 text-lg">No hay características registradas</p>
          <p class="text-white/50 text-sm">Crea la primera característica usando el formulario</p>
        </div>

        <div v-else class="space-y-3">
          <div
            v-for="caracteristica in caracteristicas"
            :key="caracteristica.caracteristicaId"
            class="glass-caracteristica-card group"
          >
            <div class="flex items-center justify-between p-4">
              <!-- Característica Info -->
              <div class="flex items-center space-x-4 flex-1">
                <div class="flex-shrink-0">
                  <div
                    class="w-12 h-12 bg-white/10 rounded-xl flex items-center justify-center overflow-hidden"
                  >
                    <img
                      v-if="caracteristica.icono"
                      :src="caracteristica.icono"
                      :alt="caracteristica.nombre"
                      class="w-8 h-8 object-contain"
                    />
                    <i v-else class="pi pi-star text-white/60 text-lg"></i>
                  </div>
                </div>
                <div class="flex-1 min-w-0">
                  <h3 class="text-white font-semibold text-lg truncate">
                    {{ caracteristica.nombre }}
                  </h3>
                  <p v-if="caracteristica.descripcion" class="text-white/70 text-sm truncate">
                    {{ caracteristica.descripcion }}
                  </p>
                  <p v-else class="text-white/50 text-sm italic">Sin descripción</p>
                  <p class="text-white/40 text-xs mt-1">
                    ID: {{ caracteristica.caracteristicaId }}
                  </p>
                </div>
              </div>

              <!-- Action Buttons -->
              <div class="flex items-center space-x-2 flex-shrink-0">
                <button
                  @click="startEdit(caracteristica)"
                  class="glass-action-button text-blue-400 hover:text-blue-300 w-10 h-10"
                  title="Editar característica"
                >
                  <i class="pi pi-pencil text-sm"></i>
                </button>
                <button
                  @click="deleteCaracteristica(caracteristica)"
                  class="glass-action-button text-red-400 hover:text-red-300 w-10 h-10"
                  title="Eliminar característica"
                >
                  <i class="pi pi-trash text-sm"></i>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
  import { ref, onMounted, onUnmounted } from 'vue'
  import { useCaracteristicas } from '../composables/useCaracteristicas.js'
  import { CaracteristicasService } from '../services/caracteristicasService.ts'

  // Composable
  const {
    formData,
    isEditMode,
    isSubmitting,
    isLoadingImage,
    imagePreview,
    caracteristicas,
    isFormValid,
    isNameDuplicate,
    resetForm,
    handleImageUpload,
    startEdit,
    cancelEdit,
    validateForm,
    showSuccess,
    showError,
    confirmDelete,
  } = useCaracteristicas()

  // Refs
  const fileInput = ref(null)

  // Lifecycle
  onMounted(() => {
    fetchCaracteristicas()
  })

  onUnmounted(() => {
    // Cleanup blob URLs to prevent memory leaks
    CaracteristicasService.cleanupBlobUrls(caracteristicas.value)
  })

  // Methods
  const handleImageUploadEvent = (event) => {
    const file = event.target.files[0]
    if (file) {
      // Validate file size (5MB max)
      if (file.size > 5 * 1024 * 1024) {
        showError('El archivo es demasiado grande. Máximo 5MB permitido.')
        return
      }

      // Validate file type
      if (!file.type.startsWith('image/')) {
        showError('Solo se permiten archivos de imagen.')
        return
      }

      handleImageUpload(file)
    }
  }

  const resetFormAndClear = () => {
    resetForm()
    if (fileInput.value) {
      fileInput.value.value = ''
    }
  }

  // Form submission
  const submitForm = async () => {
    if (!validateForm()) return

    isSubmitting.value = true

    try {
      if (isEditMode.value) {
        await updateCaracteristica()
      } else {
        await createCaracteristica()
      }
    } catch (error) {
      console.error('Error in form submission:', error)
    } finally {
      isSubmitting.value = false
    }
  }

  // Create característica
  const createCaracteristica = async () => {
    try {
      const caracteristicaData = {
        nombre: formData.value.nombre,
        descripcion: formData.value.descripcion,
        icono: formData.value.icono,
      }

      const response = await CaracteristicasService.createCaracteristica(caracteristicaData)

      if (response && response.isSuccess) {
        showSuccess('Característica creada exitosamente')
        resetFormAndClear()
        await fetchCaracteristicas()
      } else {
        showError(response?.message || 'Error al crear la característica')
      }
    } catch (error) {
      console.error('Error creating característica:', error)
      showError('Error al crear la característica')
    }
  }

  // Update característica
  const updateCaracteristica = async () => {
    try {
      const caracteristicaData = {
        nombre: formData.value.nombre,
        descripcion: formData.value.descripcion,
        icono: formData.value.icono,
      }

      const response = await CaracteristicasService.updateCaracteristica(
        formData.value.caracteristicaId,
        caracteristicaData
      )

      if (response && response.isSuccess) {
        showSuccess('Característica actualizada exitosamente')
        resetFormAndClear()
        await fetchCaracteristicas()
      } else {
        showError(response?.message || 'Error al actualizar la característica')
      }
    } catch (error) {
      console.error('Error updating característica:', error)
      showError('Error al actualizar la característica')
    }
  }

  // Delete característica
  const deleteCaracteristica = async (caracteristica) => {
    const confirmed = await confirmDelete(caracteristica)
    if (!confirmed) return

    try {
      const response = await CaracteristicasService.deleteCaracteristica(
        caracteristica.caracteristicaId
      )

      if (response && response.isSuccess) {
        showSuccess('Característica eliminada exitosamente')
        await fetchCaracteristicas()
      } else {
        showError(response?.message || 'Error al eliminar la característica')
      }
    } catch (error) {
      console.error('Error deleting característica:', error)
      showError('Error al eliminar la característica')
    }
  }

  // Fetch características
  const fetchCaracteristicas = async () => {
    try {
      // Cleanup previous blob URLs
      CaracteristicasService.cleanupBlobUrls(caracteristicas.value)

      const response = await CaracteristicasService.getCaracteristicas()

      if (response && response.isSuccess && response.data) {
        const caracteristicasWithImages =
          await CaracteristicasService.processCaracteristicasWithImages(response.data)
        caracteristicas.value = caracteristicasWithImages
      } else {
        showError(response?.message || 'Error al cargar las características')
      }
    } catch (error) {
      console.error('Error fetching características:', error)
      showError('Error al cargar las características')
    }
  }
</script>

<style scoped>
  .glass-image-preview {
    @apply bg-white/5 backdrop-blur-sm border border-white/20 rounded-xl p-4 aspect-square flex items-center justify-center;
    background: rgba(255, 255, 255, 0.05);
    backdrop-filter: blur(10px);
  }

  .glass-caracteristica-card {
    @apply bg-neutral-900/90 backdrop-blur-xl rounded-xl border border-white/20 shadow-lg hover:shadow-xl transition-all duration-300;
    background: linear-gradient(135deg, rgba(255, 255, 255, 0.1), rgba(255, 255, 255, 0.05));
    backdrop-filter: blur(20px);
  }

  .glass-caracteristica-card:hover {
    border-color: rgba(244, 63, 184, 0.3);
    box-shadow: 0 8px 32px rgba(244, 63, 184, 0.15);
  }
</style>