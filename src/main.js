import { createApp } from "vue";
import App from "./App.vue";
import router from "./router";
import "./style.css";
import "./styles/auth.css";
import "./utils/console-filters.js";
import { createPinia } from 'pinia'
import piniaPluginPersistedstate from 'pinia-plugin-persistedstate'
import PrimeVue from 'primevue/config';
import Lara from './presets/lara';      //import preset 
import ToastService from 'primevue/toastservice';
import ConfirmationService from 'primevue/confirmationservice';
import Tooltip from 'primevue/tooltip';
import 'primevue/resources/primevue.min.css';
import 'primeicons/primeicons.css';
import { useWebSocketStore } from "./store/websocket";


const pinia = createPinia();
const app = createApp(App);

app.use(pinia);  // ✅ Install Pinia before using stores
pinia.use(piniaPluginPersistedstate);  // ✅ Add persistence after installing Pinia

// WebSocket will connect automatically when user authenticates and selects institution

app.use(router)
   .use(PrimeVue, { unstyled: true, pt: Lara })
   .use(ToastService)
   .use(ConfirmationService);

app.directive('tooltip', Tooltip);

app.mount("#app");

