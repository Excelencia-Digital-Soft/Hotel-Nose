using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hotel.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Caracteristicas",
                columns: table => new
                {
                    CaracteristicaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Icono = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Caracteristicas", x => x.CaracteristicaId);
                });

            migrationBuilder.CreateTable(
                name: "CategoriasArticulos",
                columns: table => new
                {
                    CategoriaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreCategoria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Anulado = table.Column<bool>(type: "bit", nullable: true),
                    InstitucionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasArticulos", x => x.CategoriaId);
                });

            migrationBuilder.CreateTable(
                name: "CategoriasHabitaciones",
                columns: table => new
                {
                    CategoriaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreCategoria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CapacidadMaxima = table.Column<int>(type: "int", nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Anulado = table.Column<bool>(type: "bit", nullable: true),
                    PrecioNormal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    InstitucionID = table.Column<int>(type: "int", nullable: false),
                    PorcentajeXPersona = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasHabitaciones", x => x.CategoriaId);
                });

            migrationBuilder.CreateTable(
                name: "DescuentoEfectivo",
                columns: table => new
                {
                    DescuentoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MontoPorcentual = table.Column<int>(type: "int", nullable: false),
                    InstitucionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DescuentoEfectivo", x => x.DescuentoID);
                });

            migrationBuilder.CreateTable(
                name: "DiasSemana",
                columns: table => new
                {
                    DiaSemanaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreDiaSemana = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiasSemana", x => x.DiaSemanaId);
                });

            migrationBuilder.CreateTable(
                name: "Imagenes",
                columns: table => new
                {
                    imagenID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreArchivo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InstitucionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Imagenes", x => x.imagenID);
                });

            migrationBuilder.CreateTable(
                name: "Instituciones",
                columns: table => new
                {
                    InstitucionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoID = table.Column<int>(type: "int", nullable: true),
                    FechaAnulado = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instituciones", x => x.InstitucionId);
                });

            migrationBuilder.CreateTable(
                name: "MediosPago",
                columns: table => new
                {
                    MedioPagoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediosPago", x => x.MedioPagoId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RolId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreRol = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RolId);
                });

            migrationBuilder.CreateTable(
                name: "Servicios",
                columns: table => new
                {
                    ServicioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreServicio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servicios", x => x.ServicioId);
                });

            migrationBuilder.CreateTable(
                name: "ServiciosAdicionales",
                columns: table => new
                {
                    ServicioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreServicio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Anulado = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiciosAdicionales", x => x.ServicioId);
                });

            migrationBuilder.CreateTable(
                name: "Tarjetas",
                columns: table => new
                {
                    TarjetaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MontoPorcentual = table.Column<int>(type: "int", nullable: false),
                    InstitucionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tarjetas", x => x.TarjetaID);
                });

            migrationBuilder.CreateTable(
                name: "TipoEgreso",
                columns: table => new
                {
                    TipoEgresoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InstitucionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoEgreso", x => x.TipoEgresoId);
                });

            migrationBuilder.CreateTable(
                name: "TipoMovimiento",
                columns: table => new
                {
                    TipoMovimientoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreTipoMovimiento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Anulado = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoMovimiento", x => x.TipoMovimientoId);
                });

            migrationBuilder.CreateTable(
                name: "TipoTarifa",
                columns: table => new
                {
                    TipoTarifaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreTipoTarifa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Anulado = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoTarifa", x => x.TipoTarifaId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Promociones",
                columns: table => new
                {
                    PromocionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tarifa = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CantidadHoras = table.Column<int>(type: "int", nullable: false),
                    CategoriaID = table.Column<int>(type: "int", nullable: false),
                    Anulado = table.Column<bool>(type: "bit", nullable: true),
                    Detalle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InstitucionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promociones", x => x.PromocionID);
                    table.ForeignKey(
                        name: "FK_Promociones_CategoriasHabitaciones_CategoriaID",
                        column: x => x.CategoriaID,
                        principalTable: "CategoriasHabitaciones",
                        principalColumn: "CategoriaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Articulos",
                columns: table => new
                {
                    ArticuloId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreArticulo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Anulado = table.Column<bool>(type: "bit", nullable: true),
                    imagenID = table.Column<int>(type: "int", nullable: false),
                    CategoriaID = table.Column<int>(type: "int", nullable: true),
                    InstitucionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articulos", x => x.ArticuloId);
                    table.ForeignKey(
                        name: "FK_Articulos_Imagenes_imagenID",
                        column: x => x.imagenID,
                        principalTable: "Imagenes",
                        principalColumn: "imagenID");
                });

            migrationBuilder.CreateTable(
                name: "Acompanantes",
                columns: table => new
                {
                    AcompananteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombres = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentoIdentidad = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Parentesco = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InstitucionId = table.Column<int>(type: "int", nullable: true),
                    InstitucionId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Acompanantes", x => x.AcompananteId);
                    table.ForeignKey(
                        name: "FK_Acompanantes_Instituciones_InstitucionId",
                        column: x => x.InstitucionId,
                        principalTable: "Instituciones",
                        principalColumn: "InstitucionId");
                    table.ForeignKey(
                        name: "FK_Acompanantes_Instituciones_InstitucionId1",
                        column: x => x.InstitucionId1,
                        principalTable: "Instituciones",
                        principalColumn: "InstitucionId");
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InstitucionId = table.Column<int>(type: "int", nullable: true),
                    LegacyUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ForcePasswordChange = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Instituciones_InstitucionId",
                        column: x => x.InstitucionId,
                        principalTable: "Instituciones",
                        principalColumn: "InstitucionId");
                });

            migrationBuilder.CreateTable(
                name: "Configuracion",
                columns: table => new
                {
                    ConfiguracionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Clave = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Valor = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Categoria = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    InstitucionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuracion", x => x.ConfiguracionId);
                    table.ForeignKey(
                        name: "FK_Configuracion_Institucion",
                        column: x => x.InstitucionId,
                        principalTable: "Instituciones",
                        principalColumn: "InstitucionId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Personal",
                columns: table => new
                {
                    PersonalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreCompleto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RolId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personal", x => x.PersonalId);
                    table.ForeignKey(
                        name: "FK_Personal_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "RolId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreUsuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Contraseña = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RolId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.UsuarioId);
                    table.ForeignKey(
                        name: "FK_Usuarios_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "RolId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tarifas",
                columns: table => new
                {
                    TarifaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DetalleTarifa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CategoriaId = table.Column<int>(type: "int", nullable: true),
                    DiaSemanaId = table.Column<int>(type: "int", nullable: true),
                    HoraInicio = table.Column<TimeSpan>(type: "time", nullable: true),
                    HoraFin = table.Column<TimeSpan>(type: "time", nullable: true),
                    TipoTarifaId = table.Column<int>(type: "int", nullable: true),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Anulado = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tarifas", x => x.TarifaId);
                    table.ForeignKey(
                        name: "FK_Tarifas_CategoriasHabitaciones_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "CategoriasHabitaciones",
                        principalColumn: "CategoriaId");
                    table.ForeignKey(
                        name: "FK_Tarifas_DiasSemana_DiaSemanaId",
                        column: x => x.DiaSemanaId,
                        principalTable: "DiasSemana",
                        principalColumn: "DiaSemanaId");
                    table.ForeignKey(
                        name: "FK_Tarifas_TipoTarifa_TipoTarifaId",
                        column: x => x.TipoTarifaId,
                        principalTable: "TipoTarifa",
                        principalColumn: "TipoTarifaId");
                });

            migrationBuilder.CreateTable(
                name: "InventarioGeneral",
                columns: table => new
                {
                    InventarioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArticuloId = table.Column<int>(type: "int", nullable: true),
                    Cantidad = table.Column<int>(type: "int", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Anulado = table.Column<bool>(type: "bit", nullable: true),
                    InstitucionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventarioGeneral", x => x.InventarioId);
                    table.ForeignKey(
                        name: "FK_InventarioGeneral_Articulos_ArticuloId",
                        column: x => x.ArticuloId,
                        principalTable: "Articulos",
                        principalColumn: "ArticuloId");
                });

            migrationBuilder.CreateTable(
                name: "InventarioInicial",
                columns: table => new
                {
                    ArticuloId = table.Column<int>(type: "int", nullable: false),
                    CantidadInicial = table.Column<int>(type: "int", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Anulado = table.Column<bool>(type: "bit", nullable: true),
                    InstitucionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventarioInicial", x => x.ArticuloId);
                    table.ForeignKey(
                        name: "FK_InventarioInicial_Articulos_ArticuloId",
                        column: x => x.ArticuloId,
                        principalTable: "Articulos",
                        principalColumn: "ArticuloId");
                    table.ForeignKey(
                        name: "FK_InventarioInicial_Instituciones_InstitucionId",
                        column: x => x.InstitucionId,
                        principalTable: "Instituciones",
                        principalColumn: "InstitucionId");
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cierre",
                columns: table => new
                {
                    CierreId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    FechaHoraCierre = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalIngresosEfectivo = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalIngresosBillVirt = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalIngresosTarjeta = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstadoCierre = table.Column<bool>(type: "bit", nullable: true),
                    MontoInicialCaja = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    InstitucionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cierre", x => x.CierreId);
                    table.ForeignKey(
                        name: "FK_Cierre_Instituciones_InstitucionID",
                        column: x => x.InstitucionID,
                        principalTable: "Instituciones",
                        principalColumn: "InstitucionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cierre_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId");
                });

            migrationBuilder.CreateTable(
                name: "MovimientosUsuarios",
                columns: table => new
                {
                    MovimientoId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosUsuarios", x => new { x.MovimientoId, x.UsuarioId });
                    table.ForeignKey(
                        name: "FK_MovimientosUsuarios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosInstituciones",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    InstitucionID = table.Column<int>(type: "int", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosInstituciones", x => new { x.UsuarioId, x.InstitucionID });
                    table.ForeignKey(
                        name: "FK_UsuariosInstituciones_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UsuariosInstituciones_Instituciones_InstitucionID",
                        column: x => x.InstitucionID,
                        principalTable: "Instituciones",
                        principalColumn: "InstitucionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuariosInstituciones_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CuadreCierre",
                columns: table => new
                {
                    CuadreCierreId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CierreId = table.Column<int>(type: "int", nullable: true),
                    TipoMovimiento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InstitucionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuadreCierre", x => x.CuadreCierreId);
                    table.ForeignKey(
                        name: "FK_CuadreCierre_Cierre_CierreId",
                        column: x => x.CierreId,
                        principalTable: "Cierre",
                        principalColumn: "CierreId");
                    table.ForeignKey(
                        name: "FK_CuadreCierre_Instituciones_InstitucionId",
                        column: x => x.InstitucionId,
                        principalTable: "Instituciones",
                        principalColumn: "InstitucionId");
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    PagoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MontoEfectivo = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MontoBillVirt = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MontoTarjeta = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Adicional = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MontoDescuento = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MedioPagoId = table.Column<int>(type: "int", nullable: true),
                    CierreId = table.Column<int>(type: "int", nullable: true),
                    TarjetaId = table.Column<int>(type: "int", nullable: true),
                    fechaHora = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InstitucionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.PagoId);
                    table.ForeignKey(
                        name: "FK_Pagos_Cierre_CierreId",
                        column: x => x.CierreId,
                        principalTable: "Cierre",
                        principalColumn: "CierreId");
                    table.ForeignKey(
                        name: "FK_Pagos_Instituciones_InstitucionID",
                        column: x => x.InstitucionID,
                        principalTable: "Instituciones",
                        principalColumn: "InstitucionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pagos_MediosPago_MedioPagoId",
                        column: x => x.MedioPagoId,
                        principalTable: "MediosPago",
                        principalColumn: "MedioPagoId");
                });

            migrationBuilder.CreateTable(
                name: "Recargos",
                columns: table => new
                {
                    RecargoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PagoID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recargos", x => x.RecargoID);
                    table.ForeignKey(
                        name: "FK_Recargos_Pagos_PagoID",
                        column: x => x.PagoID,
                        principalTable: "Pagos",
                        principalColumn: "PagoId");
                });

            migrationBuilder.CreateTable(
                name: "Consumo",
                columns: table => new
                {
                    ConsumoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovimientosId = table.Column<int>(type: "int", nullable: true),
                    ArticuloId = table.Column<int>(type: "int", nullable: true),
                    Cantidad = table.Column<int>(type: "int", nullable: true),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EsHabitacion = table.Column<bool>(type: "bit", nullable: true),
                    Anulado = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consumo", x => x.ConsumoId);
                    table.ForeignKey(
                        name: "FK_Consumo_Articulos_ArticuloId",
                        column: x => x.ArticuloId,
                        principalTable: "Articulos",
                        principalColumn: "ArticuloId");
                });

            migrationBuilder.CreateTable(
                name: "Egresos",
                columns: table => new
                {
                    EgresoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoEgresoId = table.Column<int>(type: "int", nullable: true),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MovimientoId = table.Column<int>(type: "int", nullable: true),
                    InstitucionID = table.Column<int>(type: "int", nullable: false),
                    CierreID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Egresos", x => x.EgresoId);
                    table.ForeignKey(
                        name: "FK_Egresos_Cierre_CierreID",
                        column: x => x.CierreID,
                        principalTable: "Cierre",
                        principalColumn: "CierreId");
                    table.ForeignKey(
                        name: "FK_Egresos_TipoEgreso_TipoEgresoId",
                        column: x => x.TipoEgresoId,
                        principalTable: "TipoEgreso",
                        principalColumn: "TipoEgresoId");
                });

            migrationBuilder.CreateTable(
                name: "Empeño",
                columns: table => new
                {
                    EmpeñoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitaID = table.Column<int>(type: "int", nullable: false),
                    Detalle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Monto = table.Column<double>(type: "float", nullable: false),
                    PagoID = table.Column<int>(type: "int", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Anulado = table.Column<bool>(type: "bit", nullable: true),
                    InstitucionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empeño", x => x.EmpeñoID);
                    table.ForeignKey(
                        name: "FK_Empeño_Pagos_PagoID",
                        column: x => x.PagoID,
                        principalTable: "Pagos",
                        principalColumn: "PagoId");
                });

            migrationBuilder.CreateTable(
                name: "Encargos",
                columns: table => new
                {
                    EncargosId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArticuloId = table.Column<int>(type: "int", nullable: true),
                    VisitaId = table.Column<int>(type: "int", nullable: true),
                    CantidadArt = table.Column<int>(type: "int", nullable: true),
                    Comentario = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Entregado = table.Column<bool>(type: "bit", nullable: true),
                    Anulado = table.Column<bool>(type: "bit", nullable: true),
                    FechaCrea = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InstitucionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Encargos", x => x.EncargosId);
                    table.ForeignKey(
                        name: "FK_Encargos_Articulos_ArticuloId",
                        column: x => x.ArticuloId,
                        principalTable: "Articulos",
                        principalColumn: "ArticuloId");
                    table.ForeignKey(
                        name: "FK_Encargos_Instituciones_InstitucionId",
                        column: x => x.InstitucionId,
                        principalTable: "Instituciones",
                        principalColumn: "InstitucionId");
                });

            migrationBuilder.CreateTable(
                name: "HabitacionCaracteristicas",
                columns: table => new
                {
                    HabitacionId = table.Column<int>(type: "int", nullable: false),
                    CaracteristicaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HabitacionCaracteristicas", x => new { x.HabitacionId, x.CaracteristicaId });
                    table.ForeignKey(
                        name: "FK_HabitacionCaracteristicas_Caracteristicas",
                        column: x => x.CaracteristicaId,
                        principalTable: "Caracteristicas",
                        principalColumn: "CaracteristicaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Habitaciones",
                columns: table => new
                {
                    HabitacionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreHabitacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CategoriaId = table.Column<int>(type: "int", nullable: true),
                    Disponible = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    ProximaReserva = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETDATE()"),
                    Anulado = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    VisitaID = table.Column<int>(type: "int", nullable: true),
                    InstitucionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Habitaciones", x => x.HabitacionId);
                    table.ForeignKey(
                        name: "FK_Habitaciones_CategoriasHabitaciones",
                        column: x => x.CategoriaId,
                        principalTable: "CategoriasHabitaciones",
                        principalColumn: "CategoriaId");
                    table.ForeignKey(
                        name: "FK_Habitaciones_Instituciones_InstitucionID",
                        column: x => x.InstitucionID,
                        principalTable: "Instituciones",
                        principalColumn: "InstitucionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HabitacionesVirtuales",
                columns: table => new
                {
                    HabitacionVirtualId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Habitacion1Id = table.Column<int>(type: "int", nullable: true),
                    Habitacion2Id = table.Column<int>(type: "int", nullable: true),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Anulado = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HabitacionesVirtuales", x => x.HabitacionVirtualId);
                    table.ForeignKey(
                        name: "FK_HabitacionesVirtuales_Habitaciones_Habitacion1Id",
                        column: x => x.Habitacion1Id,
                        principalTable: "Habitaciones",
                        principalColumn: "HabitacionId");
                    table.ForeignKey(
                        name: "FK_HabitacionesVirtuales_Habitaciones_Habitacion2Id",
                        column: x => x.Habitacion2Id,
                        principalTable: "Habitaciones",
                        principalColumn: "HabitacionId");
                });

            migrationBuilder.CreateTable(
                name: "HabitacionImagenes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HabitacionID = table.Column<int>(type: "int", nullable: false),
                    ImagenID = table.Column<int>(type: "int", nullable: false),
                    Anulado = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    EsPrincipal = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Orden = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HabitacionImagenes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HabitacionImagenes_Habitaciones",
                        column: x => x.HabitacionID,
                        principalTable: "Habitaciones",
                        principalColumn: "HabitacionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HabitacionImagenes_Imagenes",
                        column: x => x.ImagenID,
                        principalTable: "Imagenes",
                        principalColumn: "imagenID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Inventarios",
                columns: table => new
                {
                    InventarioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HabitacionId = table.Column<int>(type: "int", nullable: true),
                    ArticuloId = table.Column<int>(type: "int", nullable: true),
                    Cantidad = table.Column<int>(type: "int", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Anulado = table.Column<bool>(type: "bit", nullable: true),
                    InstitucionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventarios", x => x.InventarioId);
                    table.ForeignKey(
                        name: "FK_Inventarios_Articulos_ArticuloId",
                        column: x => x.ArticuloId,
                        principalTable: "Articulos",
                        principalColumn: "ArticuloId");
                    table.ForeignKey(
                        name: "FK_Inventarios_Habitaciones_HabitacionId",
                        column: x => x.HabitacionId,
                        principalTable: "Habitaciones",
                        principalColumn: "HabitacionId");
                    table.ForeignKey(
                        name: "FK_Inventarios_Instituciones_InstitucionID",
                        column: x => x.InstitucionID,
                        principalTable: "Instituciones",
                        principalColumn: "InstitucionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Visitas",
                columns: table => new
                {
                    VisitaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatenteVehiculo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Identificador = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumeroTelefono = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaPrimerIngreso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Anulado = table.Column<bool>(type: "bit", nullable: false),
                    InstitucionID = table.Column<int>(type: "int", nullable: false),
                    HabitacionId = table.Column<int>(type: "int", nullable: true),
                    HabitacionesHabitacionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visitas", x => x.VisitaId);
                    table.ForeignKey(
                        name: "FK_Visitas_Habitaciones_HabitacionesHabitacionId",
                        column: x => x.HabitacionesHabitacionId,
                        principalTable: "Habitaciones",
                        principalColumn: "HabitacionId");
                    table.ForeignKey(
                        name: "FK_Visitas_Instituciones_InstitucionID",
                        column: x => x.InstitucionID,
                        principalTable: "Instituciones",
                        principalColumn: "InstitucionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Movimientos",
                columns: table => new
                {
                    MovimientosId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitaId = table.Column<int>(type: "int", nullable: true),
                    PagoId = table.Column<int>(type: "int", nullable: true),
                    HabitacionId = table.Column<int>(type: "int", nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalHoras = table.Column<int>(type: "int", nullable: true),
                    TotalFacturado = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Anulado = table.Column<bool>(type: "bit", nullable: true),
                    InstitucionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movimientos", x => x.MovimientosId);
                    table.ForeignKey(
                        name: "FK_Movimientos_Habitaciones_HabitacionId",
                        column: x => x.HabitacionId,
                        principalTable: "Habitaciones",
                        principalColumn: "HabitacionId");
                    table.ForeignKey(
                        name: "FK_Movimientos_Pagos_PagoId",
                        column: x => x.PagoId,
                        principalTable: "Pagos",
                        principalColumn: "PagoId");
                    table.ForeignKey(
                        name: "FK_Movimientos_Visitas_VisitaId",
                        column: x => x.VisitaId,
                        principalTable: "Visitas",
                        principalColumn: "VisitaId");
                });

            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    ReservaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitaId = table.Column<int>(type: "int", nullable: true),
                    HabitacionId = table.Column<int>(type: "int", nullable: true),
                    FechaReserva = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalHoras = table.Column<int>(type: "int", nullable: true),
                    TotalMinutos = table.Column<int>(type: "int", nullable: true),
                    MovimientoId = table.Column<int>(type: "int", nullable: true),
                    PromocionId = table.Column<int>(type: "int", nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    PausaHoras = table.Column<int>(type: "int", nullable: true),
                    PausaMinutos = table.Column<int>(type: "int", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InstitucionID = table.Column<int>(type: "int", nullable: false),
                    FechaAnula = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.ReservaId);
                    table.ForeignKey(
                        name: "FK_Reservas_Habitaciones_HabitacionId",
                        column: x => x.HabitacionId,
                        principalTable: "Habitaciones",
                        principalColumn: "HabitacionId");
                    table.ForeignKey(
                        name: "FK_Reservas_Instituciones_InstitucionID",
                        column: x => x.InstitucionID,
                        principalTable: "Instituciones",
                        principalColumn: "InstitucionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservas_Promociones_PromocionId",
                        column: x => x.PromocionId,
                        principalTable: "Promociones",
                        principalColumn: "PromocionID");
                    table.ForeignKey(
                        name: "FK_Reservas_Visitas_VisitaId",
                        column: x => x.VisitaId,
                        principalTable: "Visitas",
                        principalColumn: "VisitaId");
                });

            migrationBuilder.CreateTable(
                name: "MovimientosServicios",
                columns: table => new
                {
                    MovimientosServicioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovimientosId = table.Column<int>(type: "int", nullable: true),
                    ServicioId = table.Column<int>(type: "int", nullable: true),
                    Cantidad = table.Column<int>(type: "int", nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Anulado = table.Column<bool>(type: "bit", nullable: true),
                    ServiciosServicioId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosServicios", x => x.MovimientosServicioId);
                    table.ForeignKey(
                        name: "FK_MovimientosServicios_Movimientos_MovimientosId",
                        column: x => x.MovimientosId,
                        principalTable: "Movimientos",
                        principalColumn: "MovimientosId");
                    table.ForeignKey(
                        name: "FK_MovimientosServicios_ServiciosAdicionales_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "ServiciosAdicionales",
                        principalColumn: "ServicioId");
                    table.ForeignKey(
                        name: "FK_MovimientosServicios_Servicios_ServiciosServicioId",
                        column: x => x.ServiciosServicioId,
                        principalTable: "Servicios",
                        principalColumn: "ServicioId");
                });

            migrationBuilder.CreateTable(
                name: "MovimientosStock",
                columns: table => new
                {
                    MovimientoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArticuloId = table.Column<int>(type: "int", nullable: true),
                    TipoMovimientoId = table.Column<int>(type: "int", nullable: true),
                    Cantidad = table.Column<int>(type: "int", nullable: true),
                    FechaMovimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MovimientosId = table.Column<int>(type: "int", nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Anulado = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosStock", x => x.MovimientoId);
                    table.ForeignKey(
                        name: "FK_MovimientosStock_Articulos_ArticuloId",
                        column: x => x.ArticuloId,
                        principalTable: "Articulos",
                        principalColumn: "ArticuloId");
                    table.ForeignKey(
                        name: "FK_MovimientosStock_Movimientos_MovimientosId",
                        column: x => x.MovimientosId,
                        principalTable: "Movimientos",
                        principalColumn: "MovimientosId");
                    table.ForeignKey(
                        name: "FK_MovimientosStock_TipoMovimiento_TipoMovimientoId",
                        column: x => x.TipoMovimientoId,
                        principalTable: "TipoMovimiento",
                        principalColumn: "TipoMovimientoId");
                });

            migrationBuilder.CreateTable(
                name: "Registros",
                columns: table => new
                {
                    RegistroID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registros", x => x.RegistroID);
                    table.ForeignKey(
                        name: "FK_Registros_Reservas_ReservaId",
                        column: x => x.ReservaId,
                        principalTable: "Reservas",
                        principalColumn: "ReservaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Acompanantes_InstitucionId",
                table: "Acompanantes",
                column: "InstitucionId");

            migrationBuilder.CreateIndex(
                name: "IX_Acompanantes_InstitucionId1",
                table: "Acompanantes",
                column: "InstitucionId1");

            migrationBuilder.CreateIndex(
                name: "IX_Articulos_imagenID",
                table: "Articulos",
                column: "imagenID");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_InstitucionId",
                table: "AspNetUsers",
                column: "InstitucionId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Caracteristicas_Nombre",
                table: "Caracteristicas",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_Cierre_InstitucionID",
                table: "Cierre",
                column: "InstitucionID");

            migrationBuilder.CreateIndex(
                name: "IX_Cierre_UsuarioId",
                table: "Cierre",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Configuracion_Activo",
                table: "Configuracion",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_Configuracion_Categoria",
                table: "Configuracion",
                column: "Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_Configuracion_Clave_InstitucionId",
                table: "Configuracion",
                columns: new[] { "Clave", "InstitucionId" },
                unique: true,
                filter: "[InstitucionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Configuracion_InstitucionId",
                table: "Configuracion",
                column: "InstitucionId");

            migrationBuilder.CreateIndex(
                name: "IX_Consumo_ArticuloId",
                table: "Consumo",
                column: "ArticuloId");

            migrationBuilder.CreateIndex(
                name: "IX_Consumo_MovimientosId",
                table: "Consumo",
                column: "MovimientosId");

            migrationBuilder.CreateIndex(
                name: "IX_CuadreCierre_CierreId",
                table: "CuadreCierre",
                column: "CierreId");

            migrationBuilder.CreateIndex(
                name: "IX_CuadreCierre_InstitucionId",
                table: "CuadreCierre",
                column: "InstitucionId");

            migrationBuilder.CreateIndex(
                name: "IX_Egresos_CierreID",
                table: "Egresos",
                column: "CierreID");

            migrationBuilder.CreateIndex(
                name: "IX_Egresos_MovimientoId",
                table: "Egresos",
                column: "MovimientoId",
                unique: true,
                filter: "[MovimientoId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Egresos_TipoEgresoId",
                table: "Egresos",
                column: "TipoEgresoId");

            migrationBuilder.CreateIndex(
                name: "IX_Empeño_PagoID",
                table: "Empeño",
                column: "PagoID");

            migrationBuilder.CreateIndex(
                name: "IX_Empeño_VisitaID",
                table: "Empeño",
                column: "VisitaID");

            migrationBuilder.CreateIndex(
                name: "IX_Encargos_ArticuloId",
                table: "Encargos",
                column: "ArticuloId");

            migrationBuilder.CreateIndex(
                name: "IX_Encargos_InstitucionId",
                table: "Encargos",
                column: "InstitucionId");

            migrationBuilder.CreateIndex(
                name: "IX_Encargos_VisitaId",
                table: "Encargos",
                column: "VisitaId");

            migrationBuilder.CreateIndex(
                name: "IX_HabitacionCaracteristicas_CaracteristicaId",
                table: "HabitacionCaracteristicas",
                column: "CaracteristicaId");

            migrationBuilder.CreateIndex(
                name: "IX_HabitacionCaracteristicas_HabitacionId",
                table: "HabitacionCaracteristicas",
                column: "HabitacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Habitaciones_Anulado",
                table: "Habitaciones",
                column: "Anulado");

            migrationBuilder.CreateIndex(
                name: "IX_Habitaciones_CategoriaId",
                table: "Habitaciones",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Habitaciones_Disponible",
                table: "Habitaciones",
                column: "Disponible");

            migrationBuilder.CreateIndex(
                name: "IX_Habitaciones_InstitucionID",
                table: "Habitaciones",
                column: "InstitucionID");

            migrationBuilder.CreateIndex(
                name: "IX_Habitaciones_InstitucionID_Disponible_Anulado",
                table: "Habitaciones",
                columns: new[] { "InstitucionID", "Disponible", "Anulado" });

            migrationBuilder.CreateIndex(
                name: "IX_Habitaciones_VisitaID",
                table: "Habitaciones",
                column: "VisitaID",
                unique: true,
                filter: "[VisitaID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HabitacionesVirtuales_Habitacion1Id",
                table: "HabitacionesVirtuales",
                column: "Habitacion1Id");

            migrationBuilder.CreateIndex(
                name: "IX_HabitacionesVirtuales_Habitacion2Id",
                table: "HabitacionesVirtuales",
                column: "Habitacion2Id");

            migrationBuilder.CreateIndex(
                name: "IX_HabitacionImagenes_Anulado",
                table: "HabitacionImagenes",
                column: "Anulado");

            migrationBuilder.CreateIndex(
                name: "IX_HabitacionImagenes_EsPrincipal",
                table: "HabitacionImagenes",
                column: "EsPrincipal");

            migrationBuilder.CreateIndex(
                name: "IX_HabitacionImagenes_HabitacionId",
                table: "HabitacionImagenes",
                column: "HabitacionID");

            migrationBuilder.CreateIndex(
                name: "IX_HabitacionImagenes_ImagenId",
                table: "HabitacionImagenes",
                column: "ImagenID");

            migrationBuilder.CreateIndex(
                name: "IX_HabitacionImagenes_Orden",
                table: "HabitacionImagenes",
                column: "Orden");

            migrationBuilder.CreateIndex(
                name: "IX_Imagenes_NombreArchivo",
                table: "Imagenes",
                column: "NombreArchivo");

            migrationBuilder.CreateIndex(
                name: "IX_InventarioGeneral_ArticuloId",
                table: "InventarioGeneral",
                column: "ArticuloId");

            migrationBuilder.CreateIndex(
                name: "IX_InventarioInicial_InstitucionId",
                table: "InventarioInicial",
                column: "InstitucionId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventarios_ArticuloId",
                table: "Inventarios",
                column: "ArticuloId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventarios_HabitacionId",
                table: "Inventarios",
                column: "HabitacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventarios_InstitucionID",
                table: "Inventarios",
                column: "InstitucionID");

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_HabitacionId",
                table: "Movimientos",
                column: "HabitacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_PagoId",
                table: "Movimientos",
                column: "PagoId");

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_VisitaId",
                table: "Movimientos",
                column: "VisitaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosServicios_MovimientosId",
                table: "MovimientosServicios",
                column: "MovimientosId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosServicios_ServicioId",
                table: "MovimientosServicios",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosServicios_ServiciosServicioId",
                table: "MovimientosServicios",
                column: "ServiciosServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosStock_ArticuloId",
                table: "MovimientosStock",
                column: "ArticuloId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosStock_MovimientosId",
                table: "MovimientosStock",
                column: "MovimientosId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosStock_TipoMovimientoId",
                table: "MovimientosStock",
                column: "TipoMovimientoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosUsuarios_UsuarioId",
                table: "MovimientosUsuarios",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_CierreId",
                table: "Pagos",
                column: "CierreId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_InstitucionID",
                table: "Pagos",
                column: "InstitucionID");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_MedioPagoId",
                table: "Pagos",
                column: "MedioPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_Personal_RolId",
                table: "Personal",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_Promociones_CategoriaID",
                table: "Promociones",
                column: "CategoriaID");

            migrationBuilder.CreateIndex(
                name: "IX_Recargos_PagoID",
                table: "Recargos",
                column: "PagoID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registros_ReservaId",
                table: "Registros",
                column: "ReservaId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_HabitacionId",
                table: "Reservas",
                column: "HabitacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_InstitucionID",
                table: "Reservas",
                column: "InstitucionID");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_PromocionId",
                table: "Reservas",
                column: "PromocionId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_VisitaId",
                table: "Reservas",
                column: "VisitaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tarifas_CategoriaId",
                table: "Tarifas",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tarifas_DiaSemanaId",
                table: "Tarifas",
                column: "DiaSemanaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tarifas_TipoTarifaId",
                table: "Tarifas",
                column: "TipoTarifaId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_RolId",
                table: "Usuarios",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosInstituciones_ApplicationUserId",
                table: "UsuariosInstituciones",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosInstituciones_InstitucionID",
                table: "UsuariosInstituciones",
                column: "InstitucionID");

            migrationBuilder.CreateIndex(
                name: "IX_Visitas_HabitacionesHabitacionId",
                table: "Visitas",
                column: "HabitacionesHabitacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Visitas_InstitucionID",
                table: "Visitas",
                column: "InstitucionID");

            migrationBuilder.AddForeignKey(
                name: "FK_Consumo_Movimientos_MovimientosId",
                table: "Consumo",
                column: "MovimientosId",
                principalTable: "Movimientos",
                principalColumn: "MovimientosId");

            migrationBuilder.AddForeignKey(
                name: "FK_Egresos_Movimientos_MovimientoId",
                table: "Egresos",
                column: "MovimientoId",
                principalTable: "Movimientos",
                principalColumn: "MovimientosId");

            migrationBuilder.AddForeignKey(
                name: "FK_Empeño_Visitas_VisitaID",
                table: "Empeño",
                column: "VisitaID",
                principalTable: "Visitas",
                principalColumn: "VisitaId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Encargos_Visitas_VisitaId",
                table: "Encargos",
                column: "VisitaId",
                principalTable: "Visitas",
                principalColumn: "VisitaId");

            migrationBuilder.AddForeignKey(
                name: "FK_HabitacionCaracteristicas_Habitaciones",
                table: "HabitacionCaracteristicas",
                column: "HabitacionId",
                principalTable: "Habitaciones",
                principalColumn: "HabitacionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Habitaciones_Visitas",
                table: "Habitaciones",
                column: "VisitaID",
                principalTable: "Visitas",
                principalColumn: "VisitaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Habitaciones_Instituciones_InstitucionID",
                table: "Habitaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Visitas_Instituciones_InstitucionID",
                table: "Visitas");

            migrationBuilder.DropForeignKey(
                name: "FK_Habitaciones_Visitas",
                table: "Habitaciones");

            migrationBuilder.DropTable(
                name: "Acompanantes");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CategoriasArticulos");

            migrationBuilder.DropTable(
                name: "Configuracion");

            migrationBuilder.DropTable(
                name: "Consumo");

            migrationBuilder.DropTable(
                name: "CuadreCierre");

            migrationBuilder.DropTable(
                name: "DescuentoEfectivo");

            migrationBuilder.DropTable(
                name: "Egresos");

            migrationBuilder.DropTable(
                name: "Empeño");

            migrationBuilder.DropTable(
                name: "Encargos");

            migrationBuilder.DropTable(
                name: "HabitacionCaracteristicas");

            migrationBuilder.DropTable(
                name: "HabitacionesVirtuales");

            migrationBuilder.DropTable(
                name: "HabitacionImagenes");

            migrationBuilder.DropTable(
                name: "InventarioGeneral");

            migrationBuilder.DropTable(
                name: "InventarioInicial");

            migrationBuilder.DropTable(
                name: "Inventarios");

            migrationBuilder.DropTable(
                name: "MovimientosServicios");

            migrationBuilder.DropTable(
                name: "MovimientosStock");

            migrationBuilder.DropTable(
                name: "MovimientosUsuarios");

            migrationBuilder.DropTable(
                name: "Personal");

            migrationBuilder.DropTable(
                name: "Recargos");

            migrationBuilder.DropTable(
                name: "Registros");

            migrationBuilder.DropTable(
                name: "Tarifas");

            migrationBuilder.DropTable(
                name: "Tarjetas");

            migrationBuilder.DropTable(
                name: "UsuariosInstituciones");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "TipoEgreso");

            migrationBuilder.DropTable(
                name: "Caracteristicas");

            migrationBuilder.DropTable(
                name: "ServiciosAdicionales");

            migrationBuilder.DropTable(
                name: "Servicios");

            migrationBuilder.DropTable(
                name: "Articulos");

            migrationBuilder.DropTable(
                name: "Movimientos");

            migrationBuilder.DropTable(
                name: "TipoMovimiento");

            migrationBuilder.DropTable(
                name: "Reservas");

            migrationBuilder.DropTable(
                name: "DiasSemana");

            migrationBuilder.DropTable(
                name: "TipoTarifa");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Imagenes");

            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "Promociones");

            migrationBuilder.DropTable(
                name: "Cierre");

            migrationBuilder.DropTable(
                name: "MediosPago");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Instituciones");

            migrationBuilder.DropTable(
                name: "Visitas");

            migrationBuilder.DropTable(
                name: "Habitaciones");

            migrationBuilder.DropTable(
                name: "CategoriasHabitaciones");
        }
    }
}
