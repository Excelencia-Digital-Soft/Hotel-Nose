<template>
  <!-- Modal Overlay -->
  <div class="fixed inset-0 bg-black/60 backdrop-blur-xl z-50 flex items-center justify-center p-4">
    <div class="glass-container max-w-4xl w-full max-h-[90vh] overflow-hidden">
      <!-- Header -->
      <div class="bg-gradient-to-r from-primary-400 to-accent-400 p-6 rounded-t-3xl">
        <div class="flex items-center justify-between">
          <div class="flex items-center">
            <div class="bg-white/20 p-3 rounded-full mr-3">
              <i class="pi pi-pencil text-white text-xl"></i>
            </div>
            <div>
              <h2 class="text-2xl font-bold text-white">✏️ Editar Artículo</h2>
              <p class="text-white/80">Actualiza la información de tu artículo</p>
            </div>
          </div>
          <button
            @click="$emit('close')"
            class="bg-white/20 hover:bg-white/30 p-2 rounded-full transition-colors"
          >
            <i class="pi pi-times text-white text-lg"></i>
          </button>
        </div>
      </div>

      <!-- Form Content -->
      <div class="p-6 max-h-[calc(90vh-140px)] overflow-y-auto">
        <form @submit.prevent="handleSubmit" class="grid grid-cols-1 lg:grid-cols-2 gap-8">
          <!-- Left Column - Form Fields -->
          <div class="space-y-6">
            <!-- Name Field -->
            <div class="glass-card p-4 hover:bg-white/15 transition-all duration-300">
              <label class="flex items-center text-white font-semibold mb-3">
                <i class="pi pi-tag text-primary-400 mr-2"></i>
                Nombre del artículo *
              </label>
              <input
                v-model="formData.name"
                type="text"
                class="glass-input w-full px-4 py-3"
                placeholder="Nombre del artículo"
                :class="{ 'border-red-500 shake': isNameDuplicate }"
                required
              />
              <div v-if="isNameDuplicate" class="mt-2 p-2 bg-red-500/20 rounded-lg border border-red-500/30">
                <p class="text-red-300 text-sm flex items-center">
                  <i class="pi pi-exclamation-triangle mr-2"></i>
                  Ya existe un artículo con este nombre
                </p>
              </div>
            </div>

            <!-- Price Field -->
            <div class="glass-card p-4 hover:bg-white/15 transition-all duration-300">
              <label class="flex items-center text-white font-semibold mb-3">
                <i class="pi pi-dollar text-secondary-400 mr-2"></i>
                Precio *
              </label>
              <input
                v-model="formData.price"
                type="number"
                step="0.01"
                min="0"
                class="glass-input w-full px-4 py-3"
                placeholder="0.00"
                required
              />
            </div>

            <!-- Category Field -->
            <div class="glass-card p-4 hover:bg-white/15 transition-all duration-300">
              <label class="flex items-center text-white font-semibold mb-3">
                <i class="pi pi-list text-primary-400 mr-2"></i>
                Categoría *
              </label>
              <select
                v-model="formData.categoryId"
                class="glass-input w-full px-4 py-3"
                required
              >
                <option value="">Selecciona una categoría...</option>
                <option
                  v-for="category in categories"
                  :key="category.categoriaId"
                  :value="category.categoriaId"
                >
                  {{ category.nombreCategoria }}
                </option>
              </select>
            </div>
          </div>

          <!-- Right Column - Image Upload -->
          <div class="space-y-6">
            <!-- Image Upload -->
            <div class="glass-card p-4 hover:bg-white/15 transition-all duration-300">
              <label class="flex items-center text-white font-semibold mb-3">
                <i class="pi pi-camera text-accent-400 mr-2"></i>
                Imagen (opcional)
              </label>
              
              <!-- Image Preview -->
              <div 
                class="relative group cursor-pointer transform hover:scale-[1.02] transition-all duration-300"
                @click="$refs.imageInput.click()"
                @dragover.prevent="onDragOver"
                @dragleave.prevent="onDragLeave"
                @drop.prevent="onDrop"
                :class="{ 'border-primary-400 bg-primary-400/10': isDragging }"
              >
                <div class="glass-card p-2 mb-4 relative">
                  <!-- Loading Overlay -->
                  <div v-if="isLoadingImage" 
                       class="absolute inset-0 bg-black/60 backdrop-blur-sm rounded-lg z-10 flex items-center justify-center">
                    <div class="text-center">
                      <i class="pi pi-spinner pi-spin text-white text-3xl mb-3"></i>
                      <p class="text-white font-semibold">Cargando imagen...</p>
                    </div>
                  </div>
                  
                  <img
                    :src="imagePreview"
                    alt="Vista previa"
                    class="w-full h-64 object-cover rounded-lg"
                    :class="{ 'opacity-50': isLoadingImage }"
                  />
                  
                  <!-- Clear Image Button -->
                  <button
                    v-if="formData.image && !isLoadingImage"
                    @click.stop="clearImage"
                    class="absolute top-4 right-4 bg-red-500/80 hover:bg-red-500 p-2 rounded-full transition-colors z-20"
                  >
                    <i class="pi pi-times text-white"></i>
                  </button>
                  
                  <!-- Drag & Drop Overlay -->
                  <div v-if="isDragging && !isLoadingImage"
                       class="absolute inset-2 bg-primary-500/30 backdrop-blur-sm rounded-lg z-30 flex flex-col items-center justify-center border-2 border-dashed border-primary-400">
                    <div class="bg-primary-400/80 p-4 rounded-full mb-2 animate-bounce">
                      <i class="pi pi-upload text-white text-3xl"></i>
                    </div>
                    <p class="text-white font-bold text-lg">¡Suelta aquí tu imagen!</p>
                    <p class="text-primary-200 text-sm">Se cargará automáticamente</p>
                  </div>
                  
                  <div class="absolute inset-2 bg-gradient-to-t from-black/60 via-transparent to-transparent 
                             opacity-0 group-hover:opacity-100 transition-all duration-300 rounded-lg 
                             flex flex-col items-center justify-center"
                       :class="{ 'opacity-0': isLoadingImage || isDragging }">
                    <div class="bg-white/20 backdrop-blur-sm p-4 rounded-full mb-2 transform group-hover:scale-110 transition-transform">
                      <i class="pi pi-cloud-upload text-white text-2xl"></i>
                    </div>
                    <p class="text-white font-semibold">
                      {{ imagePreview.includes('sin-imagen') ? '¡Agregar imagen!' : '¡Cambiar imagen!' }}
                    </p>
                    <p class="text-gray-300 text-sm">Haz clic aquí o arrastra la imagen</p>
                  </div>
                </div>
                <input
                  ref="imageInput"
                  type="file"
                  accept="image/*"
                  @change="handleImageChange"
                  class="hidden"
                  :disabled="isLoadingImage"
                />
              </div>

              <!-- Upload/Change Button -->
              <button
                type="button"
                @click="$refs.imageInput.click()"
                :disabled="isLoadingImage"
                class="w-full glass-button py-3 text-white hover:bg-white/20 transform hover:scale-105 transition-all duration-300 disabled:opacity-50 disabled:cursor-not-allowed mb-2"
              >
                <i :class="isLoadingImage ? 'pi pi-spinner pi-spin' : 'pi pi-cloud-upload'" class="mr-2"></i>
                <span class="font-semibold">
                  {{ isLoadingImage ? '⏳ Cargando...' : (imagePreview.includes('sin-imagen') ? '🎨 Agregar Imagen' : '🔄 Cambiar Imagen') }}
                </span>
              </button>
              
              <!-- Clear Image Button -->
              <button
                v-if="formData.image && !isLoadingImage"
                type="button"
                @click="clearImage"
                class="w-full glass-button py-2 text-white hover:text-red-300 transform hover:scale-105 transition-all duration-300"
              >
                <i class="pi pi-trash mr-2"></i>
                <span class="font-semibold">🗑️ Remover Imagen</span>
              </button>
              
              <div class="mt-3 p-3 bg-blue-500/20 rounded-lg border border-blue-500/30">
                <p class="text-blue-300 text-sm flex items-center">
                  <i class="pi pi-info-circle mr-2"></i>
                  JPG, PNG o GIF • Máximo 5MB • ¡Que se vea genial! ✨
                </p>
              </div>
            </div>
          </div>
        </form>

        <!-- Form Actions -->
        <div class="flex items-center justify-end space-x-4 pt-6 border-t border-white/10 mt-8">
          <button
            type="button"
            @click="$emit('close')"
            class="glass-button py-3 px-6 text-white hover:text-red-300 transform hover:scale-105 transition-all"
          >
            <i class="pi pi-times mr-2"></i>
            Cancelar
          </button>
          
          <button
            @click="handleSubmit"
            :disabled="!isFormValid || isSubmitting"
            class="bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 
                   hover:from-primary-500 hover:via-secondary-500 hover:to-accent-500 
                   disabled:opacity-50 disabled:cursor-not-allowed
                   text-white font-bold py-3 px-6 rounded-lg 
                   transition-all duration-300 transform hover:scale-105"
          >
            <i :class="isSubmitting ? 'pi pi-spinner pi-spin' : 'pi pi-check'" class="mr-2"></i>
            {{ isSubmitting ? '✨ Actualizando...' : '🎉 Actualizar Artículo' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useToast } from 'primevue/usetoast'
import { useArticleCreate } from '../composables/useArticleCreate'
import { ArticleService } from '../services/ArticleService'
import { useAuthStore } from '../store/auth'

const emit = defineEmits(['close', 'updated'])
const toast = useToast()

const props = defineProps({
  article: {
    type: Object,
    required: true
  },
  categories: {
    type: Array,
    default: () => []
  }
})

// Use the composable but override some methods for modal-specific behavior
const {
  formData,
  isLoadingImage,
  imagePreview,
  isNameDuplicate,
  handleImageUpload,
  clearImage,
  validateForm,
  getFormattedData,
  loadArticleImageForEdit,
  showSuccess,
  showError
} = useArticleCreate()

// Store
const authStore = useAuthStore()

// Local state
const isSubmitting = ref(false)
const isDragging = ref(false)

// Computed
const isFormValid = computed(() => {
  const hasValidName = formData.value.name.trim() !== ''
  const hasValidPrice = formData.value.price !== '' && parseFloat(formData.value.price) > 0
  const hasValidCategory = formData.value.categoryId !== null
  return hasValidName && hasValidPrice && hasValidCategory
})

// Methods
const handleImageChange = (event) => {
  const file = event.target.files[0]
  if (file && handleImageUpload(file)) {
    // Image uploaded successfully
  }
}

// Drag & Drop handlers
const onDragOver = (event) => {
  event.preventDefault()
  isDragging.value = true
}

const onDragLeave = (event) => {
  event.preventDefault()
  isDragging.value = false
}

const onDrop = (event) => {
  event.preventDefault()
  isDragging.value = false
  
  const files = event.dataTransfer.files
  if (files.length > 0) {
    const file = files[0]
    if (file.type.startsWith('image/')) {
      handleImageUpload(file)
    } else {
      showError('Solo se permiten archivos de imagen')
    }
  }
}

const handleSubmit = async () => {
  if (!validateForm()) return
  
  isSubmitting.value = true
  
  try {
    const articleData = getFormattedData()
    
    // Update article details
    await ArticleService.updateArticle(
      formData.value.articuloId,
      articleData,
      authStore.auth?.usuarioID
    )
    
    // Update image if provided
    if (formData.value.image) {
      await ArticleService.updateArticleImage(
        formData.value.articuloId,
        formData.value.image
      )
    }
    
    showSuccess('🎉 ¡Artículo actualizado exitosamente!')
    emit('updated')
    
  } catch (error) {
    console.error('Error updating article:', error)
    showError('❌ Error al actualizar el artículo')
  } finally {
    isSubmitting.value = false
  }
}

// Initialize form with article data
onMounted(() => {
  // Set form data
  formData.value = {
    articuloId: props.article.articuloId,
    name: props.article.nombreArticulo,
    price: props.article.precio.toString(),
    categoryId: props.article.categoriaID || props.article.categoriaId,
    image: null
  }
  
  // Load article image
  loadArticleImageForEdit(props.article)
})
</script>

<style scoped>
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

/* Custom scrollbar */
::-webkit-scrollbar {
  width: 6px;
}

::-webkit-scrollbar-track {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 3px;
}

::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.3);
  border-radius: 3px;
}

::-webkit-scrollbar-thumb:hover {
  background: rgba(255, 255, 255, 0.5);
}
</style>