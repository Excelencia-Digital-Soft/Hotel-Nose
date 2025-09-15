/**
 * Cierres Service
 * Handles all API calls related to cash register closures
 * 
 * 🚀 API V1 Only - Legacy endpoints removed
 * Using endpoints from Cierre-Migrate.md
 */

import axiosClient from '../axiosClient';
import type { ApiResponse } from '../types';
import type { 
  CierresyActualDto, 
  CierreDetalleCompletoDto, 
  CajaDetalladaDto,
  CierreBasicoDto,
  CierresPaginatedDto,
  PaginationParams
} from '../types/cierre-model';

// Re-export types for backward compatibility
export type { 
  CierresyActualDto, 
  CierreDetalleCompletoDto, 
  CajaDetalladaDto,
  CierreBasicoDto,
  TransaccionPendienteDto,
  EgresoDetalleDto,
  PagoDto,
  CierresPaginatedDto,
  PaginationParams
} from '../types/cierre-model';

export class CierresService {
  
  // ==================== V1 API METHODS (From Cierre-Migrate.md) ====================
  
  /**
   * Get all closures
   * GET /api/v1/caja
   */
  static async getCierres(): Promise<ApiResponse<CierreBasicoDto[]>> {
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
  static async getCierresConPagos(params: PaginationParams): Promise<ApiResponse<CierresPaginatedDto>> {
    const urlParams = new URLSearchParams({
      page: params.page.toString(),
      pageSize: params.pageSize.toString(),
    });
    
    if (params.startDate) urlParams.append('startDate', params.startDate);
    if (params.endDate) urlParams.append('endDate', params.endDate);
    
    const response = await axiosClient.get(`/api/v1/caja/con-pagos?${urlParams}`);
    return response.data;
  }

  /**
   * Get paginated closures for historical view
   * Uses the con-pagos endpoint for historical data with pagination
   */
  static async getCierresHistoricos(params: PaginationParams): Promise<ApiResponse<CierresPaginatedDto>> {
    try {
      return await this.getCierresConPagos(params);
    } catch (error: any) {
      // If endpoint returns 404 or 405, it's likely not implemented yet
      if (error.response?.status === 404 || error.response?.status === 405) {
        console.warn('⚠️ Paginated endpoint not available, status:', error.response.status);
        throw new Error('ENDPOINT_NOT_AVAILABLE');
      }
      // Re-throw other errors
      throw error;
    }
  }

  /**
   * Create new cash register (Caja)
   * POST /api/v1/caja
   */
  static async createCaja(data: { montoInicial: number; observacion?: string }): Promise<ApiResponse<CierreBasicoDto>> {
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