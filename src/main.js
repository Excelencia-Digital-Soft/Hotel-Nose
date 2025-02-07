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
import VueHtmlToPaper from 'vue-html-to-paper';


const pinia = createPinia()
pinia.use(piniaPluginPersistedstate)
const app = createApp(App)

const options = {
  name: '_blank',
  specs: [
    'fullscreen=yes',
    'titlebar=yes',
    'scrollbars=yes'
  ],
  styles: ['./style.css',
    `
    @media print {
      * {
        -webkit-print-color-adjust: exact !important;
        print-color-adjust: exact !important;
      }

      #cierre-caja-content {
        display: grid !important;
        grid-template-columns: repeat(5, 1fr) !important; /* Adjust the number of columns */
        gap: 4px !important;
        border: 1px solid black !important;
        padding: 10px !important;
        font-size: 12px !important;
      }

      #cierre-caja-content > div {
        border: 1px solid black !important;
        padding: 4px !important;
        color: black !important;
        background-color: white !important;
        text-align: center; /* Center align text */
      }

      button, .btn-secondary, .absolute {
        display: none !important;
      }

      @page {
        size: A4 landscape;
        margin: 10mm;
      }
    }
    `
  ]
};


app.provide('htmlToPaper', (element) => {
  app.config.globalProperties.$htmlToPaper(element);
});
app.use(router)
   .use(pinia)
   .use(PrimeVue, { unstyled: true, pt: Lara  })
   .use(ToastService)
   .use(VueHtmlToPaper, options)
   .mount("#app");
