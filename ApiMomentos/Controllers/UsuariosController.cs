using ApiObjetos.Models;
using ApiObjetos.Auth; // Para usar JwtService
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiObjetos.Data;
using ApiObjetos.DTOs;
using AutoMapper;
using ApiObjetos.Models.Sistema;

namespace ApiObjetos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly HotelDbContext _context;
        private readonly JwtService _jwtService;
        private readonly IMapper _mapper;

        public UsuariosController(HotelDbContext context, JwtService jwtService, IMapper mapper)
        {
            _context = context;
            _jwtService = jwtService;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetUsuarios")]

        public async Task<ActionResult<IEnumerable<UsuarioDTO>>> GetUsuarios(int InstitucionID)
        {
            // Obtén los usuarios con su rol relacionado
            var usuarios = await _context.Usuarios
                .Where(u => u.InstitucionID == InstitucionID)
                .Include(u => u.Rol)  // Incluye la relación con Rol
                .ToListAsync();

            // Mapea las entidades a DTOs usando AutoMapper
            var usuariosDTO = _mapper.Map<IEnumerable<UsuarioDTO>>(usuarios);

            return Ok(usuariosDTO);
        }

        // GET: api/Usuarios/5
        [HttpGet]
        [Route("GetUsuario")]
        public async Task<ActionResult<Usuarios>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.UsuarioId == id);

            if (usuario == null)
            {
                return NotFound();
            }

            return usuario;
        }

        [HttpGet("institucionUsuario")]
        public async Task<ActionResult<int>> GetInstitucionUsuario(int usuarioID)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioId == usuarioID);

            if (usuario == null)
            {
                return NotFound();
            }

            return usuario.InstitucionID;
        }
        [HttpPost]
        [Route("CrearUsuario")]

        public async Task<ActionResult<Usuarios>> PostUsuario(int InstitucionID, UsuarioCreateDto usuarioDto)
        {
            // Crear un nuevo usuario a partir del DTO
            var usuario = new Usuarios
            {
                NombreUsuario = usuarioDto.NombreUsuario,
                InstitucionID = InstitucionID,
                Contraseña = BCrypt.Net.BCrypt.HashPassword(usuarioDto.Contraseña),
                RolId = usuarioDto.RolId
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUsuario", new { id = usuario.UsuarioId }, usuario);
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

        // POST: api/Usuarios/Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.NombreUsuario == loginDto.NombreUsuario);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(loginDto.Contraseña, usuario.Contraseña))
            {
                return Unauthorized(new { Message = "Nombre de usuario o contraseña incorrectos" });
            }

            var token = _jwtService.GenerateToken(usuario.UsuarioId.ToString(), usuario.Rol.NombreRol);
            return Ok(new {
                Token = token,
                Rol = usuario.Rol.RolId,
                UsuarioID = usuario.UsuarioId,
                UsuarioName = usuario.NombreUsuario,
                InstitucionID = usuario.InstitucionID
            });
        }

        [HttpGet("GetRoles")]
        public async Task<ActionResult<IEnumerable<RolDTO>>> GetRoles()
        {
            var roles = await _context.Roles.ToListAsync();

            // Si estás usando AutoMapper
            var rolesDTO = _mapper.Map<IEnumerable<RolDTO>>(roles);

            return Ok(rolesDTO);
        }


        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.UsuarioId == id);
        }

    }


    public class LoginDto
    {
        public string NombreUsuario { get; set; }
        public string Contraseña { get; set; }
    }

}