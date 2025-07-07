import axiosClient from '../axiosClient';

/**
 * Configuration service for API interactions
 * All methods expect and return the new standardized API format
 */
export const configurationService = {
  /**
   * Get timer update interval from API
   * @returns {Promise<{isSuccess: boolean, data: {intervalMinutos: number, descripcion: string, fechaModificacion: string|null}, errors: string[], message: string|null}>}
   */
  async getTimerUpdateInterval() {
    try {
      const response = await axiosClient.get('/api/v1/configuration/timer-update-interval');
      
      // API now returns the standardized format directly
      return response.data;
    } catch (error) {
      console.error('Error fetching timer update interval:', error);
      
      // Return standardized error response
      return {
        isSuccess: false,
        data: {
          intervalMinutos: 10,
          descripcion: "Default timer update interval (fallback)",
          fechaModificacion: null
        },
        errors: [error.message || 'Unknown error occurred'],
        message: error.response?.status === 404 
          ? 'Timer interval configuration not found' 
          : 'Error fetching timer update interval'
      };
    }
  },

  /**
   * Update timer update interval
   * @param {number} intervalMinutos - New interval in minutes
   * @returns {Promise<{isSuccess: boolean, data: {intervalMinutos: number, descripcion: string, fechaModificacion: string}, errors: string[], message: string|null}>}
   */
  async updateTimerUpdateInterval(intervalMinutos) {
    try {
      const response = await axiosClient.put('/api/v1/configuration/timer-update-interval', {
        intervalMinutos
      });
      
      return response.data;
    } catch (error) {
      console.error('Error updating timer update interval:', error);
      
      return {
        isSuccess: false,
        data: null,
        errors: [error.message || 'Unknown error occurred'],
        message: 'Error updating timer update interval'
      };
    }
  },

  /**
   * Get all configuration settings
   * @returns {Promise<{isSuccess: boolean, data: Object[], errors: string[], message: string|null}>}
   */
  async getAllConfigurations() {
    try {
      const response = await axiosClient.get('/api/v1/configuration');
      
      return response.data;
    } catch (error) {
      console.error('Error fetching configurations:', error);
      
      return {
        isSuccess: false,
        data: [],
        errors: [error.message || 'Unknown error occurred'],
        message: 'Error fetching configurations'
      };
    }
  },

  /**
   * Get specific configuration by key
   * @param {string} key - Configuration key
   * @returns {Promise<{isSuccess: boolean, data: Object, errors: string[], message: string|null}>}
   */
  async getConfiguration(key) {
    try {
      const response = await axiosClient.get(`/api/v1/configuration/${key}`);
      
      return response.data;
    } catch (error) {
      console.error(`Error fetching configuration ${key}:`, error);
      
      return {
        isSuccess: false,
        data: null,
        errors: [error.message || 'Unknown error occurred'],
        message: `Error fetching configuration: ${key}`
      };
    }
  },

  /**
   * Update specific configuration
   * @param {string} key - Configuration key
   * @param {any} value - New configuration value
   * @returns {Promise<{isSuccess: boolean, data: Object, errors: string[], message: string|null}>}
   */
  async updateConfiguration(key, value) {
    try {
      const response = await axiosClient.put(`/api/v1/configuration/${key}`, { value });
      
      return response.data;
    } catch (error) {
      console.error(`Error updating configuration ${key}:`, error);
      
      return {
        isSuccess: false,
        data: null,
        errors: [error.message || 'Unknown error occurred'],
        message: `Error updating configuration: ${key}`
      };
    }
  }
};