<template>
  <Teleport to="body">
    <!-- PrimeVue Toast for notifications -->
    <Toast position="top-right" />
    
    <div class="fixed inset-0 flex justify-center items-center bg-black bg-opacity-50 z-50">
    <div class="bg-white p-6 rounded-lg w-96 relative">
      <!-- Close Button -->
      <button 
        @click.prevent="$emit('close-modal')" 
        class="absolute -top-4 -right-4 w-10 h-10 flex items-center justify-center rounded-full bg-red-500 text-white text-xl font-bold hover:bg-red-600 focus:outline-none shadow-lg"
      >
        ×
      </button>
      
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
    </div>
  </div>
  </Teleport>
</template>


  
  <script setup>
  import { ref } from 'vue';
  import axiosClient from '../axiosClient';
  import Toast from 'primevue/toast';
  import { useToast } from 'primevue/usetoast';
  
  const props = defineProps({
    reservaId: Number,
  });
  
  const emit = defineEmits(['close-modal', 'ocupacion-anulada']);
  const toast = useToast();
  
  const motivo = ref('');
  
  // Confirm Button Action
  const confirmAnulacion = () => {
    if (!motivo.value) return;
  
    axiosClient.delete(`/AnularOcupacion?reservaId=${props.reservaId}&motivo=${motivo.value}`)
      .then(res => {
        console.log(res.data);
        toast.add({
          severity: 'success',
          summary: 'Éxito',
          detail: 'Reserva anulada exitosamente',
          life: 10000
        });
        
        // Emit event to parent component instead of reloading
        setTimeout(() => {
          emit('ocupacion-anulada', props.reservaId);
          emit('close-modal');
        }, 1500);
      })
      .catch(error => {
        console.error(error);
        toast.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Error al anular la reserva. Por favor, intente nuevamente.',
          life: 10000
        });
      });
  };


  </script>
  
  <style scoped>
  /* Add any additional styles for the modal */
  </style>
  