<template>
  <div
    class="min-h-screen bg-gradient-to-br from-neutral-900 via-neutral-800 to-neutral-900 p-4 lg:p-6"
  >
    <!-- Enhanced Header with Hero Section -->
    <div class="glass-container mb-6 p-8 relative overflow-hidden">
      <div
        class="absolute inset-0 bg-gradient-to-r from-primary-400/20 via-secondary-400/20 to-accent-400/20 blur-3xl"
      ></div>
      <div class="relative z-10">
        <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-6">
          <div class="text-center lg:text-left">
            <div class="flex items-center justify-center lg:justify-start gap-4 mb-4">
              <div
                class="bg-gradient-to-br from-primary-400 to-accent-400 p-4 rounded-2xl shadow-2xl transform hover:rotate-3 transition-transform duration-300"
              >
                <i class="pi pi-warehouse text-white text-3xl"></i>
              </div>
              <div>
                <h1
                  class="text-4xl lg:text-5xl font-bold text-transparent bg-clip-text bg-gradient-to-r from-white to-gray-300"
                >
                  Gestión de Inventario
                </h1>
                <p class="text-gray-400 text-lg mt-1">Sistema V1 - Control Avanzado</p>
              </div>
            </div>
            <div class="flex flex-wrap gap-2 justify-center lg:justify-start">
              <span class="glass-badge">
                <i class="pi pi-bolt text-yellow-400 mr-1"></i>
                API V1 Activa
              </span>
              <span class="glass-badge">
                <i class="pi pi-sync text-green-400 mr-1"></i>
                Sincronización en tiempo real
              </span>
              <span class="glass-badge">
                <i class="pi pi-shield text-blue-400 mr-1"></i>
                Gestión segura
              </span>
            </div>
          </div>

          <!-- Enhanced Stats Dashboard -->
          <div class="glass-card p-6 bg-white/5">
            <div class="grid grid-cols-2 lg:grid-cols-4 gap-4">
              <div class="stat-card">
                <div class="stat-icon bg-gradient-to-br from-primary-400 to-primary-500">
                  <i class="pi pi-box text-2xl"></i>
                </div>
                <div class="stat-value">{{ stats.totalItems }}</div>
                <div class="stat-label">Productos</div>
              </div>
              <div class="stat-card">
                <div class="stat-icon bg-gradient-to-br from-secondary-400 to-secondary-500">
                  <i class="pi pi-chart-line text-2xl"></i>
                </div>
                <div class="stat-value">{{ stats.totalStock }}</div>
                <div class="stat-label">Stock Total</div>
              </div>
              <div class="stat-card">
                <div class="stat-icon bg-gradient-to-br from-yellow-400 to-yellow-500">
                  <i class="pi pi-exclamation-triangle text-2xl"></i>
                </div>
                <div class="stat-value">{{ stats.lowStockCount }}</div>
                <div class="stat-label">Stock Bajo</div>
              </div>
              <div class="stat-card">
                <div class="stat-icon bg-gradient-to-br from-red-400 to-red-500">
                  <i class="pi pi-times-circle text-2xl"></i>
                </div>
                <div class="stat-value">{{ stats.outOfStockCount }}</div>
                <div class="stat-label">Sin Stock</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Enhanced Search and Filters -->
    <div class="glass-container mb-6 p-6">
      <div class="flex items-center justify-between mb-4">
        <h3 class="text-2xl font-bold text-white flex items-center">
          <i class="pi pi-filter text-accent-400 mr-3"></i>
          Búsqueda y Filtros Avanzados
        </h3>
        <button
          @click="showAdvancedFilters = !showAdvancedFilters"
          class="glass-button px-4 py-2 text-white"
        >
          <i :class="showAdvancedFilters ? 'pi pi-angle-up' : 'pi pi-angle-down'" class="mr-2"></i>
          {{ showAdvancedFilters ? 'Ocultar' : 'Mostrar' }} Filtros
        </button>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <!-- Enhanced Search Input -->
        <div class="relative group">
          <i class="pi pi-search absolute left-4 top-1/2 -translate-y-1/2 text-gray-400 group-focus-within:text-primary-400 transition-colors"></i>
          <input
            v-model="filters.search"
            type="text"
            class="glass-input-enhanced w-full pl-12 pr-12"
            placeholder="      Buscar productos..."
            @input="debouncedSearch"
          />
          <button
            v-if="filters.search"
            @click="filters.search = ''"
            class="absolute right-4 top-1/2 -translate-y-1/2 text-gray-400 hover:text-white transition-colors"
          >
            <i class="pi pi-times"></i>
          </button>
        </div>

        <!-- Stock Filter with Icons -->
        <select v-model="filters.stockStatus" class="glass-select">
          <option value="all">📦 Todo el inventario</option>
          <option value="in-stock">✅ Con stock disponible</option>
          <option value="low-stock">⚠️ Stock bajo (≤ 5)</option>
          <option value="out-of-stock">🚫 Sin stock</option>
          <option value="critical">🚨 Stock crítico</option>
        </select>

        <!-- Sort Options -->
        <select v-model="filters.sortBy" class="glass-select">
          <option value="name-asc">🔤 Nombre (A-Z)</option>
          <option value="name-desc">🔤 Nombre (Z-A)</option>
          <option value="stock-asc">📈 Stock (menor a mayor)</option>
          <option value="stock-desc">📉 Stock (mayor a menor)</option>
          <option value="updated">🕐 Última actualización</option>
        </select>
      </div>

      <!-- Advanced Filters Panel -->
      <Transition name="slide-down">
        <div v-if="showAdvancedFilters" class="mt-4 p-4 glass-card bg-white/5">
          <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <label class="block text-white font-medium mb-2">Categoría</label>
              <select v-model="filters.category" class="glass-select">
                <option value="">Todas las categorías</option>
                <option value="bebidas">🥤 Bebidas</option>
                <option value="snacks">🍿 Snacks</option>
                <option value="amenities">🧴 Amenidades</option>
                <option value="minibar">🍾 Minibar</option>
              </select>
            </div>
            <div>
              <label class="block text-white font-medium mb-2">Precio Rango</label>
              <div class="flex gap-2">
                <input
                  v-model.number="filters.priceMin"
                  type="number"
                  class="glass-input flex-1"
                  placeholder="Min"
                  min="0"
                />
                <input
                  v-model.number="filters.priceMax"
                  type="number"
                  class="glass-input flex-1"
                  placeholder="Max"
                  min="0"
                />
              </div>
            </div>
            <div>
              <label class="block text-white font-medium mb-2">Ubicación</label>
              <select v-model="filters.location" class="glass-select">
                <option value="">Todas las ubicaciones</option>
                <option value="general">🏢 Inventario General</option>
                <option value="room">🚪 Por Habitación</option>
              </select>
            </div>
          </div>
        </div>
      </Transition>

      <!-- Quick Action Buttons -->
      <div class="flex flex-wrap gap-2 mt-4">
        <button
          @click="toggleBulkMode"
          class="glass-button-primary"
          :class="{ 'bg-primary-500/30': bulkMode }"
        >
          <i class="pi pi-pencil mr-2"></i>
          {{ bulkMode ? 'Salir de' : 'Activar' }} Edición Masiva
        </button>
        <button @click="showImportModal = true" class="glass-button-secondary">
          <i class="pi pi-upload mr-2"></i>
          Importar
        </button>
        <button @click="exportInventory" class="glass-button-secondary">
          <i class="pi pi-download mr-2"></i>
          Exportar
        </button>
        <button
          @click="refreshInventory"
          :disabled="loading"
          class="glass-button-secondary"
        >
          <i :class="loading ? 'pi pi-spin pi-spinner' : 'pi pi-refresh'" class="mr-2"></i>
          Actualizar
        </button>
      </div>
    </div>

    <!-- Main Inventory Grid -->
    <div class="glass-container p-6">
      <!-- Grid Header -->
      <div class="flex items-center justify-between mb-6">
        <h3 class="text-2xl font-bold text-white flex items-center">
          <i class="pi pi-list text-primary-400 mr-3"></i>
          Inventario
          <span class="ml-2 text-gray-400 text-lg">({{ filteredInventory.length }} productos)</span>
        </h3>
        
        <div class="flex items-center gap-4">
          <!-- View Toggle -->
          <div class="flex bg-white/10 rounded-lg p-1">
            <button
              @click="viewMode = 'grid'"
              :class="viewMode === 'grid' ? 'bg-primary-500' : 'bg-transparent'"
              class="px-3 py-1 rounded transition-all text-white"
            >
              <i class="pi pi-th-large"></i>
            </button>
            <button
              @click="viewMode = 'list'"
              :class="viewMode === 'list' ? 'bg-primary-500' : 'bg-transparent'"
              class="px-3 py-1 rounded transition-all text-white"
            >
              <i class="pi pi-bars"></i>
            </button>
          </div>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="flex items-center justify-center py-20">
        <div class="text-center">
          <div class="relative">
            <div class="w-24 h-24 bg-gradient-to-r from-primary-400 to-accent-400 rounded-full animate-ping absolute"></div>
            <div class="w-24 h-24 bg-gradient-to-r from-primary-400 to-accent-400 rounded-full flex items-center justify-center relative">
              <i class="pi pi-spin pi-spinner text-white text-3xl"></i>
            </div>
          </div>
          <p class="text-white mt-4 text-lg">Cargando inventario...</p>
        </div>
      </div>

      <!-- Inventory Grid/List -->
      <div v-else-if="filteredInventory.length > 0">
        <!-- Grid View -->
        <div v-if="viewMode === 'grid'" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
          <TransitionGroup name="list">
            <div
              v-for="item in paginatedInventory"
              :key="item.inventoryId"
              class="inventory-card"
              :class="{
                'border-green-500/50': item.cantidad > 5,
                'border-yellow-500/50': item.cantidad > 0 && item.cantidad <= 5,
                'border-red-500/50': item.cantidad === 0,
                'ring-2 ring-primary-400': selectedItems.includes(item.inventoryId)
              }"
            >
              <!-- Bulk Select Checkbox -->
              <div v-if="bulkMode" class="absolute top-3 right-3 z-10">
                <input
                  type="checkbox"
                  :checked="selectedItems.includes(item.inventoryId)"
                  @change="toggleItemSelection(item.inventoryId)"
                  class="w-5 h-5 rounded border-white/30 bg-white/10 text-primary-500 focus:ring-primary-400"
                />
              </div>

              <!-- Product Image/Icon -->
              <div class="relative mb-4">
                <!-- <div class="aspect-square bg-gradient-to-br from-white/5 to-white/10 rounded-xl flex items-center justify-center">
                  <i class="pi pi-box text-5xl text-white/30"></i>
                  <img :src="'https://excelencia.myiphost.com:86/apihotalnose/uploads/' + item.articuloImagenUrl" />
                </div>-->
                <div class="aspect-square bg-gradient-to-br from-white/5 to-white/10 rounded-xl flex items-center justify-center">
                  <!-- Mostrar icono cuando no haya imagen -->
                  <i v-if="!item.articuloImagenUrl" class="pi pi-box text-5xl text-white/30"></i>
                  
                  <!-- Mostrar imagen cuando exista -->
                  <img 
                    v-else
                    :src="getArticleImage(item.articuloImagenUrl)" 
                    class="object-contain max-h-full max-w-full"
                    alt="Imagen artículo"
                  />
                </div>

                <div
                  class="absolute -bottom-2 -right-2 w-12 h-12 rounded-full flex items-center justify-center text-white font-bold"
                  :class="{
                    'bg-gradient-to-br from-green-400 to-green-500': item.cantidad > 5,
                    'bg-gradient-to-br from-yellow-400 to-yellow-500': item.cantidad > 0 && item.cantidad <= 5,
                    'bg-gradient-to-br from-red-400 to-red-500': item.cantidad === 0
                  }"
                >
                  {{ item.cantidad }}
                </div>
              </div>

              <!-- Product Info -->
              <div class="mb-4">
                <h4 class="text-white font-semibold text-lg mb-1 line-clamp-2">
                  {{ item.articuloNombre }}
                </h4>
                <p class="text-gray-400 text-sm">
                  ID: {{ item.articuloId }} | Precio: ${{ item.articuloPrecio?.toFixed(2) || '0.00' }}
                </p>
                <div class="flex items-center gap-2 mt-2">
                  <span :class="getStockBadgeClass(item.cantidad)" class="stock-badge">
                    {{ getStockLabel(item.cantidad) }}
                  </span>
                  <span v-if="item.lastUpdated" class="text-xs text-gray-500">
                    <i class="pi pi-clock text-xs mr-1"></i>
                    {{ formatDate(item.lastUpdated) }}
                  </span>
                </div>
              </div>

              <!-- Stock Controls -->
              <div class="space-y-3">
                <div class="flex items-center gap-2">
                  <button
                    @click="decrementStock(item)"
                    :disabled="item.newQuantity <= 0"
                    class="stock-control-btn"
                  >
                    <i class="pi pi-minus"></i>
                  </button>
                  <input
                    v-model.number="item.newQuantity"
                    type="number"
                    min="0"
                    class="glass-input flex-1 text-center text-lg font-semibold"
                    @change="markAsModified(item)"
                  />
                  <button
                    @click="incrementStock(item)"
                    class="stock-control-btn"
                  >
                    <i class="pi pi-plus"></i>
                  </button>
                </div>

                <!-- Quick Actions -->
                <div class="grid grid-cols-2 gap-2">
                  <button
                    @click="quickSetStock(item, 0)"
                    class="glass-button-small text-red-300 hover:text-red-400"
                  >
                    <i class="pi pi-ban mr-1"></i>
                    Vaciar
                  </button>
                  <button
                    @click="quickSetStock(item, 10)"
                    class="glass-button-small text-blue-300 hover:text-blue-400"
                  >
                    <i class="pi pi-plus-circle mr-1"></i>
                    +10
                  </button>
                </div>

                <!-- Save Button -->
                <button
                  v-if="item.isModified"
                  @click="saveItem(item)"
                  :disabled="saving"
                  class="w-full glass-button-primary"
                >
                  <i :class="saving ? 'pi pi-spin pi-spinner' : 'pi pi-check'" class="mr-2"></i>
                  Guardar Cambios
                </button>
              </div>
            </div>
          </TransitionGroup>
        </div>

        <!-- List View -->
        <div v-else class="space-y-2">
          <TransitionGroup name="list">
            <div
              v-for="item in paginatedInventory"
              :key="item.inventoryId"
              class="glass-card p-4 hover:bg-white/10 transition-all"
              :class="{
                'border-l-4 border-green-500': item.cantidad > 5,
                'border-l-4 border-yellow-500': item.cantidad > 0 && item.cantidad <= 5,
                'border-l-4 border-red-500': item.cantidad === 0,
                'ring-2 ring-primary-400': selectedItems.includes(item.inventoryId)
              }"
            >
              <div class="flex items-center justify-between">
                <div class="flex items-center gap-4">
                  <div v-if="bulkMode">
                    <input
                      type="checkbox"
                      :checked="selectedItems.includes(item.inventoryId)"
                      @change="toggleItemSelection(item.inventoryId)"
                      class="w-5 h-5 rounded border-white/30 bg-white/10 text-primary-500"
                    />
                  </div>
                  <div>
                    <h4 class="text-white font-semibold">{{ item.articuloNombre }}</h4>
                    <p class="text-gray-400 text-sm">
                      ID: {{ item.articuloId }} | Precio: ${{ item.articuloPrecio?.toFixed(2) || '0.00' }}
                    </p>
                  </div>
                </div>
                <div class="flex items-center gap-4">
                  <span :class="getStockBadgeClass(item.cantidad)" class="stock-badge">
                    {{ item.cantidad }} unidades
                  </span>
                  <div class="flex items-center gap-2">
                    <button @click="decrementStock(item)" class="stock-control-btn">
                      <i class="pi pi-minus"></i>
                    </button>
                    <input
                      v-model.number="item.newQuantity"
                      type="number"
                      min="0"
                      class="glass-input w-20 text-center"
                      @change="markAsModified(item)"
                    />
                    <button @click="incrementStock(item)" class="stock-control-btn">
                      <i class="pi pi-plus"></i>
                    </button>
                  </div>
                  <button
                    v-if="item.isModified"
                    @click="saveItem(item)"
                    :disabled="saving"
                    class="glass-button-primary"
                  >
                    <i :class="saving ? 'pi pi-spin pi-spinner' : 'pi pi-check'"></i>
                  </button>
                </div>
              </div>
            </div>
          </TransitionGroup>
        </div>

        <!-- Pagination -->
        <div v-if="totalPages > 1" class="mt-6 flex items-center justify-center gap-2">
          <button
            @click="currentPage = 1"
            :disabled="currentPage === 1"
            class="pagination-btn"
          >
            <i class="pi pi-angle-double-left"></i>
          </button>
          <button
            @click="currentPage--"
            :disabled="currentPage === 1"
            class="pagination-btn"
          >
            <i class="pi pi-angle-left"></i>
          </button>
          
          <div class="flex gap-1">
            <button
              v-for="page in visiblePages"
              :key="page"
              @click="currentPage = page"
              :class="currentPage === page ? 'bg-primary-500' : 'bg-white/10'"
              class="pagination-number"
            >
              {{ page }}
            </button>
          </div>

          <button
            @click="currentPage++"
            :disabled="currentPage === totalPages"
            class="pagination-btn"
          >
            <i class="pi pi-angle-right"></i>
          </button>
          <button
            @click="currentPage = totalPages"
            :disabled="currentPage === totalPages"
            class="pagination-btn"
          >
            <i class="pi pi-angle-double-right"></i>
          </button>
        </div>
      </div>

      <!-- Empty State -->
      <div v-else class="flex items-center justify-center py-20">
        <div class="text-center max-w-md">
          <div class="w-32 h-32 bg-gradient-to-br from-gray-700 to-gray-800 rounded-full flex items-center justify-center mx-auto mb-6">
            <i class="pi pi-inbox text-gray-500 text-5xl"></i>
          </div>
          <h3 class="text-2xl font-bold text-white mb-2">No se encontraron productos</h3>
          <p class="text-gray-400 mb-6">
            {{ filters.search ? 'No hay productos que coincidan con tu búsqueda' : 'El inventario está vacío' }}
          </p>
          <button
            @click="resetFilters"
            class="glass-button-primary"
          >
            <i class="pi pi-refresh mr-2"></i>
            Limpiar Filtros
          </button>
        </div>
      </div>
    </div>

    <!-- Floating Action Buttons -->
    <div class="fixed bottom-6 right-6 flex flex-col gap-3">
      <!-- Save All Button -->
      <Transition name="slide-up">
        <button
          v-if="modifiedItems.size > 0"
          @click="saveAllChanges"
          :disabled="saving"
          class="fab-primary"
        >
          <i :class="saving ? 'pi pi-spin pi-spinner' : 'pi pi-save'" class="mr-2"></i>
          Guardar Todo ({{ modifiedItems.size }})
        </button>
      </Transition>

      <!-- Bulk Actions -->
      <Transition name="slide-up">
        <button
          v-if="bulkMode && selectedItems.length > 0"
          @click="showBulkActionsModal = true"
          class="fab-secondary"
        >
          <i class="pi pi-cog mr-2"></i>
          Acciones ({{ selectedItems.length }})
        </button>
      </Transition>

      <!-- Add New Item -->
      <button
        @click="showAddItemModal = true"
        class="fab-accent"
      >
        <i class="pi pi-plus text-2xl"></i>
      </button>
    </div>

    <!-- Alerts Panel -->
    <Transition name="slide-left">
      <div v-if="activeAlerts.length > 0" class="fixed top-24 right-4 w-80 space-y-2 z-40">
        <div
          v-for="alert in activeAlerts"
          :key="alert.alertaId"
          class="glass-card p-4 border-l-4"
          :class="{
            'border-red-500': alert.severidad === 'Critica',
            'border-yellow-500': alert.severidad === 'Alta',
            'border-blue-500': alert.severidad === 'Media'
          }"
        >
          <div class="flex items-start justify-between">
            <div class="flex-1">
              <h4 class="text-white font-semibold">{{ alert.titulo }}</h4>
              <p class="text-gray-300 text-sm mt-1">{{ alert.mensaje }}</p>
              <p class="text-gray-500 text-xs mt-2">
                <i class="pi pi-clock mr-1"></i>
                {{ formatDate(alert.fechaCreacion) }}
              </p>
            </div>
            <button
              @click="acknowledgeAlert(alert.alertaId)"
              class="text-gray-400 hover:text-white ml-2"
            >
              <i class="pi pi-times"></i>
            </button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- Import Modal -->
    <Transition name="fade">
      <div v-if="showImportModal" class="modal-overlay" @click.self="showImportModal = false">
        <div class="modal-content">
          <div class="modal-header">
            <h3 class="text-2xl font-bold text-white">Importar Inventario</h3>
            <button @click="showImportModal = false" class="text-gray-400 hover:text-white">
              <i class="pi pi-times text-xl"></i>
            </button>
          </div>
          <div class="modal-body">
            <div class="upload-area">
              <i class="pi pi-cloud-upload text-5xl text-primary-400 mb-4"></i>
              <p class="text-white text-lg mb-2">Arrastra tu archivo aquí</p>
              <p class="text-gray-400 text-sm mb-4">o</p>
              <input type="file" id="file-upload" class="hidden" accept=".csv,.xlsx" />
              <label for="file-upload" class="glass-button-primary cursor-pointer">
                <i class="pi pi-file mr-2"></i>
                Seleccionar Archivo
              </label>
              <p class="text-gray-500 text-xs mt-4">Formatos soportados: CSV, XLSX</p>
            </div>
          </div>
        </div>
      </div>
    </Transition>

    <!-- Bulk Actions Modal -->
    <Transition name="fade">
      <div v-if="showBulkActionsModal" class="modal-overlay" @click.self="showBulkActionsModal = false">
        <div class="modal-content">
          <div class="modal-header">
            <h3 class="text-2xl font-bold text-white">Acciones Masivas</h3>
            <button @click="showBulkActionsModal = false" class="text-gray-400 hover:text-white">
              <i class="pi pi-times text-xl"></i>
            </button>
          </div>
          <div class="modal-body">
            <p class="text-gray-300 mb-4">
              {{ selectedItems.length }} productos seleccionados
            </p>
            <div class="space-y-3">
              <button @click="bulkSetStock" class="w-full glass-button-primary justify-start">
                <i class="pi pi-pencil mr-3"></i>
                Establecer cantidad
              </button>
              <button @click="bulkAddStock" class="w-full glass-button-secondary justify-start">
                <i class="pi pi-plus-circle mr-3"></i>
                Agregar al stock
              </button>
              <button @click="bulkRemoveStock" class="w-full glass-button-secondary justify-start">
                <i class="pi pi-minus-circle mr-3"></i>
                Reducir stock
              </button>
              <button @click="bulkResetStock" class="w-full glass-button-danger justify-start">
                <i class="pi pi-refresh mr-3"></i>
                Resetear a cero
              </button>
            </div>
          </div>
        </div>
      </div>
    </Transition>

    <!-- Toast Notifications -->
    <Toast />
    <ConfirmDialog />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch, type Ref, type ComputedRef } from 'vue'
import { useToast, type ToastServiceMethods } from 'primevue/usetoast'
import { useConfirm, type ConfirmationService } from 'primevue/useconfirm'
import Toast from 'primevue/toast'
import ConfirmDialog from 'primevue/confirmdialog'
import { useInventoryV1 } from '../composables/useInventoryV1'
import { debounce } from '../utils/helpers'
import type { InventoryDto } from '../types'
import { removeTrailingSlash } from '@/utils/url-helpers'

// Types for component state
interface Filters {
  search: string
  stockStatus: 'all' | 'in-stock' | 'low-stock' | 'out-of-stock' | 'critical'
  category: string
  location: string
  priceMin: number | null
  priceMax: number | null
  sortBy: 'name-asc' | 'name-desc' | 'stock-asc' | 'stock-desc' | 'updated'
}

interface EnrichedInventoryDto extends InventoryDto {
  newQuantity: number
  isModified: boolean
  stockStatus?: string
  lastUpdated?: string
}

// Composables
const toast: ToastServiceMethods = useToast()
const confirm: ConfirmationService = useConfirm()
const {
  inventory,
  loading,
  saving,
  error,
  stats,
  alerts: activeAlerts,
  fetchInventory,
  updateInventory,
  batchUpdateInventory,
  acknowledgeAlert,
  exportToCSV
} = useInventoryV1()

// State
const filters: Ref<Filters> = ref({
  search: '',
  stockStatus: 'all',
  category: '',
  location: '',
  priceMin: null,
  priceMax: null,
  sortBy: 'name-asc'
})

const showAdvancedFilters: Ref<boolean> = ref(false)
const bulkMode: Ref<boolean> = ref(false)
const selectedItems: Ref<number[]> = ref([])
const modifiedItems: Ref<Set<number>> = ref(new Set())
const viewMode: Ref<'grid' | 'list'> = ref('grid')
const currentPage: Ref<number> = ref(1)
const itemsPerPage: Ref<number> = ref(12)
const showImportModal: Ref<boolean> = ref(false)
const showBulkActionsModal: Ref<boolean> = ref(false)
const showAddItemModal: Ref<boolean> = ref(false)

// Computed
const filteredInventory: ComputedRef<EnrichedInventoryDto[]> = computed(() => {
  let result = [...inventory.value] as EnrichedInventoryDto[]

  // Search filter
  if (filters.value.search) {
    const search = filters.value.search.toLowerCase()
    result = result.filter(item => 
      item.articuloNombre.toLowerCase().includes(search) ||
      item.articuloId.toString().includes(search)
    )
  }

  // Stock status filter
  switch (filters.value.stockStatus) {
    case 'in-stock':
      result = result.filter(item => item.cantidad > 5)
      break
    case 'low-stock':
      result = result.filter(item => item.cantidad > 0 && item.cantidad <= 5)
      break
    case 'out-of-stock':
      result = result.filter(item => item.cantidad === 0)
      break
    case 'critical':
      result = result.filter(item => item.cantidad <= (item.cantidadMinima || 5))
      break
  }

  // Category filter
  if (filters.value.category) {
    result = result.filter(item => item.categoria === filters.value.category)
  }

  // Price filter
  if (filters.value.priceMin !== null) {
    result = result.filter(item => (item.articuloPrecio || 0) >= filters.value.priceMin!)
  }
  if (filters.value.priceMax !== null) {
    result = result.filter(item => (item.articuloPrecio || 0) <= filters.value.priceMax!)
  }

  // Sort
  const [sortField, sortOrder] = filters.value.sortBy.split('-')
  result.sort((a, b) => {
    let comparison = 0
    switch (sortField) {
      case 'name':
        comparison = a.articuloNombre.localeCompare(b.articuloNombre)
        break
      case 'stock':
        comparison = a.cantidad - b.cantidad
        break
      case 'updated':
        comparison = new Date(b.lastUpdated || 0).getTime() - new Date(a.lastUpdated || 0).getTime()
        break
    }
    return sortOrder === 'desc' ? -comparison : comparison
  })

  return result
})

const totalPages: ComputedRef<number> = computed(() => 
  Math.ceil(filteredInventory.value.length / itemsPerPage.value)
)

const paginatedInventory: ComputedRef<EnrichedInventoryDto[]> = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage.value
  const end = start + itemsPerPage.value
  console.log("AQUI EL TEST")
  console.log(filteredInventory)
  return filteredInventory.value.slice(start, end)
})

const visiblePages: ComputedRef<(number | string)[]> = computed(() => {
  const pages: (number | string)[] = []
  const total = totalPages.value
  const current = currentPage.value
  
  if (total <= 7) {
    for (let i = 1; i <= total; i++) pages.push(i)
  } else {
    if (current <= 3) {
      for (let i = 1; i <= 5; i++) pages.push(i)
      pages.push('...', total)
    } else if (current >= total - 2) {
      pages.push(1, '...')
      for (let i = total - 4; i <= total; i++) pages.push(i)
    } else {
      pages.push(1, '...')
      for (let i = current - 1; i <= current + 1; i++) pages.push(i)
      pages.push('...', total)
    }
  }
  
  return pages
})

// Methods

const getArticleImage = (url?: string): string => {
    try {
      if (url) {
        const baseUrl = removeTrailingSlash(import.meta.env.VITE_API_BASE_URL || '')

        return `${baseUrl}/apihotalnose/uploads/${url}`
      }
    } catch (error) {
      console.error('Error getting article image:', error)
    }

    return defaultProductImage
  }

const debouncedSearch = debounce(() => {
  currentPage.value = 1
}, 300)

const getStockBadgeClass = (cantidad: number): string => {
  if (cantidad === 0) return 'bg-red-500/20 text-red-300 border-red-500/30'
  if (cantidad <= 5) return 'bg-yellow-500/20 text-yellow-300 border-yellow-500/30'
  return 'bg-green-500/20 text-green-300 border-green-500/30'
}

const getStockLabel = (cantidad: number): string => {
  if (cantidad === 0) return 'Sin stock'
  if (cantidad <= 5) return 'Stock bajo'
  return 'Disponible'
}

const formatDate = (date: string | Date | undefined): string => {
  if (!date) return ''
  return new Date(date).toLocaleDateString('es-ES', {
    day: '2-digit',
    month: 'short',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const toggleBulkMode = (): void => {
  bulkMode.value = !bulkMode.value
  if (!bulkMode.value) {
    selectedItems.value = []
  }
}

const toggleItemSelection = (itemId: number): void => {
  const index = selectedItems.value.indexOf(itemId)
  if (index > -1) {
    selectedItems.value.splice(index, 1)
  } else {
    selectedItems.value.push(itemId)
  }
}

const incrementStock = (item: EnrichedInventoryDto): void => {
  item.newQuantity = (item.newQuantity || item.cantidad) + 1
  markAsModified(item)
}

const decrementStock = (item: EnrichedInventoryDto): void => {
  if (item.newQuantity > 0) {
    item.newQuantity = (item.newQuantity || item.cantidad) - 1
    markAsModified(item)
  }
}

const quickSetStock = (item: EnrichedInventoryDto, quantity: number): void => {
  item.newQuantity = quantity
  markAsModified(item)
}

const markAsModified = (item: EnrichedInventoryDto): void => {
  if (item.newQuantity !== item.cantidad) {
    item.isModified = true
    modifiedItems.value.add(item.inventoryId)
  } else {
    item.isModified = false
    modifiedItems.value.delete(item.inventoryId)
  }
}

const saveItem = async (item: EnrichedInventoryDto): Promise<void> => {
  try {
    await updateInventory(item.inventoryId, {
      cantidad: item.newQuantity,
      notes: `Actualización manual a ${item.newQuantity} unidades`
    })
    
    item.cantidad = item.newQuantity
    item.isModified = false
    modifiedItems.value.delete(item.inventoryId)
    
    toast.add({
      severity: 'success',
      summary: 'Éxito',
      detail: `Stock de "${item.articuloNombre}" actualizado`,
      life: 3000
    })
  } catch (error) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: 'No se pudo actualizar el stock',
      life: 5000
    })
  }
}

const saveAllChanges = async (): Promise<void> => {
  const itemsToUpdate = inventory.value
    .filter((item: EnrichedInventoryDto) => modifiedItems.value.has(item.inventoryId))
    .map((item: EnrichedInventoryDto) => ({
      inventoryId: item.inventoryId,
      cantidad: item.newQuantity
    }))

  if (itemsToUpdate.length === 0) return

  const confirmed = await new Promise<boolean>((resolve) => {
    confirm.require({
      message: `¿Guardar ${itemsToUpdate.length} cambios en el inventario?`,
      header: 'Confirmar Cambios',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Guardar Todo',
      rejectLabel: 'Cancelar',
      acceptClass: 'p-button-success',
      accept: () => resolve(true),
      reject: () => resolve(false)
    })
  })

  if (!confirmed) return

  try {
    await batchUpdateInventory(itemsToUpdate)
    
    // Update local state
    itemsToUpdate.forEach(update => {
      const item = inventory.value.find((i: EnrichedInventoryDto) => i.inventoryId === update.inventoryId)
      if (item) {
        item.cantidad = update.cantidad
        item.isModified = false
      }
    })
    
    modifiedItems.value.clear()
    
    toast.add({
      severity: 'success',
      summary: 'Éxito',
      detail: `${itemsToUpdate.length} cambios guardados correctamente`,
      life: 3000
    })
  } catch (error) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: 'No se pudieron guardar los cambios',
      life: 5000
    })
  }
}

const resetFilters = (): void => {
  filters.value = {
    search: '',
    stockStatus: 'all',
    category: '',
    location: '',
    priceMin: null,
    priceMax: null,
    sortBy: 'name-asc'
  }
  currentPage.value = 1
}

const refreshInventory = async (): Promise<void> => {
  await fetchInventory()
  toast.add({
    severity: 'info',
    summary: 'Actualizado',
    detail: 'Inventario actualizado correctamente',
    life: 3000
  })
}

const exportInventory = (): void => {
  exportToCSV()
  toast.add({
    severity: 'success',
    summary: 'Exportación Exitosa',
    detail: 'El inventario ha sido exportado',
    life: 3000
  })
}

// Lifecycle
onMounted(() => {
  fetchInventory()
})

// Watch for page changes
watch(currentPage, () => {
  window.scrollTo({ top: 0, behavior: 'smooth' })
})
</script>

<style scoped>
/* Glass morphism base styles */
.glass-container {
  @apply bg-white/5 backdrop-blur-2xl border border-white/20 rounded-3xl;
}

.glass-card {
  @apply bg-white/10 backdrop-blur-md border border-white/20 rounded-xl;
}

.glass-button {
  @apply bg-white/10 backdrop-blur-sm border border-white/30 rounded-lg transition-all;
}

.glass-button:hover {
  background-color: rgba(255, 255, 255, 0.2);
}

.glass-input {
  @apply bg-white/10 backdrop-blur-sm border border-white/30 rounded-lg text-white placeholder-gray-400 focus:ring-2 focus:ring-primary-400 focus:border-primary-400 outline-none;
}

.glass-input-enhanced {
  @apply glass-input px-4 py-3 transition-all duration-300;
}

.glass-input-enhanced:hover {
  background-color: rgba(255, 255, 255, 0.15);
}

.glass-select {
  @apply glass-input px-4 py-3 cursor-pointer;
}

/* Enhanced button styles */
.glass-button-primary {
  @apply bg-gradient-to-r from-primary-400 to-primary-500 hover:from-primary-500 hover:to-primary-600 text-white font-medium px-4 py-2 rounded-lg transition-all duration-300 transform hover:scale-105 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center;
}

.glass-button-secondary {
  @apply bg-white/10 text-white font-medium px-4 py-2 rounded-lg transition-all duration-300 border border-white/30 flex items-center justify-center;
}

.glass-button-secondary:hover {
  background-color: rgba(255, 255, 255, 0.2);
}

.glass-button-danger {
  @apply bg-red-500/20 text-red-300 font-medium px-4 py-2 rounded-lg transition-all duration-300 border border-red-500/30 flex items-center justify-center;
}

.glass-button-danger:hover {
  background-color: rgba(239, 68, 68, 0.3);
}

.glass-button-small {
  @apply glass-button px-2 py-1 text-sm;
}

/* Badge styles */
.glass-badge {
  @apply bg-white/10 backdrop-blur-sm border border-white/30 rounded-full px-3 py-1 text-sm text-white flex items-center;
}

.stock-badge {
  @apply px-2 py-1 rounded-full text-xs font-medium border;
}

/* Stat card styles */
.stat-card {
  @apply text-center transform hover:scale-105 transition-transform duration-200;
}

.stat-icon {
  @apply w-12 h-12 rounded-full flex items-center justify-center mx-auto mb-2 text-white;
}

.stat-value {
  @apply text-2xl font-bold text-white;
}

.stat-label {
  @apply text-xs text-gray-400;
}

/* Inventory card styles */
.inventory-card {
  @apply glass-card p-4 transition-all duration-300 transform hover:scale-105 border-2 relative;
}

.inventory-card:hover {
  background-color: rgba(255, 255, 255, 0.15);
}

/* Stock control buttons */
.stock-control-btn {
  @apply w-10 h-10 glass-button flex items-center justify-center text-white disabled:opacity-50 disabled:cursor-not-allowed;
}

.stock-control-btn:hover:not(:disabled) {
  background-color: rgba(255, 255, 255, 0.3);
}

/* Pagination styles */
.pagination-btn {
  @apply w-10 h-10 glass-button flex items-center justify-center text-white disabled:opacity-50 disabled:cursor-not-allowed;
}

.pagination-number {
  @apply w-10 h-10 rounded-lg flex items-center justify-center text-white transition-all duration-300;
}

.pagination-number:hover {
  background-color: rgba(255, 255, 255, 0.2);
}

/* Floating Action Buttons */
.fab-primary {
  @apply bg-gradient-to-r from-green-400 to-green-500 hover:from-green-500 hover:to-green-600 text-white font-bold py-3 px-6 rounded-full shadow-2xl transform hover:scale-105 transition-all duration-300 flex items-center;
}

.fab-secondary {
  @apply bg-gradient-to-r from-blue-400 to-blue-500 hover:from-blue-500 hover:to-blue-600 text-white font-bold py-3 px-6 rounded-full shadow-2xl transform hover:scale-105 transition-all duration-300 flex items-center;
}

.fab-accent {
  @apply bg-gradient-to-r from-primary-400 to-accent-400 hover:from-primary-500 hover:to-accent-500 text-white w-14 h-14 rounded-full shadow-2xl transform hover:scale-110 hover:rotate-90 transition-all duration-300 flex items-center justify-center;
}

/* Modal styles */
.modal-overlay {
  @apply fixed inset-0 bg-black/60 backdrop-blur-xl z-50 flex items-center justify-center p-4;
}

.modal-content {
  @apply glass-container max-w-2xl w-full max-h-[90vh] overflow-hidden;
}

.modal-header {
  @apply flex items-center justify-between p-6 border-b border-white/20;
}

.modal-body {
  @apply p-6 overflow-y-auto max-h-[calc(90vh-200px)];
}

.upload-area {
  @apply border-2 border-dashed border-white/30 rounded-xl p-12 text-center hover:border-primary-400 transition-colors;
}

/* Utility classes */
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

/* Animations */
@keyframes slide-down {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes slide-up {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes slide-left {
  from {
    opacity: 0;
    transform: translateX(10px);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

/* Transitions */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

.slide-down-enter-active,
.slide-down-leave-active {
  transition: all 0.3s ease;
}

.slide-down-enter-from,
.slide-down-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}

.slide-up-enter-active,
.slide-up-leave-active {
  transition: all 0.3s ease;
}

.slide-up-enter-from,
.slide-up-leave-to {
  opacity: 0;
  transform: translateY(10px);
}

.slide-left-enter-active,
.slide-left-leave-active {
  transition: all 0.3s ease;
}

.slide-left-enter-from,
.slide-left-leave-to {
  opacity: 0;
  transform: translateX(10px);
}

.list-enter-active,
.list-leave-active {
  transition: all 0.3s ease;
}

.list-enter-from,
.list-leave-to {
  opacity: 0;
  transform: translateX(-30px);
}

/* Scrollbar styling */
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