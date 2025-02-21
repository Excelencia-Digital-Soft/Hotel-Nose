<template>
    <div class="text-white min-h-screen p-4">
      <h1 class="text-2xl font-bold mb-6">Lista de Empeños</h1>
    
      <!-- List of Empeños -->
      <div class="mb-6">
        <h3 class="font-semibold text-lg mb-4">Empeños</h3>
        
        <ul class="space-y-2">
          <!-- Iterate through the list of empeños -->
          <li 
            v-for="empeno in empeños.data" 
            :key="empeno.empeñoID"
            class="cursor-pointer hover:text-blue-600 border border-white p-4 h-[15vh] flex items-center justify-between"
          >
            <div>
              <p><strong>Detalle:</strong> {{ empeno.detalle || 'Sin descripción' }}</p>
              <p><strong>Monto:</strong> ${{ empeno.monto.toFixed(2) }}</p>
              <p><strong>Fecha:</strong> {{ new Date(empeno.fechaRegistro).toLocaleString() }}</p>
            </div>
            <!-- Pagar button -->
            <button 
            class="btn-primary" 
            @click="openPagarModal(empeno)">
            Pagar
          </button>          </li>
        </ul>
      </div>
      <div
  v-if="showPagarModal"
  class="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50"
>
  <ModalPagarEmpeño
    :empenoId="selectedEmpeño.empeñoID"
    :total="selectedEmpeño.monto"
    :visitaId="selectedEmpeño.visitaID"
    @close="toggleModal">
  </ModalPagarEmpeño>
  </div>
    </div>
  </template>
  
  <script setup>
  import { ref, onMounted } from 'vue';
  import axiosClient from '../axiosClient'; // Adjust the path to match your project structure
import ModalPagarEmpeño from '../components/ModalPagarEmpeño.vue';
  
  // Reactive array to hold empeños
  const empeños = ref([]);
  const showPagarModal = ref(false);
  const selectedEmpeño = ref({});
  const toggleModal = () => {
    showPagarModal.value = !showPagarModal.value
  }

  const openPagarModal = (empeno) => {
  selectedEmpeño.value = empeno; // Set the selected empeño
  console.log(selectedEmpeño.value.empeñoID);
  showPagarModal.value = true; // Show the modal
};

  // Fetch all empeños when the component is mounted
  const fetchEmpenos = async () => {
    try {
      const response = await axiosClient.get(`/api/Empeño/GetAllEmpenos?InstitucionID=${InstitucionID.value}`);
      if (response.data) {
        empeños.value = response.data;  // Assuming the data is directly in response.data
        console.log(empeños.value); // For debugging
      } else {
        console.error('Error fetching empeños:', response.data.message);
      }
    } catch (error) {
      console.error('Error fetching empeños:', error);
    }
  };
  
  // Call fetchEmpenos when the component is mounted
  onMounted(() => {
    getDatosLogin();
    fetchEmpenos();
  });

  import { useAuthStore } from '../store/auth.js'; // Import the auth store

const authStore = useAuthStore();
const InstitucionID = ref(null)
function getDatosLogin(){
    InstitucionID.value = authStore.auth?.institucionID;
  }

  </script>
  
  <style scoped>
  /* Add custom styles for the "Pagar" button */
  .btn-primary {
    background-color: #007bff;
    color: white;
    padding: 8px 16px;
    border-radius: 4px;
    border: none;
    cursor: pointer;
  }
  
  .btn-primary:hover {
    background-color: #0056b3;
  }
  
  /* Optional: Style for the list items */
  li {
    background-color: #1c1c1c;
    border-radius: 8px;
  }
  
  li:hover {
    background-color: #2d2d2d;
  }
  </style>
  