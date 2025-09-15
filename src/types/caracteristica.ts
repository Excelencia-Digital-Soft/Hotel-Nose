export interface CaracteristicaDto {
  caracteristicaId: number
  nombre: string
  descripcion?: string
  icono?: string | File | Blob
  activo?: boolean
  fechaCreacion?: string
  fechaModificacion?: string
}

export interface CaracteristicaCreateDto {
  nombre: string
  descripcion?: string
  icono?: File
}

export interface CaracteristicaUpdateDto {
  nombre: string
  descripcion?: string
  icono?: File
}

export interface CaracteristicaRoomAssignDto {
  caracteristicaIds: number[]
}

export interface CaracteristicaWithImageDto extends CaracteristicaDto {
  icono: string | null
}