import { defineStore } from "pinia";
import { useAuthStore } from "./auth.js";
import * as signalR from "@microsoft/signalr";

export const useWebSocketStore = defineStore("websocket", {
  state: () => ({
    connection: null,
    notification: null, // Stores the latest notification
    showModal: false, // Controls the modal visibility
  }),
  actions: {
    async connect() {
      const authStore = useAuthStore(); // Get the user's role

      this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${import.meta.env.VITE_API_BASE_URL}notifications`)
      .withAutomaticReconnect()
      .build();

      try {
        await this.connection.start();
        console.log("WebSocket connected");

        // Listen for new order notifications
        this.connection.on("ReceiveNotification", (message) => {
          console.log("New notification:", message);

          // Show modal only for users with the right role
            this.notification = message;
            this.showModal = true;
        });
      } catch (error) {
        console.error("WebSocket connection error:", error);
      }
    },
  },
});
