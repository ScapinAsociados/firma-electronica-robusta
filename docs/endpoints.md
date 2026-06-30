# Endpoints iniciales

## POST /api/documentos

Crea un documento pendiente de firma electronica, almacena el PDF original, calcula su hash SHA-256, crea la solicitud de firma y devuelve una URL segura.

Requiere header:

```http
X-API-Key: dev-firma-electronica-api-key
```

La empresa se obtiene desde el usuario API autenticado. El cliente no debe enviar `idEmpresa` como fuente de autorizacion.

Formato: `multipart/form-data`

Campos:

| Campo | Tipo | Requerido | Descripcion |
| --- | --- | --- | --- |
| `idSistemaOrigen` | `string` | No | Identificador del sistema Access u origen. |
| `idComprobanteOrigen` | `string` | No | Identificador del comprobante en el sistema origen. |
| `tipoDocumento` | `string` | Si | Tipo de comprobante o documento. |
| `nombreFirmante` | `string` | Si | Nombre del firmante. |
| `cuitDniFirmante` | `string` | No | CUIT, CUIL o DNI informado. |
| `emailFirmante` | `string` | Si | Email del firmante. |
| `celularFirmante` | `string` | No | Celular del firmante. |
| `creadoPor` | `string` | No | Usuario o sistema que crea la solicitud. |
| `archivo` | `file` | Si | PDF original. Solo se acepta `application/pdf`, extension `.pdf` y cabecera `%PDF-`. |

Eventos de auditoria registrados:

- `documento_creado`
- `pdf_subido`
- `hash_calculado`
- `link_generado`

Respuesta `201 Created`:

```json
{
  "idDocumento": "2f9a67ed-0ed6-457b-8276-a5fd0487cb9f",
  "estado": "pendiente_firma",
  "urlFirma": "http://localhost:5186/f/token-seguro",
  "hashOriginal": "hash-sha-256-en-hexadecimal",
  "fechaVencimiento": "2026-07-03T14:30:00Z"
}
```

Notas de seguridad:

- El token plano se devuelve una sola vez dentro de `urlFirma`.
- En base de datos se guarda solamente `TokenHash`.
- El PDF original se guarda mediante `IFileStorage`; en desarrollo se usa FileSystem.

## GET /api/documentos/{id}/estado

Consulta el estado actual de un documento.

Respuesta `200 OK`:

```json
{
  "idDocumento": "2f9a67ed-0ed6-457b-8276-a5fd0487cb9f",
  "estado": "pendiente_firma",
  "fechaAlta": "2026-06-30T14:30:00Z",
  "fechaVencimiento": "2026-07-03T14:30:00Z",
  "fechaFirma": null,
  "ultimoError": null
}
```

Respuesta `404 Not Found`: el documento no existe.

Respuesta `401 Unauthorized`: API key ausente o invalida.

## GET /api/documentos/{id}/firma-imagen

Descarga la imagen PNG de la firma manuscrita capturada para un documento ya firmado.

Requiere header:

```http
X-API-Key: dev-firma-electronica-api-key
```

Respuesta `200 OK`:

```http
Content-Type: image/png
Content-Disposition: attachment; filename="{idDocumento}-{idFirmante}.png"
```

Respuestas:

- `404 Not Found`: el documento no existe para la empresa autenticada o todavia no tiene imagen de firma.
- `401 Unauthorized`: API key ausente o invalida.

## GET /f/{token}

Muestra el portal local de firma electronica. La pagina consume los endpoints publicos por token para validar el enlace, mostrar datos, embeber el PDF original y confirmar la aceptacion.

Respuesta `200 OK`:

```http
Content-Type: text/html; charset=utf-8
```

## GET /api/firmas/{token}/validar

Valida un link de firma para el portal o pruebas API. El token recibido se hashea con SHA-256 y se compara contra `SolicitudesFirma.TokenHash`; el token plano no se persiste.

Si el token es valido y no vencio, registra el evento `link_abierto`.

Respuesta `200 OK`:

```json
{
  "status": "valido",
  "idDocumento": "2f9a67ed-0ed6-457b-8276-a5fd0487cb9f",
  "estadoDocumento": "pendiente_firma",
  "fechaVencimiento": "2026-07-03T14:30:00Z"
}
```

Errores:

- `404 Not Found`: token inexistente.
- `400 Bad Request`: token vencido.
- `409 Conflict`: token no disponible.

## GET /api/firmas/{token}/documento

Devuelve datos basicos del documento y firmante asociados al token, sin exponer `TokenHash` ni rutas internas de storage.

No entrega el PDF ni registra visualizacion del archivo. La visualizacion real se registra en `GET /api/firmas/{token}/pdf`.

Respuesta `200 OK`:

```json
{
  "idDocumento": "2f9a67ed-0ed6-457b-8276-a5fd0487cb9f",
  "estado": "pendiente_firma",
  "tipoDocumento": "comprobante",
  "nombreArchivoOriginal": "comprobante.pdf",
  "hashOriginal": "hash-sha-256-en-hexadecimal",
  "fechaVencimiento": "2026-07-03T14:30:00Z",
  "firmante": {
    "nombre": "Cliente Demo",
    "cuitDni": "20123456789",
    "email": "cliente@example.com",
    "celular": null,
    "estadoFirma": "pendiente"
  }
}
```

## GET /api/firmas/{token}/pdf

Entrega el PDF original asociado al token de firma. Si el token es valido, sirve el archivo desde `IFileStorage`, registra `pdf_visualizado`, setea `Firmantes.FechaVista` si todavia estaba vacia y pasa el documento de `pendiente_firma` a `visto`.

Respuesta `200 OK`:

```http
Content-Type: application/pdf
Content-Disposition: attachment; filename="comprobante.pdf"
```

Errores:

- `404 Not Found`: token inexistente.
- `400 Bad Request`: token vencido o documento no disponible.
- `409 Conflict`: token ya utilizado u otro conflicto de estado.

## POST /api/firmas/{token}/aceptar

Confirma la aceptacion expresa de firma electronica. En esta iteracion, `firmado` significa que la aceptacion electronica fue confirmada; mas adelante se completaran `HashFirmado`, `RutaPdfFirmado` y la constancia cuando exista la generacion final del PDF firmado.

Body:

```json
{
  "aceptaTerminos": true,
  "textoAceptado": "Acepto firmar electronicamente el documento.",
  "nombreConfirmado": "Cliente Demo",
  "cuitDniConfirmado": "20123456789",
  "firmaImagenBase64": "data:image/png;base64,...",
  "firmaMetodo": "firma",
  "firmaPuntosCapturados": 8
}
```

Efectos:

- `Documentos.Estado` pasa a `firmado`.
- `Documentos.FechaFirma` se establece en UTC.
- `Firmantes.EstadoFirma` pasa a `firmado`.
- `firmaImagenBase64` es obligatorio. La imagen PNG se guarda en storage y su ruta queda en `Firmantes.RutaFirmaImagen`.
- `firmaMetodo` puede ser `firma` o `huella`; se registra en auditoria.
- `firmaPuntosCapturados` debe ser mayor a cero. En el portal, el modo firma requiere trazo real y el modo huella requiere apoyar el dedo en el recuadro.
- `SolicitudesFirma.FechaUso` se establece en UTC.
- Se registran `terminos_aceptados` y `firma_confirmada`.

Respuesta `200 OK`:

```json
{
  "idDocumento": "2f9a67ed-0ed6-457b-8276-a5fd0487cb9f",
  "estado": "firmado",
  "fechaFirma": "2026-06-30T15:10:00Z"
}
```

## Datos demo en desarrollo

En ambiente `Development`, al iniciar la API se aplican migraciones y se crea una empresa demo si no existe:

```text
idEmpresa: 11111111-1111-1111-1111-111111111111
razonSocial: Empresa Demo
apiKey: dev-firma-electronica-api-key
```

La API key demo se guarda hasheada en `UsuariosApi.ApiKeyHash`.
