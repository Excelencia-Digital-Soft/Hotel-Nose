<template>
  <div class="min-h-screen bg-gradient-to-br from-neutral-900 via-neutral-800 to-neutral-900 p-4">
    <!-- Header -->
    <div class="glass-container mb-6">
      <div class="flex justify-between items-center p-6">
        <div>
          <h1 class="text-3xl font-bold bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 bg-clip-text text-transparent">
            Inventario por Habitación
          </h1>
          <p class="text-gray-300 mt-2">Gestión de inventarios de habitaciones del hotel</p>
        </div>
        <div class="flex gap-3">
          <button 
            @click="refreshData"
            :disabled="loading"
            class="glass-button px-4 py-2 text-white transition-all hover:scale-105"
          >
            <i class="pi pi-refresh mr-2" :class="{ 'pi-spin': loading }"></i>
            Actualizar
          </button>
          <button 
            @click="showCreateModal = true"
            class="bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 
                   hover:from-primary-500 hover:via-secondary-500 hover:to-accent-500 
                   backdrop-blur-sm border border-white/30 rounded-lg px-4 py-2 text-white 
                   transition-all hover:scale-105"
          >
            <i class="pi pi-plus mr-2"></i>
            Nuevo Inventario
          </button>
        </div>
      </div>
    </div>

    <!-- Filters and Room Selection -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-6">
      <!-- Room Selection -->
      <div class="glass-card p-4">
        <h3 class="text-white font-semibold mb-3">Seleccionar Habitación</h3>
        <Dropdown 
          v-model="selectedRoomModel"
          :options="availableRooms"
          optionLabel="numero"
          placeholder="Todas las habitaciones"
          class="w-full glass-input"
          :class="{ 'border-accent-400': selectedRoom }"
          @change="onRoomChange"
        >
          <template #option="slotProps">
            <div class="flex items-center gap-2">
              <span class="font-medium">{{ slotProps.option.numero }}</span>
              <span class="text-gray-300">{{ slotProps.option.nombre }}</span>
              <Badge 
                v-if="slotProps.option.tieneInventario" 
                value="Con inventario" 
                severity="success" 
                class="ml-auto"
              />
            </div>
          </template>
        </Dropdown>
      </div>

      <!-- Filters -->
      <div class="glass-card p-4">
        <h3 class="text-white font-semibold mb-3">Filtros</h3>
        <div class="space-y-3">
          <div class="flex items-center gap-2">
            <Checkbox v-model="filters.bajoStock" :binary="true" />
            <label class="text-gray-300">Stock Bajo</label>
            <Badge v-if="lowStockInventories.length > 0" :value="lowStockInventories.length" severity="warning" />
          </div>
          <div class="flex items-center gap-2">
            <Checkbox v-model="filters.agotados" :binary="true" />
            <label class="text-gray-300">Agotados</label>
            <Badge v-if="outOfStockInventories.length > 0" :value="outOfStockInventories.length" severity="danger" />
          </div>
          <div class="flex items-center gap-2">
            <Checkbox v-model="filters.alertas" :binary="true" />
            <label class="text-gray-300">Con Alertas</label>
            <Badge v-if="criticalAlerts.length > 0" :value="criticalAlerts.length" severity="danger" />
          </div>
        </div>
      </div>

      <!-- Search -->
      <div class="glass-card p-4">
        <h3 class="text-white font-semibold mb-3">Buscar</h3>
        <InputText 
          v-model="filters.busqueda"
          placeholder="Buscar artículo o habitación..."
          class="w-full glass-input"
        />
      </div>
    </div>

    <!-- Statistics Cards -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
      <div class="glass-card p-4 text-center">
        <div class="text-2xl font-bold text-primary-400">{{ inventories.length }}</div>
        <div class="text-gray-300 text-sm">Total Inventarios</div>
      </div>
      <div class="glass-card p-4 text-center">
        <div class="text-2xl font-bold text-accent-400">{{ lowStockInventories.length }}</div>
        <div class="text-gray-300 text-sm">Stock Bajo</div>
      </div>
      <div class="glass-card p-4 text-center">
        <div class="text-2xl font-bold text-red-400">{{ outOfStockInventories.length }}</div>
        <div class="text-gray-300 text-sm">Agotados</div>
      </div>
      <div class="glass-card p-4 text-center">
        <div class="text-2xl font-bold text-green-400">{{ formatCurrency(totalInventoryValue) }}</div>
        <div class="text-gray-300 text-sm">Valor Total</div>
      </div>
    </div>

    <!-- Alerts Section -->
    <div v-if="criticalAlerts.length > 0" class="glass-card p-4 mb-6 border-red-400/30">
      <h3 class="text-red-400 font-semibold mb-3 flex items-center">
        <i class="pi pi-exclamation-triangle mr-2"></i>
        Alertas Críticas
      </h3>
      <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
        <div 
          v-for="alert in criticalAlerts.slice(0, 4)" 
          :key="alert.roomInventoryId"
          class="bg-red-500/10 border border-red-400/30 rounded-lg p-3"
        >
          <div class="flex justify-between items-start">
            <div>
              <div class="text-white font-medium">{{ alert.articuloNombre }}</div>
              <div class="text-gray-300 text-sm">Habitación {{ alert.habitacionNumero }}</div>
              <div class="text-red-400 text-sm">{{ alert.mensaje }}</div>
            </div>
            <Badge :value="alert.prioridad" :severity="alert.prioridad === 'CRITICAL' ? 'danger' : 'warning'" />
          </div>
        </div>
      </div>
    </div>

    <!-- Inventory Table -->
    <div class="glass-card p-6">
      <DataTable 
        :value="filteredInventories"
        :loading="loading"
        paginator 
        :rows="20"
        dataKey="roomInventoryId"
        :globalFilterFields="['articulo.nombreArticulo', 'habitacion.numero', 'habitacion.nombre']"
        class="inventory-table"
      >
        <template #loading>
          <div class="flex items-center justify-center p-8">
            <ProgressSpinner style="width:50px;height:50px" strokeWidth="4" fill="transparent" animationDuration="1s"/>
            <span class="ml-3 text-gray-300">Cargando inventarios...</span>
          </div>
        </template>

        <template #empty>
          <div class="text-center p-8">
            <i class="pi pi-inbox text-gray-400 text-4xl mb-3"></i>
            <div class="text-gray-300">No hay inventarios disponibles</div>
          </div>
        </template>

        <Column field="habitacion.numero" header="Habitación" sortable class="min-w-[120px]">
          <template #body="slotProps">
            <div>
              <div class="font-semibold text-white">{{ slotProps.data.habitacion?.numero }}</div>
              <div class="text-gray-300 text-sm">{{ slotProps.data.habitacion?.nombre }}</div>
            </div>
          </template>
        </Column>

        <Column field="articulo.nombreArticulo" header="Artículo" sortable class="min-w-[200px]">
          <template #body="slotProps">
            <div>
              <div class="font-semibold text-white">{{ slotProps.data.articulo?.nombreArticulo }}</div>
              <div class="text-gray-300 text-sm">{{ slotProps.data.articulo?.categoria }}</div>
            </div>
          </template>
        </Column>

        <Column field="cantidad" header="Stock" sortable class="min-w-[120px]">
          <template #body="slotProps">
            <div class="flex items-center gap-2">
              <span 
                class="font-semibold"
                :class="{
                  'text-green-400': getInventoryStatus(slotProps.data) === 'OK',
                  'text-yellow-400': getInventoryStatus(slotProps.data) === 'LOW_STOCK',
                  'text-red-400': getInventoryStatus(slotProps.data) === 'OUT_OF_STOCK',
                  'text-orange-400': getInventoryStatus(slotProps.data) === 'OVERSTOCKED'
                }"
              >
                {{ slotProps.data.cantidad }}
              </span>
              <Badge 
                :value="getInventoryStatus(slotProps.data)"
                :severity="getStatusSeverity(getInventoryStatus(slotProps.data))"
                class="text-xs"
              />
            </div>
          </template>
        </Column>

        <Column field="cantidadMinima" header="Min/Max" sortable class="min-w-[100px]">
          <template #body="slotProps">
            <div class="text-gray-300 text-sm">
              {{ slotProps.data.cantidadMinima }} / {{ slotProps.data.cantidadMaxima }}
            </div>
          </template>
        </Column>

        <Column field="articulo.precio" header="Precio" sortable class="min-w-[100px]">
          <template #body="slotProps">
            <div class="text-gray-300">
              {{ formatCurrency(slotProps.data.articulo?.precio || 0) }}
            </div>
          </template>
        </Column>

        <Column field="fechaActualizacion" header="Última Act." sortable class="min-w-[120px]">
          <template #body="slotProps">
            <div class="text-gray-300 text-sm">
              {{ formatDate(slotProps.data.fechaActualizacion) }}
            </div>
          </template>
        </Column>

        <Column header="Acciones" class="min-w-[200px]">
          <template #body="slotProps">
            <div class="flex gap-2">
              <Button 
                icon="pi pi-pencil"
                size="small"
                text
                @click="editInventory(slotProps.data)"
                class="glass-button-sm text-primary-400 hover:bg-primary-400/10"
              />
              <Button 
                icon="pi pi-plus"
                size="small"
                text
                @click="increaseStock(slotProps.data)"
                class="glass-button-sm text-green-400 hover:bg-green-400/10"
              />
              <Button 
                icon="pi pi-minus"
                size="small"
                text
                @click="decreaseStock(slotProps.data)"
                class="glass-button-sm text-yellow-400 hover:bg-yellow-400/10"
              />
              <Button 
                icon="pi pi-exchange"
                size="small"
                text
                @click="transferStock(slotProps.data)"
                class="glass-button-sm text-accent-400 hover:bg-accent-400/10"
              />
            </div>
          </template>
        </Column>
      </DataTable>
    </div>

    <!-- Create/Edit Inventory Modal -->
    <Dialog 
      v-model:visible="showCreateModal"
      :header="editingInventory ? 'Editar Inventario' : 'Crear Inventario'"
      modal
      class="w-[500px]"
      :closable="true"
    >
      <template #header>
        <div class="text-white font-semibold">
          {{ editingInventory ? 'Editar Inventario' : 'Crear Inventario' }}
        </div>
      </template>
      
      <div class="space-y-4 p-2">
        <!-- Room Selection -->
        <div>
          <label class="text-gray-300 text-sm mb-2 block">Habitación *</label>
          <Dropdown 
            v-model="inventoryForm.habitacionId"
            :options="availableRooms"
            optionLabel="numero"
            optionValue="habitacionId"
            placeholder="Seleccionar habitación"
            class="w-full glass-input"
            :disabled="editingInventory"
          />
        </div>

        <!-- Article Selection -->
        <div>
          <label class="text-gray-300 text-sm mb-2 block">Artículo *</label>
          <Dropdown 
            v-model="inventoryForm.articuloId"
            :options="availableArticles"
            optionLabel="nombreArticulo"
            optionValue="articuloId"
            placeholder="Seleccionar artículo"
            class="w-full glass-input"
            :disabled="editingInventory"
          />
        </div>

        <!-- Quantity -->
        <div>
          <label class="text-gray-300 text-sm mb-2 block">Cantidad *</label>
          <InputNumber 
            v-model="inventoryForm.cantidad"
            :min="0"
            class="w-full glass-input"
          />
        </div>

        <!-- Min/Max Quantities -->
        <div class="grid grid-cols-2 gap-4">
          <div>
            <label class="text-gray-300 text-sm mb-2 block">Cantidad Mínima *</label>
            <InputNumber 
              v-model="inventoryForm.cantidadMinima"
              :min="0"
              class="w-full glass-input"
            />
          </div>
          <div>
            <label class="text-gray-300 text-sm mb-2 block">Cantidad Máxima *</label>
            <InputNumber 
              v-model="inventoryForm.cantidadMaxima"
              :min="inventoryForm.cantidadMinima || 1"
              class="w-full glass-input"
            />
          </div>
        </div>
      </div>

      <template #footer>
        <div class="flex justify-end gap-2">
          <Button 
            label="Cancelar"
            text
            @click="closeCreateModal"
            class="glass-button text-gray-300"
          />
          <Button 
            :label="editingInventory ? 'Actualizar' : 'Crear'"
            @click="saveInventory"
            :loading="saving"
            class="bg-gradient-to-r from-primary-400 to-secondary-400 hover:from-primary-500 hover:to-secondary-500 text-white"
          />
        </div>
      </template>
    </Dialog>

    <!-- Stock Adjustment Modal -->
    <Dialog 
      v-model:visible="showAdjustmentModal"
      header="Ajustar Inventario"
      modal
      class="w-[400px]"
    >
      <template #header>
        <div class="text-white font-semibold">Ajustar Inventario</div>
      </template>

      <div class="space-y-4 p-2">
        <div class="text-center p-4 bg-white/5 rounded-lg">
          <div class="text-white font-medium">{{ selectedInventoryForAdjustment?.articulo?.nombreArticulo }}</div>
          <div class="text-gray-300 text-sm">
            Habitación {{ selectedInventoryForAdjustment?.habitacion?.numero }}
          </div>
          <div class="text-lg font-semibold text-primary-400 mt-2">
            Stock actual: {{ selectedInventoryForAdjustment?.cantidad }}
          </div>
        </div>

        <div>
          <label class="text-gray-300 text-sm mb-2 block">Nueva Cantidad *</label>
          <InputNumber 
            v-model="adjustmentForm.nuevaCantidad"
            :min="0"
            class="w-full glass-input"
          />
        </div>

        <div>
          <label class="text-gray-300 text-sm mb-2 block">Motivo del Ajuste *</label>
          <Textarea 
            v-model="adjustmentForm.motivo"
            placeholder="Describe el motivo del ajuste..."
            rows="3"
            class="w-full glass-input"
          />
        </div>
      </div>

      <template #footer>
        <div class="flex justify-end gap-2">
          <Button 
            label="Cancelar"
            text
            @click="closeAdjustmentModal"
            class="glass-button text-gray-300"
          />
          <Button 
            label="Ajustar"
            @click="confirmAdjustment"
            :loading="saving"
            class="bg-gradient-to-r from-accent-400 to-secondary-400 hover:from-accent-500 hover:to-secondary-500 text-white"
          />
        </div>
      </template>
    </Dialog>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { useRoomInventory } from '../composables/useRoomInventory'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import Dropdown from 'primevue/dropdown'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Textarea from 'primevue/textarea'
import Checkbox from 'primevue/checkbox'
import Badge from 'primevue/badge'
import ProgressSpinner from 'primevue/progressspinner'

// Composable
const {
  inventories,
  selectedRoom,
  availableRooms,
  availableArticles,
  alerts,
  loading,
  saving,
  error,
  filteredInventories,
  lowStockInventories,
  outOfStockInventories,
  totalInventoryValue,
  criticalAlerts,
  filters,
  inventoryForm,
  adjustmentForm,
  setSelectedRoom,
  createInventory,
  updateInventory,
  adjustInventoryQuantity,
  getInventoryStatus,
  refreshData,
  clearError
} = useRoomInventory()

// UI State
const showCreateModal = ref(false)
const showAdjustmentModal = ref(false)
const editingInventory = ref(null)
const selectedInventoryForAdjustment = ref(null)
const selectedRoomModel = ref(null)

// Watchers
watch(() => selectedRoom.value, (newRoom) => {
  selectedRoomModel.value = newRoom
})

// Methods
const onRoomChange = (event) => {
  setSelectedRoom(event.value)
}

const editInventory = (inventory) => {
  editingInventory.value = inventory
  inventoryForm.value = {
    habitacionId: inventory.habitacionId,
    articuloId: inventory.articuloId,
    cantidad: inventory.cantidad,
    cantidadMinima: inventory.cantidadMinima,
    cantidadMaxima: inventory.cantidadMaxima
  }
  showCreateModal.value = true
}

const increaseStock = (inventory) => {
  selectedInventoryForAdjustment.value = inventory
  adjustmentForm.value = {
    roomInventoryId: inventory.roomInventoryId,
    nuevaCantidad: inventory.cantidad + 1,
    motivo: 'Aumento de stock',
    tipoMovimiento: 'ENTRADA'
  }
  showAdjustmentModal.value = true
}

const decreaseStock = (inventory) => {
  selectedInventoryForAdjustment.value = inventory
  adjustmentForm.value = {
    roomInventoryId: inventory.roomInventoryId,
    nuevaCantidad: Math.max(0, inventory.cantidad - 1),
    motivo: 'Disminución de stock',
    tipoMovimiento: 'SALIDA'
  }
  showAdjustmentModal.value = true
}

const transferStock = (inventory) => {
  // TODO: Implement transfer functionality
  console.log('Transfer stock for:', inventory)
}

const saveInventory = async () => {
  let success = false

  if (editingInventory.value) {
    success = await updateInventory(editingInventory.value.roomInventoryId, {
      cantidad: inventoryForm.value.cantidad,
      cantidadMinima: inventoryForm.value.cantidadMinima,
      cantidadMaxima: inventoryForm.value.cantidadMaxima
    })
  } else {
    success = await createInventory(inventoryForm.value)
  }

  if (success) {
    closeCreateModal()
  }
}

const confirmAdjustment = async () => {
  const success = await adjustInventoryQuantity(
    adjustmentForm.value.roomInventoryId,
    adjustmentForm.value.nuevaCantidad,
    adjustmentForm.value.motivo
  )

  if (success) {
    closeAdjustmentModal()
  }
}

const closeCreateModal = () => {
  showCreateModal.value = false
  editingInventory.value = null
  inventoryForm.value = {
    habitacionId: null,
    articuloId: null,
    cantidad: 0,
    cantidadMinima: 1,
    cantidadMaxima: 100
  }
}

const closeAdjustmentModal = () => {
  showAdjustmentModal.value = false
  selectedInventoryForAdjustment.value = null
  adjustmentForm.value = {
    roomInventoryId: 0,
    nuevaCantidad: 0,
    motivo: '',
    tipoMovimiento: 'AJUSTE'
  }
}

const getStatusSeverity = (status) => {
  switch (status) {
    case 'OK': return 'success'
    case 'LOW_STOCK': return 'warning'
    case 'OUT_OF_STOCK': return 'danger'
    case 'OVERSTOCKED': return 'info'
    default: return 'info'
  }
}

const formatCurrency = (value) => {
  return new Intl.NumberFormat('es-ES', {
    style: 'currency',
    currency: 'EUR'
  }).format(value)
}

const formatDate = (dateString) => {
  return new Date(dateString).toLocaleDateString('es-ES', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  })
}
</script>

<style scoped>
.inventory-table :deep(.p-datatable) {
  background: transparent;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.inventory-table :deep(.p-datatable-header) {
  background: rgba(255, 255, 255, 0.05);
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.inventory-table :deep(.p-datatable-tbody > tr) {
  background: rgba(255, 255, 255, 0.02);
  border-bottom: 1px solid rgba(255, 255, 255, 0.05);
}

.inventory-table :deep(.p-datatable-tbody > tr:hover) {
  background: rgba(255, 255, 255, 0.08);
}

.inventory-table :deep(.p-column-header-content) {
  color: white;
  font-weight: 600;
}

.glass-button-sm {
  @apply backdrop-blur-sm border border-white/20 rounded-lg p-1 transition-all hover:scale-105;
}
</style>