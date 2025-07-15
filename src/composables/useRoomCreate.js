import { ref, computed } from 'vue'
import { useToast } from 'primevue/usetoast'
import { useAuthStore } from '../store/auth'

export function useRoomCreate() {
  const toast = useToast()
  const authStore = useAuthStore()

  // Form state
  const formData = ref({
    roomName: '',
    selectedCategory: null,
    imageFiles: [],
    imagePreviews: []
  })

  // Update mode state
  const isUpdateMode = ref(false)
  const roomToUpdate = ref(null)
  const removedImageIds = ref([])

  // Loading states
  const isSubmitting = ref(false)
  const isLoadingImages = ref(false)

  // Validation
  const isFormValid = computed(() => {
    return formData.value.roomName.trim() !== '' && 
           formData.value.selectedCategory !== null
  })

  // Initialize form for update
  const initializeUpdateMode = (room) => {
    isUpdateMode.value = true
    roomToUpdate.value = room
    formData.value.roomName = room.nombreHabitacion
    formData.value.selectedCategory = room.categoriaId
    removedImageIds.value = []
  }

  // Reset form
  const resetForm = () => {
    isUpdateMode.value = false
    roomToUpdate.value = null
    formData.value = {
      roomName: '',
      selectedCategory: null,
      imageFiles: [],
      imagePreviews: []
    }
    removedImageIds.value = []
    isSubmitting.value = false
  }

  // Handle image preview
  const handleImagePreview = (files) => {
    const fileArray = Array.from(files)
    formData.value.imageFiles = fileArray
    formData.value.imagePreviews = []

    fileArray.forEach(file => {
      const reader = new FileReader()
      reader.onload = (e) => {
        formData.value.imagePreviews.push({
          id: 0,
          url: e.target.result,
          isNew: true
        })
      }
      reader.readAsDataURL(file)
    })
  }

  // Remove image
  const removeImage = (index, imageId = null) => {
    if (imageId && imageId !== 0) {
      removedImageIds.value.push(imageId)
    }
    
    formData.value.imageFiles.splice(index, 1)
    formData.value.imagePreviews.splice(index, 1)
  }

  // Select category
  const selectCategory = (categoryId) => {
    formData.value.selectedCategory = categoryId
  }

  // Show success message
  const showSuccess = (message) => {
    toast.add({
      severity: 'success',
      summary: 'Éxito',
      detail: message,
      life: 5000
    })
  }

  // Show error message
  const showError = (message) => {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: message,
      life: 5000
    })
  }

  return {
    formData,
    isUpdateMode,
    roomToUpdate,
    removedImageIds,
    isSubmitting,
    isLoadingImages,
    isFormValid,
    initializeUpdateMode,
    resetForm,
    handleImagePreview,
    removeImage,
    selectCategory,
    showSuccess,
    showError
  }
}