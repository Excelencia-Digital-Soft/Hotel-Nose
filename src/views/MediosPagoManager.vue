<template>
  <div class="text-white min-h-screen p-4">
    <h1 class="text-2xl font-bold mb-6">Tarjetas</h1>

    <!-- Dark Gray Background Rectangle -->
    <div class="bg-gray-800 rounded-lg p-4">
      <!-- List of Tarjetas -->
      <div class="mb-4">
        <h3 class="font-semibold text-lg mb-2">Tarjetas</h3>
        <div class="overflow-x-auto">
          <table class="table-auto w-full border border-gray-600 text-sm">
            <thead>
              <tr class="bg-gray-700 border-b border-gray-500">
                <th class="px-3 py-2 border-r border-gray-500 text-center">Nombre</th>
                <th class="px-3 py-2 w-1 border-r border-gray-500 text-center">Porcentaje</th>
                <th class="px-3 py-2 w-1 text-center">Editar</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="tarjeta in tarjetas"
                :key="tarjeta.tarjetaID"
                class="border-b border-gray-600 text-center"
                :class="{ 'bg-gray-700': editingTarjetaId === tarjeta.tarjetaID }"
              >
                <td class="px-3 py-2 border-r border-gray-600">
                  <div v-if="editingTarjetaId !== tarjeta.tarjetaID">
                    <span class="font-semibold">{{ tarjeta.nombre }}</span>
                  </div>
                  <input
                    v-else
                    type="text"
                    v-model="editingTarjetaNombre"
                    class="bg-gray-700 text-white p-1 rounded w-full text-center"
                  />
                </td>
                <td class="px-3 py-2 border-r border-gray-600">
                  <div v-if="editingTarjetaId !== tarjeta.tarjetaID">
                    <span>{{ tarjeta.montoPorcentual }}%</span>
                  </div>
                  <input
                    v-else
                    type="number"
                    v-model.number="editingTarjetaMonto"
                    class="bg-gray-700 text-white p-1 rounded w-16 text-center"
                    min="0"
                    max="100"
                  />
                </td>
                <td class="px-3 py-2">
                  <button
                    class="btn btn-primary rounded-lg"
                    @click="editOrSaveTarjeta(tarjeta)"
                  >
                    <i v-if="editingTarjetaId !== tarjeta.tarjetaID" class="fas fa-pen"></i>
                    <i v-else class="fas fa-check"></i>
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Add Tarjeta Button (Inside Border) -->
        <div class="border-t border-gray-600">
          <div class="mt-2 flex justify-center py-2">
            <button class="btn btn-success rounded-lg border border-gray-500 px-4 py-1" @click="showCreateForm = !showCreateForm">
              <i class="fas fa-plus"></i> Agregar Tarjeta
            </button>
          </div>
        </div>

        <!-- New Tarjeta Creation Form -->
        <div v-if="showCreateForm" class="mt-2 border border-gray-500 p-4 bg-gray-700 rounded-lg">
          <div class="flex items-center space-x-2">
            <input
              type="text"
              v-model="newTarjetaNombre"
              placeholder="Nombre"
              class="bg-gray-600 text-white p-1 rounded w-full text-center"
            />
            <input
              type="number"
              v-model.number="newTarjetaMonto"
              placeholder="Monto (0-100)"
              class="bg-gray-600 text-white p-1 rounded w-16 text-center"
              min="0"
              max="100"
            />
          </div>

          <div class="mt-2 flex justify-center">
            <button
              class="btn rounded-lg"
              :class="{
                'btn-primary': newTarjetaNombre || newTarjetaMonto,
                'btn-danger': !newTarjetaNombre && !newTarjetaMonto,
              }"
              @click="createTarjeta"
            >
              <i v-if="newTarjetaNombre || newTarjetaMonto" class="fas fa-plus"></i>
              <i v-else class="fas fa-times"></i>
            </button>
          </div>
        </div>

      </div>

      <!-- Efectivo Section -->
      <div class="mb-4 mt-4 hidden">
        <h3 class="font-semibold text-lg mb-2">Efectivo</h3>
        <div class="overflow-x-auto">
          <table class="table-auto w-full border border-gray-600 text-sm">
            <thead>
              <tr class="bg-gray-700 border-b border-gray-500">
                <th class="px-3 py-2 border-r border-gray-500 text-center">Descripción</th>
                <th class="px-3 py-2 w-1 border-r border-gray-500 text-center">Porcentaje</th>
                <th class="px-3 py-2 w-1 text-center">Editar</th>
              </tr>
            </thead>
            <tbody>
              <tr class="border-b border-gray-600 text-center">
                <td class="px-3 py-2 border-r border-gray-600">
                  Descuento en efectivo
                </td>
                <td class="px-3 py-2 border-r border-gray-600">
                  <div v-if="!editingEfectivo">
                    <span>{{ descuentoEfectivo ? descuentoEfectivo.montoPorcentual : 0 }}%</span>
                  </div>
                  <input
                    v-else
                    type="number"
                    v-model.number="editingEfectivoMonto"
                    class="bg-gray-700 text-white p-1 rounded w-16 text-center"
                    min="0"
                    max="100"
                  />
                </td>
                <td class="px-3 py-2">
                  <button
                    class="btn btn-primary rounded-lg"
                    @click="editOrSaveEfectivo()"
                  >
                    <i v-if="!editingEfectivo" class="fas fa-pen"></i>
                    <i v-else class="fas fa-check"></i>
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import axiosClient from '../axiosClient'; // Adjust the path

// Data for Tarjetas
const tarjetas = ref([]);
const editingTarjetaId = ref(null);
const editingTarjetaNombre = ref('');
const editingTarjetaMonto = ref('');

const newTarjetaNombre = ref('');
const newTarjetaMonto = ref('');

const showCreateForm = ref(false); // Control visibility of create form

// Data for Efectivo
const descuentoEfectivo = ref(null);
const editingEfectivo = ref(false);
const editingEfectivoMonto = ref('');

// Fetch Tarjetas
const fetchTarjetas = async () => {
  try {
    const response = await axiosClient.get(`/GetTarjetas?InstitucionID=${InstitucionID.value}`);
    if (response.data.ok) {
      tarjetas.value = response.data.data;
    } else {
      console.error('Error fetching tarjetas:', response.data.message);
    }
  } catch (error) {
    console.error('Error fetching tarjetas:', error);
  }
};

// Edit or Save Tarjeta
const editOrSaveTarjeta = async (tarjeta) => {
  console.log("Editing ID:", tarjeta); // Debugging
  console.log("Monto Before Edit:", tarjeta.montoPorcentual); // Debugging

  if (editingTarjetaId.value === tarjeta.tarjetaID) {
    // Save
    if (
      editingTarjetaNombre.value !== tarjeta.nombre ||
      Number(editingTarjetaMonto.value) !== tarjeta.montoPorcentual
    ) {
      if (editingTarjetaMonto.value < 0 || editingTarjetaMonto.value > 100) {
        alert("El porcentaje debe estar entre 0 y 100.");
        return;
      }
      try {
        await axiosClient.put(
          `/UpdateTarjeta?id=${tarjeta.tarjetaID}&Nombre=${editingTarjetaNombre.value}&Monto=${editingTarjetaMonto.value}`
        );
        fetchTarjetas(); // Refresh list
      } catch (error) {
        console.error('Error updating tarjeta:', error);
      }
    }
    editingTarjetaId.value = null;
  } else {
    // Edit Mode: Set editing values correctly
    editingTarjetaId.value = tarjeta.tarjetaID; // ✅ Fix: Ensure tarjetaId is assigned
    editingTarjetaNombre.value = tarjeta.nombre;
    editingTarjetaMonto.value = tarjeta.montoPorcentual; // ✅ Fix: Corrected property name
  }
};

// Create Tarjeta
const createTarjeta = async () => {
  if (!newTarjetaNombre && !newTarjetaMonto) return;

  if (newTarjetaMonto.value < 0 || newTarjetaMonto.value > 100) {
    alert("El porcentaje debe estar entre 0 y 100.");
    return;
  }

  try {
    await axiosClient.post(
      `/CrearRecargoTarjeta?Nombre=${newTarjetaNombre.value}&Monto=${newTarjetaMonto.value}&InstitucionID=${InstitucionID.value}`
    );
    newTarjetaNombre.value = '';
    newTarjetaMonto.value = '';
    showCreateForm.value = false; // Hide the form after creation
    fetchTarjetas();
  } catch (error) {
    console.error('Error creating tarjeta:', error);
  }
};

// Fetch Descuento Efectivo
const fetchDescuentoEfectivo = async () => {
  try {
    const response = await axiosClient.get('/GetDescuentoEfectivo?institucionID=0');
    if (response.data) {
      descuentoEfectivo.value = response.data.data; // Assuming only one descuento exists
    } else {
      descuentoEfectivo.value = null; // No descuento found
    }
  } catch (error) {
    console.error('Error fetching descuento en efectivo:', error);
  }
};

// Edit or Save Descuento Efectivo
const editOrSaveEfectivo = async () => {
  if (editingEfectivo.value) {
    // Save
    if (editingEfectivoMonto.value < 0 || editingEfectivoMonto.value > 100) {
      alert("El porcentaje debe estar entre 0 y 100.");
      return;
    }

    try {
      if (descuentoEfectivo.value) {
        await axiosClient.put(
          `/UpdateDescuento?id=${descuentoEfectivo.value.descuentoID}&Monto=${editingEfectivoMonto.value}`
        );
      } else {
        // Create new
        await axiosClient.post(`/CrearDescuentoEfectivo?Monto=${editingEfectivoMonto.value}`);
      }
      fetchDescuentoEfectivo(); // Refresh
    } catch (error) {
      console.error('Error updating/creating descuento en efectivo:', error);
    }
    editingEfectivo.value = false;
  } else {
    // Edit
    editingEfectivo.value = true;
    editingEfectivoMonto.value = descuentoEfectivo.value ? descuentoEfectivo.value.montoPorcentual : 0;
  }
};

onMounted(() => {
  getDatosLogin();
  fetchTarjetas();
  fetchDescuentoEfectivo();
});
import { useAuthStore } from '../store/auth.js'; // Import the auth store
const InstitucionID = ref(null);
const authStore = useAuthStore();
function getDatosLogin(){
    InstitucionID.value = authStore.institucionID;
  }
</script>

<style scoped>

.btn-success {
  background-color: #28a745;
  color: white;
  padding: 0.5rem 1rem;
  border-radius: 0.25rem;
  cursor: pointer;
}

.btn {
    padding: 0.5rem 1rem;
    border-radius: 0.25rem;
    cursor: pointer;
}

</style>
