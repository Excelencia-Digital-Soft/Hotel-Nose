import { ref, computed } from 'vue'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'

export function useCaracteristicas() {
  const toast = useToast()
  const confirm = useConfirm()

  // Form state
  const formData = ref({
    nombre: '',
    descripcion: '',
    icono: null,
    caracteristicaId: null
  })

  // UI state
  const isEditMode = ref(false)
  const isSubmitting = ref(false)
  const isLoadingImage = ref(false)
  const imagePreview = ref('https://static.vecteezy.com/system/resources/previews/004/141/669/original/no-photo-or-blank-image-icon-loading-images-or-missing-image-mark-image-not-available-or-image-coming-soon-sign-simple-nature-silhouette-in-frame-isolated-illustration-vector.jpg')
  
  // Data state
  const caracteristicas = ref([])

  // Validation
  const isFormValid = computed(() => {
    return formData.value.nombre.trim() !== ''
  })

  const isNameDuplicate = computed(() => {
    if (!caracteristicas.value || !formData.value.nombre) return false
    
    return caracteristicas.value.some((item) =>
      item.nombre.toLowerCase() === formData.value.nombre.toLowerCase() &&
      item.caracteristicaId !== formData.value.caracteristicaId
    )
  })

  // Form operations
  const resetForm = () => {
    formData.value = {
      nombre: '',
      descripcion: '',
      icono: null,
      caracteristicaId: null
    }
    imagePreview.value = 'https://static.vecteezy.com/system/resources/previews/004/141/669/original/no-photo-or-blank-image-icon-loading-images-or-missing-image-mark-image-not-available-or-image-coming-soon-sign-simple-nature-silhouette-in-frame-isolated-illustration-vector.jpg'
    isEditMode.value = false
  }

  const handleImageUpload = (file) => {
    if (file) {
      formData.value.icono = file
      imagePreview.value = URL.createObjectURL(file)
    }
  }

  const startEdit = (caracteristica) => {
    isEditMode.value = true
    formData.value = {
      caracteristicaId: caracteristica.caracteristicaId,
      nombre: caracteristica.nombre,
      descripcion: caracteristica.descripcion || '',
      icono: null
    }
    imagePreview.value = caracteristica.icono || imagePreview.value
  }

  const cancelEdit = () => {
    resetForm()
  }

  // Validation methods
  const validateForm = () => {
    if (!isFormValid.value) {
      showError('Por favor ingresa un nombre para la característica')
      return false
    }

    if (isNameDuplicate.value) {
      showError('Ya existe una característica con este nombre')
      return false
    }

    return true
  }

  // Toast messages
  const showSuccess = (message) => {
    toast.add({
      severity: 'success',
      summary: 'Éxito',
      detail: message,
      life: 5000
    })
  }

  const showError = (message) => {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: message,
      life: 5000
    })
  }

  const showInfo = (message) => {
    toast.add({
      severity: 'info',
      summary: 'Información',
      detail: message,
      life: 5000
    })
  }

  // Confirmation dialog
  const confirmDelete = (caracteristica) => {
    return new Promise((resolve) => {
      confirm.require({
        message: `¿Estás seguro de eliminar la característica "${caracteristica.nombre}"?`,
        header: 'Confirmar Eliminación',
        icon: 'pi pi-exclamation-triangle',
        acceptLabel: 'Sí, eliminar',
        rejectLabel: 'Cancelar',
        acceptClass: 'p-button-danger',
        accept: () => resolve(true),
        reject: () => resolve(false)
      })
    })
  }

  return {
    // Form state
    formData,
    isEditMode,
    isSubmitting,
    isLoadingImage,
    imagePreview,
    
    // Data state
    caracteristicas,
    
    // Computed
    isFormValid,
    isNameDuplicate,
    
    // Methods
    resetForm,
    handleImageUpload,
    startEdit,
    cancelEdit,
    validateForm,
    showSuccess,
    showError,
    showInfo,
    confirmDelete
  }
}