<template>
  <Teleport to="body">
    <!-- PrimeVue Toast for notifications -->
    <Toast position="top-right" />

    <Transition
      enter-active-class="transition-all duration-300 ease-out"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-all duration-200 ease-in"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div
        v-if="visible"
        class="fixed inset-0 overflow-y-auto bg-black/60 backdrop-blur-xl z-50 flex items-center justify-center p-4"
      >
        <Transition
          enter-active-class="transition-all duration-300 ease-out"
          enter-from-class="opacity-0 scale-95"
          enter-to-class="opacity-100 scale-100"
          leave-active-class="transition-all duration-200 ease-in"
          leave-from-class="opacity-100 scale-100"
          leave-to-class="opacity-0 scale-95"
        >
          <div
            v-if="visible"
            class="bg-white/5 backdrop-blur-2xl border border-white/20 rounded-3xl p-8 w-full max-w-md relative overflow-hidden"
          >
            <!-- Background glow effect -->
            <div
              class="absolute w-96 h-96 bg-gradient-to-r from-red-400 via-pink-400 to-purple-400 opacity-20 blur-[100px] -left-48 -top-48"
            ></div>
            <div
              class="absolute w-96 h-96 bg-gradient-to-r from-purple-400 via-indigo-400 to-blue-400 opacity-20 blur-[100px] -right-48 -bottom-48"
            ></div>

            <!-- Content -->
            <div class="relative z-10">
              <!-- Header -->
              <div class="flex items-center justify-between mb-6">
                <h3 class="text-2xl font-bold text-white">Anular Ocupación</h3>
                <button
                  @click="$emit('close-modal')"
                  class="group relative w-10 h-10 flex items-center justify-center rounded-full bg-white/10 hover:bg-white/20 backdrop-blur-sm border border-white/30 transition-all duration-300"
                >
                  <svg
                    class="w-5 h-5 text-white group-hover:rotate-90 transition-transform duration-300"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M6 18L18 6M6 6l12 12"
                    />
                  </svg>
                </button>
              </div>

              <!-- Warning Icon -->
              <div class="flex justify-center mb-6">
                <div class="relative">
                  <div
                    class="absolute inset-0 bg-gradient-to-r from-red-400 to-pink-400 blur-xl opacity-50"
                  ></div>
                  <div
                    class="relative w-20 h-20 bg-gradient-to-r from-red-400 to-pink-400 rounded-full flex items-center justify-center"
                  >
                    <svg
                      class="w-10 h-10 text-white"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
                      />
                    </svg>
                  </div>
                </div>
              </div>

              <!-- Warning Message -->
              <div
                class="bg-red-500/10 backdrop-blur-sm border border-red-500/30 rounded-xl p-4 mb-6"
              >
                <p class="text-white text-center">
                  <span class="font-semibold">¡Atención!</span> Al anular esta habitación también se
                  anularán todos los consumos asociados a la visita.
                </p>
                <p class="text-white/80 text-center text-sm mt-2">¿Estás seguro de continuar?</p>
              </div>

              <!-- Room Info -->
              <div
                v-if="habitacion"
                class="bg-white/10 backdrop-blur-sm border border-white/30 rounded-xl p-4 mb-6"
              >
                <div class="flex items-center justify-between">
                  <span class="text-white/60 text-sm">Habitación</span>
                  <span class="text-white font-semibold">{{
                    habitacion.NombreHabitacion || habitacion.nombreHabitacion
                  }}</span>
                </div>
                <div v-if="habitacion.ReservaID" class="flex items-center justify-between mt-2">
                  <span class="text-white/60 text-sm">ID Reserva</span>
                  <span class="text-white font-semibold">#{{ habitacion.ReservaID }}</span>
                </div>
                <div v-if="habitacion.VisitaID" class="flex items-center justify-between mt-2">
                  <span class="text-white/60 text-sm">ID Visita</span>
                  <span class="text-white font-semibold">#{{ habitacion.VisitaID }}</span>
                </div>
              </div>

              <!-- Motivo Input -->
              <div class="mb-6">
                <label class="block text-white/80 text-sm font-medium mb-2">
                  Motivo de anulación <span class="text-red-400">*</span>
                </label>
                <textarea
                  v-model="motivo"
                  @input="handleMotivoInput"
                  placeholder="Describa el motivo de la anulación..."
                  :class="[
                    'w-full h-32 p-4 bg-white/10 backdrop-blur-sm border rounded-xl',
                    'text-white placeholder-white/40 resize-none',
                    'transition-all duration-300',
                    'focus:outline-none focus:ring-2 focus:ring-white/30 focus:border-transparent',
                    motivoError ? 'border-red-500/50' : 'border-white/30',
                  ]"
                />
                <div class="flex items-center justify-between mt-2">
                  <p v-if="motivoError" class="text-red-400 text-sm">
                    {{ motivoError }}
                  </p>
                  <p class="text-white/40 text-sm ml-auto">{{ motivo.length }}/500</p>
                </div>
              </div>

              <!-- Action Buttons -->
              <div class="flex gap-3">
                <button
                  @click="$emit('close-modal')"
                  class="flex-1 py-3 px-6 bg-white/10 hover:bg-white/20 backdrop-blur-sm border border-white/30 rounded-xl text-white font-medium transition-all duration-300"
                >
                  Cancelar
                </button>
                <button
                  @click="confirmAnulacion"
                  :disabled="!isValidForm || loading"
                  :class="[
                    'flex-1 py-3 px-6 rounded-xl font-medium transition-all duration-300',
                    'bg-gradient-to-r from-red-400 to-pink-400',
                    'text-white shadow-lg',
                    isValidForm && !loading
                      ? 'hover:from-red-500 hover:to-pink-500 hover:shadow-xl hover:scale-[1.02]'
                      : 'opacity-50 cursor-not-allowed',
                  ]"
                >
                  <span v-if="!loading" class="flex items-center justify-center gap-2">
                    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
                      />
                    </svg>
                    Confirmar Anulación
                  </span>
                  <span v-else class="flex items-center justify-center gap-2">
                    <svg class="animate-spin h-5 w-5" fill="none" viewBox="0 0 24 24">
                      <circle
                        class="opacity-25"
                        cx="12"
                        cy="12"
                        r="10"
                        stroke="currentColor"
                        stroke-width="4"
                      ></circle>
                      <path
                        class="opacity-75"
                        fill="currentColor"
                        d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                      ></path>
                    </svg>
                    Procesando...
                  </span>
                </button>
              </div>
            </div>
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
  import { ref, computed, watch } from 'vue'
  import { useHabitacionAvailability } from '../composables/useHabitacionAvailability'
  import Toast from 'primevue/toast'
  import { AnularOcupacionModalProps, AnularOcupacionModalEmits } from '../types'

  const props = withDefaults(defineProps<AnularOcupacionModalProps>(), {
    visible: false,
    habitacion: null,
  })

  const emit = defineEmits<AnularOcupacionModalEmits>()

  const { loading, motivo, anularOcupacion } = useHabitacionAvailability()

  const motivoError = ref<string>('')

  const isValidForm = computed((): boolean => {
    return motivo.value.trim().length >= 10 && motivo.value.length <= 500
  })

  const handleMotivoInput = (): void => {
    if (motivo.value.length < 10) {
      motivoError.value = 'El motivo debe tener al menos 10 caracteres'
    } else if (motivo.value.length > 500) {
      motivoError.value = 'El motivo no puede exceder 500 caracteres'
    } else {
      motivoError.value = ''
    }
  }

  const confirmAnulacion = async (): Promise<void> => {
    if (!isValidForm.value || !props.habitacion) return

    await anularOcupacion(props.habitacion, () => {
      if (props.habitacion?.HabitacionID) {
        emit('ocupacion-anulada', props.habitacion.HabitacionID)
      }
      emit('close-modal')
    })
  }

  // Reset form when modal is closed
  watch(
    () => props.visible,
    (newVal) => {
      if (!newVal) {
        motivo.value = ''
        motivoError.value = ''
      }
    }
  )
</script>

<style scoped>
  /* Additional custom styles if needed */
</style>

