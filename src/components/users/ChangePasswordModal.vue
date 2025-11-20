<template>
  <Teleport to="body">
    <Transition name="modal-outer" appear>
      <div
        v-if="show"
        class="fixed inset-0 z-50 flex items-start justify-center p-4 bg-black/50 backdrop-blur-sm overflow-y-auto"
      >
        <Transition name="modal-inner">
          <div
            class="w-full max-w-lg bg-white/10 backdrop-blur-xl border border-yellow-500/30 rounded-3xl shadow-2xl overflow-hidden my-8"
          >
            <!-- Header -->
            <div class="relative p-8 bg-gradient-to-r from-yellow-600/20 via-orange-600/20 to-yellow-600/20 border-b border-yellow-500/20">
              <button
                type="button"
                @click="$emit('close')"
                class="absolute top-4 right-4 w-10 h-10 bg-red-500/20 hover:bg-red-500/30 border border-red-500/30 hover:border-red-500/50 rounded-xl text-red-400 hover:text-red-300 transition-all duration-300 transform hover:scale-110"
              >
                <i class="fas fa-times"></i>
              </button>

              <div class="text-center">
                <div class="w-16 h-16 bg-gradient-to-r from-yellow-400 to-orange-500 rounded-2xl flex items-center justify-center mx-auto mb-4 shadow-lg">
                  <i class="fas fa-key text-white text-xl"></i>
                </div>
                <h2 class="text-3xl font-bold text-white lexend-exa">
                  Cambiar Contraseña
                </h2>
                <p class="text-gray-300 mt-2">
                  Usuario: <span class="font-bold text-white">{{ user?.userName || user?.username }}</span>
                </p>
              </div>
            </div>

            <!-- Form content -->
            <div class="p-8 max-h-[60vh] overflow-y-auto custom-scrollbar">
              <form
                class="space-y-6"
                @submit.prevent="handleSubmit"
              >
                <!-- Current password (if needed) -->
                <div v-if="showCurrentPassword">
                  <label class="block mb-3 text-sm font-medium text-gray-300">
                    <i class="fas fa-lock mr-2 text-gray-400"></i>
                    Contraseña Actual
                  </label>
                  <div class="relative">
                    <input
                      v-model="currentPasswordInput"
                      :type="showCurrentPasswordField ? 'text' : 'password'"
                      class="w-full p-4 pr-12 rounded-2xl bg-white/5 backdrop-blur-md border border-white/10 text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-yellow-500 focus:border-transparent transition-all duration-300"
                      :class="{ 'border-red-500 focus:ring-red-500': errors.currentPassword }"
                      placeholder="••••••••"
                      autocomplete="current-password"
                    />
                    <button
                      type="button"
                      @click="showCurrentPasswordField = !showCurrentPasswordField"
                      class="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 hover:text-white transition-colors duration-300"
                    >
                      <i :class="showCurrentPasswordField ? 'fas fa-eye-slash' : 'fas fa-eye'"></i>
                    </button>
                  </div>
                  <p v-if="errors.currentPassword" class="text-red-400 text-sm mt-2 flex items-center">
                    <i class="fas fa-exclamation-circle mr-1"></i>{{ errors.currentPassword }}
                  </p>
                </div>

                <!-- New password -->
                <div>
                  <label class="block mb-3 text-sm font-medium text-gray-300">
                    <i class="fas fa-key mr-2 text-yellow-400"></i>
                    Nueva Contraseña
                  </label>
                  <div class="relative">
                    <input
                      v-model="form.newPassword"
                      :type="showNewPasswordField ? 'text' : 'password'"
                      class="w-full p-4 pr-12 rounded-2xl bg-white/5 backdrop-blur-md border border-white/10 text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-yellow-500 focus:border-transparent transition-all duration-300"
                      :class="{ 'border-red-500 focus:ring-red-500': errors.newPassword }"
                      placeholder="••••••••"
                      autocomplete="new-password"
                    />
                    <button
                      type="button"
                      @click="showNewPasswordField = !showNewPasswordField"
                      class="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 hover:text-white transition-colors duration-300"
                    >
                      <i :class="showNewPasswordField ? 'fas fa-eye-slash' : 'fas fa-eye'"></i>
                    </button>
                  </div>
                  <p v-if="errors.newPassword" class="text-red-400 text-sm mt-2 flex items-center">
                    <i class="fas fa-exclamation-circle mr-1"></i>{{ errors.newPassword }}
                  </p>
                  
                  <!-- Password requirements -->
                  <div class="mt-4 p-4 rounded-2xl bg-white/5 backdrop-blur-md border border-white/10">
                    <p class="text-gray-300 text-sm font-medium mb-2">
                      <i class="fas fa-shield-alt mr-2 text-blue-400"></i>
                      Requisitos de seguridad:
                    </p>
                    <ul class="space-y-1 text-sm text-gray-400">
                      <li class="flex items-center">
                        <i class="fas fa-check text-green-400 mr-2 text-xs"></i>
                        Al menos 6 caracteres
                      </li>
                      <li class="flex items-center">
                        <i class="fas fa-check text-green-400 mr-2 text-xs"></i>
                        Una letra minúscula
                      </li>
                      <li class="flex items-center">
                        <i class="fas fa-check text-green-400 mr-2 text-xs"></i>
                        Una letra mayúscula
                      </li>
                      <li class="flex items-center">
                        <i class="fas fa-check text-green-400 mr-2 text-xs"></i>
                        Un número
                      </li>
                    </ul>
                  </div>
                </div>

                <!-- Confirm password -->
                <div>
                  <label class="block mb-3 text-sm font-medium text-gray-300">
                    <i class="fas fa-key mr-2 text-yellow-400"></i>
                    Confirmar Nueva Contraseña
                  </label>
                  <div class="relative">
                    <input
                      v-model="form.confirmPassword"
                      :type="showConfirmPasswordField ? 'text' : 'password'"
                      class="w-full p-4 pr-12 rounded-2xl bg-white/5 backdrop-blur-md border border-white/10 text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-yellow-500 focus:border-transparent transition-all duration-300"
                      :class="{ 'border-red-500 focus:ring-red-500': errors.confirmPassword }"
                      placeholder="••••••••"
                      autocomplete="new-password"
                    />
                    <button
                      type="button"
                      @click="showConfirmPasswordField = !showConfirmPasswordField"
                      class="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 hover:text-white transition-colors duration-300"
                    >
                      <i :class="showConfirmPasswordField ? 'fas fa-eye-slash' : 'fas fa-eye'"></i>
                    </button>
                  </div>
                  <p v-if="errors.confirmPassword" class="text-red-400 text-sm mt-2 flex items-center">
                    <i class="fas fa-exclamation-circle mr-1"></i>{{ errors.confirmPassword }}
                  </p>
                </div>

                <!-- Error display -->
                <div v-if="apiError" class="p-4 rounded-2xl bg-red-500/10 border border-red-500/20 backdrop-blur-md">
                  <div class="flex items-center gap-3">
                    <i class="fas fa-exclamation-triangle text-red-400"></i>
                    <p class="text-red-300">{{ apiError }}</p>
                  </div>
                </div>
              </form>
            </div>

            <!-- Footer with actions -->
            <div class="px-8 py-6 bg-white/5 border-t border-white/10 flex justify-end space-x-4">
              <button
                type="button"
                @click="$emit('close')"
                class="px-6 py-3 bg-gray-600/20 hover:bg-gray-600/30 border border-gray-600/30 hover:border-gray-600/50 rounded-2xl text-gray-300 hover:text-white font-medium transition-all duration-300 transform hover:scale-105"
              >
                <i class="fas fa-times mr-2"></i>
                Cancelar
              </button>
              <button
                type="submit"
                @click="handleSubmit"
                :disabled="loading || !isFormValid"
                class="px-6 py-3 bg-gradient-to-r from-yellow-500 to-orange-500 hover:from-yellow-600 hover:to-orange-600 rounded-2xl text-white font-medium transition-all duration-300 transform hover:scale-105 disabled:opacity-50 disabled:cursor-not-allowed disabled:transform-none shadow-lg hover:shadow-xl"
              >
                <i v-if="loading" class="fas fa-spinner fa-spin mr-2"></i>
                <i v-else class="fas fa-key mr-2"></i>
                {{ loading ? 'Cambiando...' : 'Cambiar Contraseña' }}
              </button>
            </div>
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { ref, watch, computed } from 'vue';
import { useUserValidation } from '../../composables/useUserValidation';
import { useAuthStore } from '../../store/auth';

const props = defineProps({
  show: {
    type: Boolean,
    default: false
  },
  user: {
    type: Object,
    default: null
  },
  loading: {
    type: Boolean,
    default: false
  },
  apiError: {
    type: String,
    default: null
  }
});

const emit = defineEmits(['close', 'submit']);

const authStore = useAuthStore();
const { errors, hasErrors, validatePasswordChange, clearErrors } = useUserValidation();

const form = ref({
  newPassword: '',
  confirmPassword: ''
});

// Separate ref for current password
const currentPasswordInput = ref('');

// State for password visibility
const showCurrentPasswordField = ref(false);
const showNewPasswordField = ref(false);
const showConfirmPasswordField = ref(false);

const showCurrentPassword = computed(() => {
  return props.user?.id === authStore.user?.id;
});

const isFormValid = computed(() => {
  // Verificar que los campos requeridos estén completos
  const hasRequiredFields = form.value.newPassword && form.value.confirmPassword;
  
  // Si se requiere contraseña actual, verificar que esté presente
  if (showCurrentPassword.value) {
    return hasRequiredFields && currentPasswordInput.value && !hasErrors.value;
  }
  
  return hasRequiredFields && !hasErrors.value;
});

watch(() => props.show, (newVal) => {
  if (newVal) {
    resetForm();
  } else {
    clearErrors();
  }
});

// Watch form changes to validate in real time
watch(() => [form.value.newPassword, form.value.confirmPassword, currentPasswordInput.value], () => {
  // Solo validar si el usuario ha empezado a escribir en algún campo
  if (form.value.newPassword || form.value.confirmPassword || currentPasswordInput.value) {
    const formToValidate = {
      ...form.value,
      ...(showCurrentPassword.value && { currentPassword: currentPasswordInput.value })
    };
    validatePasswordChange(formToValidate);
  }
}, { immediate: false });

const handleSubmit = () => {
  const formToValidate = {
    ...form.value,
    ...(showCurrentPassword.value && { currentPassword: currentPasswordInput.value })
  };
  
  const isValid = validatePasswordChange(formToValidate);
  
  if (isValid) {
    const passwordData = {
      ...form.value,
      userId: props.user?.id
    };
    
    if (showCurrentPassword.value) {
      passwordData.currentPassword = currentPasswordInput.value;
    }
    
    emit('submit', passwordData);
  }
};

const resetForm = () => {
  form.value = {
    newPassword: '',
    confirmPassword: ''
  };
  currentPasswordInput.value = '';
  // Reset password visibility
  showCurrentPasswordField.value = false;
  showNewPasswordField.value = false;
  showConfirmPasswordField.value = false;
  clearErrors();
};
</script>

<style scoped>
/* Modal animations */
.modal-outer-enter-active,
.modal-outer-leave-active {
  transition: all 0.3s ease;
}

.modal-outer-enter-from,
.modal-outer-leave-to {
  opacity: 0;
}

.modal-inner-enter-active {
  transition: all 0.4s cubic-bezier(0.34, 1.56, 0.64, 1);
}

.modal-inner-leave-active {
  transition: all 0.3s ease;
}

.modal-inner-enter-from {
  transform: scale(0.8) translateY(-20px);
  opacity: 0;
}

.modal-inner-leave-to {
  transform: scale(0.9) translateY(20px);
  opacity: 0;
}

/* Enhanced glassmorphism effects */
.backdrop-blur-xl {
  backdrop-filter: blur(20px);
}

.backdrop-blur-md {
  backdrop-filter: blur(12px);
}
</style>