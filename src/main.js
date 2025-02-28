import { createApp } from "vue";
import App from "./App.vue";
import router from "./router";
import "./style.css";
import { createPinia } from 'pinia'
import piniaPluginPersistedstate from 'pinia-plugin-persistedstate'
import PrimeVue from 'primevue/config';
import Lara from './presets/lara';      //import preset 
import ToastService from 'primevue/toastservice'; 
import 'primevue/resources/primevue.min.css';
import 'primeicons/primeicons.css';
import { useWebSocketStore } from "./store/websocket";


const pinia = createPinia();
const app = createApp(App);

app.use(pinia);  // ✅ Install Pinia before using stores
pinia.use(piniaPluginPersistedstate);  // ✅ Add persistence after installing Pinia

const websocketStore = useWebSocketStore(); // ✅ Now Pinia is active
websocketStore.connect();

app.use(router)
   .use(PrimeVue, { unstyled: true, pt: Lara })
   .use(ToastService)
   .mount("#app");

