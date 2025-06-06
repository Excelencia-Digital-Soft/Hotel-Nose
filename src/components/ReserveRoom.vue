<template>
  <Teleport to="body">
    <!-- PrimeVue Toast for notifications -->
    <Toast position="top-right" />
    <ConfirmDialog />
    
    <Transition name="modal-outer" appear>
      <div class="fixed w-full h-full bg-black bg-opacity-80 backdrop-blur-lg top-0 left-0 flex justify-center items-center px-4 py-4 z-50">
        <Transition name="modal-inner">
          <div class="relative w-full max-w-7xl h-[95vh] flex flex-col bg-neutral-800 border-x-4 border-fuchsia-900 rounded-3xl overflow-hidden">
            
            <!-- Header Compacto -->
            <div class="flex-shrink-0 px-4 py-3 bg-neutral-800 border-b border-neutral-700">
              <div class="flex justify-between items-center">
                <!-- Timer Section Compacto -->
                <div class="timer-container flex items-center bg-black bg-clip-padding backdrop-filter backdrop-blur-md bg-opacity-40 rounded-xl shadow-lg">
                  <div class="timer flex items-center border-x-2 border-primary-500 rounded-xl shadow-lg px-3 py-2">
                    <p class="text-primary-400 text-xs font-semibold mr-2">Tiempo:</p>
                    <p class="time mr-2 text-lg font-bold">
                      <span v-for="(char, index) in formattedTime" :key="index" class="digit">{{ char }}</span>
                    </p>
                    <!-- Timer Buttons Compactos -->
                    <div class="flex gap-1">
                      <button @click="ignorarTiempoExtra" type="button" 
                        :class="[
                          'w-8 h-8 font-semibold text-white text-xs rounded-lg transition-all flex items-center justify-center',
                          {
                            'bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 transform scale-95': ignorarTiempo,
                            'bg-neutral-600 hover:bg-neutral-500': !ignorarTiempo
                          }
                        ]">
                        <span class="material-symbols-outlined text-sm">block</span>
                      </button>
                      <button @click="toggleModalExtender" type="button" 
                        class="px-2 py-1 font-semibold text-white text-xs bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 hover:from-primary-500 hover:via-secondary-500 hover:to-accent-500 rounded-lg transition-all">
                        +Tiempo
                      </button>
                    </div>
                  </div>
                </div>

                <!-- Room Title -->
                <h1 class="text-xl lexend-exa font-bold bg-gradient-to-l from-accent-200 via-secondary-500 to-primary-300 bg-clip-text text-transparent">
                  {{ selectedRoom.nombreHabitacion }}
                </h1>

                <!-- Close Button -->
                <button class="text-xl w-10 h-10 text-white btn-danger rounded-full transition duration-150 flex items-center justify-center" @click="$emit('close-modal')">
                  ✕
                </button>
              </div>
            </div>

            <!-- Content Area - Sin Scroll -->
            <div class="flex-1 flex flex-col min-h-0">
              <div class="flex-1 grid grid-cols-1 lg:grid-cols-3 gap-4 p-4 min-h-0">
                
                <!-- Column 1: Customer Info & Promotions -->
                <div class="flex flex-col gap-3 min-h-0">
                  
                  <!-- Customer Information - Compacto -->
                  <div class="flex-1 grid grid-cols-1 gap-2 relative drop-shadow-xl overflow-hidden rounded-xl bg-[#691660]">
                    <div class="flex flex-col text-white z-[1] opacity-90 rounded-xl inset-0.5 bg-[#323132] p-3 h-full">
                      <h3 class="text-sm font-semibold text-white flex items-center gap-2 mb-2">
                        <span class="material-symbols-outlined text-sm">person</span>
                        Info Cliente
                      </h3>

                      <!-- Customer Fields Grid 2x2 -->
                      <div class="grid grid-cols-2 gap-2 flex-1">
                        <!-- Identificador -->
                        <div class="flex flex-col space-y-1">
                          <label class="text-xs font-semibold text-white flex items-center gap-1">
                            <span class="material-symbols-outlined text-xs">person</span>
                            Identificador
                          </label>
                          <input type="text"
                            class="text-xs text-neutral-900 border border-purple-200 rounded-lg py-1 px-2 focus:ring-purple-500 focus:border-purple-500 transition"
                            v-model="selectedRoom.Identificador" placeholder="Cliente" maxlength="40">
                        </div>

                        <!-- Hora de Entrada -->
                        <div class="flex flex-col space-y-1">
                          <label class="text-xs font-semibold text-white flex items-center gap-1">
                            <span class="material-symbols-outlined text-xs">schedule</span>
                            Entrada
                          </label>
                          <input type="datetime-local"
                            class="text-xs text-neutral-900 border border-purple-200 rounded-lg py-1 px-2 bg-gray-50"
                            v-model="horaEntrada" readonly>
                        </div>

                        <!-- Patente -->
                        <div class="flex flex-col space-y-1">
                          <label class="text-xs font-semibold text-white flex items-center gap-1">
                            <span class="material-symbols-outlined text-xs">directions_car</span>
                            Patente
                          </label>
                          <input type="text"
                            class="text-xs text-neutral-900 border border-purple-200 rounded-lg py-1 px-2 focus:ring-purple-500 focus:border-purple-500 transition"
                            v-model="selectedRoom.PatenteVehiculo" placeholder="ABC123">
                        </div>

                        <!-- Teléfono -->
                        <div class="flex flex-col space-y-1">
                          <label class="text-xs font-semibold text-white flex items-center gap-1">
                            <span class="material-symbols-outlined text-xs">phone</span>
                            Teléfono
                          </label>
                          <input type="text"
                            class="text-xs text-neutral-900 border border-purple-200 rounded-lg py-1 px-2 focus:ring-purple-500 focus:border-purple-500 transition"
                            maxlength="11" v-model="selectedRoom.NumeroTelefono" placeholder="264 123-4567">
                        </div>
                      </div>
                    </div>

                    <!-- Promotions Section - Compacto -->
                    <div class="flex flex-col text-white z-[1] opacity-95 rounded-xl inset-0.5 bg-gradient-to-br from-neutral-800 to-neutral-900 p-3">
                      <div class="flex items-center gap-2 mb-2">
                        <span class="material-symbols-outlined text-purple-400 text-sm">local_offer</span>
                        <h3 class="text-white font-semibold text-sm">Promociones</h3>
                      </div>
                      <select v-model="selectedPromocion" 
                        class="w-full text-xs text-gray-900 p-2 rounded-lg border border-purple-300 focus:border-purple-500 focus:ring-1 focus:ring-purple-500/20 transition bg-white">
                        <option :value="null">Sin Promoción</option>
                        <option v-for="promo in promociones" :key="promo.promocionID" :value="promo">
                          {{ promo.detalle }}
                        </option>
                        <option v-if="promociones.length === 0" disabled>Sin promociones</option>
                      </select>
                    </div>

                    <div class="absolute w-full h-full bg-white opacity-5 blur-[50px] -left-1/2 -top-1/2"></div>
                  </div>
                </div>

                <!-- Column 2: Consumos -->
                <div class="flex flex-col min-h-0">
                  <div class="flex-1 bg-gradient-to-br from-neutral-800 to-neutral-900 border-l-4 border-accent-400 rounded-l-3xl p-4 shadow-2xl flex flex-col min-h-0">
                    <!-- Header del consumo -->
                    <div class="flex items-center gap-2 mb-3 pb-2 border-b border-accent-400/30">
                      <span class="material-symbols-outlined text-accent-400">receipt_long</span>
                      <h3 class="text-white font-semibold">Consumos</h3>
                    </div>
                    
                    <!-- Header row -->
                    <div class="grid grid-cols-[2fr_50px_50px_50px_60px_30px_30px] gap-2 text-white text-xs font-semibold mb-2 px-1">
                      <span>Producto</span>
                      <span>Cant</span>
                      <span>$Unit</span>
                      <span>Tipo</span>
                      <span>Total</span>
                      <span>✏️</span>
                      <span>🗑️</span>
                    </div>

                    <!-- Consumos list con altura fija y scroll interno solo en desktop si es necesario -->
                    <div class="flex-1 min-h-0 overflow-y-auto lg:max-h-[calc(100vh-400px)]">
                      <ul class="space-y-1 pr-1">
                        <li v-for="consumo in consumos" :key="consumo.consumoId"
                          class="grid grid-cols-[2fr_50px_50px_50px_60px_30px_30px] gap-2 bg-gradient-to-r from-neutral-600 to-neutral-700 p-2 rounded-lg text-white items-center hover:from-neutral-500 hover:to-neutral-600 transition-all duration-200 border border-neutral-500/50">
                          
                          <span class="text-xs font-medium truncate" :title="consumo.articleName">{{ consumo.articleName }}</span>

                          <!-- Quantity Display/Edit -->
                          <template v-if="editingConsumoId !== consumo.consumoId">
                            <span class="text-xs text-center font-medium">{{ consumo.cantidad }}</span>
                          </template>
                          <template v-else>
                            <input type="number" v-model.number="editedCantidad" @blur="saveConsumo(consumo.consumoId)"
                              class="text-xs text-center p-1 rounded bg-neutral-800 text-white border border-accent-400 w-full" />
                          </template>

                          <span class="text-xs text-center text-green-300 font-medium">${{ consumo.precioUnitario }}</span>
                          <span class="text-xs text-center px-1 py-1 rounded font-medium"
                            :class="consumo.esHabitacion ? 'bg-purple-500/30 text-purple-200' : 'bg-blue-500/30 text-blue-200'">
                            {{ consumo.esHabitacion ? 'H' : 'I' }}
                          </span>
                          <span class="text-xs font-bold text-green-400 text-center">${{ consumo.total }}</span>

                          <!-- Edit/Cancel/Delete Buttons -->
                          <template v-if="editingConsumoId !== consumo.consumoId">
                            <button type="button" 
                              class="bg-blue-600 hover:bg-blue-500 rounded text-xs h-6 w-6 text-white flex justify-center items-center transition-colors material-symbols-outlined"
                              @click="startEditConsumo(consumo.consumoId)" title="Editar">
                              edit
                            </button>
                          </template>
                          <template v-else>
                            <button type="button" 
                              class="bg-gray-600 hover:bg-gray-500 rounded text-xs h-6 w-6 text-white flex justify-center items-center transition-colors material-symbols-outlined"
                              @click="cancelEditConsumo()" title="Cancelar">
                              close
                            </button>
                          </template>

                          <!-- Delete button -->
                          <button type="button"
                            class="bg-red-600 hover:bg-red-500 rounded text-xs h-6 w-6 text-white flex justify-center items-center transition-colors material-symbols-outlined"
                            @click="anularConsumo(consumo.consumoId)" title="Eliminar">
                            delete
                          </button>
                        </li>
                      </ul>
                    </div>

                    <!-- Consumption buttons -->
                    <div class="mt-3 flex gap-2 border-t border-accent-400/30 pt-3">
                      <button type="button" @click.stop="toggleModalConsumo(false)"
                        class="flex-1 bg-gradient-to-r from-blue-600 to-blue-700 hover:from-blue-500 hover:to-blue-600 text-white px-3 py-2 rounded-lg font-medium text-xs transition-all duration-200 flex items-center justify-center gap-1 shadow-lg">
                        <span class="material-symbols-outlined text-sm">inventory_2</span>
                        General
                      </button>
                      <button type="button" @click.stop="toggleModalConsumo(true)"
                        class="flex-1 bg-gradient-to-r from-purple-600 to-purple-700 hover:from-purple-500 hover:to-purple-600 text-white px-3 py-2 rounded-lg font-medium text-xs transition-all duration-200 flex items-center justify-center gap-1 shadow-lg">
                        <span class="material-symbols-outlined text-sm">hotel</span>
                        Habitación
                      </button>
                    </div>
                  </div>
                </div>

                <!-- Column 3: Billing Summary -->
                <div class="flex flex-col gap-3">
                  <!-- Billing Summary - Compacto -->
                  <div class="flex-1 relative drop-shadow-xl overflow-hidden rounded-xl bg-gradient-to-br from-green-900/40 to-emerald-900/40 border border-green-500/30">
                    <div class="absolute flex flex-col text-white z-[1] opacity-95 rounded-xl inset-0.5 bg-gradient-to-br from-neutral-800 to-neutral-900 p-3 h-full">
                      <div class="flex items-center gap-2 mb-3 pb-2 border-b border-green-500/30">
                        <span class="material-symbols-outlined text-green-400">calculate</span>
                        <h3 class="text-white font-semibold text-sm">Facturación</h3>
                      </div>

                      <div class="space-y-2 flex-1">
                        <!-- Consumos -->
                        <div class="flex justify-between items-center p-2 bg-neutral-700/50 rounded-lg hover:bg-neutral-600/50 transition-all duration-200">
                          <div class="flex items-center gap-2">
                            <span class="material-symbols-outlined text-blue-400 text-sm">restaurant</span>
                            <span class="font-medium text-sm">Consumos</span>
                          </div>
                          <span class="font-bold text-blue-300 text-sm">${{ consumos.reduce((sum, consumo) => sum + consumo.total, 0) }}</span>
                        </div>

                        <!-- Periodo -->
                        <div class="flex justify-between items-center p-2 bg-neutral-700/50 rounded-lg hover:bg-neutral-600/50 transition-all duration-200">
                          <div class="flex items-center gap-2">
                            <span class="material-symbols-outlined text-purple-400 text-sm">schedule</span>
                            <span class="font-medium text-sm">Periodo</span>
                          </div>
                          <span class="font-bold text-purple-300 text-sm">${{ periodoCost }}</span>
                        </div>

                        <!-- Adicional -->
                        <div class="flex justify-between items-center p-2 bg-neutral-700/50 rounded-lg hover:bg-neutral-600/50 transition-all duration-200">
                          <div class="flex items-center gap-2">
                            <span class="material-symbols-outlined text-orange-400 text-sm">add_circle</span>
                            <span class="font-medium text-sm">Tiempo Extra</span>
                          </div>
                          <span class="font-bold text-orange-300 text-sm">${{ adicional }}</span>
                        </div>

                        <!-- Total -->
                        <div class="flex justify-between items-center p-3 bg-gradient-to-r from-green-600/30 to-emerald-600/30 rounded-xl border border-green-500/40 mt-3">
                          <div class="flex items-center gap-2">
                            <span class="material-symbols-outlined text-green-400">paid</span>
                            <span class="font-bold text-base">TOTAL</span>
                          </div>
                          <span class="font-bold text-lg text-green-300">
                            ${{ (() => {
                              const consumoTotal = consumos.reduce((sum, consumo) => sum + (Number(consumo.total) || 0), 0);
                              const periodo = Number(periodoCost) || 0;
                              const adicionalValue = Number(adicional) || 0;
                              const total = consumoTotal + periodo + adicionalValue;
                              return isNaN(total) ? "0.00" : total.toFixed(2);
                            })() }}
                          </span>
                        </div>
                      </div>
                    </div>
                    <div class="absolute w-full h-full bg-green-400/5 blur-[50px] -left-1/2 -top-1/2"></div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Footer Compacto: Action Buttons -->
            <div class="flex-shrink-0 px-4 py-3 bg-neutral-800 border-t border-neutral-700">
              <div class="flex gap-3 justify-center">
                <button @click="toggleAnularOcupacionModal" type="button"
                  class="flex-1 max-w-xs bg-gradient-to-r from-red-600 to-red-700 hover:from-red-500 hover:to-red-600 text-white px-4 py-2 rounded-xl font-semibold transition-all duration-200 flex items-center justify-center gap-2 shadow-lg hover:shadow-red-500/25 disabled:opacity-50 disabled:cursor-not-allowed">
                  <span class="material-symbols-outlined">cancel</span>
                  Anular Ocupación
                </button>
                <button @click="openPaymentModal" type="button" 
                  :disabled="selectedRoom.pedidosPendientes || isProcessingPayment"
                  class="flex-1 max-w-xs bg-gradient-to-r from-green-600 to-green-700 hover:from-green-500 hover:to-green-600 text-white px-4 py-2 rounded-xl font-semibold transition-all duration-200 flex items-center justify-center gap-2 shadow-lg hover:shadow-green-500/25 disabled:opacity-50 disabled:cursor-not-allowed">
                  <span v-if="!isProcessingPayment" class="flex items-center gap-2">
                    <span class="material-symbols-outlined">door_open</span>
                    Desocupar Habitación
                  </span>
                  <span v-else class="flex items-center gap-2">
                    <span class="material-symbols-outlined animate-spin">sync</span>
                    Procesando...
                  </span>
                </button>
              </div>
              
              <!-- Conditional warning text -->
              <p v-if="selectedRoom.pedidosPendientes" class="text-red-500 mt-1 text-center text-xs">
                Hay pedidos pendientes, no se puede desocupar la habitación.
              </p>
            </div>

            <!-- Modals -->
            <ModalPagar v-if="modalPayment" 
              :periodo="Number(periodoCost) || 0" 
              :consumo="consumos.reduce((sum, consumo) => sum + (consumo.total || 0), 0)" 
              :total="Number(totalAmount) || 0" 
              :adicional="Number(adicional) || 0"
              :habitacionId="selectedRoom.HabitacionID" 
              :visitaId="selectedRoom.VisitaID" 
              :pausa="Boolean(Pausa)"
              @close="modalPayment = false" 
              @confirm-payment="handlePaymentConfirmation" />

            <AnularOcupacionModal v-if="modalAnular" 
              :reservaId="selectedRoom.ReservaID"
              @close-modal="modalAnular = false" 
              @ocupacion-anulada="handleOcupacionAnulada" />
              
            <ModalConsumo v-if="modalConsumo" 
              :name="selectedRoom.Identificador"
              :habitacionID="selectedRoom.HabitacionID" 
              :consumoHabitacion="esConsumoHabitacion"
              @confirmaAccion="confirmAndSend" 
              @close="modalConsumo = false" />
              
            <ModalExtenderOcupacion v-if="modalExtender" 
              :name="selectedRoom.Identificador"
              :reservaID="selectedRoom.ReservaID"
              @confirmExtension="agregarTiempoExtra" 
              @close-modal="modalExtender = false" />
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
import Toast from 'primevue/toast';
import ConfirmDialog from 'primevue/confirmdialog';
import { useToast } from 'primevue/usetoast';
import { useConfirm } from 'primevue/useconfirm';
import ModalConsumo from './ModalConsumo.vue';
import ModalExtenderOcupacion from './ExtenderOcupacionModal.vue';
import AnularOcupacionModal from './AnularOcupacionModal.vue';
import ModalPagar from './ModalPagar.vue';
import dayjs from 'dayjs';

const emits = defineEmits(['close-modal', 'update-room', 'update-tiempo', 'room-checkout']);
const props = defineProps({
  room: Object,
});

// PrimeVue composables
const toast = useToast();
const confirm = useConfirm();

const periodoCost = computed(() => {
  // Validate and get total hours and minutes
  const totalHours = Number(selectedRoom.value.TotalHoras) || 0;
  const totalMinutes = Number(selectedRoom.value.TotalMinutos) || 0;
  
  // Get hourly rate with proper validation
  let hourlyRate = 0;
  if (promocionActiva.value && selectedPromocion.value && selectedPromocion.value.tarifa) {
    hourlyRate = Number(selectedPromocion.value.tarifa) || 0;
  } else if (selectedRoom.value && selectedRoom.value.Precio) {
    hourlyRate = Number(selectedRoom.value.Precio) || 0;
  }
  
  // Calculate total period in hours
  const totalPeriod = totalHours + (totalMinutes / 60);
  
  // Calculate cost
  const cost = totalPeriod * hourlyRate;
  
  // Return with validation
  return isNaN(cost) ? "0.00" : cost.toFixed(2);
});

const adicional = computed(() => {
  console.log("Adicional overtime", overtime.value)
  
  // Validate overtime value
  const validOvertime = isNaN(overtime.value) || overtime.value === null || overtime.value === undefined ? 0 : overtime.value;
  
  // Get the hourly rate with validation
  let hourlyRate = 0;
  if (promocionActiva.value && selectedPromocion.value && selectedPromocion.value.tarifa) {
    hourlyRate = Number(selectedPromocion.value.tarifa) || 0;
  } else if (selectedRoom.value && selectedRoom.value.Precio) {
    hourlyRate = Number(selectedRoom.value.Precio) || 0;
  }
  
  // Calculate per minute rate
  const perMinuteRate = hourlyRate / 60;
  
  // Calculate the overtime value
  const valueInOverTime = validOvertime * perMinuteRate;
  
  // Round to nearest 100 with validation
  const roundedValue = Math.round(valueInOverTime / 100) * 100;
  
  // Final validation to ensure we never return NaN
  return isNaN(roundedValue) ? 0 : roundedValue;
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

  // Formatear la fecha de reserva para el input datetime-local (horario Argentina UTC-3)
  if (selectedRoom.value.FechaReserva) {
    const fecha = new Date(selectedRoom.value.FechaReserva);
    // Restar 3 horas para horario de Argentina
    fecha.setHours(fecha.getHours() - 3);
    horaEntrada.value = fecha.toISOString().slice(0, 16);
  }

  console.log(selectedRoom.value)
  actualizarConsumos();

  // Initialize timer interval from localStorage
  getTimerUpdateInterval();

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

const modalConsumo = ref(false);
const modalExtender = ref(false);
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
const cheatRefresh = ref(false);
const idNewTag = ref(0);
const numeroError = ref('');
const esConsumoHabitacion = ref(false)
const verconsumo = () => {
  console.log(selectedRoom.value.VisitaID)
  console.log("Consumos", consumos.value)
}
const agregarConsumos = (selectedItems) => {
  console.log("📦 agregarConsumos called with:", selectedItems);
  axiosClient.post(
    `/ConsumoGeneral?habitacionId=${selectedRoom.value.HabitacionID}&visitaId=${selectedRoom.value.VisitaID}`,
    selectedItems // Send selectedItems directly as the body
  )
    .then(response => {
      console.log('✅ Consumo general agregado exitosamente:', response.data);
      actualizarConsumos();
    })
    .catch(error => {
      console.error('❌ Error al agregar consumo general:', error);
    });
};

const anularConsumo = (consumoId) => {
  // Use PrimeVue confirm dialog for confirmation
  confirm.require({
    message: '¿Está seguro que desea anular este consumo?',
    header: 'Confirmar Anulación',
    icon: 'pi pi-exclamation-triangle',
    acceptLabel: 'Sí, anular',
    rejectLabel: 'Cancelar',
    acceptClass: 'p-button-danger',
    accept: () => {
      console.log("elimino", consumoId);
      axiosClient.delete(`/AnularConsumo?idConsumo=${consumoId}`)
        .then(response => {
          actualizarConsumos();
          toast.add({
            severity: 'success',
            summary: 'Éxito',
            detail: 'Consumo anulado exitosamente',
            life: 10000
          });
          console.log('Consumo anulado exitosamente');
        })
        .catch(error => {
          console.error('Error al anular consumo:', error);
          toast.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Error al anular el consumo. Por favor, intente nuevamente.',
            life: 10000
          });
        });
    },
    reject: () => {
      toast.add({
        severity: 'info',
        summary: 'Cancelado',
        detail: 'Operación cancelada',
        life: 5000
      });
    }
  });
};
const agregarConsumosHabitacion = (selectedItems) => {
  console.log("🏠 agregarConsumosHabitacion called with:", selectedItems);
  axiosClient.post(
    `/ConsumoHabitacion?habitacionId=${selectedRoom.value.HabitacionID}&visitaId=${selectedRoom.value.VisitaID}`,
    selectedItems // Send selectedItems directly as the body
  )
    .then(response => {
      console.log('✅ Consumo habitación agregado exitosamente:', response.data);
      actualizarConsumos();
    })
    .catch(error => {
      console.error('❌ Error al agregar consumo habitación:', error);
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
let isToggling = false;
const toggleModalConsumo = (esHabitacion) => {
  if (isToggling) return;
  isToggling = true;
  
  console.log('toggleModalConsumo called with esHabitacion:', esHabitacion);
  console.log('modalConsumo before toggle:', modalConsumo.value);
  
  esConsumoHabitacion.value = esHabitacion;
  modalConsumo.value = !modalConsumo.value;
  
  console.log('modalConsumo after toggle:', modalConsumo.value);
  console.log('selectedRoom.HabitacionID:', selectedRoom.value.HabitacionID);
  console.log('selectedRoom.Identificador:', selectedRoom.value.Identificador);
  
  setTimeout(() => {
    isToggling = false;
  }, 300);
}
const toggleModalExtender = () => {
  modalExtender.value = !modalExtender.value;
}
const agregarTiempoExtra = (horas, minutos) => {
  selectedRoom.value.TotalHoras = selectedRoom.value.TotalHoras + horas;
  selectedRoom.value.TotalMinutos = selectedRoom.value.TotalMinutos + minutos;
  // Update local time calculation
  calculateRemainingTime()
  modalExtender.value = false;
  // No need to reload immediately, just update local state
}
const toggleAnularOcupacionModal = () => {
  modalAnular.value = !modalAnular.value;
}

const confirmAndSend = (ConfirmedArticles) => {
  console.log("📦 confirmAndSend called with:", JSON.stringify(ConfirmedArticles));
  console.log("🏠 esConsumoHabitacion:", esConsumoHabitacion.value);
  
  if (esConsumoHabitacion.value) {
    console.log("📦 Calling agregarConsumosHabitacion");
    agregarConsumosHabitacion(ConfirmedArticles);
  } else {
    console.log("📦 Calling agregarConsumos");
    agregarConsumos(ConfirmedArticles);
  }
  modalConsumo.value = false;
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
      toast.add({
        severity: 'success',
        summary: 'Éxito',
        detail: 'Se terminó la reserva exitosamente',
        life: 10000
      });
      setTimeout(() => {
        window.location.reload();
      }, 1500);
    })
    .catch(error => {
      console.error(error);
    });
}

// LOGICA TIMER
const formattedTime = ref('');
let timerInterval = null;
let additionalCalculationInterval = null;
const timerUpdateInterval = ref(10); // Default 10 minutes

// Timer calculation logic for display only
function calculateRemainingTime() {
  if (!modalPayment.value) {
    if (selectedRoom.value.PausaHoras == 0 && selectedRoom.value.PausaMinutos == 0) {
      const endTime = dayjs(selectedRoom.value.FechaReserva)
        .add(selectedRoom.value.TotalHoras, 'hour')
        .add(selectedRoom.value.TotalMinutos, 'minute');
      const now = dayjs();
      const diffInMinutes = endTime.diff(now, 'minute');
      const isOvertime = diffInMinutes < 0;
      
      const hours = Math.floor(Math.abs(diffInMinutes) / 60);
      const minutes = Math.abs(diffInMinutes) % 60;
      
      if (isOvertime && ignorarTiempo.value == false) {
        formattedTime.value = `-${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}`;
      } else if (isOvertime) {
        formattedTime.value = `00:00`;
      } else {
        formattedTime.value = `${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}`;
      }
    } else {
      const absolutePausaHoras = Math.abs(selectedRoom.value.PausaHoras);
      const absolutePausaMinutos = Math.abs(selectedRoom.value.PausaMinutos);

      if (selectedRoom.value.PausaHoras < 0 || selectedRoom.value.PausaMinutos < 0) {
        Pausa.value = true;
        if (ignorarTiempo.value == false) {
          formattedTime.value = `-${String(absolutePausaHoras).padStart(2, '0')}:${String(absolutePausaMinutos).padStart(2, '0')}`;
        } else {
          formattedTime.value = `00:00`;
        }
      } else if (selectedRoom.value.PausaHoras > 0 || selectedRoom.value.PausaMinutos > 0) {
        Pausa.value = true;
        formattedTime.value = `${String(absolutePausaHoras).padStart(2, '0')}:${String(absolutePausaMinutos).padStart(2, '0')}`;
      }
    }
  }
}

// Separate function to calculate overtime for billing (runs every N minutes)
function calculateOvertimeForBilling() {
  if (!modalPayment.value) {
    if (selectedRoom.value.PausaHoras == 0 && selectedRoom.value.PausaMinutos == 0) {
      const endTime = dayjs(selectedRoom.value.FechaReserva)
        .add(selectedRoom.value.TotalHoras, 'hour')
        .add(selectedRoom.value.TotalMinutos, 'minute');
      const now = dayjs();
      const diffInMinutes = endTime.diff(now, 'minute');
      const isOvertime = diffInMinutes < 0;
      
      if (isOvertime && ignorarTiempo.value == false) {
        overtime.value = diffInMinutes * (-1);
      } else if (isOvertime) {
        overtime.value = 0;
      } else {
        overtime.value = 0;
      }
    } else {
      const absolutePausaHoras = Math.abs(selectedRoom.value.PausaHoras);
      const absolutePausaMinutos = Math.abs(selectedRoom.value.PausaMinutos);

      if (selectedRoom.value.PausaHoras < 0 || selectedRoom.value.PausaMinutos < 0) {
        Pausa.value = true;
        if (ignorarTiempo.value == false) {
          overtime.value = absolutePausaHoras * 60 + absolutePausaMinutos;
        } else {
          overtime.value = 0;
        }
      }
    }
  }
}

function ignorarTiempoExtra() {
  ignorarTiempo.value = !ignorarTiempo.value;
  calculateRemainingTime();
  calculateOvertimeForBilling();
}

// Function to get timer update interval from localStorage only
function getTimerUpdateInterval() {
  const storedInterval = localStorage.getItem('timerUpdateInterval');
  const intervalMinutes = storedInterval ? parseInt(storedInterval) : 10;
  timerUpdateInterval.value = intervalMinutes;
  console.log('Timer interval retrieved from localStorage:', intervalMinutes, 'minutes');
  return intervalMinutes;
}

// Function to start timer intervals
function startTimerIntervals() {
  // Clear any existing intervals
  if (timerInterval) clearInterval(timerInterval);
  if (additionalCalculationInterval) clearInterval(additionalCalculationInterval);
  
  // Start display timer (every second)
  timerInterval = setInterval(calculateRemainingTime, 1000);
  
  // Start additional cost calculation (every N minutes)
  const intervalMs = timerUpdateInterval.value * 60 * 1000; // Convert minutes to milliseconds
  additionalCalculationInterval = setInterval(calculateOvertimeForBilling, intervalMs);
  
  // Initial calculations
  calculateRemainingTime();
  calculateOvertimeForBilling();
}
// Watch for changes in selectedRoom
watch(() => selectedRoom.value, (newValue) => {
  if (newValue.FechaReserva && newValue.TotalHoras !== undefined && newValue.TotalMinutos !== undefined) {
    // Get timer update interval from localStorage and start timers
    getTimerUpdateInterval();
    startTimerIntervals();
  }
}, { deep: true });

// Clean up the intervals when the component is unmounted
onUnmounted(() => {
  if (timerInterval) clearInterval(timerInterval);
  if (additionalCalculationInterval) clearInterval(additionalCalculationInterval);
});




// States
const modalPayment = ref(false);
const totalAmount = ref(0);
const modalAnular = ref(false);
const isProcessingPayment = ref(false);

// Debug watcher
watch(modalPayment, (newVal) => {
  console.log("🔍 modalPayment changed to:", newVal);
});
// Props from your existing data, for example:

// Methods
const openPaymentModal = async () => {
  console.log("🔄 openPaymentModal called");
  // Prevent multiple clicks
  if (modalPayment.value || isProcessingPayment.value) {
    console.warn("Modal de pago ya está abierto o procesándose");
    return;
  }

  // Validate required data
  if (!selectedRoom.value.VisitaID) {
    console.error("No se puede abrir el modal de pago: VisitaID no está definido");
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: 'No se encontró la información de la visita',
      life: 10000
    });
    return;
  }

  // Set loading state
  isProcessingPayment.value = true;

  try {
    console.log("Abriendo modal de pago...");
    
    // Pause the room occupation
    try {
      await axiosClient.put(`/PausarOcupacion?visitaId=${selectedRoom.value.VisitaID}`);
      console.log("Habitación pausada exitosamente");
    } catch (pauseError) {
      console.error("Error al pausar la habitación:", pauseError);
      
      // Use PrimeVue confirm dialog
      confirm.require({
        message: 'No se pudo pausar la habitación. ¿Desea continuar con el pago?',
        header: 'Advertencia',
        icon: 'pi pi-exclamation-triangle',
        acceptLabel: 'Sí, continuar',
        rejectLabel: 'No, cancelar',
        acceptClass: 'p-button-warning',
        accept: async () => {
          // Continue with the payment process
          continuePaymentProcess();
        },
        reject: () => {
          isProcessingPayment.value = false;
          toast.add({
            severity: 'info',
            summary: 'Cancelado',
            detail: 'Operación cancelada por el usuario',
            life: 10000
          });
        }
      });
      return; // Exit early, the accept callback will continue if needed
    }

    // Continue with payment process
    continuePaymentProcess();

  } catch (error) {
    console.error("Error al abrir el modal de pago:", error);
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: 'Ocurrió un error al procesar el pago. Por favor, intente nuevamente.',
      life: 10000
    });
  } finally {
    // Always reset loading state
    isProcessingPayment.value = false;
  }
};

// Separate function to continue payment process
const continuePaymentProcess = () => {
  // Calculate all values with proper validation
  const calcularTotales = () => {
    // Consumos total with validation
    const consumoTotal = consumos.value.reduce((sum, consumo) => {
      const total = Number(consumo.total) || 0;
      return sum + total;
    }, 0);

    // Period cost with validation
    const periodoCostValue = Number(periodoCost.value) || 0;
    
    // Additional cost with validation
    const adicionalValue = Number(adicional.value) || 0;

    // Calculate final total
    const total = consumoTotal + periodoCostValue + adicionalValue;

    return {
      consumoTotal,
      periodoCostValue,
      adicionalValue,
      total: isNaN(total) ? 0 : total
    };
  };

  const totales = calcularTotales();
  
  // Set the total amount for the payment modal
  totalAmount.value = totales.total;

  // Log for debugging
  console.log("Totales calculados:", {
    consumos: totales.consumoTotal,
    periodo: totales.periodoCostValue,
    adicional: totales.adicionalValue,
    total: totales.total
  });

  // Validate total is reasonable (optional business logic)
  if (totales.total < 0) {
    console.error("Total negativo detectado");
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: 'El total no puede ser negativo',
      life: 10000
    });
    isProcessingPayment.value = false;
    return;
  }

  // Open the modal
  modalPayment.value = true;
  isProcessingPayment.value = false;
  console.log("Opening payment modal with:", {
    modalPayment: modalPayment.value,
    totalAmount: totalAmount.value,
    habitacionID: selectedRoom.value.HabitacionID,
    visitaID: selectedRoom.value.VisitaID,
    pausa: Pausa.value,
    periodo: Number(periodoCost.value) || 0,
    consumo: consumos.value.reduce((sum, consumo) => sum + (consumo.total || 0), 0),
    adicional: Number(adicional.value) || 0
  });
  
  // Debug: Check if modal gets closed immediately
  setTimeout(() => {
    console.log("🔍 Modal status after 100ms:", modalPayment.value);
  }, 100);
};

const handlePaymentConfirmation = (paymentDetails) => {
  console.log('Payment Confirmed:', paymentDetails);
  modalPayment.value = false;
  
  // Show success toast
  toast.add({
    severity: 'success',
    summary: 'Pago Confirmado',
    detail: 'El pago se procesó exitosamente. Habitación liberada.',
    life: 10000
  });
  
  // Reload page to update rooms
  setTimeout(() => {
    window.location.reload();
  }, 1500);
};

const handleOcupacionAnulada = (reservaId) => {
  console.log('🚫 Ocupación anulada para reserva:', reservaId);
  
  // Close the modal
  modalAnular.value = false;
  
  // Reload page to update rooms (room goes back to free)
  setTimeout(() => {
    window.location.reload();
  }, 1500);
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

  if (selectedRoom.value.PromocionID != null) {

    const matchedPromo = promociones.value.find(
      (promo) => promo.promocionID === selectedRoom.value.PromocionID
    );

    if (matchedPromo) {
      selectedPromocion.value = matchedPromo; // Set the selected promotion
      promocionActiva.value = true;
    }
  }

})


watch(selectedPromocion, (newVal) => {
  promocionActiva.value = newVal !== null; // True if a promo is selected
  if(promocionActiva.value)
  props.room.visita.reservaActiva.promocionId = selectedPromocion.value.promocionID;
  else props.room.visita.reservaActiva.promocionId = null
  actualizarPromocion();
});

const actualizarPromocion = () => {
  if (!selectedRoom.value || !selectedRoom.value.ReservaID) {
    console.error("Reserva or HabitacionID is not set.");
    return;
  }

  const reservaId = selectedRoom.value.ReservaID;
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

// SECCION CONSUMOS

const editingConsumoId = ref(null);
const editedCantidad = ref(0);
const horaEntrada = ref('');

const startEditConsumo = (consumoId) => {
  editingConsumoId.value = consumoId;
  const consumo = consumos.value.find(c => c.consumoId === consumoId);
  if (consumo) {
    editedCantidad.value = consumo.cantidad;
  }
};

const cancelEditConsumo = () => {
  editingConsumoId.value = null;
};

const saveConsumo = (consumoId) => {
  if (editingConsumoId.value === consumoId) {
    axiosClient.put(`/UpdateConsumo?idConsumo=${consumoId}&Cantidad=${editedCantidad.value}`)
      .then(response => {
        console.log('Consumo updated successfully:', response.data);
        // Update the consumos array with the new quantity
        const consumo = consumos.value.find(c => c.consumoId === consumoId);
        if (consumo) {
          consumo.cantidad = editedCantidad.value;
          consumo.total = consumo.cantidad * consumo.precioUnitario; // Recalculate total
        }
        editingConsumoId.value = null; // Exit edit mode
      })
      .catch(error => {
        console.error('Error updating consumo:', error);
        toast.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Error al actualizar el consumo. Ver consola para más detalles.',
          life: 10000
        });
      });
  }
};
</script>
<style scoped>
.timer {
  text-align: center;
}

.time {
  font-size: 24px;
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

.timer {
  font-family: 'Courier New', monospace;
  text-align: center;

  border-radius: 10px;
  padding: 4px;
}

.time {
  font-size: 2rem;
  font-weight: bold;
  color: #ff1cc3;
  display: flex;
  justify-content: center;
  align-items: center;
  letter-spacing: -10px;
}


.digit {
  display: inline-block;
  min-width: 10px;
  text-align: center;
  animation: glow 1.5s infinite alternate;
}

@keyframes glow {
  0% {
    text-shadow: 0 0 5px #ff1cc3, 0 0 10px #ff1cc3;
  }

  100% {
    text-shadow: 0 0 10px #ff1cc3, 0 0 20px #ff1cc3;
  }
}
</style>
