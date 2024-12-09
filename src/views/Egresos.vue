<template>
  <div class="container mx-auto p-4">
    <!-- Componente Input para seleccionar o agregar Tipo de Gasto -->
    <div class="mb-4">
      <h2 class="text-lg font-semibold mb-2">Agregar Gasto</h2>
      <DropDownCreateSearchGastos 
        @addGasto="agregarAGastos"
      />
    </div>

    <!-- Tabla para listar los gastos -->
    <div v-if="gastos.length > 0" class="overflow-x-auto">
      <table class="table-auto w-full border-collapse border border-purple-200">
        <thead class="bg-purple-300 text-white">
          <tr>
            <th class="px-4 py-2">Tipo de Gasto</th>
            <th class="px-4 py-2">Cantidad</th>
            <th class="px-4 py-2">Importe Unitario</th>
            <th class="px-4 py-2">Importe Total</th>
            <th class="px-4 py-2">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(gasto, index) in gastos" :key="gasto.id">
            <td class="border px-4 py-2">{{ gasto.nombre }}</td>
            <td class="border px-4 py-2">
              <input 
                type="number" 
                v-model.number="gasto.cantidad" 
                class="w-20 border rounded-md p-1 text-center"
              />
            </td>
            <td class="border px-4 py-2">
              <input 
                type="number" 
                v-model.number="gasto.importeUnitario" 
                class="w-20 border rounded-md p-1 text-center"
              />
            </td>
            <td class="border px-4 py-2">{{ gasto.cantidad * gasto.importeUnitario }}</td>
            <td class="border px-4 py-2 text-center">
              <button 
                @click="eliminarGasto(index)" 
                class="text-red-500 hover:text-red-700 font-semibold"
              >
                Eliminar
              </button>
            </td>
          </tr>
        </tbody>
        <tfoot>
          <tr>
            <td colspan="3" class="border px-4 py-2 font-bold text-right">Total:</td>
            <td colspan="2" class="border px-4 py-2 font-bold">{{ totalGastos }}</td>
          </tr>
        </tfoot>
      </table>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue';
import DropDownCreateSearchGastos from '../components/DropDownCreateSearchGastos.vue';

const gastos = ref([]);

// Agregar un gasto a la tabla
const agregarAGastos = (nuevoGasto) => {
  if (!nuevoGasto) return;

  const existente = gastos.value.find(gasto => gasto.id === nuevoGasto.id);
  if (existente) {
    alert('El gasto ya fue agregado.');
    return;
  }

  gastos.value.push({
    id: nuevoGasto.id,
    nombre: nuevoGasto.nombre,
    cantidad: 1,
    importeUnitario: 0,
  });
};

// Eliminar gasto de la tabla
const eliminarGasto = (index) => {
  gastos.value.splice(index, 1);
};

// Calcular el total
const totalGastos = computed(() =>
  gastos.value.reduce((total, gasto) => total + gasto.cantidad * gasto.importeUnitario, 0)
);
</script>
