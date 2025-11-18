<template>
  <Teleport to="body">
    <Transition name="modal-outer" appear>
      <div
        v-if="show"
        class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm"
      >
        <Transition name="modal-inner">
          <div
            class="w-full max-w-lg bg-white/10 backdrop-blur-xl border border-red-500/30 rounded-3xl shadow-2xl overflow-hidden"
          >
            <!-- Header -->
            <div class="relative p-8 bg-gradient-to-r from-red-600/20 via-orange-600/20 to-red-600/20 border-b border-red-500/20">
              <div class="text-center">
                <div class="w-20 h-20 bg-gradient-to-r from-red-400 to-red-600 rounded-full flex items-center justify-center mx-auto mb-4 shadow-lg">
                  <i class="fas fa-exclamation-triangle text-white text-3xl"></i>
                </div>
                <h2 class="text-3xl font-bold text-white mb-2 lexend-exa">
                  Confirmar Eliminación
                </h2>
                <p class="text-gray-300">
                  Esta acción es permanente e irreversible
                </p>
              </div>
            </div>

            <!-- Content -->
            <div class="p-8">
              <!-- User info -->
              <div class="mb-6 p-6 rounded-2xl bg-red-500/10 border border-red-500/20 backdrop-blur-md">
                <div class="flex items-center gap-4 mb-4">
                  <div class="w-12 h-12 bg-gradient-to-r from-primary-400 to-secondary-400 rounded-2xl flex items-center justify-center shadow-lg">
                    <i class="fas fa-user text-white"></i>
                  </div>
                  <div>
                    <p class="text-white font-bold text-lg">
                      {{ user?.userName || user?.username || 'Usuario' }}
                    </p>
                    <p v-if="user?.email" class="text-gray-300 text-sm">
                      {{ user.email }}
                    </p>
                  </div>
                </div>
                
                <div class="text-center">
                  <p class="text-red-300 font-medium">
                    ¿Estás seguro que deseas eliminar este usuario?
                  </p>
                </div>
              </div>

              <!-- Warning messages -->
              <div class="space-y-4 mb-6">
                <div class="bg-yellow-500/10 border border-yellow-500/20 rounded-2xl p-4 backdrop-blur-md">
                  <div class="flex items-start gap-3">
                    <i class="fas fa-info-circle text-yellow-400 mt-1"></i>
                    <div>
                      <p class="text-yellow-300 font-medium text-sm">Información importante</p>
                      <p class="text-yellow-200/80 text-sm mt-1">
                        Se eliminarán todos los datos asociados a este usuario y no podrá recuperarlos posteriormente.
                      </p>
                    </div>
                  </div>
                </div>

                <div v-if="apiError" class="bg-red-500/10 border border-red-500/20 rounded-2xl p-4 backdrop-blur-md">
                  <div class="flex items-center gap-3">
                    <i class="fas fa-exclamation-triangle text-red-400"></i>
                    <p class="text-red-300">{{ apiError }}</p>
                  </div>
                </div>
              </div>

              <!-- Actions -->
              <div class="flex justify-center space-x-4">
                <button
                  type="button"
                  @click="$emit('close')"
                  class="px-6 py-3 bg-gray-600/20 hover:bg-gray-600/30 border border-gray-600/30 hover:border-gray-600/50 rounded-2xl text-gray-300 hover:text-white font-medium transition-all duration-300 transform hover:scale-105"
                >
                  <i class="fas fa-times mr-2"></i>
                  Cancelar
                </button>
                <button
                  type="button"
                  @click="handleDelete"
                  :disabled="loading"
                  class="px-6 py-3 bg-gradient-to-r from-red-500 to-red-600 hover:from-red-600 hover:to-red-700 rounded-2xl text-white font-medium transition-all duration-300 transform hover:scale-105 disabled:opacity-50 disabled:cursor-not-allowed disabled:transform-none shadow-lg hover:shadow-xl"
                >
                  <i v-if="loading" class="fas fa-spinner fa-spin mr-2"></i>
                  <i v-else class="fas fa-trash mr-2"></i>
                  {{ loading ? 'Eliminando...' : 'Eliminar Usuario' }}
                </button>
              </div>
            </div>
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
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

const emit = defineEmits(['close', 'confirm']);

const handleDelete = () => {
  if (props.user) {
    emit('confirm', props.user);
  }
};
</script>

<style scoped>
.modal-outer-enter-active,
.modal-outer-leave-active {
  transition: opacity 0.3s ease;
}

.modal-outer-enter-from,
.modal-outer-leave-to {
  opacity: 0;
}

.modal-inner-enter-active {
  transition: all 0.3s ease;
}

.modal-inner-leave-active {
  transition: all 0.3s ease;
}

.modal-inner-enter-from {
  transform: scale(0.9);
  opacity: 0;
}

.modal-inner-leave-to {
  transform: scale(1.1);
  opacity: 0;
}
</style>