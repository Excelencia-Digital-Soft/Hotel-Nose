import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { resolve } from 'path'

// https://vitejs.dev/config/
export default defineConfig(({ command, mode }) => {
  const isProduction = mode === 'production'
  
  return {
    plugins: [vue()],
    base: '/hotel/', 
    server: {
      historyApiFallback: true,
    },
    resolve: {
      alias: {
        '@': resolve(__dirname, 'src'),
        'primeicons': resolve(__dirname, 'node_modules/primeicons'),
      },
    },
    optimizeDeps: {
      include: [],
      exclude: isProduction ? [] : [],
    },
    define: {
      __VUE_PROD_HYDRATION_MISMATCH_DETAILS__: false,
    },
    build: {
      // Production optimizations
      minify: isProduction ? 'terser' : false,
      sourcemap: !isProduction,
      cssCodeSplit: isProduction,
      
      // Terser options for production (only when using terser)
      ...(isProduction && {
        terserOptions: {
          compress: {
            drop_console: true,
            drop_debugger: true,
            pure_funcs: ['console.log', 'console.info', 'console.debug'],
          },
          mangle: {
            safari10: true,
          },
        }
      }),
      
      // Rollup options
      rollupOptions: {
        output: {
          // Manual chunks for better caching
          manualChunks: isProduction ? {
            // Vue core
            'vue-vendor': ['vue', 'vue-router', 'pinia'],
            // PrimeVue components (excluding primeicons due to resolution issues)
            'primevue-vendor': ['primevue/config', 'primevue/toast'],
            // Other vendors
            'utils-vendor': ['axios', 'dayjs', 'date-fns'],
            // Charts and heavy libs
            'charts-vendor': ['chart.js'],
            // SignalR
            'signalr-vendor': ['@microsoft/signalr']
          } : undefined,
        },
      },
      
      // Chunk size warning limit
      chunkSizeWarningLimit: 1000,
      
      // Asset inlining threshold
      assetsInlineLimit: isProduction ? 4096 : 0,
    },
    
    // CSS optimization
    css: {
      devSourcemap: !isProduction,
      preprocessorOptions: {
        scss: {
          additionalData: isProduction ? '' : `/* Development mode */\n`
        }
      }
    },
    
    // Performance hints
    esbuild: {
      drop: isProduction ? ['console', 'debugger'] : [],
      legalComments: isProduction ? 'none' : 'eof',
    },
  }
})