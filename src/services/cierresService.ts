/**
 * Cierres Service
 * Handles all API calls related to cash register closures
 * 
 * 🚀 API V1 Only - Legacy endpoints removed
 * Using endpoints from Cierre-Migrate.md
 */

import axiosClient from '../axiosClient';
import type { ApiResponse } from '../types';

// ==================== V1 DTOs ====================

export interface CajaDto {
  cajaId: number;
  fechaHoraCierre: string;
  montoInicial: number;
  montoFinal: number;
  totalEfectivo: number;
  totalTarjeta: number;
  totalDescuentos: number;
  observacion?: string;
  usuarioId: number;
  institucionId: number;
}

export interface TransaccionDto {
  pagoId: number;
  fecha: string;
  montoEfectivo: number;
  montoTarjeta: number;
  montoDescuento: number;
  montoBillVirt?: number;
  tipoHabitacion?: string;
  categoriaNombre?: string;
  periodo?: number;
  montoAdicional?: number;
  totalConsumo?: number;
  horaIngreso?: string;
  horaSalida?: string;
  tarjetaNombre?: string;
  observacion?: string;
  cierreId?: number;
  visitaId?: number;
}

export interface EgresoDto {
  egresoId: number;
  fecha: string;
  montoEfectivo: number;
  montoTarjeta: number;
  montoDescuento: number;
  observacion?: string;
  cierreId?: number;
  usuarioId: number;
  institucionId: number;
}

export interface CierresyActualDto {
  cierres: CajaDto[];
  transaccionesPendientes: TransaccionDto[];
  egresosPendientes: EgresoDto[];
}

export interface CierreDetalleCompletoDto {
  cierre: CajaDto;
  pagos: TransaccionDto[];
  anulaciones: TransaccionDto[];
  egresos: EgresoDto[];
}

export interface CajaDetalladaDto {
  cajaId: number;
  fechaHoraCierre: string;
  montoInicial: number;
  montoFinal: number;
  totalEfectivo: number;
  totalTarjeta: number;
  totalDescuentos: number;
  observacion?: string;
  usuarioNombre: string;
  institucionNombre?: string;
}

export class CierresService {
  
  // ==================== V1 API METHODS (From Cierre-Migrate.md) ====================
  
  /**
   * Get all closures
   * GET /api/v1/caja
   */
  static async getCierres(): Promise<ApiResponse<CajaDto[]>> {
    const response = await axiosClient.get('/api/v1/caja');
    return response.data;
  }

  /**
   * Get closures and pending transactions
   * GET /api/v1/caja/actual
   */
  static async getCierresyActual(): Promise<ApiResponse<CierresyActualDto>> {
    const response = await axiosClient.get('/api/v1/caja/actual');
    return response.data;
  }

  /**
   * Get specific closure
   * GET /api/v1/caja/{id}
   */
  static async getCierre(cajaId: number): Promise<ApiResponse<CajaDetalladaDto>> {
    const response = await axiosClient.get(`/api/v1/caja/${cajaId}`);
    return response.data;
  }

  /**
   * Get complete closure details
   * GET /api/v1/caja/{id}/detalle
   */
  static async getDetalleCierre(cajaId: number): Promise<ApiResponse<CierreDetalleCompletoDto>> {
    const response = await axiosClient.get(`/api/v1/caja/${cajaId}/detalle`);
    return response.data;
  }

  /**
   * Get closures with payments (paginated)
   * GET /api/v1/caja/con-pagos
   */
  static async getCierresConPagos(page: number = 1, pageSize: number = 10): Promise<ApiResponse<any>> {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });
    const response = await axiosClient.get(`/api/v1/caja/con-pagos?${params}`);
    return response.data;
  }

  /**
   * Create new cash register (Caja)
   * POST /api/v1/caja
   */
  static async createCaja(data: { montoInicial: number; observacion?: string }): Promise<ApiResponse<CajaDto>> {
    const response = await axiosClient.post('/api/v1/caja', data);
    return response.data;
  }

  /**
   * Close cash register (Custom endpoint not in migration doc)
   * This needs to be verified with backend team
   */
  static async cerrarCaja(montoInicial: number, observacion: string = 'Cierre de caja'): Promise<ApiResponse<any>> {
    // TODO: Verify correct V1 endpoint for closing cash register
    // This might be handled differently in V1
    const response = await axiosClient.post('/api/v1/caja', { 
      montoInicial, 
      observacion 
    });
    return response.data;
  }
}