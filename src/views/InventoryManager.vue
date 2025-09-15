<template>
  <div
    class="min-h-screen bg-gradient-to-br from-neutral-900 via-neutral-800 to-neutral-900 p-4 lg:p-6"
  >
    <!-- Header Section -->
    <div class="glass-container mb-6 p-6 transform hover:scale-[1.02] transition-all duration-300">
      <div
        class="flex flex-col lg:flex-row lg:items-center lg:justify-between space-y-4 lg:space-y-0"
      >
        <div class="text-center lg:text-left">
          <div class="flex items-center justify-center lg:justify-start mb-2">
            <div class="bg-gradient-to-r from-primary-400 to-accent-400 p-3 rounded-full mr-3">
              <i class="pi pi-warehouse text-white text-2xl"></i>
            </div>
            <h1 class="text-3xl lg:text-4xl font-bold text-white">📦 Gestión de Inventario</h1>
          </div>
          <p class="text-gray-300 text-lg">
            Controla y actualiza tu stock de manera fácil y eficiente 🎯
          </p>
        </div>

        <!-- Quick Stats -->
        <div class="glass-card p-4">
          <div class="grid grid-cols-3 gap-4 text-center">
            <div class="transform hover:scale-110 transition-transform duration-200">
              <div
                class="bg-gradient-to-r from-primary-400 to-primary-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2"
              >
                <span class="font-bold text-lg">{{ totalItems }}</span>
              </div>
              <p class="text-xs text-gray-300">Productos</p>
            </div>
            <div class="transform hover:scale-110 transition-transform duration-200">
              <div
                class="bg-gradient-to-r from-secondary-400 to-secondary-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2"
              >
                <span class="font-bold text-lg">{{ totalStock }}</span>
              </div>
              <p class="text-xs text-gray-300">Stock Total</p>
            </div>
            <div class="transform hover:scale-110 transition-transform duration-200">
              <div
                class="bg-gradient-to-r from-accent-400 to-accent-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2"
              >
                <span class="font-bold text-lg">{{ lowStockCount }}</span>
              </div>
              <p class="text-xs text-gray-300">Stock Bajo</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Search and Filter Section -->
    <div class="glass-container mb-6 p-6">
      <div class="flex items-center mb-4">
        <i class="pi pi-search text-accent-400 text-xl mr-2"></i>
        <h3 class="text-xl font-bold text-white">🔍 Buscar productos en inventario</h3>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <!-- Search -->
        <div class="md:col-span-2 relative">
          <i class="pi pi-search absolute left-4 top-4 text-gray-400"></i>
          <input
            v-model="keyword"
            type="text"
            class="glass-input w-full pl-12 pr-4 py-3"
            placeholder="🔎 Buscar por nombre..."
          />
          <div v-if="keyword" class="absolute right-4 top-4">
            <button @click="keyword = ''" class="text-gray-400 hover:text-white transition-colors">
              <i class="pi pi-times"></i>
            </button>
          </div>
        </div>

        <!-- Stock Filter -->
        <select v-model="stockFilter" class="glass-input px-4 py-3">
          <option value="all">📦 Todo el stock</option>
          <option value="low">⚠️ Stock bajo</option>
          <option value="empty">🚫 Sin stock</option>
          <option value="good">✅ Stock bueno</option>
        </select>

        <!-- Sort -->
        <select v-model="sortBy" class="glass-input px-4 py-3">
          <option value="name">🔤 Por nombre</option>
          <option value="stock-asc">📈 Stock menor</option>
          <option value="stock-desc">📉 Stock mayor</option>
        </select>
      </div>

      <!-- Quick Actions -->
      <div class="flex flex-wrap gap-2 mt-4">
        <span class="text-gray-300 text-sm">Acciones rápidas:</span>
        <button
          @click="showBulkEdit = !showBulkEdit"
          class="glass-button px-3 py-1 text-sm text-white hover:bg-white/20 transform hover:scale-105 transition-all"
          :class="{ 'bg-primary-500/50': showBulkEdit }"
        >
          ⚡ Edición masiva
        </button>
        <button
          @click="stockFilter = 'low'"
          class="glass-button px-3 py-1 text-sm text-white hover:bg-white/20 transform hover:scale-105 transition-all"
          :class="{ 'bg-yellow-500/50': stockFilter === 'low' }"
        >
          ⚠️ Ver stock bajo
        </button>
        <button
          @click="refreshInventory"
          :disabled="isLoading"
          class="glass-button px-3 py-1 text-sm text-white hover:bg-white/20 transform hover:scale-105 transition-all disabled:opacity-50"
        >
          <i :class="isLoading ? 'pi pi-spinner pi-spin' : 'pi pi-refresh'" class="mr-1"></i>
          Actualizar
        </button>
      </div>
    </div>

    <!-- Bulk Edit Panel -->
    <div v-if="showBulkEdit" class="glass-container mb-6 p-6">
      <div class="flex items-center justify-between mb-4">
        <div class="flex items-center">
          <i class="pi pi-cog text-primary-400 text-xl mr-2"></i>
          <h3 class="text-xl font-bold text-white">⚡ Edición Masiva</h3>
        </div>
        <button
          @click="showBulkEdit = false"
          class="glass-button px-3 py-2 text-white hover:text-gray-300"
        >
          <i class="pi pi-times"></i>
        </button>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div class="glass-card p-4">
          <label class="block text-white font-semibold mb-2">💯 Establecer cantidad fija</label>
          <div class="flex space-x-2">
            <input
              v-model.number="bulkAmount"
              type="number"
              min="0"
              class="glass-input flex-1 px-3 py-2"
              placeholder="Cantidad"
            />
            <button
              @click="applyBulkAction('set')"
              class="glass-button px-4 py-2 text-white hover:bg-white/20"
            >
              Aplicar
            </button>
          </div>
        </div>

        <div class="glass-card p-4">
          <label class="block text-white font-semibold mb-2">➕ Agregar al stock</label>
          <div class="flex space-x-2">
            <input
              v-model.number="bulkAmount"
              type="number"
              min="0"
              class="glass-input flex-1 px-3 py-2"
              placeholder="Cantidad"
            />
            <button
              @click="applyBulkAction('add')"
              class="glass-button px-4 py-2 text-white hover:bg-white/20"
            >
              Agregar
            </button>
          </div>
        </div>

        <div class="glass-card p-4">
          <label class="block text-white font-semibold mb-2">🗑️ Resetear a cero</label>
          <button
            @click="applyBulkAction('reset')"
            class="w-full glass-button py-2 text-white hover:text-red-300"
          >
            <i class="pi pi-refresh mr-2"></i>
            Resetear Todo
          </button>
        </div>
      </div>
    </div>

    <!-- Inventory Grid -->
    <div class="glass-container p-6">
      <div class="flex items-center justify-between mb-6">
        <div class="flex items-center">
          <i class="pi pi-list text-primary-400 text-xl mr-2"></i>
          <h3 class="text-xl font-bold text-white">
            {{
              filteredInventory.length > 0
                ? `📋 ${filteredInventory.length} productos en inventario`
                : '📦 Tu inventario'
            }}
          </h3>
        </div>

        <div v-if="hasChanges" class="glass-card px-3 py-1">
          <span class="text-yellow-400 text-sm font-semibold flex items-center">
            <i class="pi pi-exclamation-triangle mr-1"></i>
            {{ pendingChanges }} cambios pendientes
          </span>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="isLoading" class="text-center py-16">
        <div class="glass-card max-w-md mx-auto p-8">
          <div
            class="bg-gradient-to-r from-primary-400 to-accent-400 p-6 rounded-full mx-auto w-24 h-24 flex items-center justify-center mb-4"
          >
            <i class="pi pi-spinner pi-spin text-white text-4xl"></i>
          </div>
          <h3 class="text-xl text-white font-bold mb-2">🔄 Cargando inventario...</h3>
          <p class="text-gray-300">Obteniendo la información más reciente</p>
        </div>
      </div>

      <!-- Inventory Items -->
      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div
          v-for="item in filteredInventory"
          :key="item.inventoryId || item.inventarioId"
          class="glass-card p-4 hover:bg-white/15 transition-all duration-300 group transform hover:scale-105"
          :class="{
            'border-red-500/50': item.cantidad === 0,
            'border-yellow-500/50': item.cantidad > 0 && item.cantidad <= 5,
            'border-green-500/50': item.cantidad > 5,
          }"
        >
          <!-- Product Header -->
          <div class="flex items-start justify-between mb-4">
            <div class="flex-1">
              <h4 class="text-white font-semibold text-lg mb-1 line-clamp-2">
                {{ item.articuloNombre || item.articulo?.nombreArticulo || 'Producto sin nombre' }}
              </h4>
              <div class="flex items-center space-x-2">
                <span
                  class="px-2 py-1 rounded-full text-xs font-medium"
                  :class="getStockStatusClass(item.cantidad)"
                >
                  {{ getStockStatus(item.cantidad) }}
                </span>
              </div>
            </div>
            <div class="text-right">
              <p class="text-2xl font-bold" :class="getStockTextColor(item.cantidad)">
                {{ item.cantidad }}
              </p>
              <p class="text-xs text-gray-400">unidades</p>
            </div>
          </div>

          <!-- Stock Management -->
          <div class="space-y-3">
            <!-- Current vs New Stock -->
            <div class="glass-card p-3 bg-white/5">
              <div class="flex items-center justify-between mb-2">
                <span class="text-gray-300 text-sm">Stock actual:</span>
                <span class="text-white font-semibold">{{ item.cantidad }}</span>
              </div>
              <div class="flex items-center justify-between">
                <span class="text-gray-300 text-sm">Nuevo stock:</span>
                <div class="flex items-center space-x-2">
                  <button
                    @click="decrementStock(item)"
                    class="glass-button w-8 h-8 flex items-center justify-center text-white hover:text-red-300"
                  >
                    <i class="pi pi-minus text-xs"></i>
                  </button>
                  <input
                    v-model.number="item.newStock"
                    type="number"
                    min="0"
                    class="glass-input w-16 px-2 py-1 text-center text-white"
                    @input="markAsChanged(item)"
                  />
                  <button
                    @click="incrementStock(item)"
                    class="glass-button w-8 h-8 flex items-center justify-center text-white hover:text-green-300"
                  >
                    <i class="pi pi-plus text-xs"></i>
                  </button>
                </div>
              </div>
            </div>

            <!-- Quick Actions -->
            <div class="flex space-x-2">
              <button
                @click="
                  () => {
                    item.newStock = 0
                    markAsChanged(item)
                  }
                "
                class="flex-1 glass-button py-2 text-white hover:text-red-300 text-sm"
              >
                <i class="pi pi-ban mr-1"></i>
                Vaciar
              </button>
              <button
                @click="
                  () => {
                    item.newStock = 10
                    markAsChanged(item)
                  }
                "
                class="flex-1 glass-button py-2 text-white hover:text-blue-300 text-sm"
              >
                <i class="pi pi-plus mr-1"></i>
                Stock +10
              </button>
              <button
                @click="updateSingleItem(item)"
                :disabled="!hasItemChanged(item) || isUpdating"
                class="flex-1 glass-button py-2 text-white hover:text-green-300 text-sm disabled:opacity-50"
                :class="{ 'bg-green-500/30': hasItemChanged(item) }"
              >
                <i :class="isUpdating ? 'pi pi-spinner pi-spin' : 'pi pi-check'" class="mr-1"></i>
                {{ hasItemChanged(item) ? 'Guardar' : 'OK' }}
              </button>
            </div>
          </div>

          <!-- Change Indicator -->
          <div
            v-if="hasItemChanged(item)"
            class="mt-3 p-2 bg-yellow-500/20 rounded-lg border border-yellow-500/30"
          >
            <p class="text-yellow-300 text-xs flex items-center">
              <i class="pi pi-exclamation-triangle mr-1"></i>
              Cambio de {{ item.cantidad }} → {{ item.newStock }} pendiente
            </p>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div v-if="!isLoading && filteredInventory.length === 0" class="text-center py-16">
        <div class="glass-card max-w-md mx-auto p-8">
          <div class="mb-6">
            <div
              class="bg-gradient-to-r from-primary-400 to-accent-400 p-6 rounded-full mx-auto w-24 h-24 flex items-center justify-center mb-4"
            >
              <i class="pi pi-inbox text-white text-4xl"></i>
            </div>
            <h3 class="text-2xl text-white font-bold mb-2">
              {{ keyword ? '🔍 ¡No encontramos productos!' : '📦 ¡Inventario vacío!' }}
            </h3>
            <p class="text-gray-300 mb-6">
              {{
                keyword
                  ? 'Intenta con otros términos de búsqueda o ajusta los filtros'
                  : 'Parece que no hay productos en el inventario aún'
              }}
            </p>
          </div>

          <button
            v-if="keyword"
            @click="
              () => {
                keyword = ''
                stockFilter = 'all'
              }
            "
            class="glass-button text-white py-3 px-6 hover:bg-white/20 transform hover:scale-105 transition-all"
          >
            <i class="pi pi-refresh mr-2"></i>
            🔄 Limpiar filtros
          </button>

          <button
            v-else
            @click="refreshInventory"
            class="bg-gradient-to-r from-primary-400 to-accent-400 hover:from-primary-500 hover:to-accent-500 text-white font-bold py-3 px-6 rounded-lg transform hover:scale-105 transition-all duration-300"
          >
            <i class="pi pi-refresh mr-2"></i>
            🔄 Actualizar inventario
          </button>
        </div>
      </div>
    </div>

    <!-- Floating Action Buttons -->
    <div class="fixed bottom-6 right-6 flex flex-col space-y-3">
      <!-- Save All Changes -->
      <button
        v-if="hasChanges"
        @click="saveAllChanges"
        :disabled="isUpdating"
        class="bg-gradient-to-r from-green-400 to-green-500 hover:from-green-500 hover:to-green-600 text-white font-bold py-3 px-6 rounded-full shadow-lg transform hover:scale-105 transition-all duration-300 disabled:opacity-50"
      >
        <i :class="isUpdating ? 'pi pi-spinner pi-spin' : 'pi pi-save'" class="mr-2"></i>
        💾 Guardar Todo ({{ pendingChanges }})
      </button>

      <!-- Sync Inventory -->
      <button
        @click="actualizarInventario"
        :disabled="isLoading"
        class="bg-gradient-to-r from-primary-400 to-accent-400 hover:from-primary-500 hover:to-accent-500 text-white font-bold py-3 px-6 rounded-full shadow-lg transform hover:scale-105 transition-all duration-300 disabled:opacity-50"
      >
        <i :class="isLoading ? 'pi pi-spinner pi-spin' : 'pi pi-sync'" class="mr-2"></i>
        🔄 Sincronizar
      </button>
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
  import { useGeneralInventory } from '../composables/useRoomInventory'
  import { useGlobalAlerts } from '../composables/useInventoryAlerts'
  import Toast from 'primevue/toast'
  import ConfirmDialog from 'primevue/confirmdialog'

  // Composables
  const toast = useToast()
  const confirm = useConfirm()
  const authStore = useAuthStore()

  // New V1 composables
  const generalInventory = useGeneralInventory()
  const globalAlerts = useGlobalAlerts()

  // State (keeping existing for compatibility)
  const inventory = ref([])
  const isLoading = ref(true)
  const isUpdating = ref(false)
  const keyword = ref('')
  const stockFilter = ref('all')
  const sortBy = ref('name')
  const showBulkEdit = ref(false)
  const bulkAmount = ref(10)
  const changedItems = ref(new Set())

  const totalItems = computed(() => {
    return generalInventory.inventory.value?.length || 0
  })

  const totalStock = computed(() => {
    const items =
      generalInventory.inventory.value?.length > 0
        ? generalInventory.inventory.value
        : inventory.value

    if (!items || !Array.isArray(items)) return 0
    return items.reduce((sum, item) => sum + (item.cantidad || 0), 0)
  })

  const lowStockCount = computed(() => {
    const items =
      generalInventory.inventory.value?.length > 0
        ? generalInventory.inventory.value
        : inventory.value

    if (!items || !Array.isArray(items)) return 0
    return items.filter((item) => item.cantidad <= 5 && item.cantidad > 0).length
  })

  const filteredInventory = computed(() => {
    // Use V1 data if available, fallback to legacy
    const items =
      generalInventory.inventory.value?.length > 0
        ? generalInventory.inventory.value
        : inventory.value

    if (!items || !Array.isArray(items)) return []

    let filtered = [...items]

    // Search filter
    if (keyword.value) {
      const term = keyword.value.toLowerCase()
      filtered = filtered.filter((item) => {
        // Support both V1 and legacy structure
        const itemName = item.articuloNombre || item.articulo?.nombreArticulo || ''
        return itemName.toLowerCase().includes(term)
      })
    }

    // Stock filter
    switch (stockFilter.value) {
      case 'low':
        filtered = filtered.filter((item) => item.cantidad <= 5 && item.cantidad > 0)
        break
      case 'empty':
        filtered = filtered.filter((item) => item.cantidad === 0)
        break
      case 'good':
        filtered = filtered.filter((item) => item.cantidad > 5)
        break
    }

    // Sort
    filtered.sort((a, b) => {
      switch (sortBy.value) {
        case 'stock-asc':
          return a.cantidad - b.cantidad
        case 'stock-desc':
          return b.cantidad - a.cantidad
        case 'name':
        default:
          const nameA = a.articuloNombre || a.articulo?.nombreArticulo || ''
          const nameB = b.articuloNombre || b.articulo?.nombreArticulo || ''
          return nameA.localeCompare(nameB)
      }
    })

    return filtered
  })

  const hasChanges = computed(() => changedItems.value.size > 0)
  const pendingChanges = computed(() => changedItems.value.size)

  // Methods
  const showSuccess = (message) => {
    toast.add({
      severity: 'success',
      summary: 'Éxito',
      detail: message,
      life: 5000,
    })
  }

  const showError = (message) => {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: message,
      life: 5000,
    })
  }

  const showInfo = (message) => {
    toast.add({
      severity: 'info',
      summary: 'Información',
      detail: message,
      life: 5000,
    })
  }

  const getStockStatus = (cantidad) => {
    if (cantidad === 0) return '🚫 Sin stock'
    if (cantidad <= 5) return '⚠️ Stock bajo'
    return '✅ Stock bueno'
  }

  const getStockStatusClass = (cantidad) => {
    if (cantidad === 0) return 'bg-red-500/20 text-red-300 border border-red-500/30'
    if (cantidad <= 5) return 'bg-yellow-500/20 text-yellow-300 border border-yellow-500/30'
    return 'bg-green-500/20 text-green-300 border border-green-500/30'
  }

  const getStockTextColor = (cantidad) => {
    if (cantidad === 0) return 'text-red-400'
    if (cantidad <= 5) return 'text-yellow-400'
    return 'text-green-400'
  }

  const markAsChanged = (item) => {
    if (item.newStock !== item.cantidad) {
      changedItems.value.add(item.inventarioId)
    } else {
      changedItems.value.delete(item.inventarioId)
    }
  }

  const hasItemChanged = (item) => {
    return changedItems.value.has(item.inventarioId)
  }

  const incrementStock = (item) => {
    item.newStock = (item.newStock || 0) + 1
    markAsChanged(item)
  }

  const decrementStock = (item) => {
    if (item.newStock > 0) {
      item.newStock = (item.newStock || 0) - 1
      markAsChanged(item)
    }
  }

  const fetchInventory = async () => {
    const institucionID = authStore.institucionID
    if (!institucionID) {
      showError('❌ No se pudo obtener la información de la institución')
      return
    }

    isLoading.value = true

    try {
      await generalInventory.fetchInventory()

      if (generalInventory.inventory.value?.length > 0) {
        inventory.value = generalInventory.inventory.value.map((item) => ({
          ...item,
          inventarioId: item.inventoryId,
          newStock: item.cantidad, // Initialize editable stock quantity
          articulo: {
            articuloId: item.articuloId,
            nombreArticulo: item.articuloNombre,
            precio: item.articuloPrecio,
            descripcion: item.articuloDescripcion,
          },
        }))
        changedItems.value.clear()
        showSuccess('📦 Inventario V1 cargado correctamente')
        return
      }
    } catch (v1Error) {
      console.warn('V1 API failed, falling back to legacy:', v1Error)
    } finally {
      isLoading.value = false
    }
  }

  const updateSingleItem = async (item) => {
    if (!hasItemChanged(item)) return

    try {
      isUpdating.value = true
      await axiosClient.put('/UpdateStockGeneral', [
        {
          inventarioId: item.inventarioId,
          cantidad: item.newStock,
        },
      ])

      // Update local state
      item.cantidad = item.newStock
      changedItems.value.delete(item.inventarioId)

      showSuccess(`✅ Stock de "${item.articulo?.nombreArticulo}" actualizado correctamente`)
    } catch (error) {
      console.error('Error updating stock:', error)
      showError(`❌ Error al actualizar el stock de "${item.articulo?.nombreArticulo}"`)
    } finally {
      isUpdating.value = false
    }
  }

  const saveAllChanges = async () => {
    if (!hasChanges.value) return

    const confirmed = await new Promise((resolve) => {
      confirm.require({
        message: `¿Estás seguro de guardar ${pendingChanges.value} cambios en el inventario?`,
        header: 'Confirmar Cambios Masivos',
        icon: 'pi pi-exclamation-triangle',
        acceptLabel: 'Sí, guardar todo',
        rejectLabel: 'Cancelar',
        acceptClass: 'p-button-success',
        accept: () => resolve(true),
        reject: () => resolve(false),
      })
    })

    if (!confirmed) return

    try {
      isUpdating.value = true

      const updates = Array.from(changedItems.value).map((inventarioId) => {
        const item = inventory.value.find((i) => i.inventarioId === inventarioId)
        return {
          inventarioId: item.inventarioId,
          cantidad: item.newStock,
        }
      })

      await axiosClient.put('/UpdateStockGeneral', updates)

      // Update local state
      updates.forEach((update) => {
        const item = inventory.value.find((i) => i.inventarioId === update.inventarioId)
        if (item) {
          item.cantidad = update.cantidad
        }
      })

      changedItems.value.clear()
      showSuccess(`🎉 ${updates.length} cambios guardados exitosamente`)
    } catch (error) {
      console.error('Error saving changes:', error)
      showError('❌ Error al guardar los cambios')
    } finally {
      isUpdating.value = false
    }
  }

  const applyBulkAction = async (action) => {
    const selectedItems = filteredInventory.value

    if (selectedItems.length === 0) {
      showInfo('ℹ️ No hay productos para aplicar la acción')
      return
    }

    const actionText = {
      set: `establecer a ${bulkAmount.value}`,
      add: `agregar ${bulkAmount.value}`,
      reset: 'resetear a 0',
    }[action]

    const confirmed = await new Promise((resolve) => {
      confirm.require({
        message: `¿Estás seguro de ${actionText} unidades para ${selectedItems.length} productos?`,
        header: 'Confirmar Acción Masiva',
        icon: 'pi pi-exclamation-triangle',
        acceptLabel: 'Sí, aplicar',
        rejectLabel: 'Cancelar',
        acceptClass: 'p-button-warning',
        accept: () => resolve(true),
        reject: () => resolve(false),
      })
    })

    if (!confirmed) return

    selectedItems.forEach((item) => {
      switch (action) {
        case 'set':
          item.newStock = bulkAmount.value || 0
          break
        case 'add':
          item.newStock = (item.newStock || 0) + (bulkAmount.value || 0)
          break
        case 'reset':
          item.newStock = 0
          break
      }
      markAsChanged(item)
    })

    showSuccess(`⚡ Acción aplicada a ${selectedItems.length} productos`)
  }

  const refreshInventory = async () => {
    await fetchInventory()
  }

  const actualizarInventario = async () => {
    const institucionID = authStore.institucionID
    if (!institucionID) {
      showError('❌ No se pudo obtener la información de la institución')
      return
    }

    try {
      isLoading.value = true
      await axiosClient.get(`/CoordinarInventarioGeneral?InstitucionID=${institucionID}`)
      await fetchInventory() // Refresh inventory after coordinating
      showSuccess('🔄 Inventario sincronizado correctamente')
    } catch (error) {
      console.error('Error al coordinar inventario:', error)
      showError('❌ Error al sincronizar el inventario')
    } finally {
      isLoading.value = false
    }
  }

  // Lifecycle
  onMounted(fetchInventory)
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
