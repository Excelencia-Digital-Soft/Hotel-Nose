# API V1 Migration Guide

Esta guía describe cómo migrar del frontend actual a los nuevos endpoints V1 con arquitectura RESTful.

## 🎯 Endpoints Migrados

### 1. Consumos

| Legacy Endpoint | Nuevo V1 Endpoint | Método | Descripción |
|---|---|---|---|
| `/ConsumoGeneral` | `/api/v1/consumos/general` | POST | Agregar consumos generales |
| `/ConsumoHabitacion` | `/api/v1/consumos/room` | POST | Agregar consumos de habitación |
| `/GetConsumosVisita` | `/api/v1/consumos/visita/{visitaId}` | GET | Obtener consumos por visita |
| `/AnularConsumo` | `/api/v1/consumos/{consumoId}` | DELETE | Anular consumo |
| `/UpdateConsumo` | `/api/v1/consumos/{consumoId}` | PUT | Actualizar consumo |

### 2. Reservas

| Legacy Endpoint | Nuevo V1 Endpoint | Método | Descripción |
|---|---|---|---|
| `/FinalizarReserva` | `/api/v1/reservas/finalize` | POST | Finalizar reserva |
| `/PausarOcupacion` | `/api/v1/reservas/{visitaId}/pause` | POST | Pausar ocupación |
| `/ActualizarReservaPromocion` | `/api/v1/reservas/{reservaId}/promotion` | PUT | Actualizar promoción |

### 3. Promociones

| Legacy Endpoint | Nuevo V1 Endpoint | Método | Descripción |
|---|---|---|---|
| `/api/promociones/GetPromocionesCategoria` | `/api/v1/promociones/categoria/{categoriaId}` | GET | Obtener promociones por categoría |

## 🔄 Cambios en el Frontend

### 1. Consumos - Migración de Funciones

#### **Función: `agregarconsumos`**
```javascript
// ❌ LEGACY (Obsoleto)
const agregarconsumos = (selecteditems) => {
  axiosclient.post(
    `/consumogeneral?habitacionid=${selectedroom.value.habitacionid}&visitaid=${selectedroom.value.visitaid}`,
    selecteditems
  )
}

// ✅ NUEVO V1
const agregarconsumos = (selecteditems) => {
  axiosclient.post(
    `/api/v1/consumos/general?habitacionId=${selectedroom.value.habitacionid}&visitaId=${selectedroom.value.visitaid}`,
    selecteditems
  )
}
```

#### **Función: `agregarconsumoshabitacion`**
```javascript
// ❌ LEGACY (Obsoleto)
const agregarconsumoshabitacion = (selecteditems) => {
  axiosclient.post(
    `/consumohabitacion?habitacionid=${selectedroom.value.habitacionid}&visitaid=${selectedroom.value.visitaid}`,
    selecteditems
  )
}

// ✅ NUEVO V1
const agregarconsumoshabitacion = (selecteditems) => {
  axiosclient.post(
    `/api/v1/consumos/room?habitacionId=${selectedroom.value.habitacionid}&visitaId=${selectedroom.value.visitaid}`,
    selecteditems
  )
}
```

#### **Función: `actualizarconsumos`**
```javascript
// ❌ LEGACY (Obsoleto)
const actualizarconsumos = () => {
  axiosclient.get(`/getconsumosvisita?visitaid=${selectedroom.value.visitaid}`)
    .then(({ data }) => {
      // Procesamiento legacy...
    })
}

// ✅ NUEVO V1
const actualizarconsumos = () => {
  axiosclient.get(`/api/v1/consumos/visita/${selectedroom.value.visitaid}`)
    .then(({ data }) => {
      if (data.isSuccess && data.data) {
        consumos.value = data.data;
      }
    })
}
```

#### **Función: `anularconsumo`**
```javascript
// ❌ LEGACY (Obsoleto)
const anularconsumo = (consumoid) => {
  axiosclient.delete(`/anularconsumo?idconsumo=${consumoid}`)
}

// ✅ NUEVO V1
const anularconsumo = (consumoid) => {
  axiosclient.delete(`/api/v1/consumos/${consumoid}`)
}
```

#### **Función: `saveconsumo`**
```javascript
// ❌ LEGACY (Obsoleto)
const saveconsumo = (consumoid) => {
  axiosclient.put(`/updateconsumo?idconsumo=${consumoid}&cantidad=${editedcantidad.value}`)
}

// ✅ NUEVO V1
const saveconsumo = (consumoid) => {
  axiosclient.put(`/api/v1/consumos/${consumoid}`, {
    cantidad: editedcantidad.value
  })
}
```

### 2. Reservas - Migración de Funciones

#### **Función: `endroomreserve`**
```javascript
// ❌ LEGACY (Obsoleto)
const endroomreserve = () => {
  axiosclient.put(`/finalizarreserva?idhabitacion=${selectedroom.value.habitacionid}`)
}

// ✅ NUEVO V1
const endroomreserve = () => {
  axiosclient.post(`/api/v1/reservas/finalize?habitacionId=${selectedroom.value.habitacionid}`)
}
```

#### **Función: `openpaymentmodal` - Pausar Ocupación**
```javascript
// ❌ LEGACY (Obsoleto)
await axiosclient.put(`/pausarocupacion?visitaid=${selectedroom.value.visitaid}`);

// ✅ NUEVO V1
await axiosclient.post(`/api/v1/reservas/${selectedroom.value.visitaid}/pause`);
```

#### **Función: `actualizarpromocion`**
```javascript
// ❌ LEGACY (Obsoleto)
const actualizarpromocion = () => {
  axiosclient.put('/actualizarreservapromocion', null, {
    params: {
      reservaid: selectedroom.value.reservaid,
      promocionid: promocionid,
    },
  })
}

// ✅ NUEVO V1
const actualizarpromocion = () => {
  axiosclient.put(`/api/v1/reservas/${selectedroom.value.reservaid}/promotion`, {
    promocionId: promocionid
  })
}
```

### 3. Promociones - Migración de Funciones

#### **Función: Obtener Promociones por Categoría**
```javascript
// ❌ LEGACY (Obsoleto)
const response = await axiosclient.get(`/api/promociones/getpromocionescategoria?categoriaid=${props.room.categoriaid}`);

// ✅ NUEVO V1
const response = await axiosclient.get(`/api/v1/promociones/categoria/${props.room.categoriaid}`);
```

## 📊 Estructura de Respuestas V1

### ApiResponse Estándar
Todos los endpoints V1 devuelven respuestas en formato `ApiResponse`:

```javascript
{
  "isSuccess": true,
  "data": { /* datos específicos */ },
  "errors": [],
  "message": "Operation completed successfully"
}
```

### Manejo de Errores
```javascript
// ✅ Manejo correcto de respuestas V1
axiosclient.get('/api/v1/consumos/visita/123')
  .then(response => {
    if (response.data.isSuccess) {
      // Procesar response.data.data
      consumos.value = response.data.data;
    } else {
      // Mostrar errores
      console.error('Errors:', response.data.errors);
    }
  })
  .catch(error => {
    console.error('Network error:', error);
  });
```

## 🔐 Autenticación

### Todos los endpoints V1 requieren autenticación Bearer Token:

```javascript
// Configurar interceptor de Axios
axiosclient.interceptors.request.use(config => {
  const token = localStorage.getItem('authToken'); // o donde tengas el token
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});
```

## 🎯 Pasos de Migración Recomendados

### Fase 1: Configurar Autenticación
1. Implementar interceptor de Axios para Bearer Token
2. Asegurar que todos los requests incluyan el token

### Fase 2: Migrar Consumos
1. Actualizar `agregarconsumos`
2. Actualizar `agregarconsumoshabitacion`
3. Actualizar `actualizarconsumos`
4. Actualizar `anularconsumo`
5. Actualizar `saveconsumo`

### Fase 3: Migrar Reservas
1. Actualizar `endroomreserve`
2. Actualizar lógica de pausa en `openpaymentmodal`
3. Actualizar `actualizarpromocion`

### Fase 4: Migrar Promociones
1. Actualizar obtención de promociones por categoría

### Fase 5: Testing y Validación
1. Probar cada función migrada
2. Validar manejo de errores
3. Verificar que la UI se actualiza correctamente

## 🚨 Notas Importantes

1. **Backward Compatibility**: Los endpoints legacy seguirán funcionando pero mostrarán warnings de obsoletos
2. **Parámetros**: Los nuevos endpoints usan camelCase en lugar de lowercase
3. **Métodos HTTP**: Los nuevos endpoints siguen convenciones REST apropiadas
4. **Respuestas**: Todas las respuestas están envueltas en `ApiResponse<T>`
5. **Autenticación**: Todos los endpoints V1 requieren autenticación

## 📝 Validaciones Agregadas

Los nuevos endpoints incluyen validaciones mejoradas:
- Validación de modelos con atributos de validación
- Manejo de errores más específico
- Logging estructurado para debugging
- Respuestas consistentes

## 🆘 Soporte

Para dudas sobre la migración:
1. Revisar los logs del servidor para errores específicos
2. Verificar que el token de autenticación sea válido
3. Comprobar que los parámetros coincidan con los nuevos formatos
4. Utilizar los endpoints de health check: `/api/v1/{service}/health`