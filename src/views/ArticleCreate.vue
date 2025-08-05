<template>
  <div
    class="min-h-screen bg-gradient-to-br from-neutral-900 via-neutral-800 to-neutral-900 p-4 lg:p-6"
  >
    <!-- Welcome Header -->
    <div class="glass-container mb-6 p-6 transform hover:scale-[1.02] transition-all duration-300">
      <div
        class="flex flex-col lg:flex-row lg:items-center lg:justify-between space-y-4 lg:space-y-0"
      >
        <div class="text-center lg:text-left">
          <div class="flex items-center justify-center lg:justify-start mb-2">
            <div class="bg-gradient-to-r from-primary-400 to-accent-400 p-3 rounded-full mr-3">
              <i class="pi pi-box text-white text-2xl"></i>
            </div>
            <h1 class="text-3xl lg:text-4xl font-bold text-white">¡Gestiona tus Artículos!</h1>
          </div>
          <p class="text-gray-300 text-lg">
            Crea, edita y organiza tu inventario de manera fácil y divertida 🎉
          </p>
        </div>

        <!-- Quick Stats -->
        <div class="glass-card p-4">
          <div class="grid grid-cols-3 gap-4 text-center">
            <div class="transform hover:scale-110 transition-transform duration-200">
              <div
                class="bg-gradient-to-r from-primary-400 to-primary-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2"
              >
                <span class="font-bold text-lg">{{ articles?.length || 0 }}</span>
              </div>
              <p class="text-xs text-gray-300">Artículos</p>
            </div>
            <div class="transform hover:scale-110 transition-transform duration-200">
              <div
                class="bg-gradient-to-r from-secondary-400 to-secondary-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2"
              >
                <span class="font-bold text-xs">{{
                  formatCompactCurrency(calculateTotalValue() || 0)
                }}</span>
              </div>
              <p class="text-xs text-gray-300">Valor Total</p>
            </div>
            <div class="transform hover:scale-110 transition-transform duration-200">
              <div
                class="bg-gradient-to-r from-accent-400 to-accent-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2"
              >
                <span class="font-bold text-xs">{{
                  formatCompactCurrency(calculateAveragePrice() || 0)
                }}</span>
              </div>
              <p class="text-xs text-gray-300">Promedio</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Create Article Button (Expandable) -->
    <div class="mb-6">
      <div
        class="glass-container cursor-pointer transform transition-all duration-500 hover:scale-[1.02]"
        :class="{ 'p-4': !showCreateForm, 'p-6': showCreateForm }"
        @click="!showCreateForm && toggleCreateForm()"
      >
        <!-- Collapsed State -->
        <div v-if="!showCreateForm" class="flex items-center justify-center py-4">
          <div
            class="bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 p-4 rounded-full mr-4 animate-pulse"
          >
            <i class="pi pi-plus text-white text-2xl"></i>
          </div>
          <div class="text-center">
            <h2 class="text-2xl font-bold text-white mb-1">
              {{ isEditMode ? 'Editando Artículo' : 'Crear Nuevo Artículo' }}
            </h2>
            <p class="text-gray-300">
              {{
                isEditMode
                  ? '¡Actualiza la información de tu artículo!'
                  : '¡Haz clic aquí para agregar algo increíble!'
              }}
            </p>
          </div>
          <i class="pi pi-chevron-down text-white text-xl ml-4 animate-bounce"></i>
        </div>

        <!-- Expanded Form -->
        <div v-if="showCreateForm" class="space-y-6">
          <!-- Form Header -->
          <div class="flex items-center justify-between">
            <div class="flex items-center">
              <div class="bg-gradient-to-r from-primary-400 to-accent-400 p-3 rounded-full mr-3">
                <i
                  :class="isEditMode ? 'pi pi-pencil' : 'pi pi-plus'"
                  class="text-white text-xl"
                ></i>
              </div>
              <h2 class="text-2xl font-bold text-white">
                {{ isEditMode ? '✏️ Editar Artículo' : '🎨 Crear Nuevo Artículo' }}
              </h2>
            </div>
            <div class="flex space-x-2">
              <button
                v-if="isEditMode"
                @click="cancelEdit"
                class="glass-button px-4 py-2 text-white hover:text-red-300 transform hover:scale-105 transition-all"
              >
                <i class="pi pi-times mr-2"></i>
                Cancelar
              </button>
              <button
                @click="toggleCreateForm"
                class="glass-button px-4 py-2 text-white hover:text-gray-300 transform hover:scale-105 transition-all"
              >
                <i class="pi pi-chevron-up"></i>
              </button>
            </div>
          </div>

          <!-- Friendly Form -->
          <form @submit.prevent="handleSubmit" class="grid grid-cols-1 lg:grid-cols-2 gap-8">
            <!-- Left Column - Form Fields -->
            <div class="space-y-6">
              <!-- Name Field with Fun Design -->
              <div class="glass-card p-4 hover:bg-white/15 transition-all duration-300">
                <label class="flex items-center text-white font-semibold mb-3">
                  <i class="pi pi-tag text-primary-400 mr-2"></i>
                  ¿Cómo se llama tu artículo? *
                </label>
                <div class="relative">
                  <input
                    v-model="formData.name"
                    type="text"
                    class="glass-input w-full px-4 py-3 pl-12 text-lg"
                    placeholder="Ej: Hamburguesa Deliciosa 🍔"
                    :class="{ 'border-red-500 shake': isNameDuplicate }"
                    required
                  />
                </div>
                <div
                  v-if="isNameDuplicate"
                  class="mt-2 p-2 bg-red-500/20 rounded-lg border border-red-500/30"
                >
                  <p class="text-red-300 text-sm flex items-center">
                    <i class="pi pi-exclamation-triangle mr-2"></i>
                    ¡Ups! Ya tienes un artículo con este nombre
                  </p>
                </div>
                <div
                  v-if="formData.name && !isNameDuplicate"
                  class="mt-2 p-2 bg-green-500/20 rounded-lg border border-green-500/30"
                >
                  <p class="text-green-300 text-sm flex items-center">
                    <i class="pi pi-check mr-2"></i>
                    ¡Perfecto! Este nombre está disponible
                  </p>
                </div>
              </div>

              <!-- Price Field with Currency Animation -->
              <div class="glass-card p-4 hover:bg-white/15 transition-all duration-300">
                <label class="flex items-center text-white font-semibold mb-3">
                  <i class="pi pi-dollar text-secondary-400 mr-2"></i>
                  ¿Cuánto cuesta? *
                </label>
                <div class="relative">
                  <input
                    v-model="formData.price"
                    type="number"
                    step="0.01"
                    min="0"
                    class="glass-input w-full px-4 py-3 pl-12 text-lg"
                    placeholder="0.00"
                    required
                  />
                  <div v-if="formData.price" class="absolute right-4 top-4">
                    <i class="pi pi-money-bill text-green-400 animate-pulse"></i>
                  </div>
                </div>
                <p class="text-gray-400 text-sm mt-2 flex items-center">
                  <i class="pi pi-info-circle mr-1"></i>
                  El precio debe ser mayor a 0
                </p>
              </div>

              <!-- Category Field with Icons -->
              <div class="glass-card p-4 hover:bg-white/15 transition-all duration-300">
                <label class="flex items-center text-white font-semibold mb-3">
                  <i class="pi pi-list text-primary-400 mr-2"></i>
                  ¿En qué categoría va? *
                </label>
                <select
                  v-model="formData.categoryId"
                  class="glass-input w-full px-4 py-3 text-lg"
                  required
                >
                  <option value="">🤔 Selecciona una categoría...</option>
                  <option
                    v-for="category in (categories || []).filter((c) => c.categoriaId !== null)"
                    :key="category.categoriaId"
                    :value="category.categoriaId"
                  >
                    📂 {{ category.nombreCategoria }}
                  </option>
                </select>
                <p class="text-gray-400 text-sm mt-2 flex items-center">
                  <i class="pi pi-lightbulb mr-1"></i>
                  Esto ayuda a organizar mejor tu inventario
                </p>
              </div>
            </div>

            <!-- Right Column - Image Upload -->
            <div class="space-y-6">
              <!-- Image Upload with Drag & Drop Style -->
              <div class="glass-card p-4 hover:bg-white/15 transition-all duration-300">
                <label class="flex items-center text-white font-semibold mb-3">
                  <i class="pi pi-camera text-accent-400 mr-2"></i>
                  {{ isEditMode ? '📸 Cambiar imagen (opcional)' : '📸 Añade una foto genial *' }}
                </label>

                <!-- Image Preview with Hover Effects and Drag & Drop -->
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
                    <div
                      v-if="isLoadingImage"
                      class="absolute inset-0 bg-black/60 backdrop-blur-sm rounded-lg z-10 flex items-center justify-center"
                    >
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
                    <div
                      v-if="isDragging && !isLoadingImage"
                      class="absolute inset-2 bg-primary-500/30 backdrop-blur-sm rounded-lg z-30 flex flex-col items-center justify-center border-2 border-dashed border-primary-400"
                    >
                      <div class="bg-primary-400/80 p-4 rounded-full mb-2 animate-bounce">
                        <i class="pi pi-upload text-white text-3xl"></i>
                      </div>
                      <p class="text-white font-bold text-lg">¡Suelta aquí tu imagen!</p>
                      <p class="text-primary-200 text-sm">Se cargará automáticamente</p>
                    </div>

                    <div
                      class="absolute inset-2 bg-gradient-to-t from-black/60 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-all duration-300 rounded-lg flex flex-col items-center justify-center"
                      :class="{ 'opacity-0': isLoadingImage || isDragging }"
                    >
                      <div
                        class="bg-white/20 backdrop-blur-sm p-4 rounded-full mb-2 transform group-hover:scale-110 transition-transform"
                      >
                        <i class="pi pi-cloud-upload text-white text-2xl"></i>
                      </div>
                      <p class="text-white font-semibold">
                        {{
                          imagePreview.includes('sin-imagen')
                            ? '¡Sube tu primera imagen!'
                            : '¡Cambia la imagen!'
                        }}
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

                <!-- Upload Button -->
                <button
                  type="button"
                  @click="$refs.imageInput.click()"
                  :disabled="isLoadingImage"
                  class="w-full glass-button py-4 text-white hover:bg-white/20 transform hover:scale-105 transition-all duration-300 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  <i
                    :class="isLoadingImage ? 'pi pi-spinner pi-spin' : 'pi pi-cloud-upload'"
                    class="mr-2 text-lg"
                  ></i>
                  <span class="font-semibold">
                    {{
                      isLoadingImage
                        ? '⏳ Cargando...'
                        : imagePreview.includes('sin-imagen')
                          ? isEditMode
                            ? '🎨 Agregar Imagen'
                            : '🎨 Subir Imagen'
                          : '🔄 Cambiar Imagen'
                    }}
                  </span>
                </button>

                <!-- Clear Image Button -->
                <button
                  v-if="formData.image && !isLoadingImage"
                  type="button"
                  @click="clearImage"
                  class="w-full glass-button py-2 text-white hover:text-red-300 transform hover:scale-105 transition-all duration-300 mt-2"
                >
                  <i class="pi pi-trash mr-2"></i>
                  <span class="font-semibold">🗑️ Remover Imagen</span>
                </button>

                <div class="mt-3 p-3 bg-blue-500/20 rounded-lg border border-blue-500/30">
                  <p class="text-blue-300 text-sm flex items-center mb-1">
                    <i class="pi pi-info-circle mr-2"></i>
                    JPG, PNG o GIF • Máximo 5MB • ¡Que se vea genial! ✨
                  </p>
                  <p class="text-blue-200 text-xs">
                    💡 Puedes hacer clic, arrastrar y soltar, o pegar (Ctrl+V) tu imagen
                  </p>
                </div>
              </div>

              <!-- Submit Button with Fun Animation -->
              <button
                type="submit"
                :disabled="!isFormValid || isSubmitting"
                class="w-full bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 hover:from-primary-500 hover:via-secondary-500 hover:to-accent-500 disabled:opacity-50 disabled:cursor-not-allowed text-white font-bold py-4 px-6 rounded-xl text-lg transition-all duration-300 transform hover:scale-105 shadow-lg hover:shadow-xl"
              >
                <div class="flex items-center justify-center">
                  <i v-if="isSubmitting" class="pi pi-spinner pi-spin mr-3 text-xl"></i>
                  <i
                    v-else
                    :class="isEditMode ? 'pi pi-check' : 'pi pi-sparkles'"
                    class="mr-3 text-xl"
                  ></i>
                  <span>
                    {{
                      isSubmitting
                        ? '✨ Procesando magia...'
                        : isEditMode
                          ? '🎉 ¡Actualizar Artículo!'
                          : '🚀 ¡Crear Artículo!'
                    }}
                  </span>
                </div>
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Search and Filters with Friendly Design -->
    <div class="glass-container mb-6 p-6">
      <div class="flex items-center mb-4">
        <i class="pi pi-search text-accent-400 text-xl mr-2"></i>
        <h3 class="text-xl font-bold text-white">🔍 Encuentra tus artículos</h3>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <!-- Search -->
        <div class="relative">
          <i class="pi pi-search absolute left-4 top-4 text-gray-400"></i>
          <input
            v-model="searchTerm"
            type="text"
            class="glass-input w-full pl-12 pr-4 py-3"
            placeholder="🔎 Buscar por nombre..."
          />
          <div v-if="searchTerm" class="absolute right-4 top-4">
            <button @click="searchTerm = ''" class="text-gray-400 hover:text-white">
              <i class="pi pi-times"></i>
            </button>
          </div>
        </div>

        <!-- Category Filter -->
        <select v-model="selectedCategory" class="glass-input px-4 py-3">
          <option value="all">📂 Todas las categorías</option>
          <option
            v-for="category in (categories || []).filter((c) => c.categoriaId !== null)"
            :key="category.categoriaId"
            :value="category.categoriaId"
          >
            📁 {{ category.nombreCategoria }}
          </option>
        </select>

        <!-- Sort -->
        <select v-model="sortBy" class="glass-input px-4 py-3">
          <option value="name">🔤 Ordenar por nombre</option>
          <option value="price">💰 Ordenar por precio</option>
          <option value="category">📂 Ordenar por categoría</option>
        </select>
      </div>

      <!-- Quick Filter Chips -->
      <div class="flex flex-wrap gap-2 mt-4">
        <span class="text-gray-300 text-sm">Filtros rápidos:</span>
        <button
          v-for="category in (categories || []).slice(1, 6)"
          :key="category.categoriaId"
          @click="selectedCategory = category.categoriaId"
          class="glass-button px-3 py-1 text-sm text-white hover:bg-white/20 transform hover:scale-105 transition-all"
          :class="{ 'bg-primary-500/50': selectedCategory === category.categoriaId }"
        >
          {{ category.nombreCategoria }}
        </button>
      </div>
    </div>

    <!-- Articles Grid with Cards -->
    <div class="glass-container p-6">
      <div class="flex items-center justify-between mb-6">
        <div class="flex items-center">
          <i class="pi pi-grid text-primary-400 text-xl mr-2"></i>
          <h3 class="text-xl font-bold text-white">
            {{
              (filteredArticles?.length || 0) > 0
                ? `🎉 ${filteredArticles.length} artículos encontrados`
                : '📦 Tu inventario'
            }}
          </h3>
        </div>

        <div v-if="filteredArticles.length > 0" class="glass-card px-3 py-1">
          <span class="text-accent-400 text-sm font-semibold text-white">
            Total:
            {{
              formatPrice(
                (filteredArticles || []).reduce((sum, art) => sum + parseFloat(art.precio || 0), 0)
              )
            }}
          </span>
        </div>
      </div>

      <!-- Articles Grid -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
        <div
          v-for="article in filteredArticles || []"
          :key="article.articuloId"
          class="glass-card p-4 hover:bg-white/15 transition-all duration-300 group transform hover:scale-105"
        >
          <!-- Article Image -->
          <div class="relative mb-4 overflow-hidden rounded-lg">
            <img
              :src="getArticleImage(article)"
              :alt="article.nombreArticulo"
              class="w-full h-48 object-cover transition-transform duration-300 group-hover:scale-110"
            />
            <div class="absolute top-2 right-2">
              <span class="glass-button px-2 py-1 text-xs text-white">
                📂 {{ getCategoryName(article.categoriaId) }}
              </span>
            </div>
            <div class="absolute bottom-2 left-2">
              <span class="glass-button px-2 py-1 text-sm font-bold text-accent-400">
                {{ formatPrice(article.precio) }}
              </span>
            </div>
          </div>

          <!-- Article Info -->
          <div class="mb-4">
            <h4
              class="text-white font-semibold text-lg mb-2 line-clamp-2 group-hover:text-primary-300 transition-colors"
            >
              {{ article.nombreArticulo }}
            </h4>
          </div>

          <!-- Actions -->
          <div class="flex space-x-2 opacity-0 group-hover:opacity-100 transition-all duration-300">
            <button
              @click="startEditAndShowForm(article)"
              class="flex-1 glass-button py-2 text-white hover:text-blue-300 transform hover:scale-105 transition-all"
            >
              <i class="pi pi-pencil mr-1"></i>
              ✏️ Editar
            </button>

            <button
              @click="handleDelete(article)"
              class="flex-1 glass-button py-2 text-white hover:text-red-300 transform hover:scale-105 transition-all"
            >
              <i class="pi pi-trash mr-1"></i>
              🗑️ Eliminar
            </button>
          </div>
        </div>
      </div>

      <!-- Fun Empty State -->
      <div v-if="(filteredArticles?.length || 0) === 0" class="text-center py-16">
        <div class="glass-card max-w-md mx-auto p-8">
          <div class="mb-6">
            <div
              class="bg-gradient-to-r from-primary-400 to-accent-400 p-6 rounded-full mx-auto w-24 h-24 flex items-center justify-center mb-4"
            >
              <i class="pi pi-box text-white text-4xl"></i>
            </div>
            <h3 class="text-2xl text-white font-bold mb-2">
              {{ searchTerm ? '🔍 ¡No encontramos nada!' : '📦 ¡Tu inventario está vacío!' }}
            </h3>
            <p class="text-gray-300 mb-6">
              {{
                searchTerm
                  ? 'Intenta con otros términos de búsqueda o ajusta los filtros'
                  : '¡Es hora de agregar tu primer artículo increíble!'
              }}
            </p>
          </div>

          <button
            v-if="!searchTerm"
            @click="showCreateForm = true"
            class="bg-gradient-to-r from-primary-400 to-accent-400 hover:from-primary-500 hover:to-accent-500 text-white font-bold py-3 px-6 rounded-lg transform hover:scale-105 transition-all duration-300"
          >
            <i class="pi pi-plus mr-2"></i>
            🚀 ¡Crear mi primer artículo!
          </button>

          <button
            v-else
            @click="() => { searchTerm = ''; selectedCategory = 'all' }"
            class="glass-button text-white py-3 px-6 hover:bg-white/20 transform hover:scale-105 transition-all"
          >
            <i class="pi pi-refresh mr-2"></i>
            🔄 Limpiar filtros
          </button>
        </div>
      </div>
    </div>

    <!-- Toast and Confirm Dialog -->
    <Toast />
    <ConfirmDialog />
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted, computed, type Ref } from 'vue'
  import { useArticleCreate } from '../composables/useArticleCreate'
  import { ArticleService } from '../services/ArticleService'
  import { useAuthStore } from '../store/auth'
  import Toast from 'primevue/toast'
  import ConfirmDialog from 'primevue/confirmdialog'
  import type { ArticleDto, CategoryDto, ApiResponse } from '../types'

  // Category interface
  interface CategoryWithAll extends CategoryDto {
    categoriaId: number | null
  }

  // Composables
  const {
    // State
    formData,
    isEditMode,
    isSubmitting,
    isLoadingImage,
    imagePreview,
    articles,
    categories,
    selectedCategory,
    searchTerm,
    sortBy,

    // Computed
    isFormValid,
    isNameDuplicate,
    filteredArticles,

    // Methods
    resetForm,
    handleImageUpload,
    clearImage,
    startEdit,
    cancelEdit,
    validateForm,
    getFormattedData,
    showSuccess,
    showError,
    confirmDelete,
    formatPrice,
    getArticleImage,
    getCategoryName,
  } = useArticleCreate()

  // Store
  const authStore = useAuthStore()

  // Local State
  const showCreateForm: Ref<boolean> = ref(false)
  const imageInput: Ref<HTMLInputElement | null> = ref(null)
  const isDragging: Ref<boolean> = ref(false)

  // Enhanced Methods
  const toggleCreateForm = (): void => {
    showCreateForm.value = !showCreateForm.value
    if (!showCreateForm.value && isEditMode.value) {
      cancelEdit()
    }
  }

  const startEditAndShowForm = (article: ArticleDto): void => {
    startEdit(article)
    showCreateForm.value = true

    // Scroll to the form smoothly with a slight delay to ensure DOM is updated
    setTimeout(() => {
      // Target the create/edit form container more specifically
      const formElement =
        document.querySelector('[ref="createFormContainer"]') ||
        document.querySelector('.glass-container:has(form)') ||
        document.querySelector('.glass-container:nth-of-type(2)')

      if (formElement) {
        // Add offset to account for fixed headers
        const yOffset = -100
        const y = formElement.getBoundingClientRect().top + window.pageYOffset + yOffset

        window.scrollTo({
          top: y,
          behavior: 'smooth',
        })
      }
    }, 150)
  }

  const handleImageChange = (event: Event): void => {
    const target = event.target as HTMLInputElement
    const file = target.files?.[0]
    if (file && handleImageUpload(file)) {
      // Image uploaded successfully
    }
  }

  // Drag & Drop handlers
  const onDragOver = (event: DragEvent): void => {
    event.preventDefault()
    isDragging.value = true
  }

  const onDragLeave = (event: DragEvent): void => {
    event.preventDefault()
    isDragging.value = false
  }

  const onDrop = (event: DragEvent): void => {
    event.preventDefault()
    isDragging.value = false

    const files = event.dataTransfer?.files
    if (files && files.length > 0) {
      const file = files[0]
      if (file.type.startsWith('image/')) {
        handleImageUpload(file)
      } else {
        showError('Solo se permiten archivos de imagen')
      }
    }
  }

  const formatCompactCurrency = (amount: number): string => {
    if (amount >= 1000000) {
      return `$${(amount / 1000000).toFixed(1)}M`
    } else if (amount >= 1000) {
      return `$${(amount / 1000).toFixed(1)}K`
    }
    return formatPrice(amount)
  }

  const calculateTotalValue = (): number => {
    if (!articles.value || !Array.isArray(articles.value)) return 0
    return articles.value.reduce((sum, article) => sum + article.precio, 0)
  }

  const calculateAveragePrice = (): number => {
    if (!articles.value || !Array.isArray(articles.value) || articles.value.length === 0) return 0
    return calculateTotalValue() / articles.value.length
  }

  const handleSubmit = async (): Promise<void> => {
    if (!validateForm()) return

    isSubmitting.value = true

    try {
      const articleData = getFormattedData()

      if (isEditMode.value) {
        await ArticleService.updateArticle(formData.value.articuloId!, {
          nombreArticulo: articleData.nombreArticulo,
          precio: articleData.precio,
          categoriaId: articleData.categoriaId,
        })

        if (formData.value.image) {
          await ArticleService.updateArticleImage(formData.value.articuloId!, formData.value.image)
        }

        showSuccess('🎉 ¡Artículo actualizado exitosamente!')
      } else {
        if (formData.value.image) {
          // Create with image using V1 endpoint
          await ArticleService.createArticleWithImage(articleData as any)
        } else {
          // Create without image using V1 endpoint
          await ArticleService.createArticle(articleData as any)
        }
        showSuccess('🚀 ¡Nuevo artículo creado con éxito!')
      }

      resetForm()
      showCreateForm.value = false
      await fetchArticles()
    } catch (error) {
      console.error('Error:', error)
      showError(
        isEditMode.value ? '❌ Error al actualizar el artículo' : '❌ Error al crear el artículo'
      )
    } finally {
      isSubmitting.value = false
    }
  }

  const handleDelete = async (article: ArticleDto): Promise<void> => {
    const confirmed = await confirmDelete(article)
    if (!confirmed) return

    try {
      await ArticleService.deleteArticle(article.articuloId)
      showSuccess('🗑️ Artículo eliminado correctamente')
      await fetchArticles()
    } catch (error) {
      console.error('Error deleting article:', error)
      showError('❌ Error al eliminar el artículo')
    }
  }

  const fetchArticles = async (): Promise<void> => {
    try {
      const categoriaId = selectedCategory.value === 'all' ? null : Number(selectedCategory.value)
      const response: ApiResponse<ArticleDto[]> = await ArticleService.getArticles(categoriaId)

      // Handle V1 API response structure only
      if (response.isSuccess && response.data) {
        articles.value = response.data
      } else {
        articles.value = []
        if (!response.isSuccess) {
          showError(`Error: ${response.message || 'Error al cargar artículos'}`)
        }
      }
    } catch (error) {
      console.error('Error fetching articles:', error)
      showError('❌ Error al cargar los artículos')
    }
  }

  const fetchCategories = async (): Promise<void> => {
    try {
      const response: ApiResponse<CategoryDto[]> = await ArticleService.getCategories()

      if (response.isSuccess && response.data) {
        categories.value = [
          { categoriaId: null, nombreCategoria: 'Todas las categorías' } as CategoryWithAll,
          ...response.data,
        ]
      } else {
        categories.value = [
          { categoriaId: null, nombreCategoria: 'Todas las categorías' } as CategoryWithAll,
        ]
        if (!response.isSuccess) {
          showError(`Error: ${response.message || 'Error al cargar categorías'}`)
        }
      }
    } catch (error) {
      console.error('Error fetching categories:', error)
      showError('❌ Error al cargar las categorías')
    }
  }

  // Lifecycle
  onMounted(async () => {
    await Promise.all([fetchCategories(), fetchArticles()])
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
    0%,
    100% {
      transform: translateX(0);
    }
    25% {
      transform: translateX(-5px);
    }
    75% {
      transform: translateX(5px);
    }
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

  /* Custom scrollbar for better UX */
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
