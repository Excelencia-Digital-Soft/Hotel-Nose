<template>
  <Teleport v-if="selectedRoom" to="body" class="overflow-hidden">
  <Transition name="modal-outer" appear>
    <div 
      class="fixed w-full h-full overflow-auto z-20 bg-black bg-opacity-40 backdrop-blur-lg top-0 left-0 flex justify-center px-8">
          <Transition name="modal-inner">
      <!-- Panel de la habitación seleccionada con Slider -->

      <div  class="flex flex-col justify-center items-center mb-24 p-4  rounded-lg w-10/12 h-auto text-center">
        <!-- <h3 class="text-3xl font-semibold text-white ">{{ selectedRoom.nombre }}</h3> -->
        

        <!-- Carrusel de imágenes -->
       <h3 class="text-3xl font-semibold text-white ">{{ selectedRoom.nombreHabitacion }}</h3>
        <Swiper :modules="[Navigation, Pagination]" navigation pagination class="mt-4 w-full h-full">
          <SwiperSlide v-for="(img, index) in selectedRoom.imagenes" :key="index">
            <img :src="img" class="w-full h-full object-cover rounded-lg" />
          </SwiperSlide>
        </Swiper>
        <ul class="mt-2 list-disc pl-4 text-gray-300 text-left">
          <li v-for="(detalle, index) in selectedRoom.caracteristicas" :key="index">
            {{ detalle.nombre }}
          </li>
        </ul>
        <button class="btn-secondary font-semibold rounded-lg text-2xl p-8">Alquilar ${{ selectedRoom.precio }}</button>
        <button type="button"
                class="btn-danger absolute top-6 right-4 z-10 md:right-16 text-md w-20 h-20 rounded-3xl transition-colors border-2 border-purple-200"
                @click="emits('close')">X</button>
      </div>
    </Transition>
    </div>
  </Transition>
  </Teleport>
</template>

<script setup>
import { Swiper, SwiperSlide } from 'swiper/vue';
import { Navigation, Pagination } from 'swiper/modules';
import 'swiper/css';
import 'swiper/css/navigation';
import 'swiper/css/pagination';
const props = defineProps({
  selectedRoom: Object,
})
const emits = defineEmits(["close"]);
</script>