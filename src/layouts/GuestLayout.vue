<template>
	<div class="father w-full flex justify-center items-center">
		<div class="container  backdrop-filter backdrop-blur-md bg-opacity-30" :class="{ 'right-panel-active': isSignUp }"
			id="container">
			<div class="form-container sign-up-container">
				<form action="#">
					<h1 class="text-xl text-white tracking-wider">¡Crea tu cuenta!</h1>
					<!-- <div class="social-container">
				<a href="#" class="social"><i class="fab fa-facebook-f"></i></a>
				<a href="#" class="social"><i class="fab fa-google-plus-g"></i></a>
				<a href="#" class="social"><i class="fab fa-linkedin-in"></i></a>
			</div>
			<span>or use your email for registration</span> -->
					<input class="w-full mt-8 relative  text-sm colors-input-login my-2" type="text" placeholder="UserName"
						v-model="registerUserName" />
					<input class="w-full relative  text-sm colors-input-login my-2" type="text" placeholder="Email"
						v-model="registerEmail" />
					<input class="w-full relative  text-sm colors-input-login my-2" type="text" placeholder="Name"
						v-model="registerName" />
					<input class="w-full  relative text-sm colors-input-login my-2" type="password" placeholder="Contraseña"
						v-model="registerPassword" />
					<input class="w-full relative text-sm colors-input-login mt-2 mb-6" type="password"
						placeholder="Repetir constraseña" v-model="registerRepeatPass" />
					<button class="btn-login-style w-full flex justify-center items-center text-md h-12 border-2">
						Crear nueva cuenta</button>
				</form>
			</div>
			<div class="form-container sign-in-container">
				<form action="#">
					<h1 class="text-2xl text-white tracking-wider">¡Bienvenido!</h1>
					<input class="w-full mt-8 text-sm relative colors-input-login my-2" type="text"
						placeholder="Ingresa tu Usuario" v-model="username" />
					<input class="w-full text-sm relative colors-input-login my-2" type="password"
						placeholder="Ingresa tu contraseña" v-model="password" />
					<a class="text-xs" href="#">¿Olvidaste tu contraseña?</a>
					<button class="relative btn-login-style w-full flex justify-center items-center text-md h-12 border-2"
						@click.prevent="authUser" :disabled="isLoading">
						<div class="card absolute top-2 right-8">
							<ProgressSpinner class="absolute " v-if="isLoading" style="width: 25px; height: 25px" strokeWidth="8" fill="transparent"
								animationDuration=".5s" aria-label="Custom ProgressSpinner" />
						</div>
						
						Iniciar Sesión
					</button>
					 <!-- Modal para seleccionar institución -->
    <div v-if="showInstitucionModal" class="absolute institucionModalSelector inset-0 bg-black bg-opacity-50 flex justify-center items-center">
      <div class="bg-white p-6 rounded-lg">
        <h2 class="text-xl font-bold mb-4">Selecciona una institución</h2>
        <ul>
          <li v-for="institucion in authStore.instituciones" :key="institucion.institucionId" class="mb-2">
            <button @click="selectInstitucion(institucion.institucionId)" class="w-full text-left p-2 hover:bg-gray-100">
              {{ institucion.nombre }}
            </button>
          </li>
        </ul>
      </div>
    </div>
				</form>
			</div>
			<div class="overlay-container">
				<div class="overlay bg-primary-700 backdrop-filter backdrop-blur-md bg-opacity-30">
					<div class="overlay-panel text-white overlay-left ">
						<h1>Bienvenido de vuelta!</h1>
						<p>Para mantenerse conectado con nosotros, inicie sesión con su información personal</p>
						<button @click="togglePanel" class="flex items-center cursor-pointer  px-5 h-12 border-2 btn-switch"
							id="signIn">
							Iniciar sesión</button>
					</div>
					<div class="overlay-panel overlay-right">
						<div alt="MOTEL X" class="h-1/4 flex text-white lexend text-[50px] font-bold">inROOM<img
								src="../assets/pin.png" class="h-16 invert" alt=""></div>
						<button @click="togglePanel" class="btn-switch  flex items-center cursor-pointer  px-5 h-12 border-2 mt-8"
							id="signUp">
							Crear nueva cuenta</button>
					</div>
				</div>
			</div>
		</div>
	</div>
</template>
<script setup>
import { ref } from 'vue';
import { useAuthStore } from '../store/auth';
import { useRouter } from 'vue-router';
import ProgressSpinner from 'primevue/progressspinner';
const router = useRouter();
const authStore = useAuthStore();
const isSignUp = ref(false);
let isLoading = ref(false);
//Login
let username = ref('');
let password = ref('');
let showInstitucionModal = ref(false); // Controlar la visibilidad del modal

//Register
let registerUserName = ref('')
let registerEmail = ref('')
let registerName = ref('')
let registerPassword = ref('')
let registerRepeatPass = ref('')


// Login
const authUser = async () => {
  if (isLoading.value) return; // Evitar múltiples solicitudes
  isLoading.value = true; // Activar indicador de carga

  try {
    let credentials = {
      nombreUsuario: username.value,
      contraseña: password.value,
    };

    const success = await authStore.login(credentials);

    if (success) {
      // Si hay más de una institución, mostrar el modal de selección
      if (authStore.instituciones.length > 1) {
        showInstitucionModal.value = true;
      } else {
        // Si solo hay una, redirigir al home
        router.push('/');
      }
    } else {
      alert('Usuario incorrecto');
    }
  } catch (error) {
    console.error('Error al autenticar:', error);
  } finally {
    isLoading.value = false; // Desactivar indicador de carga
  }
};

// Seleccionar institución
const selectInstitucion = (id) => {
  authStore.selectInstitucion(id); // Asignar la institución seleccionada
  showInstitucionModal.value = false; // Cerrar el modal
	console.log("Esto tiene",authStore.institucion)
  router.push('/'); // Redirigir al home
};

const togglePanel = () => {
	isSignUp.value = !isSignUp.value;
};

import axios from 'axios';

// Define la URL a la que enviar la solicitud POST
const url = 'https://ejemplo.com/api/endpoint';

// Define los datos que quieres enviar en el cuerpo de la solicitud
const registerData = {
	nombre: registerName.value,
	email: registerEmail.value,
	username: registerUserName.value,
	password: registerPassword.value,
	repeatPass: registerPassword.value
}

// Realiza la solicitud POST utilizando Axios
// axios.post('/api/LoginContable/Registro', registerData)
//   .then(response => {
//     // Maneja la respuesta exitosa
//     console.log('Respuesta exitosa:', response.data);
// 		alert('Respuesta exitosa:');
// 	})
//   .catch(error => {
//     // Maneja el error en caso de que la solicitud falle
//     console.error('Error al enviar la solicitud:', error);
//   });
</script>

<style scoped>
.father {
	height: 90vh;
}

body {
	background: #f6f5f7;
	display: flex;
	justify-content: center;
	align-items: center;
	flex-direction: column;
	font-family: 'Montserrat', sans-serif;
	height: 100vh;
	margin: -20px 0 50px;
}

h1 {
	font-weight: bold;
	margin: 0;
	font-size: 24px;
}

h2 {
	text-align: center;
}

p {
	font-size: 14px;
	font-weight: 100;
	line-height: 20px;
	font-weight: 300;
	letter-spacing: 0.5px;
	margin: 20px 0 30px;
}

span {
	font-size: 12px;
}

a {
	color: #282828;
	text-decoration: none;
	margin: 15px 0;
}

/* button {
	border-radius: 20px;
	border: 1px solid #FF4B2B;
	background-color: #FF4B2B;
	color: #FFFFFF;
	font-size: 12px;
	font-weight: bold;
	padding: 12px 45px;
	letter-spacing: 1px;
	text-transform: uppercase;
	transition: transform 80ms ease-in;
} */

button:active {
	transform: scale(0.95);
}

button:focus {
	outline: none;
}

button.ghost {
	background-color: transparent;
	border-color: #FFFFFF;
}

form {
	display: flex;
	align-items: center;
	justify-content: center;
	flex-direction: column;
	padding: 0 50px;
	height: 100%;
	text-align: center;
}

/* input {
	background-color: #eee;
	border: none;
	padding: 12px 15px;
	margin: 8px 0;
	width: 100%;
} */

.container {
	border-radius: 10px;
	box-shadow: 0 14px 28px rgba(0, 0, 0, 0.25),
		0 10px 10px rgba(0, 0, 0, 0.22);
	position: relative;
	overflow: hidden;
	width: 768px;
	max-width: 100%;
	min-height: 480px;
}

.form-container {
	position: absolute;
	top: 0;
	height: 100%;
	transition: all 0.6s ease-in-out;
}

.sign-in-container {
	left: 0;
	width: 50%;
	z-index: 2;
}

.container.right-panel-active .sign-in-container {
	transform: translateX(100%);
	opacity: 0;
}

.sign-up-container {
	left: 0;
	width: 50%;
	opacity: 0;
	z-index: 1;
}

.container.right-panel-active .sign-up-container {
	transform: translateX(100%);
	opacity: 1;
	z-index: 5;
	animation: show 0.6s;
}

@keyframes show {

	0%,
	49.99% {
		opacity: 0;
		z-index: 1;
	}

	50%,
	100% {
		opacity: 1;
		z-index: 5;
	}
}

.overlay-container {
	position: absolute;
	top: 0;
	left: 50%;
	width: 50%;
	height: 100%;
	overflow: hidden;
	transition: transform 0.6s ease-in-out;
	z-index: 100;
}

.container.right-panel-active .overlay-container {
	transform: translateX(-100%);
}

.overlay {
	background: -webkit-linear-gradient(to right, #a855f7, #06b6d4);
	background: linear-gradient(to right, #8e2fe7, #ec4899);
	background-repeat: no-repeat;
	background-size: cover;
	background-position: 0 0;
	color: #363636;
	position: relative;
	left: -100%;
	height: 100%;
	width: 200%;
	transform: translateX(0);
	transition: transform 0.6s ease-in-out;
}

.container.right-panel-active .overlay {
	transform: translateX(50%);
}

.overlay-panel {
	position: absolute;
	display: flex;
	align-items: center;
	justify-content: center;
	flex-direction: column;
	padding: 0 40px;
	text-align: center;
	top: 0;
	height: 100%;
	width: 50%;
	transform: translateX(0);
	transition: transform 0.6s ease-in-out;
}

.overlay-left {
	transform: translateX(-20%);
}

.container.right-panel-active .overlay-left {
	transform: translateX(0);
}

.overlay-right {
	right: 0;
	transform: translateX(0);
}

.container.right-panel-active .overlay-right {
	transform: translateX(20%);
}

.social-container {
	margin: 20px 0;
}

.social-container a {
	border: 1px solid #DDDDDD;
	border-radius: 50%;
	display: inline-flex;
	justify-content: center;
	align-items: center;
	margin: 0 5px;
	height: 40px;
	width: 40px;
}

footer {
	background-color: #222;
	color: #fff;
	font-size: 14px;
	bottom: 0;
	position: fixed;
	left: 0;
	right: 0;
	text-align: center;
	z-index: 999;
}

footer p {
	margin: 10px 0;
}

footer i {
	color: red;
}

footer a {
	color: #01B6BD;
	text-decoration: none;
}
</style>