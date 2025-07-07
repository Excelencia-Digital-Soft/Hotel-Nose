<template>
  <router-view/>
  <NotificationModal /> <!-- Global modal -->
  <Toast /> <!-- Global toast notifications -->
</template>

<script setup>
import NotificationModal from "./components/NotificacionPedidoModal.vue";
import Toast from "primevue/toast";
</script>

<style scoped>
</style>
