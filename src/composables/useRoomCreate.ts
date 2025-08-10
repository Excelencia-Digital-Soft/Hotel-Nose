import { ref, computed, type Ref, type ComputedRef } from 'vue'
import { useToast } from 'primevue/usetoast'
import type { HabitacionDto } from '../services/habitacionService'

// Type definitions
interface ImagePreview {
  id: number
  url: string
  isNew?: boolean
}

interface RoomFormData {
  roomName: string
  selectedCategory: number | null
  imageFiles: File[]
  imagePreviews: ImagePreview[]
}

interface UseRoomCreateReturn {
  formData: Ref<RoomFormData>
  isUpdateMode: Ref<boolean>
  roomToUpdate: Ref<HabitacionDto | null>
  removedImageIds: Ref<number[]>
  isSubmitting: Ref<boolean>
  isLoadingImages: Ref<boolean>
  isFormValid: ComputedRef<boolean>
  initializeUpdateMode: (room: HabitacionDto) => void
  resetForm: () => void
  handleImagePreview: (files: FileList | File[]) => void
  removeImage: (index: number, imageId?: number | null) => void
  selectCategory: (categoryId: number) => void
  showSuccess: (message: string) => void
  showError: (message: string) => void
}

export function useRoomCreate(): UseRoomCreateReturn {
  const toast = useToast()

  // Form state
  const formData = ref<RoomFormData>({
    roomName: '',
    selectedCategory: null,
    imageFiles: [],
    imagePreviews: [],
  })

  // Update mode state
  const isUpdateMode = ref<boolean>(false)
  const roomToUpdate = ref<HabitacionDto | null>(null)
  const removedImageIds = ref<number[]>([])

  // Loading states
  const isSubmitting = ref<boolean>(false)
  const isLoadingImages = ref<boolean>(false)

  // Validation
  const isFormValid = computed<boolean>(() => {
    return formData.value.roomName.trim() !== '' && formData.value.selectedCategory !== null
  })

  /**
   * Initialize form for update mode
   */
  const initializeUpdateMode = (room: HabitacionDto): void => {
    isUpdateMode.value = true
    roomToUpdate.value = room
    formData.value.roomName = room.nombreHabitacion
    formData.value.selectedCategory = room.categoriaId
    removedImageIds.value = []
  }

  /**
   * Reset form to initial state
   */
  const resetForm = (): void => {
    isUpdateMode.value = false
    roomToUpdate.value = null
    formData.value = {
      roomName: '',
      selectedCategory: null,
      imageFiles: [],
      imagePreviews: [],
    }
    removedImageIds.value = []
    isSubmitting.value = false
  }

  /**
   * Handle image preview generation
   */
  const handleImagePreview = (files: FileList | File[]): void => {
    const fileArray = Array.from(files)
    formData.value.imageFiles = fileArray
    formData.value.imagePreviews = []

    fileArray.forEach((file: File) => {
      const reader = new FileReader()
      reader.onload = (e: ProgressEvent<FileReader>) => {
        if (e.target?.result) {
          formData.value.imagePreviews.push({
            id: 0,
            url: e.target.result as string,
            isNew: true,
          })
        }
      }
      reader.readAsDataURL(file)
    })
  }

  /**
   * Remove image from form
   */
  const removeImage = (index: number, imageId: number | null = null): void => {
    if (imageId && imageId !== 0) {
      removedImageIds.value.push(imageId)
    }

    formData.value.imageFiles.splice(index, 1)
    formData.value.imagePreviews.splice(index, 1)
  }

  /**
   * Select a category
   */
  const selectCategory = (categoryId: number): void => {
    formData.value.selectedCategory = categoryId
  }

  /**
   * Show success toast message
   */
  const showSuccess = (message: string): void => {
    toast.add({
      severity: 'success',
      summary: 'Éxito',
      detail: message,
      life: 5000,
    })
  }

  /**
   * Show error toast message
   */
  const showError = (message: string): void => {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: message,
      life: 5000,
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
    showError,
  }
}

// Export types for external use
export type { RoomFormData, ImagePreview, UseRoomCreateReturn }

