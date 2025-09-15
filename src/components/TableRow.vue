<template>
  <div class="selected-products-container">
    <!-- Header de la tabla mejorado -->
    <div class="grid grid-cols-[50px_1fr_80px_120px_80px_50px] gap-3 px-4 py-3 bg-gradient-to-r from-neutral-700 to-neutral-600 rounded-t-lg border-b border-neutral-500/30">
      <div class="text-xs font-semibold text-neutral-300 text-center">#</div>
      <div class="text-xs font-semibold text-neutral-300">Producto</div>
      <div class="text-xs font-semibold text-neutral-300 text-center">Precio</div>
      <div class="text-xs font-semibold text-neutral-300 text-center">Cantidad</div>
      <div class="text-xs font-semibold text-neutral-300 text-center">Total</div>
      <div class="text-xs font-semibold text-neutral-300 text-center">
        <span class="material-symbols-outlined text-sm">delete</span>
      </div>
    </div>

    <!-- Contenedor con scroll para los productos -->
    <div class="max-h-40 overflow-y-auto bg-neutral-800/50 rounded-b-lg">
      <div v-if="props.selectedList.length === 0" class="flex flex-col items-center justify-center py-8 text-center">
        <div class="w-12 h-12 rounded-full bg-neutral-700/50 flex items-center justify-center mb-3">
          <span class="material-symbols-outlined text-neutral-400 text-lg">shopping_cart</span>
        </div>
        <p class="text-sm text-neutral-400">No hay productos seleccionados</p>
      </div>

      <!-- Lista de productos seleccionados -->
      <div v-else class="divide-y divide-neutral-700/30">
        <div 
          v-for="(detalle, index) in props.selectedList" 
          :key="detalle.articuloId"
          class="grid grid-cols-[50px_1fr_80px_120px_80px_50px] gap-3 px-4 py-3 hover:bg-neutral-700/30 transition-all duration-200 group">
          
          <!-- Número de item -->
          <div class="flex items-center justify-center">
            <span class="text-sm font-medium text-neutral-300 bg-neutral-700 rounded-full w-6 h-6 flex items-center justify-center text-xs">
              {{ index + 1 }}
            </span>
          </div>

          <!-- Nombre del producto -->
          <div class="flex items-center min-w-0">
            <div class="truncate">
              <h4 class="text-sm font-medium text-white group-hover:text-primary-300 transition-colors">
                {{ detalle.nombreArticulo }}
              </h4>
              <p class="text-xs text-neutral-400">Stock: {{ detalle.maximo }}</p>
            </div>
          </div>

          <!-- Precio unitario -->
          <div class="flex items-center justify-center">
            <span class="text-sm font-medium text-green-400">
              ${{ detalle.precio }}
            </span>
          </div>

          <!-- Control de cantidad mejorado -->
          <div class="flex items-center justify-center">
            <div class="flex items-center bg-neutral-700 rounded-lg border border-neutral-600 overflow-hidden">
              <!-- Botón decrementar -->
              <button 
                @click="decrementQuantity(detalle)"
                :disabled="detalle.cantidad <= 1"
                class="w-8 h-8 flex items-center justify-center text-white hover:bg-neutral-600 disabled:opacity-50 disabled:cursor-not-allowed transition-colors">
                <span class="material-symbols-outlined text-sm">remove</span>
              </button>
              
              <!-- Input de cantidad -->
              <input 
                v-model.number="detalle.cantidad"
                :max="detalle.maximo" 
                :min="1" 
                type="number"
                @input="validateQuantity(detalle)"
                @blur="validateQuantity(detalle)"
                class="w-12 h-8 bg-transparent border-0 text-center text-sm text-white focus:outline-none focus:ring-1 focus:ring-primary-500 focus:bg-neutral-600/50 transition-colors" />
              
              <!-- Botón incrementar -->
              <button 
                @click="incrementQuantity(detalle)"
                :disabled="detalle.cantidad >= detalle.maximo"
                class="w-8 h-8 flex items-center justify-center text-white hover:bg-neutral-600 disabled:opacity-50 disabled:cursor-not-allowed transition-colors">
                <span class="material-symbols-outlined text-sm">add</span>
              </button>
            </div>
          </div>

          <!-- Total -->
          <div class="flex items-center justify-center">
            <span class="text-sm font-bold text-primary-400">
              ${{ (detalle.precio * detalle.cantidad).toFixed(2) }}
            </span>
          </div>

          <!-- Botón eliminar -->
          <div class="flex items-center justify-center">
            <button 
              @click="quitarRegistro(index)"
              class="w-8 h-8 rounded-lg bg-red-500/10 hover:bg-red-500/20 border border-red-500/30 text-red-400 hover:text-red-300 transition-all duration-200 flex items-center justify-center group/delete">
              <span class="material-symbols-outlined text-sm group-hover/delete:scale-110 transition-transform">
                delete
              </span>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Footer con resumen -->
    <div v-if="props.selectedList.length > 0" class="mt-3 p-3 bg-gradient-to-r from-neutral-700/50 to-neutral-600/50 rounded-lg border border-neutral-600/30">
      <div class="flex items-center justify-between text-sm">
        <div class="flex items-center gap-4">
          <span class="text-neutral-300">
            {{ props.selectedList.length }} producto{{ props.selectedList.length !== 1 ? 's' : '' }}
          </span>
          <span class="text-neutral-300">
            {{ totalItems }} unidad{{ totalItems !== 1 ? 'es' : '' }}
          </span>
        </div>
        <div class="flex items-center gap-2">
          <span class="text-neutral-300">Total:</span>
          <span class="text-lg font-bold text-primary-400">
            ${{ totalAmount.toFixed(2) }}
          </span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, ref, watch } from 'vue';

const props = defineProps({
  selectedList: Array,
});

const emits = defineEmits(['update:productList']);

// Computed properties para cálculos
const totalItems = computed(() => {
  return props.selectedList.reduce((sum, item) => sum + item.cantidad, 0);
});

const totalAmount = computed(() => {
  return props.selectedList.reduce((sum, item) => sum + (item.precio * item.cantidad), 0);
});

// Watchers
watch(() => props.selectedList, (newList) => {
  if (newList) {
    emits('update:productList', props.selectedList);
  }
}, { deep: true });

// Métodos
const quitarRegistro = (index) => {
  // Mostrar animación de salida
  const item = props.selectedList[index];
  
  // Remover el producto
  props.selectedList.splice(index, 1);
  
  // Emitir el cambio hacia el padre
  emits('update:productList', props.selectedList);
};

const validateQuantity = (detalle) => {
  // Asegurar que la cantidad esté en el rango válido
  if (detalle.cantidad > detalle.maximo) {
    detalle.cantidad = detalle.maximo;
  } else if (detalle.cantidad < 1 || isNaN(detalle.cantidad)) {
    detalle.cantidad = 1;
  }
  
  // Emitir cambios
  emits('update:productList', props.selectedList);
};

const incrementQuantity = (detalle) => {
  if (detalle.cantidad < detalle.maximo) {
    detalle.cantidad++;
    emits('update:productList', props.selectedList);
  }
};

const decrementQuantity = (detalle) => {
  if (detalle.cantidad > 1) {
    detalle.cantidad--;
    emits('update:productList', props.selectedList);
  }
};
</script>

<style scoped>
/* Scrollbar personalizado para la lista */
.overflow-y-auto {
  scrollbar-width: thin;
  scrollbar-color: rgba(156, 163, 175, 0.3) transparent;
}

.overflow-y-auto::-webkit-scrollbar {
  width: 6px;
}

.overflow-y-auto::-webkit-scrollbar-track {
  background: rgba(0, 0, 0, 0.1);
  border-radius: 3px;
}

.overflow-y-auto::-webkit-scrollbar-thumb {
  background: rgba(156, 163, 175, 0.3);
  border-radius: 3px;
}

.overflow-y-auto::-webkit-scrollbar-thumb:hover {
  background: rgba(156, 163, 175, 0.5);
}

/* Animaciones para los inputs de número */
input[type="number"]::-webkit-outer-spin-button,
input[type="number"]::-webkit-inner-spin-button {
  -webkit-appearance: none;
  margin: 0;
}

input[type="number"] {
  -moz-appearance: textfield;
}

/* Animación suave para cambios de estado */
.transition-all {
  transition-property: all;
  transition-timing-function: cubic-bezier(0.4, 0, 0.2, 1);
}

/* Hover effects mejorados */
.group:hover .group-hover\:text-primary-300 {
  color: rgb(var(--primary-300));
}

.group:hover .group-hover\:scale-110 {
  transform: scale(1.1);
}

/* Estados de focus mejorados */
button:focus-visible {
  outline: 2px solid rgb(var(--primary-500));
  outline-offset: 2px;
}

input:focus-visible {
  outline: 2px solid rgb(var(--primary-500));
  outline-offset: 1px;
}

/* Animación de eliminación */
@keyframes fadeOut {
  from { opacity: 1; transform: translateX(0); }
  to { opacity: 0; transform: translateX(-20px); }
}

.removing {
  animation: fadeOut 0.3s ease-out forwards;
}

/* Grid responsive mejorado */
@media (max-width: 640px) {
  .grid-cols-\[50px_1fr_80px_120px_80px_50px\] {
    grid-template-columns: 40px 1fr 70px 100px 70px 40px;
    gap: 0.5rem;
  }
}

/* Mejoras de accesibilidad */
@media (prefers-reduced-motion: reduce) {
  .transition-all,
  .transition-colors,
  .transition-transform {
    transition: none;
  }
  
  .group-hover\:scale-110:hover {
    transform: none;
  }
}

/* Estados de error para validación */
.error-state {
  border-color: rgb(239, 68, 68);
  background-color: rgba(239, 68, 68, 0.1);
}

.error-state:focus {
  ring-color: rgb(239, 68, 68);
}
</style>
