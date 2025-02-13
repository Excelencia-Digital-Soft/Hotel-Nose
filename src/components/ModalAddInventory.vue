<template>
  <Teleport to="body" class="overflow-hidden">
    <Transition name="modal-outer" appear>
      <div class="fixed w-full h-full overflow-auto z-20 bg-black bg-opacity-80 backdrop-blur-lg top-0 left-0 flex justify-center px-8">
        <Transition name="modal-inner">
          <div class="w-4/6 bg-surface-900 rounded-3xl p-8 pb-12 flex flex-col">
            <h2 class="text-lg font-bold text-white">Agregar Artículo a Inventario</h2>
            <div class="container mx-auto">
              <!-- Container with scrollable content -->
              <div style="max-height: 80vh; overflow-y: auto;">
                <div class="grid grid-cols-3 gap-4 mx-2">
                  <div
                    v-for="item in availableItems"
                    :key="item.articuloId"
                    @click="toggleSeleccion(item)"
                    :class="{
                      'relative cursor-pointer text-white rounded-lg p-4 flex flex-col items-center justify-center border-2 border-transparent transition-colors': true,
                      'bg-secondary-900 border-primary-500 ring-4': seleccionados.includes(item),
                    }"
                  >
                    <div class="w-20 h-20 bg-gray-500 flex items-center justify-center rounded-md mb-2">
                      <img
                        :src="item.imageUrl || '../assets/image59.svg'"
                        alt="Imagen del producto"
                        class="w-full h-full object-cover"
                      />
                    </div>
                    <p>{{ item.nombreArticulo }}</p>
                    <input
                      v-model.number="itemQuantity[item.articuloId]"
                      type="number"
                      placeholder="Cantidad"
                      class="bg-gray-800 text-white rounded-full border-0 w-full px-4 py-2 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-purple-300"
                      min="1"
                    />
                    <button
                      class="mt-2 bg-green-500 text-white px-2 py-1 rounded hover:bg-green-700"
                      @click.prevent="addItemToInventory(item.articuloId)"
                    >
                      Agregar
                    </button>
                  </div>
                </div>
              </div>
            </div>
            <div class="flex mt-4 justify-between">
              <button
                type="button"
                class="btn-light text-md w-1/3 h-12 rounded-3xl border-2 border-purple-200"
                @click="confirmarAccion"
              >
                Confirmar
              </button>
              <button
                type="button"
                class="btn-danger text-md w-1/3 h-12 rounded-3xl transition-colors border-2 border-purple-200"
                @click="closeModal"
              >
                Cancelar
              </button>
            </div>
            <button
              class="btn-danger absolute text-md w-12 h-12 -top-6 right-0 rounded-full border-2 border-purple-200"
              @click="closeModal"
            >
              X
            </button>
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { ref, defineProps, defineEmits, onMounted, watch } from 'vue';
import axiosClient from '../axiosClient';

// Image service to fetch images
const fetchImage = async (articuloId) => {
  try {
    const response = await axiosClient.get(`api/Articulos/GetImage/${articuloId}`, {
      responseType: 'blob',
    });
    return URL.createObjectURL(response.data); // Convert blob to object URL
  } catch (error) {
    console.error(`Error fetching image for articuloId ${articuloId}:`, error);
    return null; // Return null if fetching fails
  }
};

// Props for the component
const props = defineProps({
  roomId: {
    type: Number,
    required: true,
  },
  currentInventory: {
    type: Array,
    required: true,
  },
});

const emits = defineEmits(["close", "refreshInventory"]);
const itemQuantity = ref({});
const isLoading = ref(false);
const allItems = ref([]);
const seleccionados = ref([]); // Array to store selected items
const availableItems = ref([]);

// Fetch all items and include image URLs
const fetchAllItems = async () => {
  isLoading.value = true;
  try {
    const response = await axiosClient.get(`/api/Articulos/GetArticulos?InstitucionID=${InstitucionID.value}`);
    const items = response.data?.data || [];
    // Fetch images for each item
    allItems.value = await Promise.all(
      items.map(async (item) => {
        const imageUrl = await fetchImage(item.articuloId);
        return {
          ...item,
          imageUrl,
        };
      })
    );
    filterAvailableItems(); // Filter items
  } catch (error) {
    console.error("Error fetching items:", error);
  } finally {
    isLoading.value = false;
  }
};

// Filter items not in the current inventory
const filterAvailableItems = () => {
  if (props.currentInventory != null) {
    const currentItemIds = props.currentInventory.map((item) => item.articulo.articuloId);
    availableItems.value = allItems.value.filter(
      (item) => !currentItemIds.includes(item.articuloId)
    );
  }
};

// Toggle selection of an item
const toggleSeleccion = (item) => {
  const index = seleccionados.value.findIndex(
    (selectedItem) => selectedItem.articuloId === item.articuloId
  );
  if (index !== -1) {
    seleccionados.value.splice(index, 1);
  } else {
    seleccionados.value.push(item);
  }
};

// Add selected item to inventory
const addItemToInventory = (itemId) => {
  const cantidad = itemQuantity.value[itemId];
  axiosClient
    .post(
      `/api/Inventario/AddInventario?Cantidad=${cantidad}&ArticuloID=${itemId}&HabitacionID=${props.roomId}`
    )
    .then(() => {
      emits("refreshInventory"); // Notify parent to refresh inventory
      emits("close");
    })
    .catch((error) => {
      console.error("Error adding item to inventory:", error);
      emits("close");
    });
};

// Fetch all items on component mount
onMounted(() => {
  fetchAllItems();
});

import { useAuthStore } from '../store/auth.js'; // Import the auth store
const InstitucionID = ref(null);
const authStore = useAuthStore();
function getDatosLogin(){
    InstitucionID.value = authStore.auth?.InstitucionID;
  }
  onMounted(() => {
  getDatosLogin();
});
// Watch for inventory updates and re-filter available items
watch(() => props.currentInventory, filterAvailableItems, { immediate: true });

const closeModal = () => {
  emits("close");
};
</script>
