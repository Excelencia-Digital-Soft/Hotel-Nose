<template>
  <div class="m-8 w-1/2 ">
    <h2 class="text-xl text-white font-semibold mt-6 mb-2">Manejador de Características</h2>
  <div class="grid grid-cols-3 gap-2">

    <div class="flex flex-col  col-span-2 justify-between">
      <input v-model="nuevaCaracteristica.nombre" placeholder="Nombre" class="p-2 border rounded  text-neutral-900">
      <input v-model="nuevaCaracteristica.descripcion" placeholder="Descripción"
        class="p-2 border rounded text-neutral-900">
      <input type="file" @change="handleFileUpload($event, 'ImageFile')" class="p-2 border rounded text-white">
      <button @click="validarNombre" class="bg-blue-500 text-white px-4 py-2 rounded mt-2 hover:bg-blue-600">
        Crear Característica
      </button>
    </div>
    <div class="col-span-1">
      <img class="w-full h-52 object-contain rounded-3xl shadow-neutral-900 shadow-lg " :src="imagePreview"
        alt="SET IMAGE" />
    </div>
  </div>
  <div>
    <ul as="template" v-for="tipo in listaCaracteristicas" :key="tipo.caracteristicaId" :value="tipo">
      <li
        class=" text-white">
        <div class="flex items-center">
          <!-- Mostrar el icono si está disponible -->
          <img v-if="tipo.icono" :src="tipo.icono" alt="Icono" class="w-6 h-6 mr-2" />
          <span class="font-normal ml-2 ">{{ tipo.nombre }}</span>
        </div>
      </li>
    </ul>
  </div>
</div>
</template>
<script setup>
import { ref, onMounted, computed } from 'vue';
import axiosClient from '../axiosClient';

const nuevaCaracteristica = ref({ nombre: '', descripcion: '', icono: '' });
const imagePreview = ref('https://static.vecteezy.com/system/resources/previews/004/141/669/original/no-photo-or-blank-image-icon-loading-images-or-missing-image-mark-image-not-available-or-image-coming-soon-sign-simple-nature-silhouette-in-frame-isolated-illustration-vector.jpg');
const listaCaracteristicas = ref([])
// Fetch rooms and categories on component mount
onMounted(() => {
  fetchCaracteristicas()
});

const verificaNombreEnLista = computed(() => {
  if (!listaCaracteristicas.value || !nuevaCaracteristica.value.nombre) return false;
  
  return listaCaracteristicas.value.some((i) =>
    i.nombre.toLowerCase() === nuevaCaracteristica.value.nombre.toLowerCase()
  );
});
const existeElNombre = () => { 
  if (verificaNombreEnLista.value){//comprobamos que no exista el nombre del nuevo gasto
  return true
  } 
  else {
  return false
  }
}
const validarNombre = () => {
  //VALIDACION
  if (existeElNombre())  {
  //No envíes el formulario si hay errores de validación
  console.log("Ya existe este nombre buscate otro")
  return;
  }
  crearCaracteristica()
}
const handleFileUpload = (event) => {
  const file = event.target.files[0];
  if (file) {
    nuevaCaracteristica.value.icono = file;
    imagePreview.value = URL.createObjectURL(file);
  }
};
const crearCaracteristica = async () => {
  try {

    console.log(nuevaCaracteristica)
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
      nuevaCaracteristica.value = { nombre: '', descripcion: '', icono: null };
      fetchCaracteristicas(); // Mandar caracteristica creada al DDcreateSearch
    } else {
      alert(response.data.message); // Acceder a response.data
    }
  } catch (error) {
    alert('Error al crear característica');
  }
};

const fetchCaracteristicas = async () => {
  try {
    // Obtener las características
    const { data } = await axiosClient.get("/api/Caracteristicas/GetCaracteristicas");

    // Convertir las rutas de los iconos a ObjectURL
    const caracteristicasConIconos = await Promise.all(
      data.data.map(async (caracteristica) => {
        if (caracteristica.icono) {
          try {
            // Obtener la imagen como Blob y convertirla a ObjectURL
            const response = await axiosClient.get(`/api/Caracteristicas/GetImage/${caracteristica.caracteristicaId}`, {
              responseType: 'blob',
            });
            caracteristica.icono = URL.createObjectURL(response.data);
          } catch (error) {
            console.error(`Error al cargar el icono de ${caracteristica.nombre}:`, error);
            caracteristica.icono = null; // Si hay un error, establecer el icono como null
          }
        }
        return caracteristica;
      })
    );

    // Asignar las características con los iconos convertidos
    listaCaracteristicas.value = caracteristicasConIconos;

  } catch (error) {
    console.error("Error al obtener las características:", error);
  }
};
</script>