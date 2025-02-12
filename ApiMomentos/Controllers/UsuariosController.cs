using ApiObjetos.Models;
using ApiObjetos.Auth; // Para usar JwtService
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiObjetos.Data;
using ApiObjetos.DTOs;

namespace ApiObjetos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly HotelDbContext _context;
        private readonly JwtService _jwtService;

        public UsuariosController(HotelDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // GET: api/Usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuarios>>> GetUsuarios()
        {
            return await _context.Usuarios.Include(u => u.Rol).ToListAsync();
        }

        // GET: api/Usuarios/5
        [HttpGet("{id}")]
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
        public async Task<ActionResult<Usuarios>> PostUsuario(UsuarioCreateDto usuarioDto)
        {
            // Crear un nuevo usuario a partir del DTO
            var usuario = new Usuarios
            {
                NombreUsuario = usuarioDto.NombreUsuario,
                Contraseña = BCrypt.Net.BCrypt.HashPassword(usuarioDto.Contraseña) ,
                RolId = usuarioDto.RolId
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUsuario", new { id = usuario.UsuarioId }, usuario);
        }


        // PUT: api/Usuarios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuarios usuario)
        {
            if (id != usuario.UsuarioId)
            {
                return BadRequest();
            }

            _context.Entry(usuario).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Usuarios/5
        [HttpDelete("{id}")]
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