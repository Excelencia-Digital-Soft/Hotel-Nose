<script setup>
import { computed } from "vue";
import { useWebSocketStore } from "../store/websocket.js";

const websocketStore = useWebSocketStore();

const showModal = computed(() => websocketStore.showModal);
const notification = computed(() => websocketStore.notification);

const closeModal = () => {
  websocketStore.showModal = false;
};
</script>

<template>
  <div v-if="showModal" class="modal">
    <div class="modal-content">
      <!-- Icon and notification text -->
      <div class="icon-text">
        <div class="warning-icon pi pi-exclamation-triangle"></div> <!-- PrimeVue warning icon -->
        <div class="notification-text">
          <p>{{ notification }}</p>
        </div>
      </div>
      <!-- Close button -->
      <button class="btn-close" @click="closeModal">Close</button>
    </div>
  </div>
</template>

<style scoped>
.modal {
  position: fixed;
  bottom: 20px;
  right: 20px;
  background: #2c2c2c; /* Dark background */
  color: white;
  padding: 20px;
  border-radius: 8px;
  border-color: rgb(172, 70, 172);
  border-width: 2px;
  display: flex;
  flex-direction: column;
  align-items: center;
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
}

.icon-text {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 10px;
}

.warning-icon {
  font-size: 40px; /* Icon size */
  background: linear-gradient(135deg, rgb(191, 86, 191), rgb(21, 111, 118)); /* Gradient inside the icon */
  -webkit-background-clip: text; /* Clip the gradient to text */
  color: transparent; /* Make the text itself transparent to show the gradient */
  padding: 10px;
  border-radius: 50%;
}

.notification-text {
  max-width: 200px;
  text-align: left;
}

.btn-close {
  background-color: #ad55d0; /* Primary color */
  color: white;
  padding: 8px 16px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
  transition: background-color 0.3s ease;
}

.btn-close:hover {
  background-color: #8648b6; /* Darker blue for hover effect */
}
</style>
