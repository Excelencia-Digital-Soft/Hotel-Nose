/**
 * WebSocket notification and event types
 */

import { HubConnection } from '@microsoft/signalr';

/**
 * WebSocket notification types
 */
export type NotificationType = 'info' | 'warning' | 'error' | 'success';

/**
 * WebSocket event types
 */
export type WebSocketEventType = 
  | 'warning'
  | 'ended'
  | 'room_updated'
  | 'room_reserved'
  | 'room_checkout'
  | 'time_extended'
  | 'pending_orders_updated'
  | 'bulk_update';

/**
 * Base notification structure
 */
export interface WebSocketNotification {
  id?: number;
  type: NotificationType;
  message: string;
  timestamp?: number;
}

/**
 * WebSocket event data
 */
export interface WebSocketEvent {
  type: WebSocketEventType;
  roomId?: number;
  reservationId?: number;
  reservationData?: any;
  hours?: number;
  minutes?: number;
  hasPendingOrders?: boolean;
  data?: any;
}

/**
 * WebSocket store state
 */
export interface WebSocketState {
  connection: HubConnection | null;
  notifications: WebSocketNotification[];
  nextNotificationId: number;
  eventCallbacks: Record<string, (event: WebSocketEvent) => void>;
}

/**
 * WebSocket store actions
 */
export interface WebSocketActions {
  connect(): Promise<void>;
  disconnect(): Promise<void>;
  addNotification(notification: WebSocketNotification): void;
  removeNotification(id: number): void;
  registerEventCallback(componentName: string, callback: (event: WebSocketEvent) => void): void;
  unregisterEventCallback(componentName: string): void;
}

/**
 * Complete WebSocket store type
 */
export type WebSocketStore = WebSocketState & WebSocketActions;