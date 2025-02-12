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


const pinia = createPinia()
pinia.use(piniaPluginPersistedstate)
const app = createApp(App)


app.use(router)
   .use(pinia)
   .use(PrimeVue, { unstyled: true, pt: Lara  })
   .use(ToastService)
   .mount("#app");
