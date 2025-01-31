<template>
  <Teleport to="body">
    <Transition name="modal-outer" appear>
      <div
        class="fixed w-full h-full  bg-black bg-opacity-80 backdrop-blur-lg top-0 left-0 flex justify-center items-center px-8">
        <Transition name="modal-inner">
          <div
            :class="selectedRoom.Disponible ? 'w-2/4 h-3/6 flex flex-col justify-center fixed mt-4 p-8 pt-6 border-x-8  border-secondary-400 rounded-xl bg-neutral-900' : ' w-3/4 h-[90%] flex flex-col justify-center fixed mt-4 p-8 pt-6 border-x-8  border-secondary-400 rounded-xl bg-neutral-900'">
            <i class="fa-thin fa-circle-xmark"></i>
            <!-- Modal Content -->
            <button
              class="absolute top-2 right-2 text-xl  w-14 h-14 text-white items-end btn-danger rounded-full transition duration-150 ease-out md:ease-in"
              @click="$emit('close-modal')">X</button>
            <h1
              class="absolute self-center text-2xl w-1/3 text-center lexend-exa font-bold mt-5 mb-5 bg-gradient-to-l from-accent-200 via-secondary-500 to-primary-300 bg bg-clip-text text-transparent">
              {{ selectedRoom.nombreHabitacion }}
            </h1>

            <form :class="!selectedRoom.Disponible ? 'grid grid-cols-3 gap-3 mb-2' : 'grid-cols-1'">
              <section v-if="!selectedRoom.Disponible" class="flex flex-col justify-start p-2">
                <div>
                  <h1 class="text-xl text-white font-bold mb-2">Consumos</h1>
                  <div class="bg-gray-800 rounded-lg p-4 h-56 overflow-y-auto">
                    <!-- Header row -->
                    <div class="grid grid-cols-[1fr_1fr_1fr_1fr_1fr_1fr_auto] gap-3  text-white text-xs font-semibold mb-2">
                      <span>Producto</span>
                      <span>Cant</span>
                      <span>Precio</span>
                      <span>Orig</span>
                      <span>Total</span>
                      <span>Borrar</span>
                    </div>

                    <!-- Ordered list for the consumos -->
                    <ul class="space-y-2">
                      <li v-for="consumo in consumos" :key="consumo.consumoId" class="grid grid-cols-6  bg-gray-700 p-2 rounded-md text-white items-center">
                        <span class="text-xs w-16 break-words font-semibold">{{ consumo.articleName }}</span>
                        <span class="text-xs text-center text-gray-400">{{ consumo.cantidad }}</span>
                        <span class="text-xs text-center text-gray-400">${{ consumo.precioUnitario }}</span>
                        <span class="text-xs text-center text-gray-400">
                          {{ consumo.esHabitacion ? 'Hab' : 'Inv' }}
                        </span>
                        <span class="text-xs font-bold text-green-400">${{ consumo.total }}</span>

                        <!-- Cancel button with trashcan icon -->
                        <button type="button"
                          class="btn-danger rounded-xl text-xl h-10 w-10 text-white flex justify-center ml-2 items-center material-symbols-outlined"
                          @click="anularConsumo(consumo.consumoId)">
                          delete
                        </button>
                      </li>
                    </ul>
                  </div>
                </div>

                <div class="flex">
                  <button type="button" @click="toggleModalConfirm()"
                    class="btn-primary w-full h-8 mr-2 p-1 text-sm  rounded-3xl mt-4">
                    Consumo general
                  </button>
                  <button type="button" @click="toggleModalConfirmHabitacion()"
                    class="btn-primary w-full h-8 text-sm p-2   rounded-3xl mt-4">
                    Consumo habitación
                  </button>
                </div>


              </section>

              <section class="timer-container flex flex-col items-start space-y-4">
                <div class="timer">
                  <p class="label">Tiempo restante:</p>
                  <p class="time">{{ formattedTime }}</p>
                </div>
                <button @click="ignorarTiempoExtra()" type="button" :class="[
                  'w-full',
                  'mt-4',
                  'rounded-lg',
                  'text-white',
                  {
                    'bg-gray-600': ignorarTiempo,  // Darken background when pressed
                    'transform translate-y-1': ignorarTiempo,  // Add pressed effect
                    'box-shadow inset 0 2px 5px rgba(0, 0, 0, 0.2)': ignorarTiempo  // Inner shadow effect
                  }
                ]" :style="{
                  border: '4px solid transparent',
                  borderImage: 'linear-gradient(to right, #667eea, #764ba2, #ff7e5f) 1',
                  borderRadius: '9999px', // Ensure rounded corners
                }">
                  Ignorar tiempo extra
                </button>
              </section>
              <section class="flex flex-col items-center justify-center  mb-2">
                <div class="flex flex-col items-start justify-center w-full mb-3 px-4">
                  <label for="nombre" class="text-sm font-semibold leading-6 text-white">Identificador</label>
                  <input type="text"
                    class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition duration-150 ease-out md:ease-in"
                    v-model="selectedRoom.Identificador" placeholder="Identificador" maxlength="40">
                </div>
                <div class="flex flex-col items-start justify-center w-full mb-3 px-4">
                  <label for="cuit" class="text-sm font-semibold leading-6 text-white">Telefono</label>
                  <input type="text"
                    class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition duration-150 ease-out md:ease-in"
                    maxlength="11" v-model="selectedRoom.NumeroTelefono"
                    placeholder="Ingresa Marca y modelo de vehiculo">
                </div>
                <div class="flex flex-col items-start justify-center w-full mb-3 px-4">
                  <label for="cuit" class="text-sm font-semibold leading-6 text-white">Patente</label>
                  <input type="text"
                    class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition duration-150 ease-out md:ease-in"
                    v-model="selectedRoom.PatenteVehiculo" placeholder="Ingrese el numero de Patente">
                </div>
              </section>
              <section v-if="!selectedRoom.Disponible">
                <div class="max-w-sm mx-auto border border-gray-800 bg-gray-800 text-white">
                  <table class="w-full text-left">
                    <tbody>
                      <tr class="border-b border-gray-700">
                        <td class="p-4">Consumision</td>
                        <td class="p-4 text-right">${{ consumos.reduce((sum, consumo) => sum + consumo.total, 0) }}</td>
                      </tr>
                      <tr class="border-b border-gray-700">
                        <td class="p-4">Periodo</td>
                        <td class="p-4 text-right">${{ periodoCost }}</td>
                      </tr>
                      <tr class="border-b border-gray-700">
                        <td class="p-4">Adicional</td>
                        <td class="p-4 text-right">${{ adicional }}</td> <!-- Display calculated Adicional -->
                      </tr>
                      <tr>
                        <td class="p-4">Total</td>
                        <td class="p-4 text-right">
                          ${{ (Number(consumos.reduce((sum, consumo) => sum + consumo.total, 0)) + Number(periodoCost) +
                            Number(adicional)).toFixed(2) }}
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </section>
              <section></section>
              <section v-if="!selectedRoom.Disponible">
                <div class="card flex flex-col ml-4 flex-wrap justify-center gap-4">
                  <div class="mt-4 w-full">
                    <label class="text-xs font-semibold text-white">Seleccionar Promoción</label>
                    <select v-model="selectedPromocion" class="w-full p-2 mt-2 rounded-lg">
                      <!-- Default 'Sin Promoción' option -->
                      <option :value="null">Sin Promoción</option>

                      <!-- Display promotions if available -->
                      <option v-for="promo in promociones" :key="promo.promocionID" :value="promo">
                        {{ promo.detalle }}
                      </option>

                      <!-- Show this message if there are no promociones -->
                      <option v-if="promociones.length === 0" disabled>Sin promociones disponibles</option>
                    </select>
                  </div>
                </div>
              </section>
              <div class="col-span-3 flex justify-center items-center w-full space-x-4">
                <button @click="toggleAnularOcupacionModal" type="button"
                  class="btn-danger w-1/6 h-16 rounded-2xl border-l-2 border-gray-300">
                  Anular Ocupación
                </button>

                <button @click="openPaymentModal" type="button" :disabled="selectedRoom.pedidosPendientes"
                  class="btn-primary w-2/4 h-16 rounded-2xl">
                  Desocupar Habitación
                </button>

                <!-- Conditional warning text -->
                <p v-if="selectedRoom.pedidosPendientes" class="text-red-500 mt-2 text-center">
                  Hay pedidos pendientes, no se puede desocupar la habitación.
                </p>

                <ModalPagar v-if="modalPayment" :total="totalAmount" :adicional="Number(adicional)"
                  :habitacionId="selectedRoom.HabitacionID" :visitaId="selectedRoom.VisitaID" :pausa="Pausa"
                  @close="modalPayment = false" @confirm-payment="handlePaymentConfirmation" />

                <AnularOcupacionModal v-if="modalAnular" :reservaId="selectedRoom.ReservaID"
                  @close-modal="modalAnular = false" />
              </div>

            </form>

            <ModalConfirm v-if="modalConfirm" :name="selectedRoom.Identificador" @confirmaAccion="confirmAndSend"
              @close="toggleModalConfirm" />

            <ModalConfirmHabitacion v-if="modalConfirmHabitacion" :name="selectedRoom.Identificador"
              :habitacionID="selectedRoom.HabitacionID" @confirmaAccion="confirmAndSendHabitacion"
              @close="toggleModalConfirmHabitacion" />
          </div>

        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>

import { computed, toRaw } from 'vue';
import { onMounted, ref, watch, onUnmounted } from 'vue';
import axiosClient from '../axiosClient';
import InputNumber from 'primevue/inputnumber';
import Checkbox from 'primevue/checkbox';
import ModalConfirm from './ModalConfirm.vue';
import ModalConfirmHabitacion from './ModalConfirmHabitacion.vue';
import AnularOcupacionModal from './AnularOcupacionModal.vue';

import dayjs from 'dayjs';

const emits = defineEmits(['close-modal', 'update-room']);
const props = defineProps({
  room: Object,
});

const periodoCost = computed(() => {
  const totalHours = selectedRoom.value.TotalHoras || 0;
  const totalMinutes = selectedRoom.value.TotalMinutos || 0;
  const hourlyRate = promocionActiva.value && selectedPromocion.value ? selectedPromocion.value.tarifa : selectedRoom.value.Precio;
  const totalPeriod = totalHours + totalMinutes / 60;
  return (totalPeriod * hourlyRate).toFixed(2);
});

const adicional = computed(() => {
  console.log(overtime.value)
  return (overtime.value * ((promocionActiva.value && selectedPromocion.value ? selectedPromocion.value.tarifa : selectedRoom.value.Precio) / 60)).toFixed(2); // Calculates overtime charge
});

onMounted(() => {
  console.log(props.room);
  selectedRoom.value.nombreHabitacion = props.room.nombreHabitacion;
  selectedRoom.value.HabitacionID = props.room.habitacionId;
  selectedRoom.value.Disponible = props.room.disponible;
  selectedRoom.value.TotalHoras = props.room.reservaActiva.totalHoras;
  selectedRoom.value.TotalMinutos = props.room.reservaActiva.totalMinutos;
  selectedRoom.value.FechaReserva = props.room.reservaActiva.fechaReserva;
  selectedRoom.value.Precio = props.room.precio;
  selectedRoom.value.PromocionID = props.room.visita.reservaActiva.promocionId;
  selectedRoom.value.pedidosPendientes = props.room.pedidosPendientes,
    selectedRoom.value.ReservaID = props.room.visita.reservaActiva.reservaId;
  selectedRoom.value.VisitaID = props.room.visitaID; // Safe access
  selectedRoom.value.Identificador = props.room.visita?.identificador; // Safe access
  selectedRoom.value.NumeroTelefono = props.room.visita?.numeroTelefono; // Safe access
  selectedRoom.value.PatenteVehiculo = props.room.visita?.patenteVehiculo; // Safe access
  selectedRoom.value.PausaHoras = props.room.reservaActiva.pausaHoras ?? 0;
  selectedRoom.value.PausaMinutos = props.room.reservaActiva.pausaMinutos ?? 0;

  console.log(selectedRoom.value)
  actualizarConsumos();
  document.body.style.overflow = 'hidden';
})

let selectedRoom = ref({
  HabitacionID: 0,
  Disponible: null,
  nombreHabitacion: '',
  FechaReserva: '',
  FechaFin: '',
  PromocionID: 0,
  ReservaID: 0,
  pedidosPendientes: false,
  TotalHoras: 0,
  TotalMinutos: 0,
  UsuarioID: 14,
  PatenteVehiculo: '',
  NumeroTelefono: '',
  Identificador: '',
  esReserva: true,
})







// Objeto con los valores de la tabla
const tableData = ref({
  descuento: 0,
  tarjeta: 0,
  recargos: 0,
  empenos: '',
  total: 0,
});

let modalConfirm = ref(false);
let modalConfirmHabitacion = ref(false);


const pizza = ref();
const ignorarTiempo = ref(false)
const products = ref();
const overtime = ref(0);
const hours = ref(0);
const minutes = ref(0);
const selectedTags = ref([]);
const consumos = ref([]);
const Pausa = ref(false)
let editTagRel = {}
let cheatRefresh = ref(false);
let idNewTag = ref(0);
let numeroError = ref('');

const agregarConsumos = (selectedItems) => {
  console.log(selectedItems);
  axiosClient.post(
    `/ConsumoGeneral?habitacionId=${selectedRoom.value.HabitacionID}&visitaId=${selectedRoom.value.VisitaID}`,
    selectedItems // Send selectedItems directly as the body
  )
    .then(response => {
      actualizarConsumos();
      console.log('Consumo agregado exitosamente:', response.data);
    })
    .catch(error => {
      console.error('Error al agregar consumo:', error);
    });
};

const anularConsumo = (consumoId) => {
  axiosClient.delete(
    `/AnularConsumo?idConsumo=${consumoId}`,
  )
    .then(response => {
      actualizarConsumos();
      console.log('Consumo anulado exitosamente');
    })
    .catch(error => {
      console.error('Error al agregar consumo:', error);
    });
};
const agregarConsumosHabitacion = (selectedItems) => {
  console.log(selectedItems);
  axiosClient.post(
    `/ConsumoHabitacion?habitacionId=${selectedRoom.value.HabitacionID}&visitaId=${selectedRoom.value.VisitaID}`,
    selectedItems // Send selectedItems directly as the body
  )
    .then(response => {
      actualizarConsumos();
      console.log('Consumo agregado exitosamente:', response.data);
    })
    .catch(error => {
      console.error('Error al agregar consumo:', error);
    });
};

const actualizarConsumos = () => {
  axiosClient.get(`/GetConsumosVisita?VisitaID=${selectedRoom.value.VisitaID}`)
    .then(({ data }) => {
      if (data && data.data) {
        consumos.value = []; // Clear the array before adding new data
        data.data.forEach(item => {
          // Check if the item already exists in the consumos list
          const existingItem = consumos.value.find(consumo => consumo.articuloId === item.articuloId && consumo.esHabitacion === item.esHabitacion);

          if (existingItem) {
            // Update the existing item’s quantity and recalculate subtotal if it exists
            existingItem.cantidad += item.cantidad;
            existingItem.total = existingItem.cantidad * existingItem.precioUnitario; // Recalculate subtotal
          } else {
            // Add new item if it doesn't already exist
            consumos.value.push({
              consumoId: item.consumoId,
              articuloId: item.articuloId,
              articleName: item.articleName,
              cantidad: item.cantidad,
              precioUnitario: item.precioUnitario,
              esHabitacion: item.esHabitacion,
              total: item.cantidad * item.precioUnitario // Initial subtotal
            });
          }
        });
      } else {
        console.error('Datos de la API no válidos:', data);
      }
    })
    .catch(error => {
      console.error('Error al obtener los consumos:', error);
    });
};
const toggleModalConfirm = () => {
  modalConfirm.value = !modalConfirm.value;
}

const toggleAnularOcupacionModal = () => {
  modalAnular.value = !modalAnular.value;
}
const toggleModalConfirmHabitacion = () => {
  modalConfirmHabitacion.value = !modalConfirmHabitacion.value;
}
const confirmAndSend = (ConfirmedArticles) => {

  console.log(JSON.stringify(ConfirmedArticles) + " Llegamos al ReserveROOM");
  agregarConsumos(ConfirmedArticles)
}

const confirmAndSendHabitacion = (ConfirmedArticles) => {

  console.log(JSON.stringify(ConfirmedArticles) + " Llegamos al ReserveROOM");
  console.log(ConfirmedArticles)
  if (ConfirmedArticles.length > 0) {
    agregarConsumosHabitacion(ConfirmedArticles)
  }
}
const handleCheat = (cheatIds) => {
  //le avisamos al componente DropDownTag que actualice para agregar los nuevos datos
  cheatRefresh.value = true
  idNewTag.value = cheatIds
  console.log("1 el numero", cheatIds)

};


//SECTOR DE VALIDACIONES DE FORMULARIO


// Función para calcular el total automáticamente
watch([() => tableData.value.descuento, () => tableData.value.tarjeta, () => tableData.value.recargos], () => {
  tableData.value.total = tableData.value.descuento + tableData.value.tarjeta + tableData.value.recargos;
});


const validarNumero = (num) => {
  const numero = num;
  if (!numero) {
    numeroError.value = 'Este Campo es obligatorio'
  } else if (!/^\d+$/.test(numero)) {
    numeroError.value = 'Solo se permiten números';
  } else {
    numeroError.value = '';
  }
}

const actualizarFechas = () => {
  const now = new Date();

  // Convertir la fecha y hora actual a la zona horaria local
  const localFechaReserva = new Date(now.getTime() - now.getTimezoneOffset() * 60000).toISOString().slice(0, -1);

  selectedRoom.value.FechaReserva = localFechaReserva;
  // Sumar una hora más
  const fechaConUnaHoraMas = new Date(now);
  fechaConUnaHoraMas.setHours(fechaConUnaHoraMas.getHours() + 1);

  // Convertir la nueva fecha con una hora adicional a la zona horaria local
  const localFechaReservaConUnaHoraMas = new Date(fechaConUnaHoraMas.getTime() - fechaConUnaHoraMas.getTimezoneOffset() * 60000).toISOString().slice(0, -1);

  // Aquí puedes asignar la nueva fecha si es necesario
  selectedRoom.value.FechaFin = localFechaReservaConUnaHoraMas;
};


//Reservar Habitacion
const endRoomReserve = () => {
  axiosClient.put(`/FinalizarReserva?idHabitacion=${selectedRoom.value.HabitacionID}`)
    .then(res => {
      console.log(res.data);
      alert("Se terminó la reserva exitosamente");
      emits('close-modal');
      window.location.reload();
    })
    .catch(error => {
      console.error(error);
    });
}

// LOGICA TIMER
const formattedTime = ref('');
let timerInterval = null;

// Timer calculation logic, kept separate
function calculateRemainingTime() {
  if (!modalPayment.value) {
    if (selectedRoom.value.PausaHoras == 0 && selectedRoom.value.PausaMinutos == 0) {
      const endTime = dayjs(selectedRoom.value.FechaReserva)
        .add(selectedRoom.value.TotalHoras, 'hour')
        .add(selectedRoom.value.TotalMinutos, 'minute');

      const now = dayjs();
      const diffInMinutes = endTime.diff(now, 'minute');
      const isOvertime = diffInMinutes < 0;
      if (isOvertime && ignorarTiempo.value == false) {
        if (diffInMinutes >= -12 * 60) {
          overtime.value = diffInMinutes * (-1);
          console.log(overtime.value);
        }
        if (diffInMinutes < -12 * 60) {
          overtime.value = 720;
          clearInterval(timerInterval);
          formattedTime.value = `-12:00`;
          return;
        }
        const hours = Math.floor(Math.abs(diffInMinutes) / 60);
        const minutes = Math.abs(diffInMinutes) % 60;
        if (isOvertime) formattedTime.value = `-${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}`;
        else formattedTime.value = `${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}`;


      } else if (isOvertime) {
        formattedTime.value = `00:00`
        overtime.value = 0
      }
      else {
        const hours = Math.floor(Math.abs(diffInMinutes) / 60);
        const minutes = Math.abs(diffInMinutes) % 60;
        formattedTime.value = `${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}`;
      }

    }
    else {
      const absolutePausaHoras = Math.abs(selectedRoom.value.PausaHoras);
      const absolutePausaMinutos = Math.abs(selectedRoom.value.PausaMinutos);

      if (selectedRoom.value.PausaHoras < 0 || selectedRoom.value.PausaMinutos < 0) {
        Pausa.value = true;
        if (ignorarTiempo.value == false) {
          overtime.value = absolutePausaHoras * 60 + absolutePausaMinutos;
          formattedTime.value = `-${String(absolutePausaHoras).padStart(2, '0')}:${String(absolutePausaMinutos).padStart(2, '0')}`
        }
        else {
          formattedTime.value = `00:00`
          overtime.value = 0
        }
      }
      else if (selectedRoom.value.PausaHoras > 0 || selectedRoom.value.PausaMinutos > 0) {
        Pausa.value = true;
        formattedTime.value = `${String(absolutePausaHoras).padStart(2, '0')}:${String(absolutePausaMinutos).padStart(2, '0')}`

      }
    }
  }
}

function ignorarTiempoExtra() {
  ignorarTiempo.value = !ignorarTiempo.value;
  calculateRemainingTime()
}
// Watch for changes in selectedRoom
watch(() => selectedRoom.value, (newValue) => {
  if (newValue.FechaReserva && newValue.TotalHoras !== undefined && newValue.TotalMinutos !== undefined) {
    // Start the timer only when selectedRoom data is available
    if (timerInterval) clearInterval(timerInterval); // Clear any previous interval
    timerInterval = setInterval(calculateRemainingTime, 1000); // Update every second
    calculateRemainingTime(); // Initial call to avoid waiting for the first interval
  }
}, { deep: true });

// Clean up the interval when the component is unmounted
onUnmounted(() => {
  if (timerInterval) clearInterval(timerInterval);
});


// Logica botón para pagar
import ModalPagar from './ModalPagar.vue';

// States
const modalPayment = ref(false);
const totalAmount = ref(null);
const modalAnular = ref(false);
// Props from your existing data, for example:

// Methods
const openPaymentModal = () => {
  console.log("Se abrió");
  if (!modalPayment.value) {
    // Take snapshots of the current values
    const consumosSnapshot = consumos.value.map(consumo => ({ ...consumo })); // Shallow copy
    const periodoCostSnapshot = Number(periodoCost.value);

    totalAmount.value = consumosSnapshot.reduce((sum, consumo) => sum + consumo.total, 0) + periodoCostSnapshot;

    console.log("Total Amount:", totalAmount.value);
    modalPayment.value = true; // Open the modal
  }
};
const handlePaymentConfirmation = (paymentDetails) => {
  console.log('Payment Confirmed:', paymentDetails);
  modalPayment.value = false;
};

// LOGICA PROMOCIONES
const selectedPromocion = ref(null);
const promociones = ref([]);
const promocionActiva = ref(false);

onMounted(async () => {
  try {
    const response = await axiosClient.get(`/api/Promociones/GetPromocionesCategoria?categoriaID=${props.room.categoriaId}`);
    promociones.value = response.data.data || [];
    console.log(promociones.value)
  } catch (error) {
    console.error('Error fetching promociones:', error);
  }
  console.log("Puede no haber nada")

  if (selectedRoom.value.PromocionID != null) {

    const matchedPromo = promociones.value.find(
      (promo) => promo.promocionID === selectedRoom.value.PromocionID
    );

    if (matchedPromo) {
      selectedPromocion.value = matchedPromo; // Set the selected promotion
      promocionActiva.value = true;
    }
  }
  document.body.style.overflow = 'hidden';
})


watch(selectedPromocion, (newVal) => {
  promocionActiva.value = newVal !== null; // True if a promo is selected
  props.room.visita.reservaActiva.promocionId = selectedPromocion.value.promocionID;
  actualizarPromocion();
});

const actualizarPromocion = () => {
  if (!selectedRoom.value || !selectedRoom.value.ReservaID) {
    console.error("Reserva or HabitacionID is not set.");
    return;
  }

  const reservaId = selectedRoom.value.ReservaID; // Example property, replace with the actual one holding reservaId
  const promocionId = selectedPromocion.value ? selectedPromocion.value.promocionID : null;

  // PUT request to update the promotion for the reservation
  axiosClient
    .put('/ActualizarReservaPromocion', null, {
      params: {
        reservaId,   // Pass reservaId as a query parameter
        promocionId, // Pass promocionId as a query parameter (or null if no promo is selected)
      },
    })
    .then((response) => {
      console.log("Promoción actualizada correctamente:", response.data);

      // Update the room object to reflect the change
      const updatedRoom = { ...props.room, promocionID: promocionId }; // Use the correct promocionID
      emits('update-room', updatedRoom); // Emit the updated room to the parent

      // Optionally: You can reset or set some local state here
      promocionActiva.value = promocionId !== null; // Set the promocionActiva flag accordingly
    })
    .catch((error) => {
      console.error("Error actualizando la promoción:", error);
    });
}
</script>
<style scoped>
.timer-container {
  display: flex;
  justify-content: center;
  align-items: center;
}

.timer {
  text-align: center;
}

.label {
  font-size: 24px;
  font-weight: bold;
  color: white;
  margin-bottom: 10px;
}

.time {
  font-size: 60px;
  /* Large numbers */
  font-weight: bold;
  color: white;
  display: flex;
  justify-content: center;
  align-items: center;
}

.time span {
  margin: 0 5px;
  transition: transform 0.2s ease-in-out;
}

/* Slight animation effect */
.time span:active {
  transform: scale(1.2);
}

.modal-outer-enter-active,
.modal-outer-leave-active {
  transition: opacity 0.3s cubic-bezier(0.52, 0.02, 0.19, 1.02);
}

.modal-outer-enter-from,
.modal-outer-leave-to {
  opacity: 0;
}

.modal-inner-enter-active {
  transition: all 0.3s cubic-bezier(0.52, 0.02, 0.19, 1.02) 0.10s;
}

.modal-inner-leave-active {
  transition: all 0.3s cubic-bezier(0.52, 0.02, 0.19, 1.02);
}

.modal-inner-enter-from {
  opacity: 0;
  transform: scale(0.8);
}

.modal-inner-leave-to {
  transform: scale(0.8);
}
</style>