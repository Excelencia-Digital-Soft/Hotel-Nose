<template>
    <div class="fixed inset-0 bg-black bg-opacity-80 flex justify-center items-center">
      <div class="bg-white rounded-lg p-6 w-1/3">
        <h2 class="text-xl font-bold mb-4 text-center">Empeñar</h2>
        <div class="mb-4">
          <label class="block text-sm font-semibold mb-1">Descripción:</label>
          <input
            type="text"
            v-model="descripcion"
            class="border rounded p-2 w-full"
            placeholder="Describe el empeño"
          />
        </div>
        <div class="mb-4">
          <label class="block text-sm font-semibold mb-1">Monto:</label>
          <input
            type="number"
            v-model.number="monto"
            class="border rounded p-2 w-full"
            placeholder="Ingrese el monto"
          />
        </div>
        <div class="flex justify-end space-x-4">
          <button @click="$emit('close')" class="btn-danger">Cancelar</button>
          <button @click.prevent="confirmEmpeno" class="btn-primary">Confirmar</button>
        </div>
      </div>
    </div>
  </template>
  
  <script>
  export default {
    data() {
      return {
        descripcion: '',
        monto: 0,
      };
    },
    methods: {
      confirmEmpeno() {
        // Emit the values back to the parent
        this.$emit('confirm-empeno', { detalle: this.descripcion, monto: this.monto });
        this.descripcion = '';
        this.monto = 0;
      },
    },
  };
  
  </script>
  
  <style scoped>
  /* Add any necessary styling here */
  </style>
  