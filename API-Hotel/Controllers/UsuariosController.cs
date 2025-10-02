using hotel.Models;
using hotel.Auth; // Para usar JwtService
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hotel.Data;
using hotel.DTOs;
using AutoMapper;
using hotel.Models.Sistema;
using Microsoft.AspNetCore.Authorization;

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
        [Authorize]
        public async Task<ActionResult<IEnumerable<UsuarioDTO>>> GetUsuarios(int institucionID)
        {
            try
            {
                var usuarios = await _context.UsuariosInstituciones
                    .Where(ui => ui.InstitucionID == institucionID)
                    .Include(ui => ui.Usuario)
                    .ThenInclude(u => u.Rol)
                    .Select(ui => ui.Usuario)
                    .ToListAsync();

                var usuariosDTO = _mapper.Map<IEnumerable<UsuarioDTO>>(usuarios);
                return Ok(usuariosDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al obtener usuarios", Error = ex.Message });
            }
        }

        [HttpGet("GetUsuario")]
        [Authorize]
        public async Task<ActionResult<UsuarioDTO>> GetUsuario(int id)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.UsuarioId == id);

                if (usuario == null)
                    return NotFound(new { Message = "Usuario no encontrado" });

                var usuarioDTO = _mapper.Map<UsuarioDTO>(usuario);
                return Ok(usuarioDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al obtener usuario", Error = ex.Message });
            }
        }

        [HttpGet("institucionUsuario")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<int>>> GetInstitucionUsuario(int usuarioID)
        {
            try
            {
                var instituciones = await _context.UsuariosInstituciones
                    .Where(ui => ui.UsuarioId == usuarioID)
                    .Select(ui => ui.InstitucionID)
                    .ToListAsync();

                if (instituciones == null || instituciones.Count == 0)
                    return NotFound(new { Message = "El usuario no está asociado a ninguna institución" });

                return Ok(instituciones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al obtener instituciones del usuario", Error = ex.Message });
            }
        }

        [HttpGet("GetInstitucionesPorUsuario")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<object>>> GetInstitucionesPorUsuario(int usuarioId)
        {
            try
            {
                var instituciones = await _context.UsuariosInstituciones
                    .Where(ui => ui.UsuarioId == usuarioId)
                    .Include(ui => ui.Institucion)
                    .Select(ui => new { 
                        InstitucionID = ui.InstitucionID, 
                        Nombre = ui.Institucion.Nombre 
                    })
                    .ToListAsync();

                if (instituciones == null || instituciones.Count == 0)
                    return NotFound(new { Message = "El usuario no está asociado a ninguna institución" });

                return Ok(instituciones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al obtener instituciones", Error = ex.Message });
            }
        }

        [HttpPost("CrearUsuario")]
        [Authorize]
        public async Task<ActionResult<UsuarioResponseDTO>> PostUsuario(int InstitucionID, UsuarioCreateDTO UsuarioCreateDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var usuario = new Usuarios
                {
                    NombreUsuario = UsuarioCreateDTO.NombreUsuario,
                    Contraseña = BCrypt.Net.BCrypt.HashPassword(UsuarioCreateDTO.Contraseña),
                    RolId = UsuarioCreateDTO.RolId
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                var usuarioInstitucion = new UsuariosInstituciones
                {
                    UsuarioId = usuario.UsuarioId,
                    InstitucionID = InstitucionID
                };

                _context.UsuariosInstituciones.Add(usuarioInstitucion);
                await _context.SaveChangesAsync();

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
                return StatusCode(500, new { Message = "Error al crear usuario", Error = ex.Message });
            }
        }

        [HttpPut("ActualizarUsuario")]
        [Authorize]
        public async Task<ActionResult<Respuesta>> ActualizarUsuario(int id, string? contraseña, int RolID = 404)
        {
            var res = new Respuesta();
            try
            {
                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null)
                {
                    res.Ok = false;
                    res.Message = $"No se encontró el usuario con ID: {id}.";
                    return NotFound(res);
                }

                if (!string.IsNullOrEmpty(contraseña))
                    usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(contraseña);

                if (RolID != 404)
                    usuario.RolId = RolID;

                _context.Usuarios.Update(usuario);
                await _context.SaveChangesAsync();

                res.Ok = true;
                res.Message = "Usuario actualizado correctamente.";
                res.Data = usuario;

                return Ok(res);
            }
            catch (Exception ex)
            {
                res.Message = $"Error: {ex.Message}";
                res.Ok = false;
                return StatusCode(500, res);
            }
        }

        [HttpDelete("BorrarUsuario")]
        [Authorize]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null)
                    return NotFound(new { Message = "Usuario no encontrado" });

                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Usuario eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al eliminar usuario", Error = ex.Message });
            }
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LegacyLoginDto loginDto)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.Rol)
                    .Include(u => u.UsuariosInstituciones)
                    .ThenInclude(ui => ui.Institucion)
                    .FirstOrDefaultAsync(u => u.NombreUsuario == loginDto.NombreUsuario);

                if (usuario == null || !BCrypt.Net.BCrypt.Verify(loginDto.Contraseña, usuario.Contraseña))
                    return Unauthorized(new { Message = "Nombre de usuario o contraseña incorrectos" });

                var token = _jwtService.GenerateToken(usuario.UsuarioId.ToString(), usuario.Rol.NombreRol);

                return Ok(new
                {
                    Token = token,
                    Rol = usuario.Rol.RolId,
                    UsuarioID = usuario.UsuarioId,
                    UsuarioName = usuario.NombreUsuario,
                    Instituciones = usuario.UsuariosInstituciones.Select(ui => new
                    {
                        InstitucionId = ui.Institucion.InstitucionId,
                        Nombre = ui.Institucion.Nombre
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error en el login", Error = ex.Message });
            }
        }

        [HttpGet("GetRoles")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<RolDTO>>> GetRoles()
        {
            try
            {
                var roles = await _context.HotelRoles.ToListAsync();
                var rolesDTO = _mapper.Map<IEnumerable<RolDTO>>(roles);
                return Ok(rolesDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al obtener roles", Error = ex.Message });
            }
        }

        [HttpPost("AsignarUsuarioAInstitucion")]
        [Authorize]
        public async Task<IActionResult> AsignarUsuarioAInstitucion(int usuarioId, int institucionId)
        {
            try
            {
                var existeRelacion = await _context.UsuariosInstituciones
                    .AnyAsync(ui => ui.UsuarioId == usuarioId && ui.InstitucionID == institucionId);

                if (existeRelacion)
                    return BadRequest(new { Message = "El usuario ya pertenece a esta institución" });

                var nuevaRelacion = new UsuariosInstituciones
                {
                    UsuarioId = usuarioId,
                    InstitucionID = institucionId
                };

                _context.UsuariosInstituciones.Add(nuevaRelacion);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Usuario asignado a la institución correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al asignar usuario", Error = ex.Message });
            }
        }

        [HttpDelete("EliminarUsuarioDeInstitucion")]
        [Authorize]
        public async Task<IActionResult> EliminarUsuarioDeInstitucion(int usuarioId, int institucionId)
        {
            try
            {
                var relacion = await _context.UsuariosInstituciones
                    .FirstOrDefaultAsync(ui => ui.UsuarioId == usuarioId && ui.InstitucionID == institucionId);

                if (relacion == null)
                    return NotFound(new { Message = "No se encontró la relación" });

                _context.UsuariosInstituciones.Remove(relacion);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Usuario eliminado de la institución" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al eliminar relación", Error = ex.Message });
            }
        }

        [HttpGet("UserInfo")]
        [Authorize]
        public async Task<ActionResult<hotel.DTOs.Identity.UserInfoDto>> GetUserInfo(int usuarioId)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.Rol)
                    .Include(u => u.UsuariosInstituciones)
                    .ThenInclude(ui => ui.Institucion)
                    .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

                if (usuario == null)
                    return NotFound(new { Message = "Usuario no encontrado" });

                var institucion = usuario.UsuariosInstituciones.FirstOrDefault()?.Institucion?.Nombre
                                  ?? "Institución desconocida";

                var dto = new hotel.DTOs.Identity.UserInfoDto
                {
                    UsuarioName = usuario.NombreUsuario,
                    Hotel = institucion,
                    Rol = usuario.Rol?.NombreRol ?? "Sin rol"
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al obtener información del usuario", Error = ex.Message });
            }
        }

        // Endpoint mejorado que funciona con o sin JWT
        [HttpGet("CurrentUser")]
        [Authorize]
        public async Task<ActionResult<object>> GetCurrentUser(int? usuarioId = null)
        {
            try
            {
                int targetUserId;

                // Intentar obtener del token JWT primero
                var userIdClaim = User.FindFirst("UsuarioId")?.Value ?? User.FindFirst("sub")?.Value;
                
                if (userIdClaim != null && int.TryParse(userIdClaim, out int jwtUserId))
                {
                    targetUserId = jwtUserId;
                }
                else if (usuarioId.HasValue)
                {
                    // Fallback: usar el usuarioId del query parameter
                    targetUserId = usuarioId.Value;
                }
                else
                {
                    return BadRequest(new { Message = "No se pudo determinar el ID del usuario" });
                }

                var usuario = await _context.Usuarios
                    .Include(u => u.Rol)
                    .Include(u => u.UsuariosInstituciones)
                    .ThenInclude(ui => ui.Institucion)
                    .FirstOrDefaultAsync(u => u.UsuarioId == targetUserId);

                if (usuario == null)
                    return NotFound(new { Message = "Usuario no encontrado" });

                return Ok(new
                {
                    UsuarioId = usuario.UsuarioId,
                    NombreUsuario = usuario.NombreUsuario,
                    Rol = new
                    {
                        Id = usuario.Rol?.RolId,
                        Nombre = usuario.Rol?.NombreRol
                    },
                    Instituciones = usuario.UsuariosInstituciones.Select(ui => new
                    {
                        Id = ui.Institucion?.InstitucionId,
                        Nombre = ui.Institucion?.Nombre
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al obtener usuario actual", Error = ex.Message });
            }
        }

        // Método auxiliar para validar existencia de usuario
        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.UsuarioId == id);
        }
    }
}