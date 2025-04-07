<template>
  <div class="m-8 w-1/2">
    <h2 class="text-xl text-white font-semibold mt-6 mb-2">Manejador de Características</h2>
    <div class="grid grid-cols-3 gap-2 border-2 border-primary-500 p-4 rounded-2xl">
      <div class="flex flex-col col-span-2 justify-between">
        <input v-model="nuevaCaracteristica.nombre" placeholder="Nombre" class="p-2 border rounded text-neutral-900">
        <input v-model="nuevaCaracteristica.descripcion" placeholder="Descripción"
          class="p-2 border rounded text-neutral-900">
        <input type="file" @change="handleFileUpload" class="p-2 border rounded text-white">
        <button 
          @click="modoEdicion ? actualizarCaracteristica() : crearCaracteristica()" 
          class="btn-third text-white px-4 py-2 rounded mt-2 hover:bg-blue-600"
        >
          {{ modoEdicion ? 'Actualizar' : 'Crear' }} Característica
        </button>
        <button 
          v-if="modoEdicion"
          @click="cancelarEdicion"
          class="bg-gray-500 text-white px-4 py-2 rounded mt-2 hover:bg-gray-600"
        >
          Cancelar
        </button>
      </div>
      <div class="col-span-1">
        <img class="w-full h-52 object-contain rounded-3xl shadow-neutral-900 shadow-lg" :src="imagePreview"
          alt="SET IMAGE" />
      </div>
    </div>
    <div>
      <h2 class="text-xl text-white font-semibold mt-6 mb-2">Listado de Características</h2>
      <ul class="mt-1 w-3/4">
        <li v-for="tipo in listaCaracteristicas" :key="tipo.caracteristicaId" class="text-white mb-2">
          <div class="flex items-center justify-between hover:bg-neutral-800 px-4 py-2 rounded-2xl">
            <div class="flex items-center">
              <img v-if="tipo.icono" :src="tipo.icono" alt="Icono" class="w-6 h-6 mr-2" />
              <span class="font-normal ml-2">{{ tipo.nombre }}</span>
            </div>
            <div class ="flex">
              <button 
                @click="editarCaracteristica(tipo)"
                class="btn-secondary rounded-xl text-sm h-10 w-10 text-white  mr-2 material-symbols-outlined"
              >
                Edit
              </button>
              <button 
                @click="eliminarCaracteristica(tipo.caracteristicaId)"
                class="btn-danger rounded-xl text-xl h-10 w-10 text-white  material-symbols-outlined"
              >
                delete
              </button>
            </div>
          </div>
        </li>
      </ul>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';
import axiosClient from '../axiosClient';

const nuevaCaracteristica = ref({ 
  nombre: '', 
  descripcion: '', 
  icono: '',
  caracteristicaId: null 
});
const imagePreview = ref('https://static.vecteezy.com/system/resources/previews/004/141/669/original/no-photo-or-blank-image-icon-loading-images-or-missing-image-mark-image-not-available-or-image-coming-soon-sign-simple-nature-silhouette-in-frame-isolated-illustration-vector.jpg');
const listaCaracteristicas = ref([]);
const modoEdicion = ref(false);
const fileInput = ref(null);

onMounted(() => {
  fetchCaracteristicas();
});

const verificaNombreEnLista = computed(() => {
  if (!listaCaracteristicas.value || !nuevaCaracteristica.value.nombre) return false;
  
  return listaCaracteristicas.value.some((i) =>
    i.nombre.toLowerCase() === nuevaCaracteristica.value.nombre.toLowerCase() &&
    i.caracteristicaId !== nuevaCaracteristica.value.caracteristicaId
  );
});

const validarNombre = () => {
  if (verificaNombreEnLista.value) {
    alert("Ya existe una característica con este nombre");
    return false;
  }
  return true;
};

const handleFileUpload = (event) => {
  const file = event.target.files[0];
  if (file) {
    nuevaCaracteristica.value.icono = file;
    imagePreview.value = URL.createObjectURL(file);
  }
};

const editarCaracteristica = (caracteristica) => {
  modoEdicion.value = true;
  nuevaCaracteristica.value = {
    caracteristicaId: caracteristica.caracteristicaId,
    nombre: caracteristica.nombre,
    descripcion: caracteristica.descripcion || '',
    icono: null // No asignamos el blob directamente
  };
  imagePreview.value = caracteristica.icono || imagePreview.value;
};

const cancelarEdicion = () => {
  modoEdicion.value = false;
  resetForm();
};

const resetForm = () => {
  nuevaCaracteristica.value = { 
    nombre: '', 
    descripcion: '', 
    icono: '',
    caracteristicaId: null 
  };
  imagePreview.value = 'https://static.vecteezy.com/system/resources/previews/004/141/669/original/no-photo-or-blank-image-icon-loading-images-or-missing-image-mark-image-not-available-or-image-coming-soon-sign-simple-nature-silhouette-in-frame-isolated-illustration-vector.jpg';
  if (fileInput.value) {
    fileInput.value.value = '';
  }
};

const crearCaracteristica = async () => {
  if (!validarNombre()) return;
  
  try {
    const formData = new FormData();
    formData.append('nombre', nuevaCaracteristica.value.nombre);
    formData.append('descripcion', nuevaCaracteristica.value.descripcion);
    if (nuevaCaracteristica.value.icono) {
      formData.append('icono', nuevaCaracteristica.value.icono);
    }

    const response = await axiosClient.post('/api/Caracteristicas/CrearCaracteristica', formData, {
      headers: { "Content-Type": "multipart/form-data" },
    });

    if (response.status >= 200 && response.status < 300) {
      alert('Característica creada');
      resetForm();
      await fetchCaracteristicas();
    } else {
      alert(response.data.message);
    }
  } catch (error) {
    alert('Error al crear característica');
    console.error(error);
  }
};

const actualizarCaracteristica = async () => {
  if (!validarNombre()) return;
  
  try {
    const formData = new FormData();
    formData.append('nombre', nuevaCaracteristica.value.nombre);
    formData.append('descripcion', nuevaCaracteristica.value.descripcion);
    if (nuevaCaracteristica.value.icono instanceof File) {
      formData.append('icono', nuevaCaracteristica.value.icono);
    }

    const response = await axiosClient.put(
      `/api/Caracteristicas/ActualizarCaracteristica/${nuevaCaracteristica.value.caracteristicaId}`,
      formData,
      {
        headers: { "Content-Type": "multipart/form-data" },
      }
    );

    if (response.status >= 200 && response.status < 300) {
      alert('Característica actualizada');
      resetForm();
      modoEdicion.value = false;
      await fetchCaracteristicas();
    } else {
      alert(response.data.message);
    }
  } catch (error) {
    alert('Error al actualizar característica');
    console.error(error);
  }
};

const eliminarCaracteristica = async (id) => {
  if (!confirm('¿Estás seguro de eliminar esta característica?')) return;
  
  try {
    const response = await axiosClient.delete(`/api/Caracteristicas/EliminarCaracteristica/${id}`);
    
    if (response.data.ok) {
      alert('Característica eliminada');
      await fetchCaracteristicas();
    } else {
      alert(response.data.message);
    }
  } catch (error) {
    alert('Error al eliminar característica');
    console.error(error);
  }
};

const fetchCaracteristicas = async () => {
  try {
    const { data } = await axiosClient.get("/api/Caracteristicas/GetCaracteristicas");

    // Liberar URLs de objetos previos si existen
    if (listaCaracteristicas.value) {
      listaCaracteristicas.value.forEach(c => {
        if (c.icono && c.icono.startsWith('blob:')) {
          URL.revokeObjectURL(c.icono);
        }
      });
    }

    const caracteristicasConIconos = await Promise.all(
      data.data.map(async (caracteristica) => {
        if (caracteristica.icono) {
          try {
            const response = await axiosClient.get(
              `/api/Caracteristicas/GetImage/${caracteristica.caracteristicaId}`,
              { responseType: 'blob' }
            );
            caracteristica.icono = URL.createObjectURL(response.data);
          } catch (error) {
            console.error(`Error al cargar el icono de ${caracteristica.nombre}:`, error);
            caracteristica.icono = null;
          }
        }
        return caracteristica;
      })
    );

    listaCaracteristicas.value = caracteristicasConIconos;
  } catch (error) {
    console.error("Error al obtener las características:", error);
    alert('Error al cargar características');
  }
};
</script>