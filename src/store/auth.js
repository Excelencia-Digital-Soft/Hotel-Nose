import { defineStore } from "pinia";
import axiosClient from '../axiosClient';

export const useAuthStore = defineStore('auth', {
  state: () => {
    return {
      auth: null,
      institucionID: null, // Institución seleccionada
      instituciones: [], // Lista de instituciones disponibles
    };
  },
  persist: true,
  getters: {},
  actions: {
    async login(credentials) {
      try {
        const res = await axiosClient.post('/api/Usuarios/Login', credentials);
        this.auth = res.data;

        // Guardar la lista de instituciones en el store
        this.instituciones = res.data.instituciones;

        // Si solo hay una institución, asignarla automáticamente
        if (this.instituciones.length === 1) {
          this.institucionID = this.instituciones[0].institucionId;
        }

        return true;
      } catch (error) {
        console.error(error);
        return false;
      }
    },

    // Nueva acción para seleccionar una institución
    selectInstitucion(id) {
      this.institucionID = id;
    },

    logOut() {
      this.$reset(); // Restablece el estado a su valor inicial
    },
  },
});