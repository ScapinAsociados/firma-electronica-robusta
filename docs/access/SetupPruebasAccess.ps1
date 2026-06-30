$ErrorActionPreference = "Stop"

$dbPath = Join-Path $PSScriptRoot "Pruebas.accdb"
$apiModulePath = Join-Path $PSScriptRoot "FirmaElectronicaApi.bas"
$testModulePath = Join-Path $PSScriptRoot "FirmaElectronicaPruebas.bas"
$backupPath = Join-Path $PSScriptRoot ("Pruebas.backup." + (Get-Date -Format "yyyyMMddHHmmss") + ".accdb")

Copy-Item -LiteralPath $dbPath -Destination $backupPath -Force

$access = New-Object -ComObject Access.Application
$access.Visible = $false

try {
    $access.OpenCurrentDatabase($dbPath)
    $access.DoCmd.SetWarnings($false)

    function Invoke-AccessSql([string] $sql, [switch] $IgnoreErrors) {
        try {
            $access.DoCmd.RunSQL($sql)
        } catch {
            if (-not $IgnoreErrors) {
                throw
            }
        }
    }

    Invoke-AccessSql @"
CREATE TABLE ComprobantesFirmaPrueba (
    Id COUNTER CONSTRAINT PrimaryKey PRIMARY KEY,
    IdSistemaOrigen TEXT(50),
    IdComprobanteOrigen TEXT(100),
    TipoDocumento TEXT(100),
    RutaPdf LONGTEXT,
    NombreFirmante TEXT(200),
    CuitDniFirmante TEXT(30),
    EmailFirmante TEXT(255),
    CelularFirmante TEXT(40),
    IdFirmaWeb TEXT(80),
    EstadoFirma TEXT(40),
    UrlFirma LONGTEXT,
    FechaEnvioFirma DATETIME,
    FechaFirma DATETIME,
    HashOriginal TEXT(64),
    UltimoErrorFirma LONGTEXT
)
"@ -IgnoreErrors

    Invoke-AccessSql @"
CREATE TABLE FirmaElectronicaLogPrueba (
    Id COUNTER CONSTRAINT PrimaryKey PRIMARY KEY,
    FechaHora DATETIME,
    Operacion TEXT(80),
    IdComprobante LONG,
    IdDocumento TEXT(80),
    Estado TEXT(40),
    Mensaje LONGTEXT,
    RawResponse LONGTEXT
)
"@ -IgnoreErrors

    Invoke-AccessSql @"
UPDATE ComprobantesFirmaPrueba
SET UrlFirma = Replace(Replace(UrlFirma, 'https://localhost:5001', 'http://localhost:5186'), 'http://localhost:5001', 'http://localhost:5186')
WHERE UrlFirma Is Not Null
"@ -IgnoreErrors

    Invoke-AccessSql "ALTER TABLE ComprobantesFirmaPrueba ADD COLUMN FirmaImagen LONGBINARY" -IgnoreErrors
    Invoke-AccessSql "ALTER TABLE ComprobantesFirmaPrueba ADD COLUMN FirmaImagenRutaLocal LONGTEXT" -IgnoreErrors
    Invoke-AccessSql "ALTER TABLE ComprobantesFirmaPrueba ADD COLUMN FechaDescargaFirma DATETIME" -IgnoreErrors

    Invoke-AccessSql @"
INSERT INTO ComprobantesFirmaPrueba (
    IdSistemaOrigen,
    IdComprobanteOrigen,
    TipoDocumento,
    RutaPdf,
    NombreFirmante,
    CuitDniFirmante,
    EmailFirmante,
    CelularFirmante,
    EstadoFirma
)
SELECT
    'ACCESS-DEMO',
    'COMP-0001',
    'comprobante',
    'C:\Temp\comprobante.pdf',
    'Cliente Demo',
    '20123456789',
    'cliente@example.com',
    '',
    'pendiente_envio'
WHERE NOT EXISTS (
    SELECT * FROM ComprobantesFirmaPrueba WHERE IdComprobanteOrigen='COMP-0001'
)
"@ -IgnoreErrors

    foreach ($moduleName in @("FirmaElectronicaApi", "FirmaElectronicaPruebas")) {
        try {
            $access.DoCmd.DeleteObject(5, $moduleName)
        } catch {
        }
    }

    $access.LoadFromText(5, "FirmaElectronicaApi", $apiModulePath)
    $access.LoadFromText(5, "FirmaElectronicaPruebas", $testModulePath)

    $access.DoCmd.SetWarnings($true)
    $access.CloseCurrentDatabase()

    Write-Host "OK"
    Write-Host "Backup: $backupPath"
} finally {
    try {
        $access.DoCmd.SetWarnings($true)
    } catch {
    }
    $access.Quit()
    [System.Runtime.InteropServices.Marshal]::ReleaseComObject($access) | Out-Null
}
