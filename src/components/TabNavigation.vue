<template>
  <div class="tab-navigation">
    <div class="glass-tab-container">
      <button
        v-for="tab in tabs"
        :key="tab.id"
        :class="[
          'glass-tab-button',
          { 'active': activeTab === tab.id }
        ]"
        @click="selectTab(tab.id)"
      >
        <i :class="tab.icon" class="mr-2"></i>
        {{ tab.label }}
      </button>
    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  tabs: {
    type: Array,
    required: true
  },
  activeTab: {
    type: String,
    required: true
  }
})

const emit = defineEmits(['tab-changed'])

const selectTab = (tabId) => {
  emit('tab-changed', tabId)
}
</script>

<style scoped>
.tab-navigation {
  @apply mb-6;
}

.glass-tab-container {
  @apply flex bg-white/5 backdrop-blur-md border border-white/20 rounded-xl p-1;
}

.glass-tab-button {
  @apply flex-1 flex items-center justify-center py-3 px-4 rounded-lg font-medium 
         text-white/70 transition-all duration-300 hover:text-white hover:bg-white/10;
}

.glass-tab-button.active {
  @apply bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400
         text-white shadow-lg backdrop-blur-sm border border-white/30;
}

.glass-tab-button:not(.active):hover {
  @apply bg-white/10 backdrop-blur-sm;
}
</style>