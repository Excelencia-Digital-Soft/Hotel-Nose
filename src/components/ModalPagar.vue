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
            @click="confirmPayment"
            class="btn-primary">
            Confirmar
          </button>
        </div>
      </div>
    </div>
  </template>
  
  <script>
  export default {
    props: {
      total: {
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
        faltaPorPagar: this.total,
      };
    },
    methods: {
      updateFalta() {
        const paid = this.descuento + this.efectivo + this.tarjeta + this.mercadoPago;
        this.faltaPorPagar = this.total - paid;
      },
      confirmPayment() {
        const paymentDetails = {
          descuento: this.descuento,
          efectivo: this.efectivo,
          tarjeta: this.tarjeta,
          mercadoPago: this.mercadoPago,
        };
        this.$emit('confirm-payment', paymentDetails);
      },
    },
  };
  </script>
  
  <style>
  /* Add your styles here */
  </style>
  