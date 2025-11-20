using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace hotel.Extensions
{
    /// <summary>
    /// Extensiones para gestionar el almacenamiento de archivos (imágenes, uploads).
    /// Garantiza que las carpetas necesarias existan antes de que la aplicación las use.
    /// </summary>
    public static class FileStorageExtensions
    {
        /// <summary>
        /// Rutas de carpetas de uploads usadas en la aplicación
        /// </summary>
        private const string UPLOADS_FOLDER = "uploads";
        private const string CARACTERISTICAS_SUBFOLDER = "caracteristicas";

        /// <summary>
        /// Asegura que todas las carpetas de uploads existan.
        /// Se debe llamar al inicio de la aplicación desde Program.cs.
        /// </summary>
        /// <param name="app">WebApplication</param>
        /// <returns>La misma aplicación (para chaining)</returns>
        public static WebApplication EnsureUploadsFoldersExist(this WebApplication app)
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            var environment = app.Services.GetRequiredService<IWebHostEnvironment>();

            try
            {
                // Crear carpeta principal de uploads
                var uploadsPath = GetUploadsFolderPath(environment);
                EnsureDirectoryExists(uploadsPath, "uploads", logger);

                // Crear subcarpeta de iconos de características
                var caracteristicasPath = GetCaracteristicasIconsFolderPath(environment);
                EnsureDirectoryExists(caracteristicasPath, "uploads/caracteristicas", logger);

                logger.LogInformation("✅ Todas las carpetas de almacenamiento de archivos están disponibles");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error al crear carpetas de almacenamiento de archivos");
                // No lanzamos excepción para no impedir el inicio de la app
                // Las carpetas se crearán cuando se intente subir el primer archivo
            }

            return app;
        }

        /// <summary>
        /// Obtiene la ruta absoluta de la carpeta principal de uploads.
        /// </summary>
        /// <param name="environment">IWebHostEnvironment</param>
        /// <returns>Ruta absoluta: {WebRootPath}/uploads</returns>
        public static string GetUploadsFolderPath(IWebHostEnvironment environment)
        {
            if (environment == null)
                throw new ArgumentNullException(nameof(environment));

            return Path.Combine(environment.WebRootPath, UPLOADS_FOLDER);
        }

        /// <summary>
        /// Obtiene la ruta absoluta de la carpeta de iconos de características.
        /// </summary>
        /// <param name="environment">IWebHostEnvironment</param>
        /// <returns>Ruta absoluta: {WebRootPath}/uploads/caracteristicas</returns>
        public static string GetCaracteristicasIconsFolderPath(IWebHostEnvironment environment)
        {
            if (environment == null)
                throw new ArgumentNullException(nameof(environment));

            return Path.Combine(
                environment.WebRootPath,
                UPLOADS_FOLDER,
                CARACTERISTICAS_SUBFOLDER
            );
        }

        /// <summary>
        /// Obtiene la ruta relativa de la carpeta de uploads (para URLs).
        /// </summary>
        /// <returns>Ruta relativa: /uploads</returns>
        public static string GetUploadsRelativePath()
        {
            return $"/{UPLOADS_FOLDER}";
        }

        /// <summary>
        /// Obtiene la ruta relativa de la carpeta de iconos de características (para URLs).
        /// </summary>
        /// <returns>Ruta relativa: /uploads/caracteristicas</returns>
        public static string GetCaracteristicasIconsRelativePath()
        {
            return $"/{UPLOADS_FOLDER}/{CARACTERISTICAS_SUBFOLDER}";
        }

        /// <summary>
        /// Crea un directorio si no existe.
        /// </summary>
        /// <param name="path">Ruta del directorio</param>
        /// <param name="folderName">Nombre del directorio (para logging)</param>
        /// <param name="logger">Logger</param>
        private static void EnsureDirectoryExists(string path, string folderName, ILogger logger)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                logger.LogInformation("📁 Carpeta '{FolderName}' creada en: {Path}", folderName, path);
            }
            else
            {
                logger.LogDebug("📁 Carpeta '{FolderName}' ya existe en: {Path}", folderName, path);
            }
        }

        /// <summary>
        /// Valida que un archivo sea una imagen válida.
        /// </summary>
        /// <param name="file">Archivo a validar</param>
        /// <param name="maxSizeInMB">Tamaño máximo en MB (default: 5MB)</param>
        /// <returns>Tupla (esValido, mensajeError)</returns>
        public static (bool isValid, string? errorMessage) ValidateImageFile(
            IFormFile file,
            int maxSizeInMB = 5)
        {
            if (file == null || file.Length == 0)
                return (false, "No se proporcionó ningún archivo");

            // Validar extensión
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return (false, $"Extensión no permitida. Solo se aceptan: {string.Join(", ", allowedExtensions)}");

            // Validar tamaño
            var maxSizeInBytes = maxSizeInMB * 1024 * 1024;
            if (file.Length > maxSizeInBytes)
                return (false, $"El archivo excede el tamaño máximo permitido de {maxSizeInMB}MB");

            // Validar content type
            var allowedContentTypes = new[]
            {
                "image/jpeg",
                "image/jpg",
                "image/png",
                "image/gif",
                "image/bmp",
                "image/webp"
            };

            if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
                return (false, $"Tipo de contenido no permitido: {file.ContentType}");

            return (true, null);
        }

        /// <summary>
        /// Genera un nombre único para un archivo.
        /// </summary>
        /// <param name="originalFileName">Nombre original del archivo</param>
        /// <returns>Nombre único: {GUID}{extension}</returns>
        public static string GenerateUniqueFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            return $"{Guid.NewGuid()}{extension}";
        }

        /// <summary>
        /// Elimina un archivo de forma segura (no lanza excepción si no existe).
        /// </summary>
        /// <param name="filePath">Ruta del archivo</param>
        /// <param name="logger">Logger</param>
        public static void SafeDeleteFile(string filePath, ILogger logger)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    logger.LogInformation("🗑️ Archivo eliminado: {FilePath}", filePath);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "⚠️ No se pudo eliminar el archivo: {FilePath}", filePath);
                // No lanzamos excepción para no afectar el flujo principal
            }
        }
    }
}
