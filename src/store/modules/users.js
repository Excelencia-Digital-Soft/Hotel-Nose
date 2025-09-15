import { defineStore } from 'pinia';
import { userService } from '../../services/userService';

export const useUsersStore = defineStore('users', {
  state: () => ({
    users: [],
    roles: [],
    currentUser: null,
    loading: false,
    error: null,
    pagination: {
      currentPage: 1,
      totalPages: 1,
      pageSize: 10,
      totalItems: 0
    },
    filters: {
      search: '',
      roleId: null,
      isActive: null
    }
  }),

  getters: {
    filteredUsers: (state) => {
      let filtered = [...state.users];
      
      if (state.filters.search) {
        const searchLower = state.filters.search.toLowerCase();
        filtered = filtered.filter(user => 
          user.email?.toLowerCase().includes(searchLower) ||
          user.userName?.toLowerCase().includes(searchLower) ||
          user.firstName?.toLowerCase().includes(searchLower) ||
          user.lastName?.toLowerCase().includes(searchLower)
        );
      }

      if (state.filters.roleId) {
        // roleId is actually the role name from the dropdown
        filtered = filtered.filter(user => 
          user.roles && user.roles.includes(state.filters.roleId)
        );
      }

      if (state.filters.isActive !== null) {
        filtered = filtered.filter(user => user.isActive === state.filters.isActive);
      }

      return filtered;
    },

    getRoleById: (state) => (roleId) => {
      return state.roles.find(role => role.id === roleId);
    },

    getRoleName: (state) => (roleId) => {
      const role = state.roles.find(role => role.id === roleId);
      return role?.name || 'Sin rol';
    },

    hasUsers: (state) => state.users.length > 0,
    hasRoles: (state) => state.roles.length > 0,
    isLoading: (state) => state.loading,
    hasError: (state) => !!state.error
  },

  actions: {
    async fetchUsers() {
      this.loading = true;
      this.error = null;
      
      try {
        const response = await userService.getUsers();
        
        if (response.isSuccess) {
          this.users = response.data || [];
          
          // Since the API doesn't return pagination, we'll handle it client-side
          this.pagination = {
            currentPage: 1,
            totalPages: 1,
            pageSize: this.users.length,
            totalItems: this.users.length
          };
        } else {
          throw new Error(response.message || 'Error al cargar usuarios');
        }
      } catch (error) {
        this.error = error.response?.data?.message || error.message || 'Error al cargar usuarios';
        console.error('Error fetching users:', error);
      } finally {
        this.loading = false;
      }
    },

    async fetchRoles() {
      try {
        const response = await userService.getRoles();
        
        if (response.isSuccess) {
          this.roles = response.data;
        } else {
          throw new Error(response.message || 'Error al cargar roles');
        }
      } catch (error) {
        this.error = error.response?.data?.message || error.message || 'Error al cargar roles';
        console.error('Error fetching roles:', error);
      }
    },

    async getUserById(userId) {
      this.loading = true;
      this.error = null;
      
      try {
        const response = await userService.getUserById(userId);
        
        if (response.isSuccess) {
          this.currentUser = response.data;
          return response.data;
        } else {
          throw new Error(response.message || 'Error al cargar usuario');
        }
      } catch (error) {
        this.error = error.response?.data?.message || error.message || 'Error al cargar usuario';
        console.error('Error fetching user:', error);
        throw error;
      } finally {
        this.loading = false;
      }
    },

    async createUser(userData) {
      this.loading = true;
      this.error = null;
      
      try {
        const response = await userService.createUser(userData);
        
        if (response.isSuccess) {
          await this.fetchUsers();
          return { success: true, data: response.data };
        } else {
          throw new Error(response.message || 'Error al crear usuario');
        }
      } catch (error) {
        this.error = error.response?.data?.message || error.message || 'Error al crear usuario';
        console.error('Error creating user:', error);
        return { success: false, error: this.error };
      } finally {
        this.loading = false;
      }
    },

    async updateUser(userId, userData) {
      this.loading = true;
      this.error = null;
      
      try {
        const response = await userService.updateUser(userId, userData);
        
        if (response.isSuccess) {
          await this.fetchUsers();
          return { success: true, data: response.data };
        } else {
          throw new Error(response.message || 'Error al actualizar usuario');
        }
      } catch (error) {
        this.error = error.response?.data?.message || error.message || 'Error al actualizar usuario';
        console.error('Error updating user:', error);
        return { success: false, error: this.error };
      } finally {
        this.loading = false;
      }
    },

    async deleteUser(userId) {
      this.loading = true;
      this.error = null;
      
      try {
        const response = await userService.deleteUser(userId);
        
        if (response.isSuccess) {
          await this.fetchUsers();
          return { success: true };
        } else {
          throw new Error(response.message || 'Error al eliminar usuario');
        }
      } catch (error) {
        this.error = error.response?.data?.message || error.message || 'Error al eliminar usuario';
        console.error('Error deleting user:', error);
        return { success: false, error: this.error };
      } finally {
        this.loading = false;
      }
    },

    async changeUserPassword(userId, newPassword) {
      this.loading = true;
      this.error = null;
      
      try {
        const response = await userService.changeUserPassword(userId, newPassword);
        
        if (response.isSuccess) {
          return { success: true, message: response.message };
        } else {
          throw new Error(response.message || 'Error al cambiar contraseña');
        }
      } catch (error) {
        this.error = error.response?.data?.message || error.message || 'Error al cambiar contraseña';
        console.error('Error changing password:', error);
        return { success: false, error: this.error };
      } finally {
        this.loading = false;
      }
    },

    async updateUserRoles(userId, roles) {
      this.loading = true;
      this.error = null;
      
      try {
        const response = await userService.updateUserRoles(userId, roles);
        
        if (response.isSuccess) {
          await this.fetchUsers();
          return { success: true };
        } else {
          throw new Error(response.message || 'Error al actualizar roles');
        }
      } catch (error) {
        this.error = error.response?.data?.message || error.message || 'Error al actualizar roles';
        console.error('Error updating user roles:', error);
        return { success: false, error: this.error };
      } finally {
        this.loading = false;
      }
    },

    async updateUserStatus(userId, isActive) {
      this.loading = true;
      this.error = null;
      
      try {
        const response = await userService.updateUserStatus(userId, isActive);
        
        if (response.isSuccess) {
          await this.fetchUsers();
          return { success: true };
        } else {
          throw new Error(response.message || 'Error al actualizar estado del usuario');
        }
      } catch (error) {
        this.error = error.response?.data?.message || error.message || 'Error al actualizar estado del usuario';
        console.error('Error updating user status:', error);
        return { success: false, error: this.error };
      } finally {
        this.loading = false;
      }
    },

    async verifyEmail(userId, token) {
      this.loading = true;
      this.error = null;
      
      try {
        const response = await userService.verifyEmail(userId, token);
        
        if (response.isSuccess) {
          return { success: true, message: response.message };
        } else {
          throw new Error(response.message || 'Error al verificar email');
        }
      } catch (error) {
        this.error = error.response?.data?.message || error.message || 'Error al verificar email';
        console.error('Error verifying email:', error);
        return { success: false, error: this.error };
      } finally {
        this.loading = false;
      }
    },

    async resendVerificationEmail(userId) {
      this.loading = true;
      this.error = null;
      
      try {
        const response = await userService.resendVerificationEmail(userId);
        
        if (response.isSuccess) {
          return { success: true, message: response.message };
        } else {
          throw new Error(response.message || 'Error al reenviar email de verificación');
        }
      } catch (error) {
        this.error = error.response?.data?.message || error.message || 'Error al reenviar email';
        console.error('Error resending verification email:', error);
        return { success: false, error: this.error };
      } finally {
        this.loading = false;
      }
    },

    setFilters(filters) {
      this.filters = { ...this.filters, ...filters };
      this.pagination.currentPage = 1;
    },

    clearFilters() {
      this.filters = {
        search: '',
        roleId: null,
        isActive: null
      };
      this.pagination.currentPage = 1;
    },

    setPage(page) {
      this.pagination.currentPage = page;
    },

    clearError() {
      this.error = null;
    },

    clearCurrentUser() {
      this.currentUser = null;
    }
  }
});