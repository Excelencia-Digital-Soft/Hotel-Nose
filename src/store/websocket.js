import { defineStore } from "pinia";
import { useAuthStore } from "./auth.js";
import * as signalR from "@microsoft/signalr";

export const useWebSocketStore = defineStore("websocket", {
    state: () => ({
        connection: null,
        notifications: [],
        nextNotificationId: 1,
        eventCallbacks: {}, // New: Store event listeners per component
    }),
    actions: {
        async connect() {
            const authStore = useAuthStore();
            const institucionID = authStore.institucionID;

            if (!institucionID) {
                console.error("No InstitucionID found, cannot connect to WebSocket.");
                return;
            }

            console.log("Connecting to WebSocket with InstitucionID:", institucionID);

            this.connection = new signalR.HubConnectionBuilder()
                .withUrl(`${import.meta.env.VITE_API_BASE_URL}notifications`)
                .withAutomaticReconnect()
                .build();

            try {
                await this.connection.start();
                console.log("WebSocket connected, subscribing to institution:", institucionID);

                await this.connection.invoke("SubscribeToInstitution", institucionID.toString());

                this.connection.on("ReceiveNotification", (notification) => {
                    console.log("New notification:", notification);
                    this.addNotification(notification);

                    // Trigger all registered component-specific callbacks
                    Object.values(this.eventCallbacks).forEach(callback => {
                        if (typeof callback === "function") {
                            callback(notification);
                        }
                    });
                });
            } catch (error) {
                console.error("WebSocket connection error:", error);
            }
        },

        addNotification(notification) {
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

        removeNotification(id) {
            this.notifications = this.notifications.filter(notification => notification.id !== id);
        },

        registerEventCallback(componentName, callback) {
            this.eventCallbacks[componentName] = callback;
        },

        unregisterEventCallback(componentName) {
            delete this.eventCallbacks[componentName];
        },
    },
});