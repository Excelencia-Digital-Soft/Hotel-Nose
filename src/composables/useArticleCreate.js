import { ref, computed } from 'vue'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'

export function useArticleCreate() {
  const toast = useToast()
  const confirm = useConfirm()

  // Form state
  const formData = ref({
    name: '',
    price: '',
    categoryId: null,
    image: null,
    articuloId: null
  })

  // UI state
  const isEditMode = ref(false)
  const isSubmitting = ref(false)
  const isLoadingImage = ref(false)
  const imagePreview = ref(new URL('../assets/sin-imagen.png', import.meta.url).href)

  // Data state
  const articles = ref([])
  const categories = ref([])
  const selectedCategory = ref(null)

  // Search and filters
  const searchTerm = ref('')
  const sortBy = ref('name') // name, price, category, date

  // Validation
  const isFormValid = computed(() => {
    return formData.value.name.trim() !== '' &&
      formData.value.price !== '' &&
      formData.value.categoryId !== null &&
      (isEditMode.value || formData.value.image !== null)
  })

  const isNameDuplicate = computed(() => {
    if (!articles.value || !formData.value.name) return false

    return articles.value.some((article) =>
      article.nombreArticulo.toLowerCase() === formData.value.name.toLowerCase() &&
      article.articuloId !== formData.value.articuloId
    )
  })

  // Computed values
  const filteredArticles = computed(() => {
    let filtered = articles.value

    // Search filter
    if (searchTerm.value) {
      const term = searchTerm.value.toLowerCase()
      filtered = filtered.filter(article =>
        article.nombreArticulo.toLowerCase().includes(term) ||
        article.precio.toString().includes(term)
      )
    }

    // Category filter
    if (selectedCategory.value && selectedCategory.value !== 'all') {
      filtered = filtered.filter(article =>
        (article.categoriaID === selectedCategory.value) ||
        (article.categoriaId === selectedCategory.value)
      )
    }

    // Sort
    filtered.sort((a, b) => {
      switch (sortBy.value) {
        case 'price':
          return parseFloat(b.precio) - parseFloat(a.precio)
        case 'category':
          const aCategoryId = a.categoriaID || a.categoriaId
          const bCategoryId = b.categoriaID || b.categoriaId
          return (getCategoryName(aCategoryId) || '').localeCompare(getCategoryName(bCategoryId) || '')
        case 'name':
        default:
          return a.nombreArticulo.localeCompare(b.nombreArticulo)
      }
    })

    return filtered
  })

  const statistics = computed(() => {
    const total = articles.value.length
    const totalValue = articles.value.reduce((sum, article) => sum + parseFloat(article.precio || 0), 0)
    const averagePrice = total > 0 ? totalValue / total : 0

    const categoryStats = categories.value.filter(cat => cat.categoriaId !== null).map(category => {
      const categoryArticles = articles.value.filter(article =>
        (article.categoriaID === category.categoriaId) ||
        (article.categoriaId === category.categoriaId)
      )
      return {
        name: category.nombreCategoria,
        count: categoryArticles.length,
        percentage: total > 0 ? Math.round((categoryArticles.length / total) * 100) : 0
      }
    })

    return {
      total,
      totalValue: formatCurrency(totalValue),
      averagePrice: formatCurrency(averagePrice),
      categoryStats
    }
  })

  // Form operations
  const resetForm = () => {
    formData.value = {
      name: '',
      price: '',
      categoryId: null,
      image: null,
      articuloId: null
    }
    imagePreview.value = new URL('../assets/sin-imagen.png', import.meta.url).href
    isEditMode.value = false
  }

  const handleImageUpload = (file) => {
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

  const startEdit = (article) => {
    isEditMode.value = true
    formData.value = {
      articuloId: article.articuloId,
      name: article.nombreArticulo,
      price: article.precio.toString(),
      categoryId: article.categoriaID || article.categoriaId,
      image: null
    }

    // If article has an image, show it
    if (article.imagen) {
      imagePreview.value = article.imagen
    } else {
      imagePreview.value = new URL('../assets/sin-imagen.png', import.meta.url).href
    }
  }

  const cancelEdit = () => {
    resetForm()
  }

  const clearImage = () => {
    if (imagePreview.value && imagePreview.value.startsWith('blob:')) {
      URL.revokeObjectURL(imagePreview.value)
    }
    formData.value.image = null
    imagePreview.value = new URL('../assets/sin-imagen.png', import.meta.url).href
  }

  // Category operations
  const filterByCategory = (categoryId) => {
    selectedCategory.value = categoryId
  }

  const getCategoryName = (categoryId) => {
    const category = categories.value.find(cat => cat.categoriaId === categoryId)
    return category ? category.nombreCategoria : 'Sin categoría'
  }

  // Validation methods
  const validateForm = () => {
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

    return true
  }

  // Format data for API
  const getFormattedData = () => {
    return {
      nombre: formData.value.name.trim(),
      precio: parseFloat(formData.value.price),
      categoriaID: formData.value.categoryId,
      imagen: formData.value.image
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

  // Confirmation dialogs
  const confirmDelete = (article) => {
    return new Promise((resolve) => {
      confirm.require({
        message: `¿Estás seguro de eliminar el artículo "${article.nombreArticulo}"?`,
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
  const formatCurrency = (amount) => {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP',
      minimumFractionDigits: 0
    }).format(amount)
  }

  const formatPrice = (price) => {
    return formatCurrency(parseFloat(price || 0))
  }

  const getArticleImage = (article) => {
    const urlNoImage = new URL('../assets/sin-imagen.png', import.meta.url).href

    // For V1 API, use the new image URL structure
    if (article.articuloId && article.imagenUrl) {
      const baseUrl = import.meta.env.VITE_API_BASE_URL || ''
      return `${baseUrl}uploads/${article.imagenUrl}`
    } else if (article.imagenAPI) {
      return article.imagenAPI
    }

    return article.imagenUrl || urlNoImage
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
    getArticleImage
  }
}
