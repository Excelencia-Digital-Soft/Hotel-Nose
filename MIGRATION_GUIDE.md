# 🚀 Guía de Migración a Producción - Sistema Identity

Esta guía proporciona las instrucciones paso a paso para migrar el sistema de autenticación legacy a ASP.NET Core Identity en el ambiente de producción.

## 📋 Pre-requisitos

### ✅ Checklist Antes de la Migración

- [ ] **Backup completo de la base de datos de producción**
- [ ] **Verificar que el servidor tenga .NET 9.0 runtime**
- [ ] **Confirmar acceso de escritura a la base de datos**
- [ ] **Decidir método de migración: Manual SQL vs Entity Framework**
- [ ] **Notificar a todos los usuarios sobre el cambio de contraseñas**
- [ ] **Programar ventana de mantenimiento (tiempo estimado: 30-60 minutos)**
- [ ] **Tener acceso al servidor de aplicación y base de datos**

### 🗂️ Archivos Necesarios

**Método Entity Framework (Recomendado)**:
1. `Migrations/20250701174952_InitialCreate.cs` - Migración EF para estructura
2. `Scripts/migrate_with_ef.sql` - Script de migración de datos mejorado
3. `Extensions/DatabaseExtensions.cs` - Automatización de inicialización
4. `API-Hotel.exe` - Nueva versión de la aplicación con Identity

**Método Manual (Alternativo)**:
1. `Scripts/create_identity_tables.sql` - Script principal de migración manual
2. `API-Hotel.exe` - Nueva versión de la aplicación con Identity

**Documentación**:
3. `COMPARISON_MANUAL_VS_EF.md` - Comparación de métodos
4. Esta guía de migración

## 🔄 Proceso de Migración

> **📌 IMPORTANTE**: Hay dos métodos disponibles. **Recomendamos el Método Entity Framework** para nuevas instalaciones y el método manual para sistemas existentes que prefieren mantener el enfoque actual.

### Método A: Entity Framework (Recomendado) 🆕

Este método usa migraciones de Entity Framework para crear las tablas y un script SQL mejorado para migrar los datos.

#### Paso A1: Backup de Seguridad

```sql
-- Crear backup completo ANTES de la migración
BACKUP DATABASE [HotelNose] 
TO DISK = 'C:\Backups\HotelNose_PreIdentity_YYYYMMDD.bak'
WITH FORMAT, INIT, 
     NAME = 'HotelNose-Pre-Identity-Migration-Backup',
     DESCRIPTION = 'Backup antes de migración a Identity';
```

#### Paso A2: Verificar Estado Actual

```sql
-- Verificar usuarios existentes
SELECT COUNT(*) as TotalUsuarios FROM Usuarios;
SELECT COUNT(*) as TotalRoles FROM Roles;

-- Verificar si las migraciones EF ya se aplicaron
SELECT name FROM sys.tables WHERE name LIKE 'AspNet%';
```

#### Paso A3: Aplicar Migraciones Entity Framework

```bash
# Navegar al directorio de la aplicación
cd /path/to/API-Hotel

# Aplicar migraciones EF (crea estructura de tablas Identity)
dotnet ef database update

# Verificar que las migraciones se aplicaron
dotnet ef migrations list
```

#### Paso A4: Ejecutar Migración de Datos

```bash
# Ejecutar el script mejorado de migración de datos
sqlcmd -S [SERVIDOR] -d [BASE_DATOS] -i "Scripts/migrate_with_ef.sql" -o "migration_log.txt"
```

#### Paso A5: Verificar Migración Exitosa

```sql
-- El script migrate_with_ef.sql incluye validación automática
-- También puedes verificar manualmente:
SELECT 
    'AspNetUsers' as Tabla,
    COUNT(*) as Registros 
FROM AspNetUsers
WHERE LegacyUserId IS NOT NULL;

-- Ver el reporte de migración
SELECT * FROM vw_MigrationStatus;
```

#### Paso A6: Actualizar y Configurar Aplicación

1. **Actualizar la aplicación** con la nueva versión que incluye `DatabaseExtensions.cs`

2. **Configurar inicialización automática** en `Program.cs`:
   ```csharp
   // Agregar después de app.Build()
   app.EnsureDatabase(applyMigrations: true)
      .SeedDefaultData()
      .LogDatabaseStatus();
   ```

3. **Reiniciar la aplicación** - Verificará automáticamente el estado de la migración

---

### Método B: Manual SQL (Para Sistemas Existentes) 📝

Este es el método original usando scripts SQL manuales.

#### Paso B1: Backup de Seguridad

*Mismo que Paso A1*

#### Paso B2: Verificar Estado Actual

```sql
-- Verificar usuarios existentes
SELECT COUNT(*) as TotalUsuarios FROM Usuarios;
SELECT COUNT(*) as TotalRoles FROM Roles;
SELECT COUNT(*) as TotalRelaciones FROM UsuariosInstituciones;

-- Verificar si las tablas Identity ya existen
SELECT name FROM sys.tables WHERE name LIKE 'AspNet%';
```

### Paso 3: Ejecutar Script de Migración

```bash
# Conectarse a SQL Server y ejecutar el script
sqlcmd -S [SERVIDOR] -d [BASE_DATOS] -i "Scripts/create_identity_tables.sql" -o "migration_log.txt"
```

**⚠️ IMPORTANTE**: Revisar el archivo `migration_log.txt` para verificar que no hay errores.

### Paso 4: Verificar Migración Exitosa

```sql
-- Verificar que las tablas se crearon correctamente
SELECT 
    'AspNetUsers' as Tabla,
    COUNT(*) as Registros 
FROM AspNetUsers
WHERE LegacyUserId IS NOT NULL

UNION ALL

SELECT 
    'AspNetRoles' as Tabla,
    COUNT(*) as Registros 
FROM AspNetRoles

UNION ALL

SELECT 
    'AspNetUserRoles' as Tabla,
    COUNT(*) as Registros 
FROM AspNetUserRoles;

-- Verificar mapeo de usuarios
SELECT 
    u.NombreUsuario as UsuarioLegacy,
    au.UserName as UsuarioIdentity,
    au.Email,
    au.ForcePasswordChange,
    r.Name as Rol
FROM Usuarios u
INNER JOIN AspNetUsers au ON u.UsuarioId = au.LegacyUserId
INNER JOIN AspNetUserRoles aur ON au.Id = aur.UserId
INNER JOIN AspNetRoles r ON aur.RoleId = r.Id
ORDER BY u.NombreUsuario;
```

### Paso 5: Actualizar Aplicación

1. **Detener la aplicación actual**:
   ```bash
   # Detener servicio/aplicación
   net stop "HotelNoseAPI"
   ```

2. **Reemplazar archivos de aplicación**:
   ```bash
   # Backup de la versión anterior
   move "C:\inetpub\HotelNose" "C:\inetpub\HotelNose_backup"
   
   # Copiar nueva versión
   xcopy "nueva_version\*" "C:\inetpub\HotelNose\" /E /I
   ```

3. **Actualizar cadena de conexión** en `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=SERVIDOR;Database=BASE_DATOS;Integrated Security=true;TrustServerCertificate=true;"
     }
   }
   ```

4. **Reiniciar la aplicación**:
   ```bash
   net start "HotelNoseAPI"
   ```

### Paso 6: Pruebas de Validación

#### 🧪 Pruebas Críticas

1. **Test de Login con Usuario Existente**:
   - URL: `POST /api/auth/login`
   - Usuario: `[usuario_existente]`
   - Password: `Pass123`
   - ✅ **Esperado**: Login exitoso + forzar cambio de contraseña

2. **Test de Cambio de Contraseña**:
   - URL: `POST /api/auth/change-password`
   - Password actual: `Pass123`
   - Password nueva: `[nueva_contraseña]`
   - ✅ **Esperado**: Cambio exitoso

3. **Test de Login con Nueva Contraseña**:
   - URL: `POST /api/auth/login`
   - Usuario: `[usuario_existente]`
   - Password: `[nueva_contraseña]`
   - ✅ **Esperado**: Login exitoso sin forzar cambio

4. **Test de Autorización por Roles**:
   - ✅ **Esperado**: Los roles y permisos funcionan igual que antes

#### 📝 Script de Validación Automática

```sql
-- Verificar integridad de datos migrados
DECLARE @UsuariosLegacy INT, @UsuariosIdentity INT, @Diferencia INT;

SELECT @UsuariosLegacy = COUNT(*) FROM Usuarios;
SELECT @UsuariosIdentity = COUNT(*) FROM AspNetUsers WHERE LegacyUserId IS NOT NULL;
SET @Diferencia = @UsuariosLegacy - @UsuariosIdentity;

PRINT '=== REPORTE DE VALIDACIÓN ===';
PRINT 'Usuarios Legacy: ' + CAST(@UsuariosLegacy AS NVARCHAR(10));
PRINT 'Usuarios Identity: ' + CAST(@UsuariosIdentity AS NVARCHAR(10));
PRINT 'Diferencia: ' + CAST(@Diferencia AS NVARCHAR(10));

IF @Diferencia = 0
    PRINT '✅ MIGRACIÓN EXITOSA: Todos los usuarios fueron migrados';
ELSE
    PRINT '❌ PROBLEMA: ' + CAST(@Diferencia AS NVARCHAR(10)) + ' usuarios no fueron migrados';

-- Verificar usuarios sin institución
SELECT COUNT(*) as UsuariosSinInstitucion
FROM AspNetUsers 
WHERE LegacyUserId IS NOT NULL AND InstitucionId IS NULL;
```

## 🔐 Información de Contraseñas

### Contraseña Predeterminada

- **Todos los usuarios migrados tendrán la contraseña**: `Pass123`
- **Cambio forzado**: Sí, todos deben cambiar su contraseña en el primer login
- **Formato de email**: Si el usuario no tiene '@', se agrega '@hotel.fake'

### Comunicación a Usuarios

**Mensaje sugerido para enviar a todos los usuarios**:

---
📧 **Asunto**: Actualización del Sistema - Nueva Contraseña Requerida

Estimado/a usuario/a,

Hemos actualizado nuestro sistema de seguridad. A partir de [FECHA], deberás:

1. **Usar tu usuario habitual** para ingresar al sistema
2. **Contraseña temporal**: `Pass123`
3. **Cambio obligatorio**: El sistema te pedirá crear una nueva contraseña
4. **Nueva contraseña debe tener**: Mínimo 8 caracteres, al menos 1 mayúscula, 1 minúscula y 1 número

Si tienes problemas para acceder, contacta a soporte técnico.

Gracias por tu comprensión.

**Equipo de TI**

---

## 🚨 Plan de Rollback

### Si la Migración Falla

1. **Restaurar base de datos**:
   ```sql
   USE master;
   ALTER DATABASE [HotelNose] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
   RESTORE DATABASE [HotelNose] FROM DISK = 'C:\Backups\HotelNose_PreIdentity_YYYYMMDD.bak' WITH REPLACE;
   ALTER DATABASE [HotelNose] SET MULTI_USER;
   ```

2. **Restaurar aplicación anterior**:
   ```bash
   net stop "HotelNoseAPI"
   rmdir "C:\inetpub\HotelNose" /S /Q
   move "C:\inetpub\HotelNose_backup" "C:\inetpub\HotelNose"
   net start "HotelNoseAPI"
   ```

### Si la Aplicación No Inicia

1. **Verificar logs** en `logs/` folder
2. **Verificar cadena de conexión**
3. **Verificar que .NET 9.0 runtime esté instalado**
4. **Verificar permisos de archivos**

## 📞 Contactos de Soporte

- **Desarrollador Principal**: [Nombre] - [Email/Teléfono]
- **Administrador de Base de Datos**: [Nombre] - [Email/Teléfono]
- **Administrador de Sistemas**: [Nombre] - [Email/Teléfono]

## 📅 Timeline Sugerido

| Hora | Actividad | Duración | Responsable |
|------|-----------|----------|-------------|
| 00:00 | Backup de BD | 10 min | DBA |
| 00:10 | Detener aplicación | 2 min | SysAdmin |
| 00:12 | Ejecutar migración SQL | 15 min | DBA |
| 00:27 | Verificar migración | 10 min | DBA |
| 00:37 | Actualizar aplicación | 10 min | SysAdmin |
| 00:47 | Pruebas de validación | 15 min | Developer |
| 01:02 | Comunicar a usuarios | 5 min | Team Lead |

**Total estimado**: 67 minutos

## ✅ Checklist Post-Migración

- [ ] **Todos los usuarios legacy fueron migrados**
- [ ] **Aplicación inicia correctamente**
- [ ] **Login funciona con Pass123**
- [ ] **Cambio de contraseña funciona**
- [ ] **Roles y permisos funcionan**
- [ ] **Notificación enviada a usuarios**
- [ ] **Backup de pre-migración guardado**
- [ ] **Logs de migración revisados**
- [ ] **Monitoreo del sistema activado**

---

## 🔧 Comandos de Utilidad

### Verificar Estado de la Migración

```sql
-- Ejecutar después de la migración para verificar estado
EXEC sp_executesql N'
SELECT 
    ''Legacy Users'' as Tipo, COUNT(*) as Cantidad FROM Usuarios
UNION ALL
SELECT 
    ''Identity Users'' as Tipo, COUNT(*) as Cantidad FROM AspNetUsers
UNION ALL  
SELECT 
    ''Migrated Users'' as Tipo, COUNT(*) as Cantidad FROM AspNetUsers WHERE LegacyUserId IS NOT NULL
UNION ALL
SELECT 
    ''Users Need Password Change'' as Tipo, COUNT(*) as Cantidad FROM AspNetUsers WHERE ForcePasswordChange = 1
';
```

### Reset de Usuario (Si es necesario)

```sql
-- En caso de que un usuario tenga problemas específicos
UPDATE AspNetUsers 
SET 
    PasswordHash = 'AQAAAAEAACcQAAAAEKcO/+btL3p8+DxXFz7CjAqF/T5gK3QMF7pO1TLQ8sHx/R7nN4vF2Q1Y9gH3K8Wm',
    ForcePasswordChange = 1,
    SecurityStamp = CONVERT(NVARCHAR(36), NEWID()),
    AccessFailedCount = 0,
    LockoutEnd = NULL
WHERE UserName = '[USERNAME]';
```

---

**⚠️ IMPORTANTE: Mantener esta guía actualizada con cualquier cambio en el proceso de migración.**