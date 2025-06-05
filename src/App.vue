<template>
  <router-view/>
  <NotificationModal /> <!-- Global modal -->
</template>

<script setup>
import NotificationModal from "./components/NotificacionPedidoModal.vue";
</script>

<style scoped>
</style>
