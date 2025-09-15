<template>
  <Teleport to="body" class="overflow-hidden">
    <Transition name="modal-outer" appear>
      <div
        class="fixed w-full h-full overflow-auto z-20 bg-black bg-opacity-30 top-0 left-0 flex justify-center px-8"
      >
        <Transition name="modal-inner">
          <div
            class="w-1/3 h-auto fixed top-40 flex flex-col justify-evenly items-center p-8 pb-16 rounded-3xl self-start mt-2 border-x-8 border-y-2 border-accent-500 bg-neutral-800"
          >
            <h2 class="text-xl font-bold mb-4 text-white">
              {{ usuario ? 'Editar Usuario' : 'Agregar Usuario' }}
            </h2>
            <form
              class="flex flex-col items-center justify-center w-full"
              @submit.prevent="saveUsuario"
            >
              <label v-if="!usuario" class="block mb-2 text-white"
                >Nombre de Usuario:</label
              >
              <input
                v-if="!usuario"
                v-model="form.nombreUsuario"
                type="text"
                class="w-full p-2 border rounded mb-4"
                required
              />

              <label v-if="!usuario" class="block mb-2 text-white"
                >Contraseña:</label
              >
              <input
                v-if="!usuario"
                v-model="form.contraseña"
                type="password"
                class="w-full p-2 border rounded mb-4"
                autocomplete="new-password"
                required
              />

              <label class="block mb-2 text-white">Rol:</label>
              <select
                v-model="form.rolId"
                class="w-full p-2 border rounded mb-4"
                required
              >
                <option
                  v-for="rol in roles"
                  :key="rol.rolId"
                  :value="rol.rolId"
                >
                  {{ rol.nombreRol }}
                </option>
              </select>

              <div class="flex w-full justify-center mt-4">
                <button
                  type="button"
                  @click="$emit('close')"
                  class="absolute top-2 right-2 w-10 h-10 btn-danger text-white rounded-full  font-bold"
                >
                  x
                </button>
                <button
                  type="button"
                  @click="$emit('close')"
                  class="w-full py-2  btn-danger text-white rounded mr-2"
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  class="w-full py-2  btn-secondary text-white rounded"
                >
                  Guardar
                </button>
              </div>
            </form>
            <!-- Mensaje de error -->
            <p v-if="errorMessage" class="text-red-500">{{ errorMessage }}</p>
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { ref, watch, onMounted } from 'vue';
import axiosClient from '../axiosClient';
import { useAuthStore } from '../store/auth.js';
const authStore = useAuthStore();

const props = defineProps({ usuario: Object, roles: Array });
const emit = defineEmits(['close', 'refresh']);

const form = ref({
  nombreUsuario: '',
  contraseña: '',
  rolId: null,
});

// Nuevo estado para el mensaje de error
const errorMessage = ref(null);

watch(
  () => props.usuario,
  (newUsuario) => {
    if (newUsuario) {
      form.value = {
        nombreUsuario: newUsuario.nombreUsuario,
        rolId: newUsuario.rolId,
      };
    } else {
      form.value = { nombreUsuario: '', contraseña: '', rolId: null };
    }
  },
  { immediate: true }
);

const saveUsuario = async () => {
  try {
    // Limpiar el mensaje de error antes de intentar guardar
    errorMessage.value = null;

    if (props.usuario) {
      await axiosClient.put(
        `/api/Usuarios/ActualizarUsuario?id=${props.usuario.usuarioId}&RolID=${form.value.rolId}`
      );
    } else {
      await axiosClient.post(
        `/api/Usuarios/CrearUsuario?InstitucionID=${InstitucionID.value}`,
        form.value
      );
    }
    emit('refresh');
    emit('close');
  } catch (error) {
    console.error('Error al guardar usuario:', error);
    // Verificar si el error es por nombre de usuario duplicado (puedes necesitar ajustar esto según la estructura de tu error)
    if (error.response && error.response.status === 400 && error.response.data && error.response.data.message.includes('nombre de usuario')) {
      errorMessage.value =
        'Error al crear usuario, prueba con otro nombre. Si el error persiste, prueba más tarde';
    } else {
      errorMessage.value = 'Error al crear usuario, prueba con otro nombre. Si el error persiste, prueba más tarde.';
    }
  }
};
onMounted(() => {
  console.log();
  getDatosLogin();
});
const InstitucionID = ref(null);
function getDatosLogin() {
  InstitucionID.value = authStore.institucionID;
}
</script>
