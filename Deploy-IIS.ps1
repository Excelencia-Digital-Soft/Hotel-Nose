# Deploy-IIS-Application.ps1

<#
.SYNOPSIS
    Este script automatiza el despliegue en una APLICACIÓN específica dentro de un sitio de IIS.
    Detiene el Application Pool asociado, fuerza la detención del proceso trabajador (w3wpex.e),
    limpia el directorio (PRESERVANDO web.config/appsettings.json), copia los nuevos archivos
    (EXCLUYENDO los preservados) y reinicia el pool.
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
Write-Host "IMPORTANTE: Se preservarán los archivos web.config y appsettings.json existentes en el destino."

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
    # ----- INICIO: Cambio -----
    # Cambiado a SilentlyContinue para evitar errores si el pool YA está detenido.
    Stop-WebAppPool -Name $appPool -ErrorAction SilentlyContinue

    Write-Host "Deteniendo el Sitio Web '$WebsiteName'..."
    # Cambiado a SilentlyContinue para evitar errores si el sitio YA está detenido.
    Stop-Website -Name $WebsiteName -ErrorAction SilentlyContinue
    # ----- FIN: Cambio -----
    
    Write-Host "Application Pool y Sitio detenidos."

    # ----- INICIO: Bloque modificado para forzar detención de w3wp.exe -----
    # Esto es crucial para liberar los archivos DLL (como Microsoft.Data.SqlClient.resources.dll)
    # y evitar errores de "Acceso Denegado".
    Write-Host "Asegurando que el proceso trabajador (w3wp.exe) para '$appPool' se haya detenido..."
    Start-Sleep -Seconds 3 # Dar 3 segundos para que intente cerrarse solo

    try {
        # Buscar el PID (ID de Proceso) del App Pool específico y forzar su cierre
        $ProcessToKill = Get-CimInstance Win32_Process -Filter "Name = 'w3wp.exe'" | Where-Object { $_.CommandLine -like "*$appPool*" }

        if ($ProcessToKill) {
            Write-Host "Se encontró el proceso trabajador (PID: $($ProcessToKill.ProcessId)). Forzando su detención..."
            Stop-Process -Id $ProcessToKill.ProcessId -Force -ErrorAction Stop
            Write-Host "Proceso detenido. Esperando 2 segundos adicionales..."
            Start-Sleep -Seconds 2 # Pequeña pausa después de forzar el cierre
        } else {
            Write-Host "El proceso trabajador ya se había detenido."
        }
    } catch {
        Write-Warning "No se pudo forzar la detención del proceso w3wp.exe. Esto podría ser normal si no estaba corriendo. Error: $($_.Exception.Message)"
    }
    # ----- FIN: Bloque modificado -----


    # Lista de archivos a preservar en el servidor
    $filesToPreserve = @('web.config', 'appsettings.json')
    Write-Host "Se preservarán los siguientes archivos: $($filesToPreserve -join ', ')"

    # 3. Limpiar el directorio de destino, EXCLUYENDO $filesToPreserve
    Write-Host "Limpiando el directorio de destino: '$DestinationPath'..."
    if (Test-Path $DestinationPath) {
        Get-ChildItem -Path $DestinationPath -Force | Where-Object { $_.Name -notin $filesToPreserve } | Remove-Item -Recurse -Force
        Write-Host "Directorio limpiado."
    } else {
        Write-Host "El directorio de destino no existe, se creará al copiar."
    }

    # 4. Copiar los nuevos archivos de la aplicación, EXCLUYENDO $filesToPreserve
    Write-Host "Copiando archivos desde '$SourcePath' hacia '$DestinationPath'..."
    
    Copy-Item -Path "$SourcePath\*" -Destination $DestinationPath -Recurse -Force -Exclude $filesToPreserve
    Write-Host "Archivos copiados correctamente."

    # Eliminamos la espera larga de 30 segundos aquí, ya no es necesaria
    # gracias a la detención forzada del w3wp.exe.
    # Start-Sleep -Seconds 30 

    Write-Host "Iniciando el Sitio Web '$WebsiteName'..."
    Start-Website -Name $WebsiteName -ErrorAction Stop
    
    Write-Host "Iniciando Application Pool '$appPool'..."
    Start-WebAppPool -Name $appPool -ErrorAction Stop    
    
    Write-Host "Application Pool iniciado correctamente."

    Write-Host "¡Despliegue completado con éxito! Los archivos ($($filesToPreserve -join ', ')) del servidor fueron preservados."
}
catch {
    Write-Error "Ocurrió un error durante el despliegue: $($_.Exception.Message)"
    if ($appPool) {
        Write-Host "Intentando reiniciar el Application Pool '$appPool' para dejarlo en un estado funcional..."
        Start-Website -Name $WebsiteName
        Start-WebAppPool -Name $appPool
    }
    exit 1
}

