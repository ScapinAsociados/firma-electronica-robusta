# Modelo de datos inicial

Entidades incluidas:

- `Empresa`
- `Documento`
- `Firmante`
- `SolicitudFirma`
- `EventoAuditoria`
- `UsuarioApi`

Relaciones principales:

- Una empresa tiene muchos documentos y usuarios API.
- Un documento tiene un firmante inicial, solicitudes de firma y eventos de auditoria.
- Una solicitud de firma referencia un documento y un firmante.
- Un evento de auditoria referencia siempre un documento y opcionalmente un firmante.

`SolicitudesFirma.TokenHash` guarda solo el SHA-256 del token de firma.

`Firmantes.RutaFirmaImagen` guarda la ruta de la imagen PNG capturada desde el canvas del portal, cuando el firmante dibuja una firma manuscrita.
