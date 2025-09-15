import { ref, computed, type Ref, type ComputedRef } from 'vue'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'
import type {
  ArticleFormData,
  ArticleDto,
  ArticleCreateDto,
  ArticleCreateWithImageDto,
} from '../types'

// Types for categories
interface CategoryDto {
  categoriaId: number
  nombreCategoria: string
}

// Sort options type
type SortBy = 'name' | 'price' | 'category' | 'date'

// Statistics interface
interface ArticleStatistics {
  total: number
  totalValue: string
  averagePrice: string
  categoryStats: {
    name: string
    count: number
    percentage: number
  }[]
}

// Return type for the composable
interface UseArticleCreateReturn {
  // Form state
  formData: Ref<ArticleFormData>
  isEditMode: Ref<boolean>
  isSubmitting: Ref<boolean>
  isLoadingImage: Ref<boolean>
  imagePreview: Ref<string>

  // Data state
  articles: Ref<ArticleDto[]>
  categories: Ref<CategoryDto[]>
  selectedCategory: Ref<string | number>

  // Search and filters
  searchTerm: Ref<string>
  sortBy: Ref<SortBy>

  // Computed
  isFormValid: ComputedRef<boolean>
  isNameDuplicate: ComputedRef<boolean>
  filteredArticles: ComputedRef<ArticleDto[]>
  statistics: ComputedRef<ArticleStatistics>

  // Methods
  resetForm: () => void
  handleImageUpload: (file: File) => boolean
  clearImage: () => void
  startEdit: (article: ArticleDto) => void
  cancelEdit: () => void
  filterByCategory: (categoryId: string | number) => void
  getCategoryName: (categoryId: number) => string
  validateForm: () => boolean
  getFormattedData: () => ArticleCreateDto | ArticleCreateWithImageDto
  showSuccess: (message: string) => void
  showError: (message: string) => void
  showInfo: (message: string) => void
  confirmDelete: (article: ArticleDto) => Promise<boolean>
  formatCurrency: (amount: number) => string
  formatPrice: (price: number | string) => string
  getArticleImage: (article: ArticleDto) => string
}

export function useArticleCreate(): UseArticleCreateReturn {
  const toast = useToast()
  const confirm = useConfirm()

  // Form state
  const formData: Ref<ArticleFormData> = ref({
    name: '',
    price: '',
    categoryId: null,
    image: null,
    articuloId: undefined,
  })

  // UI state
  const isEditMode: Ref<boolean> = ref(false)
  const isSubmitting: Ref<boolean> = ref(false)
  const isLoadingImage: Ref<boolean> = ref(false)
  const imagePreview: Ref<string> = ref(new URL('../assets/sin-imagen.png', import.meta.url).href)

  // Data state
  const articles: Ref<ArticleDto[]> = ref([])
  const categories: Ref<CategoryDto[]> = ref([])
  const selectedCategory: Ref<string | number> = ref('all')

  // Search and filters
  const searchTerm: Ref<string> = ref('')
  const sortBy: Ref<SortBy> = ref('name')

  // Validation
  const isFormValid: ComputedRef<boolean> = computed(() => {
    return (
      formData.value.name.trim() !== '' &&
      formData.value.price !== '' &&
      formData.value.categoryId !== null &&
      (isEditMode.value || formData.value.image !== null)
    )
  })

  const isNameDuplicate: ComputedRef<boolean> = computed(() => {
    if (!articles.value || !formData.value.name) return false

    return articles.value.some(
      (article) =>
        article.nombreArticulo.toLowerCase() === formData.value.name.toLowerCase() &&
        article.articuloId !== formData.value.articuloId
    )
  })

  // Computed values
  const filteredArticles: ComputedRef<ArticleDto[]> = computed(() => {
    let filtered = articles.value

    // Search filter
    if (searchTerm.value) {
      const term = searchTerm.value.toLowerCase()
      filtered = filtered.filter(
        (article) =>
          article.nombreArticulo.toLowerCase().includes(term) ||
          article.precio.toString().includes(term)
      )
    }

    // Category filter
    if (selectedCategory.value && selectedCategory.value !== 'all') {
      filtered = filtered.filter((article) => article.categoriaId === selectedCategory.value)
    }

    // Sort
    filtered.sort((a, b) => {
      switch (sortBy.value) {
        case 'price':
          return b.precio - a.precio
        case 'category':
          return (getCategoryName(a.categoriaId) || '').localeCompare(
            getCategoryName(b.categoriaId) || ''
          )
        case 'date':
          const aDate = new Date(a.fechaCreacion).getTime()
          const bDate = new Date(b.fechaCreacion).getTime()
          return bDate - aDate
        case 'name':
        default:
          return a.nombreArticulo.localeCompare(b.nombreArticulo)
      }
    })

    return filtered
  })

  const statistics: ComputedRef<ArticleStatistics> = computed(() => {
    const total = articles.value.length
    const totalValue = articles.value.reduce((sum, article) => sum + article.precio, 0)
    const averagePrice = total > 0 ? totalValue / total : 0

    const categoryStats = categories.value
      .filter((cat) => cat.categoriaId !== null)
      .map((category) => {
        const categoryArticles = articles.value.filter(
          (article) => article.categoriaId === category.categoriaId
        )
        return {
          name: category.nombreCategoria,
          count: categoryArticles.length,
          percentage: total > 0 ? Math.round((categoryArticles.length / total) * 100) : 0,
        }
      })

    return {
      total,
      totalValue: formatCurrency(totalValue),
      averagePrice: formatCurrency(averagePrice),
      categoryStats,
    }
  })

  // Form operations
  const resetForm = (): void => {
    formData.value = {
      name: '',
      price: '',
      categoryId: null,
      image: null,
      articuloId: undefined,
    }
    imagePreview.value = new URL('../assets/sin-imagen.png', import.meta.url).href
    isEditMode.value = false
  }

  const handleImageUpload = (file: File): boolean => {
    if (file) {
      isLoadingImage.value = true

      // Validate file size (5MB max)
      if (file.size > 5 * 1024 * 1024) {
        showError('El archivo es demasiado grande. Máximo 5MB permitido.')
        isLoadingImage.value = false
        return false
      }

      // Validate file type
      if (!file.type.startsWith('image/')) {
        showError('Solo se permiten archivos de imagen.')
        isLoadingImage.value = false
        return false
      }

      try {
        formData.value.image = file

        // Revoke previous object URL to prevent memory leaks
        if (imagePreview.value && imagePreview.value.startsWith('blob:')) {
          URL.revokeObjectURL(imagePreview.value)
        }

        imagePreview.value = URL.createObjectURL(file)
        showInfo('✅ Imagen cargada correctamente')
        isLoadingImage.value = false
        return true
      } catch (error) {
        console.error('Error loading image:', error)
        showError('Error al cargar la imagen')
        isLoadingImage.value = false
        return false
      }
    }
    return false
  }

  const startEdit = (article: ArticleDto): void => {
    isEditMode.value = true
    formData.value = {
      articuloId: article.articuloId,
      name: article.nombreArticulo,
      price: article.precio.toString(),
      categoryId: article.categoriaId,
      image: null,
    }

    imagePreview.value = getArticleImage(article)
  }

  const cancelEdit = (): void => {
    resetForm()
  }

  const clearImage = (): void => {
    // Revoke object URL if it's a blob URL to prevent memory leaks
    if (imagePreview.value && imagePreview.value.startsWith('blob:')) {
      URL.revokeObjectURL(imagePreview.value)
    }

    // Clear the image from form data
    formData.value.image = null

    // Reset to default image
    imagePreview.value = new URL('../assets/sin-imagen.png', import.meta.url).href

    // Show feedback
    showInfo('✅ Imagen removida correctamente')
  }

  // Category operations
  const filterByCategory = (categoryId: string | number): void => {
    selectedCategory.value = categoryId
  }

  const getCategoryName = (categoryId: number): string => {
    const category = categories.value.find((cat) => cat.categoriaId === categoryId)
    return category ? category.nombreCategoria : 'Sin categoría'
  }

  // Validation methods
  const validateForm = (): boolean => {
    if (!isFormValid.value) {
      if (!formData.value.name.trim()) {
        showError('Por favor ingresa un nombre para el artículo')
        return false
      }
      if (!formData.value.price) {
        showError('Por favor ingresa un precio para el artículo')
        return false
      }
      if (!formData.value.categoryId) {
        showError('Por favor selecciona una categoría')
        return false
      }
      if (!isEditMode.value && !formData.value.image) {
        showError('Por favor selecciona una imagen para el artículo')
        return false
      }
    }

    if (isNameDuplicate.value) {
      showError('Ya existe un artículo con este nombre')
      return false
    }

    // Validate price
    const price = parseFloat(formData.value.price)
    if (isNaN(price) || price <= 0) {
      showError('El precio debe ser un número válido mayor a 0')
      return false
    }

    // Additional validation for edit mode
    if (isEditMode.value && !formData.value.articuloId) {
      showError('Error: ID del artículo no encontrado')
      return false
    }

    return true
  }

  // Format data for API - V1 only
  const getFormattedData = (): ArticleCreateDto | ArticleCreateWithImageDto => {
    const baseData = {
      nombreArticulo: formData.value.name.trim(),
      precio: parseFloat(formData.value.price),
      categoriaId: formData.value.categoryId!,
    }

    if (formData.value.image) {
      return {
        ...baseData,
        imagen: formData.value.image,
      } as ArticleCreateWithImageDto
    }

    return baseData as ArticleCreateDto
  }

  // Toast messages
  const showSuccess = (message: string): void => {
    toast.add({
      severity: 'success',
      summary: 'Éxito',
      detail: message,
      life: 5000,
    })
  }

  const showError = (message: string): void => {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: message,
      life: 5000,
    })
  }

  const showInfo = (message: string): void => {
    toast.add({
      severity: 'info',
      summary: 'Información',
      detail: message,
      life: 5000,
    })
  }

  // Confirmation dialogs
  const confirmDelete = (article: ArticleDto): Promise<boolean> => {
    return new Promise((resolve) => {
      confirm.require({
        message: `¿Estás seguro de eliminar el artículo "${article.nombreArticulo}"?`,
        header: 'Confirmar Eliminación',
        icon: 'pi pi-exclamation-triangle',
        acceptLabel: 'Sí, eliminar',
        rejectLabel: 'Cancelar',
        acceptClass: 'p-button-danger',
        accept: () => resolve(true),
        reject: () => resolve(false),
      })
    })
  }

  // Helper methods for display
  const formatCurrency = (amount: number): string => {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP',
      minimumFractionDigits: 0,
    }).format(amount)
  }

  const formatPrice = (price: number | string): string => {
    return formatCurrency(parseFloat(price.toString() || '0'))
  }

  // V1 API only - clean image URL handling
  const getArticleImage = (article: ArticleDto): string => {
    const urlNoImage = new URL('../assets/sin-imagen.png', import.meta.url).href

    try {
      // V1 API direct endpoint for image
      if (article.imagenUrl) {
        const baseUrl = import.meta.env.VITE_API_BASE_URL || ''
        return `${baseUrl}uploads/${article.imagenUrl}`
      }
    } catch (error) {
      console.error('Error getting article image:', error)
    }

    return urlNoImage
  }

  return {
    // Form state
    formData,
    isEditMode,
    isSubmitting,
    isLoadingImage,
    imagePreview,

    // Data state
    articles,
    categories,
    selectedCategory,

    // Search and filters
    searchTerm,
    sortBy,

    // Computed
    isFormValid,
    isNameDuplicate,
    filteredArticles,
    statistics,

    // Methods
    resetForm,
    handleImageUpload,
    clearImage,
    startEdit,
    cancelEdit,
    filterByCategory,
    getCategoryName,
    validateForm,
    getFormattedData,
    showSuccess,
    showError,
    showInfo,
    confirmDelete,
    formatCurrency,
    formatPrice,
    getArticleImage,
  }
}
