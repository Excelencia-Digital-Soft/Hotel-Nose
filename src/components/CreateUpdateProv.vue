<template>
  <Transition name="modal-outer" appear>
    <div class="h-full min-h-screen absolute w-full bg-black bg-opacity-40 top-0 left-0 flex justify-center px-8">
      <Transition name="modal-inner">
        <div
          class="flex flex-col justify-center fixed p-8 pt-6 border-l-8 border-indigo-200 rounded-xl bg-white self-start mt-6 w-3/4 h-3/4">
          <i class="fa-thin fa-circle-xmark"></i>
          <!-- Modal Content -->
          <h1 class="self-center text-2xl text-stone-800 font-bold mt-5 mb-5">
            {{ accion === false ? prov.nombre : prov.nombre =='' ? 'Nuevo Proveedor': prov.nombre}}
          </h1>
          <div class="absolute flex justify-start items-center h-12 w-52 -top-1 left-16 ">
            <div v-for="tag in selectedTags" :key="tag.idTag"
              class="h-full w-fit px-2 mr-1 flex justify-center cursor-pointer items-center text-white font-semibold drop-shadow-lg hover:-translate-y-1 duration-300 ease-in-out rounded-b-xl rounded-t-sm"
              :style="{
                backgroundColor: tag.color,
                textShadow: '2px 2px 4px rgba(0, 0, 0, 0.2)'
              }
                ">
              {{ tag.nombre }}
            </div>
          </div>

          <form class="grid grid-cols-3 gap-3 mb-2">
            <div class="grid relative mb-3">
              <label for="nombre" class="text-sm font-semibold leading-6 text-gray-800">Nombre</label>
              <input type="text"
                class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition duration-150 ease-out md:ease-in"
                :class="{ 'border-red-400': nombreError }" v-model="prov.nombre" placeholder="Nombre"
                @input="validarNombre">
              <span class="text-red-400 font-bold text-xs absolute -bottom-5">{{ nombreError }}</span>
            </div>
            <div class="grid relative mb-3">
              <label for="cuit" class="text-sm font-semibold leading-6 text-gray-800">CUIT</label>
              <input type="text"
                class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition duration-150 ease-out md:ease-in"
                maxlength="11"
                :class="{ 'border-red-500': cuitError }" v-model="prov.cuit" placeholder="Cuit" @input="validarCuit">
              <span class="text-red-400 font-bold text-xs absolute -bottom-5 ">{{ cuitError }}</span>
            </div>
            <div class="grid shadow-lg row-span-4 w-full rounded-3xl border-2 ">
              <section
                class="grid border-2 w-full p-4 shadow-lg border-purple-200 bg-gradient-to-r from-indigo-200 via-purple-300 to-purple-200  hover:shadow-lg hover:shadow-purple-400/50  rounded-3xl transition duration-150 ease-out md:ease-in">
                <!-- <DropDownGastos :idTipoGasto="prov.idCGasto" @update:selectedId="handleSelectedIdCGasto"></DropDownGastos> -->
                <DropDownCreateSearchGastos :idTipoGasto="prov.idCGasto" @update:selectedId="handleSelectedIdCGasto"/>
                <span class="text-red-400 font-bold text-xs" v-if="!prov.idCGasto">{{ DDGerror }}</span>
                <DropDownIVA :idTipoIva="prov.idCondIva" @update:selectedId="handleSelectedId"></DropDownIVA>
                <span class="text-red-400 font-bold text-xs" v-if="!prov.idCondIva">{{ DDIVAerror }}</span>
              </section>
              <div class="w-full flex justify-center items-center px-4">
                <DropDownTags :provId="prov.idProv" :refresh="cheatRefresh" :idNewTag="idNewTag"
                  @update:selectedIds="handleSelectedTags"></DropDownTags>
                <DropDownNewTag :provId="prov.idProv" @update:cheatIds="handleCheat"></DropDownNewTag>
              </div>

            </div>
            <div class="grid mb-3">
              <label for="direccion" class="text-sm font-semibold leading-6 text-gray-800">Domicilio</label>
              <input type="text"
                class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition duration-150 ease-out md:ease-in"
                v-model="prov.domicilio" placeholder="Colon 120">
            </div>
            <div class="grid mb-3">
              <label for="localidad" class="text-sm font-semibold leading-6 text-gray-800">Localidad</label>
              <input type="text"
                class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition duration-150 ease-out md:ease-in"
                v-model="prov.localidad" placeholder="Localidad">
            </div>
            <div class="grid grid-cols-2">
              <div class="grid pr-2 mb-3">
                <label for="provincia" class="text-sm font-semibold leading-6 text-gray-800">Provincia</label>
                <DropDownProvincias :idProvincia="prov.id" @update:selectedId="handleSelectedIdProvincia">
                </DropDownProvincias>
                <span class="text-red-400 font-bold text-xs">{{ provinciaError }}</span>
              </div>
              <div class="grid pl-2 mb-3">
                <label for="codigopostal" class="text-sm font-semibold leading-6 text-gray-800">C.P.</label>
                <input type="number"
                  class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition duration-150 ease-out md:ease-in"
                  v-model="prov.codPostal" placeholder="Codigo Postal">
              </div>
            </div>

            <div class="grid relative mb-3">
              <label for="telefono" class="text-sm font-semibold leading-6 text-gray-800">Telefono</label>
              <input type="text"
                class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition duration-150 ease-out md:ease-in"
                :class="{ 'border-red-400': telefonoError }" v-model="prov.telefono" placeholder="Telefono"
                @input="validarTelefono">
              <span class="text-red-400 font-bold text-xs absolute -bottom-5">{{ telefonoError }}</span>
            </div>


            <div class="grid grid-cols-2">
              <label
                class="flex flex-col items-center group cursor-pointer rounded-3xl p-2 mr-2 border-2 focus hover:shadow-lg hover:shadow-purple-500/50 hover:bg-purple-400 border-purple-200 transition duration-150 ease-out md:ease-in">
                <div class="flex items-center justify-start w-full">
                  <span
                    class="h-6 w-6 mr-4 text-purple-600 group-hover:text-white material-symbols-outlined">library_add</span>
                  <h1 class="font-semibold text-stone-800 group-hover:text-white">Agregar Archivos</h1>
                  <input type="file" class="hidden" />
                </div>
              </label>
              <div class="grid pl-2 relative mb-3">
                <label for="ingbrutos" class="text-sm font-semibold leading-6 text-gray-800">Ing. Brutos</label>
                <input type="text"
                  class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition duration-150 ease-out md:ease-in"
                  :class="{ 'border-red-400': iBrutosError }" v-model="prov.ingBrutos" placeholder="Nº de Ingresos Brutos">
                <span class="text-red-400 font-bold text-xs absolute -bottom-5">{{ iBrutosError }}</span>
              </div>

            </div>
            <div class="grid relative mb-3">
              <label for="email" class="text-sm font-semibold leading-6 text-gray-800">Email</label>
              <input type="text"
                class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition duration-150 ease-out md:ease-in"
                v-model="prov.email" placeholder="">
            </div>

            <div class="col-span-3 flex justify-center items-center w-full">
              <input type="hidden" v-model="prov.idProv" name="idProv">
              <button @click="accion ? crearNuevoProv() : editarProv()" type="button"
                class="flex justify-center items-center w-2/4 text-xl shadow-lg h-16 mt-8 border-2 bg-gradient-to-r from-indigo-300 via-purple-400 to-purple-300 font-bold tracking-wider text-white hover:shadow-lg hover:shadow-purple-500/50 transition duration-150 ease-out md:ease-in border-purple-200 rounded-2xl ">{{
                  accion
                  ? 'Crear Proveedor' : 'Editar Proveedor' }}</button>
            </div>
          </form>
          <button
            class="absolute text-xl  w-14 h-14 text-white -top-7 right-7 flex justify-center items-center border-2 bg-gradient-to-r from-indigo-300 via-purple-400 to-purple-300  hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-full transition duration-150 ease-out md:ease-in"
            @click="$emit('close-modal')">X</button>
        </div>
      </Transition>
    </div>
  </Transition>
</template>
    
<script setup>
import { computed } from 'vue';
import { onMounted, ref } from 'vue';
import axiosClient from '../axiosClient';
import DropDownIVA from './DropDownIVA.vue';
import DropDownTags from './DropDownTags.vue';
import DropDownNewTag from './DropDownNewTag.vue';
import DropDownProvincias from './DropDownProvincias.vue';
import DropDownCreateSearchGastos from './DropDownCreateSearchGastos.vue';
const emits = defineEmits(["close-modal"])
const props = defineProps({
  proveedorEdit: Object,
  accion: Boolean
});



onMounted(() => {
  prov.value = { ...prov.value, ...props.proveedorEdit }
})

let prov = ref({
  idProv: 0,
  nombre: '',
  cuit: '',
  telefono: '',
  ingBrutos: '',
  provincia: '',
  localidad: '',
  domicilio: '',
  codPostal: 0,
  idCondIva: '',
  idInstitucion: 1,
  idCGasto: ''
})


//RECIBE EL VALOR SELECCIONADO EN LOS COMPONENTES DE DROPDOWN PARA LUEGO AGREGARLO en lo que se va a enviar en la peticion de crear o editar
const handleSelectedId = (selectedId) => {
  console.log("Valor de selectedId:" + selectedId.condicion);
  prov.value.idCondIva = selectedId.idCondicion;
};
const handleSelectedIdCGasto = (selectedId) => {
  console.log("Valor de selectedId GASTO: ", selectedId.idCGasto);
  prov.value.idCGasto = selectedId.idCGasto;
};
const handleSelectedIdProvincia = (nombreProvincia) => {
  console.log("Valor de selected idProvincia: ", nombreProvincia);
  prov.value.provincia = nombreProvincia;
};
const selectedTags = ref([]);
let editTagRel = {}
const handleSelectedTags = (selectedIds) => {

  const parsedIds = JSON.parse(selectedIds);
  // const parsedIds = selectedIds;
  console.log("Valor de TAGS:", parsedIds);
  selectedTags.value = parsedIds;
  const idsArray = parsedIds.map(tag => tag.idTag);
  console.log("id Tags:", idsArray);

  editTagRel = {
    idProv: prov.value.idProv,
    tags: idsArray
  }

};
let cheatRefresh = ref(false);
let idNewTag = ref(0);
const handleCheat = (cheatIds) => {
  //le avisamos al componente DropDownTag que actualice para agregar los nuevos datos
  cheatRefresh.value = true
  idNewTag.value = cheatIds
  console.log("1 el numero", cheatIds)

};

console.log("tagselected : " + editTagRel.value)

//SECTOR DE VALIDACIONES DE FORMULARIO
let nombreError = ref('');
let provinciaError = ref('');
let DDGerror = ref('');
let DDIVAerror = ref('');
let cuitError = ref('');
let telefonoError = ref('');
let iBrutosError = ref('');

const validarNombre = () => {
  const nombre = prov.value.nombre.trim();
  if (!nombre) {
    nombreError.value = 'El nombre es obligatorio';
  } else if (!/^[a-zA-ZÀ-ÿ\s]*$/.test(nombre)) {
    nombreError.value = 'No se permiten numeros';
  } else {
    nombreError.value = '';
  }
};

//const validarProvincia = () => { provinciaError.value = !prov.selected.value ? 'Campo obligatorio' : ''; };
const validarDDown = () => { DDGerror.value = !prov.value.idCGasto ? 'Campo obligatorio' : ''; };
const validarDDownIVA = () => { DDIVAerror.value = !prov.value.idCondIva ? 'Campo obligatorio' : ''; };
const validarCuit = () => {
  const cuit = prov.value.cuit.trim();
  if (!cuit) {
    cuitError.value = 'El cuit es obligatorio';
  } else if (!/^\d{11}$/.test(cuit)) {
    cuitError.value = 'El cuit solo debe contener 11 números';
  } else {
    cuitError.value = '';
  }
};

const validarTelefono = () => {
  const telefono = prov.value.telefono;
  if (!telefono) {
    telefonoError.value = 'El telefono es obligatorio'
  } else if (!/^\d+$/.test(telefono)) {
    telefonoError.value = 'Solo se permiten números';
  } else {
    telefonoError.value = '';
  }
}




//CREAR NUEVO PROVEEDOR 
const crearNuevoProv = () => {
  validarNombre();
  validarCuit();
  validarDDown();
  validarDDownIVA();
  validarTelefono();


  if (nombreError.value || cuitError.value || telefonoError.value || DDGerror.value || DDIVAerror.value || provinciaError.value) {
    // No envíes el formulario si hay errores de validación
    console.log("faltan datos obligatorios perra")
    return;
  }
  console.log("loquese Envía",prov.value)
  axiosClient.post('/proveedor/NewProv', prov.value)
    .then(res => {
      console.log(res.data); // Imprime los detalles del error del servidor, si están disponibles.
      if (editTagRel.tags.length > 0) {
        const idP = res.data; // Obtén el idProv de la respuesta
        editTagRel.idProv = idP; // Asigna el idProv a editTagRel
        axiosClient.put('/proveedor/Changer', editTagRel)
          .then(res => {
            console.log("Se agregaron Tags: " + res.data); // Imprime los detalles del error del servidor, si están disponibles.
          })
          .catch(error => {
            console.error(error);
          });
      }
      alert("Nuevo proveedor creado correctamente");
      emits('close-modal');
      window.location.reload();
    })
    .catch(error => {
      console.error(error);
    });
}

//EDITAR PROVEEDOR 
const editarProv = () => {
  validarNombre();
  validarCuit();
  validarDDown();
  validarDDownIVA();
  validarTelefono();

  if (nombreError.value || cuitError.value || DDGerror.value || DDIVAerror.value || iBrutosError.value) {
    // No envíes el formulario si hay errores de validación
    console.log("faltan datos obligatorios perra")
    return;
  }
  axiosClient.put('/proveedor/EditProv', prov.value)
    .then(res => {
      console.log("se editó:" + res.data); // Imprime los detalles del error del servidor, si están disponibles.
      if (editTagRel.tags.length > 0) {
        //AGREGAR LOS TAGS  
        axiosClient.put('/proveedor/Changer', editTagRel)
          .then(res => {
            console.log("se agregaron Tags :" + res.data);
          })
          .catch(error => {
            console.error(error);
          });

      }
      alert("se editó correctamente");
      emits('close-modal');
      window.location.reload();
    })
    .catch(error => {
      console.error(error);
    });
}
</script>
<style scoped>
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
}
</style>
    