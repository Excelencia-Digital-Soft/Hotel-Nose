
<template>
    <div class="fixed inset-0 bg-black bg-opacity-80 flex justify-center items-center">
      <div class="bg-white rounded-lg p-8 w-1/3">
        <h2 class="text-xl font-bold mb-4">Detalles de Pago</h2>
        <table class="w-full mb-4 text-left">
          <tbody>
            <tr>
              <td class="p-2 font-semibold">Total</td>
              <td class="p-2 text-right">${{ total.toFixed(2) }}</td>
            </tr>
            <tr>
              <td class="p-2 font-semibold">Adicional</td>
              <td class="p-2 text-right">${{ adicional.toFixed(2) }}</td>
            </tr>
            <tr>
              <td class="p-2 font-semibold">Descuento</td>
              <td class="p-2 text-right">
                <input
                  type="number"
                  class="border rounded p-1 w-full"
                  v-model.number="descuento"
                  @input="updateFalta"
                  placeholder="0.00"
                />
              </td>
            </tr>
            <tr>
              <td class="p-2 font-semibold">Efectivo</td>
              <td class="p-2 text-right">
                <input
                  type="number"
                  class="border rounded p-1 w-full"
                  v-model.number="efectivo"
                  @input="updateFalta"
                  placeholder="0.00"
                />
              </td>
            </tr>
            <tr>
              <td class="p-2 font-semibold">Tarjeta</td>
              <td class="p-2 text-right">
                <input
                  type="number"
                  class="border rounded p-1 w-full"
                  v-model.number="tarjeta"
                  @input="updateFalta"
                  placeholder="0.00"
                />
              </td>
            </tr>
            <tr>
              <td class="p-2 font-semibold">MercadoPago</td>
              <td class="p-2 text-right">
                <input
                  type="number"
                  class="border rounded p-1 w-full"
                  v-model.number="mercadoPago"
                  @input="updateFalta"
                  placeholder="0.00"
                />
              </td>
            </tr>
            <tr>
              <td class="p-2 font-semibold">Falta por pagar</td>
              <td class="p-2 text-right text-red-500">${{ faltaPorPagar.toFixed(2) }}</td>
            </tr>
          </tbody>
        </table>
        <div class="flex justify-end space-x-4">
          <button @click="$emit('close')" class="btn-secondary">Cancelar</button>
          <button 
            :disabled="faltaPorPagar > 0"
            @click="crearMovimientoAdicional"
            class="btn-primary">
            Confirmar
          </button>
        </div>
      </div>
    </div>
  </template>
  
  <script>
  import axiosClient from '../axiosClient';

  export default {
    props: {
      total: {
        type: Number,
        required: true,
      },
      adicional: {
        type: Number,
        required: true,
      },
      visitaId: {
        type: Number,
        required: true,
      },
      habitacionId: {
        type: Number,
        required: true,
      },
    },
    data() {
      return {
        descuento: 0,
        efectivo: 0,
        tarjeta: 0,
        mercadoPago: 0,
        faltaPorPagar: this.total + this.adicional,
      };
    },
    methods: {
      updateFalta() {
        console.log(this.total, " ", this.adicional, " ", this.visitaId, " ", this.habitacionId)
        const paid = this.descuento + this.efectivo + this.tarjeta + this.mercadoPago;
        this.faltaPorPagar = this.total + this.adicional - paid;
      },
      crearMovimientoAdicional() {
      // Use the necessary data for the "movimiento"
      const detallesPago = {
        totalFacturado: this.adicional, // or another value for the total
        habitacionId: this.habitacionId, // selected room's ID
        visitaId: this.visitaId, // selected visit's ID
      };

      // Call your API endpoint to create the movimiento
      axiosClient.post(
        `/MovimientoHabitacion?totalFacturado=${detallesPago.totalFacturado}&habitacionId=${detallesPago.habitacionId}&visitaId=${detallesPago.visitaId}`,
      )
      .then(response => {
        console.log('Movimiento agregado exitosamente:', response.data);
        
        // After successful creation of the "movimiento", trigger the payment
        this.pagarVisita();
      })
      .catch(error => {
        console.error('Error al agregar movimiento:', error);
      });
    },
    pagarVisita() {
      // Once the "movimiento" is created, make the payment request
      const paymentData = {
        visitaId: this.visitaId, // Use visitaId from the selected visita
        montoDescuento: this.descuento,
        montoEfectivo: this.efectivo,
        montoTarjeta: this.tarjeta,
        montoBillVirt: this.mercadoPago, // MercadoPago is assumed to be a type of virtual bill
        medioPagoId: 1, // Assuming "1" is the payment method ID for "Efectivo"
      };

      axiosClient.post(
        `/api/Pago/PagarVisita?visitaId=${paymentData.visitaId}&montoDescuento=${paymentData.montoDescuento}&montoEfectivo=${paymentData.montoEfectivo}&montoTarjeta=${paymentData.montoTarjeta}&montoBillVirt=${paymentData.montoBillVirt}&medioPagoId=${paymentData.medioPagoId}`,
      )
      .then(response => {
        console.log('Pago realizado exitosamente:', response.data);
        this.finalizarReserva(this.habitacionId)
      })
      .catch(error => {
        console.error('Error al realizar el pago:', error);
      });
    },
    finalizarReserva(idHabitacion){
      axiosClient.put(`/FinalizarReserva?idHabitacion=${idHabitacion}`)
      .then(response => {
        console.log('Reserva finalizada:', response.data);
      })
      .catch(error => {
        console.error('Error al finalizar reserva:', error);
      });
    }
    },
  };
  
  </script>
  
  <style>
  /* Add your styles here */
  </style>
  