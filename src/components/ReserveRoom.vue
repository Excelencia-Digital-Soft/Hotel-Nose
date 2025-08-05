<template>
  <Teleport to="body">
    <!-- PrimeVue Toast for notifications -->
    <Toast position="top-right" />
    <ConfirmDialog />

    <Transition name="modal-outer" appear>
      <div
        class="fixed w-full h-full bg-black/60 backdrop-blur-xl top-0 left-0 flex justify-center items-center px-4 py-4 z-50"
      >
        <Transition name="modal-inner">
          <div
            class="relative w-full max-w-7xl h-[95vh] flex flex-col bg-white/5 backdrop-blur-2xl border border-white/20 rounded-3xl overflow-hidden shadow-2xl"
          >
            <!-- Header Compacto -->
            <div
              class="flex-shrink-0 px-4 py-3 bg-white/5 backdrop-blur-md border-b border-white/10"
            >
              <div class="flex justify-between items-center">
                <!-- Timer Section Compacto -->
                <div
                  class="timer-container flex items-center bg-white/10 backdrop-blur-md border border-white/20 rounded-xl shadow-lg"
                >
                  <div
                    class="timer flex items-center border border-primary-400/50 rounded-xl shadow-lg px-3 py-2 bg-white/5 backdrop-blur-sm"
                  >
                    <p class="text-primary-400 text-xs font-semibold mr-2">Tiempo:</p>
                    <p class="time mr-2 text-lg font-bold">
                      <span v-for="(char, index) in formattedTime" :key="index" class="digit">{{
                        char
                      }}</span>
                    </p>
                    <!-- Timer Buttons Compactos -->
                    <div class="flex gap-1">
                      <button
                        @click="ignorarTiempoExtra"
                        type="button"
                        v-tooltip.top="
                          ignorarTiempo
                            ? 'Volver a contar tiempo extra'
                            : 'Ignorar tiempo extra en facturación'
                        "
                        :class="[
                          'w-8 h-8 font-semibold text-white text-xs rounded-lg transition-all flex items-center justify-center backdrop-blur-sm',
                          {
                            'bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 transform scale-95 border border-white/30':
                              ignorarTiempo,
                            'bg-white/10 hover:bg-white/20 border border-white/20': !ignorarTiempo,
                          },
                        ]"
                      >
                        <span class="material-symbols-outlined text-sm">block</span>
                      </button>
                      <button
                        ref="timeExtensionButton"
                        @click="toggleTimeExtensionPopover"
                        type="button"
                        v-tooltip.top="'Extender tiempo de ocupación'"
                        class="px-2 py-1 font-semibold text-white text-xs bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 hover:from-primary-500 hover:via-secondary-500 hover:to-accent-500 rounded-lg transition-all backdrop-blur-sm border border-white/30"
                      >
                        +Tiempo
                      </button>
                    </div>
                  </div>
                </div>

                <!-- Room Title -->
                <h1
                  class="text-xl lexend-exa font-bold bg-gradient-to-l from-accent-200 via-secondary-500 to-primary-300 bg-clip-text text-transparent"
                >
                  {{ selectedRoom.nombreHabitacion }}
                </h1>

                <!-- Close Button -->
                <button
                  class="text-xl w-10 h-10 text-white bg-red-500/20 hover:bg-red-500/30 border border-red-500/50 hover:border-red-500/70 rounded-full transition-all duration-300 flex items-center justify-center backdrop-blur-sm"
                  @click="$emit('close-modal')"
                >
                  ✕
                </button>
              </div>
            </div>

            <!-- Content Area -->
            <div class="flex-1 flex flex-col min-h-0">
              <div class="flex-1 grid grid-cols-1 lg:grid-cols-3 gap-4 p-4 min-h-0">
                <!-- Column 1: Customer Info & Promotions -->
                <div class="flex flex-col gap-3 min-h-0">
                  <!-- Customer Information -->
                  <div
                    class="flex-1 grid grid-cols-1 gap-2 relative drop-shadow-xl overflow-hidden rounded-xl bg-white/5 backdrop-blur-md border border-white/20"
                  >
                    <div class="flex flex-col text-white z-[1] rounded-xl p-3 h-full">
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
                          <input
                            type="text"
                            class="text-xs text-white bg-white/10 backdrop-blur-sm border border-white/30 rounded-lg py-1 px-2 focus:ring-primary-400 focus:border-primary-400 transition placeholder-gray-300"
                            v-model="selectedRoom.Identificador"
                            placeholder="Cliente"
                            maxlength="40"
                          />
                        </div>

                        <!-- Hora de Entrada -->
                        <div class="flex flex-col space-y-1">
                          <label class="text-xs font-semibold text-white flex items-center gap-1">
                            <span class="material-symbols-outlined text-xs">schedule</span>
                            Entrada
                          </label>
                          <input
                            type="datetime-local"
                            class="text-xs text-white bg-white/10 backdrop-blur-sm border border-white/30 rounded-lg py-1 px-2"
                            v-model="horaEntrada"
                            readonly
                          />
                        </div>

                        <!-- Patente -->
                        <div class="flex flex-col space-y-1">
                          <label class="text-xs font-semibold text-white flex items-center gap-1">
                            <span class="material-symbols-outlined text-xs">directions_car</span>
                            Patente
                          </label>
                          <input
                            type="text"
                            class="text-xs text-white bg-white/10 backdrop-blur-sm border border-white/30 rounded-lg py-1 px-2 focus:ring-primary-400 focus:border-primary-400 transition placeholder-gray-300"
                            v-model="selectedRoom.PatenteVehiculo"
                            placeholder="ABC123"
                          />
                        </div>

                        <!-- Teléfono -->
                        <div class="flex flex-col space-y-1">
                          <label class="text-xs font-semibold text-white flex items-center gap-1">
                            <span class="material-symbols-outlined text-xs">phone</span>
                            Teléfono
                          </label>
                          <input
                            type="text"
                            class="text-xs text-white bg-white/10 backdrop-blur-sm border border-white/30 rounded-lg py-1 px-2 focus:ring-primary-400 focus:border-primary-400 transition placeholder-gray-300"
                            maxlength="11"
                            v-model="selectedRoom.NumeroTelefono"
                            placeholder="264 123-4567"
                          />
                        </div>
                      </div>
                    </div>

                    <!-- Promotions Section -->
                    <div
                      class="flex flex-col text-white z-[1] rounded-xl bg-white/5 backdrop-blur-md border border-white/20 p-3"
                    >
                      <div class="flex items-center gap-2 mb-2">
                        <span class="material-symbols-outlined text-purple-400 text-sm"
                          >local_offer</span
                        >
                        <h3 class="text-white font-semibold text-sm">Promociones</h3>
                      </div>
                      <select
                        v-model="selectedPromocion"
                        class="w-full text-xs text-gray-900 p-2 rounded-lg border border-purple-300 focus:border-purple-500 focus:ring-1 focus:ring-purple-500/20 transition bg-white"
                      >
                        <option :value="null">Sin Promoción</option>
                        <option
                          v-for="promo in promociones"
                          :key="promo.promocionId"
                          :value="promo"
                        >
                          {{ promo.nombre }}
                        </option>
                        <option v-if="promociones.length === 0" disabled>Sin promociones</option>
                      </select>
                    </div>

                    <div
                      class="absolute w-full h-full bg-white opacity-5 blur-[50px] -left-1/2 -top-1/2"
                    ></div>
                  </div>
                </div>

                <!-- Column 2: Consumos -->
                <div class="flex flex-col min-h-0">
                  <div
                    class="flex-1 bg-gradient-to-br from-neutral-800 to-neutral-900 border-l-4 border-accent-400 rounded-l-3xl p-4 shadow-2xl flex flex-col min-h-0"
                  >
                    <!-- Header del consumo -->
                    <div class="flex items-center gap-2 mb-3 pb-2 border-b border-accent-400/30">
                      <span class="material-symbols-outlined text-accent-400">receipt_long</span>
                      <h3 class="text-white font-semibold">Consumos</h3>
                    </div>

                    <!-- Header row -->
                    <div
                      class="grid grid-cols-[2fr_50px_50px_50px_60px_30px_30px] gap-2 text-white text-xs font-semibold mb-2 px-1"
                    >
                      <span>Producto</span>
                      <span>Cant</span>
                      <span>$Unit</span>
                      <span>Tipo</span>
                      <span>Total</span>
                      <span>✏️</span>
                      <span>🗑️</span>
                    </div>

                    <!-- Consumos list -->
                    <div class="flex-1 min-h-0 overflow-y-auto lg:max-h-[calc(100vh-400px)]">
                      <ul class="space-y-1 pr-1">
                        <li
                          v-for="consumo in consumos"
                          :key="consumo.consumoId"
                          v-tooltip.left="consumo.articleName"
                          class="grid grid-cols-[2fr_50px_50px_50px_60px_30px_30px] gap-2 bg-gradient-to-r from-neutral-600 to-neutral-700 p-2 rounded-lg text-white items-center hover:from-neutral-500 hover:to-neutral-600 transition-all duration-200 border border-neutral-500/50 cursor-help"
                        >
                          <span class="text-xs font-medium truncate" :title="consumo.articleName">{{
                            consumo.articleName
                          }}</span>

                          <!-- Quantity Display/Edit -->
                          <template v-if="editingConsumoId !== consumo.consumoId">
                            <span class="text-xs text-center font-medium">{{
                              consumo.cantidad
                            }}</span>
                          </template>
                          <template v-else>
                            <input
                              type="number"
                              v-model.number="editedCantidad"
                              @blur="saveConsumo(consumo.consumoId)"
                              class="text-xs text-center p-1 rounded bg-neutral-800 text-white border border-accent-400 w-full"
                            />
                          </template>

                          <span class="text-xs text-center text-green-300 font-medium"
                            >${{ consumo.precioUnitario }}</span
                          >
                          <span
                            class="text-xs text-center px-1 py-1 rounded font-medium cursor-help"
                            v-tooltip.top="
                              consumo.esHabitacion
                                ? 'Consumo de Habitación'
                                : 'Consumo de Inventario General'
                            "
                            :class="
                              consumo.esHabitacion
                                ? 'bg-purple-500/30 text-purple-200'
                                : 'bg-blue-500/30 text-blue-200'
                            "
                          >
                            {{ consumo.esHabitacion ? 'H' : 'I' }}
                          </span>
                          <span class="text-xs font-bold text-green-400 text-center"
                            >${{ consumo.total }}</span
                          >

                          <!-- Edit/Cancel/Delete Buttons -->
                          <template v-if="editingConsumoId !== consumo.consumoId">
                            <button
                              type="button"
                              v-tooltip.top="'Editar cantidad'"
                              class="bg-blue-600 hover:bg-blue-500 rounded text-xs h-6 w-6 text-white flex justify-center items-center transition-colors material-symbols-outlined"
                              @click="startEditConsumo(consumo.consumoId)"
                            >
                              edit
                            </button>
                          </template>
                          <template v-else>
                            <button
                              type="button"
                              v-tooltip.top="'Cancelar edición'"
                              class="bg-gray-600 hover:bg-gray-500 rounded text-xs h-6 w-6 text-white flex justify-center items-center transition-colors material-symbols-outlined"
                              @click="cancelEditConsumo()"
                            >
                              close
                            </button>
                          </template>

                          <!-- Delete button -->
                          <button
                            type="button"
                            v-tooltip.top="'Eliminar consumo'"
                            class="bg-red-600 hover:bg-red-500 rounded text-xs h-6 w-6 text-white flex justify-center items-center transition-colors material-symbols-outlined"
                            @click="anularConsumo(consumo.consumoId)"
                          >
                            delete
                          </button>
                        </li>
                      </ul>
                    </div>

                    <!-- Consumption buttons -->
                    <div class="mt-3 flex gap-2 border-t border-accent-400/30 pt-3">
                      <button
                        type="button"
                        @click.stop="toggleModalConsumo(false)"
                        v-tooltip.top="'Agregar productos del inventario general'"
                        class="flex-1 bg-gradient-to-r from-blue-600 to-blue-700 hover:from-blue-500 hover:to-blue-600 text-white px-3 py-2 rounded-lg font-medium text-xs transition-all duration-200 flex items-center justify-center gap-1 shadow-lg"
                      >
                        <span class="material-symbols-outlined text-sm">inventory_2</span>
                        General
                      </button>
                      <button
                        type="button"
                        @click.stop="toggleModalConsumo(true)"
                        v-tooltip.top="'Agregar productos del inventario de la habitación'"
                        class="flex-1 bg-gradient-to-r from-purple-600 to-purple-700 hover:from-purple-500 hover:to-purple-600 text-white px-3 py-2 rounded-lg font-medium text-xs transition-all duration-200 flex items-center justify-center gap-1 shadow-lg"
                      >
                        <span class="material-symbols-outlined text-sm">hotel</span>
                        Habitación
                      </button>
                    </div>
                  </div>
                </div>

                <!-- Column 3: Billing Summary -->
                <div class="flex flex-col gap-3">
                  <!-- Billing Summary -->
                  <div
                    class="flex-1 relative drop-shadow-xl overflow-hidden rounded-xl bg-gradient-to-br from-green-900/40 to-emerald-900/40 border border-green-500/30"
                  >
                    <div
                      class="absolute flex flex-col text-white z-[1] opacity-95 rounded-xl inset-0.5 bg-gradient-to-br from-neutral-800 to-neutral-900 p-3 h-full"
                    >
                      <div class="flex items-center gap-2 mb-3 pb-2 border-b border-green-500/30">
                        <span class="material-symbols-outlined text-green-400">calculate</span>
                        <h3 class="text-white font-semibold text-sm">Facturación</h3>
                      </div>

                      <div class="space-y-2 flex-1">
                        <!-- Consumos -->
                        <div
                          class="flex justify-between items-center p-2 bg-neutral-700/50 rounded-lg hover:bg-neutral-600/50 transition-all duration-200"
                        >
                          <div class="flex items-center gap-2">
                            <span class="material-symbols-outlined text-blue-400 text-sm"
                              >restaurant</span
                            >
                            <span class="font-medium text-sm">Consumos</span>
                          </div>
                          <span class="font-bold text-blue-300 text-sm"
                            >${{ consumos.reduce((sum, consumo) => sum + consumo.total, 0) }}</span
                          >
                        </div>

                        <!-- Periodo -->
                        <div
                          class="flex justify-between items-center p-2 bg-neutral-700/50 rounded-lg hover:bg-neutral-600/50 transition-all duration-200"
                        >
                          <div class="flex items-center gap-2">
                            <span class="material-symbols-outlined text-purple-400 text-sm"
                              >schedule</span
                            >
                            <span class="font-medium text-sm">Periodo</span>
                          </div>
                          <span class="font-bold text-purple-300 text-sm">${{ periodoCost }}</span>
                        </div>

                        <!-- Adicional -->
                        <div
                          class="flex justify-between items-center p-2 bg-neutral-700/50 rounded-lg hover:bg-neutral-600/50 transition-all duration-200"
                        >
                          <div class="flex items-center gap-2">
                            <span class="material-symbols-outlined text-orange-400 text-sm"
                              >add_circle</span
                            >
                            <span class="font-medium text-sm">Tiempo Extra</span>
                          </div>
                          <span class="font-bold text-orange-300 text-sm">${{ adicional }}</span>
                        </div>

                        <!-- Total -->
                        <div
                          class="flex justify-between items-center p-3 bg-gradient-to-r from-green-600/30 to-emerald-600/30 rounded-xl border border-green-500/40 mt-3"
                        >
                          <div class="flex items-center gap-2">
                            <span class="material-symbols-outlined text-green-400">paid</span>
                            <span class="font-bold text-base">TOTAL</span>
                          </div>
                          <span class="font-bold text-lg text-green-300">
                            ${{
                              (() => {
                                const consumoTotal = consumos.reduce(
                                  (sum, consumo) => sum + (Number(consumo.total) || 0),
                                  0
                                )
                                const periodo = Number(periodoCost) || 0
                                const adicionalValue = Number(adicional) || 0
                                const total = consumoTotal + periodo + adicionalValue
                                return isNaN(total) ? '0.00' : total.toFixed(2)
                              })()
                            }}
                          </span>
                        </div>
                      </div>
                    </div>
                    <div
                      class="absolute w-full h-full bg-green-400/5 blur-[50px] -left-1/2 -top-1/2"
                    ></div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Footer: Action Buttons -->
            <div class="flex-shrink-0 px-4 py-3 bg-neutral-800 border-t border-neutral-700">
              <div class="flex gap-3 justify-center">
                <button
                  @click="toggleAnularOcupacionModal"
                  type="button"
                  class="flex-1 max-w-xs bg-gradient-to-r from-red-600 to-red-700 hover:from-red-500 hover:to-red-600 text-white px-4 py-2 rounded-xl font-semibold transition-all duration-200 flex items-center justify-center gap-2 shadow-lg hover:shadow-red-500/25 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  <span class="material-symbols-outlined">cancel</span>
                  Anular Ocupación
                </button>
                <button
                  @click="openPaymentModal"
                  type="button"
                  :disabled="selectedRoom.pedidosPendientes || isProcessingPayment"
                  class="flex-1 max-w-xs bg-gradient-to-r from-green-600 to-green-700 hover:from-green-500 hover:to-green-600 text-white px-4 py-2 rounded-xl font-semibold transition-all duration-200 flex items-center justify-center gap-2 shadow-lg hover:shadow-green-500/25 disabled:opacity-50 disabled:cursor-not-allowed"
                >
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
              <p
                v-if="selectedRoom.pedidosPendientes"
                class="text-red-500 mt-1 text-center text-xs"
              >
                Hay pedidos pendientes, no se puede desocupar la habitación.
              </p>
            </div>

            <!-- OverlayPanel for Time Extension -->
            <OverlayPanel ref="timeExtensionPopover" appendTo="body" :showCloseIcon="true">
              <TimeExtensionPopover
                @confirm="handleTimeExtension"
                @cancel="closeTimeExtensionPopover"
              />
            </OverlayPanel>

            <!-- Modals -->
            <ModalPagar
              v-if="modalPayment"
              @close="modalPayment = false"
              @confirm-payment="handlePaymentConfirmation"
            />

            <AnularOcupacionModal
              :visible="modalAnular"
              :habitacion="selectedRoom"
              @close-modal="modalAnular = false"
              @ocupacion-anulada="handleOcupacionAnulada"
            />

            <ModalConsumo
              v-if="modalConsumo"
              ref="modalConsumoRef"
              :name="selectedRoom.Identificador"
              :habitacionID="selectedRoom.HabitacionID"
              :consumoHabitacion="esConsumoHabitacion"
              @confirmaAccion="confirmAndSend"
              @close="modalConsumo = false"
            />
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
  import { onMounted, ref, nextTick, computed, type Ref, type ComputedRef } from 'vue'
  import Toast from 'primevue/toast'
  import ConfirmDialog from 'primevue/confirmdialog'
  import OverlayPanel from 'primevue/overlaypanel'
  import { useToast } from 'primevue/usetoast'
  import { useConfirm } from 'primevue/useconfirm'

  // Import modals and components
  import ModalConsumo from './ModalConsumo.vue'
  import AnularOcupacionModal from './AnularOcupacionModal.vue'
  import ModalPagar from './ModalPagar.vue'
  import TimeExtensionPopover from './popovers/TimeExtensionPopover.vue'

  // Import composables
  import { useReserveRoom } from '../composables/useReserveRoom'
  import { useTimer } from '../composables/useTimer'
  import { usePromociones } from '../composables/usePromociones'
  import { useConsumos } from '../composables/useConsumos'
  import { useReservas } from '../composables/useReservas'
  import { usePaymentProvider } from '../composables/usePaymentProvider'

  // Import services and types
  import { InventoryService } from '../services/roomInventoryService.ts'
  import { InventoryLocationType } from '../types'
  import type {
    RoomReservation,
    ConsumoResponseDto,
    PromocionDto,
    InventoryItem,
    InventoryDto,
  } from '../types'

  // Props and emits
  interface Props {
    room: RoomReservation
  }

  interface Emits {
    'close-modal': []
    'update-room': [room: RoomReservation]
    'update-tiempo': [tiempo: number]
    'room-checkout': [room: RoomReservation]
  }

  const emits = defineEmits<Emits>()
  const props = defineProps<Props>()

  // Composables
  const toast = useToast()
  const confirm = useConfirm()

  // Feature flag for V1 API (can be set to true to use new endpoints)
  const USE_V1_API: boolean = true

  // Use composables
  const {
    selectedRoom,
    horaEntrada,
    overtime,
    ignorarTiempo,
    Pausa,
    initializeRoom,
    ignorarTiempoExtra,
  } = useReserveRoom(props)

  const { selectedPromocion, promociones, promocionActiva } = usePromociones(
    props,
    selectedRoom,
    emits,
    USE_V1_API
  )

  const { formattedTime, startTimerIntervals, stopTimerIntervals, getTimerUpdateInterval } =
    useTimer(selectedRoom, ignorarTiempo, Pausa, overtime)

  const {
    consumos,
    editingConsumoId,
    editedCantidad,
    actualizarConsumos,
    agregarConsumos,
    agregarConsumosHabitacion,
    anularConsumo,
    startEditConsumo,
    cancelEditConsumo,
    saveConsumo,
  } = useConsumos(selectedRoom, USE_V1_API)

  const {
    modalAnular,
    isProcessingPayment,
    pauseOcupacion,
    extendTime,
    toggleAnularOcupacionModal,
    handleOcupacionAnulada,
  } = useReservas(selectedRoom, USE_V1_API)

  // Initialize payment provider
  const paymentProvider = usePaymentProvider()

  // Local modal states
  const modalConsumo = ref<boolean>(false)
  const modalConsumoRef = ref<any>(null)
  const modalPayment = ref<boolean>(false)
  const totalAmount = ref<number>(0)
  const esConsumoHabitacion = ref<boolean>(false)

  // OverlayPanel refs
  const timeExtensionButton = ref<any>(null)
  const timeExtensionPopover = ref<any>(null)

  // Calculate period cost
  const periodoCost: ComputedRef<string> = computed(() => {
    const totalHours = Number(selectedRoom.value.TotalHoras) || 0
    const totalMinutes = Number(selectedRoom.value.TotalMinutos) || 0

    let hourlyRate = 0
    if (promocionActiva.value && selectedPromocion.value && selectedPromocion.value.tarifa) {
      hourlyRate = Number(selectedPromocion.value.tarifa) || 0
    } else if (selectedRoom.value && selectedRoom.value.Precio) {
      hourlyRate = Number(selectedRoom.value.Precio) || 0
    }

    const totalPeriod = totalHours + totalMinutes / 60
    const cost = totalPeriod * hourlyRate

    return isNaN(cost) ? '0.00' : cost.toFixed(2)
  })

  // Calculate additional cost
  const adicional: ComputedRef<number> = computed(() => {
    const validOvertime =
      isNaN(overtime.value) || overtime.value === null || overtime.value === undefined
        ? 0
        : overtime.value

    let hourlyRate = 0
    if (promocionActiva.value && selectedPromocion.value && selectedPromocion.value.tarifa) {
      hourlyRate = Number(selectedPromocion.value.tarifa) || 0
    } else if (selectedRoom.value && selectedRoom.value.Precio) {
      hourlyRate = Number(selectedRoom.value.Precio) || 0
    }

    const perMinuteRate = hourlyRate / 60
    const valueInOverTime = validOvertime * perMinuteRate
    const roundedValue = Math.round(valueInOverTime / 100) * 100

    return isNaN(roundedValue) ? 0 : roundedValue
  })

  // Modal handlers
  let isToggling: boolean = false
  const toggleModalConsumo = async (esHabitacion: boolean): Promise<void> => {
    if (isToggling) return
    isToggling = true

    if (modalConsumo.value) {
      modalConsumo.value = false
      await nextTick()
    }

    esConsumoHabitacion.value = esHabitacion
    await nextTick()
    modalConsumo.value = true

    // El stock se maneja localmente, no necesitamos recargar desde API
    // Solo recargamos al abrir para obtener datos frescos del servidor

    setTimeout(() => {
      isToggling = false
    }, 300)
  }

  const confirmAndSend = async (ConfirmedArticles: InventoryItem[]): Promise<void> => {
    try {
      // 1. Agregar los consumos
      if (esConsumoHabitacion.value) {
        await agregarConsumosHabitacion(ConfirmedArticles)
      } else {
        await agregarConsumos(ConfirmedArticles)
      }

      // 2. Actualizar el stock en el inventario
      // Obtener el inventario una sola vez
      let inventoryResponse: { data: InventoryDto[] }
      if (esConsumoHabitacion.value) {
        inventoryResponse = await InventoryService.getRoomInventory(selectedRoom.value.HabitacionID)
      } else {
        inventoryResponse = await InventoryService.getGeneralInventory()
      }

      // Mostrar mensaje de éxito adicional
      toast.add({
        severity: 'success',
        summary: 'Stock actualizado',
        detail: 'El inventario ha sido actualizado correctamente',
        life: 3000,
      })

      modalConsumo.value = false
    } catch (error) {
      console.error('Error al procesar consumos:', error)
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Hubo un problema al actualizar el inventario',
        life: 5000,
      })
      modalConsumo.value = false
    }
  }

  // OverlayPanel handlers
  const toggleTimeExtensionPopover = (event: Event): void => {
    timeExtensionPopover.value.toggle(event)
  }

  const closeTimeExtensionPopover = (): void => {
    timeExtensionPopover.value.hide()
  }

  interface TimeExtension {
    hours: number
    minutes: number
  }

  const handleTimeExtension = async ({ hours, minutes }: TimeExtension): Promise<void> => {
    try {
      await extendTime(hours, minutes)
      // Update local state directly for immediate feedback
      selectedRoom.value.TotalHoras = selectedRoom.value.TotalHoras + hours
      selectedRoom.value.TotalMinutos = selectedRoom.value.TotalMinutos + minutes
      closeTimeExtensionPopover()

      toast.add({
        severity: 'success',
        summary: 'Éxito',
        detail: `Tiempo extendido: ${hours}h ${minutes}m`,
        life: 5000,
      })
    } catch (error) {
      console.error('Error extending time:', error)
    }
  }

  // Payment modal logic
  const openPaymentModal = async () => {
    if (modalPayment.value || isProcessingPayment.value) {
      return
    }

    if (!selectedRoom.value.VisitaID) {
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'No se encontró la información de la visita',
        life: 10000,
      })
      return
    }

    isProcessingPayment.value = true

    try {
      await pauseOcupacion()
      continuePaymentProcess()
    } catch (pauseError) {
      confirm.require({
        message: 'No se pudo pausar la habitación. ¿Desea continuar con el pago?',
        header: 'Advertencia',
        icon: 'pi pi-exclamation-triangle',
        acceptLabel: 'Sí, continuar',
        rejectLabel: 'No, cancelar',
        acceptClass: 'p-button-warning',
        accept: () => continuePaymentProcess(),
        reject: () => {
          isProcessingPayment.value = false
          toast.add({
            severity: 'info',
            summary: 'Cancelado',
            detail: 'Operación cancelada por el usuario',
            life: 10000,
          })
        },
      })
    }
  }

  const continuePaymentProcess = () => {
    const calcularTotales = () => {
      const consumoTotal = consumos.value.reduce((sum, consumo) => {
        const total = Number(consumo.total) || 0
        return sum + total
      }, 0)

      const periodoCostValue = Number(periodoCost.value) || 0
      const adicionalValue = Number(adicional.value) || 0
      const total = consumoTotal + periodoCostValue + adicionalValue

      return {
        consumoTotal,
        periodoCostValue,
        adicionalValue,
        total: isNaN(total) ? 0 : total,
      }
    }

    const totales = calcularTotales()

    if (totales.total < 0) {
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'El total no puede ser negativo',
        life: 10000,
      })
      isProcessingPayment.value = false
      return
    }

    // Initialize payment provider with calculated data
    paymentProvider.updatePaymentData({
      periodo: totales.periodoCostValue,
      consumo: totales.consumoTotal,
      adicional: totales.adicionalValue,
      total: totales.total,
      visitaId: selectedRoom.value.VisitaID,
      habitacionId: selectedRoom.value.HabitacionID,
      pausa: Boolean(Pausa.value),
    })

    totalAmount.value = totales.total
    modalPayment.value = true
    isProcessingPayment.value = false
  }

  const handlePaymentConfirmation = (paymentDetails) => {
    console.log('Payment Confirmed:', paymentDetails)
    modalPayment.value = false

    toast.add({
      severity: 'success',
      summary: 'Pago Confirmado',
      detail: 'El pago se procesó exitosamente. Habitación liberada.',
      life: 10000,
    })

    setTimeout(() => {
      window.location.reload()
    }, 1500)
  }

  // Initialize component
  onMounted(() => {
    console.log('Room data (new format):', props.room)

    initializeRoom()
    actualizarConsumos()
    getTimerUpdateInterval()
    startTimerIntervals()
  })
</script>

<style scoped>
  .timer {
    text-align: center;
  }

  .time {
    font-size: 24px;
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
    transition: all 0.3s cubic-bezier(0.52, 0.02, 0.19, 1.02) 0.1s;
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
      text-shadow:
        0 0 5px #ff1cc3,
        0 0 10px #ff1cc3;
    }

    100% {
      text-shadow:
        0 0 10px #ff1cc3,
        0 0 20px #ff1cc3;
    }
  }
</style>
