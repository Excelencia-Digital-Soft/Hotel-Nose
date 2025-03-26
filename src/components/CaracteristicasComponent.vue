<template>
  <div class="container mx-auto p-4 m-4 shadow-sm ring-1 ring-inset ring-purple-300 focus:ring-2 focus:ring-purple-500 py-4 border-2 border-purple-300 rounded-3xl transition duration-150 ease-out md:ease-in hover:shadow-lg hover:shadow-purple-500/50">
    <!-- Componente Input para seleccionar o agregar Tipo de Caracteristica -->
    <div class="mb-4">
      <h2 class="text-lg font-semibold mb-2 text-white">Agregar Caracteristica</h2>
      <DropDownCreateSearchCaracters @addGasto="agregarCaracteristica" />
    </div>

    <!-- Tabla para listar los caracteristicas -->
    <div v-if="caracteristicas.length > 0" class="overflow-x-auto mb-4">
      <div class="max-h-[300px] overflow-y-auto">
        <table class="table-auto w-full border-collapse border border-black">
          <thead class="bg-primary-300 text-white">
            <tr>
              <th class="px-4 py-1 border-black">Caracteristicas</th>
              <th class="px-4 py-1 border-black">icono</th>
              <th class="px-4 py-1 border-black">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(caracteristica, index) in caracteristicas" :key="caracteristica.caracteristicaId">
              <td class="border border-black px-4 py-2 text-white">{{ caracteristica.nombre }}</td>
              <td class="flex justify-center items-center mt-2">
                <img v-if="caracteristica.icono" :src="caracteristica.icono" alt="Icono" class="w-6 h-6" />
              </td>
              <td class="border border-black px-4 py-2 text-center">
                <button
                  @click="eliminarCaracteristica(index)"
                  class="text-red-500 hover:text-red-700 font-semibold"
                >
                  Eliminar
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue';
import axiosClient from '../axiosClient';
import DropDownCreateSearchCaracters from '../components/DropDownCreateSearchCaracters.vue'

const props = defineProps({
  trigger: Boolean,
  idHabitacion: Number,
  listaCaracteristicas: Array
})

const caracteristicas = ref([]);

// Watcher para listaCaracteristicas
watch(() => props.listaCaracteristicas, async (newList) => {
  if (newList && newList.length > 0) {
    caracteristicas.value = await cargarIconos(newList);
  } else {
    caracteristicas.value = []
  }
}, { immediate: true });

// Watcher para trigger
watch(() => props.trigger, (newVal) => {
  if (newVal === true) {
    AsignarCaractxHabitacion()
  }
})

// Función para obtener los IDs de las características
const obtenerIdsCaracteristicas = () => {
  return caracteristicas.value.map(c => c.caracteristicaId);
}

// Función para cargar los iconos
const cargarIconos = async (lista) => {
  return await Promise.all(
    lista.map(async (caracteristica) => {
      if (caracteristica.icono) {
        try {
          const response = await axiosClient.get(`/api/Caracteristicas/GetImage/${caracteristica.caracteristicaId}`, {
            responseType: 'blob',
          });
          return {
            ...caracteristica,
            icono: URL.createObjectURL(response.data)
          };
        } catch (error) {
          console.error(`Error al cargar el icono de ${caracteristica.nombre}:`, error);
          return {
            ...caracteristica,
            icono: null
          };
        }
      }
      return {
        ...caracteristica,
        icono: null
      };
    })
  );
};

const AsignarCaractxHabitacion = async () => {
  try {
    // Verificar que tenemos datos válidos
    if (!props.idHabitacion || caracteristicas.value.length === 0) {
      alert('Debe seleccionar características y una habitación válida');
      return;
    }

    const response = await axiosClient.post(
      `/api/Caracteristicas/AsignarCaracteristicasAHabitacion?habitacionId=${props.idHabitacion}`,
      obtenerIdsCaracteristicas(),
      {
        headers: {
          'Content-Type': 'application/json'
        }
      }
    );

    if (response.data?.ok) {
      alert(response.data.message);
      // Opcional: limpiar la selección después de guardar
      caracteristicas.value = [];
    } else {
      alert(response.data?.message || 'Error al asignar características');
    }
  } catch (error) {
    console.error('Error:', error);
    alert('Error en la conexión con el servidor');
  }
};

// Agregar una característica
const agregarCaracteristica = (nuevaCaracteristica) => {
  if (!nuevaCaracteristica) return;
  
  const existente = caracteristicas.value.some(
    c => c.caracteristicaId === nuevaCaracteristica.caracteristicaId
  );
  
  if (existente) {
    alert('La característica ya fue agregada.');
    return;
  }

  caracteristicas.value.push({
    caracteristicaId: nuevaCaracteristica.caracteristicaId,
    nombre: nuevaCaracteristica.nombre,
    icono: nuevaCaracteristica.icono
  });
};

// Eliminar característica
const eliminarCaracteristica = (index) => {
  caracteristicas.value.splice(index, 1);
};
</script>