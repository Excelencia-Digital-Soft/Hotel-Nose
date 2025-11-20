# Deploy-IIS-Application.ps1

<#
.SYNOPSIS
    Este script automatiza el despliegue en una APLICACIÓN específica dentro de un sitio de IIS.
    Detiene el Application Pool asociado, limpia el directorio (PRESERVANDO web.config), 
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
Write-Host "IMPORTANTE: Se preservará el archivo web.config existente en el destino."

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
<<<<<<< HEAD

    Write-Host "Deteniendo el Sitio Web '$WebsiteName'..."
    Stop-Website -Name $WebsiteName -ErrorAction Stop
    
    Write-Host "Application Pool y Sitio detenidos correctamente."

    # --- INICIO DE CAMBIOS ---

    # Lista de archivos a preservar en el servidor
    $filesToPreserve = @('web.config', 'appsettings.json')
    Write-Host "Se preservarán los siguientes archivos: $($filesToPreserve -join ', ')"

    # 3. Limpiar el directorio de destino, EXCLUYENDO $fileToPreserve
    Write-Host "Limpiando el directorio de destino: '$DestinationPath'..."
    if (Test-Path $DestinationPath) {
        Get-ChildItem -Path $DestinationPath -Force | Where-Object { $_.Name -notin $filesToPreserve } | Remove-Item -Recurse -Force
=======
    Write-Host "Application Pool detenido correctamente."

    # --- INICIO DE CAMBIOS ---

    # 3. Limpiar el directorio de destino, EXCLUYENDO web.config
    Write-Host "Limpiando el directorio de destino: '$DestinationPath' (excluyendo web.config)..."
    if (Test-Path $DestinationPath) {
        Get-ChildItem -Path $DestinationPath -Force | Where-Object { $_.Name -ne 'web.config' } | Remove-Item -Recurse -Force
>>>>>>> 042a1f9163b31d928325cd23a5b4a01b4b223977
        Write-Host "Directorio limpiado."
    } else {
        Write-Host "El directorio de destino no existe, se creará al copiar."
    }

<<<<<<< HEAD
    # 4. Copiar los nuevos archivos de la aplicación, EXCLUYENDO $fileToPreserve
    Write-Host "Copiando archivos desde '$SourcePath' hacia '$DestinationPath'..."
    
    Copy-Item -Path "$SourcePath\*" -Destination $DestinationPath -Recurse -Force -Exclude $filesToPreserve
=======
    # 4. Copiar los nuevos archivos de la aplicación, EXCLUYENDO web.config
    Write-Host "Copiando archivos desde '$SourcePath' hacia '$DestinationPath' (excluyendo web.config)..."
    # NUEVO: Usamos el parámetro -Exclude en Copy-Item para ignorar el web.config del origen.
    Copy-Item -Path "$SourcePath\*" -Destination $DestinationPath -Recurse -Force -Exclude "web.config"
>>>>>>> 042a1f9163b31d928325cd23a5b4a01b4b223977
    Write-Host "Archivos copiados correctamente."

    # --- FIN DE CAMBIOS ---

<<<<<<< HEAD
    Write-Host "Esperando 30 segundos antes de iniciar el pool..."
    Start-Sleep -Seconds 30

    Write-Host "Iniciando el Sitio Web '$WebsiteName'..."
    Start-Website -Name $WebsiteName -ErrorAction Stop
    
    Write-Host "Iniciando Application Pool '$appPool'..."
    Start-WebAppPool -Name $appPool -ErrorAction Stop    
    
=======
    Write-Host "Esperando 10 segundos antes de iniciar el pool..."
    Start-Sleep -Seconds 10

    Write-Host "Iniciando Application Pool '$appPool'..."
    Start-WebAppPool -Name $appPool -ErrorAction Stop
>>>>>>> 042a1f9163b31d928325cd23a5b4a01b4b223977
    Write-Host "Application Pool iniciado correctamente."

    Write-Host "¡Despliegue completado con éxito! El archivo web.config del servidor fue preservado."
}
catch {
    Write-Error "Ocurrió un error durante el despliegue: $($_.Exception.Message)"
    if ($appPool) {
        Write-Host "Intentando reiniciar el Application Pool '$appPool' para dejarlo en un estado funcional..."
        Start-WebAppPool -Name $appPool
    }
    exit 1
}
