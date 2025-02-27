<template>
  <div class="container mx-auto p-4 bg-white shadow-sm ring-1 ring-inset ring-purple-300 focus:ring-2 focus:ring-purple-500 py-4 border-2 border-purple-300 rounded-3xl transition duration-150 ease-out md:ease-in hover:shadow-lg hover:shadow-purple-500/50">
    <!-- Componente Input para seleccionar o agregar Tipo de Gasto -->
    <div class="mb-4">
      <h2 class="text-lg font-semibold mb-2">Agregar Gasto</h2>
      <DropDownCreateSearchGastos @addGasto="agregarAGastos" />
    </div>

    <!-- Tabla para listar los gastos -->
    <div v-if="gastos.length > 0" class="overflow-x-auto mb-4">
      <table class="table-auto w-full border-collapse border border-black">
        <thead class="bg-purple-300 text-white">
          <tr>
            <th class="px-4 py-2 border border-black">Tipo de Gasto</th>
            <th class="px-4 py-2 border border-black">Cantidad</th>
            <th class="px-4 py-2 border border-black">Importe Unitario</th>
            <th class="px-4 py-2 border border-black">Importe Total</th>
            <th class="px-4 py-2 border border-black">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(gasto, index) in gastos" :key="gasto.TipoId">
            <td class="border border-black px-4 py-2">{{ gasto.nombre }}</td>
            <td class="border border-black px-4 py-2">
              <input
                type="number"
                v-model.number="gasto.cantidad"
                class="w-20 border rounded-md p-1 text-center"
              />
            </td>
            <td class="border border-black px-4 py-2">
              <input
                type="number"
                v-model.number="gasto.importeUnitario"
                class="w-20 border rounded-md p-1 text-center"
              />
            </td>
            <td class="border border-black px-4 py-2">{{ gasto.cantidad * gasto.importeUnitario }}</td>
            <td class="border border-black px-4 py-2 text-center">
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
            <td colspan="3" class="border border-black px-4 py-2 font-bold text-right">Total:</td>
            <td colspan="2" class="border border-black px-4 py-2 font-bold">{{ totalGastos }}</td>
          </tr>
        </tfoot>
      </table>
    </div>

    <!-- Confirmar Button -->
    <div v-if="gastos.length > 0" class="flex justify-end">
      <button
        @click="abrirModal"
        class="btn-primary bg-purple-500 hover:bg-purple-600 text-white font-bold py-2 px-4 rounded"
      >
        Confirmar
      </button>
    </div>

    <!-- ConfirmarEgreso Modal -->
    <ConfirmarEgreso v-if="mostrarModal" :egresos="gastos" @close="cerrarModal" @success="handleSuccess" />
  </div>
</template>


<script setup>
import { ref, computed, onMounted } from 'vue';
import DropDownCreateSearchGastos from '../components/DropDownCreateSearchGastos.vue';
import ConfirmarEgreso from '../components/ConfirmarEgreso.vue'; // Import the modal component

const gastos = ref([]);
const mostrarModal = ref(false);

// Agregar un gasto a la tabla
const agregarAGastos = (nuevoGasto) => {
  if (!nuevoGasto) return;
  console.log(gastos);
  const existente = gastos.value.find(gasto => gasto.TipoId === nuevoGasto.TipoId);
  if (existente) {
    alert('El gasto ya fue agregado.');
    return;
  }

  gastos.value.push({
    TipoId: nuevoGasto.TipoId,
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

// Modal control methods
const abrirModal = () => {
  mostrarModal.value = true;
};

const cerrarModal = () => {
  mostrarModal.value = false;
};
const handleSuccess = () => {
  alert('Egresos confirmados exitosamente.');
  gastos.value = []; // Optionally clear the list after confirmation
}

</script>