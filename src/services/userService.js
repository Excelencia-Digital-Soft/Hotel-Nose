import axiosClient from '../axiosClient';

const API_BASE = '/api/v1/user-management';

export const userService = {
  async getUsers() {
    try {
      const response = await axiosClient.get(`${API_BASE}/users`);
      return response.data;
    } catch (error) {
      console.error('Error fetching users:', error);
      throw error;
    }
  },

  async getUserRoles(userId) {
    try {
      const response = await axiosClient.get(`${API_BASE}/users/${userId}/roles`);
      return response.data;
    } catch (error) {
      console.error('Error fetching user roles:', error);
      throw error;
    }
  },

  async getRoles() {
    try {
      const response = await axiosClient.get(`${API_BASE}/roles`);
      return response.data;
    } catch (error) {
      console.error('Error fetching roles:', error);
      throw error;
    }
  },

  async updateUserRoles(userId, roles) {
    try {
      const updateRequest = {
        userId: userId,
        roles: roles
      };
      const response = await axiosClient.put(`${API_BASE}/users/roles`, updateRequest);
      return response.data;
    } catch (error) {
      console.error('Error updating user roles:', error);
      throw error;
    }
  },

  async updateUserStatus(userId, isActive) {
    try {
      const blockRequest = {
        userId: userId,
        isActive: isActive
      };
      const response = await axiosClient.put(`${API_BASE}/users/status`, blockRequest);
      return response.data;
    } catch (error) {
      console.error('Error updating user status:', error);
      throw error;
    }
  },

  async updateUser(userId, userData) {
    try {
      // Remove userId from userData since it's now in the route
      const { userId: _, ...updateRequest } = userData;
      const response = await axiosClient.put(`${API_BASE}/users/${userId}`, updateRequest);
      return response.data;
    } catch (error) {
      console.error('Error updating user data:', error);
      throw error;
    }
  },

  // Legacy methods for compatibility - these would need different endpoints
  async createUser(userData) {
    try {
      // This endpoint might not exist in the new API
      // You'll need to use the authentication/register endpoint instead
      const response = await axiosClient.post('/api/v1/authentication/register', userData);
      return response.data;
    } catch (error) {
      console.error('Error creating user:', error);
      throw error;
    }
  },

  async deleteUser(userId) {
    try {
      // This endpoint might not exist in the new API
      // You might need to use updateUserStatus with isActive: false instead
      return await this.updateUserStatus(userId, false);
    } catch (error) {
      console.error('Error deleting user:', error);
      throw error;
    }
  },

  async changeUserPassword(userId, newPassword) {
    try {
      const passwordRequest = {
        userId: userId,
        newPassword: newPassword
      };
      const response = await axiosClient.put(`${API_BASE}/users/password`, passwordRequest);
      return response.data;
    } catch (error) {
      console.error('Error changing password:', error);
      throw error;
    }
  },

  async resendVerificationEmail(userId) {
    try {
      // This endpoint might not exist in the new API
      const response = await axiosClient.post(`/api/v1/authentication/resend-verification/${userId}`);
      return response.data;
    } catch (error) {
      console.error('Error resending verification email:', error);
      throw error;
    }
  }
};