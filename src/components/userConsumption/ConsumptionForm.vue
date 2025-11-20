<template>
  <!-- Modal Overlay -->
  <div class="fixed inset-0 bg-black/60 backdrop-blur-xl z-50 flex items-center justify-center p-4">
    <div class="glass-container max-w-2xl w-full max-h-[90vh] overflow-hidden">
      <!-- Header -->
      <div class="bg-gradient-to-r from-primary-400 to-accent-400 p-6 rounded-t-3xl">
        <div class="flex items-center justify-between">
          <div class="flex items-center">
            <div class="bg-white/20 p-3 rounded-full mr-3">
              <i class="pi pi-plus text-white text-xl"></i>
            </div>
            <div>
              <h2 class="text-2xl font-bold text-white">➕ Nuevo Consumo</h2>
              <p class="text-white/80">Registra un nuevo consumo personal</p>
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
        <form @submit.prevent="handleSubmit" class="space-y-6">
          <!-- Article Selection -->
          <div class="space-y-2">
            <div class="flex justify-between items-center">
              <label class="block text-white font-semibold">
                <i class="pi pi-tag text-primary-400 mr-2"></i>
                Artículo *
              </label>
              <button
                type="button"
                @click="loadArticles"
                :disabled="loadingArticles"
                class="text-xs glass-button px-2 py-1 text-gray-300 hover:text-white"
              >
                <i
                  :class="loadingArticles ? 'pi pi-spinner pi-spin' : 'pi pi-refresh'"
                  class="mr-1"
                ></i>
                {{ loadingArticles ? 'Cargando...' : 'Recargar' }}
              </button>
            </div>
            <div class="relative mt-2">
              <input
                v-model="searchTerm"
                type="text"
                placeholder="Buscar artículo por nombre o código..."
                class="glass-input w-full px-4 py-3 pr-10"
                @input="searchArticles"
                @focus="showDropdown = true"
                required
              />
              <i
                class="pi pi-search absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400"
              ></i>
            </div>

            <!-- Articles Dropdown -->
            <div
              v-if="showDropdown && filteredArticles.length > 0"
              class="glass-card mt-2 max-h-48 overflow-y-auto"
            >
              <div
                v-for="article in filteredArticles"
                :key="getArticleId(article)"
                @click="selectArticle(article)"
                class="p-3 hover:bg-white/10 cursor-pointer transition-colors border-b border-white/10 last:border-b-0"
              >
                <div class="flex justify-between items-center">
                  <div>
                    <p class="text-white font-medium">{{ getArticleName(article) }}</p>
                    <p class="text-gray-400 text-sm">ID: {{ getArticleId(article) }}</p>
                  </div>
                  <div class="text-right">
                    <p class="text-green-400 font-semibold">{{ formatCurrency(getArticlePrice(article)) }}</p>
                  </div>
                </div>
              </div>
            </div>

            <!-- Loading indicator for articles -->
            <div v-if="showDropdown && loadingArticles" class="glass-card mt-2 p-4 text-center">
              <i class="pi pi-spinner pi-spin text-white mr-2"></i>
              <span class="text-gray-300">Cargando artículos...</span>
            </div>

            <!-- Empty state for articles -->
            <div
              v-if="showDropdown && !loadingArticles && searchTerm && filteredArticles.length === 0"
              class="glass-card mt-2 p-4 text-center"
            >
              <p class="text-gray-400">No se encontraron artículos</p>
            </div>

            <!-- Selected Article Display -->
            <div v-if="form.articuloId" class="glass-card p-4 bg-green-500/10 border-green-500/30">
              <div class="flex justify-between items-center">
                <div>
                  <p class="text-white font-medium">{{ getArticleName(selectedArticle) }}</p>
                  <p class="text-gray-400 text-sm">ID: {{ getArticleId(selectedArticle) }}</p>
                </div>
                <button
                  type="button"
                  @click="clearArticle"
                  class="bg-red-500/20 hover:bg-red-500/30 p-2 rounded-full transition-colors"
                >
                  <i class="pi pi-times text-red-400"></i>
                </button>
              </div>
            </div>
          </div>

          <!-- Quantity -->
          <div class="space-y-2">
            <label class="block text-white font-semibold">
              <i class="pi pi-box text-secondary-400 mr-2"></i>
              Cantidad *
            </label>
            <input
              v-model.number="form.cantidad"
              type="number"
              min="1"
              step="1"
              placeholder="Ingrese la cantidad"
              class="glass-input w-full px-4 py-3"
              required
            />
          </div>

          <!-- Unit Price (Optional Override) -->
          <div class="space-y-2">
            <label class="block text-white font-semibold">
              <i class="pi pi-dollar text-green-400 mr-2"></i>
              Precio Unitario (Opcional)
            </label>
            <input
              v-model.number="form.precioUnitario"
              type="number"
              min="0"
              step="0.01"
              placeholder="Dejar vacío para usar precio del artículo"
              class="glass-input w-full px-4 py-3"
            />
            <p class="text-gray-400 text-xs">
              💡 Si no se especifica, se usará el precio del artículo:
              {{ formatCurrency(getArticlePrice(selectedArticle) || 0) }}
            </p>
          </div>

          <!-- Room Selection (Optional) -->
          <div class="space-y-2">
            <label class="block text-white font-semibold">
              <i class="pi pi-home text-yellow-400 mr-2"></i>
              Habitación (Opcional)
            </label>
            <select
              v-model="form.habitacionId"
              class="glass-input w-full px-4 py-3"
              :disabled="loadingRooms"
            >
              <option :value="null">
                {{ loadingRooms ? 'Cargando habitaciones...' : 'Seleccionar habitación...' }}
              </option>
              <option
                v-for="room in availableRooms"
                :key="getRoomId(room)"
                :value="getRoomId(room)"
              >
                {{ getRoomName(room) }} - Capacidad: {{ getRoomCapacity(room) }}
              </option>
            </select>
          </div>

          <!-- Consumption Type -->
          <div class="space-y-2">
            <label class="block text-white font-semibold">
              <i class="pi pi-tags text-purple-400 mr-2"></i>
              Tipo de Consumo
            </label>
            <select v-model="form.tipoConsumo" class="glass-input w-full px-4 py-3">
              <option value="">Seleccionar tipo...</option>
              <option value="Servicio">Servicio</option>
              <option value="Habitacion">Habitación</option>
            </select>
          </div>

          <!-- Observations -->
          <div class="space-y-2">
            <label class="block text-white font-semibold">
              <i class="pi pi-comment text-blue-400 mr-2"></i>
              Observaciones (Opcional)
            </label>
            <textarea
              v-model="form.observaciones"
              rows="3"
              placeholder="Agregar comentarios o notas adicionales..."
              class="glass-input w-full px-4 py-3 resize-none"
            ></textarea>
          </div>

          <!-- Total Preview -->
          <div v-if="totalPreview > 0" class="glass-card p-4 bg-blue-500/10 border-blue-500/30">
            <div class="flex justify-between items-center">
              <div>
                <p class="text-white font-semibold">Total a registrar:</p>
                <p class="text-gray-400 text-sm">
                  {{ form.cantidad || 0 }} × {{ formatCurrency(effectivePrice) }}
                </p>
              </div>
              <div>
                <p class="text-green-400 font-bold text-2xl">{{ formatCurrency(totalPreview) }}</p>
              </div>
            </div>
          </div>

          <!-- Form Actions -->
          <div class="flex items-center justify-end space-x-4 pt-4 border-t border-white/10">
            <button
              type="button"
              @click="$emit('close')"
              class="glass-button py-3 px-6 text-white hover:text-red-300 transform hover:scale-105 transition-all"
            >
              <i class="pi pi-times mr-2"></i>
              Cancelar
            </button>

            <button
              type="submit"
              :disabled="!isFormValid || loading"
              class="bg-gradient-to-r from-green-400 to-green-500 hover:from-green-500 hover:to-green-600 disabled:opacity-50 disabled:cursor-not-allowed text-white font-bold py-3 px-6 rounded-lg transition-all duration-300 transform hover:scale-105"
            >
              <i :class="loading ? 'pi pi-spinner pi-spin' : 'pi pi-check'" class="mr-2"></i>
              {{ loading ? 'Guardando...' : '💾 Registrar Consumo' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
  import { ref, computed, onMounted, watch } from 'vue'
  import { useUserConsumption } from '../../composables/useUserConsumption'
  import { useToast } from 'primevue/usetoast'
  import { ArticleService } from '../../services/ArticleService'
  import HabitacionService from '../../services/habitacionService'

  const emit = defineEmits(['close', 'created'])
  const toast = useToast()

  // Composable
  const { createConsumption, loading } = useUserConsumption()

  // Form state
  const form = ref({
    articuloId: null,
    cantidad: 1,
    precioUnitario: null,
    habitacionId: null,
    reservaId: null,
    tipoConsumo: '',
    observaciones: '',
  })

  // UI state
  const searchTerm = ref('')
  const showDropdown = ref(false)
  const selectedArticle = ref(null)
  const loadingArticles = ref(false)
  const loadingRooms = ref(false)

  // Data from API
  const availableArticles = ref([])
  const availableRooms = ref([])

  // Debounce timer
  let searchDebounceTimer = null

  // Helper functions to handle both camelCase and PascalCase
  const getArticleId = (article) => article?.articuloID || article?.ArticuloID || article?.articuloId || article?.ArticuloId
  const getArticleName = (article) => article?.nombreArticulo || article?.NombreArticulo
  const getArticlePrice = (article) => article?.precio || article?.Precio
  
  const getRoomId = (room) => room?.habitacionId || room?.HabitacionId
  const getRoomName = (room) => room?.nombreHabitacion || room?.NombreHabitacion
  const getRoomCapacity = (room) => room?.capacidadMaxima || room?.CapacidadMaxima

  // Computed properties
  const filteredArticles = computed(() => {
    if (!searchTerm.value) return availableArticles.value

    const term = searchTerm.value.toLowerCase()
    return availableArticles.value.filter(
      (article) =>
        getArticleName(article)?.toLowerCase().includes(term) ||
        getArticleId(article)?.toString().includes(term)
    )
  })

  const effectivePrice = computed(() => {
    return form.value.precioUnitario || getArticlePrice(selectedArticle.value) || 0
  })

  const totalPreview = computed(() => {
    return (form.value.cantidad || 0) * effectivePrice.value
  })

  const isFormValid = computed(() => {
    return (
      form.value.articuloId &&
      form.value.cantidad &&
      form.value.cantidad > 0 &&
      effectivePrice.value > 0
    )
  })

  // Methods
  const searchArticles = () => {
    showDropdown.value = true
    if (searchTerm.value && availableArticles.value.length === 0) {
      loadArticles()
    }
  }

  const selectArticle = (article) => {
    const articleId = getArticleId(article)
    const articlePrice = getArticlePrice(article)
    const articleName = getArticleName(article)
    
    console.log('🎯 Selecting article:', { articleId, articlePrice, articleName })
    
    form.value.articuloId = articleId
    selectedArticle.value = article
    searchTerm.value = articleName
    showDropdown.value = false

    // Auto-fill price if not manually set
    if (!form.value.precioUnitario && articlePrice) {
      form.value.precioUnitario = articlePrice
    }
  }

  const clearArticle = () => {
    form.value.articuloId = null
    selectedArticle.value = null
    searchTerm.value = ''
    form.value.precioUnitario = null
  }

  const formatCurrency = (amount) => {
    if (amount == null || amount == undefined) return 'S/ 0.00'
    return new Intl.NumberFormat('es-PE', {
      style: 'currency',
      currency: 'PEN',
    }).format(amount)
  }

  const handleSubmit = async () => {
    if (!isFormValid.value) {
      toast.add({
        severity: 'warn',
        summary: 'Validación',
        detail: 'Por favor complete todos los campos requeridos',
        life: 3000,
      })
      return
    }

    try {
      // Prepare consumption data with proper casing for C# DTO
      const consumptionData = {
        articuloId: form.value.articuloId, // Keep camelCase for axios
        cantidad: form.value.cantidad,
        precioUnitario: form.value.precioUnitario || getArticlePrice(selectedArticle.value) || null,
        habitacionId: form.value.habitacionId || null,
        reservaId: form.value.reservaId || null,
        tipoConsumo: form.value.tipoConsumo || 'Servicio',
        observaciones: form.value.observaciones || null,
      }

      console.log('📤 Sending consumption data:', consumptionData)

      const result = await createConsumption(consumptionData)

      console.log('✅ Consumption created:', result)

      toast.add({
        severity: 'success',
        summary: 'Éxito',
        detail: 'Consumo registrado exitosamente',
        life: 5000,
      })

      emit('created', result)
    } catch (error) {
      console.error('❌ Error creating consumption:', error)
      
      const errorMessage = error?.response?.data?.message || 
                          error?.response?.data?.errors?.[0] ||
                          error?.message || 
                          'Error al registrar el consumo'
      
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: errorMessage,
        life: 5000,
      })
    }
  }

  // Close dropdown when clicking outside
  const handleClickOutside = (event) => {
    if (!event.target.closest('.relative')) {
      showDropdown.value = false
    }
  }

  // Load articles from API
  const loadArticles = async () => {
    loadingArticles.value = true
    try {
      const response = await ArticleService.getArticles()
      console.log('📦 Articles response:', response)
      
      if (response.isSuccess && response.data) {
        availableArticles.value = response.data
        console.log(`✅ Loaded ${response.data.length} articles`)
      } else {
        console.warn('⚠️ Failed to load articles:', response.message)
        toast.add({
          severity: 'warn',
          summary: 'Advertencia',
          detail: 'No se pudieron cargar los artículos disponibles',
          life: 3000,
        })
      }
    } catch (error) {
      console.error('❌ Error loading articles:', error)
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Error al cargar los artículos',
        life: 3000,
      })
    } finally {
      loadingArticles.value = false
    }
  }

  // Load rooms from API
  const loadRooms = async () => {
    loadingRooms.value = true
    try {
      const response = await HabitacionService.getRooms()
      console.log('🏠 Rooms response:', response)
      
      if (response.isSuccess && response.data) {
        availableRooms.value = response.data
        console.log(`✅ Loaded ${response.data.length} rooms`)
      } else {
        console.warn('⚠️ Failed to load rooms:', response.message)
      }
    } catch (error) {
      console.error('❌ Error loading rooms:', error)
    } finally {
      loadingRooms.value = false
    }
  }

  onMounted(async () => {
    document.addEventListener('click', handleClickOutside)
    await Promise.all([loadArticles(), loadRooms()])
  })

  watch(searchTerm, (newValue) => {
    if (searchDebounceTimer) {
      clearTimeout(searchDebounceTimer)
    }

    if (newValue) {
      searchDebounceTimer = setTimeout(() => {
        showDropdown.value = true
      }, 300)
    } else {
      showDropdown.value = false
    }
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