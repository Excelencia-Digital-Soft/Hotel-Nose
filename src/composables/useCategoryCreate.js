import { ref, computed } from 'vue'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'

export function useCategoryCreate() {
  const toast = useToast()
  const confirm = useConfirm()

  // Form state
  const formData = ref({
    nombreCategoria: '',
    precioNormal: '',
    capacidadMaxima: '',
    porcentajeXPersona: '',
    categoriaId: null
  })

  // UI state
  const isEditMode = ref(false)
  const isSubmitting = ref(false)
  
  // Data state
  const categorias = ref([])

  // Validation
  const isFormValid = computed(() => {
    return formData.value.nombreCategoria.trim() !== '' &&
           formData.value.precioNormal !== '' &&
           formData.value.capacidadMaxima !== '' &&
           formData.value.porcentajeXPersona !== ''
  })

  const isNameDuplicate = computed(() => {
    if (!categorias.value || !formData.value.nombreCategoria) return false
    
    return categorias.value.some((categoria) =>
      categoria.nombreCategoria.toLowerCase() === formData.value.nombreCategoria.toLowerCase() &&
      categoria.categoriaId !== formData.value.categoriaId
    )
  })

  // Form operations
  const resetForm = () => {
    formData.value = {
      nombreCategoria: '',
      precioNormal: '',
      capacidadMaxima: '',
      porcentajeXPersona: '',
      categoriaId: null
    }
    isEditMode.value = false
  }

  const startEdit = (categoria) => {
    isEditMode.value = true
    formData.value = {
      categoriaId: categoria.categoriaId,
      nombreCategoria: categoria.nombreCategoria,
      precioNormal: categoria.precioNormal.toString(),
      capacidadMaxima: categoria.capacidadMaxima.toString(),
      porcentajeXPersona: categoria.porcentajeXPersona.toString()
    }
  }

  const cancelEdit = () => {
    resetForm()
  }

  // Validation methods
  const validateForm = () => {
    if (!isFormValid.value) {
      showError('Por favor completa todos los campos requeridos')
      return false
    }

    if (isNameDuplicate.value) {
      showError('Ya existe una categoría con este nombre')
      return false
    }

    // Validate numeric fields
    const precio = parseFloat(formData.value.precioNormal)
    const capacidad = parseInt(formData.value.capacidadMaxima)
    const porcentaje = parseFloat(formData.value.porcentajeXPersona)

    if (isNaN(precio) || precio < 0) {
      showError('El precio debe ser un número válido mayor o igual a 0')
      return false
    }

    if (isNaN(capacidad) || capacidad <= 0) {
      showError('La capacidad debe ser un número entero mayor a 0')
      return false
    }

    if (isNaN(porcentaje) || porcentaje < 0 || porcentaje > 100) {
      showError('El porcentaje debe ser un número entre 0 y 100')
      return false
    }

    return true
  }

  // Format data for API
  const getFormattedData = () => {
    return {
      nombreCategoria: formData.value.nombreCategoria.trim(),
      precioNormal: parseFloat(formData.value.precioNormal),
      capacidadMaxima: parseInt(formData.value.capacidadMaxima),
      porcentajeXPersona: parseFloat(formData.value.porcentajeXPersona)
    }
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
  const confirmDelete = (categoria) => {
    return new Promise((resolve) => {
      confirm.require({
        message: `¿Estás seguro de eliminar la categoría "${categoria.nombreCategoria}"?`,
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

  // Helper methods for display
  const formatPrice = (price) => {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP',
      minimumFractionDigits: 0
    }).format(price)
  }

  const formatPercentage = (percentage) => {
    return `${percentage}%`
  }

  return {
    // Form state
    formData,
    isEditMode,
    isSubmitting,
    
    // Data state
    categorias,
    
    // Computed
    isFormValid,
    isNameDuplicate,
    
    // Methods
    resetForm,
    startEdit,
    cancelEdit,
    validateForm,
    getFormattedData,
    showSuccess,
    showError,
    showInfo,
    confirmDelete,
    formatPrice,
    formatPercentage
  }
}