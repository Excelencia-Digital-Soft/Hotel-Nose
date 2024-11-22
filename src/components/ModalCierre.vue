<template>
    <!-- Modal container -->
    <div 
    class="fixed inset-0 bg-gray-800 bg-opacity-50 flex justify-center items-center" 
    @click.self="closeModal"
  >      <!-- Modal content -->
      <div class="bg-white w- h- rounded-lg overflow-y-auto p-6 relative">
        
        <!-- Title of the modal -->
        <h2 class="text-2xl text-black font-semibold mb-4">Lista de Pagos</h2>
  
        <!-- "Cerrar" Button -->
        <button 
          @click="$emit('close-modal')" 
          class="absolute top-4 right-4 bg-gray-300 hover:bg-gray-400 text-black px-4 py-2 rounded-full"
        >
          Cerrar
        </button>
  
        <!-- List of Pagos -->
        <div v-for="(pago, index) in selectedPagos" :key="pago.pagoId" class="border-b border-gray-300 py-4">
          <div class="flex justify-between items-center">
            <div class="font-semibold text-black">
              Pago {{ pago.pagoId }} - {{ formatFechaHora(pago.fechaHora) }}
            </div>
          </div>
          <div class="grid grid-cols-4 gap-4 mt-2">
            <div>
              <span class="text-gray-700 font-medium">Efectivo:</span> 
              <span class="text-green-600 font-semibold">${{ pago.montoEfectivo }}</span>
            </div>
            <div>
              <span class="text-gray-700 font-medium">Tarjeta:</span> 
              <span class="text-green-600 font-semibold">${{ pago.montoTarjeta }}</span>
            </div>
            <div>
              <span class="text-gray-700 font-medium">Billetera virtual:</span> 
              <span class="text-green-600 font-semibold">${{ pago.montoBillVirt }}</span>
            </div>
            <div>
              <span class="text-gray-700 font-medium">Descuento:</span> 
              <span class="text-green-600 font-semibold">${{ pago.montoDescuento }}</span>
            </div>
          </div> 
        </div>
        <div class="mt-2 font-semibold">
            <br/>
            <label class="text-black">Total: </label><span class="text-green-600">${{ calculateTotal(pago) }}</span>
        </div>
        <!-- "Cerrar Caja" Button (only shown if esAbierto is true) -->
        <div v-if="esAbierto" class="mt-4">
          <button class="w-full bg-red-600 text-black p-3 rounded-xl font-semibold hover:bg-red-700">
            Cerrar Caja
          </button>
        </div>
      </div>
    </div>
  </template>
  
  <script setup>
  import { defineProps, defineEmits } from 'vue';
  import axiosClient from '../axiosClient';
  // Props passed from the parent component
  const props = defineProps({
    selectedPagos: Array,
    esAbierto: Boolean,
  });
  const emit = defineEmits(['close-modal']);

  const closeModal = () => {
  emit('close-modal'); // Notify parent to close the modal
};

  // Method to format the fechaHora (assuming it's in ISO format)
  const formatFechaHora = (fechaHora) => {
    const date = new Date(fechaHora);
    return date.toLocaleString(); // Customize date formatting as needed
  };

  const calculateTotal = () => {
  return props.selectedPagos.reduce((total, pago) => {
    return total + pago.montoEfectivo + pago.montoTarjeta + pago.montoBillVirt;
  }, 0);

  const cerrarCaja = () =>{

  }
};
  </script>
  
  <style scoped>
  /* Modal content styles */
  .bg-white {
    background-color: white;
  }
  
  .w- {
    width: 75%;
  }
  
  .h- {
    height: 87vh;
  }
  
  .overflow-y-auto {
    overflow-y: auto;
  }
 
  
  .text-gray-700 {
    color: #4a4a4a;
  }
  
  .border-b {
    border-bottom-width: 1px;
  }
  
  .border-gray-300 {
    border-color: #e0e0e0;
  }
  
  .grid-cols-4 {
    grid-template-columns: repeat(4, 1fr);
  }
  
  .gap-4 {
    gap: 1rem;
  }
  
  .mt-4 {
    margin-top: 1rem;
  }
  
  .font-semibold {
    font-weight: 600;
  }
  
  
  .font-medium {
    font-weight: 500;
  }
  
  .p-3 {
    padding: 0.75rem;
  }
  
  .rounded-xl {
    border-radius: 1rem;
  }
  
  .bg-red-600 {
    background-color: #f56565;
  }
  
  
  .hover\:bg-red-700:hover {
    background-color: #c53030;
  }
  </style>
  