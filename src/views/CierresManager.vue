<template>
    <div class="text-white min-h-screen p-4">
      <h1 class="text-2xl font-bold mb-6">Cierres y Pagos</h1>
  
      <!-- List of Cierres -->
      <div class="mb-6">
        <h3 class="font-semibold text-lg mb-4">Cierres</h3>
        
        <button v-if="pagosSinCierres"
        class="cursor-pointer hover:text-blue-600 border border-white p-4 h-[15vh] flex items-center justify-center w-full"

          @click="abrirPagosSinCierre" 
          type="button" 
        >
          Cierre Actual
        </button>
        <ul v-if="authStore.auth && (authStore.auth.rol === 1 || authStore.auth.rol === 2)" class="space-y-2">
          <li 
            v-for="cierre in cierres" 
            :key="cierre.cierreId" 
            @click="fetchPagosByCierre(cierre.cierreId)" 
            class="cursor-pointer hover:text-blue-600 border border-white p-4 h-[15vh] flex items-center justify-center"
          >
            {{ cierre.cierreId === 0 ? 'Cierre Actual' : `Cierre ${cierre.fechaHoraCierre}` }}
          </li>
        </ul>
      </div>
  
      <!-- Display Pagos for the selected Cierre -->
      <div
  v-if="showPagosModal"
  class="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50"
>
  <ModalCierre
    :selectedPagos="pagos"
    :idcierre="selectedIdCierre"
    :selectedEgresos="egresosSinCierres"
    :esAbierto="false"
    @imprimir-modal="ImprimirModal"
    @close-modal="togglePagosModal">
  </ModalCierre>
  </div>
  <div
  v-if="showPagosSinCierreModal"
  class="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50"
>
  <ModalCierre
    :selectedPagos="pagosSinCierres"
    :selectedEgresos="egresosSinCierres"
    :esAbierto="true"
    @imprimir-modal="ImprimirModal"
    @close-modal="togglePagosSinCierreModal">
  </ModalCierre>
  </div>
    </div>
  </template>

  <script setup>
  import { ref ,onBeforeMount} from 'vue';
  import axiosClient from '../axiosClient'; // Adjust the path to match your project structure
  import ModalCierre from '../components/ModalCierre.vue';
  import { useAuthStore } from "../store/auth";

const authStore = useAuthStore();
  // Data for cierres and pagos
  const cierres = ref([]);
  const showPagosModal = ref(false);
  const showPagosSinCierreModal = ref(false);

  const pagos = ref([]);
  const egresos = ref([]);
  const pagosSinCierres = ref([]);
  const egresosSinCierres = ref([]);
  const selectedIdCierre = ref()

  onBeforeMount(() => {
    getDatosLogin();
    fetchCierres();
});

  const togglePagosModal = () => {
    showPagosModal.value = !showPagosModal.value
  }
  const togglePagosSinCierreModal = () => {
    showPagosSinCierreModal.value = !showPagosSinCierreModal.value
  }
  
  // Fetch all cierres with their associated pagos when the view is loaded
  const fetchCierres = async () => {
    try {
      const response = await axiosClient.get(`/api/Caja/GetCierresyActual?InstitucionID=${InstitucionID.value}`);
      if (response.data.ok) {
        console.log(response.data);
        cierres.value = response.data.data.cierres;
        pagosSinCierres.value = response.data.data.pagosSinCierre;
        egresosSinCierres.value = response.data.data.egresos;
      } else {
        console.error('Error fetching cierres:', response.data.message);
      }
    } catch (error) {
      console.error('Error fetching cierres:', error);
    }
  };

  // Fetch pagos by CierreId when a Cierre is clicked
  const fetchPagosByCierre = (cierreId) => {
    selectedIdCierre.value = cierreId
    console.log("idcierreenviado",selectedIdCierre.value)
    togglePagosModal()
  };

  const abrirPagosSinCierre = () => {
    pagos.value = pagosSinCierres.value
    egresos.value = egresosSinCierres.value;
    togglePagosSinCierreModal()
  };
 
  const ImprimirModal = (cierreCajaRef) => {

  // Get the modal content
  const printContent = cierreCajaRef.innerHTML;

  // Clone styles from document
  const styles = Array.from(document.styleSheets)
    .map((styleSheet) => {
      try {
        return Array.from(styleSheet.cssRules)
          .map((rule) => rule.cssText)
          .join("\n");
      } catch (error) {
        return ""; // Ignore cross-origin styles
      }
    })
    .join("\n");

  // Open a new window
  const printWindow = window.open("", "_blank");

  printWindow.document.write(`
    <html>
      <head>
        <title>Imprimir</title>
        <style>
          ${styles}  
        </style>
      </head>
      <body>
        <div id="cierre-caja-content">${printContent}</div>
        <script>
          window.onload = function() {
            window.print();
            window.onafterprint = function() { 
              window.close(); 
            };           };
        <\/script> 
      </body>
    </html>
`);

  printWindow.document.close();
};


const InstitucionID = ref(null)
function getDatosLogin(){
    InstitucionID.value = authStore.institucionID;
  }
  
  </script>
<style scoped>
/* Ensure the modal content is not truncated */
#cierre-caja-content {
  overflow: visible !important;
}

/* Print-specific styles */
@media print {
  #cierre-caja-content {
    overflow: visible !important;
    height: auto !important;
  }
  #cierre-caja-content * {
    display: grid !important; /* Ensure all elements are visible */
    grid-template-columns: repeat(14, minmax(0, 1fr)) !important;
  }

  body * {
        visibility: hidden;
      }
      #print-container,
      #print-container * {
        visibility: visible;
      }
      #print-container {
        position: absolute;
        left: 0;
        top: 0;
        width: 100%;
        height: auto !important;
        overflow: visible !important;
      }

  ol, ul {
    page-break-inside: avoid;
  }
  li {
    page-break-inside: avoid;
    page-break-after: auto;
  }
}
</style>