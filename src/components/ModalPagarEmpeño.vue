<template>
    <div class="fixed inset-0 bg-black bg-opacity-80 flex justify-center items-center">
      <div class="bg-white rounded-lg p-8 w-1/3">
        <h2 class="text-xl font-bold mb-4 text-black">Detalles de Pago</h2>
        <table class="w-full mb-4 text-left">
          <tbody>
            <tr>
              <td class="p-2 font-semibold text-black">Total</td>
              <td class="p-2 text-right text-black">${{ total.toFixed(2) }}</td>
            </tr>
            <tr>
              <td class="p-2 font-semibold text-black">Efectivo</td>
              <td class="p-2 text-right text-black">
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
              <td class="p-2 font-semibold text-black">Tarjeta</td>
              <td class="p-2 text-right text-black">
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
              <td class="p-2 font-semibold text-black">MercadoPago</td>
              <td class="p-2 text-right text-black">
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
              <td class="p-2 font-semibold text-black">Falta por pagar</td>
              <td class="p-2 text-right text-red-500">${{ faltaPorPagar.toFixed(2) }}</td>
            </tr>
            <tr>
              <td class="p-2 font-semibold text-black">Total</td>
              <td class="p-2 text-right text-red-500">${{ (total).toFixed(2) }}</td>
            </tr>
          </tbody>
        </table>
        <div class="flex justify-end space-x-4">
          <button @click="$emit('close')" class="btn-danger">Cancelar</button>
          <button 
            :disabled="faltaPorPagar != 0"
            @click.prevent="pagarEmpeno"
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
        empenoId: {
          type: Number,
          required: true,
        },
      },
      data() {
        return {
          efectivo: 0,
          tarjeta: 0,
          mercadoPago: 0,
          faltaPorPagar: this.total,
        };
      },
      methods: {
        updateFalta() {
          const paid = this.efectivo + this.tarjeta + this.mercadoPago;
          this.faltaPorPagar = this.total - paid;
        },
      pagarEmpeno() {
        const paymentData = {
          empenoId: this.empenoId,
          montoEfectivo: this.efectivo,
          montoTarjeta: this.tarjeta,
          montoBillVirt: this.mercadoPago, 
        };
  
        axiosClient.post(
          `/api/Empeño/PagarEmpeno?empeñoId=${paymentData.empenoId}&montoEfectivo=${paymentData.montoEfectivo}&montoTarjeta=${paymentData.montoTarjeta}&montoBillVirt=${paymentData.montoBillVirt}`,
        )
        .then(response => {
          console.log('Pago realizado exitosamente:', response.data);
          window.location.reload();
        })
        .catch(error => {
          console.error('Error al realizar el pago:', error);
        });
      },
      },
    };
    
    </script>
    
    <style>
    /* Add your styles here */
    </style>
    