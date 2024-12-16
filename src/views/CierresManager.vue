<template>
    <div class="text-white min-h-screen p-4">
      <h1 class="text-2xl font-bold mb-6">Cierres y Pagos</h1>
  
      <!-- List of Cierres -->
      <div class="mb-6">
        <h3 class="font-semibold text-lg mb-4">Cierres</h3>
        <DropDownCreateSearchGastos/>
        <button 
        class="cursor-pointer hover:text-blue-600 border border-white p-4 h-[15vh] flex items-center justify-center w-full"

          @click="abrirPagosSinCierre" 
          type="button" 
        >
          Cierre Actual
        </button>
        <ul class="space-y-2">
          <li 
            v-for="cierre in cierres" 
            :key="cierre.cierreId" 
            @click="fetchPagosByCierre(cierre.cierreId)" 
            class="cursor-pointer hover:text-blue-600 border border-white p-4 h-[15vh] flex items-center justify-center"
          >
            {{ cierre.cierreId === 0 ? 'Cierre Actual' : `Cierre ${cierre.fechaHoraCierre}` }}
          </li>
        </ul>
      </div>
  
      <!-- Display Pagos for the selected Cierre -->
      <div
  v-if="showPagosModal"
  class="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50"
>
  <ModalCierre
    :selectedPagos="pagos"
    :esAbierto="false"
    @close-modal="togglePagosModal">
  </ModalCierre>
  </div>
  <div
  v-if="showPagosSinCierreModal"
  class="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50"
>
  <ModalCierre
    :selectedPagos="pagosSinCierres"
    :esAbierto="true"
    @close-modal="togglePagosSinCierreModal">
  </ModalCierre>
  </div>
    </div>
  </template>

  <script setup>
  import { ref } from 'vue';
  import axiosClient from '../axiosClient'; // Adjust the path to match your project structure
  import ModalCierre from '../components/ModalCierre.vue';
  // Data for cierres and pagos
  const cierres = ref([]);
  const showPagosModal = ref(false);
  const showPagosSinCierreModal = ref(false);

  const pagos = ref([]);
  const pagosSinCierres = ref([]);

  const togglePagosModal = () => {
    showPagosModal.value = !showPagosModal.value
  }
  const togglePagosSinCierreModal = () => {
    showPagosSinCierreModal.value = !showPagosSinCierreModal.value
  }
  
  // Fetch all cierres with their associated pagos when the view is loaded
  const fetchCierres = async () => {
    try {
      const response = await axiosClient.get('/api/Caja/GetCierresConPagos');
      if (response.data.ok) {
        cierres.value = response.data.data.cierres;
        pagosSinCierres.value = response.data.data.pagosSinCierre
        console.log(pagosSinCierres.value);
      } else {
        console.error('Error fetching cierres:', response.data.message);
      }
    } catch (error) {
      console.error('Error fetching cierres:', error);
    }
  };

  // Fetch pagos by CierreId when a Cierre is clicked
  const fetchPagosByCierre = (cierreId) => {
    const cierre = cierres.value.find((item) => item.cierreId === cierreId);
    if (cierre) {
      pagos.value = cierre.pagos || [];
    }
    togglePagosModal()
  };

  const abrirPagosSinCierre = () => {
    pagos.value = pagosSinCierres.value
    togglePagosSinCierreModal()
  };
  
  
  // Call fetchCierres on component mount
  fetchCierres();
  </script>
  
  <style scoped>
  /* Add custom styles if needed */
  </style>
  