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
              <i class="pi pi-hotel text-white text-2xl"></i>
            </div>
            <h1 class="text-3xl lg:text-4xl font-bold text-white">🏨 Inventario por Habitación</h1>
            <span
              class="ml-3 px-3 py-1 bg-green-500/20 text-green-300 rounded-full text-sm font-semibold"
              >V1</span
            >
          </div>
          <p class="text-gray-300 text-lg">
            Gestión avanzada de inventario específico por habitación 🎯
          </p>
        </div>

        <!-- Room Selector -->
        <div class="glass-card p-4">
          <label class="block text-white font-semibold mb-2">Seleccionar Habitación</label>
          <select
            v-model="selectedRoomId"
            @change="changeRoom"
            class="glass-input px-4 py-2 min-w-48"
          >
            <option value="">Seleccione una habitación</option>
            <option v-for="room in availableRooms" :key="room.id" :value="room.id">
              {{ room.numero }} - {{ room.nombre }}
            </option>
          </select>
        </div>
      </div>
    </div>

    <!-- Room Summary -->
    <div v-if="selectedRoomId" class="glass-container mb-6 p-6">
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div class="glass-card p-4 text-center">
          <div
            class="bg-gradient-to-r from-blue-400 to-blue-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2"
          >
            <span class="font-bold">{{ roomInventory.stockSummary.totalItems }}</span>
          </div>
          <p class="text-white font-semibold">Total Items</p>
          <p class="text-gray-300 text-sm">en inventario</p>
        </div>

        <div class="glass-card p-4 text-center">
          <div
            class="bg-gradient-to-r from-green-400 to-green-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2"
          >
            <span class="font-bold">{{ roomInventory.stockSummary.totalQuantity }}</span>
          </div>
          <p class="text-white font-semibold">Stock Total</p>
          <p class="text-gray-300 text-sm">unidades</p>
        </div>

        <div class="glass-card p-4 text-center">
          <div
            class="bg-gradient-to-r from-yellow-400 to-yellow-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2"
          >
            <span class="font-bold">{{ roomInventory.stockSummary.lowStockItems }}</span>
          </div>
          <p class="text-white font-semibold">Stock Bajo</p>
          <p class="text-gray-300 text-sm">requieren atención</p>
        </div>

        <div class="glass-card p-4 text-center">
          <div
            class="bg-gradient-to-r from-red-400 to-red-500 text-white p-3 rounded-full mx-auto w-12 h-12 flex items-center justify-center mb-2"
          >
            <span class="font-bold">{{ roomInventory.needsAttention }}</span>
          </div>
          <p class="text-white font-semibold">Alertas</p>
          <p class="text-gray-300 text-sm">sin reconocer</p>
        </div>
      </div>
    </div>

    <!-- Inventory Items -->
    <div v-if="selectedRoomId" class="glass-container p-6">
      <div class="flex items-center justify-between mb-6">
        <h3 class="text-xl font-bold text-white flex items-center">
          <i class="pi pi-list mr-2"></i>
          Inventario de la Habitación {{ selectedRoomId }}
        </h3>

        <div class="flex space-x-2">
          <button
            @click="refreshInventory"
            :disabled="roomInventory.loading"
            class="glass-button px-4 py-2 text-white hover:bg-white/20"
          >
            <i
              :class="roomInventory.loading ? 'pi pi-spinner pi-spin' : 'pi pi-refresh'"
              class="mr-2"
            ></i>
            Actualizar
          </button>

          <button
            @click="showAddItemModal = true"
            class="bg-gradient-to-r from-primary-400 to-accent-400 hover:from-primary-500 hover:to-accent-500 text-white font-bold py-2 px-4 rounded-lg"
          >
            <i class="pi pi-plus mr-2"></i>
            Agregar Item
          </button>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="roomInventory.loading" class="text-center py-16">
        <div class="glass-card max-w-md mx-auto p-8">
          <div
            class="bg-gradient-to-r from-primary-400 to-accent-400 p-6 rounded-full mx-auto w-24 h-24 flex items-center justify-center mb-4"
          >
            <i class="pi pi-spinner pi-spin text-white text-4xl"></i>
          </div>
          <h3 class="text-xl text-white font-bold mb-2">🔄 Cargando inventario...</h3>
          <p class="text-gray-300">Obteniendo información de la habitación</p>
        </div>
      </div>

      <!-- Inventory Grid -->
      <div
        v-else-if="roomInventory.roomInventory.length > 0"
        class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6"
      >
        <div
          v-for="item in roomInventory.roomInventory"
          :key="item.inventoryId"
          class="glass-card p-4 hover:bg-white/15 transition-all duration-300"
          :class="{
            'border-red-500/50': item.cantidad === 0,
            'border-yellow-500/50': item.cantidad > 0 && item.cantidad <= 5,
            'border-green-500/50': item.cantidad > 5,
          }"
        >
          <div class="flex items-start justify-between mb-4">
            <div class="flex-1">
              <h4 class="text-white font-semibold text-lg mb-1">
                {{ item.articuloNombre }}
              </h4>
              <p class="text-gray-300 text-sm">{{ item.articuloDescripcion }}</p>
            </div>
            <div class="text-right">
              <p class="text-2xl font-bold text-white">{{ item.cantidad }}</p>
              <p class="text-xs text-gray-400">unidades</p>
            </div>
          </div>

          <!-- Actions -->
          <div class="flex space-x-2">
            <button
              @click="showUpdateModal(item)"
              class="flex-1 glass-button py-2 text-white hover:bg-white/20 text-sm"
            >
              <i class="pi pi-pencil mr-1"></i>
              Editar
            </button>
            <button
              @click="reloadItem(item)"
              class="flex-1 glass-button py-2 text-white hover:bg-green-500/20 text-sm"
            >
              <i class="pi pi-refresh mr-1"></i>
              Recargar
            </button>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div v-else class="text-center py-16">
        <div class="glass-card max-w-md mx-auto p-8">
          <div
            class="bg-gradient-to-r from-primary-400 to-accent-400 p-6 rounded-full mx-auto w-24 h-24 flex items-center justify-center mb-4"
          >
            <i class="pi pi-inbox text-white text-4xl"></i>
          </div>
          <h3 class="text-2xl text-white font-bold mb-2">📦 Sin inventario</h3>
          <p class="text-gray-300 mb-6">Esta habitación no tiene artículos en inventario</p>
          <button
            @click="showAddItemModal = true"
            class="bg-gradient-to-r from-primary-400 to-accent-400 hover:from-primary-500 hover:to-accent-500 text-white font-bold py-3 px-6 rounded-lg"
          >
            <i class="pi pi-plus mr-2"></i>
            Agregar Primer Item
          </button>
        </div>
      </div>
    </div>

    <!-- No Room Selected -->
    <div v-else class="text-center py-16">
      <div class="glass-card max-w-md mx-auto p-8">
        <div
          class="bg-gradient-to-r from-primary-400 to-accent-400 p-6 rounded-full mx-auto w-24 h-24 flex items-center justify-center mb-4"
        >
          <i class="pi pi-hotel text-white text-4xl"></i>
        </div>
        <h3 class="text-2xl text-white font-bold mb-2">🏨 Selecciona una habitación</h3>
        <p class="text-gray-300">Elige una habitación para ver y gestionar su inventario</p>
      </div>
    </div>

    <!-- Toast for notifications -->
    <Toast />
  </div>
</template>

<script setup>
  import { ref, onMounted, watch } from 'vue'
  import { useToast } from 'primevue/usetoast'
  import { useRoomInventory } from '../composables/useRoomInventory'
  import Toast from 'primevue/toast'

  // Composables
  const toast = useToast()

  // State
  const selectedRoomId = ref('')
  const availableRooms = ref([])
  const showAddItemModal = ref(false)

  // Room inventory composable
  const roomInventory = useRoomInventory()

  // Methods
  const changeRoom = () => {
    if (selectedRoomId.value) {
      roomInventory.changeRoom(parseInt(selectedRoomId.value))
      toast.add({
        severity: 'info',
        summary: 'Habitación Cambiada',
        detail: `Mostrando inventario de habitación ${selectedRoomId.value}`,
        life: 3000,
      })
    }
  }

  const refreshInventory = async () => {
    try {
      await roomInventory.refreshAll()
      toast.add({
        severity: 'success',
        summary: 'Actualizado',
        detail: 'Inventario actualizado correctamente',
        life: 3000,
      })
    } catch (error) {
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Error al actualizar el inventario',
        life: 5000,
      })
    }
  }

  const showUpdateModal = (item) => {
    toast.add({
      severity: 'info',
      summary: 'Funcionalidad',
      detail: `Editar ${item.articuloNombre} - Próximamente`,
      life: 3000,
    })
  }

  const reloadItem = async (item) => {
    try {
      await roomInventory.reloadRoomStock(item.articuloId, 10) // Default reload quantity
      toast.add({
        severity: 'success',
        summary: 'Recargado',
        detail: `Stock de ${item.articuloNombre} recargado correctamente`,
        life: 3000,
      })
    } catch (error) {
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: `Error al recargar ${item.articuloNombre}`,
        life: 5000,
      })
    }
  }

  // Lifecycle
  onMounted(() => {
    // Auto-select first room for demo
    if (availableRooms.value.length > 0) {
      selectedRoomId.value = availableRooms.value[0].id.toString()
      changeRoom()
    }
  })
</script>

<style scoped>
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
</style>

