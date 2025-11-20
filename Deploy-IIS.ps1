# Deploy-IIS-Application.ps1

<#
.SYNOPSIS
    Este script automatiza el despliegue en una APLICACIÓN específica dentro de un sitio de IIS.
    Detiene el Application Pool asociado, limpia el directorio (PRESERVANDO web.config y wwwroot),
    copia los nuevos archivos (EXCLUYENDO web.config) y reinicia el pool.
#>

param (
    [Parameter(Mandatory=$false)]
    [string]$WebsiteName = "Default Web Site",

    [Parameter(Mandatory=$true)]
    [string]$ApplicationName,

    [Parameter(Mandatory=$true)]
    [string]$SourcePath,

    [Parameter(Mandatory=$true)]
    [string]$DestinationPath
)

Import-Module WebAdministration -ErrorAction Stop

Write-Host "Iniciando despliegue para la aplicación: '$ApplicationName' en el sitio '$WebsiteName'"
Write-Host "IMPORTANTE: Se preservarán web.config, appsettings.json y la carpeta wwwroot/ (imágenes de usuarios)."

$appPath = "IIS:\Sites\$WebsiteName\$ApplicationName"
$appPool = $null

try {
    if (-not (Test-Path $appPath)) {
        throw "La aplicación '$ApplicationName' no existe en el sitio '$WebsiteName'. Por favor, créala primero."
    }

    $appPool = (Get-ItemProperty -Path $appPath -Name applicationPool).Value
    if (-not $appPool) {
        throw "No se pudo determinar el Application Pool para la aplicación '$ApplicationName'."
    }
    Write-Host "La aplicación usa el Application Pool: '$appPool'"

    Write-Host "Deteniendo Application Pool '$appPool'..."
    Stop-WebAppPool -Name $appPool -ErrorAction Stop

    Write-Host "Deteniendo el Sitio Web '$WebsiteName'..."
    Stop-Website -Name $WebsiteName -ErrorAction Stop

    Write-Host "Application Pool y Sitio detenidos correctamente."

    # --- INICIO DE CAMBIOS ---

    # Lista de archivos/carpetas a preservar en el servidor
    # CRÍTICO: wwwroot/ contiene todas las imágenes subidas por usuarios
    $filesToPreserve = @('web.config', 'appsettings.json', 'wwwroot')
    Write-Host "Se preservarán los siguientes archivos/carpetas: $($filesToPreserve -join ', ')"

    # 3. Limpiar el directorio de destino, EXCLUYENDO archivos/carpetas preservados
    Write-Host "Limpiando el directorio de destino: '$DestinationPath'..."
    if (Test-Path $DestinationPath) {
        Get-ChildItem -Path $DestinationPath -Force | Where-Object { $_.Name -notin $filesToPreserve } | Remove-Item -Recurse -Force
        Write-Host "Directorio limpiado (preservados: $($filesToPreserve -join ', '))."
    } else {
        Write-Host "El directorio de destino no existe, se creará al copiar."
    }

    # 4. Copiar los nuevos archivos de la aplicación, EXCLUYENDO archivos preservados
    Write-Host "Copiando archivos desde '$SourcePath' hacia '$DestinationPath'..."

    Copy-Item -Path "$SourcePath\*" -Destination $DestinationPath -Recurse -Force -Exclude $filesToPreserve
    Write-Host "Archivos copiados correctamente."

    # --- FIN DE CAMBIOS ---

    Write-Host "Esperando 30 segundos antes de iniciar el pool..."
    Start-Sleep -Seconds 30

    Write-Host "Iniciando el Sitio Web '$WebsiteName'..."
    Start-Website -Name $WebsiteName -ErrorAction Stop

    Write-Host "Iniciando Application Pool '$appPool'..."
    Start-WebAppPool -Name $appPool -ErrorAction Stop

    Write-Host "Application Pool y Sitio iniciados correctamente."

    Write-Host "¡Despliegue completado con éxito! Archivos preservados: web.config, appsettings.json, wwwroot/"
}
catch {
    Write-Error "Ocurrió un error durante el despliegue: $($_.Exception.Message)"
    if ($appPool) {
        Write-Host "Intentando reiniciar el Application Pool '$appPool' para dejarlo en un estado funcional..."
        Start-WebAppPool -Name $appPool
    }
    exit 1
}
