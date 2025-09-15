<template>
  <div class="flex flex-wrap gap-3">
    <!-- Search input -->
    <div class="relative group">
      <div class="absolute inset-0 bg-gradient-to-r from-primary-500/20 to-secondary-500/20 rounded-2xl blur opacity-0 group-hover:opacity-100 transition-opacity duration-300"></div>
      <div class="relative bg-white/10 glass-dropdown border border-white/20 rounded-2xl flex items-center hover:bg-white/15 transition-all duration-300" :class="searchInputClasses">
        <span class="material-symbols-outlined text-primary-300" :class="iconClasses">search</span>
        <input 
          :value="searchTerm"
          @input="$emit('update:searchTerm', $event.target.value)"
          type="text" 
          :placeholder="searchPlaceholder"
          class="bg-transparent text-white placeholder-gray-400 border-none outline-none text-sm flex-1"
        >
        <!-- Clear search button -->
        <button 
          v-if="searchTerm"
          @click="$emit('update:searchTerm', '')"
          class="ml-2 text-gray-400 hover:text-white transition-colors"
        >
          <span class="material-symbols-outlined text-sm">close</span>
        </button>
      </div>
    </div>
    
    <!-- Category filter -->
    <div class="relative group">
      <div class="absolute inset-0 bg-gradient-to-r from-secondary-500/20 to-accent-500/20 rounded-2xl blur opacity-0 group-hover:opacity-100 transition-opacity duration-300"></div>
      <div class="relative bg-white/10 glass-dropdown border border-white/20 rounded-2xl flex items-center hover:bg-white/15 transition-all duration-300" :class="selectClasses">
        <span class="material-symbols-outlined text-secondary-300" :class="iconClasses">filter_list</span>
        <select 
          :value="selectedCategory"
          @change="$emit('update:selectedCategory', $event.target.value)"
          class="bg-transparent text-white border-none outline-none text-sm cursor-pointer flex-1"
        >
          <option value="">{{ compactMode ? 'Todas' : 'Todas las categorías' }}</option>
          <option value="CLASICA">Clásica</option>
          <option value="SUITE">Suite</option>
          <option value="MASTER SUITE">Master Suite</option>
          <option value="HIDRO SUITE">Hidro Suite</option>
          <option value="HIDROMAX SUITE">Hidromax Suite</option>
          <option value="PENTHOUSE">Penthouse</option>
        </select>
      </div>
    </div>
    
    <!-- Show only occupied filter -->
    <div class="relative group">
      <div class="absolute inset-0 bg-gradient-to-r from-red-500/20 to-rose-500/20 rounded-2xl blur opacity-0 group-hover:opacity-100 transition-opacity duration-300"></div>
      <div class="relative bg-white/10 glass-dropdown border border-white/20 rounded-2xl flex items-center hover:bg-white/15 transition-all duration-300" :class="checkboxClasses">
        <span class="material-symbols-outlined text-red-300" :class="iconClasses">hotel</span>
        <label class="flex items-center gap-2 cursor-pointer">
          <input 
            type="checkbox" 
            :checked="showOnlyOccupied"
            @change="$emit('update:showOnlyOccupied', $event.target.checked)"
            class="w-4 h-4 text-red-500 bg-transparent border-2 border-red-400 rounded focus:ring-red-500 focus:ring-2"
          >
          <span class="text-white text-sm font-medium">{{ checkboxLabel }}</span>
        </label>
      </div>
    </div>

    <!-- Clear filters button -->
    <button 
      v-if="hasActiveFilters"
      @click="$emit('clearFilters')"
      class="relative group bg-gradient-to-r from-gray-500 to-gray-600 hover:from-gray-400 hover:to-gray-500 rounded-2xl flex items-center transition-all duration-300 shadow-lg hover:shadow-gray-500/25 hover:scale-105" 
      :class="buttonClasses"
    >
      <span class="material-symbols-outlined text-white" :class="iconClasses">clear_all</span>
      <span v-if="!compactMode" class="text-white font-medium">Limpiar</span>
    </button>
  </div>
</template>

<script setup>
import { computed } from 'vue';

const props = defineProps({
  searchTerm: {
    type: String,
    default: ''
  },
  selectedCategory: {
    type: String,
    default: ''
  },
  showOnlyOccupied: {
    type: Boolean,
    default: false
  },
  compactMode: {
    type: Boolean,
    default: false
  },
  hasActiveFilters: {
    type: Boolean,
    default: false
  }
});

defineEmits([
  'update:searchTerm',
  'update:selectedCategory', 
  'update:showOnlyOccupied',
  'clearFilters'
]);

// Computed classes
const iconClasses = computed(() => {
  return props.compactMode ? 'text-lg' : '';
});

const searchInputClasses = computed(() => {
  return props.compactMode ? 'px-3 py-2 gap-2' : 'px-4 py-3 gap-3';
});

const selectClasses = computed(() => {
  return props.compactMode ? 'px-3 py-2 gap-2' : 'px-4 py-3 gap-3';
});

const checkboxClasses = computed(() => {
  return props.compactMode ? 'px-3 py-2 gap-2' : 'px-4 py-3 gap-3';
});

const buttonClasses = computed(() => {
  return props.compactMode ? 'px-4 py-2 gap-2' : 'px-6 py-3 gap-3';
});

const searchPlaceholder = computed(() => {
  return props.compactMode ? 'Buscar...' : 'Buscar habitación...';
});

const checkboxLabel = computed(() => {
  return props.compactMode ? 'Ocupadas' : 'Filtrar habitaciones ocupadas';
});
</script>

<style scoped>
/* Style the select element */
select option {
  @apply bg-gray-800 text-white;
}

/* Custom checkbox styling */
input[type="checkbox"]:checked {
  background-color: rgb(239 68 68); /* red-500 */
  border-color: rgb(239 68 68);
}

input[type="checkbox"]:focus {
  box-shadow: 0 0 0 2px rgb(239 68 68 / 0.5);
}
</style>