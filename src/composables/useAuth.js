import { computed, ref } from 'vue'
import { useAuthStore } from '../store/auth'
import { useRouter } from 'vue-router'
import { showErrorToast } from '../utils/toast'

export function useAuth() {
  const authStore = useAuthStore()
  const router = useRouter()

  // Estado reactivo para formularios
  const loginForm = ref({
    username: '',
    password: ''
  })

  const registerForm = ref({
    username: '',
    email: '',
    name: '',
    password: '',
    confirmPassword: ''
  })

  // Computed properties
  const isAuthenticated = computed(() => authStore.isLoggedIn)
  const currentUser = computed(() => authStore.currentUser)
  const isLoading = computed(() => authStore.loading)
  const errors = computed(() => authStore.errors)
  const hasErrors = computed(() => authStore.hasErrors)
  const institutions = computed(() => authStore.instituciones)
  const selectedInstitution = computed(() => authStore.selectedInstitution)

  // Métodos de autenticación
  const login = async (credentials) => {
    const result = await authStore.login(credentials || loginForm.value)

    if (result.success) {
      // Limpiar formulario
      loginForm.value = { username: '', password: '' }

      // Redirigir a la página principal si no hay múltiples instituciones
      if (!result.multipleInstitutions) {
        router.push({ name: 'Home' })
      }

      // Retornar información sobre instituciones múltiples
      return {
        success: true,
        multipleInstitutions: result.multipleInstitutions,
        userName: authStore.user?.firstName || authStore.user?.userName || 'Usuario'
      }
    }

    return result
  }

  const register = async (userData) => {
    // Validar que las contraseñas coincidan
    if (userData.password !== userData.confirmPassword) {
      return {
        success: false,
        errors: ['Las contraseñas no coinciden']
      }
    }

    const result = await authStore.register(userData || registerForm.value)

    if (result.success) {
      // Limpiar formulario
      registerForm.value = {
        username: '',
        email: '',
        name: '',
        password: '',
        confirmPassword: ''
      }
    }

    return result
  }

  const logout = async () => {
    await authStore.logout()
    router.push('/guest')
  }

  const selectInstitution = (institutionId) => {
    authStore.selectInstitucion(institutionId)
    router.push({ name: 'Home' })
  }

  const changePassword = async (passwordData) => {
    return await authStore.changePassword(passwordData)
  }

  const getCurrentUser = async () => {
    return await authStore.getCurrentUser()
  }

  const clearErrors = () => {
    authStore.clearErrors()
  }

  // Validaciones
  const validateLoginForm = () => {
    const errors = []

    if (!loginForm.value.username.trim()) {
      errors.push('El nombre de usuario es requerido')
    }

    if (!loginForm.value.password.trim()) {
      errors.push('La contraseña es requerida')
    }

    return {
      isValid: errors.length === 0,
      errors
    }
  }

  const validateRegisterForm = () => {
    const errors = []

    if (!registerForm.value.username.trim()) {
      errors.push('El nombre de usuario es requerido')
    }

    if (!registerForm.value.email.trim()) {
      errors.push('El email es requerido')
    } else if (!/\S+@\S+\.\S+/.test(registerForm.value.email)) {
      errors.push('El email no es válido')
    }

    if (!registerForm.value.name.trim()) {
      errors.push('El nombre es requerido')
    }

    if (!registerForm.value.password.trim()) {
      errors.push('La contraseña es requerida')
    } else if (registerForm.value.password.length < 6) {
      errors.push('La contraseña debe tener al menos 6 caracteres')
    }

    if (registerForm.value.password !== registerForm.value.confirmPassword) {
      errors.push('Las contraseñas no coinciden')
    }

    return {
      isValid: errors.length === 0,
      errors
    }
  }

  return {
    // Estado
    loginForm,
    registerForm,

    // Computed
    isAuthenticated,
    currentUser,
    isLoading,
    errors,
    hasErrors,
    institutions,
    selectedInstitution,

    // Métodos
    login,
    register,
    logout,
    selectInstitution,
    changePassword,
    getCurrentUser,
    clearErrors,

    // Validaciones
    validateLoginForm,
    validateRegisterForm
  }
}
