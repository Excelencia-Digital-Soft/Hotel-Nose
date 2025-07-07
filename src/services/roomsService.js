/**
 * Rooms API Service
 * Handles all API calls related to rooms management
 */

import axiosClient from '../axiosClient';

export class RoomsService {
  /**
   * Fetch all rooms for a specific institution using optimized endpoint
   * @param {number} institutionId - The institution ID
   * @returns {Promise<{isSuccess: boolean, data: Array, message: string, errors: Array}>}
   */
  async fetchRooms(institutionId) {
    try {
      if (!institutionId) {
        return {
          isSuccess: false,
          data: [],
          message: 'Institution ID is required',
          errors: ['Institution ID is missing']
        };
      }

      // Use the new optimized endpoint
      const response = await axiosClient.get(`/api/v1/habitaciones/optimized?includeInactive=false`);

      // The API returns ApiResponse<List<HabitacionOptimizedDto>>
      if (response.data && response.data.isSuccess && response.data.data) {
        return {
          isSuccess: true,
          data: response.data.data,
          message: response.data.message || 'Rooms fetched successfully',
          errors: response.data.errors || []
        };
      } else if (response.data && !response.data.isSuccess) {
        return {
          isSuccess: false,
          data: [],
          message: response.data.message || 'Failed to fetch rooms',
          errors: response.data.errors || ['Unknown error']
        };
      } else {
        return {
          isSuccess: false,
          data: [],
          message: 'Invalid API response format',
          errors: ['Invalid data structure received from API']
        };
      }
    } catch (error) {
      console.error('Error fetching rooms:', error);
      return {
        isSuccess: false,
        data: [],
        message: 'Failed to fetch rooms',
        errors: [error.message || 'Unknown error occurred']
      };
    }
  }

  /**
   * Fetch only free rooms with minimal data for performance
   * @param {number} institutionId - The institution ID
   * @returns {Promise<{isSuccess: boolean, data: Array, message: string, errors: Array}>}
   */
  async fetchFreeRooms(institutionId) {
    try {
      if (!institutionId) {
        return {
          isSuccess: false,
          data: [],
          message: 'Institution ID is required',
          errors: ['Institution ID is missing']
        };
      }

      const response = await axiosClient.get(`/api/v1/habitaciones/free`);

      if (response.data && response.data.isSuccess && response.data.data) {
        return {
          isSuccess: true,
          data: response.data.data,
          message: response.data.message || 'Free rooms fetched successfully',
          errors: response.data.errors || []
        };
      } else if (response.data && !response.data.isSuccess) {
        return {
          isSuccess: false,
          data: [],
          message: response.data.message || 'Failed to fetch free rooms',
          errors: response.data.errors || ['Unknown error']
        };
      } else {
        return {
          isSuccess: false,
          data: [],
          message: 'Invalid API response format',
          errors: ['Invalid data structure received from API']
        };
      }
    } catch (error) {
      console.error('Error fetching free rooms:', error);
      return {
        isSuccess: false,
        data: [],
        message: 'Failed to fetch free rooms',
        errors: [error.message || 'Unknown error occurred']
      };
    }
  }

  /**
   * Fetch only occupied rooms with optimized data
   * @param {number} institutionId - The institution ID
   * @returns {Promise<{isSuccess: boolean, data: Array, message: string, errors: Array}>}
   */
  async fetchOccupiedRooms(institutionId) {
    try {
      if (!institutionId) {
        return {
          isSuccess: false,
          data: [],
          message: 'Institution ID is required',
          errors: ['Institution ID is missing']
        };
      }

      const response = await axiosClient.get(`/api/v1/habitaciones/occupied-optimized`);

      if (response.data && response.data.isSuccess && response.data.data) {
        return {
          isSuccess: true,
          data: response.data.data,
          message: response.data.message || 'Occupied rooms fetched successfully',
          errors: response.data.errors || []
        };
      } else if (response.data && !response.data.isSuccess) {
        return {
          isSuccess: false,
          data: [],
          message: response.data.message || 'Failed to fetch occupied rooms',
          errors: response.data.errors || ['Unknown error']
        };
      } else {
        return {
          isSuccess: false,
          data: [],
          message: 'Invalid API response format',
          errors: ['Invalid data structure received from API']
        };
      }
    } catch (error) {
      console.error('Error fetching occupied rooms:', error);
      return {
        isSuccess: false,
        data: [],
        message: 'Failed to fetch occupied rooms',
        errors: [error.message || 'Unknown error occurred']
      };
    }
  }

  /**
   * Reserve a room
   * @param {Object} reservationData - Reservation information
   * @returns {Promise<{isSuccess: boolean, data: Object, message: string, errors: Array}>}
   */
  async reserveRoom(reservationData) {
    try {
      const response = await axiosClient.post('/reservar-habitacion', reservationData);

      return {
        isSuccess: true,
        data: response.data,
        message: 'Room reserved successfully',
        errors: []
      };
    } catch (error) {
      console.error('Error reserving room:', error);
      return {
        isSuccess: false,
        data: null,
        message: 'Failed to reserve room',
        errors: [error.message || 'Unknown error occurred']
      };
    }
  }

  /**
   * Checkout a room
   * @param {number} roomId - Room ID to checkout
   * @returns {Promise<{isSuccess: boolean, data: Object, message: string, errors: Array}>}
   */
  async checkoutRoom(roomId) {
    try {
      const response = await axiosClient.post(`/checkout-habitacion/${roomId}`);

      return {
        isSuccess: true,
        data: response.data,
        message: 'Room checked out successfully',
        errors: []
      };
    } catch (error) {
      console.error('Error checking out room:', error);
      return {
        isSuccess: false,
        data: null,
        message: 'Failed to checkout room',
        errors: [error.message || 'Unknown error occurred']
      };
    }
  }

  /**
   * Add extra time to a reservation
   * @param {number} reservationId - Reservation ID
   * @param {number} hours - Additional hours
   * @param {number} minutes - Additional minutes
   * @returns {Promise<{isSuccess: boolean, data: Object, message: string, errors: Array}>}
   */
  async addExtraTime(reservationId, hours, minutes) {
    try {
      const response = await axiosClient.post('/add-tiempo-extra', {
        reservationId,
        hours,
        minutes
      });

      return {
        isSuccess: true,
        data: response.data,
        message: 'Extra time added successfully',
        errors: []
      };
    } catch (error) {
      console.error('Error adding extra time:', error);
      return {
        isSuccess: false,
        data: null,
        message: 'Failed to add extra time',
        errors: [error.message || 'Unknown error occurred']
      };
    }
  }
}

// Export singleton instance
export const roomsService = new RoomsService();
