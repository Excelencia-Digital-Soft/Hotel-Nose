<template>
  <div
    class="glass-card p-4 flex flex-col sm:flex-row items-center justify-between space-y-4 sm:space-y-0"
  >
    <!-- Pagination Info -->
    <div class="flex items-center space-x-4">
      <div class="text-gray-300 text-sm">
        <span class="font-semibold text-white">{{ totalRecords }}</span> registros en total
      </div>

      <!-- Page Size Selector -->
      <div class="flex items-center space-x-2">
        <span class="text-gray-300 text-sm">Mostrar:</span>
        <select
          :value="pageSize"
          @change="handlePageSizeChange"
          class="glass-input text-sm px-2 py-1 rounded"
        >
          <option value="5">5</option>
          <option value="10">10</option>
          <option value="20">20</option>
          <option value="50">50</option>
        </select>
        <span class="text-gray-300 text-sm">por página</span>
      </div>
    </div>

    <!-- Pagination Controls -->
    <div class="flex items-center space-x-2">
      <!-- Previous Button -->
      <button
        @click="$emit('previous')"
        :disabled="!canGoPrevious || isLoading"
        class="glass-button p-2 text-white hover:bg-white/20 disabled:opacity-50 disabled:cursor-not-allowed transition-all"
        title="Página anterior"
      >
        <i class="pi pi-chevron-left"></i>
      </button>

      <!-- Page Numbers -->
      <div class="flex items-center space-x-1">
        <!-- First page if not visible in range -->
        <button
          v-if="startPage > 1"
          @click="$emit('goToPage', 1)"
          :disabled="isLoading"
          class="glass-button w-8 h-8 text-sm text-white hover:bg-white/20 disabled:opacity-50 transition-all"
        >
          1
        </button>

        <!-- Ellipsis if gap exists -->
        <span v-if="startPage > 2" class="text-gray-400 px-1">...</span>

        <!-- Page range -->
        <button
          v-for="page in visiblePages"
          :key="page"
          @click="$emit('goToPage', page)"
          :disabled="isLoading"
          :class="[
            'w-8 h-8 text-sm transition-all',
            page === currentPage
              ? 'bg-gradient-to-r from-primary-400 to-secondary-500 text-white font-bold'
              : 'glass-button text-white hover:bg-white/20 disabled:opacity-50',
          ]"
        >
          {{ page }}
        </button>

        <!-- Ellipsis if gap exists -->
        <span v-if="endPage < totalPages - 1" class="text-gray-400 px-1">...</span>

        <!-- Last page if not visible in range -->
        <button
          v-if="endPage < totalPages"
          @click="$emit('goToPage', totalPages)"
          :disabled="isLoading"
          class="glass-button w-8 h-8 text-sm text-white hover:bg-white/20 disabled:opacity-50 transition-all"
        >
          {{ totalPages }}
        </button>
      </div>

      <!-- Next Button -->
      <button
        @click="$emit('next')"
        :disabled="!canGoNext || isLoading"
        class="glass-button p-2 text-white hover:bg-white/20 disabled:opacity-50 disabled:cursor-not-allowed transition-all"
        title="Siguiente página"
      >
        <i class="pi pi-chevron-right"></i>
      </button>
    </div>

    <!-- Current Page Info -->
    <div class="text-gray-300 text-sm">
      Página <span class="font-semibold text-white">{{ currentPage }}</span> de
      <span class="font-semibold text-white">{{ totalPages }}</span>
    </div>
  </div>
</template>

<script setup>
  import { computed } from 'vue'

  const props = defineProps({
    currentPage: {
      type: Number,
      required: true,
    },
    totalPages: {
      type: Number,
      required: true,
    },
    totalRecords: {
      type: Number,
      required: true,
    },
    pageSize: {
      type: Number,
      required: true,
    },
    canGoNext: {
      type: Boolean,
      required: true,
    },
    canGoPrevious: {
      type: Boolean,
      required: true,
    },
    isLoading: {
      type: Boolean,
      default: false,
    },
    maxVisiblePages: {
      type: Number,
      default: 5,
    },
  })

  const emit = defineEmits(['goToPage', 'next', 'previous', 'setPageSize'])

  // Computed properties for pagination display
  const startPage = computed(() => {
    const half = Math.floor(props.maxVisiblePages / 2)
    let start = Math.max(1, props.currentPage - half)
    const end = Math.min(props.totalPages, start + props.maxVisiblePages - 1)

    // Adjust start if we're near the end
    if (end - start + 1 < props.maxVisiblePages) {
      start = Math.max(1, end - props.maxVisiblePages + 1)
    }

    return start
  })

  const endPage = computed(() => {
    return Math.min(props.totalPages, startPage.value + props.maxVisiblePages - 1)
  })

  const visiblePages = computed(() => {
    const pages = []
    for (let i = startPage.value; i <= endPage.value; i++) {
      pages.push(i)
    }
    return pages
  })

  // Methods
  const handlePageSizeChange = (event) => {
    const newSize = parseInt(event.target.value)
    emit('setPageSize', newSize)
  }
</script>

<style scoped>
  .glass-card {
    background-color: rgba(255, 255, 255, 0.1);
    backdrop-filter: blur(16px);
    border: 1px solid rgba(255, 255, 255, 0.2);
    border-radius: 12px;
  }

  .glass-button {
    background-color: rgba(255, 255, 255, 0.1);
    backdrop-filter: blur(8px);
    border: 1px solid rgba(255, 255, 255, 0.3);
    border-radius: 6px;
    transition: all 0.3s ease;
  }

  .glass-button:hover:not(:disabled) {
    background-color: rgba(255, 255, 255, 0.2);
    transform: scale(1.05);
  }

  .glass-input {
    background-color: rgba(255, 255, 255, 0.1);
    backdrop-filter: blur(8px);
    border: 1px solid rgba(255, 255, 255, 0.3);
    border-radius: 6px;
    color: white;
  }

  .glass-input:focus {
    outline: none;
    border-color: rgba(139, 92, 246, 0.5);
    box-shadow: 0 0 0 2px rgba(139, 92, 246, 0.2);
  }

  .glass-input option {
    background-color: rgba(31, 41, 55, 0.95);
    color: white;
  }

  /* Smooth transitions */
  .transition-all {
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  }

  /* Responsive adjustments */
  @media (max-width: 640px) {
    .glass-card {
      padding: 0.75rem;
    }

    .w-8 {
      width: 1.75rem;
      height: 1.75rem;
      font-size: 0.75rem;
    }
  }
</style>

