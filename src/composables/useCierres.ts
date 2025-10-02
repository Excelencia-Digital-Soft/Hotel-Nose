/**
 * Cierres Composable
 * Handles business logic for cash register closures
 */

import { ref, computed } from 'vue'
import { useToast } from 'primevue/usetoast'
// @ts-ignore
import { useAuthStore } from '../store/auth.js'
import { CierresService } from '../services/cierresService'
import type {
  CierreBasicoDto,
  TransaccionPendienteDto,
  EgresoDetalleDto,
  CierresyActualDto,
  CierreDetalleCompletoDto,
  CierresPaginatedDto,
  PaginationParams,
} from '../types/cierre-model'
import type { Ref } from 'vue'

export interface UseCierresReturn {
  // State
  cierres: Ref<CierreBasicoDto[]>
  transaccionesPendientes: Ref<TransaccionPendienteDto[]>
  egresosPendientes: Ref<EgresoDetalleDto[]>
  isLoading: Ref<boolean>
  selectedCierre: Ref<CierreBasicoDto | null>

  // Pagination State
  currentPage: Ref<number>
  pageSize: Ref<number>
  totalRecords: Ref<number>
  totalPages: Ref<number>

  // Computed
  hasHistoricalClosures: Ref<boolean>
  hasCurrentSession: Ref<boolean>
  totalCierres: Ref<number>
  canCloseCash: Ref<boolean>
  canViewHistorical: Ref<boolean>
  canGoNext: Ref<boolean>
  canGoPrevious: Ref<boolean>

  // Methods
  fetchCierres(): Promise<void>
  fetchCierresHistoricos(page?: number): Promise<void>
  fetchDetalleCierre(cierreId: number): Promise<{
    pagos: TransaccionPendienteDto[]
    anulaciones: TransaccionPendienteDto[]
    egresos: EgresoDetalleDto[]
  } | null>
  cerrarCaja(montoInicial: number, observacion?: string): Promise<boolean>
  selectCierre(cierre: CierreBasicoDto): void
  openCurrentSession(): void
  handlePrint(contentElement: HTMLElement): void
  initialize(): Promise<void>

  // Pagination Methods
  goToPage(page: number): Promise<void>
  nextPage(): Promise<void>
  previousPage(): Promise<void>
  setPageSize(size: number): Promise<void>

  // Utilities
  formatFechaHora(fechaHora: string): string
  formatFecha(fechaHora: string): string
  formatHora(fechaHora: string): string
  showSuccess(message: string): void
  showError(message: string): void
  showInfo(message: string): void
}

export function useCierres(): UseCierresReturn {
  const toast = useToast()
  const authStore = useAuthStore()

  // State
  const cierres = ref<CierreBasicoDto[]>([])
  const transaccionesPendientes = ref<TransaccionPendienteDto[]>([])
  const egresosPendientes = ref<EgresoDetalleDto[]>([])
  const isLoading = ref<boolean>(false)
  const selectedCierre = ref<CierreBasicoDto | null>(null)

  // Pagination State
  const currentPage = ref<number>(1)
  const pageSize = ref<number>(10)
  const totalRecords = ref<number>(0)
  const totalPages = ref<number>(0)

  // Computed
  const hasHistoricalClosures = computed<boolean>(() => cierres.value.length > 0)
  const hasCurrentSession = computed<boolean>(
    () => transaccionesPendientes.value.length > 0 || egresosPendientes.value.length > 0
  )
  const totalCierres = computed<number>(() => totalRecords.value || cierres.value.length)
  const canGoNext = computed<boolean>(() => currentPage.value < totalPages.value)
  const canGoPrevious = computed<boolean>(() => currentPage.value > 1)

  // Check if user can close cash register (any authenticated user with roles)
  const canCloseCash = computed<boolean>(() => {
    return authStore.user?.roles && authStore.user.roles.length > 0
  })

  // Check if user has access to historical closures
  const canViewHistorical = computed<boolean>(() => {
    return (
      authStore.user?.roles &&
      (authStore.user.roles.includes('Administrator') || authStore.user.roles.includes('Director'))
    )
  })

  // Toast helpers
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
      life: 3000,
    })
  }

  // Date formatting utilities
  const formatFechaHora = (fechaHora: string): string => {
    if (!fechaHora) return 'Sin fecha'
    const date = new Date(fechaHora)
    return date.toLocaleString('es-ES', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    })
  }

  const formatFecha = (fechaHora: string): string => {
    if (!fechaHora) return 'Sin fecha'
    const date = new Date(fechaHora)
    return date.toLocaleDateString('es-ES', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    })
  }

  const formatHora = (fechaHora: string): string => {
    if (!fechaHora) return 'Sin hora'
    const date = new Date(fechaHora)
    return date.toLocaleTimeString('es-ES', {
      hour: '2-digit',
      minute: '2-digit',
    })
  }

  // API Methods
  const fetchCierres = async (page: number = 1): Promise<void> => {
    try {
      isLoading.value = true

      const response = await CierresService.getCierresyActual()

      if (response.isSuccess && response.data) {
        const data = response.data as CierresyActualDto
        const allCierres = data.cierres || []
        transaccionesPendientes.value = data.transaccionesPendientes || []
        egresosPendientes.value = data.egresosPendientes || []

        // Apply frontend pagination for current view
        const startIndex = (page - 1) * pageSize.value
        const endIndex = startIndex + pageSize.value
        cierres.value = allCierres.slice(startIndex, endIndex)

        // Update pagination state
        totalRecords.value = allCierres.length
        totalPages.value = Math.ceil(allCierres.length / pageSize.value)
        currentPage.value = page

        if (allCierres.length > 0) {
          showSuccess(
            `📚 ${cierres.value.length} de ${allCierres.length} cierres cargados (Página ${page} de ${totalPages.value})`
          )
        }
      } else {
        console.error('Error fetching cierres:', response.message)
        showError(response.message || '❌ Error al cargar los cierres')

        // Reset pagination state on error
        cierres.value = []
        totalRecords.value = 0
        totalPages.value = 0
        currentPage.value = 1

        // Show detailed errors if available
        if (response.errors && response.errors.length > 0) {
          response.errors.forEach((error) => {
            console.error('Cierre error:', error)
          })
        }
      }
    } catch (error) {
      console.error('Error fetching cierres:', error)
      showError('❌ Error al cargar los cierres')

      // Reset pagination state on exception
      cierres.value = []
      totalRecords.value = 0
      totalPages.value = 0
      currentPage.value = 1
    } finally {
      isLoading.value = false
    }
  }

  // Fetch paginated historical closures
  const fetchCierresHistoricos = async (page: number = currentPage.value): Promise<void> => {
    try {
      isLoading.value = true

      const params: PaginationParams = {
        page,
        pageSize: pageSize.value,
      }

      let response

      try {
        // Try the paginated endpoint first
        response = await CierresService.getCierresHistoricos(params)
        console.log('📊 V1 Paginated API Response:', response) // Debug log
      } catch (paginatedError: any) {
        console.warn(
          '⚠️ Paginated endpoint failed, falling back to current endpoint:',
          paginatedError
        )

        // Only fallback if it's a known issue (endpoint not available)
        if (
          paginatedError.message === 'ENDPOINT_NOT_AVAILABLE' ||
          paginatedError.response?.status === 404 ||
          paginatedError.response?.status === 405
        ) {
          showInfo(
            '📋 Usando datos actuales para vista histórica (endpoint de paginación no disponible)'
          )

          // Fallback to the current endpoint if paginated is not available
          response = await CierresService.getCierresyActual()
          console.log('📊 Fallback API Response:', response) // Debug log

          // For fallback, we'll simulate pagination on the frontend
          if (response.isSuccess && response.data) {
            const allData = response.data as CierresyActualDto
            const allCierres = allData.cierres || []

            // Simulate pagination
            const startIndex = (page - 1) * pageSize.value
            const endIndex = startIndex + pageSize.value
            const paginatedCierres = allCierres.slice(startIndex, endIndex)

            cierres.value = paginatedCierres
            totalRecords.value = allCierres.length
            totalPages.value = Math.ceil(allCierres.length / pageSize.value)
            currentPage.value = page

            showSuccess(
              `📚 ${paginatedCierres.length} cierres cargados (Página ${page} de ${totalPages.value})`
            )
            return
          }
        } else {
          // If it's a different error, re-throw it
          throw paginatedError
        }
      }

      if (response.isSuccess && response.data) {
        console.log('📊 Response data:', response.data) // Debug log

        const data = response.data

        // Handle different possible response structures
        if (Array.isArray(data)) {
          console.log('📊 Response data is an array:', data) // Debug log
          // If data is directly an array of closures
          cierres.value = data
          totalRecords.value = data.length
          totalPages.value = 1
          currentPage.value = 1
          showSuccess(`📚 ${data.length} cierres cargados`)
        } else if (data && typeof data === 'object') {
          // If data is an object with pagination info
          console.log('📊 Response data is a nested object:', data) // Debug log
          const paginatedData = data as CierresPaginatedDto
          console.log('📊 Paginated data:', paginatedData) // Debug log
          cierres.value = paginatedData.data.cierres || []
          console.log('📊 Cierres:', cierres.value) // Debug log

          // Handle nested pagination object structure
          if (paginatedData.pagination) {
            totalRecords.value = paginatedData.pagination.totalRecords || 0
            totalPages.value = paginatedData.pagination.totalPages || 1
            currentPage.value = paginatedData.pagination.pageNumber || page
          } else {
            // Fallback for flat structure
            console.warn('⚠️ Paginated data is a flat object, falling back to totalRecords')
            totalRecords.value =
              (paginatedData as any).totalRecords ||
              (paginatedData as any).total ||
              cierres.value.length
            totalPages.value =
              (paginatedData as any).totalPages || Math.ceil(totalRecords.value / pageSize.value)
            currentPage.value =
              (paginatedData as any).pageNumber || (paginatedData as any).page || page
          }

          showSuccess(
            `📚 ${cierres.value.length} cierres cargados (Página ${currentPage.value} de ${totalPages.value})`
          )
        } else {
          console.warn('⚠️ Unexpected response format:', data)
          cierres.value = []
          totalRecords.value = 0
          totalPages.value = 0
          currentPage.value = 1
          showError('❌ Formato de respuesta inesperado')
        }
      } else {
        console.error('❌ API Error:', response.message)
        showError(response.message || '❌ Error al cargar los cierres históricos')

        // Reset pagination state on error
        cierres.value = []
        totalRecords.value = 0
        totalPages.value = 0
        currentPage.value = 1

        // Show detailed errors if available
        if (response.errors && response.errors.length > 0) {
          response.errors.forEach((error) => {
            console.error('Cierre error:', error)
          })
        }
      }
    } catch (error) {
      console.error('❌ Exception fetching cierres históricos:', error)
      showError('❌ Error al cargar los cierres históricos')

      // Reset pagination state on exception
      cierres.value = []
      totalRecords.value = 0
      totalPages.value = 0
      currentPage.value = 1
    } finally {
      isLoading.value = false
    }
  }

  const fetchDetalleCierre = async (
    cierreId: number
  ): Promise<{
    pagos: TransaccionPendienteDto[]
    anulaciones: TransaccionPendienteDto[]
    egresos: EgresoDetalleDto[]
  } | null> => {
    try {
      isLoading.value = true

      const response = await CierresService.getDetalleCierre(cierreId)

      if (response.isSuccess && response.data) {
        const data = response.data as CierreDetalleCompletoDto
        return {
          pagos: data.pagos || [],
          anulaciones: data.anulaciones || [],
          egresos: data.egresos || [],
        }
      } else {
        console.error('Error fetching detalle cierre:', response.message)
        showError(response.message || '❌ Error al cargar el detalle del cierre')
        return null
      }
    } catch (error) {
      console.error('Error fetching detalle cierre:', error)
      showError('❌ Error al cargar el detalle del cierre')
      return null
    } finally {
      isLoading.value = false
    }
  }

  const cerrarCaja = async (
    montoInicial: number,
    observacion: string = 'Cierre de caja'
  ): Promise<boolean> => {
    try {
      isLoading.value = true

      const response = await CierresService.cerrarCaja(montoInicial, observacion)

      if (response.isSuccess) {
        showSuccess('✅ Caja cerrada exitosamente')
        await fetchCierres() // Refresh data
        return true
      } else {
        console.error('Error closing cash register:', response.message)
        showError(response.message || '❌ Error al cerrar la caja')
        return false
      }
    } catch (error) {
      console.error('Error closing cash register:', error)
      showError('❌ Error al cerrar la caja')
      return false
    } finally {
      isLoading.value = false
    }
  }

  // Modal management
  const selectCierre = (cierre: CierreBasicoDto): void => {
    selectedCierre.value = cierre
    showInfo(`👁️ Abriendo detalles del cierre #${cierre.cierreId}`)
  }

  const openCurrentSession = (): void => {
    showInfo('🟢 Abriendo sesión actual')
  }

  // Print utilities
  // const preparePrintContent = (
  //   content: HTMLElement
  // ): { content: string; styles: string } | null => {
  //   try {
  //     // Clone styles from document
  //     const styles = Array.from(document.styleSheets)
  //       .map((styleSheet) => {
  //         try {
  //           return Array.from(styleSheet.cssRules)
  //             .map((rule) => rule.cssText)
  //             .join('\n')
  //         } catch (error) {
  //           return '' // Ignore cross-origin styles
  //         }
  //       })
  //       .join('\n')

  //     return { content: content.innerHTML, styles }
  //   } catch (error) {
  //     console.error('Error preparing print content:', error)
  //     showError('❌ Error al preparar la impresión')
  //     return null
  //   }
  // }

  // const openPrintWindow = (htmlContent: string, styles: string): void => {
  //   try {
  //     const printWindow = window.open('', '_blank')

  //     if (!printWindow) {
  //       showError(
  //         '❌ Error al abrir la ventana de impresión. Verifique si el bloqueador de ventanas emergentes está activado.'
  //       )
  //       return
  //     }

  //     printWindow.document.write(`
  //       <html>
  //         <head>
  //           <title>Imprimir Cierre de Caja</title>
  //           <style>${styles}</style>
  //         </head>
  //         <body>
  //           <div id="cierre-caja-content">${htmlContent}</div>
  //           <script>
  //             window.onload = function() {
  //               window.print();
  //               window.onafterprint = function() { 
  //                 window.close(); 
  //               };
  //             };
  //           </script> 
  //         </body>
  //       </html>
  //     `)

  //     printWindow.document.close()
  //     showSuccess('🖨️ Preparando impresión...')
  //   } catch (error) {
  //     console.error('Error opening print window:', error)
  //     showError('❌ Error al abrir la ventana de impresión')
  //   }
  // }

  // const handlePrint = (contentElement: HTMLElement): void => {
  //   const printData = preparePrintContent(contentElement)
  //   if (printData) {
  //     openPrintWindow(printData.content, printData.styles)
  //   }
  // }

  const handlePrint = (contentElement: HTMLElement): void => {

    if (!contentElement) return;

    const printWindow = window.open('', '', 'width=900,height=650');
    if (!printWindow) return;

     printWindow.document.write(`
      <html>
        <head>
          <title>Imprimir Tabla</title>
          <style>
            /* General */
            * {
              color: black !important;   /* todo en blanco y negro */
              background: white !important;
              -webkit-print-color-adjust: exact !important;
              print-color-adjust: exact !important;
            }

            body {
              font-family: Arial, sans-serif;
              margin: 0;      /* sin márgenes */
              padding: 20px;  /* un poco de aire */
            }

            /* Tabla ocupa todo el ancho */
            table {
              border-collapse: collapse;
              width: 100%;
              margin: 0;
            }

            th, td {
              border: 1px solid #000;
              padding: 6px 8px;
              font-size: 12px;
            }

            th {
              font-weight: bold;
              text-align: left;
            }

            /* Pie de tabla (totales) bien marcado */
            tfoot td {
              font-weight: bold;
              border-top: 2px solid #000;
            }

            @page {
              margin: 10mm; /* márgenes mínimos en impresión */
            }
          </style>
        </head>
        <body>
          ${contentElement.outerHTML}
        </body>
      </html>
    `)

    printWindow.document.close();
    printWindow.focus();
    printWindow.print();
    printWindow.close();

  }


  // Pagination Methods
  const goToPage = async (page: number, viewMode: string = 'historical'): Promise<void> => {
    if (page >= 1 && page <= totalPages.value) {
      if (viewMode === 'current') {
        await fetchCierres(page)
      } else {
        await fetchCierresHistoricos(page)
      }
    }
  }

  const nextPage = async (viewMode: string = 'historical'): Promise<void> => {
    if (canGoNext.value) {
      await goToPage(currentPage.value + 1, viewMode)
    }
  }

  const previousPage = async (viewMode: string = 'historical'): Promise<void> => {
    if (canGoPrevious.value) {
      await goToPage(currentPage.value - 1, viewMode)
    }
  }

  const setPageSize = async (size: number, viewMode: string = 'historical'): Promise<void> => {
    pageSize.value = size
    currentPage.value = 1 // Reset to first page
    if (viewMode === 'current') {
      await fetchCierres(1)
    } else {
      await fetchCierresHistoricos(1)
    }
  }

  // Initialize
  const initialize = async (): Promise<void> => {
    console.log('🚀 Inicializando módulo de Cierres - V1 API')
    await fetchCierres()
  }

  return {
    // State
    cierres,
    transaccionesPendientes,
    egresosPendientes,
    isLoading,
    selectedCierre,

    // Pagination State
    currentPage,
    pageSize,
    totalRecords,
    totalPages,

    // Computed
    hasHistoricalClosures,
    hasCurrentSession,
    totalCierres,
    canCloseCash,
    canViewHistorical,
    canGoNext,
    canGoPrevious,

    // Methods
    fetchCierres,
    fetchCierresHistoricos,
    fetchDetalleCierre,
    cerrarCaja,
    selectCierre,
    openCurrentSession,
    handlePrint,
    initialize,

    // Pagination Methods
    goToPage,
    nextPage,
    previousPage,
    setPageSize,

    // Utilities
    formatFechaHora,
    formatFecha,
    formatHora,
    showSuccess,
    showError,
    showInfo,
  }
}

/**
 * Export the main composable
 */
export { useCierres as default }
