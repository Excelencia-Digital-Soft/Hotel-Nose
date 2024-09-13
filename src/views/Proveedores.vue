<template>
    <div class="flex relative flex-col justify-center items-center">
        <h1 class="text-3xl tracking-normal text-stone-800 font-bold p-6">PROVEEDORES</h1>
        <div class="absolute top-5 left-8 w-80">
          <!-- <YouTubeButton :href="'https://dolarsanjuan.com/'" >Ejemplo boton</YouTubeButton> -->
            <input 
                type="text"
                v-model="keyword" 
                class="focus:ring-purple-500 border-2 w-full focus hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition-colors " 
                placeholder="Buscar proveedor"
            />
        </div>
        <table id="example" class="table" style="width:95%">
        <thead>
            <tr class="header-names">
            <th class="nam">NOMBRE</th>
            <th>CUIT</th>
            <th>UBICACION</th>
            <th>TELEFONO</th>
            <th>EMAIL</th>
            <th><a v-if="authVerif.includes(2)" @click="toggleModal()" class="flex cursor-pointer px-5 py-2 border-2 bg-gradient-to-r from-indigo-300 via-purple-400 to-purple-300  text-white hover:shadow-lg hover:shadow-purple-500/50 border-purple-200 rounded-3xl transition-colors">Nuevo</a></th>
            </tr>
        </thead>
            <tbody v-if="computedProveedores.length > 0">
                <tr  v-if="authVerif.includes(1)" v-for="proveedor in computedProveedores" :key="proveedor.idProv" class="h-16">
                    <td>{{ proveedor.nombre }}</td>
                    <td>{{ proveedor.cuit }}</td>
                    <td>{{ proveedor.provincia }}, {{ proveedor.localidad }}</td>
                    <td>{{ proveedor.telefono }}</td>
                    <td>{{ proveedor.email }}</td>
                    <td class="button-container">
                        <a v-if="authVerif.includes(3)" @click="toggleModal(proveedor,false)" class="flex cursor-pointer w-full px-5 py-2 my-3 mr-2 border-2 font-semibold  border-purple-400 text-purple-500 hover:bg-gradient-to-r from-indigo-300 via-purple-400 to-purple-300  hover:text-white hover:shadow-lg hover:shadow-purple-500/50 hover:border-purple-200 rounded-3xl transition-colors">Editar</a>
                        <a v-if="authVerif.includes(4)" @click=toggleModalAnular(proveedor.idProv,proveedor.nombre) class="flex cursor-pointer w-full px-5 py-2 my-3 mr-2 border-2 font-semibold border-purple-400 text-purple-500 hover:bg-gradient-to-r from-rose-300 via-rose-400 to-red-200  hover:text-white hover:shadow-lg hover:shadow-purple-500/50 hover:border-purple-200 rounded-3xl transition-colors">Anular</a>
                    </td>
                </tr>
            </tbody>
            <tbody class="relative" v-else>
              <tr class="">
                <button type="button" class="cargando absolute top-32 bg-white w-32 font-semibold text-stone-800" disabled>
                  <svg class = "motion-safe:animate-spin" enable-background="new 0 0 24 24" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
                    <path d="M5.1,16c-0.3-0.5-0.9-0.6-1.4-0.4c-0.5,0.3-0.6,0.9-0.4,1.4c0.3,0.5,0.9,0.6,1.4,0.4C5.2,17.1,5.3,16.5,5.1,16C5.1,16,5.1,16,5.1,16z M4.7,6.6C4.2,6.4,3.6,6.5,3.3,7C3.1,7.5,3.2,8.1,3.7,8.4C4.2,8.6,4.8,8.5,5.1,8C5.3,7.5,5.2,6.9,4.7,6.6z M20.3,8.4c0.5-0.3,0.6-0.9,0.4-1.4c-0.3-0.5-0.9-0.6-1.4-0.4c-0.5,0.3-0.6,0.9-0.4,1.4C19.2,8.5,19.8,8.6,20.3,8.4z M4,12c0-0.6-0.4-1-1-1s-1,0.4-1,1s0.4,1,1,1S4,12.6,4,12z M7.2,18.8c-0.5,0.1-0.9,0.7-0.7,1.2c0.1,0.5,0.7,0.9,1.2,0.7c0.5-0.1,0.9-0.7,0.7-1.2C8.3,19,7.8,18.7,7.2,18.8z M21,11c-0.6,0-1,0.4-1,1s0.4,1,1,1s1-0.4,1-1S21.6,11,21,11z M20.3,15.6c-0.5-0.3-1.1-0.1-1.4,0.4c-0.3,0.5-0.1,1.1,0.4,1.4c0.5,0.3,1.1,0.1,1.4-0.4c0,0,0,0,0,0C20.9,16.5,20.8,15.9,20.3,15.6z M17,3.3c-0.5-0.3-1.1-0.1-1.4,0.4c-0.3,0.5-0.1,1.1,0.4,1.4c0.5,0.3,1.1,0.1,1.4-0.4c0,0,0,0,0,0C17.6,4.2,17.5,3.6,17,3.3z M16.8,18.8c-0.5-0.1-1.1,0.2-1.2,0.7c-0.1,0.5,0.2,1.1,0.7,1.2c0.5,0.1,1.1-0.2,1.2-0.7C17.6,19.5,17.3,19,16.8,18.8z M12,20c-0.6,0-1,0.4-1,1s0.4,1,1,1s1-0.4,1-1S12.6,20,12,20z M12,2c-0.6,0-1,0.4-1,1s0.4,1,1,1s1-0.4,1-1S12.6,2,12,2z" fill="#6563ff"/></svg>
                  Processing...
                </button>
              </tr>
            </tbody>
        </table>
        <CreateUpdateProv 
            v-if="show" 
            :proveedorEdit="proveedorEdit" 
            :accion="accion"
            @close-modal="toggleModal"></CreateUpdateProv>
        <ModalAnular v-if="modalAnular" 
        :endPoint="endPoint"
        :name="name" 
        @close="toggleModalAnular"></ModalAnular>
    </div>
</template>

<script setup>
import { computed } from "@vue/reactivity";
import { onMounted, ref } from 'vue';
import axiosClient from '../axiosClient';
import CreateUpdateProv from "../components/CreateUpdateProv.vue";
import ModalAnular from "../components/ModalAnular.vue";
import { useAuthStore } from '../store/auth';
import YouTubeButton from '../components/YouTubeButton.vue'

const authStore = useAuthStore();//si el usuario está logueado y guardado en el state
const authVerif = authStore.auth.datos.permisos;
console.log("PERMISOS DE ESTE USUARIO "+ authVerif); 
const show = ref(false);
const accion = ref(true);
const proveedorEdit = ref(null);

const modalAnular = ref(false);
const name = ref('');
const endPoint=ref('')

function toggleModalAnular(idProv,nameProv){
  modalAnular.value = !modalAnular.value;//Mostramos el modal
  endPoint.value = '/proveedor/CancelProv?pid='+ idProv;;//Mandamos enpoint para anular proveedor
  name.value = nameProv;
}
function toggleModal(provEditData = {},isNewDoc=true){
    accion.value = isNewDoc
    proveedorEdit.value= provEditData
    show.value = !show.value
}
const keyword = ref('');
const proveedores = ref([]);
const computedProveedores = computed(() => proveedores.value.filter(i => i.nombre.toLowerCase().includes(keyword.value.toLowerCase())))

console.log(import.meta.env.VITE_API_BASE_URL);
onMounted(() => {
    axiosClient.get("/proveedor/ProvAll")
        .then(({data})=>{
            
            proveedores.value = data;

        });
});

// IdPermiso	Permiso
// 1	Ver
// 2	Crear
// 3	Editar
// 4	Anular
</script>
<style scoped>
    .cargando{
      left:47%
    }
    thead{
        box-shadow: 0px 0px 9px 0px rgba(179,127,253,1);
        border-radius:2rem;
        height: 100%;
        align-items: center;
        width:100%;
    }

    th, td {
        /*border: 1px solid black;*/
        vertical-align: middle;
        align-items: center;
        padding: 8px;
        text-align: left;
        border-style: none;

    }
    .nam{
        padding-left: 2rem;
    }
    .button-container{
    display:flex;

    }


    .button:hover{
        color:white;
        background: linear-gradient(90deg, rgba(110,114,240,1) 23%, rgba(140,120,240,1) 42%, rgba(170,100,220,1) 100%);
    }
    .button-new {
        width: 100%;
        /* height:  2rem;
        align-items:center;
        display:flex;
        border-radius:2rem;
        justify-content:center;
        text-decoration:none;
        color:white;
        background: linear-gradient(90deg, rgba(120,124,250,1) 23%, rgba(151,125,250,1) 42%, rgba(182,126,251,1) 100%); */
    }

    /* .button-new:hover{
        color:white;
        background: linear-gradient(90deg, rgba(110,114,240,1) 23%, rgba(140,120,240,1) 42%, rgba(170,100,220,1) 100%);
    } */

    tbody, td, tfoot, th, thead, tr{
        border-style: none;
    }
    tr {
        border-bottom: 2px solid #9447ff1a;
         /* Cambia el color de fondo según tu preferencia */
    }
    .header-names {
    border-bottom: none; /* Elimina el borde inferior de la primera fila */
    }
    tbody {
        border-radius: 2rem;
    }
</style>