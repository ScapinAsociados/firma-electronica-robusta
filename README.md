# Proyecto: Plataforma de Firma Electrónica Robusta

## 1. Objetivo general

Desarrollar una plataforma web/API para firma electrónica robusta de documentos PDF generados por sistemas existentes en Microsoft Access.

Los sistemas Access generan comprobantes en PDF. La plataforma debe permitir que Access suba esos PDFs a una API web, genere una solicitud de firma, envíe o devuelva un enlace seguro para el cliente, y permita que el cliente firme desde una página web compatible con celulares.

La solución debe incluir:

* API REST.
* Portal web responsive/mobile.
* Base de datos.
* Almacenamiento de PDFs.
* Auditoría completa del proceso de firma.
* Integración simple desde Microsoft Access/VBA.
* Capacidad futura para multiempresa y posible integración posterior con proveedores de firma digital real.

## 2. Alcance funcional inicial

### Flujo principal

1. Un sistema Microsoft Access genera un PDF.
2. Access llama a la API y sube el PDF junto con los datos del cliente/firmante.
3. La API guarda el PDF original.
4. La API calcula hash SHA-256 del PDF original.
5. La API crea una solicitud de firma.
6. La API genera un enlace seguro, único y con vencimiento.
7. El cliente abre el enlace desde celular o PC.
8. El portal muestra el documento PDF.
9. El cliente valida datos mínimos de identidad.
10. El cliente acepta expresamente los términos de firma electrónica.
11. El cliente firma mediante botón de aceptación y, opcionalmente, trazo táctil.
12. El sistema registra toda la auditoría.
13. El sistema genera un PDF final firmado electrónicamente.
14. El sistema genera una constancia de auditoría.
15. Access puede consultar el estado y descargar el PDF firmado y la constancia.

## 3. Tecnología sugerida

Backend:

* ASP.NET Core Web API.
* C#.
* Entity Framework Core.
* Autenticación para API mediante API Key o Bearer Token.

Frontend:

* Web responsive compatible con celulares.
* Puede ser Razor Pages, Blazor, React o HTML/JavaScript simple.
* Visor PDF embebido.
* Firma táctil opcional usando canvas HTML5.

Base de datos:

* SQL Server / Azure SQL Database.

Archivos:

* Azure Blob Storage o almacenamiento compatible.
* En desarrollo local se puede usar FileSystem Storage abstraído mediante una interfaz.

Infraestructura destino:

* Microsoft Azure.
* Azure App Service para API y portal web.
* Azure SQL Database.
* Azure Blob Storage.
* HTTPS obligatorio.

## 4. Módulos principales

### 4.1 API de documentos

Debe permitir:

* Subir un PDF.
* Crear una solicitud de firma.
* Consultar estado.
* Descargar PDF firmado.
* Descargar constancia.
* Anular una solicitud.
* Reenviar o regenerar enlace, si corresponde.

Endpoints mínimos:

```http
POST   /api/documentos
GET    /api/documentos/{id}
GET    /api/documentos/{id}/estado
GET    /api/documentos/{id}/pdf-firmado
GET    /api/documentos/{id}/constancia
POST   /api/documentos/{id}/anular
```

Ejemplo de respuesta al crear documento:

```json
{
  "idDocumento": "DOC-2026-000001",
  "estado": "pendiente_firma",
  "urlFirma": "https://firma.dominio.com/f/abc123"
}
```

### 4.2 Portal de firma

Ruta sugerida:

```http
GET /f/{token}
```

Pantallas mínimas:

1. Acceso por enlace seguro.
2. Validación del token.
3. Vista de datos del documento y firmante.
4. Visualización del PDF.
5. Aceptación expresa de términos.
6. Firma electrónica.
7. Confirmación final.
8. Descarga o visualización del comprobante firmado.

Debe ser totalmente compatible con celulares.

### 4.3 Auditoría

Registrar eventos relevantes del circuito:

* Documento creado.
* PDF subido.
* Hash calculado.
* Link generado.
* Link abierto.
* PDF visualizado.
* Datos del firmante confirmados.
* Términos aceptados.
* Firma confirmada.
* PDF firmado generado.
* Constancia generada.
* Documento descargado.
* Solicitud rechazada.
* Solicitud anulada.
* Solicitud vencida.

Cada evento debe registrar:

* Fecha y hora UTC.
* Tipo de evento.
* IdDocumento.
* IdFirmante, si corresponde.
* IP.
* User Agent.
* Hash del documento en ese momento, cuando aplique.
* Datos adicionales en JSON.

### 4.4 Generación de PDF firmado

El PDF final debe incluir:

* Documento original.
* Leyenda visible de firma electrónica.
* Nombre del firmante.
* CUIT/DNI si fue informado.
* Email del firmante.
* Fecha y hora de firma.
* Código de verificación.
* Firma visual opcional si se usa trazo táctil.

### 4.5 Constancia de auditoría

Generar un PDF adicional con:

* ID del documento.
* Datos del firmante.
* Hash SHA-256 del PDF original.
* Hash SHA-256 del PDF final.
* Fecha/hora de creación.
* Fecha/hora de firma.
* IP.
* User Agent.
* Texto exacto aceptado por el firmante.
* Eventos principales del circuito.
* Código de verificación.

## 5. Estados del documento

Usar estos estados iniciales:

```text
borrador
pendiente_envio
pendiente_firma
visto
firmado
rechazado
vencido
anulado
error
```

## 6. Modelo de datos inicial

### Tabla: Empresas

Campos:

```text
IdEmpresa
RazonSocial
CUIT
Dominio
LogoUrl
ColorPrincipal
Estado
FechaAlta
```

### Tabla: Documentos

Campos:

```text
IdDocumento
IdEmpresa
IdSistemaOrigen
IdComprobanteOrigen
TipoDocumento
NombreArchivoOriginal
HashOriginal
HashFirmado
Estado
FechaAlta
FechaVencimiento
FechaFirma
RutaPdfOriginal
RutaPdfFirmado
RutaConstancia
CreadoPor
UltimoError
```

### Tabla: Firmantes

Campos:

```text
IdFirmante
IdDocumento
Nombre
CUIT_DNI
Email
Celular
OrdenFirma
EstadoFirma
FechaVista
FechaFirma
```

### Tabla: SolicitudesFirma

Campos:

```text
IdSolicitud
IdDocumento
IdFirmante
TokenHash
FechaCreacion
FechaVencimiento
FechaUso
Intentos
Estado
```

Importante: no guardar el token plano en la base de datos. Guardar solamente hash del token.

### Tabla: EventosAuditoria

Campos:

```text
IdEvento
IdDocumento
IdFirmante
FechaHoraUTC
TipoEvento
Descripcion
IP
UserAgent
HashDocumentoEnEvento
DatosJson
```

### Tabla: UsuariosApi

Campos:

```text
IdUsuarioApi
IdEmpresa
Nombre
ApiKeyHash
Permisos
Estado
FechaAlta
UltimoUso
```

## 7. Seguridad mínima requerida

La solución debe implementar:

* HTTPS obligatorio.
* Tokens largos, aleatorios y con vencimiento.
* Hash SHA-256 de PDF original y final.
* Registro de IP y User Agent.
* Registro de fecha/hora UTC.
* API protegida con credenciales.
* No guardar tokens planos.
* No permitir modificar auditoría desde la interfaz común.
* Validar tamaño y tipo de archivo.
* Aceptar solamente PDF.
* Manejo seguro de errores.
* Logs técnicos.
* Preparación para backups.

## 8. Integración con Microsoft Access

Access deberá poder:

* Subir PDF por HTTP POST.
* Enviar datos del cliente.
* Guardar el idDocumento devuelto.
* Guardar la URL de firma.
* Consultar estado.
* Descargar PDF firmado.
* Descargar constancia.

Campos sugeridos para agregar en Access:

```text
IdFirmaWeb
EstadoFirma
UrlFirma
FechaEnvioFirma
FechaFirma
HashOriginal
HashFirmado
RutaPdfFirmado
RutaConstanciaFirma
UltimoErrorFirma
```

## 9. Requisitos del MVP

Primera versión mínima:

1. Crear proyecto ASP.NET Core.
2. Crear base de datos con migraciones.
3. Implementar subida de PDF.
4. Guardar PDF original.
5. Calcular hash SHA-256.
6. Crear solicitud de firma.
7. Generar token seguro.
8. Crear portal `/f/{token}`.
9. Mostrar PDF.
10. Registrar apertura del link.
11. Registrar aceptación de términos.
12. Capturar firma táctil opcional.
13. Confirmar firma.
14. Generar PDF firmado.
15. Generar constancia de auditoría.
16. Permitir consulta de estado por API.
17. Permitir descarga de PDF firmado y constancia.

## 10. Requisitos fuera del MVP

No implementar inicialmente:

* WhatsApp Business API.
* SMS OTP.
* Múltiples firmantes.
* Biometría.
* Integración con firma digital real.
* Panel administrativo avanzado.
* Multiempresa completo con personalización visual avanzada.

Dejar la arquitectura preparada para agregarlo después.

## 11. Entregables esperados de la primera iteración

Crear una solución inicial con:

```text
/src
  /FirmaElectronica.Api
  /FirmaElectronica.Web
  /FirmaElectronica.Application
  /FirmaElectronica.Domain
  /FirmaElectronica.Infrastructure

/tests
  /FirmaElectronica.Tests

/docs
  arquitectura.md
  endpoints.md
  modelo-datos.md
```

También crear:

```text
README.md
AGENTS.md
appsettings.Development.json.example
docker-compose.yml opcional
```

## 12. Primera tarea concreta para Codex

Implementar el esqueleto inicial del proyecto ASP.NET Core para la plataforma de firma electrónica robusta.

Debe incluir:

* Solución .NET organizada por capas.
* Entidades principales.
* DbContext.
* Migración inicial.
* Endpoint `POST /api/documentos`.
* Endpoint `GET /api/documentos/{id}/estado`.
* Servicio para calcular hash SHA-256.
* Servicio de almacenamiento de archivos con interfaz.
* Implementación local en FileSystem para desarrollo.
* Generación de token seguro.
* Guardado de hash del token, no token plano.
* Documentación básica de endpoints.

No implementar todavía el PDF firmado final. Preparar las interfaces para hacerlo en la siguiente iteración.
