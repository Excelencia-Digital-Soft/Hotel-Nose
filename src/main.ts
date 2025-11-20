import { createApp } from 'vue'
import App from './App.vue'
//* @ts-ignore*
import router from './router'
import './style.css'
import './styles/auth.css'
import { createPinia } from 'pinia'
import piniaPluginPersistedstate from 'pinia-plugin-persistedstate'
import PrimeVue from 'primevue/config'
//* @ts-ignore*
import Lara from './presets/lara' //*import preset*
import ToastService from 'primevue/toastservice'
import ConfirmationService from 'primevue/confirmationservice'
import Tooltip from 'primevue/tooltip'
import 'primevue/resources/primevue.min.css'
import 'primeicons/primeicons.css'
//* SignalR Global Setup*
import { useSignalRStore } from './store/signalr'
//* @ts-ignore*
import { useAuthStore } from './store/auth'
import type { SignalRConfig } from './types/signalr'

const pinia = createPinia()
const app = createApp(App)

app.use(pinia) //* ✅ Install Pinia before using stores*
pinia.use(piniaPluginPersistedstate) //* ✅ Add persistence after installing Pinia*

// CONFIGURACIÓN CORREGIDA: Una sola configuración de PrimeVue
app
  .use(router)
  .use(PrimeVue, { 
    unstyled: true, 
    pt: Lara,
    // Configuración de tema integrada
    theme: {
      preset: 'aura',
      options: {
        darkModeSelector: false,
        cssLayer: false
      }
    }
  })
  .use(ToastService)
  .use(ConfirmationService)

app.directive('tooltip', Tooltip)

// MOUNT UNA SOLA VEZ
app.mount('#app')

//* Initialize SignalR after app setup*
const initializeSignalR = async () => {
  try {
    const authStore = useAuthStore()
    const signalRStore = useSignalRStore()
    //* Only initialize if user is authenticated*
    if (authStore.isAuthenticated && authStore.token) {
      const signalRConfig: SignalRConfig = {
        serverUrl:
          import.meta.env.VITE_SIGNALR_HUB_URL || 'http://localhost:5000/api/v1/notifications',
        accessToken: authStore.token,
        automaticReconnect: [0, 2000, 5000, 10000, 30000],
        logging: import.meta.env.DEV, //* Enable logging in development*
      }
      //* Global event handlers*
      const eventHandlers = {
        onReceiveNotification: (type: string, message: string, data?: any) => {
          console.log(`🔔 Global SignalR notification [${type}]:`, message, data)
          //* Show visual SignalR notification*
          import('./utils/toast.ts')
            .then(({ showSignalRToast }) => {
              showSignalRToast(type, message, data)
            })
            .catch((error) => {
              console.error('Failed to show SignalR toast:', error)
            })
        },
        onReconnected: (connectionId: string) => {
          console.log('🔄 SignalR reconnected globally:', connectionId)
        },
        onClose: (error?: Error) => {
          console.log('❌ SignalR connection closed:', error?.message)
        },
      }
      await signalRStore.initialize(signalRConfig, eventHandlers)
      console.log('✅ SignalR initialized globally')
    } else {
      console.log('ℹ️ SignalR not initialized - user not authenticated')
    }
  } catch (error) {
    console.error('❌ Failed to initialize SignalR:', error)
  }
}

//* Watch for authentication changes to initialize/destroy SignalR connection*
const authStore = useAuthStore()
authStore.$subscribe((_: any, state: any) => {
  if (state.isAuthenticated && state.token) {
    //* User just logged in - initialize SignalR*
    initializeSignalR()
  } else {
    //* User logged out - disconnect SignalR*
    const signalRStore = useSignalRStore()
    signalRStore.disconnect()
  }
})

//* Initialize SignalR if user is already authenticated on app start*
initializeSignalR()