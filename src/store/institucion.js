import { defineStore } from 'pinia';
import { useAuthStore } from './authStore';

export const useHotelStore = defineStore('hotel', {
  state: () => ({
    hotelData: null, // Información del hotel
  }),
  actions: {
    async fetchHotelData() {
      const authStore = useAuthStore();
      if (!authStore.idInstitucion) return;

      // Obtener la información específica del hotel según el idInstitucion
      const response = await fetch(`/api/hotel/${authStore.idInstitucion}`);
      this.hotelData = await response.json();
    }
  }
});
