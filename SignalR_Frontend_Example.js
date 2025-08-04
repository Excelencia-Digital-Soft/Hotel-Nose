// SignalR Frontend Integration Example
// This example shows how to connect to the SignalR hub and handle notifications

// Import SignalR library (make sure to include @microsoft/signalr in your project)
// npm install @microsoft/signalr

import * as signalR from "@microsoft/signalr";

class NotificationService {
    constructor() {
        this.connection = null;
        this.isConnected = false;
    }

    // Initialize SignalR connection
    async initialize(accessToken) {
        try {
            // Create connection with authentication (V1 endpoint)
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/api/v1/notifications", {
                    accessTokenFactory: () => accessToken
                })
                .withAutomaticReconnect([0, 2000, 5000, 10000, 30000]) // Retry intervals
                .configureLogging(signalR.LogLevel.Information)
                .build();

            // Setup event handlers
            this.setupEventHandlers();

            // Start connection
            await this.start();
        } catch (error) {
            console.error("Failed to initialize SignalR:", error);
            throw error;
        }
    }

    // Setup all event handlers
    setupEventHandlers() {
        // Handle incoming notifications
        this.connection.on("ReceiveNotification", (type, message, data) => {
            console.log(`Notification received - Type: ${type}, Message: ${message}`, data);
            
            // Handle different notification types
            switch(type) {
                case "info":
                    this.showInfoNotification(message, data);
                    break;
                case "warning":
                    this.showWarningNotification(message, data);
                    break;
                case "error":
                    this.showErrorNotification(message, data);
                    break;
                case "success":
                    this.showSuccessNotification(message, data);
                    break;
                default:
                    this.showDefaultNotification(message, data);
            }

            // Emit custom event for other parts of the application
            this.emitNotificationEvent(type, message, data);
        });

        // Handle subscription confirmation
        this.connection.on("SubscriptionConfirmed", (message) => {
            console.log("Subscription confirmed:", message);
        });

        // Handle connection lifecycle events
        this.connection.onreconnecting((error) => {
            console.warn("Connection lost. Attempting to reconnect...", error);
            this.isConnected = false;
        });

        this.connection.onreconnected((connectionId) => {
            console.log("Reconnected with connection ID:", connectionId);
            this.isConnected = true;
        });

        this.connection.onclose((error) => {
            console.error("Connection closed:", error);
            this.isConnected = false;
        });
    }

    // Start the connection
    async start() {
        try {
            await this.connection.start();
            this.isConnected = true;
            console.log("SignalR Connected - Connection ID:", this.connection.connectionId);
            
            // Note: Auto-subscription to institution happens on the server side based on user claims
            // But you can still manually subscribe if needed
        } catch (error) {
            console.error("Failed to start SignalR connection:", error);
            // Retry after 5 seconds
            setTimeout(() => this.start(), 5000);
        }
    }

    // Manually subscribe to an institution (optional - server auto-subscribes based on claims)
    async subscribeToInstitution(institucionId) {
        if (!this.isConnected) {
            console.error("Not connected to SignalR hub");
            return;
        }

        try {
            await this.connection.invoke("SubscribeToInstitution", institucionId);
        } catch (error) {
            console.error("Failed to subscribe to institution:", error);
        }
    }

    // Stop the connection
    async stop() {
        if (this.connection) {
            await this.connection.stop();
            this.isConnected = false;
        }
    }

    // Notification display methods (customize based on your UI framework)
    showInfoNotification(message, data) {
        // Example: Using a toast library or custom notification component
        console.info("ℹ️ Info:", message, data);
        // Your UI notification code here
    }

    showWarningNotification(message, data) {
        console.warn("⚠️ Warning:", message, data);
        // Your UI notification code here
    }

    showErrorNotification(message, data) {
        console.error("❌ Error:", message, data);
        // Your UI notification code here
    }

    showSuccessNotification(message, data) {
        console.log("✅ Success:", message, data);
        // Your UI notification code here
    }

    showDefaultNotification(message, data) {
        console.log("📢 Notification:", message, data);
        // Your UI notification code here
    }

    // Emit custom event for other parts of the application
    emitNotificationEvent(type, message, data) {
        const event = new CustomEvent('signalr-notification', {
            detail: { type, message, data }
        });
        window.dispatchEvent(event);
    }
}

// Usage Example
export default NotificationService;

// In your main application file:
/*
// Initialize when user logs in
const notificationService = new NotificationService();
await notificationService.initialize(authToken);

// Listen for custom events in other components
window.addEventListener('signalr-notification', (event) => {
    const { type, message, data } = event.detail;
    // Handle notification in your component
});

// Clean up when user logs out
await notificationService.stop();
*/

// React Hook Example
/*
import { useEffect, useState } from 'react';

export function useNotifications(authToken) {
    const [notifications, setNotifications] = useState([]);
    const [isConnected, setIsConnected] = useState(false);

    useEffect(() => {
        if (!authToken) return;

        const notificationService = new NotificationService();
        
        // Setup notification listener
        const handleNotification = (event) => {
            const notification = event.detail;
            setNotifications(prev => [...prev, {
                ...notification,
                timestamp: new Date(),
                id: Date.now()
            }]);
        };

        window.addEventListener('signalr-notification', handleNotification);

        // Initialize connection
        notificationService.initialize(authToken)
            .then(() => setIsConnected(true))
            .catch(console.error);

        // Cleanup
        return () => {
            window.removeEventListener('signalr-notification', handleNotification);
            notificationService.stop();
        };
    }, [authToken]);

    return { notifications, isConnected };
}
*/