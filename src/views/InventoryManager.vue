<template>
  <div class="inventory-container p-4">
    <h2 class="text-lg font-bold text-white text-center mb-4">Inventario</h2>
    <input type="text" v-model="keyword"
          class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition-colors mb-4 "
          placeholder="Buscar articulos" />
    <div v-if="isLoading" class="text-gray-600">Cargando inventario...</div>
    <div v-else class="inventory-list grid grid-cols-1 md:grid-cols-2 gap-4">
      <div
        v-for="item in computedinventory"
        :key="item.articuloId"
        class="inventory-item bg-white p-4 shadow rounded-lg flex justify-between items-center"
      >
        <div>
          <h3 class="font-semibold text-gray-700">{{ item.articulo.nombreArticulo }}</h3>
          <p class="text-sm text-gray-500">Stock Actual: {{ item.cantidad }}</p>
        </div>
        <div class="flex items-center space-x-2">
          <input
            v-model.number="item.newStock"
            type="number"
            min="0"
            class="w-16 p-1 border rounded-lg text-center text-gray-700"
            placeholder="Qty"
          />
          <button
            @click="updateStock(item.inventarioId, item.newStock)"
            class="bg-blue-500 text-white px-3 py-1 rounded-lg hover:bg-blue-600"
          >
            Actualizar
          </button>
        </div>
      </div>
    </div>
    <!-- Main inventory container -->
  </div>

  <!-- "Actualizar" button always fixed at the bottom-right of the screen -->
  <button
    @click="actualizarInventario"
    class="fixed bottom-6 right-6 bg-green-500 text-white px-4 py-2 rounded-lg shadow-lg hover:bg-green-700"
  >
    Actualizar
  </button>
</template>

<script setup>
import { onMounted, ref,computed } from 'vue';
import axiosClient from '../axiosClient';
import { useAuthStore } from '../store/auth.js'; // Import the auth store

const authStore = useAuthStore();

const inventory = ref([]);
const isLoading = ref(true);

const keyword = ref("");

const computedinventory = computed(() => inventory.value.filter(i => i.articulo.nombreArticulo.toLowerCase().includes(keyword.value.toLowerCase())))

// Fetch all inventory items
const fetchInventory = async () => {
  const institucionID = authStore.institucionID;
  if (institucionID == null) {
  console.warn('InstitucionID is not available.  Please ensure the user is logged in.');
  return; }
  try {
    const response = await axiosClient.get(`/GetInventarioGeneral?InstitucionID=${institucionID}`);
    if (response.data && response.data.data) {
      inventory.value = response.data.data.map((item) => ({
        ...item,
        newStock: item.cantidad, // Initialize editable stock quantity
      }));
    }
  } catch (error) {
    console.error('Error fetching inventory:', error);
  } finally {
    isLoading.value = false;
  }
};

// Update stock quantity
const updateStock = async (itemId, newStock) => {
  try {
    await axiosClient.put('/UpdateStockGeneral', [
      {
        inventarioId: itemId,
        cantidad: newStock,
      },
    ]);

    // Refresh inventory list after updating stock
    await fetchInventory();
  } catch (error) {
    console.error('Error updating stock:', error);
  }
};

// Call /CoordinarInventarioGeneral
const actualizarInventario = async () => {
  const institucionID = authStore.institucionID;
  if (institucionID == null) {
  console.warn('InstitucionID is not available.  Please ensure the user is logged in.');
  return; }
  try {
    isLoading.value = true;
    await axiosClient.get(`/CoordinarInventarioGeneral?InstitucionID=${institucionID}`);
    await fetchInventory(); // Refresh inventory after coordinating
    console.log('Inventario coordinado con éxito.');
  } catch (error) {
    console.error('Error al coordinar inventario:', error);
  } finally {
    isLoading.value = false;
  }
};

// Fetch inventory on mount
onMounted(fetchInventory);
</script>

<style scoped>
.inventory-container {
  max-width: 800px;
  margin: auto;
}

.inventory-list {
  display: grid;
  gap: 1rem;
}

.inventory-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

button {
  transition: background-color 0.3s ease;
}

/* Styles for the fixed "Actualizar" button */
.fixed {
  position: fixed;
  z-index: 1000;
}
</style>
