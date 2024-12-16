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
          <!-- Empeño row -->
          <tr v-if="empenoMonto > 0">
            <td class="p-2 font-semibold">Empeño</td>
            <td class="p-2 text-right text-green-500">-${{ empenoMonto.toFixed(2) }}</td>
          </tr>
          <tr v-if="recargoMonto > 0">
            <td class="p-2 font-semibold">Recargo</td>
            <td class="p-2 text-right text-red-500">${{ recargoMonto.toFixed(2) }}</td>
          </tr>
          <tr>
            <td class="p-2 font-semibold">Total</td>
            <td class="p-2 text-right text-red-500">${{ (total + adicional).toFixed(2) }}</td>
          </tr>
          <tr>
            <td class="p-2 font-semibold">Falta por pagar</td>
            <td class="p-2 text-right text-red-500">${{ faltaPorPagar.toFixed(2) }}</td>
          </tr>
          <tr>
            <td class="p-2 font-semibold">Comentario</td>
            <td class="p-2 text-right">
              <textarea
  class="border rounded p-1 w-full"
  :class="{'border-red-500': descuento > 0 && !comentario}"
  v-model="comentario"
  placeholder="Escribe un comentario..."
></textarea>
          </td>
        </tr>
        </tbody>
      </table>
      <div class="flex justify-end space-x-4">
        <button @click="$emit('close')" class="btn-danger">Cancelar</button>
        <button @click.prevent="toggleEmpenoModal" class="btn-third">Empeño</button>
        <button @click.prevent="toggleRecargoModal" class="btn-third">Recargo</button> <!-- New Recargo button -->
        <button 
          :disabled="faltaPorPagar != 0"
          @click.prevent="crearMovimientoAdicional"
          class="btn-primary">
          Confirmar
        </button>
      </div>
    </div>

    <RecargoModal
  v-if="showRecargoModal"
  @close="showRecargoModal = false"
  @confirm-recargo="confirmoRecargo"
/>
    <!-- Empeño Modal -->
    <EmpenoModal
    v-if="showEmpenoModal" 
    @close="showEmpenoModal = false"
    @confirm-empeno="confirmoEmpeno"
  />
  </div>
</template>

  
  <script>
  import axiosClient from '../axiosClient';
  import EmpenoModal from './EmpenoModal.vue';
  import RecargoModal from './RecargoModal.vue';

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
    components: {
    EmpenoModal,
    RecargoModal,

  },
    data() {
      return {
        descuento: 0,
        efectivo: 0,
        tarjeta: 0,
        mercadoPago: 0,
        faltaPorPagar: this.total + this.adicional,
        empenoMonto: 0, 
    empenoDetalle: '', 
    showEmpenoModal: false, 
    comentario: '',
    recargoDetalle: '',
    recargoMonto: 0,
    showRecargoModal: false, // State to control RecargoModal visibility

      };
    },
    methods: {
      toggleRecargoModal() {
    this.showRecargoModal = true;
  },
  confirmoRecargo(recargo) {
    this.recargoMonto = recargo.monto;
    this.recargoDetalle = recargo.detalle;
    this.updateFalta();
    const recargoObservacion = `Recargo por ${recargo.detalle} con un valor de $${recargo.monto.toFixed(2)}. `;
    this.comentario = this.comentario + recargoObservacion;
    this.showRecargoModal = false;
  },
      updateFalta() {
        const paid = this.descuento + this.efectivo + this.tarjeta + this.mercadoPago + this.empenoMonto;
        this.faltaPorPagar = this.total + this.adicional - paid + this.recargoMonto;
      },
      toggleEmpenoModal() {
    this.showEmpenoModal = !this.showEmpenoModal;
  },
  confirmoEmpeno({ monto, detalle }) {
      this.empenoMonto = monto;
      this.empenoDetalle = detalle;
      const empeñoObservacion = `Empeño de ${detalle} por un valor de $${monto.toFixed(2)}. `;
      this.comentario = empeñoObservacion + this.comentario;
      this.updateFalta();
      this.showEmpenoModal = false;
    },
  crearEmpeno(){
    axiosClient.post(
        `api/Empeño/AddEmpeno?visitaID=${this.visitaId}&detalle=${this.empenoDetalle}&monto=${this.empenoMonto}`,
      ).then(response =>{
        console.log("Empeño exitoso", response.data);
      })
      .catch(error => {
        console.error("ERROR AL CREAR EL EMPEÑO:", error)
        return false;  // Explicitly return false if there's an error

      })
  },
      crearMovimientoAdicional() {
      // Use the necessary data for the "movimiento"
      if (this.descuento < 0) {
        alert("No se puede aplicar descuentos negativos");
        return; // Stop execution if the comentario is not provided
      }
      if (this.descuento > 0 && !this.comentario) {
        alert("El comentario es obligatorio cuando se aplica un descuento.");
        return; // Stop execution if the comentario is not provided
      }
      if (this.empenoMonto > 0){
        const empeñoCreated = this.crearEmpeno(); // Call the empeño creation
      if (empeñoCreated === false) {
        return; // Stop execution if creating empeño failed
      }
      }
      const detallesPago = {
        totalFacturado: this.adicional, // or another value for the total
        habitacionId: this.habitacionId, // selected room's ID
        visitaId: this.visitaId, // selected visit's ID
        comentario: this.comentario
      };

      // Call your API endpoint to create the movimiento
      axiosClient.post(
        `/MovimientoHabitacion?totalFacturado=${detallesPago.totalFacturado}&habitacionId=${detallesPago.habitacionId}&visitaId=${detallesPago.visitaId}&comentario=${detallesPago.comentario}`,
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
        comentario: this.comentario,
        recargoMonto: this.recargoMonto,
        descripcionRecargo: this.recargoDetalle
      };
      if (paymentData.recargoMonto > 0){
        axiosClient.post(
        `/api/Pago/PagarVisita?visitaId=${paymentData.visitaId}&montoDescuento=${paymentData.montoDescuento}&montoEfectivo=${paymentData.montoEfectivo}&montoTarjeta=${paymentData.montoTarjeta}&montoBillVirt=${paymentData.montoBillVirt}&medioPagoId=${paymentData.medioPagoId}&comentario=${paymentData.comentario}&montoRecargo=${paymentData.recargoMonto}&descripcionRecargo=${paymentData.descripcionRecargo}`,
      )
      .then(response => {
        console.log('Pago realizado exitosamente:', response.data);
        this.finalizarReserva(this.habitacionId)
      })
      .catch(error => {
        console.error('Error al realizar el pago:', error);
      });
    }
      else {axiosClient.post(
        `/api/Pago/PagarVisita?visitaId=${paymentData.visitaId}&montoDescuento=${paymentData.montoDescuento}&montoEfectivo=${paymentData.montoEfectivo}&montoTarjeta=${paymentData.montoTarjeta}&montoBillVirt=${paymentData.montoBillVirt}&medioPagoId=${paymentData.medioPagoId}&comentario=${paymentData.comentario}`,
      )
      .then(response => {
        console.log('Pago realizado exitosamente:', response.data);
        this.finalizarReserva(this.habitacionId)
      })
      .catch(error => {
        console.error('Error al realizar el pago:', error);
      });
    }},
    finalizarReserva(idHabitacion){
      axiosClient.put(`/FinalizarReserva?idHabitacion=${idHabitacion}`)
      .then(response => {
        console.log('Reserva finalizada:', response.data);
        window.location.reload();
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
  