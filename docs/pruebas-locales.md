# Pruebas locales

## Ejecutar la API

Desde la raiz del repo:

```bash
dotnet run --project src/FirmaElectronica.Api
```

En ambiente `Development`, la API aplica migraciones y crea datos demo:

```text
idEmpresa: 11111111-1111-1111-1111-111111111111
apiKey: dev-firma-electronica-api-key
```

La API key demo se guarda hasheada en la tabla `UsuariosApi`.

Para facilitar las primeras pruebas locales, `appsettings.Development.json` usa EF Core InMemory. SQL Server queda configurado como proveedor por defecto para entornos no locales y para aplicar migraciones reales.

## Subir un PDF

Usar `POST /api/documentos` con `multipart/form-data` y header:

```http
X-API-Key: dev-firma-electronica-api-key
```

Campos minimos:

```text
tipoDocumento=comprobante
nombreFirmante=Cliente Demo
emailFirmante=cliente@example.com
archivo=@documento.pdf
```

La respuesta incluye `urlFirma`, por ejemplo:

```json
{
  "idDocumento": "2f9a67ed-0ed6-457b-8276-a5fd0487cb9f",
  "estado": "pendiente_firma",
  "urlFirma": "http://localhost:5186/f/token-seguro"
}
```

## Firmar desde el portal local

Abrir `urlFirma` en el navegador.

El portal:

- valida el token con `GET /api/firmas/{token}/validar`;
- carga datos con `GET /api/firmas/{token}/documento`;
- muestra el PDF desde `GET /api/firmas/{token}/pdf`;
- confirma la aceptacion con `POST /api/firmas/{token}/aceptar`.

Cuando se entrega el PDF, el documento pasa a `visto` y se registra `pdf_visualizado`. Cuando se confirma la aceptacion, el documento pasa a `firmado`.

## Consultar estado

```http
GET /api/documentos/{id}/estado
X-API-Key: dev-firma-electronica-api-key
```

En esta etapa, `firmado` significa aceptacion electronica confirmada. El PDF final firmado y la constancia se completaran en una iteracion posterior.

## Pruebas desde Microsoft Access

Ver guia y modulo VBA en:

```text
docs/access-vba-flujo-pruebas.md
docs/access/FirmaElectronicaApi.bas
```
