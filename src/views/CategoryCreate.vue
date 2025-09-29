<template>
  
  <div class="min-h-screen bg-gradient-to-br from-slate-900 via-purple-900 to-slate-900 p-6">
    <!-- Glass Container -->
    <div class="max-w-7xl mx-auto">
      <!-- Header -->
      <div class="glass-card mb-8">
        <h1 class="text-3xl font-bold text-white lexend-exa mb-2">Gestión de Categorías</h1>
        <p class="text-white/70">Crear y administrar categorías de habitaciones</p>
      </div>

      <!-- Main Content Grid -->
      <div class="grid lg:grid-cols-2 gap-8 mb-8">
        <!-- Form Section -->
        <div class="glass-card">
          <div class="flex items-center justify-between mb-6">
            <h2 class="text-xl font-bold text-white lexend-exa">
              {{ isEditMode ? 'Editar Categoría' : 'Nueva Categoría' }}
            </h2>
            <div v-if="isEditMode" class="text-sm text-white/70">
              ID: {{ formData.categoriaId }}
            </div>
          </div>

          <form @submit.prevent="submitForm" class="space-y-6">
            <!-- Category Name -->
            <div class="space-y-2">
              <label class="block text-white font-medium text-sm"> Nombre de la Categoría * </label>
              <input
                v-model="formData.nombreCategoria"
                type="text"
                placeholder="Ej: Suite Presidencial, Habitación Estándar"
                class="glass-input w-full"
                :class="{ 'border-red-400': isNameDuplicate && formData.nombreCategoria }"
                required
              />
              <p v-if="isNameDuplicate && formData.nombreCategoria" class="text-red-400 text-sm">
                Ya existe una categoría con este nombre
              </p>
            </div>
  <!-- Alerta emergente -->
<transition name="fade">
  <div
    v-if="alertaVisible"
    class="fixed top-12 left-0 w-full bg-yellow-400 text-yellow-900 text-center font-medium py-2 shadow-lg z-50"
  >
   Estas editando la categoría <strong>"{{ formData.nombreCategoria }}"</strong>
  </div>
</transition>
          <!-- Price -->
<div class="space-y-2">
  <label class="block text-white font-medium text-sm"> Precio Base * </label>

  <div class="glass-input flex items-center">
    <span class="text-white/70 ml-3 mr-2">$</span>
    <input
      v-model="formData.precioNormal"
      type="number"
      min="0"
      step="0.01"
      placeholder="0.00"
      class="bg-transparent w-full text-white placeholder-white/50 focus:outline-none"
      required
    />
  </div>

  <p class="text-white/60 text-xs">Precio base por noche</p>
</div>
            <!-- Capacity -->
<div class="space-y-2">
  <label class="block text-white font-medium text-sm"> Capacidad Máxima * </label>

  <div class="glass-input flex items-center">
    <input
      v-model="formData.capacidadMaxima"
      type="number"
      min="1"
      max="20"
      placeholder="2"
      class="bg-transparent w-full text-white placeholder-white/50 focus:outline-none"
      required
    />
    <span class="text-white/70 mr-3">
      <i class="pi pi-users text-sm"></i>
    </span>
  </div>

  <p class="text-white/60 text-xs">Número máximo de personas</p>
</div>

<!-- Percentage per person -->
<div class="space-y-2">
  <label class="block text-white font-medium text-sm">
    Porcentaje por Persona Adicional *
  </label>

  <div class="glass-input flex items-center">
    <input
      v-model="formData.porcentajeXPersona"
      type="number"
      min="0"
      max="100"
      step="0.1"
      placeholder="0.0"
      class="bg-transparent w-full text-white placeholder-white/50 focus:outline-none"
      required
    />
    <span class="text-white/70 mr-3">%</span>
  </div>

  <p class="text-white/60 text-xs">
    Incremento del precio base por persona adicional
  </p>
</div>

            <!-- Price Calculator -->
            <div
              v-if="formData.precioNormal && formData.porcentajeXPersona"
              class="glass-preview-card"
            >
              <h4 class="text-white font-medium text-sm mb-3">
                <i class="pi pi-calculator mr-2"></i>
                Calculadora de Precios
              </h4>
              <div class="space-y-2">
                <div class="flex justify-between text-sm">
                  <span class="text-white/70">1 persona:</span>
                  <span class="text-white font-medium">{{
                    formatPrice(parseFloat(formData.precioNormal || 0))
                  }}</span>
                </div>
                <div v-if="formData.capacidadMaxima > 1" class="flex justify-between text-sm">
                  <span class="text-white/70">{{ formData.capacidadMaxima }} personas:</span>
                  <span class="text-white font-medium">
                    {{ calculateMaxPrice() }}
                  </span>
                </div>
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
                {{ isEditMode ? 'Actualizar Categoría' : 'Crear Categoría' }}
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

        <!-- Summary Section -->
        <div class="space-y-6">
          <!-- Form Preview -->
          <div class="glass-card">
            <h3 class="text-lg font-bold text-white lexend-exa mb-4">Vista Previa</h3>
            <div class="space-y-4">
              <!-- Category Preview Card -->
              <div class="glass-category-preview">
                <div class="flex items-start justify-between">
                  <div class="flex-1">
                    <h4 class="text-white font-semibold text-lg">
                      {{ formData.nombreCategoria || 'Nombre de la categoría' }}
                    </h4>
                    <p class="text-white/70 text-sm mt-1">
                      {{ formData.capacidadMaxima || 0 }} persona{{
                        (formData.capacidadMaxima || 0) !== '1' ? 's' : ''
                      }}
                      máximo
                    </p>
                  </div>
                  <div class="text-right">
                    <div class="text-xl font-bold text-white">
                      {{ formatPrice(parseFloat(formData.precioNormal || 0)) }}
                    </div>
                    <div class="text-white/70 text-sm">
                      +{{ formData.porcentajeXPersona || 0 }}% por persona
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Statistics -->
          <div class="glass-card">
            <h3 class="text-lg font-bold text-white lexend-exa mb-4">Estadísticas</h3>
            <div class="grid grid-cols-2 gap-4">
              <div class="text-center">
                <div class="text-2xl font-bold text-white">{{ categorias.length }}</div>
                <div class="text-white/70 text-sm">Categorías Totales</div>
              </div>
              <div class="text-center">
                <div class="text-2xl font-bold text-white">{{ averagePrice }}</div>
                <div class="text-white/70 text-sm">Precio Promedio</div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Categories List -->
      <div class="glass-card">
        <div class="flex items-center justify-between mb-6">
          <h2 class="text-xl text-white lexend-exa font-bold">
            Categorías Registradas ({{ categorias.length }})
          </h2>
          <div class="text-sm text-white/70">
            Total: {{ categorias.length }} categoría{{ categorias.length !== 1 ? 's' : '' }}
          </div>
        </div>

        <div v-if="categorias.length === 0" class="text-center py-12">
          <i class="pi pi-th-large text-4xl text-white/50 mb-4 block"></i>
          <p class="text-white/70 text-lg">No hay categorías registradas</p>
          <p class="text-white/50 text-sm">Crea la primera categoría usando el formulario</p>
        </div>

        <div v-else class="grid lg:grid-cols-2 xl:grid-cols-3 gap-4">
          <div
            v-for="categoria in categorias"
            :key="categoria.categoriaId"
            class="glass-category-card group"
          >
            <div class="p-6">
              <div class="flex items-start justify-between mb-4">
                <div class="flex-1">
                  <h3 class="text-lg font-semibold text-white mb-1">
                    {{ categoria.nombreCategoria }}
                  </h3>
                  <p class="text-white/60 text-sm">ID: {{ categoria.categoriaId }}</p>
                </div>
                <span class="glass-badge">
                  <i class="pi pi-users mr-1"></i>
                  {{ categoria.capacidadMaxima }}
                </span>
              </div>

              <!-- Pricing Info -->
              <div class="space-y-2 mb-4">
                <div class="flex justify-between items-center">
                  <span class="text-white/70 text-sm">Precio base:</span>
                  <span class="text-white font-semibold">{{
                    formatPrice(categoria.precioNormal)
                  }}</span>
                </div>
                <div class="flex justify-between items-center">
                  <span class="text-white/70 text-sm">Por persona adicional:</span>
                  <span class="text-white font-semibold">+{{ categoria.porcentajeXPersona }}%</span>
                </div>
                <div class="border-t border-white/10 pt-2">
                  <div class="flex justify-between items-center">
                    <span class="text-white/70 text-sm">Precio máximo:</span>
                    <span class="text-white font-bold">
                      {{ formatPrice(calculateCategoryMaxPrice(categoria)) }}
                    </span>
                  </div>
                </div>
              </div>

             <!-- Action Buttons -->
<div class="grid grid-cols-2 gap-2">
  <button
    @click="handleEdit(categoria)"
    class="glass-action-button text-blue-400 hover:text-blue-300"
  >
    <i class="pi pi-pencil text-sm"></i>
    <span class="text-xs mt-1">Editar</span>
  </button>

  <button
    @click="deleteCategory(categoria)"
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
  </div>
</template>

<script setup>
  import { ref, onMounted, computed } from 'vue'
  import { useCategoryCreate } from '../composables/useCategoryCreate.js'
  import CategoryService from '../services/categoryService'
  import { useAuthStore } from '../store/auth.js'

  // Auth store
  const authStore = useAuthStore()

  // Composable
  const {
    formData,
    isEditMode,
    isSubmitting,
    categorias,
    isFormValid,
    isNameDuplicate,
    resetForm,
    startEdit,
    cancelEdit,
    validateForm,
    getFormattedData,
    showSuccess,
    showError,
    confirmDelete,
    formatPrice,
  } = useCategoryCreate()

  // Computed
  const averagePrice = computed(() => {
    if (categorias.value.length === 0) return formatPrice(0)
    const total = categorias.value.reduce((sum, cat) => sum + cat.precioNormal, 0)
    return formatPrice(total / categorias.value.length)
  })

  //Mensaje de alerta de edicion
  const alertaVisible = ref(false)

const handleEdit = (categoria) => {
  startEdit(categoria) // tu funcionalidad original
  window.scrollTo({ top: 0, behavior: "smooth" }) // desliza hacia arriba

  // mostrar la alerta emergente una vez
  alertaVisible.value = true
  setTimeout(() => {
    alertaVisible.value = false
  }, 3000) // se oculta a los 3 segundos
}

  // Lifecycle
  onMounted(() => {
    fetchCategories()
  })

  // Methods
  const calculateMaxPrice = () => {
    const basePrice = parseFloat(formData.value.precioNormal || 0)
    const percentage = parseFloat(formData.value.porcentajeXPersona || 0)
    const capacity = parseInt(formData.value.capacidadMaxima || 1)
    const extraPersons = Math.max(0, capacity - 1)

    return formatPrice(
      CategoryService.calculatePriceWithPercentage(basePrice, percentage, extraPersons)
    )
  }

  const calculateCategoryMaxPrice = (categoria) => {
    const extraPersons = Math.max(0, categoria.capacidadMaxima - 1)
    return CategoryService.calculatePriceWithPercentage(
      categoria.precioNormal,
      categoria.porcentajeXPersona,
      extraPersons
    )
  }

  // Form submission
  const submitForm = async () => {
    if (!validateForm()) return

    isSubmitting.value = true

    try {
      if (isEditMode.value) {
        await updateCategory()
      } else {
        await createCategory()
      }
    } catch (error) {
      console.error('Error in form submission:', error)
    } finally {
      isSubmitting.value = false
    }
  }

  // Create category
  const createCategory = async () => {
    try {
      const categoryData = {
        ...getFormattedData(),
        institucionId: authStore.institucionID,
        usuarioId: authStore.auth?.usuarioID,
      }

      // Validate required auth data
      if (!categoryData.institucionId || !categoryData.usuarioId) {
        showError(
          'No se pudo obtener la información del usuario. Por favor inicia sesión nuevamente.'
        )
        return
      }

      const response = await CategoryService.createCategory(categoryData)

      if (response && response.isSuccess) {
        showSuccess(response.message || 'Categoría creada exitosamente')
        resetForm()
        await fetchCategories()
      } else {
        showError(response?.message || 'Error al crear la categoría')
      }
    } catch (error) {
      console.error('Error creating category:', error)
      showError('Error al crear la categoría')
    }
  }

  // Update category
  const updateCategory = async () => {
    try {
      const categoryData = {
        ...getFormattedData(),
      }

      const response = await CategoryService.updateCategory(
        formData.value.categoriaId,
        categoryData
      )

      if (response && response.isSuccess) {
        showSuccess(response.message || 'Categoría actualizada exitosamente')
        resetForm()
        await fetchCategories()
      } else {
        showError(response?.message || 'Error al actualizar la categoría')
      }
    } catch (error) {
      console.error('Error updating category:', error)
      showError('Error al actualizar la categoría')
    }
  }

  // Delete category
  const deleteCategory = async (categoria) => {
    const confirmed = await confirmDelete(categoria)
    if (!confirmed) return

    try {
      const response = await CategoryService.deleteCategory(categoria.categoriaId)

      if (response && response.isSuccess) {
        showSuccess('Categoría eliminada exitosamente')
        await fetchCategories()
      } else {
        showError(response?.message || 'Error al eliminar la categoría')
      }
    } catch (error) {
      console.error('Error deleting category:', error)
      showError('Error al eliminar la categoría')
    }
  }

  // Fetch categories
  const fetchCategories = async () => {
    try {
      const institucionId = authStore.institucionID
      if (!institucionId) {
        showError('No se pudo obtener el ID de la institución. Por favor inicia sesión nuevamente.')
        return
      }

      const response = await CategoryService.getCategories(institucionId)

      if (response && response.isSuccess && response.data) {
        categorias.value = response.data
      } else {
        showError(response?.message || 'Error al cargar las categorías')
      }
    } catch (error) {
      console.error('Error fetching categories:', error)
      showError('Error al cargar las categorías')
    }
  }
</script>

<style scoped>
  .glass-preview-card {
    @apply bg-white/5 backdrop-blur-sm border border-white/20 rounded-xl p-4;
    background: rgba(255, 255, 255, 0.05);
    backdrop-filter: blur(10px);
  }

  .glass-category-preview {
    @apply bg-white/10 backdrop-blur-sm border border-white/30 rounded-xl p-4;
    background: rgba(255, 255, 255, 0.1);
    backdrop-filter: blur(10px);
  }

  .glass-category-card {
    @apply bg-neutral-900/95 backdrop-blur-xl rounded-2xl border border-white/20 shadow-xl hover:shadow-2xl transition-all duration-300 hover:scale-105;
    background: linear-gradient(135deg, rgba(255, 255, 255, 0.1), rgba(255, 255, 255, 0.05));
    backdrop-filter: blur(20px);
  }

  .glass-category-card:hover {
    border-color: rgba(244, 63, 184, 0.4);
    box-shadow: 0 12px 40px rgba(244, 63, 184, 0.2);
  }
</style>

