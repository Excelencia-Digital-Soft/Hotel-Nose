<template>
    <div class="fixed inset-0 flex justify-center items-center bg-black bg-opacity-50 z-50">
      <div class="bg-white p-6 rounded-lg w-96">
        <!-- Modal Content -->
        <p class="text-lg text-center mb-4">
          ¡Si anulas esta habitación también se anularán los consumos que tenga la visita!
          ¿Estás seguro de anular esta habitación?
        </p>
        
        <!-- Motivo Textbox -->
        <textarea 
          v-model="motivo" 
          placeholder="Escribe el motivo" 
          class="w-full h-24 p-2 border rounded-md mb-4"
        ></textarea>
        
        <!-- Confirm Button -->
        <button 
          :disabled="!motivo" 
          @click="confirmAnulacion" 
          class="btn-danger w-full h-12 rounded-md text-white mt-4"
        >
          Confirmar
        </button>
        
        <!-- Close Button -->
        <button 
        @click="$emit('close-modal')" 
        class="btn-danger absolute top-2 right-2 w-10 h-10 flex items-center justify-center rounded-full bg-red-500 text-white text-xl font-bold hover:bg-red-600 focus:outline-none"
>
  ×
</button>
      </div>
    </div>
  </template>
  
  <script setup>
  import { ref } from 'vue';
  import axiosClient from '../axiosClient'; // Import AxiosClient or adjust path as needed
  
  const props = defineProps({
    reservaId: Number,
  });
  
  const motivo = ref('');
  
  // Confirm Button Action
  const confirmAnulacion = () => {
    if (!motivo.value) return;
  
    axiosClient.delete(`/AnularOcupacion?reservaId=${props.reservaId}&motivo=${motivo.value}`)
      .then(res => {
        console.log(res.data);
        alert("Se anuló la reserva exitosamente");
      })
      .catch(error => {
        console.error(error);
      });
  };


  </script>
  
  <style scoped>
  /* Add any additional styles for the modal */
  </style>
  