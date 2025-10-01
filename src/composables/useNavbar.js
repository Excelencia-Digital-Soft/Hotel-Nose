import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../store/auth.js'

export function useNavbar() {
  const router = useRouter()
  const authStore = useAuthStore()
  
  const selectedInstitutionId = ref(null)
  
  const userInfo = computed(() => {
    if (!authStore.isAuthenticated) {
      return {
        hotel: '',
        rol: '',
        nombreUsuario: ''
      }
    }
    
    const selectedInst = authStore.instituciones.find(
      inst => inst.institucionId === authStore.institucionID
    )
    
    return {
      hotel: selectedInst?.nombre || 'Hotel',
      rol: 'Admin', // Temporal
      nombreUsuario: authStore.user?.nombreUsuario || 'Usuario'
    }
  })
  
  const isAuthenticated = computed(() => authStore.isAuthenticated)
  const institutions = computed(() => authStore.instituciones || [])
  const hasMultipleInstitutions = computed(() => institutions.value.length > 1)
  
  const handleInstitutionChange = (institutionId) => {
    selectedInstitutionId.value = institutionId
    authStore.selectInstitucion(institutionId)
  }
  
  const handleLogout = async () => {
    await authStore.logout()
    router.push({ name: 'Guest' })
  }
  
  const handleUserAction = async (action) => {
    if (action === 'logout') {
      await handleLogout()
    } else {
      console.log('User action:', action)
    }
  }
  
  const handleMenuAction = (action) => {
    console.log('Menu action:', action)
  }
  
  onMounted(() => {
    if (authStore.institucionID) {
      selectedInstitutionId.value = authStore.institucionID
    }
  })
  
  return {
    selectedInstitutionId,
    userInfo,
    isAuthenticated,
    institutions,
    hasMultipleInstitutions,
    handleInstitutionChange,
    handleUserAction,
    handleMenuAction,
    authStore
  }
}