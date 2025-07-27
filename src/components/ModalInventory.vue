<template>
    <Teleport to="body" class="overflow-hidden">
      <Transition name="modal-outer" appear>
        <div class="fixed w-full h-full overflow-auto z-20 bg-black bg-opacity-80 backdrop-blur-lg top-0 left-0 flex justify-center px-8">
          <Transition name="modal-inner">
            <div class="w-4/6 bg-surface-900 fixed top-6 flex flex-col justify-evenly items-start p-8 pb-12 rounded-3xl border-8 border-purple-300/75">
              <h2 class="text-lg font-bold text-white">Inventario Actual de la Habitación</h2>
              <div class="container mx-auto">
                <!-- Contenedor con overflow-hidden y altura de 50vh para hacer la lista scrollable -->
                <div style="max-height: 50vh; overflow-y: auto;">
  <div v-if="!isLoading && inventario.length" class="grid grid-cols-3 gap-4 mx-2">
    <!-- Iterar sobre los artículos del inventario -->
    <div
      v-for="item in inventario"
      :key="item.inventarioId"
      class="bg-surface-700 text-white rounded-lg p-4 flex flex-col items-center justify-center"
    >
      <!-- Image container -->
      <div class="w-20 h-20 bg-gray-500 flex items-center justify-center rounded-md mb-2">
        <!-- Display fetched image or fallback -->
        <img
          :src="item.articulo.imageUrl || '../assets/image59.svg'"
          alt="Imagen del producto"
          class="w-full h-full object-cover"
        />
      </div>
      <!-- Article details -->
      <p>{{ item.articulo.nombreArticulo }}</p>
      <p class="text-sm text-gray-300">Cantidad: {{ item.cantidad }}</p>
    </div>
  </div>

  <!-- Loading state -->
  <div v-if="isLoading" class="flex justify-center items-center h-32">
    <p class="text-white">Cargando inventario...</p>
  </div>

  <!-- No items state -->
  <div v-if="!isLoading && !inventario.length" class="flex justify-center items-center h-32">
    <p class="text-white">No hay artículos en el inventario.</p>
  </div>
</div>

                <div>
    <!-- Button to show Add Item Modal -->
    <button
      @click="toggleAddItemModal"
      class="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-700"
    >
      Agregar Artículo
    </button>
    <div
      v-if="showAddModal"
      class="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50"
    >
      <ModalAddInventory
        :roomId="habitacionID"
        :currentInventory="inventario"
        @refreshInventory="fetchInventario"
        @close="toggleAddItemModal"
      />
    </div>
    </div>
              </div>
              <button
                class="btn-danger absolute text-md w-12 h-12 -top-6 right-0 rounded-full border-2 border-purple-200"
                @click="closeModal">X</button>
            </div>
          </Transition>
        </div>
      </Transition>
    </Teleport>
  </template>
  
  <script setup>
  import { onMounted, ref } from 'vue';
  import axiosClient from '../axiosClient';
  import ModalAddInventory from '../components/ModalAddInventory.vue'
import { fetchImage } from '../services/imageService';

  const emits = defineEmits(["close"]);
  const props = defineProps({
  room: Object
});
  
  const habitacionID = ref(props.room.habitacionId); // This should be passed as a prop in real implementation
  console.log(props.room.habitacionId)
  let isLoading = ref(false);
  const inventario = ref([]);
  const showAddModal = ref(false)
  
const fetchInventario = async () => {
  isLoading.value = true;
  try {
    // Use new V1 API endpoint
    const response = await axiosClient.get(`/api/v1/inventory/rooms/${habitacionID.value}`);
    if (response.data && response.data.isSuccess) {
      // Map over inventory data and fetch images for each articulo
      inventario.value = await Promise.all(
        response.data.data.map(async (item) => {
          const imageUrl = await fetchImage(item.articuloId); // Fetch the image URL
          return {
            ...item,
            // Adapt V1 API structure to component expectations
            inventarioId: item.inventoryId,
            articulo: {
              articuloId: item.articuloId,
              nombreArticulo: item.articuloNombre,
              precio: item.articuloPrecio,
              descripcion: item.articuloDescripcion,
              imageUrl,
            },
          };
        })
      );
      console.log('V1 Inventory loaded:', inventario.value);
    } else {
      console.error("Error en respuesta de API V1:", response.data);
    }
  } catch (error) {
    console.error("Error al obtener el inventario V1:", error);
    // Fallback to old API if V1 fails
    try {
      const fallbackResponse = await axiosClient.get(`/api/Inventario/GetInventario?habitacionID=${habitacionID.value}`);
      if (fallbackResponse.data && fallbackResponse.data.data) {
        inventario.value = await Promise.all(
          fallbackResponse.data.data.map(async (item) => {
            const imageUrl = await fetchImage(item.articulo.articuloId);
            return {
              ...item,
              articulo: {
                ...item.articulo,
                imageUrl,
              },
            };
          })
        );
        console.log('Fallback inventory loaded');
      }
    } catch (fallbackError) {
      console.error("Error en fallback:", fallbackError);
    }
  } finally {
    isLoading.value = false;
  }
};
  const toggleAddItemModal = () => {
  showAddModal.value = !showAddModal.value;

};
  const closeModal = () => {
    emits('close-modal');
  };
  
  onMounted(() => {
    // Fetch inventory data on component mount
    fetchInventario();
  });
  </script>
  
  <style scoped>
  .modal-outer-enter-active,
  .modal-outer-leave-active {
    transition: opacity 0.3s cubic-bezier(0.52, 0.02, 0.19, 1.02);
  }
  
  .modal-outer-enter-from,
  .modal-outer-leave-to {
    opacity: 0;
  }
  
  .modal-inner-enter-active {
    transition: all 0.3s cubic-bezier(0.52, 0.02, 0.19, 1.02) 0.10s;
  }
  
  .modal-inner-leave-active {
    transition: all 0.3s cubic-bezier(0.52, 0.02, 0.19, 1.02);
  }
  
  .modal-inner-enter-from {
    opacity: 0;
    transform: scale(0.8);
  }
  
  .modal-inner-leave-to {
    transform: scale(0.8);
  }
  </style>