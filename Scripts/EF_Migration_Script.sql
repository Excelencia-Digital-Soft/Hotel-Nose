IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Description] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Caracteristicas] (
    [CaracteristicaId] int NOT NULL IDENTITY,
    [Nombre] nvarchar(100) NOT NULL,
    [Descripcion] nvarchar(500) NULL,
    [Icono] nvarchar(255) NULL,
    CONSTRAINT [PK_Caracteristicas] PRIMARY KEY ([CaracteristicaId])
);
GO

CREATE TABLE [CategoriasArticulos] (
    [CategoriaId] int NOT NULL IDENTITY,
    [NombreCategoria] nvarchar(max) NULL,
    [Anulado] bit NULL,
    [InstitucionID] int NOT NULL,
    CONSTRAINT [PK_CategoriasArticulos] PRIMARY KEY ([CategoriaId])
);
GO

CREATE TABLE [CategoriasHabitaciones] (
    [CategoriaId] int NOT NULL IDENTITY,
    [NombreCategoria] nvarchar(max) NULL,
    [CapacidadMaxima] int NULL,
    [UsuarioId] int NULL,
    [FechaRegistro] datetime2 NULL,
    [Anulado] bit NULL,
    [PrecioNormal] decimal(18,2) NULL,
    [InstitucionID] int NOT NULL,
    [PorcentajeXPersona] int NOT NULL,
    CONSTRAINT [PK_CategoriasHabitaciones] PRIMARY KEY ([CategoriaId])
);
GO

CREATE TABLE [DescuentoEfectivo] (
    [DescuentoID] int NOT NULL IDENTITY,
    [MontoPorcentual] int NOT NULL,
    [InstitucionID] int NOT NULL,
    CONSTRAINT [PK_DescuentoEfectivo] PRIMARY KEY ([DescuentoID])
);
GO

CREATE TABLE [DiasSemana] (
    [DiaSemanaId] int NOT NULL IDENTITY,
    [NombreDiaSemana] nvarchar(max) NULL,
    CONSTRAINT [PK_DiasSemana] PRIMARY KEY ([DiaSemanaId])
);
GO

CREATE TABLE [Imagenes] (
    [imagenID] int NOT NULL IDENTITY,
    [NombreArchivo] nvarchar(255) NOT NULL,
    [FechaSubida] datetime2 NOT NULL,
    [InstitucionID] int NOT NULL,
    CONSTRAINT [PK_Imagenes] PRIMARY KEY ([imagenID])
);
GO

CREATE TABLE [Instituciones] (
    [InstitucionId] int NOT NULL IDENTITY,
    [Nombre] nvarchar(max) NOT NULL,
    [Direccion] nvarchar(max) NULL,
    [Telefono] nvarchar(max) NULL,
    [Email] nvarchar(max) NULL,
    [Estado] nvarchar(max) NULL,
    [FechaCreacion] datetime2 NULL,
    [Descripcion] nvarchar(max) NULL,
    [TipoID] int NULL,
    [FechaAnulado] datetime2 NULL,
    CONSTRAINT [PK_Instituciones] PRIMARY KEY ([InstitucionId])
);
GO

CREATE TABLE [MediosPago] (
    [MedioPagoId] int NOT NULL IDENTITY,
    [Nombre] nvarchar(max) NULL,
    [Descripcion] nvarchar(max) NULL,
    CONSTRAINT [PK_MediosPago] PRIMARY KEY ([MedioPagoId])
);
GO

CREATE TABLE [Roles] (
    [RolId] int NOT NULL IDENTITY,
    [NombreRol] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([RolId])
);
GO

CREATE TABLE [Servicios] (
    [ServicioId] int NOT NULL IDENTITY,
    [NombreServicio] nvarchar(max) NOT NULL,
    [Descripcion] nvarchar(max) NULL,
    [Precio] decimal(18,2) NOT NULL,
    [Activo] bit NOT NULL,
    [Categoria] nvarchar(max) NULL,
    [FechaCreacion] datetime2 NOT NULL,
    CONSTRAINT [PK_Servicios] PRIMARY KEY ([ServicioId])
);
GO

CREATE TABLE [ServiciosAdicionales] (
    [ServicioId] int NOT NULL IDENTITY,
    [NombreServicio] nvarchar(max) NULL,
    [Precio] decimal(18,2) NULL,
    [UsuarioId] int NULL,
    [FechaRegistro] datetime2 NULL,
    [Anulado] bit NULL,
    CONSTRAINT [PK_ServiciosAdicionales] PRIMARY KEY ([ServicioId])
);
GO

CREATE TABLE [Tarjetas] (
    [TarjetaID] int NOT NULL IDENTITY,
    [Nombre] nvarchar(max) NOT NULL,
    [MontoPorcentual] int NOT NULL,
    [InstitucionID] int NOT NULL,
    CONSTRAINT [PK_Tarjetas] PRIMARY KEY ([TarjetaID])
);
GO

CREATE TABLE [TipoEgreso] (
    [TipoEgresoId] int NOT NULL IDENTITY,
    [Nombre] nvarchar(max) NOT NULL,
    [InstitucionID] int NOT NULL,
    CONSTRAINT [PK_TipoEgreso] PRIMARY KEY ([TipoEgresoId])
);
GO

CREATE TABLE [TipoMovimiento] (
    [TipoMovimientoId] int NOT NULL IDENTITY,
    [NombreTipoMovimiento] nvarchar(max) NULL,
    [Tipo] nvarchar(max) NULL,
    [Anulado] bit NULL,
    CONSTRAINT [PK_TipoMovimiento] PRIMARY KEY ([TipoMovimientoId])
);
GO

CREATE TABLE [TipoTarifa] (
    [TipoTarifaId] int NOT NULL IDENTITY,
    [NombreTipoTarifa] nvarchar(max) NULL,
    [UsuarioId] int NULL,
    [FechaRegistro] datetime2 NULL,
    [Anulado] bit NULL,
    CONSTRAINT [PK_TipoTarifa] PRIMARY KEY ([TipoTarifaId])
);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Promociones] (
    [PromocionID] int NOT NULL IDENTITY,
    [Tarifa] decimal(18,2) NOT NULL,
    [CantidadHoras] int NOT NULL,
    [CategoriaID] int NOT NULL,
    [Anulado] bit NULL,
    [Detalle] nvarchar(max) NOT NULL,
    [InstitucionID] int NOT NULL,
    CONSTRAINT [PK_Promociones] PRIMARY KEY ([PromocionID]),
    CONSTRAINT [FK_Promociones_CategoriasHabitaciones_CategoriaID] FOREIGN KEY ([CategoriaID]) REFERENCES [CategoriasHabitaciones] ([CategoriaId]) ON DELETE CASCADE
);
GO

CREATE TABLE [Articulos] (
    [ArticuloId] int NOT NULL IDENTITY,
    [NombreArticulo] nvarchar(max) NULL,
    [Precio] decimal(18,2) NOT NULL,
    [UsuarioId] int NULL,
    [FechaRegistro] datetime2 NULL,
    [Anulado] bit NULL,
    [imagenID] int NOT NULL,
    [CategoriaID] int NULL,
    [InstitucionID] int NOT NULL,
    CONSTRAINT [PK_Articulos] PRIMARY KEY ([ArticuloId]),
    CONSTRAINT [FK_Articulos_Imagenes_imagenID] FOREIGN KEY ([imagenID]) REFERENCES [Imagenes] ([imagenID])
);
GO

CREATE TABLE [Acompanantes] (
    [AcompananteId] int NOT NULL IDENTITY,
    [Nombres] nvarchar(max) NOT NULL,
    [DocumentoIdentidad] nvarchar(max) NULL,
    [Telefono] nvarchar(max) NULL,
    [Parentesco] nvarchar(max) NULL,
    [FechaRegistro] datetime2 NOT NULL,
    [InstitucionId] int NULL,
    [InstitucionId1] int NULL,
    CONSTRAINT [PK_Acompanantes] PRIMARY KEY ([AcompananteId]),
    CONSTRAINT [FK_Acompanantes_Instituciones_InstitucionId] FOREIGN KEY ([InstitucionId]) REFERENCES [Instituciones] ([InstitucionId]),
    CONSTRAINT [FK_Acompanantes_Instituciones_InstitucionId1] FOREIGN KEY ([InstitucionId1]) REFERENCES [Instituciones] ([InstitucionId])
);
GO

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [FirstName] nvarchar(max) NULL,
    [LastName] nvarchar(max) NULL,
    [InstitucionId] int NULL,
    [LegacyUserId] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [LastLoginAt] datetime2 NULL,
    [IsActive] bit NOT NULL,
    [ForcePasswordChange] bit NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUsers_Instituciones_InstitucionId] FOREIGN KEY ([InstitucionId]) REFERENCES [Instituciones] ([InstitucionId])
);
GO

CREATE TABLE [Configuracion] (
    [ConfiguracionId] int NOT NULL IDENTITY,
    [Clave] varchar(100) NOT NULL,
    [Valor] nvarchar(500) NOT NULL,
    [Descripcion] nvarchar(255) NULL,
    [Categoria] varchar(50) NULL,
    [FechaCreacion] datetime2 NOT NULL DEFAULT (GETDATE()),
    [FechaModificacion] datetime2 NULL,
    [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
    [InstitucionId] int NULL,
    CONSTRAINT [PK_Configuracion] PRIMARY KEY ([ConfiguracionId]),
    CONSTRAINT [FK_Configuracion_Institucion] FOREIGN KEY ([InstitucionId]) REFERENCES [Instituciones] ([InstitucionId]) ON DELETE SET NULL
);
GO

CREATE TABLE [Personal] (
    [PersonalId] int NOT NULL IDENTITY,
    [NombreCompleto] nvarchar(max) NOT NULL,
    [RolId] int NOT NULL,
    CONSTRAINT [PK_Personal] PRIMARY KEY ([PersonalId]),
    CONSTRAINT [FK_Personal_Roles_RolId] FOREIGN KEY ([RolId]) REFERENCES [Roles] ([RolId]) ON DELETE CASCADE
);
GO

CREATE TABLE [Usuarios] (
    [UsuarioId] int NOT NULL IDENTITY,
    [NombreUsuario] nvarchar(max) NOT NULL,
    [Contraseña] nvarchar(max) NOT NULL,
    [RolId] int NOT NULL,
    CONSTRAINT [PK_Usuarios] PRIMARY KEY ([UsuarioId]),
    CONSTRAINT [FK_Usuarios_Roles_RolId] FOREIGN KEY ([RolId]) REFERENCES [Roles] ([RolId]) ON DELETE CASCADE
);
GO

CREATE TABLE [Tarifas] (
    [TarifaId] int NOT NULL IDENTITY,
    [DetalleTarifa] nvarchar(max) NULL,
    [CategoriaId] int NULL,
    [DiaSemanaId] int NULL,
    [HoraInicio] time NULL,
    [HoraFin] time NULL,
    [TipoTarifaId] int NULL,
    [Precio] decimal(18,2) NULL,
    [UsuarioId] int NULL,
    [FechaRegistro] datetime2 NULL,
    [Anulado] bit NULL,
    CONSTRAINT [PK_Tarifas] PRIMARY KEY ([TarifaId]),
    CONSTRAINT [FK_Tarifas_CategoriasHabitaciones_CategoriaId] FOREIGN KEY ([CategoriaId]) REFERENCES [CategoriasHabitaciones] ([CategoriaId]),
    CONSTRAINT [FK_Tarifas_DiasSemana_DiaSemanaId] FOREIGN KEY ([DiaSemanaId]) REFERENCES [DiasSemana] ([DiaSemanaId]),
    CONSTRAINT [FK_Tarifas_TipoTarifa_TipoTarifaId] FOREIGN KEY ([TipoTarifaId]) REFERENCES [TipoTarifa] ([TipoTarifaId])
);
GO

CREATE TABLE [InventarioGeneral] (
    [InventarioId] int NOT NULL IDENTITY,
    [ArticuloId] int NULL,
    [Cantidad] int NULL,
    [FechaRegistro] datetime2 NULL,
    [Anulado] bit NULL,
    [InstitucionID] int NOT NULL,
    CONSTRAINT [PK_InventarioGeneral] PRIMARY KEY ([InventarioId]),
    CONSTRAINT [FK_InventarioGeneral_Articulos_ArticuloId] FOREIGN KEY ([ArticuloId]) REFERENCES [Articulos] ([ArticuloId])
);
GO

CREATE TABLE [InventarioInicial] (
    [ArticuloId] int NOT NULL,
    [CantidadInicial] int NULL,
    [FechaRegistro] datetime2 NULL,
    [Anulado] bit NULL,
    [InstitucionId] int NULL,
    CONSTRAINT [PK_InventarioInicial] PRIMARY KEY ([ArticuloId]),
    CONSTRAINT [FK_InventarioInicial_Articulos_ArticuloId] FOREIGN KEY ([ArticuloId]) REFERENCES [Articulos] ([ArticuloId]),
    CONSTRAINT [FK_InventarioInicial_Instituciones_InstitucionId] FOREIGN KEY ([InstitucionId]) REFERENCES [Instituciones] ([InstitucionId])
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Cierre] (
    [CierreId] int NOT NULL IDENTITY,
    [UsuarioId] int NULL,
    [FechaHoraCierre] datetime2 NULL,
    [TotalIngresosEfectivo] decimal(18,2) NULL,
    [TotalIngresosBillVirt] decimal(18,2) NULL,
    [TotalIngresosTarjeta] decimal(18,2) NULL,
    [Observaciones] nvarchar(max) NULL,
    [EstadoCierre] bit NULL,
    [MontoInicialCaja] decimal(18,2) NULL,
    [InstitucionID] int NOT NULL,
    CONSTRAINT [PK_Cierre] PRIMARY KEY ([CierreId]),
    CONSTRAINT [FK_Cierre_Instituciones_InstitucionID] FOREIGN KEY ([InstitucionID]) REFERENCES [Instituciones] ([InstitucionId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Cierre_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([UsuarioId])
);
GO

CREATE TABLE [MovimientosUsuarios] (
    [MovimientoId] int NOT NULL,
    [UsuarioId] int NOT NULL,
    [FechaHora] datetime2 NOT NULL,
    [Accion] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_MovimientosUsuarios] PRIMARY KEY ([MovimientoId], [UsuarioId]),
    CONSTRAINT [FK_MovimientosUsuarios_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([UsuarioId]) ON DELETE CASCADE
);
GO

CREATE TABLE [UsuariosInstituciones] (
    [UsuarioId] int NOT NULL,
    [InstitucionID] int NOT NULL,
    [ApplicationUserId] nvarchar(450) NULL,
    CONSTRAINT [PK_UsuariosInstituciones] PRIMARY KEY ([UsuarioId], [InstitucionID]),
    CONSTRAINT [FK_UsuariosInstituciones_AspNetUsers_ApplicationUserId] FOREIGN KEY ([ApplicationUserId]) REFERENCES [AspNetUsers] ([Id]),
    CONSTRAINT [FK_UsuariosInstituciones_Instituciones_InstitucionID] FOREIGN KEY ([InstitucionID]) REFERENCES [Instituciones] ([InstitucionId]) ON DELETE CASCADE,
    CONSTRAINT [FK_UsuariosInstituciones_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([UsuarioId]) ON DELETE CASCADE
);
GO

CREATE TABLE [CuadreCierre] (
    [CuadreCierreId] int NOT NULL IDENTITY,
    [CierreId] int NULL,
    [TipoMovimiento] nvarchar(max) NULL,
    [Descripcion] nvarchar(max) NULL,
    [Monto] decimal(18,2) NOT NULL,
    [InstitucionId] int NULL,
    CONSTRAINT [PK_CuadreCierre] PRIMARY KEY ([CuadreCierreId]),
    CONSTRAINT [FK_CuadreCierre_Cierre_CierreId] FOREIGN KEY ([CierreId]) REFERENCES [Cierre] ([CierreId]),
    CONSTRAINT [FK_CuadreCierre_Instituciones_InstitucionId] FOREIGN KEY ([InstitucionId]) REFERENCES [Instituciones] ([InstitucionId])
);
GO

CREATE TABLE [Pagos] (
    [PagoId] int NOT NULL IDENTITY,
    [MontoEfectivo] decimal(18,2) NULL,
    [MontoBillVirt] decimal(18,2) NULL,
    [MontoTarjeta] decimal(18,2) NULL,
    [Adicional] decimal(18,2) NULL,
    [MontoDescuento] decimal(18,2) NULL,
    [MedioPagoId] int NULL,
    [CierreId] int NULL,
    [TarjetaId] int NULL,
    [fechaHora] datetime2 NULL,
    [Observacion] nvarchar(max) NULL,
    [InstitucionID] int NOT NULL,
    CONSTRAINT [PK_Pagos] PRIMARY KEY ([PagoId]),
    CONSTRAINT [FK_Pagos_Cierre_CierreId] FOREIGN KEY ([CierreId]) REFERENCES [Cierre] ([CierreId]),
    CONSTRAINT [FK_Pagos_Instituciones_InstitucionID] FOREIGN KEY ([InstitucionID]) REFERENCES [Instituciones] ([InstitucionId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Pagos_MediosPago_MedioPagoId] FOREIGN KEY ([MedioPagoId]) REFERENCES [MediosPago] ([MedioPagoId])
);
GO

CREATE TABLE [Recargos] (
    [RecargoID] int NOT NULL IDENTITY,
    [Descripcion] nvarchar(max) NULL,
    [Valor] decimal(18,2) NULL,
    [PagoID] int NOT NULL,
    CONSTRAINT [PK_Recargos] PRIMARY KEY ([RecargoID]),
    CONSTRAINT [FK_Recargos_Pagos_PagoID] FOREIGN KEY ([PagoID]) REFERENCES [Pagos] ([PagoId])
);
GO

CREATE TABLE [Consumo] (
    [ConsumoId] int NOT NULL IDENTITY,
    [MovimientosId] int NULL,
    [ArticuloId] int NULL,
    [Cantidad] int NULL,
    [PrecioUnitario] decimal(18,2) NULL,
    [EsHabitacion] bit NULL,
    [Anulado] bit NULL,
    CONSTRAINT [PK_Consumo] PRIMARY KEY ([ConsumoId]),
    CONSTRAINT [FK_Consumo_Articulos_ArticuloId] FOREIGN KEY ([ArticuloId]) REFERENCES [Articulos] ([ArticuloId])
);
GO

CREATE TABLE [Egresos] (
    [EgresoId] int NOT NULL IDENTITY,
    [TipoEgresoId] int NULL,
    [Cantidad] int NOT NULL,
    [Precio] decimal(18,2) NOT NULL,
    [Fecha] datetime2 NULL,
    [MovimientoId] int NULL,
    [InstitucionID] int NOT NULL,
    [CierreID] int NULL,
    CONSTRAINT [PK_Egresos] PRIMARY KEY ([EgresoId]),
    CONSTRAINT [FK_Egresos_Cierre_CierreID] FOREIGN KEY ([CierreID]) REFERENCES [Cierre] ([CierreId]),
    CONSTRAINT [FK_Egresos_TipoEgreso_TipoEgresoId] FOREIGN KEY ([TipoEgresoId]) REFERENCES [TipoEgreso] ([TipoEgresoId])
);
GO

CREATE TABLE [Empeño] (
    [EmpeñoID] int NOT NULL IDENTITY,
    [VisitaID] int NOT NULL,
    [Detalle] nvarchar(max) NULL,
    [Monto] float NOT NULL,
    [PagoID] int NULL,
    [FechaRegistro] datetime2 NULL,
    [Anulado] bit NULL,
    [InstitucionID] int NOT NULL,
    CONSTRAINT [PK_Empeño] PRIMARY KEY ([EmpeñoID]),
    CONSTRAINT [FK_Empeño_Pagos_PagoID] FOREIGN KEY ([PagoID]) REFERENCES [Pagos] ([PagoId])
);
GO

CREATE TABLE [Encargos] (
    [EncargosId] int NOT NULL IDENTITY,
    [ArticuloId] int NULL,
    [VisitaId] int NULL,
    [CantidadArt] int NULL,
    [Comentario] nvarchar(max) NULL,
    [Entregado] bit NULL,
    [Anulado] bit NULL,
    [FechaCrea] datetime2 NULL,
    [InstitucionId] int NULL,
    CONSTRAINT [PK_Encargos] PRIMARY KEY ([EncargosId]),
    CONSTRAINT [FK_Encargos_Articulos_ArticuloId] FOREIGN KEY ([ArticuloId]) REFERENCES [Articulos] ([ArticuloId]),
    CONSTRAINT [FK_Encargos_Instituciones_InstitucionId] FOREIGN KEY ([InstitucionId]) REFERENCES [Instituciones] ([InstitucionId])
);
GO

CREATE TABLE [HabitacionCaracteristicas] (
    [HabitacionId] int NOT NULL,
    [CaracteristicaId] int NOT NULL,
    CONSTRAINT [PK_HabitacionCaracteristicas] PRIMARY KEY ([HabitacionId], [CaracteristicaId]),
    CONSTRAINT [FK_HabitacionCaracteristicas_Caracteristicas] FOREIGN KEY ([CaracteristicaId]) REFERENCES [Caracteristicas] ([CaracteristicaId]) ON DELETE CASCADE
);
GO

CREATE TABLE [Habitaciones] (
    [HabitacionId] int NOT NULL IDENTITY,
    [NombreHabitacion] nvarchar(100) NULL,
    [CategoriaId] int NULL,
    [Disponible] bit NULL DEFAULT CAST(1 AS bit),
    [ProximaReserva] datetime2 NULL,
    [UsuarioId] int NULL,
    [FechaRegistro] datetime2 NULL DEFAULT (GETDATE()),
    [Anulado] bit NULL DEFAULT CAST(0 AS bit),
    [VisitaID] int NULL,
    [InstitucionID] int NOT NULL,
    CONSTRAINT [PK_Habitaciones] PRIMARY KEY ([HabitacionId]),
    CONSTRAINT [FK_Habitaciones_CategoriasHabitaciones] FOREIGN KEY ([CategoriaId]) REFERENCES [CategoriasHabitaciones] ([CategoriaId]),
    CONSTRAINT [FK_Habitaciones_Instituciones_InstitucionID] FOREIGN KEY ([InstitucionID]) REFERENCES [Instituciones] ([InstitucionId]) ON DELETE CASCADE
);
GO

CREATE TABLE [HabitacionesVirtuales] (
    [HabitacionVirtualId] int NOT NULL IDENTITY,
    [Habitacion1Id] int NULL,
    [Habitacion2Id] int NULL,
    [Precio] decimal(18,2) NULL,
    [FechaInicio] datetime2 NULL,
    [FechaFin] datetime2 NULL,
    [UsuarioId] int NULL,
    [FechaRegistro] datetime2 NULL,
    [Anulado] bit NULL,
    CONSTRAINT [PK_HabitacionesVirtuales] PRIMARY KEY ([HabitacionVirtualId]),
    CONSTRAINT [FK_HabitacionesVirtuales_Habitaciones_Habitacion1Id] FOREIGN KEY ([Habitacion1Id]) REFERENCES [Habitaciones] ([HabitacionId]),
    CONSTRAINT [FK_HabitacionesVirtuales_Habitaciones_Habitacion2Id] FOREIGN KEY ([Habitacion2Id]) REFERENCES [Habitaciones] ([HabitacionId])
);
GO

CREATE TABLE [HabitacionImagenes] (
    [Id] int NOT NULL IDENTITY,
    [HabitacionID] int NOT NULL,
    [ImagenID] int NOT NULL,
    [Anulado] bit NULL DEFAULT CAST(0 AS bit),
    [EsPrincipal] bit NOT NULL DEFAULT CAST(0 AS bit),
    [Orden] int NOT NULL DEFAULT 0,
    CONSTRAINT [PK_HabitacionImagenes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HabitacionImagenes_Habitaciones] FOREIGN KEY ([HabitacionID]) REFERENCES [Habitaciones] ([HabitacionId]) ON DELETE CASCADE,
    CONSTRAINT [FK_HabitacionImagenes_Imagenes] FOREIGN KEY ([ImagenID]) REFERENCES [Imagenes] ([imagenID]) ON DELETE CASCADE
);
GO

CREATE TABLE [Inventarios] (
    [InventarioId] int NOT NULL IDENTITY,
    [HabitacionId] int NULL,
    [ArticuloId] int NULL,
    [Cantidad] int NULL,
    [FechaRegistro] datetime2 NULL,
    [Anulado] bit NULL,
    [InstitucionID] int NOT NULL,
    CONSTRAINT [PK_Inventarios] PRIMARY KEY ([InventarioId]),
    CONSTRAINT [FK_Inventarios_Articulos_ArticuloId] FOREIGN KEY ([ArticuloId]) REFERENCES [Articulos] ([ArticuloId]),
    CONSTRAINT [FK_Inventarios_Habitaciones_HabitacionId] FOREIGN KEY ([HabitacionId]) REFERENCES [Habitaciones] ([HabitacionId]),
    CONSTRAINT [FK_Inventarios_Instituciones_InstitucionID] FOREIGN KEY ([InstitucionID]) REFERENCES [Instituciones] ([InstitucionId]) ON DELETE CASCADE
);
GO

CREATE TABLE [Visitas] (
    [VisitaId] int NOT NULL IDENTITY,
    [PatenteVehiculo] nvarchar(max) NULL,
    [Identificador] nvarchar(max) NULL,
    [NumeroTelefono] nvarchar(max) NULL,
    [FechaPrimerIngreso] datetime2 NULL,
    [UsuarioId] int NULL,
    [FechaRegistro] datetime2 NULL,
    [Anulado] bit NOT NULL,
    [InstitucionID] int NOT NULL,
    [HabitacionId] int NULL,
    [HabitacionesHabitacionId] int NULL,
    CONSTRAINT [PK_Visitas] PRIMARY KEY ([VisitaId]),
    CONSTRAINT [FK_Visitas_Habitaciones_HabitacionesHabitacionId] FOREIGN KEY ([HabitacionesHabitacionId]) REFERENCES [Habitaciones] ([HabitacionId]),
    CONSTRAINT [FK_Visitas_Instituciones_InstitucionID] FOREIGN KEY ([InstitucionID]) REFERENCES [Instituciones] ([InstitucionId]) ON DELETE CASCADE
);
GO

CREATE TABLE [Movimientos] (
    [MovimientosId] int NOT NULL IDENTITY,
    [VisitaId] int NULL,
    [PagoId] int NULL,
    [HabitacionId] int NULL,
    [FechaInicio] datetime2 NULL,
    [FechaFin] datetime2 NULL,
    [TotalHoras] int NULL,
    [TotalFacturado] decimal(18,2) NULL,
    [UsuarioId] int NULL,
    [FechaRegistro] datetime2 NULL,
    [Descripcion] nvarchar(max) NULL,
    [Anulado] bit NULL,
    [InstitucionID] int NOT NULL,
    CONSTRAINT [PK_Movimientos] PRIMARY KEY ([MovimientosId]),
    CONSTRAINT [FK_Movimientos_Habitaciones_HabitacionId] FOREIGN KEY ([HabitacionId]) REFERENCES [Habitaciones] ([HabitacionId]),
    CONSTRAINT [FK_Movimientos_Pagos_PagoId] FOREIGN KEY ([PagoId]) REFERENCES [Pagos] ([PagoId]),
    CONSTRAINT [FK_Movimientos_Visitas_VisitaId] FOREIGN KEY ([VisitaId]) REFERENCES [Visitas] ([VisitaId])
);
GO

CREATE TABLE [Reservas] (
    [ReservaId] int NOT NULL IDENTITY,
    [VisitaId] int NULL,
    [HabitacionId] int NULL,
    [FechaReserva] datetime2 NULL,
    [FechaFin] datetime2 NULL,
    [TotalHoras] int NULL,
    [TotalMinutos] int NULL,
    [MovimientoId] int NULL,
    [PromocionId] int NULL,
    [UsuarioId] int NULL,
    [PausaHoras] int NULL,
    [PausaMinutos] int NULL,
    [FechaRegistro] datetime2 NULL,
    [InstitucionID] int NOT NULL,
    [FechaAnula] datetime2 NULL,
    CONSTRAINT [PK_Reservas] PRIMARY KEY ([ReservaId]),
    CONSTRAINT [FK_Reservas_Habitaciones_HabitacionId] FOREIGN KEY ([HabitacionId]) REFERENCES [Habitaciones] ([HabitacionId]),
    CONSTRAINT [FK_Reservas_Instituciones_InstitucionID] FOREIGN KEY ([InstitucionID]) REFERENCES [Instituciones] ([InstitucionId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Reservas_Promociones_PromocionId] FOREIGN KEY ([PromocionId]) REFERENCES [Promociones] ([PromocionID]),
    CONSTRAINT [FK_Reservas_Visitas_VisitaId] FOREIGN KEY ([VisitaId]) REFERENCES [Visitas] ([VisitaId])
);
GO

CREATE TABLE [MovimientosServicios] (
    [MovimientosServicioId] int NOT NULL IDENTITY,
    [MovimientosId] int NULL,
    [ServicioId] int NULL,
    [Cantidad] int NULL,
    [UsuarioId] int NULL,
    [FechaRegistro] datetime2 NULL,
    [Anulado] bit NULL,
    [ServiciosServicioId] int NULL,
    CONSTRAINT [PK_MovimientosServicios] PRIMARY KEY ([MovimientosServicioId]),
    CONSTRAINT [FK_MovimientosServicios_Movimientos_MovimientosId] FOREIGN KEY ([MovimientosId]) REFERENCES [Movimientos] ([MovimientosId]),
    CONSTRAINT [FK_MovimientosServicios_ServiciosAdicionales_ServicioId] FOREIGN KEY ([ServicioId]) REFERENCES [ServiciosAdicionales] ([ServicioId]),
    CONSTRAINT [FK_MovimientosServicios_Servicios_ServiciosServicioId] FOREIGN KEY ([ServiciosServicioId]) REFERENCES [Servicios] ([ServicioId])
);
GO

CREATE TABLE [MovimientosStock] (
    [MovimientoId] int NOT NULL IDENTITY,
    [ArticuloId] int NULL,
    [TipoMovimientoId] int NULL,
    [Cantidad] int NULL,
    [FechaMovimiento] datetime2 NULL,
    [MovimientosId] int NULL,
    [UsuarioId] int NULL,
    [FechaRegistro] datetime2 NULL,
    [Anulado] bit NULL,
    CONSTRAINT [PK_MovimientosStock] PRIMARY KEY ([MovimientoId]),
    CONSTRAINT [FK_MovimientosStock_Articulos_ArticuloId] FOREIGN KEY ([ArticuloId]) REFERENCES [Articulos] ([ArticuloId]),
    CONSTRAINT [FK_MovimientosStock_Movimientos_MovimientosId] FOREIGN KEY ([MovimientosId]) REFERENCES [Movimientos] ([MovimientosId]),
    CONSTRAINT [FK_MovimientosStock_TipoMovimiento_TipoMovimientoId] FOREIGN KEY ([TipoMovimientoId]) REFERENCES [TipoMovimiento] ([TipoMovimientoId])
);
GO

CREATE TABLE [Registros] (
    [RegistroID] int NOT NULL IDENTITY,
    [ReservaId] int NOT NULL,
    CONSTRAINT [PK_Registros] PRIMARY KEY ([RegistroID]),
    CONSTRAINT [FK_Registros_Reservas_ReservaId] FOREIGN KEY ([ReservaId]) REFERENCES [Reservas] ([ReservaId]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Acompanantes_InstitucionId] ON [Acompanantes] ([InstitucionId]);
GO

CREATE INDEX [IX_Acompanantes_InstitucionId1] ON [Acompanantes] ([InstitucionId1]);
GO

CREATE INDEX [IX_Articulos_imagenID] ON [Articulos] ([imagenID]);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO

CREATE INDEX [IX_AspNetUsers_InstitucionId] ON [AspNetUsers] ([InstitucionId]);
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

CREATE INDEX [IX_Caracteristicas_Nombre] ON [Caracteristicas] ([Nombre]);
GO

CREATE INDEX [IX_Cierre_InstitucionID] ON [Cierre] ([InstitucionID]);
GO

CREATE INDEX [IX_Cierre_UsuarioId] ON [Cierre] ([UsuarioId]);
GO

CREATE INDEX [IX_Configuracion_Activo] ON [Configuracion] ([Activo]);
GO

CREATE INDEX [IX_Configuracion_Categoria] ON [Configuracion] ([Categoria]);
GO

CREATE UNIQUE INDEX [IX_Configuracion_Clave_InstitucionId] ON [Configuracion] ([Clave], [InstitucionId]) WHERE [InstitucionId] IS NOT NULL;
GO

CREATE INDEX [IX_Configuracion_InstitucionId] ON [Configuracion] ([InstitucionId]);
GO

CREATE INDEX [IX_Consumo_ArticuloId] ON [Consumo] ([ArticuloId]);
GO

CREATE INDEX [IX_Consumo_MovimientosId] ON [Consumo] ([MovimientosId]);
GO

CREATE INDEX [IX_CuadreCierre_CierreId] ON [CuadreCierre] ([CierreId]);
GO

CREATE INDEX [IX_CuadreCierre_InstitucionId] ON [CuadreCierre] ([InstitucionId]);
GO

CREATE INDEX [IX_Egresos_CierreID] ON [Egresos] ([CierreID]);
GO

CREATE UNIQUE INDEX [IX_Egresos_MovimientoId] ON [Egresos] ([MovimientoId]) WHERE [MovimientoId] IS NOT NULL;
GO

CREATE INDEX [IX_Egresos_TipoEgresoId] ON [Egresos] ([TipoEgresoId]);
GO

CREATE INDEX [IX_Empeño_PagoID] ON [Empeño] ([PagoID]);
GO

CREATE INDEX [IX_Empeño_VisitaID] ON [Empeño] ([VisitaID]);
GO

CREATE INDEX [IX_Encargos_ArticuloId] ON [Encargos] ([ArticuloId]);
GO

CREATE INDEX [IX_Encargos_InstitucionId] ON [Encargos] ([InstitucionId]);
GO

CREATE INDEX [IX_Encargos_VisitaId] ON [Encargos] ([VisitaId]);
GO

CREATE INDEX [IX_HabitacionCaracteristicas_CaracteristicaId] ON [HabitacionCaracteristicas] ([CaracteristicaId]);
GO

CREATE INDEX [IX_HabitacionCaracteristicas_HabitacionId] ON [HabitacionCaracteristicas] ([HabitacionId]);
GO

CREATE INDEX [IX_Habitaciones_Anulado] ON [Habitaciones] ([Anulado]);
GO

CREATE INDEX [IX_Habitaciones_CategoriaId] ON [Habitaciones] ([CategoriaId]);
GO

CREATE INDEX [IX_Habitaciones_Disponible] ON [Habitaciones] ([Disponible]);
GO

CREATE INDEX [IX_Habitaciones_InstitucionID] ON [Habitaciones] ([InstitucionID]);
GO

CREATE INDEX [IX_Habitaciones_InstitucionID_Disponible_Anulado] ON [Habitaciones] ([InstitucionID], [Disponible], [Anulado]);
GO

CREATE UNIQUE INDEX [IX_Habitaciones_VisitaID] ON [Habitaciones] ([VisitaID]) WHERE [VisitaID] IS NOT NULL;
GO

CREATE INDEX [IX_HabitacionesVirtuales_Habitacion1Id] ON [HabitacionesVirtuales] ([Habitacion1Id]);
GO

CREATE INDEX [IX_HabitacionesVirtuales_Habitacion2Id] ON [HabitacionesVirtuales] ([Habitacion2Id]);
GO

CREATE INDEX [IX_HabitacionImagenes_Anulado] ON [HabitacionImagenes] ([Anulado]);
GO

CREATE INDEX [IX_HabitacionImagenes_EsPrincipal] ON [HabitacionImagenes] ([EsPrincipal]);
GO

CREATE INDEX [IX_HabitacionImagenes_HabitacionId] ON [HabitacionImagenes] ([HabitacionID]);
GO

CREATE INDEX [IX_HabitacionImagenes_ImagenId] ON [HabitacionImagenes] ([ImagenID]);
GO

CREATE INDEX [IX_HabitacionImagenes_Orden] ON [HabitacionImagenes] ([Orden]);
GO

CREATE INDEX [IX_Imagenes_NombreArchivo] ON [Imagenes] ([NombreArchivo]);
GO

CREATE INDEX [IX_InventarioGeneral_ArticuloId] ON [InventarioGeneral] ([ArticuloId]);
GO

CREATE INDEX [IX_InventarioInicial_InstitucionId] ON [InventarioInicial] ([InstitucionId]);
GO

CREATE INDEX [IX_Inventarios_ArticuloId] ON [Inventarios] ([ArticuloId]);
GO

CREATE INDEX [IX_Inventarios_HabitacionId] ON [Inventarios] ([HabitacionId]);
GO

CREATE INDEX [IX_Inventarios_InstitucionID] ON [Inventarios] ([InstitucionID]);
GO

CREATE INDEX [IX_Movimientos_HabitacionId] ON [Movimientos] ([HabitacionId]);
GO

CREATE INDEX [IX_Movimientos_PagoId] ON [Movimientos] ([PagoId]);
GO

CREATE INDEX [IX_Movimientos_VisitaId] ON [Movimientos] ([VisitaId]);
GO

CREATE INDEX [IX_MovimientosServicios_MovimientosId] ON [MovimientosServicios] ([MovimientosId]);
GO

CREATE INDEX [IX_MovimientosServicios_ServicioId] ON [MovimientosServicios] ([ServicioId]);
GO

CREATE INDEX [IX_MovimientosServicios_ServiciosServicioId] ON [MovimientosServicios] ([ServiciosServicioId]);
GO

CREATE INDEX [IX_MovimientosStock_ArticuloId] ON [MovimientosStock] ([ArticuloId]);
GO

CREATE INDEX [IX_MovimientosStock_MovimientosId] ON [MovimientosStock] ([MovimientosId]);
GO

CREATE INDEX [IX_MovimientosStock_TipoMovimientoId] ON [MovimientosStock] ([TipoMovimientoId]);
GO

CREATE INDEX [IX_MovimientosUsuarios_UsuarioId] ON [MovimientosUsuarios] ([UsuarioId]);
GO

CREATE INDEX [IX_Pagos_CierreId] ON [Pagos] ([CierreId]);
GO

CREATE INDEX [IX_Pagos_InstitucionID] ON [Pagos] ([InstitucionID]);
GO

CREATE INDEX [IX_Pagos_MedioPagoId] ON [Pagos] ([MedioPagoId]);
GO

CREATE INDEX [IX_Personal_RolId] ON [Personal] ([RolId]);
GO

CREATE INDEX [IX_Promociones_CategoriaID] ON [Promociones] ([CategoriaID]);
GO

CREATE UNIQUE INDEX [IX_Recargos_PagoID] ON [Recargos] ([PagoID]);
GO

CREATE INDEX [IX_Registros_ReservaId] ON [Registros] ([ReservaId]);
GO

CREATE INDEX [IX_Reservas_HabitacionId] ON [Reservas] ([HabitacionId]);
GO

CREATE INDEX [IX_Reservas_InstitucionID] ON [Reservas] ([InstitucionID]);
GO

CREATE INDEX [IX_Reservas_PromocionId] ON [Reservas] ([PromocionId]);
GO

CREATE INDEX [IX_Reservas_VisitaId] ON [Reservas] ([VisitaId]);
GO

CREATE INDEX [IX_Tarifas_CategoriaId] ON [Tarifas] ([CategoriaId]);
GO

CREATE INDEX [IX_Tarifas_DiaSemanaId] ON [Tarifas] ([DiaSemanaId]);
GO

CREATE INDEX [IX_Tarifas_TipoTarifaId] ON [Tarifas] ([TipoTarifaId]);
GO

CREATE INDEX [IX_Usuarios_RolId] ON [Usuarios] ([RolId]);
GO

CREATE INDEX [IX_UsuariosInstituciones_ApplicationUserId] ON [UsuariosInstituciones] ([ApplicationUserId]);
GO

CREATE INDEX [IX_UsuariosInstituciones_InstitucionID] ON [UsuariosInstituciones] ([InstitucionID]);
GO

CREATE INDEX [IX_Visitas_HabitacionesHabitacionId] ON [Visitas] ([HabitacionesHabitacionId]);
GO

CREATE INDEX [IX_Visitas_InstitucionID] ON [Visitas] ([InstitucionID]);
GO

ALTER TABLE [Consumo] ADD CONSTRAINT [FK_Consumo_Movimientos_MovimientosId] FOREIGN KEY ([MovimientosId]) REFERENCES [Movimientos] ([MovimientosId]);
GO

ALTER TABLE [Egresos] ADD CONSTRAINT [FK_Egresos_Movimientos_MovimientoId] FOREIGN KEY ([MovimientoId]) REFERENCES [Movimientos] ([MovimientosId]);
GO

ALTER TABLE [Empeño] ADD CONSTRAINT [FK_Empeño_Visitas_VisitaID] FOREIGN KEY ([VisitaID]) REFERENCES [Visitas] ([VisitaId]) ON DELETE CASCADE;
GO

ALTER TABLE [Encargos] ADD CONSTRAINT [FK_Encargos_Visitas_VisitaId] FOREIGN KEY ([VisitaId]) REFERENCES [Visitas] ([VisitaId]);
GO

ALTER TABLE [HabitacionCaracteristicas] ADD CONSTRAINT [FK_HabitacionCaracteristicas_Habitaciones] FOREIGN KEY ([HabitacionId]) REFERENCES [Habitaciones] ([HabitacionId]) ON DELETE CASCADE;
GO

ALTER TABLE [Habitaciones] ADD CONSTRAINT [FK_Habitaciones_Visitas] FOREIGN KEY ([VisitaID]) REFERENCES [Visitas] ([VisitaId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250701174952_InitialCreate', N'7.0.0');
GO

COMMIT;
GO

