/**
 * Room Actions Composable
 * Handles room-related actions like reservations, checkouts, and time extensions
 */

import { ref } from 'vue';
import { useRoomsStore } from '../../store/modules/roomsStore';
import { roomsService } from '../../services/roomsService';
import { showSuccessToast, showErrorToast } from '../../utils/toast';

export function useRoomActions() {
  const roomsStore = useRoomsStore();

  // Modal states
  const selectedRoom = ref(null);
  const showOccupiedModal = ref(false);
  const showFreeModal = ref(false);

  // Loading states
  const isProcessing = ref(false);

  /**
   * Open modal for occupied room management
   */
  const openOccupiedRoomModal = (room) => {
    if (!room) {
      console.error('Room data is required to open occupied room modal');
      return;
    }
    
    selectedRoom.value = room;
    showOccupiedModal.value = true;
    showFreeModal.value = false;
    document.body.style.overflow = 'hidden';
  };

  /**
   * Open modal for free room reservation
   */
  const openFreeRoomModal = (room) => {
    selectedRoom.value = room;
    showFreeModal.value = true;
    showOccupiedModal.value = false;
    document.body.style.overflow = 'hidden';
  };

  /**
   * Close all modals
   */
  const closeModals = () => {
    showOccupiedModal.value = false;
    showFreeModal.value = false;
    selectedRoom.value = null;
    document.body.style.overflow = 'auto';
  };

  /**
   * Handle room reservation
   */
  const handleRoomReservation = async (roomId, reservationData) => {
    isProcessing.value = true;

    try {
      const result = await roomsService.reserveRoom({
        roomId,
        ...reservationData
      });

      if (result.isSuccess) {
        // Update store state
        roomsStore.markRoomAsOccupied(roomId, reservationData);

        // Show success message
        showSuccessToast('Habitación reservada exitosamente');

        // Close modal
        closeModals();

        // Refresh data after a short delay
        setTimeout(() => {
          roomsStore.refreshRooms();
        }, 1000);

        return true;
      } else {
        showErrorToast(result.message);
        return false;
      }
    } catch (error) {
      showErrorToast('Error al reservar la habitación');
      console.error('Reservation error:', error);
      return false;
    } finally {
      isProcessing.value = false;
    }
  };

  /**
   * Handle room checkout
   */
  const handleRoomCheckout = async (roomId) => {
    isProcessing.value = true;

    try {
      const result = await roomsService.checkoutRoom(roomId);

      if (result.isSuccess) {
        // Update store state
        roomsStore.markRoomAsFree(roomId);

        // Show success message
        showSuccessToast('Check-out realizado exitosamente');

        // Close modal
        closeModals();

        return true;
      } else {
        showErrorToast(result.message);
        return false;
      }
    } catch (error) {
      showErrorToast('Error al realizar el check-out');
      console.error('Checkout error:', error);
      return false;
    } finally {
      isProcessing.value = false;
    }
  };

  /**
   * Handle adding extra time to reservation
   */
  const handleAddExtraTime = async (reservationId, hours, minutes) => {
    isProcessing.value = true;

    try {
      const result = await roomsService.addExtraTime(reservationId, hours, minutes);

      if (result.isSuccess) {
        // Update store state
        roomsStore.addExtraTime(reservationId, hours, minutes);

        // Show success message
        showSuccessToast(`Tiempo agregado: ${hours}h ${minutes}m`);

        return true;
      } else {
        showErrorToast(result.message);
        return false;
      }
    } catch (error) {
      showErrorToast('Error al agregar tiempo extra');
      console.error('Add time error:', error);
      return false;
    } finally {
      isProcessing.value = false;
    }
  };

  /**
   * Refresh rooms data using optimized loading
   */
  const refreshRooms = async () => {
    const startTime = performance.now();
    
    const success = await roomsStore.refreshRoomsOptimized();

    const endTime = performance.now();

    if (success) {
      showSuccessToast('Datos actualizados correctamente');
    } else {
      showErrorToast('Error al actualizar los datos');
    }

    return success;
  };

  /**
   * Handle room update from external events (WebSocket, etc.)
   */
  const handleRoomUpdate = (updatedRoom) => {
    roomsStore.updateRoom(updatedRoom);

    // If the updated room is currently selected, update the selected room
    if (selectedRoom.value && selectedRoom.value.habitacionId === updatedRoom.habitacionId) {
      selectedRoom.value = { ...selectedRoom.value, ...updatedRoom };
    }
  };

  /**
   * Handle bulk actions
   */
  const handleBulkCheckout = async (roomIds) => {
    isProcessing.value = true;
    const results = [];

    try {
      for (const roomId of roomIds) {
        const result = await roomsService.checkoutRoom(roomId);
        results.push({ roomId, success: result.isSuccess });

        if (result.isSuccess) {
          roomsStore.markRoomAsFree(roomId);
        }
      }

      const successCount = results.filter(r => r.success).length;
      const failureCount = results.length - successCount;

      if (successCount > 0) {
        showSuccessToast(`${successCount} habitaciones liberadas exitosamente`);
      }

      if (failureCount > 0) {
        showErrorToast(`Error en ${failureCount} habitaciones`);
      }

      return results;
    } catch (error) {
      showErrorToast('Error en la operación masiva');
      console.error('Bulk checkout error:', error);
      return [];
    } finally {
      isProcessing.value = false;
    }
  };

  return {
    // Modal states
    selectedRoom,
    showOccupiedModal,
    showFreeModal,
    isProcessing,

    // Modal actions
    openOccupiedRoomModal,
    openFreeRoomModal,
    closeModals,

    // Room actions
    handleRoomReservation,
    handleRoomCheckout,
    handleAddExtraTime,
    handleRoomUpdate,
    refreshRooms,

    // Bulk actions
    handleBulkCheckout
  };
}
