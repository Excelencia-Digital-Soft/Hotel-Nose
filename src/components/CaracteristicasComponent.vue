<template>
  <div class="container mx-auto p-4 m-4 shadow-sm ring-1 ring-inset ring-purple-300 focus:ring-2 focus:ring-purple-500 py-4 border-2 border-purple-300 rounded-3xl transition duration-150 ease-out md:ease-in hover:shadow-lg hover:shadow-purple-500/50">
    <!-- Componente Input para seleccionar o agregar Tipo de Caracteristica -->
    <div class="mb-4">
      <h2 class="text-lg font-semibold mb-2 text-white">Agregar Caracteristica</h2>
      <DropDownCreateSearchCaracters @addGasto="agregarAGastos" />
    </div>

    <!-- Tabla para listar los caracteristicas -->
    <div v-if="caracteristicas.length > 0" class="overflow-x-auto mb-4">
      <table class="table-auto w-full border-collapse border border-black">
        <thead class="bg-primary-300 text-white">
          <tr>
            <th class="px-4 py-1 border-black">Caracteristicas</th>
            <th class="px-4 py-1 border-black">icono</th>
            <th class="px-4 py-1 border-black">Descripcion</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(caracteristica, index) in caracteristicas" :key="caracteristica.TipoId">
            <td class="border border-black px-4 py-2 text-white">{{ caracteristica.nombre }}</td>
            <td class="flex justify-center items-center mt-2"><img v-if="caracteristica.icono" :src="caracteristica.icono" alt="Icono" class="w-6 h-6" /></td>
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
      </table>
    </div>
  </div>
  <!-- <div>
    <h2 class="text-xl font-semibold mt-6 mb-2">Crear Característica</h2>
    <div class="flex flex-col gap-2">
      <input v-model="nuevaCaracteristica.nombre" placeholder="Nombre" class="p-2 border rounded  text-neutral-900">
      <input v-model="nuevaCaracteristica.descripcion" placeholder="Descripción" class="p-2 border rounded text-neutral-900">
      <input type="file" @change="e => nuevaCaracteristica.icono = e.target.files[0]" class="p-2 border rounded">
      <button @click="crearCaracteristica" class="bg-blue-500 text-white px-4 py-2 rounded mt-2 hover:bg-blue-600">
        Crear Característica
      </button>
    </div>
  </div> -->
</template>

<script setup>
import { ref, onMounted, watch } from 'vue';
import axiosClient from '../axiosClient';
import DropDownCreateSearchCaracters from '../components/DropDownCreateSearchCaracters.vue'

const props = defineProps({
  trigger: Boolean,
  idHabitacion:Number,
  listaCaracteristicas:Array
})
const caracteristicas = ref([]);
const IdsCaracteristicas = ref([]);

watch(() => props.trigger, (newVal) => {
  if (newVal==true) {
    console.log("TRIGGER LLEGO",props.idHabitacion)
    AsignarCaractxHabitacion()
  }
})
// Watcher para listaCaracteristicas
watch(() => props.listaCaracteristicas, async (newList) => {
  if (newList && newList.length > 0) {
    caracteristicas.value = await cargarIconos(newList);
  }else{
    caracteristicas.value=[]
  }
}, { immediate: true });

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
    if (!props.idHabitacion || !IdsCaracteristicas.value?.length) {
      alert('Debe seleccionar características y una habitación válida');
      return;
    }

    const response = await axiosClient.post(
      `/api/Caracteristicas/AsignarCaracteristicasAHabitacion?habitacionId=${props.idHabitacion}`,
      IdsCaracteristicas.value, // Enviamos directamente el array como cuerpo
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
      IdsCaracteristicas.value = [];
    } else {
      alert(response.data?.message || 'Error al asignar características');
    }
  } catch (error) {
    console.error('Error:', error);
    alert('Error en la conexión con el servidor');
  }
};


const mostrarModal = ref(false);

// Agregar un caracteristica a la tabla
const agregarAGastos = (nuevoGasto) => {
  if (!nuevoGasto) return;
  console.log("HOLAAA",caracteristicas);
  const existente = caracteristicas.value.find(caracteristica => caracteristica.caracteristicaId === nuevoGasto.caracteristicaId);
  if (existente) {
    alert('El caracteristica ya fue agregado.');
    return;
  }

  caracteristicas.value.push({
    caracteristicaId: nuevoGasto.caracteristicaId,
    nombre: nuevoGasto.nombre,
    icono:nuevoGasto.icono
  });
  IdsCaracteristicas.value.push(nuevoGasto.caracteristicaId);
};

// Eliminar caracteristica de la tabla
const eliminarGasto = (index) => {
  caracteristicas.value.splice(index, 1);
};



// Modal control methods
const abrirModal = () => {
  mostrarModal.value = true;
};

const cerrarModal = () => {
  mostrarModal.value = false;
};
const handleSuccess = () => {
  alert('Egresos confirmados exitosamente.');
  caracteristicas.value = []; // Optionally clear the list after confirmation
}

</script>