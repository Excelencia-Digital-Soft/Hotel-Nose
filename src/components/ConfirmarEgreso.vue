<!-- File: src/components/ConfirmarEgreso.vue -->
<template>
    <div class="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50 z-50">
      <div class="bg-white p-6 rounded-lg shadow-lg w-1/3">
        <h3 class="text-xl font-bold mb-4">Confirmar Egreso</h3>
        <p class="mb-4">¿Estás seguro de que deseas confirmar estos egresos?</p>
  
        <!-- Loading indicator -->
        <div v-if="isLoading" class="text-center mb-4">
          <span class="text-purple-500">Procesando egresos...</span>
        </div>
  
        <!-- Error message display -->
        <div v-if="errorMessage" class="text-red-500 mb-4">
          {{ errorMessage }}
        </div>
  
        <div class="flex justify-end space-x-4">
          <button @click="$emit('close')" :disabled="isLoading" class="bg-gray-300 hover:bg-gray-400 text-black py-2 px-4 rounded">
            Cancelar
          </button>
          <button @click="confirmarEgresos" :disabled="isLoading" class="bg-purple-500 hover:bg-purple-600 text-white py-2 px-4 rounded">
            Confirmar
          </button>
        </div>
      </div>
    </div>
  </template>
  
  <script setup>
import axiosClient from '../axiosClient';
import { ref } from 'vue';
  
  const props = defineProps({
    egresos: {
      type: Array,
      required: true,
    },
  });
  
  const emit = defineEmits(['close', 'success']);
  
  const isLoading = ref(false);
  const errorMessage = ref('');
  
  // Method to confirm egresos by making POST requests for each item
  const confirmarEgresos = async () => {
  isLoading.value = true;
  errorMessage.value = '';

  try {
    for (const egreso of props.egresos) {
      const payload = {
        tipoEgresoId: egreso.TipoId,
        cantidad: egreso.cantidad,
        precio: egreso.importeUnitario,
        fecha: new Date().toISOString(),
      };

      await axiosClient.post('/CreateEgreso', payload);
    }

    emit('success'); // Notify the parent that egresos were successfully created
    emit('close');   // Close the modal
  } catch (error) {
    errorMessage.value = 'Hubo un error al procesar los egresos. Inténtalo de nuevo.';
    console.error('Error creating egresos:', error);
  } finally {
    isLoading.value = false;
  }
};
  </script>
  