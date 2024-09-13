<template>
  <Transition name="modal-outer" appear>
    <div class="fondo absolute w-full h-full bg-black border-2 bg-opacity-30 top-0 left-0 flex justify-center px-8 z-40">
      <Transition name="modal-inner">
        <div
          class="flex flex-col h-5/6 fixed verifModal p-12 border-8 border-purple-300/75 shadow-xl shadow-purple-400 my-24  justify-center items-center  pb-12 rounded-xl bg-white self-start mt-16 w-2/3">
          <header class="flex w-full justify-evenly items-center">
            <section>
              <img src="https://i.imgur.com/eAxg2oP.png" alt="UPCN" class="h-12">
              <h1>{{ props.facturaR.nombre }}</h1>
              <h1>condicion:{{ props.facturaR.condicion }}</h1>
            </section>
            <section class="flex text-3xl justify-center items-center w-16 h-16 border-2">
              {{ props.facturaR.tipoFactura }}
            </section>
            <section>
              <h1>FACTURA</h1>
              <h1 class="text-xl text-stone-800 font-bold">{{ numFa }}</h1>
              <h1>Fecha: {{ dateR }} Hora: {{ hourR }}</h1>
              <h2>Cuit: {{ props.facturaR.cuit }}</h2>
            </section>
          </header>

          <body class = "w-full h-full">
            <table class="table h-min-full p-12  my-12 border-2 border-purple-300/75 shadow-lg shadow-purple-400"
              style="width:100%">
              <thead>
                <tr class="header-names">
                  <th>Detalle</th>
                  <th>Importe</th>
                  <th>Alicuota</th>
                  <th>Codigo Deuda</th>
                  <th>Nombre</th>
                </tr>
              </thead>
              <tbody v-if="props.facturaR.detalles.length > 0">
                <tr v-for="detalle in props.facturaR.detalles" :key="detalle.idDetalle">
                  <td>{{ detalle.detalle }}</td>
                  <td>{{ detalle.importe }}</td>
                  <td>{{ detalle.nomAlicuota }} {{ detalle.idAlicuota }} </td>
                  <td>{{ detalle.idCDeuda }}</td>
                  <td>{{ detalle.nombre }}</td>
                </tr>
              </tbody>
              <tbody class="relative" v-else>
                <tr>
                  <button type="button" class="cargando absolute top-32 bg-stone-50 w-32 font-semibold text-stone-800"
                    disabled>
                    Aún no has seleccionado un proveedor...
                  </button>
                </tr>
              </tbody>
            </table>
          </body>

          <div class="flex">
            <button type="button"
              class="flex absolute -bottom-5 right-14 text-lg justify-center items-center w-1/3 h-16 border-2 bg-gradient-to-r from-rose-300 via-rose-400 to-red-200  text-white hover:shadow-lg hover:shadow-red-500/50 border-purple-200 rounded-3xl transition-colors"
              @click="emits('close')">Salir</button>
          </div>
          <button
            class="absolute text-xl  w-14 h-14 text-white -top-7 right-7 flex justify-center items-center border-2 bg-gradient-to-r from-indigo-300 via-purple-400 to-purple-300  hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-full transition-colors"
            @click="emits('close')">X</button>
        </div>
      </Transition>
    </div>
  </Transition>
</template>
  
<script setup>
import axiosClient from '../axiosClient';
import { onMounted, ref } from 'vue';
const emits = defineEmits(["close"])
const props = defineProps({
  facturaR: Object,
});
const numFa = ref(formatNumFact(props.facturaR.numFact));
const dateR = ref(formatDate(props.facturaR.fecEmision));
const hourR = ref(formatHour(props.facturaR.fecEmision));

function formatDate(datetime) {
  const date = datetime.slice(0, 10);
  return date
}
function formatHour(datetime) {
  const hour = datetime.slice(11);
  return hour
}

function formatNumFact(numFact) {
  if (numFact && numFact.length >= 4) {
    const firstPart = numFact.slice(0, 4);
    const secondPart = numFact.slice(4);
    return `${firstPart}-${secondPart}`;
  }
  return numFact; // Si no es lo suficientemente largo, no hacemos cambios.
}
console.log(props.provName)
console.log(props.proveedorId)
</script>
  
<style scoped>
.verifModal {
  padding: 4rem;
  border-collapse: separate;
  text-indent: 0;
  background-color: #FFFFFF;
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='100%25'%3E%3Cdefs%3E%3ClinearGradient id='a' gradientUnits='userSpaceOnUse' x1='0' x2='0' y1='0' y2='100%25' gradientTransform='rotate(343,768,369)'%3E%3Cstop offset='0' stop-color='%23FFFFFF'/%3E%3Cstop offset='1' stop-color='%23E5D1F7'/%3E%3C/linearGradient%3E%3Cpattern patternUnits='userSpaceOnUse' id='b' width='390' height='325' x='0' y='0' viewBox='0 0 1080 900'%3E%3Cg fill-opacity='0.02'%3E%3Cpolygon fill='%23444' points='90 150 0 300 180 300'/%3E%3Cpolygon points='90 150 180 0 0 0'/%3E%3Cpolygon fill='%23AAA' points='270 150 360 0 180 0'/%3E%3Cpolygon fill='%23DDD' points='450 150 360 300 540 300'/%3E%3Cpolygon fill='%23999' points='450 150 540 0 360 0'/%3E%3Cpolygon points='630 150 540 300 720 300'/%3E%3Cpolygon fill='%23DDD' points='630 150 720 0 540 0'/%3E%3Cpolygon fill='%23444' points='810 150 720 300 900 300'/%3E%3Cpolygon fill='%23FFF' points='810 150 900 0 720 0'/%3E%3Cpolygon fill='%23DDD' points='990 150 900 300 1080 300'/%3E%3Cpolygon fill='%23444' points='990 150 1080 0 900 0'/%3E%3Cpolygon fill='%23DDD' points='90 450 0 600 180 600'/%3E%3Cpolygon points='90 450 180 300 0 300'/%3E%3Cpolygon fill='%23666' points='270 450 180 600 360 600'/%3E%3Cpolygon fill='%23AAA' points='270 450 360 300 180 300'/%3E%3Cpolygon fill='%23DDD' points='450 450 360 600 540 600'/%3E%3Cpolygon fill='%23999' points='450 450 540 300 360 300'/%3E%3Cpolygon fill='%23999' points='630 450 540 600 720 600'/%3E%3Cpolygon fill='%23FFF' points='630 450 720 300 540 300'/%3E%3Cpolygon points='810 450 720 600 900 600'/%3E%3Cpolygon fill='%23DDD' points='810 450 900 300 720 300'/%3E%3Cpolygon fill='%23AAA' points='990 450 900 600 1080 600'/%3E%3Cpolygon fill='%23444' points='990 450 1080 300 900 300'/%3E%3Cpolygon fill='%23222' points='90 750 0 900 180 900'/%3E%3Cpolygon points='270 750 180 900 360 900'/%3E%3Cpolygon fill='%23DDD' points='270 750 360 600 180 600'/%3E%3Cpolygon points='450 750 540 600 360 600'/%3E%3Cpolygon points='630 750 540 900 720 900'/%3E%3Cpolygon fill='%23444' points='630 750 720 600 540 600'/%3E%3Cpolygon fill='%23AAA' points='810 750 720 900 900 900'/%3E%3Cpolygon fill='%23666' points='810 750 900 600 720 600'/%3E%3Cpolygon fill='%23999' points='990 750 900 900 1080 900'/%3E%3Cpolygon fill='%23999' points='180 0 90 150 270 150'/%3E%3Cpolygon fill='%23444' points='360 0 270 150 450 150'/%3E%3Cpolygon fill='%23FFF' points='540 0 450 150 630 150'/%3E%3Cpolygon points='900 0 810 150 990 150'/%3E%3Cpolygon fill='%23222' points='0 300 -90 450 90 450'/%3E%3Cpolygon fill='%23FFF' points='0 300 90 150 -90 150'/%3E%3Cpolygon fill='%23FFF' points='180 300 90 450 270 450'/%3E%3Cpolygon fill='%23666' points='180 300 270 150 90 150'/%3E%3Cpolygon fill='%23222' points='360 300 270 450 450 450'/%3E%3Cpolygon fill='%23FFF' points='360 300 450 150 270 150'/%3E%3Cpolygon fill='%23444' points='540 300 450 450 630 450'/%3E%3Cpolygon fill='%23222' points='540 300 630 150 450 150'/%3E%3Cpolygon fill='%23AAA' points='720 300 630 450 810 450'/%3E%3Cpolygon fill='%23666' points='720 300 810 150 630 150'/%3E%3Cpolygon fill='%23FFF' points='900 300 810 450 990 450'/%3E%3Cpolygon fill='%23999' points='900 300 990 150 810 150'/%3E%3Cpolygon points='0 600 -90 750 90 750'/%3E%3Cpolygon fill='%23666' points='0 600 90 450 -90 450'/%3E%3Cpolygon fill='%23AAA' points='180 600 90 750 270 750'/%3E%3Cpolygon fill='%23444' points='180 600 270 450 90 450'/%3E%3Cpolygon fill='%23444' points='360 600 270 750 450 750'/%3E%3Cpolygon fill='%23999' points='360 600 450 450 270 450'/%3E%3Cpolygon fill='%23666' points='540 600 630 450 450 450'/%3E%3Cpolygon fill='%23222' points='720 600 630 750 810 750'/%3E%3Cpolygon fill='%23FFF' points='900 600 810 750 990 750'/%3E%3Cpolygon fill='%23222' points='900 600 990 450 810 450'/%3E%3Cpolygon fill='%23DDD' points='0 900 90 750 -90 750'/%3E%3Cpolygon fill='%23444' points='180 900 270 750 90 750'/%3E%3Cpolygon fill='%23FFF' points='360 900 450 750 270 750'/%3E%3Cpolygon fill='%23AAA' points='540 900 630 750 450 750'/%3E%3Cpolygon fill='%23FFF' points='720 900 810 750 630 750'/%3E%3Cpolygon fill='%23222' points='900 900 990 750 810 750'/%3E%3Cpolygon fill='%23222' points='1080 300 990 450 1170 450'/%3E%3Cpolygon fill='%23FFF' points='1080 300 1170 150 990 150'/%3E%3Cpolygon points='1080 600 990 750 1170 750'/%3E%3Cpolygon fill='%23666' points='1080 600 1170 450 990 450'/%3E%3Cpolygon fill='%23DDD' points='1080 900 1170 750 990 750'/%3E%3C/g%3E%3C/pattern%3E%3C/defs%3E%3Crect x='0' y='0' fill='url(%23a)' width='100%25' height='100%25'/%3E%3Crect x='0' y='0' fill='url(%23b)' width='100%25' height='100%25'/%3E%3C/svg%3E");
  background-size: cover;
}

.table {
  border-collapse: collapse;
  width: 100%;
}

.table, .table th, .table td {
  border: 2px solid #c4c4c471; /* Establece el grosor y el color del borde */
}

.table th, .table td {
  padding: 8px; /* Añade espacio al contenido de las celdas */
  text-align: left; /* Alinea el texto a la izquierda */
}

.table th {
  background-color: #f2f2f2; /* Agrega un color de fondo para los encabezados */
}
.fondo {
  height: 300%;
  /*MEJORAR*/
}

.modal-outer-enter-active,
.modal-outer-leave-active {
  transition: opacity 0.3s cubic-bezier(0.52, 0.02, 0.19, 1.02);
}

.modal-outer-enter-from,
.modal-outer-leave-to {
  opacity: 0;
}

.modal-inner-enter-active {
  transition: all 0.3s cubic-bezier(0.52, 0.02, 0.19, 1.02) 0.10s;
}

.modal-inner-leave-active {
  transition: all 0.3s cubic-bezier(0.52, 0.02, 0.19, 1.02);
}

.modal-inner-enter-from {
  opacity: 0;
  transform: scale(0.8);
}

.modal-inner-leave-to {
  transform: scale(0.8);
}</style>
  