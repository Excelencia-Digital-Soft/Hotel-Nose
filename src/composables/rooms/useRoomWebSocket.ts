/**
 * Room WebSocket Composable
 * Handles real-time updates for rooms via WebSocket connections
 */

import { onMounted, onUnmounted } from 'vue';
import { useWebSocketStore } from '../../store/websocket';
import { useRoomsStore } from '../../store/modules/roomsStore';
import { configurationService } from '../../services/configurationService';
import type { WebSocketEvent } from '../../types';

export function useRoomWebSocket() {
  const websocketStore = useWebSocketStore();
  const roomsStore = useRoomsStore();

  /**
   * Handle WebSocket events related to rooms with optimized loading
   */
  const handleWebSocketEvent = (data: WebSocketEvent): void => {
    console.log('🔄 Room WebSocket event received:', data.type);

    switch (data.type) {
      case 'warning':
      case 'ended':
        // For time-related events, only refresh occupied rooms
        console.log('🔄 Refreshing occupied rooms due to time event');
        roomsStore.fetchRoomsConditional({ loadOccupiedOnly: true });
        break;

      case 'room_updated':
        // For general room updates, use optimized parallel loading
        console.log('🔄 Refreshing all rooms due to room update');
        roomsStore.refreshRoomsOptimized();
        break;

      case 'room_reserved':
        if (data.roomId && data.reservationData) {
          // Move room from free to occupied
          roomsStore.markRoomAsOccupied(data.roomId, data.reservationData);
          console.log('🔄 Room marked as occupied via WebSocket:', data.roomId);
        } else {
          // Fallback: refresh free rooms only
          roomsStore.fetchRoomsConditional({ loadFreeOnly: true });
        }
        break;

      case 'room_checkout':
        if (data.roomId) {
          // Move room from occupied to free
          roomsStore.markRoomAsFree(data.roomId);
          console.log('🔄 Room marked as free via WebSocket:', data.roomId);
        } else {
          // Fallback: refresh both types
          roomsStore.refreshRoomsOptimized();
        }
        break;

      case 'time_extended':
        if (data.reservationId && data.hours !== undefined && data.minutes !== undefined) {
          // Update time directly in store (no API call needed)
          roomsStore.addExtraTime(data.reservationId, data.hours, data.minutes);
          console.log('🔄 Time extended via WebSocket:', data.reservationId);
        }
        break;

      case 'pending_orders_updated':
        if (data.roomId) {
          // Update only the specific room's pending orders status
          const room = roomsStore.occupiedRooms.find(r => r.habitacionId === data.roomId);
          if (room) {
            roomsStore.updateRoom({
              ...room,
              pedidosPendientes: data.hasPendingOrders
            });
            console.log('🔄 Pending orders updated via WebSocket:', data.roomId);
          }
        }
        break;

      case 'bulk_update':
        // For bulk updates, use parallel loading
        console.log('🔄 Bulk update received, refreshing all rooms');
        roomsStore.refreshRoomsOptimized();
        break;

      default:
        console.log('⚠️ Unhandled room WebSocket event type:', data.type);
    }
  };

  /**
   * Get and set timer update interval from configuration
   */
  const setupTimerInterval = async (): Promise<number> => {
    try {
      const result = await configurationService.getTimerUpdateInterval();

      if (result.isSuccess) {
        localStorage.setItem('timerUpdateInterval', result.data.toString());
        console.log('Room timer interval set to:', result.data, 'minutes');
        return result.data;
      } else {
        console.log(result.message);
        result.errors.forEach(error => console.error('Timer interval error:', error));

        // Use default value
        localStorage.setItem('timerUpdateInterval', result.data.toString());
        console.log('Room timer interval set to default:', result.data, 'minutes');
        return result.data;
      }
    } catch (error) {
      console.error('Error setting up timer interval:', error);
      const defaultInterval = 10;
      localStorage.setItem('timerUpdateInterval', defaultInterval.toString());
      return defaultInterval;
    }
  };

  /**
   * Initialize WebSocket connection for rooms
   */
  const initializeWebSocket = (): void => {
    console.log('🔹 Registering WebSocket event listener for Rooms');
    websocketStore.registerEventCallback('RoomsModule', handleWebSocketEvent);
  };

  /**
   * Cleanup WebSocket connection
   */
  const cleanupWebSocket = (): void => {
    console.log('❌ Unregistering WebSocket event listener for Rooms');
    websocketStore.unregisterEventCallback('RoomsModule');
  };

  /**
   * Setup real-time monitoring
   */
  const setupRealTimeMonitoring = async (): Promise<void> => {
    // Initialize WebSocket
    initializeWebSocket();

    // Setup timer interval
    await setupTimerInterval();
  };

  /**
   * Lifecycle hooks
   */
  onMounted(async () => {
    await setupRealTimeMonitoring();
  });

  onUnmounted(() => {
    cleanupWebSocket();
  });

  return {
    // Event handlers
    handleWebSocketEvent,

    // Setup methods
    setupTimerInterval,
    initializeWebSocket,
    cleanupWebSocket,
    setupRealTimeMonitoring
  };
}