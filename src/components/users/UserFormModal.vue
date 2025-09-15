<template>
  <Teleport to="body">
    <Transition name="modal-outer" appear>
      <div
        v-if="show"
        class="fixed inset-0 z-50 flex items-start justify-center p-4 bg-black/50 backdrop-blur-sm overflow-y-auto"
      >
        <Transition name="modal-inner">
          <div
            class="w-full max-w-2xl bg-white/10 backdrop-blur-xl border border-white/20 rounded-3xl shadow-2xl overflow-hidden my-8"
          >
            <!-- Header -->
            <div
              class="relative p-8 bg-gradient-to-r from-primary-600/20 via-secondary-600/20 to-accent-600/20 border-b border-white/10"
            >
              <button
                type="button"
                @click="$emit('close')"
                class="absolute top-4 right-4 w-10 h-10 bg-red-500/20 hover:bg-red-500/30 border border-red-500/30 hover:border-red-500/50 rounded-xl text-red-400 hover:text-red-300 transition-all duration-300 transform hover:scale-110"
              >
                <i class="fas fa-times"></i>
              </button>

              <div class="flex items-center gap-4">
                <div
                  class="w-16 h-16 bg-gradient-to-r from-primary-400 to-secondary-400 rounded-2xl flex items-center justify-center shadow-lg"
                >
                  <i class="fas fa-user-plus text-white text-xl"></i>
                </div>
                <div>
                  <h2 class="text-3xl font-bold text-white lexend-exa">
                    {{ user ? 'Editar Usuario' : 'Crear Usuario' }}
                  </h2>
                  <p class="text-gray-300 text-sm mt-1">
                    {{
                      user
                        ? 'Modificar información del usuario'
                        : 'Agregar nuevo usuario al sistema'
                    }}
                  </p>
                </div>
              </div>
            </div>

            <!-- Form content -->
            <div class="p-8 max-h-[70vh] overflow-y-auto custom-scrollbar">
              <form class="space-y-6" @submit.prevent="handleSubmit">
                <!-- Grid layout for better organization -->
                <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <!-- Username -->
                  <div class="md:col-span-2">
                    <label class="block mb-3 text-sm font-medium text-gray-300">
                      <i class="fas fa-user mr-2 text-primary-400"></i>
                      Nombre de Usuario
                    </label>
                    <input
                      v-model="form.userName"
                      type="text"
                      class="w-full p-4 rounded-2xl bg-white/5 backdrop-blur-md border border-white/10 text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all duration-300"
                      :class="{ 'border-red-500 focus:ring-red-500': errors.username }"
                      placeholder="usuario123"
                      :disabled="!!user"
                    />
                    <p v-if="errors.username" class="text-red-400 text-sm mt-2 flex items-center">
                      <i class="fas fa-exclamation-circle mr-1"></i>{{ errors.username }}
                    </p>
                  </div>

                  <!-- First name -->
                  <div>
                    <label class="block mb-3 text-sm font-medium text-gray-300">
                      <i class="fas fa-id-card mr-2 text-secondary-400"></i>
                      Nombre
                    </label>
                    <input
                      v-model="form.firstName"
                      type="text"
                      class="w-full p-4 rounded-2xl bg-white/5 backdrop-blur-md border border-white/10 text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all duration-300"
                      :class="{ 'border-red-500 focus:ring-red-500': errors.firstName }"
                      placeholder="Juan"
                    />
                    <p v-if="errors.firstName" class="text-red-400 text-sm mt-2 flex items-center">
                      <i class="fas fa-exclamation-circle mr-1"></i>{{ errors.firstName }}
                    </p>
                  </div>

                  <!-- Last name -->
                  <div>
                    <label class="block mb-3 text-sm font-medium text-gray-300">
                      <i class="fas fa-id-card mr-2 text-secondary-400"></i>
                      Apellido
                    </label>
                    <input
                      v-model="form.lastName"
                      type="text"
                      class="w-full p-4 rounded-2xl bg-white/5 backdrop-blur-md border border-white/10 text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all duration-300"
                      :class="{ 'border-red-500 focus:ring-red-500': errors.lastName }"
                      placeholder="Pérez"
                    />
                    <p v-if="errors.lastName" class="text-red-400 text-sm mt-2 flex items-center">
                      <i class="fas fa-exclamation-circle mr-1"></i>{{ errors.lastName }}
                    </p>
                  </div>
                </div>

                <!-- Email (only for new users) -->
                <div v-if="!user" class="md:col-span-2">
                  <label class="block mb-3 text-sm font-medium text-gray-300">
                    <i class="fas fa-envelope mr-2 text-accent-400"></i>
                    Correo Electrónico
                  </label>
                  <input
                    v-model="form.email"
                    type="email"
                    class="w-full p-4 rounded-2xl bg-white/5 backdrop-blur-md border border-white/10 text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all duration-300"
                    :class="{ 'border-red-500 focus:ring-red-500': errors.email }"
                    placeholder="usuario@ejemplo.com"
                  />
                  <p v-if="errors.email" class="text-red-400 text-sm mt-2 flex items-center">
                    <i class="fas fa-exclamation-circle mr-1"></i>{{ errors.email }}
                  </p>
                </div>

                <!-- Password fields (only for new users) -->
                <template v-if="!user">
                  <div>
                    <label class="block mb-3 text-sm font-medium text-gray-300">
                      <i class="fas fa-lock mr-2 text-yellow-400"></i>
                      Contraseña
                    </label>
                    <div class="relative">
                      <input
                        v-model="form.password"
                        :type="showPasswordField ? 'text' : 'password'"
                        class="w-full p-4 pr-12 rounded-2xl bg-white/5 backdrop-blur-md border border-white/10 text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all duration-300"
                        :class="{ 'border-red-500 focus:ring-red-500': errors.password }"
                        placeholder="••••••••"
                        autocomplete="new-password"
                      />
                      <button
                        type="button"
                        @click="showPasswordField = !showPasswordField"
                        class="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 hover:text-white transition-colors duration-300"
                      >
                        <i :class="showPasswordField ? 'fas fa-eye-slash' : 'fas fa-eye'"></i>
                      </button>
                    </div>
                    <p v-if="errors.password" class="text-red-400 text-sm mt-2 flex items-center">
                      <i class="fas fa-exclamation-circle mr-1"></i>{{ errors.password }}
                    </p>
                  </div>

                  <div>
                    <label class="block mb-3 text-sm font-medium text-gray-300">
                      <i class="fas fa-lock mr-2 text-yellow-400"></i>
                      Confirmar Contraseña
                    </label>
                    <div class="relative">
                      <input
                        v-model="form.passwordConfirmation"
                        :type="showPasswordConfirmationField ? 'text' : 'password'"
                        class="w-full p-4 pr-12 rounded-2xl bg-white/5 backdrop-blur-md border border-white/10 text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all duration-300"
                        :class="{
                          'border-red-500 focus:ring-red-500': errors.passwordConfirmation,
                        }"
                        placeholder="••••••••"
                        autocomplete="new-password"
                      />
                      <button
                        type="button"
                        @click="showPasswordConfirmationField = !showPasswordConfirmationField"
                        class="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 hover:text-white transition-colors duration-300"
                      >
                        <i
                          :class="showPasswordConfirmationField ? 'fas fa-eye-slash' : 'fas fa-eye'"
                        ></i>
                      </button>
                    </div>
                    <p
                      v-if="errors.passwordConfirmation"
                      class="text-red-400 text-sm mt-2 flex items-center"
                    >
                      <i class="fas fa-exclamation-circle mr-1"></i
                      >{{ errors.passwordConfirmation }}
                    </p>
                  </div>
                </template>

                <!-- Roles selection (only for editing) -->
                <div v-if="user" class="md:col-span-2">
                  <label class="block mb-3 text-sm font-medium text-gray-300">
                    <i class="fas fa-user-tag mr-2 text-indigo-400"></i>
                    Roles del Usuario
                  </label>
                  <div
                    class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3 p-4 rounded-2xl bg-white/5 backdrop-blur-md border border-white/10 max-h-40 overflow-y-auto custom-scrollbar"
                  >
                    <div
                      v-for="role in roles"
                      :key="role.id"
                      class="flex items-center p-3 rounded-xl bg-white/5 border border-white/10 hover:bg-white/10 transition-all duration-300"
                    >
                      <input
                        :id="`role-${role.id}`"
                        v-model="form.roles"
                        type="checkbox"
                        :value="role.name"
                        class="mr-3 w-4 h-4 text-primary-500 bg-white/10 border-white/20 rounded focus:ring-primary-500 focus:ring-2"
                      />
                      <label
                        :for="`role-${role.id}`"
                        class="text-white text-sm font-medium cursor-pointer flex-1"
                      >
                        {{ role.name }}
                        <span class="block text-xs text-gray-400 mt-1">{{ role.description }}</span>
                      </label>
                    </div>
                  </div>
                  <p v-if="errors.role" class="text-red-400 text-sm mt-2 flex items-center">
                    <i class="fas fa-exclamation-circle mr-1"></i>{{ errors.role }}
                  </p>
                </div>

                <!-- Default role notice for new users -->
                <div v-if="!user" class="md:col-span-2">
                  <div class="p-4 rounded-2xl bg-white/5 backdrop-blur-md border border-white/10">
                    <div class="flex items-center gap-3">
                      <i class="fas fa-info-circle text-blue-400 text-xl"></i>
                      <div>
                        <p class="text-white font-medium">Rol Predeterminado</p>
                        <p class="text-gray-300 text-sm">
                          Se asignará automáticamente el rol de <span class="font-semibold text-primary-400">Mucama</span> al crear el usuario.
                          Podrá cambiar el rol editando el usuario después de crearlo.
                        </p>
                      </div>
                    </div>
                  </div>
                </div>

                <!-- Status toggle -->
                <div class="md:col-span-2">
                  <div
                    class="flex items-center justify-between p-4 rounded-2xl bg-white/5 backdrop-blur-md border border-white/10"
                  >
                    <div class="flex items-center gap-3">
                      <i class="fas fa-toggle-on text-green-400 text-xl"></i>
                      <div>
                        <label for="isActive" class="text-white font-medium cursor-pointer"
                          >Estado del Usuario</label
                        >
                        <p class="text-gray-400 text-sm">
                          Determina si el usuario puede acceder al sistema
                        </p>
                      </div>
                    </div>
                    <label class="relative inline-flex items-center cursor-pointer">
                      <input
                        v-model="form.isActive"
                        type="checkbox"
                        id="isActive"
                        class="sr-only peer"
                      />
                      <div
                        class="w-11 h-6 bg-gray-600 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-primary-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-primary-500"
                      ></div>
                    </label>
                  </div>
                </div>

                <!-- Error display -->
                <div v-if="apiError" class="md:col-span-2">
                  <div
                    class="p-4 rounded-2xl bg-red-500/10 border border-red-500/20 backdrop-blur-md"
                  >
                    <div class="flex items-center gap-3">
                      <i class="fas fa-exclamation-triangle text-red-400"></i>
                      <p class="text-red-300">{{ apiError }}</p>
                    </div>
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
                :disabled="loading || hasErrors"
                class="px-6 py-3 bg-gradient-to-r from-primary-500 to-secondary-500 hover:from-primary-600 hover:to-secondary-600 rounded-2xl text-white font-medium transition-all duration-300 transform hover:scale-105 disabled:opacity-50 disabled:cursor-not-allowed disabled:transform-none shadow-lg hover:shadow-xl"
              >
                <i v-if="loading" class="fas fa-spinner fa-spin mr-2"></i>
                <i v-else :class="user ? 'fas fa-save' : 'fas fa-plus'" class="mr-2"></i>
                {{ user ? 'Actualizar Usuario' : 'Crear Usuario' }}
              </button>
            </div>
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
  import { ref, watch, computed } from 'vue'
  import { useUserValidation } from '../../composables/useUserValidation'
  import { useAuthStore } from '../../store/auth'

  const props = defineProps({
    show: {
      type: Boolean,
      default: false,
    },
    user: {
      type: Object,
      default: null,
    },
    roles: {
      type: Array,
      default: () => [],
    },
    loading: {
      type: Boolean,
      default: false,
    },
    apiError: {
      type: String,
      default: null,
    },
  })

  const emit = defineEmits(['close', 'submit'])

  const authStore = useAuthStore()
  const { errors, hasErrors, validateUserForm, clearErrors } = useUserValidation()

  // State for password visibility
  const showPasswordField = ref(false)
  const showPasswordConfirmationField = ref(false)

  const form = ref({
    userName: '',
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    passwordConfirmation: '',
    roles: ['Mucama'], // Default role for new users
    isActive: true,
    institucionId: authStore.institucionID,
  })

  const resetForm = () => {
    form.value = {
      userName: '',
      firstName: '',
      lastName: '',
      email: '',
      password: '',
      passwordConfirmation: '',
      roles: ['Mucama'], // Default role for new users
      isActive: true,
      institucionId: authStore.institucionID,
    }
    // Reset password visibility
    showPasswordField.value = false
    showPasswordConfirmationField.value = false
    clearErrors()
  }

  watch(
    () => props.user,
    (newUser) => {
      if (newUser) {
        // Los roles vienen como array de strings hasheados
        const roleNames = newUser.roles || []

        form.value = {
          userName: newUser.userName || newUser.username || '',
          firstName: newUser.firstName || '',
          lastName: newUser.lastName || '',
          email: newUser.email || '',
          password: '',
          passwordConfirmation: '',
          roles: [...roleNames], // Clonar el array para evitar referencias
          isActive: newUser.isActive !== undefined ? newUser.isActive : true,
          institucionId: authStore.institucionID,
        }
      } else {
        resetForm()
      }
    },
    { immediate: true }
  )

  watch(
    () => props.show,
    (newVal) => {
      if (!newVal) {
        clearErrors()
      }
    }
  )

  // Clear specific field errors when user types
  watch(
    () => form.value.firstName,
    () => {
      if (errors.value.firstName) {
        delete errors.value.firstName
      }
    }
  )

  watch(
    () => form.value.lastName,
    () => {
      if (errors.value.lastName) {
        delete errors.value.lastName
      }
    }
  )

  watch(
    () => form.value.userName,
    () => {
      if (errors.value.username) {
        delete errors.value.username
      }
    }
  )

  watch(
    () => form.value.email,
    () => {
      if (errors.value.email) {
        delete errors.value.email
      }
    }
  )

  watch(
    () => form.value.password,
    () => {
      if (errors.value.password) {
        delete errors.value.password
      }
    }
  )

  watch(
    () => form.value.passwordConfirmation,
    () => {
      if (errors.value.passwordConfirmation) {
        delete errors.value.passwordConfirmation
      }
    }
  )

  watch(
    () => form.value.roles,
    () => {
      if (errors.value.role) {
        delete errors.value.role
      }
    },
    { deep: true }
  )

  const handleSubmit = () => {
    const isValid = validateUserForm(form.value, !!props.user)

    if (isValid) {
      if (props.user) {
        // For editing, send all editable fields
        emit('submit', {
          userId: props.user.id,
          firstName: form.value.firstName,
          lastName: form.value.lastName,
          roles: form.value.roles,
          isActive: form.value.isActive,
        })
      } else {
        // For creating, send full user data
        const userData = {
          userName: form.value.userName,
          firstName: form.value.firstName,
          lastName: form.value.lastName,
          email: form.value.email,
          password: form.value.password,
          confirmPassword: form.value.passwordConfirmation,
          roles: form.value.roles,
          institucionId: authStore.institucionID,
        }
        emit('submit', userData)
      }
    }
  }
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

  /* Custom scrollbar */
  .custom-scrollbar::-webkit-scrollbar {
    width: 6px;
  }

  .custom-scrollbar::-webkit-scrollbar-track {
    background: rgba(255, 255, 255, 0.05);
    border-radius: 10px;
  }

  .custom-scrollbar::-webkit-scrollbar-thumb {
    background: linear-gradient(45deg, rgba(168, 85, 247, 0.5), rgba(236, 72, 153, 0.5));
    border-radius: 10px;
  }

  .custom-scrollbar::-webkit-scrollbar-thumb:hover {
    background: linear-gradient(45deg, rgba(168, 85, 247, 0.8), rgba(236, 72, 153, 0.8));
  }
</style>

