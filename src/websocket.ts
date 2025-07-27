import * as signalR from "@microsoft/signalr";
import { HubConnection } from "@microsoft/signalr";
import { buildWebSocketUrl } from "./utils/url-helpers";

// This file is for backward compatibility only
// Use the websocket store instead for authenticated connections
const connection: HubConnection = new signalR.HubConnectionBuilder()
  .withUrl(buildWebSocketUrl(import.meta.env.VITE_API_BASE_URL), {
    accessTokenFactory: () => {
      // Get token from localStorage as fallback
      return localStorage.getItem('auth-token') || '';
    }
  })
  .withAutomaticReconnect()
  .build();

// Don't auto-start connection - let the store handle it
// connection.start().catch(err => console.error("Connection failed: ", err));

connection.on("ReceiveNotification", (message: string) => {
  console.log("New Notification: ", message);
  alert(message); // Show notification
});

export default connection;