<template>
  <div class="text-white min-h-screen p-4">
    <h1 class="text-2xl font-bold mb-6">Gestión de Promociones</h1>

    <!-- Dropdown for selecting a category -->
    <div class="mb-6">
      <label class="block text-lg font-semibold mb-2">Seleccionar Categoría</label>
      <select 
        v-model="selectedCategory" 
        @change="fetchPromociones" 
        class="bg-neutral-800 text-white rounded p-6 w-full"
      >
        <option v-for="categoria in categorias" :key="categoria.categoriaId" :value="categoria.categoriaId">
          {{ categoria.nombreCategoria }}
        </option>
      </select>
    </div>

  <!-- Promotions Grid -->
  <div 
      class="grid gap-6"
      :class="{
        'grid-cols-1': screenWidth < 640,
        'grid-cols-2': screenWidth >= 640 && screenWidth < 1024,
        'grid-cols-3': screenWidth >= 1024 && screenWidth < 1280,
        'grid-cols-4': screenWidth >= 1280
      }"
    >
      <!-- Existing Promotions -->
      <div 
        v-for="promocion in promociones" 
        :key="promocion.promocionID" 
        class="border border-white rounded p-6 bg-neutral-900 cursor-pointer hover:bg-neutral-800 flex flex-col items-center justify-center text-center"
        @click="openModal(promocion)"
      >
        <h3 class="font-bold text-xl mb-2">{{ promocion.detalle }}</h3>
        <p class="text-lg">Tarifa: {{ promocion.tarifa }}</p>
        <p class="text-lg">Horas: {{ promocion.cantidadHoras }}</p>
      </div>

      <!-- Add New Promotion Box -->
      <div 
        class="border btn-primary border-white rounded p-6 flex items-center justify-center bg-neutral-700 hover:bg-neutral-600 cursor-pointer"
        @click="openCreateModal"
      >
        <span class="text-3xl font-bold">+</span>
      </div>
    </div>

    <!-- Promotion Modal -->
    <PromotionModal 
      v-if="showModal" 
      :promocion="selectedPromocion" 
      @close-modal="closeModal"
      @save="handleSave"
    />
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue';
import axiosClient from '../axiosClient'; // Adjust to your project's path
import PromotionModal from '../components/PromotionModal.vue'; // Modal component

// Reactive states
const categorias = ref([]);
const selectedCategory = ref(null);
const promociones = ref([]);
const showModal = ref(false);
const selectedPromocion = ref(null);

// Fetch categories on mount
const fetchCategorias = async () => {
  try {
    const response = await axiosClient.get(`/GetCategorias?InstitucionID=${InstitucionID.value}`);
    if (response.data.ok) {
      categorias.value = response.data.data;
    } else {
      console.error('Error fetching categories:', response.data.message);
    }
  } catch (error) {
    console.error('Error fetching categories:', error);
  }
};

// Fetch promociones when category is selected
const fetchPromociones = async () => {
  if (!selectedCategory.value) return;

  try {
    const response = await axiosClient.get(`/api/Promociones/GetPromocionesCategoria?categoriaID=${selectedCategory.value}`);
    if (response.data.ok) {
      promociones.value = response.data.data;
    } else {
      console.error('Error fetching promociones:', response.data.message);
    }
  } catch (error) {
    console.error('Error fetching promociones:', error);
  }
};

// Modal handlers
const openModal = (promocion) => {
  selectedPromocion.value = promocion;
  showModal.value = true;
};

const openCreateModal = () => {
  selectedPromocion.value = null; // No data for creation
  showModal.value = true;
};

const closeModal = () => {
  showModal.value = false;
};

const handleSave = async (data) => {
  try {
    if (data.promocionID) {
      // Update Promotion
      const response = await axiosClient.put(
        `/api/Promociones/UpdatePromocion?promocionID=${data.promocionID}&tarifa=${data.tarifa}&cantidadHoras=${data.cantidadHoras}&detalle=${encodeURIComponent(data.detalle)}`
      );
      if (response.data.ok) {
        console.log('Promoción actualizada:', response.data);
      } else {
        console.error('Error updating promoción:', response.data.message);
      }
    } else {
      // Create Promotion
      const response = await axiosClient.post(
        `/api/Promociones/AddPromocion?tarifa=${data.tarifa}&cantidadHoras=${data.cantidadHoras}&categoriaID=${selectedCategory.value}&Detalle=${encodeURIComponent(data.detalle)}&InstitucionID=${InstitucionID.value}`
      );
      if (response.data.ok) {
        console.log('Promoción creada:', response.data);
      } else {
        console.error('Error creating promoción:', response.data.message);
      }
    }

    // Refetch promociones to refresh the grid
    await fetchPromociones();
    closeModal();
  } catch (error) {
    console.error('Error saving promoción:', error);
  }
};

// Fetch categories on component mount
onMounted(getDatosLogin);

onMounted(fetchCategorias);

// TODO ESTO ES PARA TEMA VISUAL
const screenWidth = ref(window.innerWidth);

const updateScreenWidth = () => {
  screenWidth.value = window.innerWidth;
};

onMounted(() => {
  window.addEventListener('resize', updateScreenWidth);
});

onUnmounted(() => {
  window.removeEventListener('resize', updateScreenWidth);
});
import { useAuthStore } from '../store/auth.js'; // Import the auth store
const InstitucionID = ref(null);
const authStore = useAuthStore();
function getDatosLogin(){
    InstitucionID.value = authStore.auth?.institucionID;
  }

</script>

  