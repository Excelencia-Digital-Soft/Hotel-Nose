<template>
  <Teleport to="body" class="overflow-hidden">
    <Transition name="modal-outer" appear>
      <div
        class="fixed inset-0 bg-black/60 backdrop-blur-xl z-50 flex items-center justify-center px-4"
      >
        <Transition name="modal-inner">
          <div class="glass-container w-full max-w-6xl max-h-[90vh] flex flex-col p-6 relative">
            <!-- Header -->
            <div class="flex items-center justify-between mb-6">
              <h2
                class="text-2xl font-bold bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 bg-clip-text text-transparent lexend-exa"
              >
                Gestión de Inventario
              </h2>
              <button
                @click="closeModal"
                class="glass-action-button text-red-400 hover:text-red-300 w-10 h-10"
                title="Cerrar"
              >
                <i class="pi pi-times text-lg"></i>
              </button>
            </div>

            <!-- Tab Navigation -->
            <TabNavigation :tabs="tabs" :activeTab="activeTab" @tab-changed="handleTabChange" />
            <!-- Main Content -->
            <div class="flex-1 overflow-y-auto">
              <!-- Current Inventory Tab -->
              <div v-if="activeTab === 'current'" class="glass-card p-4 h-full">
                <div
                  class="h-full overflow-y-auto scrollbar-thin scrollbar-thumb-white/20 scrollbar-track-transparent"
                >
                  <!-- Inventory Grid -->
                  <div
                    v-if="!isLoading && inventario.length"
                    class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4"
                  >
                    <div
                      v-for="item in inventario"
                      :key="item.inventarioId"
                      class="glass-item-card group"
                    >
                      <!-- Image container with glow effect -->
                      <div class="relative mb-3">
                        <div
                          class="absolute inset-0 bg-gradient-to-r from-primary-400 to-secondary-400 rounded-xl blur-md opacity-20 group-hover:opacity-40 transition-opacity"
                        ></div>
                        <div
                          class="relative w-full h-20 bg-white/5 flex items-center justify-center rounded-xl border border-white/20 overflow-hidden"
                        >
                          <img
                            :src="item.articulo.imageUrl || '../assets/image59.svg'"
                            :alt="item.articulo.articuloNombre"
                            class="w-full h-full object-cover"
                          />
                        </div>
                      </div>

                      <!-- Article details -->
                      <div class="text-center">
                        <h3
                          class="text-white font-semibold text-sm mb-1 truncate"
                          :title="item.articulo.nombreArticulo"
                        >
                          {{ item.articulo.nombreArticulo }}
                        </h3>
                        <div class="glass-badge inline-block text-xs">
                          <i class="pi pi-box mr-1"></i>
                          {{ item.cantidad }}
                        </div>
                      </div>
                    </div>
                  </div>

                  <!-- Loading state -->
                  <div v-else-if="isLoading" class="flex flex-col items-center justify-center h-64">
                    <div
                      class="w-8 h-8 border-2 border-primary-400 border-t-transparent rounded-full animate-spin mb-4"
                    ></div>
                    <p class="text-white/70">Cargando inventario...</p>
                  </div>

                  <!-- No items state -->
                  <div
                    v-else-if="!inventario.length"
                    class="flex flex-col items-center justify-center h-64"
                  >
                    <i class="pi pi-inbox text-4xl text-white/50 mb-4 block"></i>
                    <p class="text-white/70 text-lg mb-2">No hay artículos en el inventario</p>
                    <p class="text-white/50 text-sm">
                      Usa la pestaña "Agregar Artículos" para añadir elementos
                    </p>
                  </div>
                </div>
              </div>

              <!-- Add Articles Tab -->
              <div v-else-if="activeTab === 'add'" class="glass-card p-4 h-full">
                <div
                  class="h-full overflow-y-auto scrollbar-thin scrollbar-thumb-white/20 scrollbar-track-transparent"
                >
                  <!-- Search and Filter -->
                  <div class="mb-4 flex gap-4">
                    <div class="flex-1">
                      <input
                        v-model="searchQuery"
                        type="text"
                        placeholder="Buscar artículos..."
                        class="glass-input w-full py-2 px-4 rounded-xl"
                      />
                    </div>
                    <button
                      v-if="searchQuery"
                      @click="clearSearch"
                      class="glass-action-button px-3"
                      title="Limpiar búsqueda"
                    >
                      <i class="pi pi-times"></i>
                    </button>
                  </div>

                  <!-- Available Articles Grid -->
                  <div
                    v-if="!isLoadingArticles && filteredAvailableArticles.length"
                    class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4"
                  >
                    <div
                      v-for="article in filteredAvailableArticles"
                      :key="article.articuloId"
                      class="glass-item-card group cursor-pointer relative"
                      @click="selectArticle(article)"
                      :class="{
                        'ring-2 ring-primary-400 bg-white/15': selectedArticles.some(
                          (s) => s.articuloId === article.articuloId
                        ),
                      }"
                    >
                      <!-- Image container -->
                      <div class="relative mb-3">
                        <div
                          class="absolute inset-0 bg-gradient-to-r from-accent-400 to-primary-400 rounded-xl blur-md opacity-20 group-hover:opacity-40 transition-opacity"
                        ></div>
                        <div
                          class="relative w-full h-20 bg-white/5 flex items-center justify-center rounded-xl border border-white/20 overflow-hidden"
                        >
                          <img
                            :src="article.imageUrl || '../assets/image59.svg'"
                            :alt="article.nombreArticulo"
                            class="w-full h-full object-cover"
                          />
                        </div>
                      </div>

                      <!-- Article details -->
                      <div class="text-center">
                        <h3
                          class="text-white font-semibold text-sm mb-1 truncate"
                          :title="article.nombreArticulo"
                        >
                          {{ article.nombreArticulo }}
                        </h3>
                        <div class="glass-badge inline-block text-xs mb-2">
                          ${{ article.precio }}
                        </div>

                        <!-- Quantity input -->
                        <input
                          v-if="selectedArticles.some((s) => s.articuloId === article.articuloId)"
                          v-model.number="articleQuantities[article.articuloId]"
                          type="number"
                          min="1"
                          placeholder="Cantidad"
                          class="glass-input w-full text-center text-sm py-1 px-2 rounded-lg"
                          @click.stop
                        />
                      </div>
                    </div>
                  </div>

                  <!-- Loading articles state -->
                  <div
                    v-else-if="isLoadingArticles"
                    class="flex flex-col items-center justify-center h-64"
                  >
                    <div
                      class="w-8 h-8 border-2 border-accent-400 border-t-transparent rounded-full animate-spin mb-4"
                    ></div>
                    <p class="text-white/70">Cargando artículos disponibles...</p>
                  </div>

                  <!-- No articles available -->
                  <div
                    v-else-if="!filteredAvailableArticles.length"
                    class="flex flex-col items-center justify-center h-64"
                  >
                    <i class="pi pi-search text-4xl text-white/50 mb-4 block"></i>
                    <p class="text-white/70 text-lg mb-2">
                      {{
                        searchQuery ? 'No se encontraron artículos' : 'No hay artículos disponibles'
                      }}
                    </p>
                    <p class="text-white/50 text-sm">
                      {{
                        searchQuery
                          ? 'Intenta con otros términos de búsqueda'
                          : 'Todos los artículos ya están en el inventario'
                      }}
                    </p>
                  </div>
                </div>
              </div>
            </div>

            <!-- Footer Actions -->
            <div class="flex justify-between items-center mt-6 pt-4 border-t border-white/20">
              <div class="text-white/60 text-sm">
                <span v-if="activeTab === 'current'">
                  {{ inventario.length }} artículo{{ inventario.length !== 1 ? 's' : '' }} en
                  inventario
                </span>
                <span v-else>
                  {{ selectedArticles.length }} artículo{{
                    selectedArticles.length !== 1 ? 's' : ''
                  }}
                  seleccionado{{ selectedArticles.length !== 1 ? 's' : '' }}
                </span>
              </div>

              <div class="flex gap-3">
                <button
                  v-if="activeTab === 'add' && selectedArticles.length"
                  @click="clearSelections"
                  class="glass-button py-2 px-4 rounded-xl font-medium transition-all duration-300"
                >
                  <i class="pi pi-times mr-2"></i>
                  Limpiar Selección
                </button>

                <button
                  v-if="activeTab === 'add' && selectedArticles.length"
                  @click="addSelectedArticles"
                  :disabled="isAddingArticles"
                  class="glass-button-primary py-2 px-4 rounded-xl font-medium transition-all duration-300 hover:scale-105 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  <i class="pi pi-check mr-2" v-if="!isAddingArticles"></i>
                  <div
                    class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin mr-2"
                    v-else
                  ></div>
                  {{ isAddingArticles ? 'Agregando...' : 'Agregar Seleccionados' }}
                </button>
              </div>
            </div>
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
  import { onMounted, ref, computed } from 'vue'
  import TabNavigation from './TabNavigation.vue'
  import InventarioService from '../services/inventarioService'
  import ArticleService from '../services/ArticleService'
  import { fetchImage } from '../services/imageService'
  import { useAuthStore } from '../store/auth.js'

  const emits = defineEmits(['close-modal'])
  const props = defineProps({
    room: Object,
  })

  const habitacionID = ref(props.room.habitacionId)
  const authStore = useAuthStore()

  // Tab management
  const activeTab = ref('current')
  const tabs = [
    { id: 'current', label: 'Inventario Actual', icon: 'pi pi-box' },
    { id: 'add', label: 'Agregar Artículos', icon: 'pi pi-plus' },
  ]

  // Current inventory state
  const isLoading = ref(false)
  const inventario = ref([])

  // Add articles state
  const isLoadingArticles = ref(false)
  const isAddingArticles = ref(false)
  const availableArticles = ref([])
  const selectedArticles = ref([])
  const articleQuantities = ref({})
  const searchQuery = ref('')

  // Computed properties
  const filteredAvailableArticles = computed(() => {
    if (!searchQuery.value.trim()) {
      return availableArticles.value
    }
    const query = searchQuery.value.toLowerCase()
    return availableArticles.value.filter((article) =>
      article.nombreArticulo.toLowerCase().includes(query)
    )
  })

  // Tab management
  const handleTabChange = (tabId) => {
    activeTab.value = tabId
    if (tabId === 'add' && availableArticles.value.length === 0) {
      fetchAvailableArticles()
    }
  }

  // Current inventory methods
  const fetchInventario = async () => {
    isLoading.value = true
    try {
      console.log('Fetching inventory for room:', habitacionID.value)

      // Use V1 API endpoint through the service
      const response = await InventarioService.getRoomInventory(habitacionID.value)

      if (response && response.isSuccess && response.data) {
        console.log('V1 Inventory API response:', response.data)

        // Process inventory items and fetch images - articuloImagenUrl
        const baseUrl = import.meta.env.VITE_API_BASE_URL || ''
        inventario.value = response.data.map((v1Item) => {
          // Transform V1 structure to component-expected format
          const componentItem = InventarioService.adaptV1ToComponent(v1Item)
          // Fetch image for the article
          componentItem.articulo.imageUrl = componentItem.articulo.imageUrl
            ? `${baseUrl}uploads/${componentItem.articulo.imageUrl}`
            : '../assets/image59.svg'
          return componentItem
        })

        console.log('V1 Inventory loaded successfully:', inventario.value)

        // Refresh available articles if needed
        if (activeTab.value === 'add') {
          filterAvailableArticles()
        }
      } else {
        console.error('Error en respuesta de API V1:', response)
        throw new Error(response?.message || 'Error loading inventory')
      }
    } catch (error) {
      console.error('Error al obtener el inventario V1:', error)
      inventario.value = []
    } finally {
      isLoading.value = false
    }
  }

  // Add articles methods
  const fetchAvailableArticles = async () => {
    isLoadingArticles.value = true
    try {
      // Fetch all articles using V1 API
      const response = await ArticleService.getArticles()

      if (response && response.isSuccess && response.data) {
        console.log('V1 Articles API response:', response.data)
        const baseUrl = import.meta.env.VITE_API_BASE_URL || ''

        const articlesWithImages = response.data.map((article) => {
          return {
            ...article,
            imageUrl: article.imagenUrl
              ? `${baseUrl}uploads/${article.imagenUrl}`
              : '../assets/image59.svg',
          }
        })

        // // Process articles and fetch images
        // const articlesWithImages = await Promise.all(
        //   response.data.map(async (article) => {
        //     try {
        //       const imageUrl = await fetchImage(article.articuloId);
        //       return {
        //         ...article,
        //         imageUrl
        //       };
        //     } catch (imageError) {
        //       console.warn(`Failed to fetch image for article ${article.articuloId}:`, imageError);
        //       return {
        //         ...article,
        //         imageUrl: '../assets/sin-imagen.png'
        //       };
        //     }
        //   })
        // );

        availableArticles.value = articlesWithImages
        filterAvailableArticles()

        console.log('V1 Articles loaded successfully:', availableArticles.value)
      } else {
        console.error('Error en respuesta de API V1:', response)
      }
    } catch (error) {
      console.error('Error al obtener los artículos V1:', error)
      availableArticles.value = []
    } finally {
      isLoadingArticles.value = false
    }
  }

  const filterAvailableArticles = () => {
    if (inventario.value.length > 0) {
      const currentItemIds = inventario.value.map((item) => item.articulo.articuloId)
      availableArticles.value = availableArticles.value.filter(
        (article) => !currentItemIds.includes(article.articuloId)
      )
    }
  }

  const selectArticle = (article) => {
    const index = selectedArticles.value.findIndex((s) => s.articuloId === article.articuloId)
    if (index !== -1) {
      // Deselect
      selectedArticles.value.splice(index, 1)
      delete articleQuantities.value[article.articuloId]
    } else {
      // Select
      selectedArticles.value.push(article)
      articleQuantities.value[article.articuloId] = 1
    }
  }

  const clearSearch = () => {
    searchQuery.value = ''
  }

  const clearSelections = () => {
    selectedArticles.value = []
    articleQuantities.value = {}
  }

  const addSelectedArticles = async () => {
    isAddingArticles.value = true
    try {
      // Add articles one by one using V1 API
      for (const article of selectedArticles.value) {
        const quantity = articleQuantities.value[article.articuloId] || 1

        const inventoryData = {
          articuloId: article.articuloId,
          cantidad: quantity,
          locationType: 1, // Room type from InventoryLocationType enum
        }

        await InventarioService.addRoomInventory(habitacionID.value, inventoryData)
      }

      // Clear selections
      clearSelections()

      // Refresh inventory
      await fetchInventario()

      console.log('Articles added successfully')
    } catch (error) {
      console.error('Error adding articles to inventory:', error)
    } finally {
      isAddingArticles.value = false
    }
  }
  const closeModal = () => {
    emits('close-modal')
  }

  onMounted(() => {
    // Fetch inventory data on component mount
    fetchInventario()
  })
</script>

<style scoped>
  /* Glassmorphism classes following project guidelines */
  .glass-container {
    @apply bg-white/5 backdrop-blur-2xl border border-white/20 rounded-3xl;
  }

  .glass-card {
    @apply bg-white/10 backdrop-blur-md border border-white/20 rounded-xl;
  }

  .glass-item-card {
    @apply bg-white/5 backdrop-blur-sm border border-white/20 rounded-xl p-4 hover:bg-white/10 transition-all duration-300;
  }

  .glass-badge {
    @apply bg-white/10 backdrop-blur-sm border border-white/30 rounded-lg px-2 py-1 text-white;
  }

  .glass-button-primary {
    @apply bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 
           hover:from-primary-500 hover:via-secondary-500 hover:to-accent-500 
           text-white backdrop-blur-sm border border-white/30;
  }

  .glass-action-button {
    @apply bg-white/10 hover:bg-white/20 backdrop-blur-sm border border-white/30 
           rounded-lg flex items-center justify-center transition-all duration-200;
  }

  .glass-input {
    @apply bg-white/10 backdrop-blur-sm border border-white/30 rounded-lg 
           text-white placeholder-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-400 focus:border-transparent;
  }

  .glass-button {
    @apply bg-white/10 hover:bg-white/20 backdrop-blur-sm border border-white/30 
           rounded-lg text-white transition-all duration-200;
  }

  /* Animations */
  .modal-outer-enter-active,
  .modal-outer-leave-active {
    transition: opacity 0.3s cubic-bezier(0.52, 0.02, 0.19, 1.02);
  }

  .modal-outer-enter-from,
  .modal-outer-leave-to {
    opacity: 0;
  }

  .modal-inner-enter-active {
    transition: all 0.3s cubic-bezier(0.52, 0.02, 0.19, 1.02) 0.15s;
  }

  .modal-inner-leave-active {
    transition: all 0.3s cubic-bezier(0.52, 0.02, 0.19, 1.02);
  }

  .modal-inner-enter-from {
    opacity: 0;
    transform: scale(0.9);
  }

  .modal-inner-leave-to {
    opacity: 0;
    transform: scale(0.9);
  }

  .fade-enter-active,
  .fade-leave-active {
    transition: opacity 0.3s ease;
  }

  .fade-enter-from,
  .fade-leave-to {
    opacity: 0;
  }

  /* Custom scrollbar */
  .scrollbar-thin {
    scrollbar-width: thin;
  }

  .scrollbar-thumb-white\/20::-webkit-scrollbar-thumb {
    background-color: rgba(255, 255, 255, 0.2);
    border-radius: 9999px;
  }

  .scrollbar-track-transparent::-webkit-scrollbar-track {
    background-color: transparent;
  }

  ::-webkit-scrollbar {
    width: 6px;
    height: 6px;
  }
</style>
