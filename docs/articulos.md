  📖 Documentación API V1 - Artículos

  🔗 Base URL

  /api/v1/articulos

  🔐 Autenticación

  Todos los endpoints requieren Bearer Token JWT (excepto /health e imágenes públicas)

  ---
  📋 Endpoints

  1. Obtener Todos los Artículos

  GET /?categoriaId=1
  Devuelve: Lista de artículos con información completa
  {
    "isSuccess": true,
    "data": [
      {
        "articuloId": 1,
        "nombreArticulo": "Coca Cola",
        "precio": 2.50,
        "categoriaId": 1,
        "categoriaNombre": "Bebidas",
        "imagenId": 10,
        "imagenUrl": "/api/v1/articulos/1/image",
        "anulado": false,
        "fechaRegistro": "2024-07-15T10:00:00",
        "fechaModificacion": null,
        "creadoPorId": "user123",
        "creadoPorNombre": "Juan Pérez",
        "modificadoPorId": null,
        "modificadoPorNombre": null
      }
    ]
  }

  2. Obtener Artículo por ID

  GET /1
  Devuelve: Artículo específico con toda la información

  3. Crear Artículo

  POST /
  Content-Type: application/json

  {
    "nombreArticulo": "Pepsi",
    "precio": 2.30,
    "categoriaId": 1
  }
  Devuelve: Artículo creado con status 201

  4. Crear Artículo con Imagen

  POST /with-image
  Content-Type: multipart/form-data

  nombreArticulo: "Pizza Margherita"
  precio: 15.50
  categoriaId: 2
  imagen: [archivo]
  Devuelve: Artículo creado con imagen y status 201

  5. Actualizar Artículo

  PUT /1
  Content-Type: application/json

  {
    "nombreArticulo": "Coca Cola Zero",
    "precio": 2.60
  }
  Devuelve: Artículo actualizado

  6. Actualizar Solo Imagen

  PATCH /1/image
  Content-Type: multipart/form-data

  imagen: [archivo]
  Devuelve: Artículo con imagen actualizada

  7. Eliminar Artículo 🔒 Admin/Director

  DELETE /1
  Devuelve: Confirmación de eliminación (solo si no tiene consumos)

  8. Cambiar Estado (Anular/Activar) 🔒 Admin/Director/Manager

  PATCH /1/status
  Content-Type: application/json

  {
    "anulado": true
  }
  Devuelve: Confirmación del cambio de estado

  9. Obtener Imagen del Artículo

  GET /1/image
  Devuelve: Archivo de imagen (público, sin autenticación)

  10. Health Check

  GET /health
  Devuelve: Estado del servicio

  ---
  🔒 Roles de Autorización

  | Endpoint                | Roles Requeridos                 |
  |-------------------------|----------------------------------|
  | GET endpoints           | Cualquier usuario autenticado    |
  | POST, PUT, PATCH /image | Cualquier usuario autenticado    |
  | DELETE                  | Administrator, Director          |
  | PATCH /status           | Administrator, Director, Manager |
  | GET /health, /image     | Público                          |

  📝 Validaciones

  - Nombre: Requerido, máximo 200 caracteres
  - Precio: Requerido, mayor a 0
  - Categoría: Debe existir en la institución
  - Imagen: Formatos JPG, PNG, GIF, BMP - Máximo 5MB

  🚨 Códigos de Respuesta

  | Código | Descripción                                             |
  |--------|---------------------------------------------------------|
  | 200    | Operación exitosa                                       |
  | 201    | Creado exitosamente                                     |
  | 400    | Error de validación                                     |
  | 401    | No autenticado                                          |
  | 403    | Sin permisos                                            |
  | 404    | No encontrado                                           |
  | 409    | Conflicto (ej: no se puede eliminar por tener consumos) |
  | 500    | Error interno                                           |
