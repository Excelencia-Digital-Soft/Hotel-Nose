# Pre-Deploy-Backup.ps1

<#
.SYNOPSIS
    Script de BACKUP AUTOMÁTICO de imágenes antes de deployment.
    Crea una copia de seguridad timestamped de la carpeta wwwroot/uploads.

.DESCRIPTION
    Este script debe ejecutarse ANTES de Deploy-IIS.ps1 para proteger las imágenes de usuarios.
    Crea backups en C:\HotelBackups\uploads_{timestamp} con rotación automática de backups antiguos.

.PARAMETER DestinationPath
    Ruta del directorio de la aplicación en IIS (donde está wwwroot/).
    Ejemplo: "C:\inetpub\wwwroot\HotelAPI"

.PARAMETER BackupBasePath
    Ruta base donde se guardarán los backups (default: C:\HotelBackups).

.PARAMETER KeepLastBackups
    Cantidad de backups a mantener (default: 10). Los más antiguos se eliminan automáticamente.

.EXAMPLE
    .\Pre-Deploy-Backup.ps1 -DestinationPath "C:\inetpub\wwwroot\HotelAPI"

.EXAMPLE
    .\Pre-Deploy-Backup.ps1 -DestinationPath "C:\inetpub\wwwroot\HotelAPI" -BackupBasePath "D:\Backups" -KeepLastBackups 20
#>

param (
    [Parameter(Mandatory=$true)]
    [string]$DestinationPath,

    [Parameter(Mandatory=$false)]
    [string]$BackupBasePath = "C:\HotelBackups",

    [Parameter(Mandatory=$false)]
    [int]$KeepLastBackups = 10
)

# Configuración
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$uploadsSourcePath = Join-Path $DestinationPath "wwwroot\uploads"
$backupDestinationPath = Join-Path $BackupBasePath "uploads_$timestamp"

Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  BACKUP DE IMÁGENES PRE-DEPLOYMENT" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Validar que la carpeta de uploads existe
if (-not (Test-Path $uploadsSourcePath)) {
    Write-Host "⚠️  ADVERTENCIA: La carpeta de uploads no existe en:" -ForegroundColor Yellow
    Write-Host "   $uploadsSourcePath" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "   Posibles razones:" -ForegroundColor Yellow
    Write-Host "   1. Es el primer deployment (normal, no hay imágenes aún)" -ForegroundColor Yellow
    Write-Host "   2. La ruta especificada es incorrecta" -ForegroundColor Yellow
    Write-Host "   3. Las imágenes ya fueron eliminadas" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "   No se realizará backup. Presione cualquier tecla para continuar..." -ForegroundColor Yellow
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 0
}

# Contar archivos en uploads
$imageCount = (Get-ChildItem -Path $uploadsSourcePath -Recurse -File).Count
Write-Host "📊 Estadísticas de origen:" -ForegroundColor White
Write-Host "   Ruta: $uploadsSourcePath" -ForegroundColor Gray
Write-Host "   Archivos: $imageCount imagen(es)" -ForegroundColor Gray
Write-Host ""

if ($imageCount -eq 0) {
    Write-Host "ℹ️  No hay imágenes para respaldar. Saltando backup..." -ForegroundColor Cyan
    exit 0
}

try {
    # Crear directorio base de backups si no existe
    if (-not (Test-Path $BackupBasePath)) {
        New-Item -Path $BackupBasePath -ItemType Directory -Force | Out-Null
        Write-Host "✅ Directorio de backups creado: $BackupBasePath" -ForegroundColor Green
    }

    # Crear backup
    Write-Host "📦 Creando backup..." -ForegroundColor Yellow
    Write-Host "   Destino: $backupDestinationPath" -ForegroundColor Gray

    Copy-Item -Path $uploadsSourcePath -Destination $backupDestinationPath -Recurse -Force

    $backupSize = (Get-ChildItem -Path $backupDestinationPath -Recurse -File | Measure-Object -Property Length -Sum).Sum / 1MB
    Write-Host "✅ Backup completado exitosamente!" -ForegroundColor Green
    Write-Host "   Tamaño: $([math]::Round($backupSize, 2)) MB" -ForegroundColor Gray
    Write-Host ""

    # Rotación de backups antiguos
    Write-Host "🔄 Rotación de backups antiguos..." -ForegroundColor Yellow
    $allBackups = Get-ChildItem -Path $BackupBasePath -Directory -Filter "uploads_*" |
                  Sort-Object Name -Descending

    if ($allBackups.Count -gt $KeepLastBackups) {
        $backupsToDelete = $allBackups | Select-Object -Skip $KeepLastBackups

        Write-Host "   Eliminando $($backupsToDelete.Count) backup(s) antiguo(s):" -ForegroundColor Gray
        foreach ($backup in $backupsToDelete) {
            Remove-Item -Path $backup.FullName -Recurse -Force
            Write-Host "   🗑️  Eliminado: $($backup.Name)" -ForegroundColor DarkGray
        }
    }

    Write-Host "   Backups actuales: $([math]::Min($allBackups.Count, $KeepLastBackups))/$KeepLastBackups" -ForegroundColor Gray
    Write-Host ""

    # Resumen
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  ✅ BACKUP COMPLETADO" -ForegroundColor Green
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "📁 Backup guardado en:" -ForegroundColor White
    Write-Host "   $backupDestinationPath" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "💡 Recuperación de backup:" -ForegroundColor White
    Write-Host "   En caso de pérdida de datos, copie el contenido de la carpeta" -ForegroundColor Gray
    Write-Host "   de backup a: $uploadsSourcePath" -ForegroundColor Gray
    Write-Host ""

    exit 0
}
catch {
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Red
    Write-Host "  ❌ ERROR EN BACKUP" -ForegroundColor Red
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Red
    Write-Host ""
    Write-Error "Error al crear backup: $($_.Exception.Message)"
    Write-Host ""
    Write-Host "⚠️  ADVERTENCIA: El backup falló. ¿Desea continuar con el deployment?" -ForegroundColor Yellow
    Write-Host "   Presione 'S' para continuar (RIESGOSO) o cualquier otra tecla para cancelar." -ForegroundColor Yellow

    $response = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    if ($response.Character -eq 'S' -or $response.Character -eq 's') {
        Write-Host ""
        Write-Host "⚠️  Continuando SIN backup..." -ForegroundColor Yellow
        exit 0
    } else {
        Write-Host ""
        Write-Host "❌ Deployment cancelado por el usuario." -ForegroundColor Red
        exit 1
    }
}
