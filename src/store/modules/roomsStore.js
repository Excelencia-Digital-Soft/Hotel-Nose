/**
 * Rooms Store Module
 * Manages rooms state using Pinia
 */

import { defineStore } from 'pinia';
import { roomsService } from '../../services/roomsService';
import { useAuthStore } from '../auth';

export const useRoomsStore = defineStore('rooms', {
  state: () => ({
    // Room collections
    allRooms: [],
    freeRooms: [],
    occupiedRooms: [],
    maintenanceRooms: [],
    
    // Loading states
    isLoading: false,
    isRefreshing: false,
    
    // Error handling
    errors: [],
    lastError: null,
    
    // UI state
    selectedRoom: null,
    filters: {
      searchTerm: '',
      category: '',
      showOnlyOccupied: false
    },
    viewSettings: {
      mode: 'grid', // 'grid' | 'list'
      compactMode: false
    },
    
    // Update tracking
    lastUpdated: null,
    updateInterval: null
  }),

  getters: {
    /**
     * Get filtered free rooms based on current filters
     */
    filteredFreeRooms: (state) => {
      const { searchTerm, category } = state.filters;
      
      if (!searchTerm && !category) {
        return state.freeRooms;
      }
      
      return state.freeRooms.filter(room => {
        const matchesSearch = !searchTerm || 
          room.nombreHabitacion.toLowerCase().includes(searchTerm.toLowerCase());
        const matchesCategory = !category || 
          room.nombreHabitacion.includes(category);
        
        return matchesSearch && matchesCategory;
      });
    },

    /**
     * Get filtered occupied rooms based on current filters
     */
    filteredOccupiedRooms: (state) => {
      const { searchTerm, category, showOnlyOccupied } = state.filters;
      
      // If "show only occupied" is false, return all occupied rooms
      if (!showOnlyOccupied) {
        return state.occupiedRooms;
      }
      
      // Apply search and category filters
      return state.occupiedRooms.filter(room => {
        const matchesSearch = !searchTerm || 
          room.nombreHabitacion.toLowerCase().includes(searchTerm.toLowerCase());
        const matchesCategory = !category || 
          room.nombreHabitacion.includes(category);
        
        return matchesSearch && matchesCategory;
      });
    },

    /**
     * Get rooms that are about to expire (within 15 minutes)
     */
    roomsAboutToExpire: (state) => {
      return state.occupiedRooms.filter(room => {
        if (!room.reservaActiva) return false;
        
        const timeLeft = state.getTimeLeftInMinutes(room);
        return timeLeft > 0 && timeLeft <= 15;
      });
    },

    /**
     * Get room statistics
     */
    roomStats: (state) => ({
      total: state.allRooms.length,
      free: state.freeRooms.length,
      occupied: state.occupiedRooms.length,
      maintenance: state.maintenanceRooms.length,
      aboutToExpire: state.roomsAboutToExpire.length,
      occupancyRate: state.allRooms.length > 0 
        ? Math.round((state.occupiedRooms.length / state.allRooms.length) * 100)
        : 0,
      availabilityRate: state.allRooms.length > 0 
        ? Math.round(((state.freeRooms.length + state.maintenanceRooms.length) / state.allRooms.length) * 100) 
        : 0
    }),

    /**
     * Check if there are any errors
     */
    hasErrors: (state) => state.errors.length > 0,

    /**
     * Get the most recent error
     */
    currentError: (state) => state.lastError
  },

  actions: {
    /**
     * Fetch all rooms from the API
     */
    async fetchRooms() {
      const authStore = useAuthStore();
      const institutionId = authStore.institucionID;
      
      if (!institutionId) {
        this.addError('Institution ID not available');
        return false;
      }

      this.isLoading = true;
      this.clearErrors();

      try {
        const result = await roomsService.fetchRooms(institutionId);
        
        if (result.isSuccess) {
          this.setRooms(result.data);
          this.lastUpdated = new Date();
          return true;
        } else {
          this.addError(result.message);
          result.errors.forEach(error => this.addError(error));
          return false;
        }
      } catch (error) {
        this.addError('Unexpected error while fetching rooms');
        console.error('Store error:', error);
        return false;
      } finally {
        this.isLoading = false;
      }
    },

    /**
     * Fetch rooms with conditional loading for better performance
     * @param {Object} options - Loading options
     * @param {boolean} options.loadFreeOnly - Load only free rooms
     * @param {boolean} options.loadOccupiedOnly - Load only occupied rooms
     * @param {boolean} options.parallel - Load both types in parallel
     */
    async fetchRoomsConditional(options = {}) {
      const authStore = useAuthStore();
      const institutionId = authStore.institucionID;
      
      if (!institutionId) {
        this.addError('Institution ID not available');
        return false;
      }

      this.isLoading = true;
      this.clearErrors();
      
      try {
        if (options.loadFreeOnly) {
          // Load only free rooms with minimal data (70% reduction)
          const result = await roomsService.fetchFreeRooms(institutionId);
          if (result.isSuccess) {
            this.freeRooms = result.data;
            this.allRooms = [...this.freeRooms, ...this.maintenanceRooms, ...this.occupiedRooms];
            this.lastUpdated = new Date();
            console.log('🏨 Free rooms loaded optimized:', this.freeRooms.length);
            return true;
          } else {
            this.addError(result.message);
            result.errors.forEach(error => this.addError(error));
            return false;
          }
        } else if (options.loadOccupiedOnly) {
          // Load only occupied rooms with optimized data (40% reduction)
          const result = await roomsService.fetchOccupiedRooms(institutionId);
          if (result.isSuccess) {
            this.occupiedRooms = result.data;
            this.allRooms = [...this.freeRooms, ...this.maintenanceRooms, ...this.occupiedRooms];
            this.lastUpdated = new Date();
            console.log('🏨 Occupied rooms loaded optimized:', this.occupiedRooms.length);
            return true;
          } else {
            this.addError(result.message);
            result.errors.forEach(error => this.addError(error));
            return false;
          }
        } else if (options.parallel) {
          // Load both types in parallel for maximum performance
          console.log('🏨 Loading rooms in parallel...');
          const [freeResult, occupiedResult] = await Promise.all([
            roomsService.fetchFreeRooms(institutionId),
            roomsService.fetchOccupiedRooms(institutionId)
          ]);
          
          let success = true;
          
          if (freeResult.isSuccess) {
            // Re-categorize free rooms data to separate free from maintenance
            const allFreeRooms = freeResult.data;
            this.freeRooms = allFreeRooms.filter(room => !room.estado || room.estado.toLowerCase() === 'disponible');
            this.maintenanceRooms = allFreeRooms.filter(room => room.estado && room.estado.toLowerCase() !== 'disponible');
            console.log('🏨 Free rooms loaded:', this.freeRooms.length);
            console.log('🏨 Maintenance rooms loaded:', this.maintenanceRooms.length);
          } else {
            this.addError(freeResult.message);
            freeResult.errors.forEach(error => this.addError(error));
            success = false;
          }
          
          if (occupiedResult.isSuccess) {
            this.occupiedRooms = occupiedResult.data;
            console.log('🏨 Occupied rooms loaded:', this.occupiedRooms.length);
          } else {
            this.addError(occupiedResult.message);
            occupiedResult.errors.forEach(error => this.addError(error));
            success = false;
          }
          
          this.allRooms = [...this.freeRooms, ...this.maintenanceRooms, ...this.occupiedRooms];
          this.lastUpdated = new Date();
          console.log('🏨 Parallel loading completed, total rooms:', this.allRooms.length);
          return success;
        } else {
          // Default: load all rooms with optimized endpoint
          return await this.fetchRooms();
        }
      } catch (error) {
        this.addError('Unexpected error while fetching rooms conditionally');
        console.error('Store error:', error);
        return false;
      } finally {
        this.isLoading = false;
      }
    },

    /**
     * Refresh rooms data
     */
    async refreshRooms() {
      this.isRefreshing = true;
      const success = await this.fetchRooms();
      this.isRefreshing = false;
      return success;
    },

    /**
     * Refresh rooms with optimized loading strategy
     */
    async refreshRoomsOptimized() {
      this.isRefreshing = true;
      // Use parallel loading for maximum performance during refresh
      const success = await this.fetchRoomsConditional({ parallel: true });
      this.isRefreshing = false;
      return success;
    },

    /**
     * Set rooms data and categorize them
     */
    setRooms(rooms) {
      this.allRooms = rooms;
      
      // Categorize rooms based on availability and maintenance status
      this.freeRooms = rooms.filter(room => {
        return room.disponible === true && (!room.estado || room.estado.toLowerCase() === 'disponible');
      });
      
      this.occupiedRooms = rooms.filter(room => room.disponible === false);
      
      this.maintenanceRooms = rooms.filter(room => {
        return room.disponible === true && room.estado && room.estado.toLowerCase() !== 'disponible';
      });
    },

    /**
     * Update a specific room
     */
    updateRoom(updatedRoom) {
      // Update in all rooms
      const allIndex = this.allRooms.findIndex(r => r.habitacionId === updatedRoom.habitacionId);
      if (allIndex !== -1) {
        this.allRooms[allIndex] = { ...this.allRooms[allIndex], ...updatedRoom };
      }

      // Update in free rooms
      const freeIndex = this.freeRooms.findIndex(r => r.habitacionId === updatedRoom.habitacionId);
      if (freeIndex !== -1) {
        this.freeRooms[freeIndex] = { ...this.freeRooms[freeIndex], ...updatedRoom };
      }

      // Update in occupied rooms
      const occupiedIndex = this.occupiedRooms.findIndex(r => r.habitacionId === updatedRoom.habitacionId);
      if (occupiedIndex !== -1) {
        this.occupiedRooms[occupiedIndex] = { ...this.occupiedRooms[occupiedIndex], ...updatedRoom };
      }
    },

    /**
     * Move room from free to occupied
     */
    markRoomAsOccupied(roomId, reservationData) {
      const freeIndex = this.freeRooms.findIndex(r => r.habitacionId === roomId);
      
      if (freeIndex !== -1) {
        const room = this.freeRooms.splice(freeIndex, 1)[0];
        const occupiedRoom = {
          ...room,
          disponible: false,
          reservaActiva: reservationData,
          ...reservationData
        };
        
        this.occupiedRooms.push(occupiedRoom);
        this.updateRoom(occupiedRoom);
      }
    },

    /**
     * Move room from occupied to free
     */
    markRoomAsFree(roomId) {
      const occupiedIndex = this.occupiedRooms.findIndex(r => r.habitacionId === roomId);
      
      if (occupiedIndex !== -1) {
        const room = this.occupiedRooms.splice(occupiedIndex, 1)[0];
        const freeRoom = {
          ...room,
          disponible: true,
          reservaActiva: null,
          visita: null,
          visitaID: null,
          pedidosPendientes: false
        };
        
        this.freeRooms.push(freeRoom);
        this.updateRoom(freeRoom);
      }
    },

    /**
     * Add extra time to a room reservation
     */
    addExtraTime(reservationId, hours, minutes) {
      const room = this.occupiedRooms.find(r => 
        r.reservaActiva && r.reservaActiva.reservaId === reservationId
      );
      
      if (room && room.reservaActiva) {
        let newHours = room.reservaActiva.totalHoras + hours;
        let newMinutes = room.reservaActiva.totalMinutos + minutes;
        
        // Handle minute overflow
        while (newMinutes >= 60) {
          newHours += 1;
          newMinutes -= 60;
        }
        
        room.reservaActiva.totalHoras = newHours;
        room.reservaActiva.totalMinutos = newMinutes;
        
        this.updateRoom(room);
      }
    },

    /**
     * Set selected room
     */
    selectRoom(room) {
      this.selectedRoom = room;
    },

    /**
     * Clear selected room
     */
    clearSelectedRoom() {
      this.selectedRoom = null;
    },

    /**
     * Update filters
     */
    updateFilters(newFilters) {
      this.filters = { ...this.filters, ...newFilters };
    },

    /**
     * Update view settings
     */
    updateViewSettings(newSettings) {
      this.viewSettings = { ...this.viewSettings, ...newSettings };
      
      // Persist view mode
      if (newSettings.mode) {
        localStorage.setItem('roomsViewMode', newSettings.mode);
      }
      
      if (newSettings.compactMode !== undefined) {
        localStorage.setItem('roomsCompactMode', newSettings.compactMode.toString());
      }
    },

    /**
     * Load saved view preferences
     */
    loadViewPreferences() {
      const savedViewMode = localStorage.getItem('roomsViewMode');
      if (savedViewMode && ['grid', 'list'].includes(savedViewMode)) {
        this.viewSettings.mode = savedViewMode;
      }
      
      const savedCompactMode = localStorage.getItem('roomsCompactMode');
      if (savedCompactMode !== null) {
        this.viewSettings.compactMode = savedCompactMode === 'true';
      }
    },

    /**
     * Error management
     */
    addError(error) {
      this.errors.push({
        id: Date.now(),
        message: error,
        timestamp: new Date()
      });
      this.lastError = error;
    },

    clearErrors() {
      this.errors = [];
      this.lastError = null;
    },

    removeError(errorId) {
      this.errors = this.errors.filter(e => e.id !== errorId);
      if (this.errors.length === 0) {
        this.lastError = null;
      }
    },

    /**
     * Helper methods
     */
    getTimeLeftInMinutes(room) {
      if (!room.reservaActiva) return 0;
      
      const now = new Date();
      const reservationDate = new Date(room.reservaActiva.fechaReserva);
      const endTime = new Date(reservationDate.getTime() + 
        (room.reservaActiva.totalHoras * 60 + room.reservaActiva.totalMinutos) * 60000);
      
      return Math.max(0, Math.floor((endTime - now) / 60000));
    }
  }
});