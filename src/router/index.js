import { createRouter, createWebHistory } from "vue-router";
import DefaultLayout from "../layouts/DefaultLayout.vue";
import GuestLayout from "../layouts/GuestLayout.vue";
import Home from "../views/Home.vue";
import Proveedores from "../views/Proveedores.vue";
import Rooms from "../views/Rooms.vue";
import RoomCreate from "../views/RoomCreate.vue";
import CategoryCreate from "../views/CategoryCreate.vue";
import ArticuloCreate from "../views/ArticuloCreate.vue";
import SubmitOrder from "../views/SubmitOrder.vue";
import { useAuthStore } from '../store/auth';
const routes = [
  {
    path: '/',
    component: DefaultLayout,
    meta:{
      requireAuth:true
    },
    children:[
      {
        path: "/",
        name: "home",
        component: Home,
        meta:{
          requireAuth:false
        }
      },
      {
        path: "/proveedores",
        name: "proveedores",
        component: Proveedores,
        meta:{
          requireAuth:true
        }
      },
      {
        path: "/Rooms",
        name: "Rooms",
        component: Rooms,
        meta:{
          requireAuth:true
        }
      },
      {
        path: "/RoomCreate",
        name: "RoomCreate",
        component: RoomCreate,
        meta:{
          requireAuth:true
        }
      },
      {
        path: "/CategoryCreate",
        name: "CategoryCreate",
        component: CategoryCreate,
        meta:{
          requireAuth:true
        }
      },
      {
        path: "/ArticleCreate",
        name: "ArticleCreate",
        component: ArticuloCreate,
        meta:{
          requireAuth:true
        }
      },
      {
        path: "/SubmitOrder",
        name: "SubmitOrder",
        component: SubmitOrder,
        meta:{
          requireAuth:true
        }
      },
    ]
  },
  {
    path:'/guest',
    component: GuestLayout,
    meta:{
      requireAuth:false
    }
  }  
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach((to, from, next) => {
  // to and from are both route objects. must call `next`.
  const authStore = useAuthStore();//si el usuario está logueado y guardado en el state
  const authVerif = authStore.auth !== null && authStore.auth !== undefined;
  const needAuth = to.meta.requireAuth

  if(needAuth && !authVerif){
    next('/guest')
  }else{
    next()
  }
})

export default router;
