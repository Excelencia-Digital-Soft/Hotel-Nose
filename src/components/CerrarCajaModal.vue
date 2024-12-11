<!-- File: src/components/CerrarCajaModal.vue -->
<template>
    <!-- Modal container -->
    <div class="fixed inset-0 bg-gray-800 bg-opacity-50 flex justify-center items-center" @click.self="closeModal">
      <!-- Modal content -->
      <div class="bg-white p-6 rounded-lg shadow-lg w-1/3">
        <h3 class="text-xl text-black font-bold mb-4">¿Estás seguro de cerrar caja?</h3>
        
        <!-- New initial amount input -->
        <div class="mb-4">
          <label for="montoInicial" class="block text-gray-700 font-medium mb-2">Monto inicial de la nueva caja:</label>
          <input
            type="number"
            v-model="montoInicial"
            id="montoInicial"
            class="w-full border text-black rounded-lg p-2 focus:outline-none focus:ring-2 focus:ring-purple-500"
            placeholder="Ingrese el monto inicial"
          />
        </div>
  
        <!-- Action buttons -->
        <div class="flex justify-end space-x-4">
          <button @click="closeModal" class="bg-gray-300 hover:bg-gray-400 text-black py-2 px-4 rounded">
            Cancelar
          </button>
          <button @click="confirmCerrarCaja" class="bg-purple-500 hover:bg-purple-600 text-white py-2 px-4 rounded">
            Confirmar
          </button>
        </div>
      </div>
    </div>
  </template>
  
  <script setup>
  import { ref, defineEmits } from 'vue';
  import axiosClient from '../axiosClient';
  
  // Emit to close the modal
  const emit = defineEmits(['close', 'refresh']);
  
  // New initial amount for the next cash register
  const montoInicial = ref(0);
  
  // Function to close the modal
  const closeModal = () => {
    emit('close');
  };
  
  // Function to handle the confirm action
  const confirmCerrarCaja = async () => {
    try {
      const response = await axiosClient.post(`/api/Caja/CierreCaja?montoInicial=${montoInicial.value}&observacion=Prueba`);
      if (response.data.ok) {
        alert('Caja cerrada exitosamente');
        emit('refresh'); // Emit event to refresh the page
      } else {
        alert('Error al cerrar caja: ' + response.data.message);
      }
    } catch (error) {
      console.error('Error al cerrar caja:', error);
      alert('Ocurrió un error al cerrar caja.');
    }
  };
  </script>
  
  <style scoped>
  /* Modal styling */
  .bg-white {
    background-color: white;
  }
  </style>
  