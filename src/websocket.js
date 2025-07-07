import * as signalR from "@microsoft/signalr";
import { buildWebSocketUrl } from "./utils/url-helpers.js";

const connection = new signalR.HubConnectionBuilder()
  .withUrl(buildWebSocketUrl(import.meta.env.VITE_API_BASE_URL)) // Properly formatted URL
  .withAutomaticReconnect()
  .build();

connection.start().catch(err => console.error("Connection failed: ", err));

connection.on("ReceiveNotification", message => {
  console.log("New Notification: ", message);
  alert(message); // Show notification
});

export default connection;
