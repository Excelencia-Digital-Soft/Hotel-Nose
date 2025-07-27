/**
 * Cierres Composable
 * Handles business logic for cash register closures
 * 
 * 🚀 V1 API Only - Legacy support removed
 */

import { ref, computed } from 'vue';
import { useToast } from 'primevue/usetoast';
import { useAuthStore } from '../store/auth';
import { 
  CierresService, 
  type CajaDto,
  type TransaccionDto,
  type EgresoDto,
  type CierresyActualDto,
  type CierreDetalleCompletoDto
} from '../services/cierresService';
import type { Ref } from 'vue';

export interface UseCierresReturn {
  // State
  cierres: Ref<CajaDto[]>;
  transaccionesPendientes: Ref<TransaccionDto[]>;
  egresosPendientes: Ref<EgresoDto[]>;
  isLoading: Ref<boolean>;
  selectedCierre: Ref<CajaDto | null>;

  // Computed
  hasHistoricalClosures: Ref<boolean>;
  hasCurrentSession: Ref<boolean>;
  totalCierres: Ref<number>;
  canViewHistorical: Ref<boolean>;

  // Methods
  fetchCierres(): Promise<void>;
  fetchDetalleCierre(cajaId: number): Promise<{ pagos: TransaccionDto[]; anulaciones: TransaccionDto[]; egresos: EgresoDto[] } | null>;
  cerrarCaja(montoInicial: number, observacion?: string): Promise<boolean>;
  selectCierre(cierre: CajaDto): void;
  openCurrentSession(): void;
  handlePrint(contentElement: HTMLElement): void;
  initialize(): Promise<void>;

  // Utilities
  formatFechaHora(fechaHora: string): string;
  formatFecha(fechaHora: string): string;
  formatHora(fechaHora: string): string;
  showSuccess(message: string): void;
  showError(message: string): void;
  showInfo(message: string): void;
}

export function useCierres(): UseCierresReturn {
  const toast = useToast();
  const authStore = useAuthStore();

  // State
  const cierres = ref<CajaDto[]>([]);
  const transaccionesPendientes = ref<TransaccionDto[]>([]);
  const egresosPendientes = ref<EgresoDto[]>([]);
  const isLoading = ref<boolean>(false);
  const selectedCierre = ref<CajaDto | null>(null);

  // Computed
  const hasHistoricalClosures = computed<boolean>(() => cierres.value.length > 0);
  const hasCurrentSession = computed<boolean>(() => transaccionesPendientes.value.length > 0 || egresosPendientes.value.length > 0);
  const totalCierres = computed<number>(() => cierres.value.length);

  // Check if user has access to historical closures
  const canViewHistorical = computed<boolean>(() => {
    return authStore.user?.roles && (
      authStore.user.roles.includes('Administrator') || 
      authStore.user.roles.includes('Director')
    );
  });

  // Toast helpers
  const showSuccess = (message: string): void => {
    toast.add({
      severity: 'success',
      summary: 'Éxito',
      detail: message,
      life: 5000,
    });
  };

  const showError = (message: string): void => {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: message,
      life: 5000,
    });
  };

  const showInfo = (message: string): void => {
    toast.add({
      severity: 'info',
      summary: 'Información',
      detail: message,
      life: 3000,
    });
  };

  // Date formatting utilities
  const formatFechaHora = (fechaHora: string): string => {
    if (!fechaHora) return 'Sin fecha';
    const date = new Date(fechaHora);
    return date.toLocaleString('es-ES', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const formatFecha = (fechaHora: string): string => {
    if (!fechaHora) return 'Sin fecha';
    const date = new Date(fechaHora);
    return date.toLocaleDateString('es-ES', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  const formatHora = (fechaHora: string): string => {
    if (!fechaHora) return 'Sin hora';
    const date = new Date(fechaHora);
    return date.toLocaleTimeString('es-ES', {
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  // API Methods - V1 Only
  const fetchCierres = async (): Promise<void> => {
    try {
      isLoading.value = true;
      
      const response = await CierresService.getCierresyActual();

      if (response.isSuccess && response.data) {
        const data = response.data as CierresyActualDto;
        cierres.value = data.cierres || [];
        transaccionesPendientes.value = data.transaccionesPendientes || [];
        egresosPendientes.value = data.egresosPendientes || [];
        
        if (cierres.value.length > 0) {
          showSuccess(`📚 ${cierres.value.length} cierres cargados correctamente`);
        }
      } else {
        console.error('Error fetching cierres:', response.message);
        showError(response.message || '❌ Error al cargar los cierres');
        
        // Show detailed errors if available
        if (response.errors && response.errors.length > 0) {
          response.errors.forEach(error => {
            console.error('Cierre error:', error);
          });
        }
      }
    } catch (error) {
      console.error('Error fetching cierres:', error);
      showError('❌ Error al cargar los cierres');
    } finally {
      isLoading.value = false;
    }
  };

  const fetchDetalleCierre = async (cajaId: number): Promise<{ pagos: TransaccionDto[]; anulaciones: TransaccionDto[]; egresos: EgresoDto[] } | null> => {
    try {
      isLoading.value = true;
      
      const response = await CierresService.getDetalleCierre(cajaId);

      if (response.isSuccess && response.data) {
        const data = response.data as CierreDetalleCompletoDto;
        return {
          pagos: data.pagos || [],
          anulaciones: data.anulaciones || [],
          egresos: data.egresos || []
        };
      } else {
        console.error('Error fetching detalle cierre:', response.message);
        showError(response.message || '❌ Error al cargar el detalle del cierre');
        return null;
      }
    } catch (error) {
      console.error('Error fetching detalle cierre:', error);
      showError('❌ Error al cargar el detalle del cierre');
      return null;
    } finally {
      isLoading.value = false;
    }
  };

  const cerrarCaja = async (montoInicial: number, observacion: string = 'Cierre de caja'): Promise<boolean> => {
    try {
      isLoading.value = true;
      
      const response = await CierresService.cerrarCaja(montoInicial, observacion);

      if (response.isSuccess) {
        showSuccess('✅ Caja cerrada exitosamente');
        await fetchCierres(); // Refresh data
        return true;
      } else {
        console.error('Error closing cash register:', response.message);
        showError(response.message || '❌ Error al cerrar la caja');
        return false;
      }
    } catch (error) {
      console.error('Error closing cash register:', error);
      showError('❌ Error al cerrar la caja');
      return false;
    } finally {
      isLoading.value = false;
    }
  };

  // Modal management
  const selectCierre = (cierre: CajaDto): void => {
    selectedCierre.value = cierre;
    showInfo(`👁️ Abriendo detalles del cierre #${cierre.cajaId}`);
  };

  const openCurrentSession = (): void => {
    showInfo('🟢 Abriendo sesión actual');
  };

  // Print utilities
  const preparePrintContent = (content: HTMLElement): { content: string; styles: string } | null => {
    try {
      // Clone styles from document
      const styles = Array.from(document.styleSheets)
        .map((styleSheet) => {
          try {
            return Array.from(styleSheet.cssRules)
              .map((rule) => rule.cssText)
              .join('\n');
          } catch (error) {
            return ''; // Ignore cross-origin styles
          }
        })
        .join('\n');

      return { content: content.innerHTML, styles };
    } catch (error) {
      console.error('Error preparing print content:', error);
      showError('❌ Error al preparar la impresión');
      return null;
    }
  };

  const openPrintWindow = (htmlContent: string, styles: string): void => {
    try {
      const printWindow = window.open('', '_blank');
      
      if (!printWindow) {
        showError('❌ Error al abrir la ventana de impresión. Verifique si el bloqueador de ventanas emergentes está activado.');
        return;
      }
      
      printWindow.document.write(`
        <html>
          <head>
            <title>Imprimir Cierre de Caja</title>
            <style>${styles}</style>
          </head>
          <body>
            <div id="cierre-caja-content">${htmlContent}</div>
            <script>
              window.onload = function() {
                window.print();
                window.onafterprint = function() { 
                  window.close(); 
                };
              };
            </script> 
          </body>
        </html>
      `);

      printWindow.document.close();
      showSuccess('🖨️ Preparando impresión...');
    } catch (error) {
      console.error('Error opening print window:', error);
      showError('❌ Error al abrir la ventana de impresión');
    }
  };

  const handlePrint = (contentElement: HTMLElement): void => {
    const printData = preparePrintContent(contentElement);
    if (printData) {
      openPrintWindow(printData.content, printData.styles);
    }
  };

  // Initialize
  const initialize = async (): Promise<void> => {
    console.log('🚀 Inicializando módulo de Cierres - V1 API');
    await fetchCierres();
  };

  return {
    // State
    cierres,
    transaccionesPendientes,
    egresosPendientes,
    isLoading,
    selectedCierre,

    // Computed
    hasHistoricalClosures,
    hasCurrentSession,
    totalCierres,
    canViewHistorical,

    // Methods
    fetchCierres,
    fetchDetalleCierre,
    cerrarCaja,
    selectCierre,
    openCurrentSession,
    handlePrint,
    initialize,

    // Utilities
    formatFechaHora,
    formatFecha,
    formatHora,
    showSuccess,
    showError,
    showInfo,
  };
}

/**
 * Export the main composable
 */
export { useCierres as default };