import { defineStore } from 'pinia'
import axiosClient from '../axiosClient'

export const useAuthStore = defineStore('auth', {
  state: () => {
    return {
      user: null,
      token: null,
      isAuthenticated: false,
      loading: false,
      errors: [],
      institucionID: null,
      instituciones: [],
    }
  },
  persist: true,
  getters: {
    isLoggedIn: (state) => state.isAuthenticated && !!state.token,
    currentUser: (state) => state.user,
    hasErrors: (state) => state.errors.length > 0,
    selectedInstitution: (state) =>
      state.instituciones.find((inst) => inst.institucionId === state.institucionID),
  },
  actions: {
    async login(credentials) {
      this.loading = true
      this.errors = []

      try {
        const loginData = {
          Email: credentials.nombreUsuario || credentials.username || credentials.email,
          Password: credentials.contraseña || credentials.password,
        }

        const response = await axiosClient.post('/api/v1/authentication/login', loginData)

        console.log('login response:', response)

        if (response.data.isSuccess) {
          const { token, tokenExpiration, user } = response.data.data

          this.token = token
          this.user = user
          this.isAuthenticated = true

          // Guardar token en localStorage para el interceptor
          localStorage.setItem('auth-token', token)
          localStorage.setItem('token-expiration', tokenExpiration)
          localStorage.setItem('user', JSON.stringify(user))
          localStorage.setItem('user-roles', JSON.stringify(user.roles))

          // La nueva estructura tiene institucionId directamente en el user
          if (user.institucionId) {
            this.institucionID = user.institucionId
            // Para compatibilidad, crear array de instituciones
            this.instituciones = [
              {
                institucionId: user.institucionId,
                nombre: user.institucionName || 'Hotel',
              },
            ]
            // Connect to WebSocket after setting institution
            this.connectWebSocket()
          }

          return {
            success: true,
            multipleInstitutions: false, // Solo una institución en esta estructura
            message: response.data.message,
          }
        } else {
          this.errors =
            response.data.errors?.length > 0
              ? response.data.errors
              : [response.data.message || 'Error en el login']
          return { success: false, errors: this.errors }
        }
      } catch (error) {
        // Handle new API error structure
        if (error.response?.data) {
          const errorData = error.response.data
          if (errorData.errors && errorData.errors.length > 0) {
            this.errors = errorData.errors
          } else if (errorData.message) {
            this.errors = [errorData.message]
          } else {
            this.errors = ['Error de conexión']
          }
        } else {
          this.errors = ['Error de conexión']
        }
        console.error('Login error:', error)
        return { success: false, errors: this.errors }
      } finally {
        this.loading = false
      }
    },

    async register(userData) {
      this.loading = true
      this.errors = []

      try {
        const response = await axiosClient.post('/api/v1/authentication/register', userData)

        if (response.data.isSuccess) {
          return {
            success: true,
            message: response.data.message,
            data: response.data.data,
          }
        } else {
          this.errors =
            response.data.errors?.length > 0
              ? response.data.errors
              : [response.data.message || 'Error en el registro']
          return { success: false, errors: this.errors, message: response.data.message }
        }
      } catch (error) {
        // Handle new API error structure
        if (error.response?.data) {
          const errorData = error.response.data
          if (errorData.errors && errorData.errors.length > 0) {
            this.errors = errorData.errors
          } else if (errorData.message) {
            this.errors = [errorData.message]
          } else {
            this.errors = ['Error de conexión']
          }
        } else {
          this.errors = ['Error de conexión']
        }
        console.error('Register error:', error)
        return { success: false, errors: this.errors }
      } finally {
        this.loading = false
      }
    },

    async getCurrentUser() {
      if (!this.token) return null

      try {
        const response = await axiosClient.get('/api/v1/authentication/me')

        if (response.data.isSuccess) {
          this.user = response.data.data
          // Update institution info if present
          if (this.user.institucionId) {
            this.institucionID = this.user.institucionId
          }
          return this.user
        }
      } catch (error) {
        console.error('Get current user error:', error)
        if (error.response?.status === 401) {
          // Silently logout without showing connection error on login page
          this.logout()
        }
      }
      return null
    },

    async changePassword(passwordData) {
      this.loading = true
      this.errors = []

      try {
        const response = await axiosClient.put('/api/v1/authentication/password', passwordData)

        if (response.data.isSuccess) {
          return { success: true, message: response.data.message }
        } else {
          this.errors = response.data.errors || [
            response.data.message || 'Error al cambiar contraseña',
          ]
          return { success: false, errors: this.errors }
        }
      } catch (error) {
        const errorMessage = error.response?.data?.message || 'Error de conexión'
        this.errors = [errorMessage]
        console.error('Change password error:', error)
        return { success: false, errors: this.errors }
      } finally {
        this.loading = false
      }
    },

    selectInstitucion(id) {
      this.institucionID = id
      // Connect to WebSocket after selecting institution
      this.connectWebSocket()
    },

    async connectWebSocket() {
      try {
        // Use the new SignalR NotificationService
        const { NotificationService } = await import('../services/NotificationService')
        const notificationService = NotificationService.getInstance()
        
        if (this.token && this.institucionID) {
          await notificationService.initialize(this.token)
          // Auto-subscribe to institution (handled by server based on token claims)
          console.log('SignalR connected for institution:', this.institucionID)
        }
      } catch (error) {
        console.error('Failed to connect SignalR:', error)
      }
    },

    async logout() {
      try {
        if (this.token) {
          await axiosClient.post('/api/v1/authentication/logout')
        }

        // Disconnect SignalR
        const { NotificationService } = await import('../services/NotificationService')
        const notificationService = NotificationService.getInstance()
        await notificationService.stop()
      } catch (error) {
        console.error('Logout error:', error)
      } finally {
        this.token = null
        this.user = null
        this.isAuthenticated = false
        this.instituciones = []
        this.institucionID = null
        this.errors = []
        localStorage.removeItem('auth-token')
      }
    },

    clearErrorsAndState() {
      this.errors = []
      this.loading = false
    },

    clearErrors() {
      this.errors = []
    },
  },
})
