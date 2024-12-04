<template>
    <div class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center">
      <div class="bg-neutral-900 text-white p-6 rounded shadow-lg w-96">
        <h2 class="text-xl font-bold mb-4">
          {{ promocion ? 'Editar Promoción' : 'Crear Nueva Promoción' }}
        </h2>
  
        <div class="mb-4">
          <label class="block font-semibold mb-2">Detalle</label>
          <input 
            v-model="form.detalle" 
            type="text" 
            class="w-full p-2 rounded bg-neutral-800 text-white"
          />
        </div>
  
        <div class="mb-4">
          <label class="block font-semibold mb-2">Tarifa (Por hora)</label>
          <input 
            v-model.number="form.tarifa" 
            type="number" 
            class="w-full p-2 rounded bg-neutral-800 text-white"
          />
        </div>
  
        <div class="mb-4">
          <label class="block font-semibold mb-2">Cantidad de Horas</label>
          <input 
            v-model.number="form.cantidadHoras" 
            type="number" 
            class="w-full p-2 rounded bg-neutral-800 text-white"
          />
        </div>
  
        <div class="flex justify-end gap-2">
          <button 
            class="bg-red-600 px-4 py-2 rounded"
            @click="$emit('close-modal')"
          >
            Cancelar
          </button>
          <button 
            class="bg-green-600 px-4 py-2 rounded"
            @click="save"
          >
            Guardar
          </button>
        </div>
      </div>
    </div>
  </template>
  
  <script setup>
  import { ref, watch } from 'vue';
  
  const props = defineProps({
    promocion: Object, // Optional: null for new
  });
  
  const emit = defineEmits(['close-modal', 'save']);
  
  const form = ref({
    detalle: '',
    tarifa: null,
    cantidadHoras: null,
  });
  
  // Pre-fill form when editing
  watch(() => props.promocion, (newVal) => {
  if (newVal) {
    form.value = { ...newVal };
  } else {
    form.value = { detalle: '', tarifa: null, cantidadHoras: null };
  }
}, { immediate: true });

const save = () => {
  emit('save', {
    ...form.value,
    promocionID: props.promocion?.promocionID || null, // Pass ID for updates
  });
};
  </script>
  