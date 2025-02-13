namespace ApiObjetos.Mapping
{
    using ApiObjetos.DTOs;
    using ApiObjetos.Models;
    using AutoMapper;

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Define los mapeos aquí
            CreateMap<Encargos, EncargoDTO>();
            CreateMap<Encargos, EncargoRequestDTO>();
            // Puedes añadir otros mapeos aquí si es necesario
            // Mapeo de Usuario a UsuarioDTO
            // Mapeo de Usuarios a UsuarioDTO
            CreateMap<Usuarios, UsuarioDTO>()
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.NombreUsuario))
                .ForMember(dest => dest.NombreRol, opt => opt.MapFrom(src => src.Rol.NombreRol));
            // Mapeo de Roles a RolDTO
            CreateMap<Roles, RolDTO>();
        }
    }
}
