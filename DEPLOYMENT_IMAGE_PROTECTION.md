# 🛡️ Protección de Imágenes en Deployment

## 📋 Resumen

Este documento describe las mejoras implementadas para **proteger las imágenes de usuarios** durante el proceso de deployment, evitando la pérdida de datos que ocurría anteriormente.

---

## 🚨 Problema Original

**CRÍTICO**: El script `Deploy-IIS.ps1` estaba **eliminando la carpeta `wwwroot/` completa** en cada deploy, causando:

- ❌ Pérdida de **TODAS** las imágenes de artículos
- ❌ Pérdida de **TODAS** las imágenes de categorías
- ❌ Pérdida de **TODAS** las imágenes de habitaciones
- ❌ Pérdida de **TODOS** los iconos de características
- ❌ Referencias huérfanas en base de datos
- ❌ Errores 404 en el frontend

**Carpetas afectadas**:
```
API-Hotel/wwwroot/
└── uploads/                          ← SE ELIMINABA COMPLETA ❌
    ├── *.png, *.jpg                  ← Imágenes de artículos/categorías
    └── caracteristicas/              ← Iconos de características
        └── *.png, *.jpg
```

---

## ✅ Solución Implementada

### **1. Deploy-IIS.ps1 Modificado** ⚡ CRÍTICO

**Archivo**: `/Deploy-IIS.ps1`

**Cambios**:
- ✅ Resueltos conflictos de merge pendientes
- ✅ Agregado `'wwwroot'` a la lista de archivos/carpetas preservados
- ✅ Carpeta `wwwroot/` ahora **NO SE ELIMINA** durante el deploy

**Antes**:
```powershell
$filesToPreserve = @('web.config', 'appsettings.json')
# Resultado: wwwroot/ SE ELIMINABA ❌
```

**Después**:
```powershell
$filesToPreserve = @('web.config', 'appsettings.json', 'wwwroot')
# Resultado: wwwroot/ SE PRESERVA ✅
```

---

### **2. FileStorageExtensions.cs** 🆕

**Archivo**: `/API-Hotel/Extensions/FileStorageExtensions.cs`

**Funcionalidad**:
- 📁 Crea automáticamente carpetas de uploads si no existen
- 🔍 Valida archivos de imagen (extensión, tamaño, content-type)
- 🔑 Genera nombres únicos para archivos (GUID)
- 🗑️ Elimina archivos de forma segura
- 📊 Proporciona métodos utility reutilizables

**Métodos Principales**:
```csharp
// Inicialización automática al arrancar la app
app.EnsureUploadsFoldersExist();

// Obtener rutas
string uploadsPath = FileStorageExtensions.GetUploadsFolderPath(environment);
string caracteristicasPath = FileStorageExtensions.GetCaracteristicasIconsFolderPath(environment);

// Validar imagen
var (isValid, error) = FileStorageExtensions.ValidateImageFile(file, maxSizeInMB: 5);

// Nombre único
string fileName = FileStorageExtensions.GenerateUniqueFileName("foto.jpg");
// Resultado: "a3f5e891-4c2d-4b5a-9a7e-3f512d9c3619.jpg"

// Eliminar archivo seguro
FileStorageExtensions.SafeDeleteFile(filePath, logger);
```

**Carpetas creadas automáticamente**:
- ✅ `wwwroot/uploads/`
- ✅ `wwwroot/uploads/caracteristicas/`

---

### **3. Program.cs Actualizado**

**Archivo**: `/API-Hotel/Program.cs`

**Cambios**:
```csharp
var app = builder.Build();

// 🆕 NUEVO: Asegurar que carpetas de uploads existan al iniciar
app.EnsureUploadsFoldersExist();

app.UseApplicationPipeline();
```

**Resultado**:
- ✅ Carpetas se crean automáticamente si no existen
- ✅ Logs informativos en consola al arrancar
- ✅ No falla si las carpetas ya existen

---

### **4. Pre-Deploy-Backup.ps1** 🆕 (OPCIONAL pero recomendado)

**Archivo**: `/Pre-Deploy-Backup.ps1`

**Funcionalidad**:
- 📦 Crea backup timestamped antes de cada deploy
- 🔄 Rotación automática (mantiene últimos 10 backups)
- 📊 Estadísticas de imágenes y tamaño
- ⚠️ Validaciones y confirmaciones
- 💾 Backups guardados en `C:\HotelBackups\uploads_{timestamp}`

**Uso**:
```powershell
# Crear backup ANTES de ejecutar Deploy-IIS.ps1
.\Pre-Deploy-Backup.ps1 -DestinationPath "C:\inetpub\wwwroot\HotelAPI"

# Con opciones personalizadas
.\Pre-Deploy-Backup.ps1 `
    -DestinationPath "C:\inetpub\wwwroot\HotelAPI" `
    -BackupBasePath "D:\Backups" `
    -KeepLastBackups 20
```

**Recuperación de backup**:
```powershell
# Si se pierden las imágenes, restaurar desde backup
Copy-Item -Path "C:\HotelBackups\uploads_20251120_143022\*" `
          -Destination "C:\inetpub\wwwroot\HotelAPI\wwwroot\uploads\" `
          -Recurse -Force
```

---

## 📖 Instrucciones de Uso

### **Deploy Normal (CON protección)**

```powershell
# Paso 1: Backup (recomendado)
.\Pre-Deploy-Backup.ps1 -DestinationPath "C:\ruta\a\la\app"

# Paso 2: Deploy (ya NO borra wwwroot/)
.\Deploy-IIS.ps1 `
    -ApplicationName "HotelAPI" `
    -SourcePath "C:\ruta\al\build" `
    -DestinationPath "C:\inetpub\wwwroot\HotelAPI"
```

### **Verificar Carpetas al Iniciar la App**

Al arrancar la aplicación, verás en los logs:

```
info: Program[0]
      📁 Carpeta 'uploads' creada en: C:\inetpub\wwwroot\HotelAPI\wwwroot\uploads
info: Program[0]
      📁 Carpeta 'uploads/caracteristicas' creada en: C:\inetpub\wwwroot\HotelAPI\wwwroot\uploads\caracteristicas
info: Program[0]
      ✅ Todas las carpetas de almacenamiento de archivos están disponibles
```

---

## 🔍 Estructura de Carpetas

```
Hotel-Nose/
│
├── Deploy-IIS.ps1                           ← ✅ MODIFICADO (preserva wwwroot)
├── Pre-Deploy-Backup.ps1                    ← 🆕 NUEVO (backup opcional)
│
└── API-Hotel/
    ├── Program.cs                           ← ✅ MODIFICADO (inicializa carpetas)
    │
    ├── Extensions/
    │   └── FileStorageExtensions.cs         ← 🆕 NUEVO (métodos utility)
    │
    └── wwwroot/                             ← 🛡️ PROTEGIDA (no se elimina)
        └── uploads/                         ← Imágenes de usuarios
            ├── *.png, *.jpg                 ← Artículos, categorías, habitaciones
            └── caracteristicas/             ← Iconos de características
                └── *.png, *.jpg
```

---

## 📊 Entidades que Usan Imágenes

### **Tabla `Imagenes` (central)**
- **Articulos** → `Articulos.imagenID` → `Imagenes.imagenID`
- **Categorías** → `CategoriasArticulos.imagenID` → `Imagenes.imagenID`
- **Habitaciones** → `HabitacionImagenes.ImagenID` → `Imagenes.imagenID`

### **Tabla `Caracteristica` (ruta directa)**
- **Características** → `Caracteristica.Icono` (string con ruta completa)

---

## ✅ Beneficios

### **Seguridad de Datos**
- ✅ Imágenes de usuarios **NUNCA se pierden** en deploy
- ✅ Referencias en BD permanecen válidas
- ✅ Backups automáticos opcionales

### **Mantenibilidad**
- ✅ Carpetas se crean automáticamente si faltan
- ✅ Código centralizado en `FileStorageExtensions`
- ✅ Logs informativos para debugging
- ✅ Métodos reutilizables para validación

### **Robustez**
- ✅ Script de deploy sin conflictos de merge
- ✅ Validaciones de archivos consistentes
- ✅ Eliminación segura de archivos
- ✅ Rotación automática de backups

---

## 🧪 Testing

### **Prueba 1: Deploy NO elimina imágenes**
```powershell
# 1. Subir una imagen de prueba
# 2. Ejecutar Deploy-IIS.ps1
# 3. Verificar que la imagen sigue existiendo

Test-Path "C:\inetpub\wwwroot\HotelAPI\wwwroot\uploads\test.jpg"
# Resultado esperado: True ✅
```

### **Prueba 2: Carpetas se crean automáticamente**
```powershell
# 1. Eliminar wwwroot/uploads/
# 2. Iniciar la aplicación
# 3. Verificar logs y carpetas

# Logs esperados:
# 📁 Carpeta 'uploads' creada en: ...
# ✅ Todas las carpetas de almacenamiento...
```

### **Prueba 3: Backup funciona**
```powershell
# 1. Ejecutar Pre-Deploy-Backup.ps1
# 2. Verificar que se creó la carpeta de backup

Get-ChildItem "C:\HotelBackups\" -Filter "uploads_*"
# Resultado esperado: carpeta uploads_20251120_HHMMSS ✅
```

---

## 🚀 Próximos Pasos (Roadmap)

### **Corto Plazo** (Opcional)
- [ ] Actualizar servicios existentes para usar `FileStorageExtensions`
- [ ] Migrar código legacy a métodos centralizados
- [ ] Agregar más validaciones de seguridad

### **Mediano Plazo** (Recomendado)
- [ ] Mover `wwwroot/uploads/` fuera de la carpeta de deployment
  - Ruta propuesta: `C:\HotelData\uploads\`
  - Beneficio: Total separación de código y datos
- [ ] Configurar IIS para servir archivos desde ruta externa
- [ ] Script de migración de imágenes existentes

### **Largo Plazo** (Enterprise)
- [ ] Evaluar Azure Blob Storage / AWS S3
- [ ] Implementar CDN para imágenes
- [ ] Política de retención de imágenes
- [ ] Compresión automática de imágenes

---

## 📞 Soporte

### **Si las imágenes se pierden después de un deploy**

1. **Verificar** que Deploy-IIS.ps1 tiene la versión actualizada:
   ```powershell
   Get-Content Deploy-IIS.ps1 | Select-String "wwwroot"
   # Debe aparecer en $filesToPreserve
   ```

2. **Restaurar desde backup** (si usaste Pre-Deploy-Backup.ps1):
   ```powershell
   # Listar backups disponibles
   Get-ChildItem "C:\HotelBackups\" -Filter "uploads_*" | Sort-Object Name -Descending

   # Restaurar último backup
   $latestBackup = Get-ChildItem "C:\HotelBackups\uploads_*" | Sort-Object Name -Descending | Select-Object -First 1
   Copy-Item -Path "$($latestBackup.FullName)\*" -Destination "C:\ruta\a\wwwroot\uploads\" -Recurse -Force
   ```

3. **Contactar al equipo** si persiste el problema

---

## 📝 Notas Importantes

- ⚠️ **SIEMPRE** ejecutar `Pre-Deploy-Backup.ps1` ANTES de `Deploy-IIS.ps1`
- ⚠️ Verificar que `Deploy-IIS.ps1` tiene `'wwwroot'` en `$filesToPreserve`
- ⚠️ No modificar manualmente las carpetas mientras la app está corriendo
- ⚠️ Los backups ocupan espacio, revisar periódicamente `C:\HotelBackups\`

---

**Fecha de Implementación**: 20 de noviembre de 2025
**Autor**: Claude (Sonnet 4.5)
**Versión**: 1.0
**Estado**: ✅ Producción-Ready
