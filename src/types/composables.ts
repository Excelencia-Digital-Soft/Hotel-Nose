import { Ref } from 'vue';
import { Habitacion } from './api';

// Composable return types
export interface UseHabitacionAvailabilityReturn {
  loading: Ref<boolean>;
  showModal: Ref<boolean>;
  selectedRoom: Ref<Habitacion | null>;
  motivo: Ref<string>;
  openModal: (room: Habitacion) => void;
  closeModal: () => void;
  anularOcupacion: (habitacion: Habitacion, onSuccess?: () => void) => Promise<void>;
}

// Toast notification interfaces
export interface ToastMessage {
  severity: 'success' | 'error' | 'warn' | 'info';
  summary: string;
  detail: string;
  life?: number;
}

// Modal props interfaces
export interface AnularOcupacionModalProps {
  visible: boolean;
  habitacion: Habitacion | null;
}

// Modal emits interface
export interface AnularOcupacionModalEmits {
  (e: 'close-modal'): void;
  (e: 'ocupacion-anulada', habitacionId: number): void;
}