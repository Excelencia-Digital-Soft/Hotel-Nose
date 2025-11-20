<template>
  <div class="min-h-screen bg-black text-white flex flex-col items-center p-4">
    <h1 class="text-3xl font-bold text-center">HOTEL</h1>
    <h2 class="text-4xl font-bold text-transparent bg-clip-text bg-gradient-to-r from-purple-500 to-pink-500">
      NOSE
    </h2>

    <!-- Selección de Habitaciones -->
    <h3 class="mt-6 text-lg">Seleccionar Categoría:</h3>
    <div class="w-full px-8 mt-4 space-y-3">
      <button v-for="categoria in categorias" :key="categoria.categoriaId"
        @click="seleccionarCategoria(categoria.habitaciones)"
        class="w-full sm:h-32 h-14 flex justify-between items-center space-x-2 pl-4 bg-gradient-to-r from-purple-500 to-blue-500 rounded-lg hover:scale-105 transition">
        <span class="text-xl font-semibold">{{ categoria.nombreCategoria }}</span>
        <span class="text-xl text-green-400">${{ categoria.precioNormal }}</span>
        <img
                    :src="categoria.habitaciones[0].imagenes.length > 0 ? categoria.habitaciones[0].imagenes[0] : '../assets/image59.svg'"
                    alt="Imagen de la habitación"
                    class="w-full h-full object-cover rounded-lg "
                  />
      </button>
    </div>
    <ModalSelectCategory v-if="modalSCategory" :selectedCategory="categoriaSeleccionada" @close="toggleModalSelectCat" />


  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';
import ModalSelectCategory from '../components/ModalSelectCategory.vue';
import { Swiper, SwiperSlide } from 'swiper/vue';
import { Navigation, Pagination } from 'swiper/modules';
import { fetchImagenes } from '../services/imageService';
import { useAuthStore } from '../store/auth.js';
import axiosClient from '../axiosClient';
import 'swiper/css';
import 'swiper/css/navigation';
import 'swiper/css/pagination';

const authStore = useAuthStore();
let modalSCategory = ref(false);
let categorias = ref([])



// const habitaciones = ref([
//   {
//     id: 1,
//     nombre: "Pent House",
//     imagenes: [
//       "https://th.bing.com/th/id/OIP.hnwKytWWjo6iyWQ93HLIMgAAAA?w=474&h=266&rs=1&pid=ImgDetMain",
//       "https://th.bing.com/th/id/OIP.JdcE4MoP2CA0yh8Q5XdxJAHaE8?rs=1&pid=ImgDetMain",
//       "https://th.bing.com/th/id/OIP.Eisq4HshoGdlUVw17bK2cwAAAA?rs=1&pid=ImgDetMain"
//     ],
//     descripcion: ["Jacuzzi privado", "Vista panorámica", "Cama King Size", "Servicio 24h"]
//   },
//   {
//     id: 2,
//     nombre: "Hidro Suite",
//     imagenes: [
//       "https://th.bing.com/th/id/OIP.b_1recShkcZK_xhQ5qFPdgHaF6?rs=1&pid=ImgDetMain",
//       "https://th.bing.com/th/id/OIP.00258vC2Pwd8-vShg1GpqQHaEo?rs=1&pid=ImgDetMain",
//       "https://th.bing.com/th/id/OIP.LU4zvSP8btTR0qJ4iLkLBQHaFj?w=1000&h=751&rs=1&pid=ImgDetMain"
//     ],
//     descripcion: ["Hidromasaje", "Iluminación LED", "Cama Queen Size", "Mini Bar"]
//   },
//   {
//     id: 3,
//     nombre: "Master Suite",
//     imagenes: [
//       "https://th.bing.com/th/id/OIP.00258vC2Pwd8-vShg1GpqQHaEo?rs=1&pid=ImgDetMain",
//       "https://th.bing.com/th/id/OIP.b_1recShkcZK_xhQ5qFPdgHaF6?rs=1&pid=ImgDetMain",
//       "https://th.bing.com/th/id/OIP.LU4zvSP8btTR0qJ4iLkLBQHaFj?w=1000&h=751&rs=1&pid=ImgDetMain"
//     ],
//     descripcion: ["Decoración temática", "Baño de lujo", "Sonido envolvente", "Balcón privado"]
//   },
//   {
//     id: 4,
//     nombre: "Suite",
//     imagenes: [
//       "https://th.bing.com/th/id/OIP.LU4zvSP8btTR0qJ4iLkLBQHaFj?w=1000&h=751&rs=1&pid=ImgDetMain",
//       "https://th.bing.com/th/id/OIP.00258vC2Pwd8-vShg1GpqQHaEo?rs=1&pid=ImgDetMain",
//       "https://th.bing.com/th/id/OIP.b_1recShkcZK_xhQ5qFPdgHaF6?rs=1&pid=ImgDetMain"
//     ],
//     descripcion: ["Ambiente acogedor", "Ducha de lluvia", "TV 55”", "Wi-Fi rápido"]
//   }
// ]);

const fetchHabitaciones = async () => {
  const institucionID = authStore.institucionID;

  if (!institucionID) {
    console.warn('InstitucionID is not available. Please ensure the user is logged in.');
    return;
  }

  try {
    const response = await axiosClient.get(`GetHabLibresXCat?InstitucionID=${institucionID}`);
if (response.data && response.data.data) {
  categorias.value = await Promise.all(
    response.data.data.map(async (categoria) => ({
      ...categoria,
      habitaciones: await Promise.all( // Cambia "categorias" por "habitaciones"
        categoria.habitaciones.map(async (habitacion) => {
          // Verifica si hay imágenes y obtén las URLs
          const imagenIds = habitacion.imagenes || [];
          console.log("IDs de imágenes:", imagenIds); // Depuración
          const imagenes = imagenIds.length > 0 ? await fetchImagenes(imagenIds) : [];

          return {
            ...habitacion,
            imagenes, // Asigna las URLs de las imágenes
          };
        })
      ),
    }))
  );

      console.log(categorias.value);
    } else {
      console.error('Datos de la API no válidos:', response.data);
    }
  } catch (error) {
    console.error('Error al obtener las categorias:', error);
  }
};

onMounted(fetchHabitaciones)
const categoriaSeleccionada = ref(null);
const toggleModalSelectCat = () => {
  modalSCategory.value = !modalSCategory.value
}
const seleccionarCategoria = (categoria) => {
  console.log("SELECCIONADA:",categoria)
  categoriaSeleccionada.value = categoria;
  modalSCategory.value = true
};
</script>

<style scoped>
button {
  transition: all 0.2s ease-in-out;
}

button:hover {
  transform: scale(1.05);
}
</style>
