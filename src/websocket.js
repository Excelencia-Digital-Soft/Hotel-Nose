import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl(`${import.meta.env.VITE_API_BASE_URL}notifications`) // Your backend URL
  .withAutomaticReconnect()
  .build();

connection.start().catch(err => console.error("Connection failed: ", err));

connection.on("ReceiveNotification", message => {
  console.log("New Notification: ", message);
  alert(message); // Show notification
});

export default connection;
