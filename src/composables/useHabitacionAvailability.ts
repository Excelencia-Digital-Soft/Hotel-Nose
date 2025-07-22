import { ref } from 'vue';
import { ReservasService } from '../services/reservasService';
import { useToast } from 'primevue/usetoast';
import { Habitacion, UseHabitacionAvailabilityReturn, ToastMessage } from '../types';

export function useHabitacionAvailability(): UseHabitacionAvailabilityReturn {
  const toast = useToast();
  const loading = ref<boolean>(false);
  const showModal = ref<boolean>(false);
  const selectedRoom = ref<Habitacion | null>(null);
  const motivo = ref<string>('');

  const openModal = (room: Habitacion): void => {
    selectedRoom.value = room;
    motivo.value = '';
    showModal.value = true;
  };

  const closeModal = (): void => {
    showModal.value = false;
    selectedRoom.value = null;
    motivo.value = '';
  };

  const comprehensiveCancel = async (
    reservaId: number, 
    motivoText: string, 
    onSuccess?: () => void
  ): Promise<void> => {
    loading.value = true;
    try {
      const response = await ReservasService.comprehensiveCancel(reservaId, motivoText);
      
      if (response.isSuccess) {
        const successMessage: ToastMessage = {
          severity: 'success',
          summary: 'Éxito',
          detail: 'Ocupación anulada correctamente. Se han cancelado todos los consumos asociados.',
          life: 3000
        };
        toast.add(successMessage);
        
        closeModal();
        
        if (onSuccess && typeof onSuccess === 'function') {
          onSuccess();
        }
      } else {
        const errorMessage: ToastMessage = {
          severity: 'error',
          summary: 'Error',
          detail: response.message || 'Error al anular la ocupación',
          life: 5000
        };
        toast.add(errorMessage);
      }
    } catch (error: unknown) {
      console.error('Error in comprehensive cancel:', error);
      const errorMessage: ToastMessage = {
        severity: 'error',
        summary: 'Error',
        detail: error instanceof Error && 'response' in error 
          ? (error as any).response?.data?.message || 'Error al anular la ocupación'
          : 'Error al anular la ocupación',
        life: 5000
      };
      toast.add(errorMessage);
    } finally {
      loading.value = false
    }
  }

  const anularOcupacion = async (habitacion: Habitacion, onSuccess?: () => void): Promise<void> => {
    if (!habitacion || !motivo.value.trim()) {
      const warningMessage: ToastMessage = {
        severity: 'warn',
        summary: 'Advertencia',
        detail: 'Debe ingresar un motivo para anular la ocupación',
        life: 3000
      };
      toast.add(warningMessage);
      return;
    }

    // Use comprehensive cancel endpoint that also cancels consumos
    if (habitacion.ReservaID) {
      await comprehensiveCancel(habitacion.ReservaID, motivo.value, onSuccess);
    }
  };

  return {
    loading,
    showModal,
    selectedRoom,
    motivo,
    openModal,
    closeModal,
    anularOcupacion
  }
}