import { defineStore } from "pinia";
import { useAuthStore } from "./auth";
import * as signalR from "@microsoft/signalr";
import { buildWebSocketUrl } from "../utils/url-helpers";
import type { WebSocketState, WebSocketNotification, WebSocketEvent } from "../types";

export const useWebSocketStore = defineStore("websocket", {
  state: (): WebSocketState => ({
    connection: null,
    notifications: [],
    nextNotificationId: 1,
    eventCallbacks: {},
  }),
  
  actions: {
    async connect(): Promise<void> {
      const authStore = useAuthStore();
      const institucionID = authStore.institucionID;

      if (!authStore.isAuthenticated || !institucionID) {
        console.log("WebSocket connection skipped - user not authenticated or no institution selected");
        return;
      }

      console.log("Connecting to WebSocket with InstitucionID:", institucionID);

      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(buildWebSocketUrl(import.meta.env.VITE_API_BASE_URL), {
          accessTokenFactory: () => authStore.token
        })
        .withAutomaticReconnect()
        .build();

      try {
        await this.connection.start();
        console.log("WebSocket connected, subscribing to institution:", institucionID);

        await this.connection.invoke("SubscribeToInstitution", institucionID.toString());

        this.connection.on("ReceiveNotification", (notification: WebSocketNotification) => {
          console.log("New notification:", notification);
          this.addNotification(notification);

          // Trigger all registered component-specific callbacks
          Object.values(this.eventCallbacks).forEach(callback => {
            if (typeof callback === "function") {
              callback(notification as WebSocketEvent);
            }
          });
        });
      } catch (error) {
        console.error("WebSocket connection error:", error);
        
        // Handle authentication errors specifically
        if (error instanceof Error && error.message.includes('401')) {
          console.error("Authentication failed for WebSocket. Token:", authStore.token ? 'Present' : 'Missing');
          console.error("Make sure the token is valid and not expired.");
        }
        
        throw error; // Re-throw to let caller handle
      }
    },

    addNotification(notification: WebSocketNotification): void {
      const id = this.nextNotificationId++;

      this.notifications.push({
        id: id,
        type: notification.type,
        message: notification.message,
        timestamp: Date.now(),
      });

      // Auto-remove after 5 minutes (300000 ms)
      setTimeout(() => {
        this.removeNotification(id);
      }, 300000);
    },

    removeNotification(id: number): void {
      this.notifications = this.notifications.filter(notification => notification.id !== id);
    },

    registerEventCallback(componentName: string, callback: (event: WebSocketEvent) => void): void {
      this.eventCallbacks[componentName] = callback;
    },

    unregisterEventCallback(componentName: string): void {
      delete this.eventCallbacks[componentName];
    },

    async disconnect(): Promise<void> {
      if (this.connection) {
        try {
          await this.connection.stop();
          console.log("WebSocket disconnected");
        } catch (error) {
          console.error("Error disconnecting WebSocket:", error);
        }
        this.connection = null;
      }
    },
  },
});