<template>
    <Teleport to="body">
      <Transition name="modal-outer" appear>
        <div class="fixed w-full h-full bg-black bg-opacity-80 backdrop-blur-lg top-0 left-0 flex justify-center items-center px-8">
          <Transition name="modal-inner">
            <div class="w-full md:w-2/4 h-auto flex flex-col justify-center fixed mt-4 p-8 pt-6 border-x-8 border-secondary-400 rounded-xl bg-neutral-900">
              <form class="w-full grid-cols-1">
                <section class="grid  place-items-center mb-3">
                  <label class="text-sm font-semibold text-white">Tiempo a agregar</label>
                  <div class="flex flex-col  space-y-4 w-full border-y-2 border-accent-500 rounded-xl p-4 ">
                    <div class="grid ">
                      <label class="text-xs font-semibold text-white">Horas</label>
                      <InputNumber v-model="hours" :min="0" :max="99" showButtons />
                    </div>
                    <div class="grid">
                      <label class="text-xs font-semibold text-white">Minutos</label>
                      <InputNumber v-model="minutes" :min="0" :max="59" showButtons />
                    </div>
                  </div>
                </section>
                <div class="col-span-3 flex justify-center items-center w-full">
                  <button @click="confirmExtension" type="button" class="btn-primary w-full md:w-2/4 h-16 rounded-2xl">Agregar tiempo</button>
                </div>
              </form>
              <button class="absolute text-xl w-12 h-12 lg:w-14 lg:h-14 text-white top-0 -right-2 lg:top-6 lg:right-6 btn-danger rounded-xl lg:rounded-full"
                      @click="$emit('close-modal')">X</button>
            </div>
          </Transition>
        </div>
      </Transition>
    </Teleport>
  </template>
  
  
  
    
    <script setup>
    import { ref } from 'vue';
    import axiosClient from '../axiosClient'; // Import AxiosClient or adjust path as needed
    import InputNumber from 'primevue/inputnumber';
    const hours = ref(0);
    const minutes = ref(0);
    const emit = defineEmits(['confirmExtension', 'close-modal'])

    const props = defineProps({
      reservaID: Number,
    });
    
    const motivo = ref('');
    console.log(props.reservaID);
    // Confirm Button Action
    const confirmExtension = () => {
      axiosClient.put(`/ExtenderReserva?idReserva=${props.reservaID}&horas=${hours.value}&minutos=${minutes.value}`)
        .then(res => {
          console.log(res.data);
          alert("Se extendió la reserva exitosamente");
          emit('confirmExtension', hours.value, minutes.value);
        })
        .catch(error => {
          console.error(error);
          emit('close-modal');
        });
    };
  
  
    </script>
    
    <style scoped>
    /* Add any additional styles for the modal */
    </style>
    