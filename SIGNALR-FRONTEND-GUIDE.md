# 🔌 SignalR Frontend Integration Guide - Hotel Management System

## ⚡ **Configuración Optimizada para Evitar Bloqueo de APIs**

### 📋 **Problema Común**
Cuando SignalR está conectado, puede interferir con otras llamadas HTTP de la API. Esta guía muestra cómo configurarlo correctamente.

---

## 🚀 **Configuración Correcta del Cliente**

### **1. Configuración Básica de Conexión**

```javascript
// ✅ CONFIGURACIÓN CORRECTA
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/api/v1/notifications", {
        // Token para autenticación
        accessTokenFactory: () => {
            return localStorage.getItem("jwt_token") || sessionStorage.getItem("jwt_token");
        },
        
        // Configurar transports para mejor rendimiento
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents,
        
        // NO usar LongPolling para evitar bloqueos
        skipNegotiation: false,
        
        // Headers adicionales si necesarios
        headers: {
            "Content-Type": "application/json"
        }
    })
    .withAutomaticReconnect([0, 2000, 10000, 30000]) // Reconexión automática
    .configureLogging(signalR.LogLevel.Information)
    .build();
```

### **2. Configuración de Axios/Fetch Independiente**

```javascript
// ✅ CONFIGURAR AXIOS DE FORMA INDEPENDIENTE
const apiClient = axios.create({
    baseURL: 'http://localhost:5000/api',
    timeout: 10000,
    headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('jwt_token')}`
    }
});

// Interceptor para manejar tokens automáticamente
apiClient.interceptors.request.use(config => {
    const token = localStorage.getItem('jwt_token');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});
```

### **3. Manejo de Conexión SignalR**

```javascript
class SignalRManager {
    constructor() {
        this.connection = null;
        this.isConnected = false;
        this.reconnectAttempts = 0;
        this.maxReconnectAttempts = 5;
    }

    async connect() {
        try {
            if (this.connection?.state === signalR.HubConnectionState.Connected) {
                console.log("SignalR already connected");
                return;
            }

            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/api/v1/notifications", {
                    accessTokenFactory: () => localStorage.getItem("jwt_token"),
                    transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents
                })
                .withAutomaticReconnect([0, 2000, 10000, 30000])
                .build();

            // Configurar event listeners
            this.setupEventListeners();

            // Conectar
            await this.connection.start();
            this.isConnected = true;
            this.reconnectAttempts = 0;
            
            console.log("SignalR connected successfully");
            
            // Auto-suscribir a institución
            await this.subscribeToInstitution();
            
        } catch (error) {
            console.error("SignalR connection failed:", error);
            this.isConnected = false;
            
            // Retry logic
            if (this.reconnectAttempts < this.maxReconnectAttempts) {
                this.reconnectAttempts++;
                setTimeout(() => this.connect(), 5000);
            }
        }
    }

    setupEventListeners() {
        // Eventos de conexión
        this.connection.onreconnecting(() => {
            console.log("SignalR reconnecting...");
            this.isConnected = false;
        });

        this.connection.onreconnected(() => {
            console.log("SignalR reconnected");
            this.isConnected = true;
            this.reconnectAttempts = 0;
            this.subscribeToInstitution();
        });

        this.connection.onclose(() => {
            console.log("SignalR disconnected");
            this.isConnected = false;
        });

        // ✅ EVENTOS DE NEGOCIO
        this.connection.on("RoomStatusChanged", (data) => {
            console.log("Room status changed:", data);
            this.handleRoomStatusChange(data);
        });

        this.connection.on("RoomProgressUpdated", (data) => {
            console.log("Room progress updated:", data);
            this.handleRoomProgressUpdate(data);
        });

        this.connection.on("RoomReservationChanged", (data) => {
            console.log("Room reservation changed:", data);
            this.handleReservationChange(data);
        });

        this.connection.on("ForceDisconnect", (data) => {
            console.log("Forced disconnect:", data);
            alert("Tu sesión fue reemplazada por otra conexión.");
            this.disconnect();
        });

        // Eventos generales
        this.connection.on("ReceiveNotification", (type, message, data) => {
            console.log(`Notification [${type}]: ${message}`, data);
        });

        this.connection.on("SubscriptionConfirmed", (message) => {
            console.log("Subscription confirmed:", message);
        });
    }

    async subscribeToInstitution() {
        if (!this.isConnected || !this.connection) return;
        
        try {
            const institutionId = this.getInstitutionId(); // Implementar según tu lógica
            if (institutionId) {
                await this.connection.invoke("SubscribeToInstitution", institutionId);
            }
        } catch (error) {
            console.error("Failed to subscribe to institution:", error);
        }
    }

    async disconnect() {
        if (this.connection) {
            await this.connection.stop();
            this.isConnected = false;
        }
    }

    // Métodos específicos para habitaciones
    async joinRoomGroup(roomId) {
        if (!this.isConnected) return;
        await this.connection.invoke("JoinRoomGroup", roomId);
    }

    async subscribeToRoomProgress(roomId, enable = true) {
        if (!this.isConnected) return;
        await this.connection.invoke("SubscribeToRoomProgress", roomId, enable);
    }

    // Handlers de eventos
    handleRoomStatusChange(data) {
        // Actualizar UI según el cambio de estado
        const { roomId, status, visitaId, timestamp } = data;
        
        // Ejemplo: actualizar tarjeta de habitación
        this.updateRoomCard(roomId, status);
        
        // Mostrar notificación
        this.showNotification(`Habitación ${roomId} cambió a: ${status}`);
    }

    handleRoomProgressUpdate(data) {
        // Actualizar barra de progreso
        const { roomId, progressPercentage, timeElapsed } = data;
        this.updateProgressBar(roomId, progressPercentage, timeElapsed);
    }

    handleReservationChange(data) {
        // Manejar cambios en reservas
        const { roomId, action, reservaId } = data;
        this.refreshReservationData(roomId);
    }
}
```

### **4. Uso en React/Vue**

```javascript
// ✅ REACT HOOK PERSONALIZADO
import { useEffect, useState, useRef } from 'react';

export const useSignalR = () => {
    const [isConnected, setIsConnected] = useState(false);
    const signalRManager = useRef(null);

    useEffect(() => {
        // Inicializar SignalR solo una vez
        signalRManager.current = new SignalRManager();
        
        // Conectar
        signalRManager.current.connect().then(() => {
            setIsConnected(true);
        });

        // Cleanup al desmontar
        return () => {
            if (signalRManager.current) {
                signalRManager.current.disconnect();
                setIsConnected(false);
            }
        };
    }, []);

    return {
        isConnected,
        signalR: signalRManager.current,
        joinRoomGroup: (roomId) => signalRManager.current?.joinRoomGroup(roomId),
        subscribeToProgress: (roomId, enable) => signalRManager.current?.subscribeToRoomProgress(roomId, enable)
    };
};
```

---

## 🔧 **Configuraciones Adicionales**

### **1. Variables de Entorno**

```javascript
// .env
REACT_APP_API_BASE_URL=http://localhost:5000/api
REACT_APP_SIGNALR_HUB_URL=/api/v1/notifications
```

### **2. Service Worker (Opcional)**

```javascript
// Para manejar notificaciones en background
if ('serviceWorker' in navigator) {
    navigator.serviceWorker.register('/sw.js');
}
```

---

## ⚠️ **Problemas Comunes y Soluciones**

### **❌ NO HACER:**
```javascript
// ❌ MAL: Usar LongPolling
transport: signalR.HttpTransportType.LongPolling

// ❌ MAL: No configurar timeout
// Sin timeout puede bloquear conexiones

// ❌ MAL: Múltiples instancias de conexión
// Crear múltiples conexiones SignalR
```

### **✅ SÍ HACER:**
```javascript
// ✅ BIEN: Usar WebSockets + SSE
transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents

// ✅ BIEN: Configurar timeouts
timeout: 10000

// ✅ BIEN: Una sola instancia global
// Singleton pattern para SignalR
```

---

## 📊 **Monitoreo y Debug**

### **Console Logs Útiles:**
```javascript
// Verificar estado de conexión
console.log("SignalR State:", connection.state);

// Verificar transporte usado
connection.onreconnected((connectionId) => {
    console.log("Reconnected with ID:", connectionId);
    console.log("Transport:", connection.transport);
});
```

### **Herramientas de Debug:**
- Browser Network Tab → Ver negotiate y connect requests
- SignalR LogLevel.Debug para más información
- Monitor de conexiones en DevTools

---

## 🎯 **Resultado Esperado**

Después de implementar esta configuración:
- ✅ SignalR no bloquea llamadas HTTP de la API
- ✅ Reconexión automática funciona correctamente  
- ✅ Solo una conexión por usuario (ya implementado en backend)
- ✅ Notificaciones en tiempo real funcionan
- ✅ Rendimiento optimizado

## 🚀 **Testing**

1. **Conectar SignalR** → Verificar que no bloquea APIs
2. **Hacer llamadas HTTP** → Deben funcionar normalmente
3. **Abrir múltiples tabs** → Solo una conexión activa
4. **Reconexión** → Debe ser automática
5. **Notificaciones** → Recibir en tiempo real