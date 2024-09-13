import { defineStore } from "pinia";
import axiosClient from '../axiosClient';

export const useAuthStore = defineStore('auth',{
  state:() => {
    return{
      auth:null
    }
  },
  persist:true,
  getters:{},
  
  actions:{
    async login(credentials){
     try{
      this.auth ={  //ESTO ESTA HARCODEADO BORRAR!! SOLO PRUEBAS
        "token": "eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTUxMiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoibWdvbWV6IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoibWdvbWV6QGV4Y2VsZW5jaWEuY29tIiwiZXhwIjoxNzA4OTYwMDYwfQ.YQWsLhfi3vQVQ-wvXTM5s4JzMwEBfW1tD61udcS4QtIdWCx6DydCnaBnCjgLvokAXuIR8FEv5SD-jD_8LUxXXg",
        "datos": {
          "username": "mgomez",
          "mail": "mgomez@excelencia.com",
          "rol": 1,
          "permisos": [
            1,
            2,
            3,
            4

          ]
        }
      }//ABAJO LO QUE VA A PRODUCCION! 
      // debugger
      // const res = await axiosClient.post('/api/LoginContable/Login', credentials)  
      // this.auth = res.data;
      // console.log(res.data);
      return true;
     }catch(error) {
        console.error(error)
        return false;  
      }
    }
  }
}) 