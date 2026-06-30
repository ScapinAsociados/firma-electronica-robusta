# Arquitectura inicial

La solucion esta organizada por capas:

- `FirmaElectronica.Domain`: entidades, estados y nombres de eventos.
- `FirmaElectronica.Application`: contratos, DTOs e interfaces de servicios.
- `FirmaElectronica.Infrastructure`: Entity Framework Core, storage local, hash SHA-256, generacion de tokens y servicios concretos.
- `FirmaElectronica.Api`: endpoints HTTP.
- `FirmaElectronica.Tests`: pruebas unitarias iniciales.

La API depende de Application e Infrastructure. Application no depende de Infrastructure.

El almacenamiento de archivos queda abstraido por `IFileStorage`; la implementacion local `FileSystemStorage` es solo para desarrollo.

Los tokens de firma se generan con bytes aleatorios criptograficos. El token plano solo se usa para armar la URL de respuesta y no se persiste.

Los endpoints pensados para Microsoft Access requieren `X-API-Key`. La key se hashea con SHA-256 y se compara contra `UsuariosApi.ApiKeyHash`; el request queda asociado a la empresa del usuario API, no a un `idEmpresa` enviado por el cliente.

En esta iteracion, el estado `firmado` representa aceptacion electronica confirmada. La generacion del PDF final firmado y la constancia queda preparada para una etapa posterior mediante los campos `HashFirmado`, `RutaPdfFirmado` y `RutaConstancia`.

El portal local de firma se sirve desde `/f/{token}` como HTML/CSS/JavaScript estatico. No requiere login del firmante: el token seguro es el factor de acceso. El portal usa los endpoints `/api/firmas/{token}/validar`, `/api/firmas/{token}/documento`, `/api/firmas/{token}/pdf` y `/api/firmas/{token}/aceptar`.

El portal captura una evidencia visual en canvas compatible con mouse y pantalla tactil. En escritorio se muestra firma manuscrita. En dispositivos moviles se ofrece elegir entre firma manuscrita y huella visual. El modo firma exige trazo real; el modo huella genera una marca visual de mayor resolucion con crestas, textura y datos de contacto cuando el navegador informa radio/presion. La imagen se envia como PNG Base64 en la confirmacion, se guarda mediante `IFileStorage` en el contenedor `firmas` y la ruta queda asociada al firmante en `Firmantes.RutaFirmaImagen`.

Importante: los navegadores comunes no permiten extraer la huella biometrica real. El modo `huella` actual captura una marca visual declarativa en canvas. Una validacion biometrica real requeriria integrar WebAuthn/passkeys o un proveedor/dispositivo biometrico compatible.
