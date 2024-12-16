<template>
  <div class="fixed inset-0 bg-black bg-opacity-80 flex justify-center items-center">
    <div class="bg-white rounded-lg p-6 w-1/3">
      <h2 class="text-xl font-bold mb-4 text-center">Recargo</h2>

      <div class="mb-4">
        <label class="block text-sm font-semibold mb-1">Descripción:</label>
        <input
          type="text"
          v-model="descripcion"
          :class="{'border-red-500': descripcionError}"
          class="border rounded p-2 w-full"
          placeholder="Describe el recargo"
        />
        <p v-if="descripcionError" class="text-red-500 text-sm mt-1">La descripción es obligatoria.</p>
      </div>

      <div class="mb-4">
        <label class="block text-sm font-semibold mb-1">Monto:</label>
        <input
          type="number"
          v-model.number="monto"
          :class="{'border-red-500': montoError}"
          class="border rounded p-2 w-full"
          placeholder="Ingrese el monto"
        />
        <p v-if="montoError" class="text-red-500 text-sm mt-1">El monto es obligatorio y debe ser mayor que 0.</p>
      </div>

      <div class="flex justify-end space-x-4">
        <button @click="$emit('close')" class="btn-danger">Cancelar</button>
        <button @click.prevent="confirmRecargo" class="btn-primary">Confirmar</button>
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
      descripcionError: false,
      montoError: false,
    };
  },
  methods: {
    confirmRecargo() {
      // Reset errors
      this.descripcionError = false;
      this.montoError = false;

      // Validation
      let valid = true;
      if (!this.descripcion.trim()) {
        this.descripcionError = true;
        valid = false;
      }
      if (!this.monto || this.monto <= 0) {
        this.montoError = true;
        valid = false;
      }

      // If valid, emit the values back to the parent
      if (valid) {
        this.$emit('confirm-recargo', { detalle: this.descripcion, monto: this.monto });
        this.descripcion = '';
        this.monto = 0;
      }
    },
  },
};
</script>

<style scoped>
.border-red-500 {
  border-color: #f56565;
}
.text-red-500 {
  color: #f56565;
}
</style>
