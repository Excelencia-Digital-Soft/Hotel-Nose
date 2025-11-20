using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace hotel.Filters;

/// <summary>
/// Filtro para manejar correctamente el upload de archivos en Swagger
/// </summary>
public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Buscar parámetros que contengan IFormFile
        var formFileParameters = context.ApiDescription.ParameterDescriptions
            .Where(p => p.Type == typeof(IFormFile) || 
                       p.Type == typeof(IFormFile[]) ||
                       p.Type == typeof(IEnumerable<IFormFile>) ||
                       p.Type == typeof(List<IFormFile>))
            .ToList();

        // Si hay parámetros IFormFile directos (sin DTO), manejarlos
        if (formFileParameters.Any())
        {
            var properties = new Dictionary<string, OpenApiSchema>();

            foreach (var param in formFileParameters)
            {
                if (param.Type == typeof(IFormFile))
                {
                    properties[param.Name] = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    };
                }
                else if (param.Type == typeof(IFormFile[]) || 
                         param.Type == typeof(IEnumerable<IFormFile>) ||
                         param.Type == typeof(List<IFormFile>))
                {
                    properties[param.Name] = new OpenApiSchema
                    {
                        Type = "array",
                        Items = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "binary"
                        }
                    };
                }
            }

            // Agregar otros parámetros del form
            foreach (var param in context.ApiDescription.ParameterDescriptions)
            {
                if (!formFileParameters.Contains(param) && param.Source?.Id == "Form")
                {
                    properties[param.Name] = CreateSchemaForType(param.Type);
                }
            }

            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = properties,
                            Required = formFileParameters
                                .Where(p => p.IsRequired)
                                .Select(p => p.Name)
                                .ToHashSet()
                        }
                    }
                }
            };

            // Remover los parámetros que ya están en el body
            operation.Parameters?.Clear();
            return;
        }

        // Buscar DTOs que contengan propiedades IFormFile
        var formFileProps = context.ApiDescription.ParameterDescriptions
            .Where(p => p.Source?.Id == "Form")
            .SelectMany(p => p.Type?.GetProperties() ?? Array.Empty<System.Reflection.PropertyInfo>())
            .Where(prop => prop.PropertyType == typeof(IFormFile) ||
                          prop.PropertyType == typeof(IFormFile[]) ||
                          prop.PropertyType == typeof(IEnumerable<IFormFile>) ||
                          prop.PropertyType == typeof(List<IFormFile>))
            .ToList();

        if (formFileProps.Any())
        {
            var properties = new Dictionary<string, OpenApiSchema>();
            var required = new HashSet<string>();

            // Obtener todas las propiedades del DTO
            var dtoParameter = context.ApiDescription.ParameterDescriptions
                .FirstOrDefault(p => p.Source?.Id == "Form");

            if (dtoParameter?.Type != null)
            {
                foreach (var prop in dtoParameter.Type.GetProperties())
                {
                    if (prop.PropertyType == typeof(IFormFile))
                    {
                        properties[prop.Name] = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "binary"
                        };
                    }
                    else if (prop.PropertyType == typeof(IFormFile[]) || 
                             prop.PropertyType == typeof(IEnumerable<IFormFile>) ||
                             prop.PropertyType == typeof(List<IFormFile>))
                    {
                        properties[prop.Name] = new OpenApiSchema
                        {
                            Type = "array",
                            Items = new OpenApiSchema
                            {
                                Type = "string",
                                Format = "binary"
                            }
                        };
                    }
                    else
                    {
                        properties[prop.Name] = CreateSchemaForType(prop.PropertyType);
                    }

                    // Check if property is required
                    var requiredAttribute = prop.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute), false);
                    if (requiredAttribute.Any())
                    {
                        required.Add(prop.Name);
                    }
                }
            }

            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = properties,
                            Required = required
                        }
                    }
                }
            };
        }
    }

    private OpenApiSchema CreateSchemaForType(Type type)
    {
        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        if (underlyingType == typeof(string))
            return new OpenApiSchema { Type = "string" };
        
        if (underlyingType == typeof(int))
            return new OpenApiSchema { Type = "integer", Format = "int32" };
        
        if (underlyingType == typeof(long))
            return new OpenApiSchema { Type = "integer", Format = "int64" };
        
        if (underlyingType == typeof(decimal) || underlyingType == typeof(double) || underlyingType == typeof(float))
            return new OpenApiSchema { Type = "number", Format = "decimal" };
        
        if (underlyingType == typeof(bool))
            return new OpenApiSchema { Type = "boolean" };
        
        if (underlyingType == typeof(DateTime))
            return new OpenApiSchema { Type = "string", Format = "date-time" };
        
        if (underlyingType == typeof(Guid))
            return new OpenApiSchema { Type = "string", Format = "uuid" };

        // Default
        return new OpenApiSchema { Type = "string" };
    }
}