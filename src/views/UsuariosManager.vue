<template>
  <div class="p-4">
    <div alt="MOTEL X" class="h-auto mb-4 text-xs flex bg-gradient-to-l from-accent-200 via-secondary-500 to-primary-300 bg bg-clip-text text-transparent lexend-exa md:text-3xl font-bold"
          >Gestión de Usuarios
        </div>
    
    <button v-if="authStore.auth.rol === 1" @click="openModal(null)" class="mb-4 p-2 btn-third text-white rounded">
      Agregar Usuario<span class="ml-2 material-symbols-outlined">person_add</span>
    </button>
    
    <table class="w-full border-collapse border border-gray-300">
      <thead>
        <tr class="bg-gray-200">
          <th class="border p-2">ID</th>
          <th class="border p-2">Nombre</th>
          <th class="border p-2">Rol</th>
          <th v-if="authStore.auth.rol === 1" class="border p-2">Acciones</th>
        </tr>
      </thead>
      <tbody class="text-white">
        <tr v-for="usuario in usuarios" :key="usuario.usuarioId">
          <td class="border p-2">{{ usuario.usuarioId }}</td>
          <td class="border p-2">{{ usuario.nombre }}</td>
          <td class="border p-2">{{ usuario.nombreRol }}</td>
          <td v-if="authStore.auth.rol === 1" class="border flex p-2">
            <button @click="openModal(usuario)" class="p-1 btn-secondary text-white rounded flex items-center">Editar<span class=" material-symbols-outlined">edit</span></button>
            <button @click="deleteUsuario(usuario.usuarioId)" class="p-1 btn-danger text-white rounded ml-2 flex items-center">Eliminar <span class="text-md material-symbols-outlined">delete</span></button>
          </td>
        </tr>
      </tbody>
    </table>
    <UserForm v-if="showModal" :usuario="selectedUsuario" :roles="roles" @close="closeModal" @refresh="fetchUsuarios" />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import axiosClient from '../axiosClient';
import UserForm from '../components/UserForm.vue'
import { useAuthStore } from '../store/auth.js';

const authStore = useAuthStore();
const usuarios = ref([]);
const roles = ref([]);
const showModal = ref(false);
const selectedUsuario = ref(null);

const fetchUsuarios = async () => {
  try {
    const response = await axiosClient.get(`/api/Usuarios/GetUsuarios?InstitucionID=${InstitucionID.value}`);
    usuarios.value = response.data;
    console.log(usuarios.value)
  } catch (error) {
    console.error('Error al obtener usuarios:', error);
  }
};

const fetchRoles = async () => {
  try {
    const response = await axiosClient.get('/api/Usuarios/GetRoles');
    roles.value = response.data;
  } catch (error) {
    console.error('Error al obtener roles:', error);
  }
};

const deleteUsuario = async (id) => {
  if (!confirm('¿Seguro que deseas eliminar este usuario?')) return;
  try {
    await axiosClient.delete(`/api/Usuarios/BorrarUsuario?id=${id}`);
    fetchUsuarios();
  } catch (error) {
    console.error('Error al eliminar usuario:', error);
  }
};

const openModal = (usuario) => {
  selectedUsuario.value = usuario;
  showModal.value = true;
};

const closeModal = () => {
  showModal.value = false;
  selectedUsuario.value = null;
};

onMounted(() => {
  getDatosLogin();
  fetchUsuarios();
  fetchRoles();
});
const InstitucionID = ref(null)
function getDatosLogin(){
    InstitucionID.value = authStore.institucionID;
  }
  
</script>
