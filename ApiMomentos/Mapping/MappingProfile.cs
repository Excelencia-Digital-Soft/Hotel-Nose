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
        }
    }
}
