# Flujo de pruebas desde Microsoft Access VBA

Este flujo esta pensado para las primeras pruebas locales contra:

```text
http://localhost:5186
```

API key demo:

```text
dev-firma-electronica-api-key
```

## 1. Preparar Access

Crear o usar una tabla de comprobantes con estos campos sugeridos:

| Campo | Tipo sugerido |
| --- | --- |
| `IdFirmaWeb` | Texto corto |
| `EstadoFirma` | Texto corto |
| `UrlFirma` | Texto largo |
| `FechaEnvioFirma` | Fecha/Hora |
| `FechaFirma` | Fecha/Hora |
| `HashOriginal` | Texto corto |
| `UltimoErrorFirma` | Texto largo |

Importar el modulo:

```text
docs/access/FirmaElectronicaApi.bas
```

En el editor VBA de Access:

1. Abrir `ALT + F11`.
2. Menu `File` / `Import File`.
3. Seleccionar `FirmaElectronicaApi.bas`.

El modulo usa late binding, por lo que no hace falta marcar referencias manuales.

## 2. Levantar la API local

Desde la raiz del repo:

```bash
dotnet run --project src/FirmaElectronica.Api --launch-profile http
```

Debe quedar disponible:

```text
http://localhost:5186
```

## 3. Subir un PDF desde VBA

Ejemplo desde una ventana Immediate o desde un boton de Access:

```vb
Sub ProbarSubidaFirma()
    Dim r As FirmaCrearDocumentoResult

    r = CrearSolicitudFirmaPdf( _
        "C:\Temp\comprobante.pdf", _
        "comprobante", _
        "Cliente Demo", _
        "cliente@example.com", _
        "20123456789", _
        "", _
        "ACCESS-DEMO", _
        "COMP-0001", _
        "Access Local")

    If r.Ok Then
        Debug.Print "IdDocumento: "; r.IdDocumento
        Debug.Print "Estado: "; r.Estado
        Debug.Print "UrlFirma: "; r.UrlFirma
        Debug.Print "HashOriginal: "; r.HashOriginal

        ' Ejemplo de guardado:
        ' CurrentDb.Execute "UPDATE Comprobantes SET IdFirmaWeb='" & r.IdDocumento & "', EstadoFirma='" & r.Estado & "', UrlFirma='" & r.UrlFirma & "' WHERE Id=1"
    Else
        Debug.Print "Error: "; r.ErrorMessage
    End If
End Sub
```

Abrir `r.UrlFirma` en el navegador. El portal local permite ver el PDF y confirmar la firma electronica.

## 4. Consultar estado desde VBA

```vb
Sub ProbarConsultaEstado()
    Dim r As FirmaEstadoResult

    r = ConsultarEstadoFirma("PEGAR-ID-DOCUMENTO")

    If r.Ok Then
        Debug.Print "Estado: "; r.Estado
        Debug.Print "FechaFirma: "; r.FechaFirma
    Else
        Debug.Print "Error: "; r.ErrorMessage
    End If
End Sub
```

## 5. Descargar imagen de firma

Cuando el estado sea `firmado`, ejecutar:

```vb
Sub ProbarDescargaFirma()
    DescargarFirmaImagenComprobantePrueba 1
End Sub
```

Esto guarda:

- el PNG en el campo `FirmaImagen` de `ComprobantesFirmaPrueba`;
- una copia local en `firmas-descargadas`;
- la ruta local en `FirmaImagenRutaLocal`;
- la fecha en `FechaDescargaFirma`.

Estados esperados durante la prueba:

- `pendiente_firma`: se creo el link.
- `visto`: el firmante abrio/visualizo el PDF desde el portal.
- `firmado`: el firmante confirmo la aceptacion electronica.

## 6. Probar el circuito completo

1. Generar o elegir un PDF local.
2. Ejecutar `CrearSolicitudFirmaPdf`.
3. Guardar `IdDocumento`, `UrlFirma`, `HashOriginal` y `Estado`.
4. Abrir `UrlFirma` en navegador.
5. Marcar aceptacion y presionar `Firmar electronicamente`.
6. Ejecutar `ConsultarEstadoFirma`.
7. Verificar que `Estado` sea `firmado`.
8. Ejecutar `DescargarFirmaImagenComprobantePrueba 1`.

## 7. Notas importantes

- Access nunca envia `idEmpresa`; la empresa se resuelve desde `X-API-Key`.
- La API key viaja en el header `X-API-Key`.
- El token plano de firma solo aparece en `UrlFirma`; en la base se guarda su hash.
- En esta etapa `firmado` significa aceptacion electronica confirmada. El PDF final firmado y la constancia se implementaran despues.
