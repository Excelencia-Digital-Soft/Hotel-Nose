using hotel.Models;
using hotel.Auth; // Para usar JwtService
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hotel.Data;
using hotel.DTOs;
using AutoMapper;
using hotel.Models.Sistema;

namespace hotel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController(HotelDbContext context, JwtService jwtService, IMapper mapper) : ControllerBase
    {
        private readonly HotelDbContext _context = context;
        private readonly JwtService _jwtService = jwtService;
        private readonly IMapper _mapper = mapper;

        [HttpGet("GetUsuarios")]
        public async Task<ActionResult<IEnumerable<UsuarioDTO>>> GetUsuarios(int institucionID)
        {
            // Obtener los usuarios asociados a la institución a través de la tabla intermedia
            var usuarios = await _context.UsuariosInstituciones
                .Where(ui => ui.InstitucionID == institucionID)  // Filtrar por InstitucionID
                .Include(ui => ui.Usuario)  // Incluir la relación con Usuario
                .ThenInclude(u => u.Rol)  // Incluir la relación con Rol dentro de Usuario
                .Select(ui => ui.Usuario)  // Obtener los usuarios relacionados
                .ToListAsync();

            // Mapear a DTOs usando AutoMapper
            var usuariosDTO = _mapper.Map<IEnumerable<UsuarioDTO>>(usuarios);

            return Ok(usuariosDTO);
        }

        // GET: api/Usuarios/5
        [HttpGet]
        [Route("GetUsuario")]
        public async Task<ActionResult<UsuarioDTO>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.UsuarioId == id);

            if (usuario == null)
            {
                return NotFound();
            }

            // Mapear a DTO antes de devolver
            var usuarioDTO = _mapper.Map<UsuarioDTO>(usuario);

            return Ok(usuarioDTO);
        }

        [HttpGet("institucionUsuario")]
        public async Task<ActionResult<IEnumerable<int>>> GetInstitucionUsuario(int usuarioID)
        {
            var instituciones = await _context.UsuariosInstituciones
                .Where(ui => ui.UsuarioId == usuarioID)
                .Select(ui => ui.InstitucionID)
                .ToListAsync();

            if (instituciones == null || instituciones.Count == 0)
            {
                return NotFound("El usuario no está asociado a ninguna institución.");
            }

            return Ok(instituciones);
        }
        [HttpGet("GetInstitucionesPorUsuario")]
        public async Task<ActionResult<IEnumerable<Institucion>>> GetInstitucionesPorUsuario(int usuarioId)
        {
            var instituciones = await _context.UsuariosInstituciones
                .Where(ui => ui.UsuarioId == usuarioId)
                .Select(ui => ui.InstitucionID)
                .ToListAsync();
            if (instituciones == null || instituciones.Count == 0)
            {
                return NotFound("El usuario no está asociado a ninguna institución.");
            }
            return Ok(instituciones);
        }
        [HttpPost]
        [Route("CrearUsuario")]
        public async Task<ActionResult<Usuarios>> PostUsuario(int InstitucionID, UsuarioCreateDTO UsuarioCreateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Crear el usuario sin la referencia directa a InstitucionID
                    var usuario = new Usuarios
                    {
                        NombreUsuario = UsuarioCreateDTO.NombreUsuario,
                        Contraseña = BCrypt.Net.BCrypt.HashPassword(UsuarioCreateDTO.Contraseña),
                        RolId = UsuarioCreateDTO.RolId
                    };

                    _context.Usuarios.Add(usuario);
                    await _context.SaveChangesAsync(); // Guardamos el usuario primero para obtener su ID

                    // Crear la relación en la tabla intermedia
                    var usuarioInstitucion = new UsuariosInstituciones
                    {
                        UsuarioId = usuario.UsuarioId,
                        InstitucionID = InstitucionID
                    };

                    _context.UsuariosInstituciones.Add(usuarioInstitucion);
                    await _context.SaveChangesAsync(); // Guardamos la relación en la tabla intermedia
                                                       // Mapear a DTO
                    var response = new UsuarioResponseDTO
                    {
                        UsuarioId = usuario.UsuarioId,
                        NombreUsuario = usuario.NombreUsuario,
                        RolId = usuario.RolId
                    };

                    await transaction.CommitAsync();



                    return CreatedAtAction("GetUsuario", new { id = usuario.UsuarioId }, response);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, "An error occurred while creating the user.");
                }
            }
        }


        [HttpPut]
        [Route("ActualizarUsuario")]
        public async Task<Respuesta> ActualizarUsuario(int id, string? contraseña, int RolID = 404)
        {
            Respuesta res = new Respuesta();

            try
            {
                // Step 1: Retrieve the articulo by ID
                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null)
                {
                    res.Ok = false;
                    res.Message = $"No se encontró el usuario con ID: {id}.";
                    return res;
                }

                // Step 2: Update the articulo fields if the new values are provided
                if (!string.IsNullOrEmpty(contraseña))
                {
                    usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(contraseña);
                }

                if (RolID != 404)
                {
                    usuario.RolId = RolID;
                }

                // Step 3: Save changes to the database
                _context.Usuarios.Update(usuario);
                await _context.SaveChangesAsync();

                // Set response on success
                res.Ok = true;
                res.Message = "Usuario actualizado correctamente.";
                res.Data = usuario;
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
            }

            return res;
        }
        // DELETE: api/Usuarios/5
        [HttpDelete]
        [Route("BorrarUsuario")]

        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LegacyLoginDto loginDto)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.UsuariosInstituciones)
                .ThenInclude(ui => ui.Institucion)
                .FirstOrDefaultAsync(u => u.NombreUsuario == loginDto.NombreUsuario);

            var passValidate = BCrypt.Net.BCrypt.Verify(loginDto.Contraseña, usuario?.Contraseña);

            if (usuario == null || !passValidate)
            {
                return Unauthorized(new { Message = "Nombre de usuario o contraseña incorrectos" });
            }

            var token = _jwtService.GenerateToken(usuario.UsuarioId.ToString(), usuario.Rol.NombreRol);

            return Ok(new
            {
                Token = token,
                Rol = usuario.Rol.RolId,
                UsuarioID = usuario.UsuarioId,
                UsuarioName = usuario.NombreUsuario,
                Instituciones = usuario.UsuariosInstituciones.Select(ui => new {
                    ui.Institucion.InstitucionId,
                    ui.Institucion.Nombre
                }).ToList()
            });
        }

        [HttpGet("GetRoles")]
        public async Task<ActionResult<IEnumerable<RolDTO>>> GetRoles()
        {
            var roles = await _context.HotelRoles.ToListAsync();

            // Si estás usando AutoMapper
            var rolesDTO = _mapper.Map<IEnumerable<RolDTO>>(roles);

            return Ok(rolesDTO);
        }


        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.UsuarioId == id);
        }


        [HttpPost("AsignarUsuarioAInstitucion")]
        public async Task<IActionResult> AsignarUsuarioAInstitucion(int usuarioId, int institucionId)
        {
            var existeRelacion = await _context.UsuariosInstituciones
                .AnyAsync(ui => ui.UsuarioId == usuarioId && ui.InstitucionID == institucionId);

            if (existeRelacion)
            {
                return BadRequest("El usuario ya pertenece a esta institución.");
            }

            var nuevaRelacion = new UsuariosInstituciones
            {
                UsuarioId = usuarioId,
                InstitucionID = institucionId
            };

            _context.UsuariosInstituciones.Add(nuevaRelacion);
            await _context.SaveChangesAsync();

            return Ok("Usuario asignado a la institución correctamente.");
        }

        [HttpDelete("EliminarUsuarioDeInstitucion")]
        public async Task<IActionResult> EliminarUsuarioDeInstitucion(int usuarioId, int institucionId)
        {
            var relacion = await _context.UsuariosInstituciones
                .FirstOrDefaultAsync(ui => ui.UsuarioId == usuarioId && ui.InstitucionID == institucionId);

            if (relacion == null)
            {
                return NotFound("No se encontró la relación.");
            }

            _context.UsuariosInstituciones.Remove(relacion);
            await _context.SaveChangesAsync();

            return Ok("Usuario eliminado de la institución.");
        }

    }


    public class LegacyLoginDto
    {
        public string NombreUsuario { get; set; }
        public string Contraseña { get; set; }
    }

}