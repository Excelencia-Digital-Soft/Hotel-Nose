<script setup>
import { computed, watch, nextTick } from "vue";
import { useWebSocketStore } from "../store/websocket.js";
import { useRouter } from "vue-router";

const websocketStore = useWebSocketStore();
const router = useRouter();
const notifications = computed(() => websocketStore.notifications);

const closeNotification = (id) => {
  websocketStore.removeNotification(id);
};

const goToRooms = () => {
  router.push("/Rooms");
};

const goToPedidos = () => {
  router.push("/ReceptionOrder");
};

// Function to play the notification sound
const playNotificationSound = () => {
  const audio = new Audio("/sounds/notification.mp3"); // Ensure file exists in "public/sounds"
  audio.play()  ;
};

// Watch for new notifications and play sound when one arrives
watch(
  notifications,
  async (newNotifications, oldNotifications) => {

    // If a new notification is added, play the sound
    if (newNotifications.length >= oldNotifications.length) {
      await nextTick(); // Ensure DOM updates first
      playNotificationSound();
    }
  },
  { deep: true } // Ensures watch triggers even if array contents change
);
</script>

<template>
  <div class="notifications-container">
    <div 
      v-for="notification in notifications" 
      :key="notification.id" 
      :class="['modal', notification.type || 'default']"
    >
      <div class="modal-content">
        <div class="icon-text">
          <div 
            class="notification-icon"
            :class="{
              'warning': notification.type === 'warning',
              'ended': notification.type === 'ended',
              'warning-encargo': notification.type === 'warning-encargo',
              'default-icon': !notification.type
            }"
          ></div>
          <div class="notification-text">
            <p>{{ notification.message }}</p>
          </div>
        </div>

        <!-- "Go to Rooms" button for warning and ended types -->
        <button 
          v-if="notification.type === 'warning' || notification.type === 'ended'" 
          class="btn-go-rooms mr-2" 
          @click="goToRooms"
        >
          Habitaciones  
        </button>

        <!-- "Go to ReceptionOrder" button for warning-encargo notifications -->
        <button 
          v-if="notification.type === 'warning-encargo'" 
          class="btn-go-pedidos" 
          @click="goToPedidos"
        >
          Ver Pedidos
        </button>

        <!-- Close button -->
        <button class="btn-close" @click="closeNotification(notification.id)">Close</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.notifications-container {
  position: fixed;
  bottom: 20px;
  right: 20px;
  display: flex;
  flex-direction: column-reverse;
  gap: 10px;
}

/* Default modal style */
.modal {
  background: #2c2c2c;
  color: white;
  padding: 20px;
  border-radius: 8px;
  border-width: 2px;
  display: flex;
  flex-direction: column;
  align-items: center;
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
  animation: fadeIn 0.3s ease-in-out;
}

/* Different styles for different notification types */
.modal.warning {
  border-color: #fbc02d; /* Yellow for warnings */
}

.modal.ended {
  border-color: #d32f2f; /* Red for ended */
}

.modal.warning-encargo {
  border-color: #ff9800; /* Orange for order warnings */
}

/* Default notification styling when no type is provided */
.modal.default {
  border-color: #fbc02d; /* Same as warning */
}

/* Notification icon */
.notification-icon {
  font-size: 40px;
  padding: 10px;
  border-radius: 50%;
}

.notification-icon.warning {
  color: #fbc02d; /* Yellow */
}

.notification-icon.ended {
  color: #d32f2f; /* Red */
}

.notification-icon.warning-encargo {
  color: #ff9800; /* Orange */
}

.notification-icon.default-icon {
  color: #fbc02d; /* Default warning */
}

/* Button to go to rooms */
.btn-go-rooms {
  background-color: #008cba; /* Blue */
  color: white;
  padding: 8px 16px;
  margin-bottom: 10px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
  transition: background-color 0.3s ease;
}

.btn-go-rooms:hover {
  background-color: #006f9a;
}

/* Button to go to ReceptionOrder */
.btn-go-pedidos {
  background-color: #ff9800; /* Orange */
  color: white;
  padding: 8px 16px;
  margin-bottom: 10px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
  transition: background-color 0.3s ease;
}

.btn-go-pedidos:hover {
  background-color: #e68900;
}

.notification-text {
  max-width: 200px;
  text-align: left;
}

.btn-close {
  background-color: #ad55d0;
  color: white;
  padding: 8px 16px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
  transition: background-color 0.3s ease;
}

.btn-close:hover {
  background-color: #8648b6;
}

/* Smooth fade-in effect */
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
