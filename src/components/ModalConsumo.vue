<template>
  <Teleport to="body">
    <Transition name="modal-outer" appear>
      <div
        class="fixed inset-0 z-[60] bg-black/80 backdrop-blur-sm flex items-center justify-center p-4"
        @click.self="emits('close')"
      >
        <Transition name="modal-inner">
          <div
            class="relative w-full max-w-6xl h-[90vh] bg-gradient-to-br from-neutral-900 via-neutral-800 to-neutral-900 rounded-2xl shadow-2xl border border-primary-500/20 flex flex-col overflow-hidden"
            @click.stop
          >
            <!-- Header mejorado -->
            <div
              class="relative px-6 py-4 bg-gradient-to-r from-primary-600/10 via-secondary-600/10 to-accent-600/10 border-b border-neutral-700/50 flex-shrink-0"
            >
              <div class="flex items-center justify-between">
                <div class="flex items-center gap-3">
                  <div
                    class="w-10 h-10 rounded-lg bg-gradient-to-r from-primary-500 to-secondary-500 flex items-center justify-center"
                  >
                    <span class="material-symbols-outlined text-white text-lg">inventory_2</span>
                  </div>
                  <div>
                    <h2 class="text-xl font-bold text-white lexend-exa">
                      {{ consumoHabitacion ? 'Inventario de Habitación' : 'Inventario General' }}
                    </h2>
                    <p class="text-sm text-neutral-400">
                      {{
                        props.name ? `Cliente: ${props.name}` : 'Selecciona productos para agregar'
                      }}
                    </p>
                  </div>
                </div>

                <button
                  @click="emits('close')"
                  class="w-10 h-10 rounded-lg bg-red-500/10 hover:bg-red-500/20 border border-red-500/30 text-red-400 hover:text-red-300 transition-all duration-200 flex items-center justify-center group"
                >
                  <span
                    class="material-symbols-outlined group-hover:rotate-90 transition-transform duration-200"
                    >close</span
                  >
                </button>
              </div>
            </div>

            <!-- Filtros mejorados -->
            <div class="px-6 py-4 bg-neutral-800/50 border-b border-neutral-700/50 flex-shrink-0">
              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <!-- Búsqueda -->
                <div class="relative">
                  <label class="block text-sm font-medium text-neutral-300 mb-2">Categoría</label>

                  <div class="relative">
                    <div class="absolute top-2 left-0 pl-3 flex items-center pointer-events-none">
                      <span class="material-symbols-outlined text-neutral-400 text-lg">search</span>
                    </div>
                    <input
                      type="text"
                      v-model="keyword"
                      class="w-full pl-10 pr-4 py-3 bg-neutral-700/50 border border-neutral-600 rounded-lg text-white placeholder-neutral-400 focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition-all duration-200"
                      placeholder="Buscar productos..."
                    />

                    <!-- Clear button -->
                    <button
                      v-if="keyword"
                      @click="keyword = ''"
                      class="absolute top-2 right-0 pr-3 flex items-center text-neutral-400 hover:text-white transition-colors"
                    >
                      <span class="material-symbols-outlined text-lg">clear</span>
                    </button>
                  </div>
                </div>

                <!-- Filtro categoría -->
                <div class="relative">
                  <label class="block text-sm font-medium text-neutral-300 mb-2">Categoría</label>
                  <select
                    v-model="selectedCategory"
                    class="w-full px-4 py-3 bg-neutral-700/50 border border-neutral-600 rounded-lg text-white focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition-all duration-200 appearance-none cursor-pointer"
                  >
                    <option :value="null">Todas las categorías</option>
                    <option
                      v-for="category in categorias"
                      :key="category.categoriaId"
                      :value="category"
                    >
                      {{ category.nombreCategoria }}
                    </option>
                  </select>
                  <div
                    class="absolute inset-y-0 right-0 flex items-center pr-3 pointer-events-none mt-6"
                  >
                    <span class="material-symbols-outlined text-neutral-400 text-lg"
                      >expand_more</span
                    >
                  </div>
                </div>
              </div>

              <!-- Contador de productos -->
              <div class="mt-3 flex items-center justify-between text-sm">
                <span class="text-neutral-400">
                  {{ filteredProductos.length }} producto{{
                    filteredProductos.length !== 1 ? 's' : ''
                  }}
                  disponible{{ filteredProductos.length !== 1 ? 's' : '' }}
                </span>
                <span v-if="seleccionados.length > 0" class="text-primary-400 font-medium">
                  {{ seleccionados.length }} seleccionado{{ seleccionados.length !== 1 ? 's' : '' }}
                </span>
              </div>
            </div>

            <!-- Grid de productos con scroll fijo -->
            <div class="flex-1 min-h-0 flex flex-col">
              <!-- Contenedor con altura fija y scroll -->
              <div
                class="flex-1 overflow-y-auto px-6 py-4"
                style="min-height: 200px; max-height: calc(90vh - 400px)"
              >
                <!-- Estado vacío mejorado -->
                <div
                  v-if="filteredProductos.length === 0"
                  class="flex flex-col items-center justify-center min-h-[300px] text-center"
                >
                  <div
                    class="w-16 h-16 rounded-full bg-neutral-700/50 flex items-center justify-center mb-4"
                  >
                    <span class="material-symbols-outlined text-neutral-400 text-2xl"
                      >inventory</span
                    >
                  </div>
                  <h3 class="text-lg font-semibold text-white mb-2">
                    No hay productos disponibles
                  </h3>
                  <p class="text-neutral-400 max-w-md">
                    {{
                      consumoHabitacion
                        ? 'No se encontraron productos en el inventario de esta habitación'
                        : 'No se encontraron productos en el inventario general'
                    }}
                  </p>
                  <button
                    @click="cleanKeyword"
                    class="mt-4 px-4 py-2 bg-primary-600 hover:bg-primary-700 text-white rounded-lg transition-colors"
                  >
                    Limpiar filtros
                  </button>
                </div>

                <!-- Grid de productos con scroll visible -->
                <div
                  v-else
                  class="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-4 pb-4"
                >
                  <div
                    v-for="producto in filteredProductos"
                    :key="producto.articuloId"
                    @click="toggleSeleccion(producto)"
                    :class="[
                      'group relative bg-gradient-to-br from-neutral-800 to-neutral-900 rounded-xl p-4 cursor-pointer transition-all duration-300 border-2',
                      'hover:scale-105 hover:shadow-xl hover:shadow-primary-500/10',
                      seleccionados.some((p) => p.articuloId === producto.articuloId)
                        ? 'border-primary-500 bg-gradient-to-br from-primary-900/30 to-secondary-900/30 shadow-lg shadow-primary-500/20'
                        : 'border-neutral-700 hover:border-primary-500/50',
                    ]"
                  >
                    <!-- Badge de seleccionado -->
                    <div
                      v-if="seleccionados.some((p) => p.articuloId === producto.articuloId)"
                      class="absolute -top-2 -right-2 w-6 h-6 bg-primary-500 rounded-full flex items-center justify-center shadow-lg z-10"
                    >
                      <span class="material-symbols-outlined text-white text-sm">check</span>
                    </div>

                    <!-- Imagen del producto -->
                    <div
                      class="aspect-square bg-neutral-700/50 rounded-lg mb-3 overflow-hidden group-hover:scale-105 transition-transform duration-300"
                    >
                      <img
                        :src="producto.imageUrl || defaultProductImage"
                        :alt="producto.nombreArticulo"
                        class="w-full h-full object-cover"
                        @error="handleImageError"
                        loading="lazy"
                      />
                    </div>

                    <!-- Info del producto -->
                    <div class="space-y-2">
                      <h3
                        class="font-semibold text-white text-sm leading-tight group-hover:text-primary-300 transition-colors line-clamp-2"
                      >
                        {{ producto.nombreArticulo }}
                      </h3>

                      <div class="flex items-center justify-between text-xs">
                        <span class="text-neutral-400">Stock:</span>
                        <span
                          :class="[
                            'font-medium px-2 py-1 rounded-full',
                            producto.cantidad > 10
                              ? 'text-green-400 bg-green-400/10'
                              : producto.cantidad > 5
                                ? 'text-yellow-400 bg-yellow-400/10'
                                : 'text-red-400 bg-red-400/10',
                          ]"
                        >
                          {{ producto.cantidad }}
                        </span>
                      </div>

                      <div class="flex items-center justify-between text-xs">
                        <span class="text-neutral-400">Precio:</span>
                        <span class="font-bold text-primary-400">${{ producto.precio }}</span>
                      </div>
                    </div>

                    <!-- Overlay de hover -->
                    <div
                      class="absolute inset-0 bg-gradient-to-t from-primary-600/20 to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300 rounded-xl pointer-events-none"
                    ></div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Tabla de seleccionados mejorada -->
            <div
              v-if="seleccionados.length > 0"
              class="border-t border-neutral-700/50 bg-neutral-800/30 flex-shrink-0"
            >
              <div class="px-6 py-3 border-b border-neutral-700/30">
                <h3 class="text-sm font-semibold text-white flex items-center gap-2">
                  <span class="material-symbols-outlined text-primary-400">shopping_cart</span>
                  Productos Seleccionados ({{ seleccionados.length }})
                </h3>
              </div>

              <div class="max-h-40 overflow-y-auto">
                <TableRow
                  :selectedList="seleccionados"
                  @update:productList="actualizarSeleccionados"
                  class="bg-transparent"
                />
              </div>
            </div>

            <!-- Footer con acciones -->
            <div class="px-6 py-4 bg-neutral-800/50 border-t border-neutral-700/50 flex-shrink-0">
              <div class="flex items-center justify-between">
                <!-- Info de selección -->
                <div class="text-sm text-neutral-400">
                  <span v-if="seleccionados.length === 0">Selecciona productos para continuar</span>
                  <span v-else>
                    {{ seleccionados.length }} producto{{ seleccionados.length !== 1 ? 's' : '' }} •
                    Total: ${{
                      seleccionados
                        .reduce((sum, item) => sum + item.precio * item.cantidad, 0)
                        .toFixed(2)
                    }}
                  </span>
                </div>

                <!-- Botones de acción -->
                <div class="flex gap-3">
                  <button
                    @click="emits('close')"
                    class="px-6 py-2.5 bg-neutral-600 hover:bg-neutral-500 text-white rounded-lg font-medium transition-colors duration-200 flex items-center gap-2"
                  >
                    <span class="material-symbols-outlined text-lg">close</span>
                    Cancelar
                  </button>

                  <button
                    @click="confirmarAccion"
                    :disabled="seleccionados.length === 0 || isLoading"
                    class="relative px-6 py-2.5 bg-gradient-to-r from-primary-600 to-secondary-600 hover:from-primary-700 hover:to-secondary-700 disabled:from-neutral-600 disabled:to-neutral-600 text-white rounded-lg font-medium transition-all duration-200 flex items-center gap-2 disabled:cursor-not-allowed min-w-[120px] justify-center"
                  >
                    <template v-if="!isLoading">
                      <span class="material-symbols-outlined text-lg">check</span>
                      Confirmar
                    </template>

                    <template v-else>
                      <div
                        class="animate-spin w-5 h-5 border-2 border-white/30 border-t-white rounded-full"
                      ></div>
                      Procesando...
                    </template>
                  </button>
                </div>
              </div>
            </div>
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
  import { onMounted, ref, computed, type Ref, type ComputedRef } from 'vue'
  import { useAuthStore } from '../store/auth.js'
  import axiosClient from '../axiosClient'
  import { removeTrailingSlash } from '../utils/url-helpers'
  import defaultProductImage from '../assets/sin-imagen.png'
  import TableRow from './TableRow.vue'
  import type { ApiResponse, InventoryDto, InventoryItem, CategoriaDto } from '../types'

  const authStore = useAuthStore()

  interface Props {
    name?: string
    habitacionID?: number
    consumoHabitacion?: boolean
  }

  const props = defineProps<Props>()

  const emits = defineEmits(['close', 'confirmaAccion'])

  // Función para recargar el inventario
  const recargarInventario = async (): Promise<void> => {
    await fetchArticulos()
  }

  // Estados reactivos
  const isLoading = ref<boolean>(false)
  const productos = ref<InventoryItem[]>([])
  const categorias = ref<CategoriaDto[]>([])
  const seleccionados = ref<InventoryItem[]>([])
  const selectedCategory = ref<CategoriaDto | null>(null)
  const keyword = ref<string>('')
  const getInv = ref<string>('')

  // Computed para productos filtrados
  const filteredProductos: ComputedRef<InventoryItem[]> = computed(() => {
    return productos.value.filter((producto) => {
      const matchesCategory =
        !selectedCategory.value || producto.categoriaID === selectedCategory.value.categoriaId
      const matchesKeyword =
        !keyword.value ||
        producto.nombreArticulo.toLowerCase().includes(keyword.value.toLowerCase())
      return matchesCategory && matchesKeyword
    })
  })

  // Funciones del ciclo de vida
  onMounted(async () => {
    try {
      getDatosLogin()

      if (!props.consumoHabitacion) {
        getInv.value = `/api/v1/inventory/general`
      } else {
        getInv.value = `/api/v1/inventory/rooms/${props.habitacionID}`
      }

      // Clear previous selections when mounting
      seleccionados.value = []

      await Promise.all([fetchCategorias(), fetchArticulos()])
    } catch (error) {
      console.error('Error initializing ModalConsumo:', error)
    }
  })

  const cleanKeyword = (): void => {
    keyword.value = ''
    selectedCategory.value = null
  }

  // Efficient image URL function (same as ArticleCreate)
  const getArticleImage = (url?: string): string => {
    try {
      if (url) {
        const baseUrl = removeTrailingSlash(import.meta.env.VITE_API_BASE_URL || '')

        return `${baseUrl}/uploads/${url}`
      }
    } catch (error) {
      console.error('Error getting article image:', error)
    }

    return defaultProductImage
  }

  // Funciones de datos
  const fetchArticulos = async (): Promise<void> => {
    try {
      const response = await axiosClient.get<ApiResponse<InventoryDto[]>>(getInv.value)

      let inventoryData: InventoryDto[] = []
      if (response.data?.isSuccess) {
        inventoryData = response.data.data || []
      }
      // Filter items with stock and map to component format
      const articlesWithStock = inventoryData.filter((item) => item.cantidad > 0)

      productos.value = articlesWithStock.map((inventoryItem): InventoryItem => {
        return {
          articuloId: inventoryItem.articuloId,
          nombreArticulo: inventoryItem.articuloNombre,
          precio: inventoryItem.articuloPrecio,
          cantidad: inventoryItem.cantidad,
          maximo: inventoryItem.cantidad,
          categoriaID: inventoryItem.categoriaId || undefined,
          descripcion: inventoryItem.articuloDescripcion,
          imageUrl: getArticleImage(inventoryItem.articuloImagenUrl),
        }
      })
    } catch (error) {
      console.error('Error al obtener los artículos:', error)
      productos.value = []
    }
  }

  const fetchCategorias = async (): Promise<void> => {
    try {
      const response = await axiosClient.get<ApiResponse<CategoriaDto[]>>(`/api/v1/categorias`)
      console.log(response.data)
      if (response.data.isSuccess && response.data.data) {
        categorias.value = response.data.data
      }
    } catch (error) {
      console.error('Error al obtener las categorías:', error)
    }
  }

  // Funciones de interacción
  const handleImageError = (event: Event): void => {
    const target = event.target as HTMLImageElement
    target.src = defaultProductImage
  }

  const toggleSeleccion = (producto: InventoryItem): void => {
    const index = seleccionados.value.findIndex((p) => p.articuloId === producto.articuloId)
    if (index === -1) {
      // Agregar producto a la selección
      seleccionados.value.push({
        articuloId: producto.articuloId,
        nombreArticulo: producto.nombreArticulo,
        precio: producto.precio,
        cantidad: 1,
        maximo: producto.cantidad,
        categoriaID: producto.categoriaID,
        descripcion: producto.descripcion,
        imageUrl: producto.imageUrl,
      })
    } else {
      // Remover producto de la selección - no necesitamos restaurar stock aquí
      // porque el stock se descuenta solo al confirmar
      seleccionados.value.splice(index, 1)
    }
  }

  const actualizarSeleccionados = (nuevaLista: InventoryItem[]): void => {
    seleccionados.value = nuevaLista
  }

  const confirmarAccion = (): void => {
    if (seleccionados.value.length === 0) return

    isLoading.value = true

    // Descontar stock localmente para reflejar el consumo inmediatamente
    seleccionados.value.forEach((itemSeleccionado) => {
      const producto = productos.value.find((p) => p.articuloId === itemSeleccionado.articuloId)
      if (producto) {
        producto.cantidad = Math.max(0, producto.cantidad - itemSeleccionado.cantidad)
      }
    })

    // Enviar productos seleccionados
    emits('confirmaAccion', seleccionados.value)

    // Limpiar selecciones
    seleccionados.value = []

    emits('close')
  }

  const InstitucionID = ref<number | null>(null)

  function getDatosLogin(): void {
    InstitucionID.value = authStore.institucionID
  }

  // Exponer funciones para el componente padre
  defineExpose({
    recargarInventario,
  })
</script>

<style scoped>
  /* Transiciones mejoradas */
  .modal-outer-enter-active,
  .modal-outer-leave-active {
    transition: opacity 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  }

  .modal-outer-enter-from,
  .modal-outer-leave-to {
    opacity: 0;
  }

  .modal-inner-enter-active {
    transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
  }

  .modal-inner-leave-active {
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  }

  .modal-inner-enter-from {
    opacity: 0;
    transform: scale(0.95) translateY(-20px);
  }

  .modal-inner-leave-to {
    opacity: 0;
    transform: scale(0.95) translateY(-20px);
  }

  /* Scrollbar personalizado mejorado */
  .overflow-y-auto {
    scrollbar-width: thin;
    scrollbar-color: rgba(156, 163, 175, 0.3) transparent;
  }

  .overflow-y-auto::-webkit-scrollbar {
    width: 8px;
  }

  .overflow-y-auto::-webkit-scrollbar-track {
    background: rgba(0, 0, 0, 0.1);
    border-radius: 4px;
  }

  .overflow-y-auto::-webkit-scrollbar-thumb {
    background: rgba(156, 163, 175, 0.3);
    border-radius: 4px;
    border: 1px solid rgba(0, 0, 0, 0.1);
  }

  .overflow-y-auto::-webkit-scrollbar-thumb:hover {
    background: rgba(156, 163, 175, 0.5);
  }

  .overflow-y-auto::-webkit-scrollbar-thumb:active {
    background: rgba(156, 163, 175, 0.7);
  }

  /* Animación de hover para las cards */
  @keyframes pulse-border {
    0%,
    100% {
      border-color: rgb(var(--primary-500));
    }
    50% {
      border-color: rgb(var(--secondary-500));
    }
  }

  .group:hover .animate-pulse-border {
    animation: pulse-border 2s infinite;
  }

  /* Utilidad para limitar líneas de texto */
  .line-clamp-2 {
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
  }

  /* Asegurar que el contenedor principal tenga altura fija */
  .modal-container {
    height: 90vh;
    max-height: 90vh;
  }

  /* Estilos para mejor contraste del scroll */
  .scroll-container {
    background: 
    /* Shadow covers */
      linear-gradient(rgba(23, 23, 23, 1) 30%, transparent),
      linear-gradient(transparent, rgba(23, 23, 23, 1) 70%) 0 100%,
      /* Shadows */ radial-gradient(farthest-side at 50% 0, rgba(0, 0, 0, 0.2), transparent),
      radial-gradient(farthest-side at 50% 100%, rgba(0, 0, 0, 0.2), transparent) 0 100%;
    background-repeat: no-repeat;
    background-size:
      100% 40px,
      100% 40px,
      100% 14px,
      100% 14px;
    background-attachment: local, local, scroll, scroll;
  }
</style>
