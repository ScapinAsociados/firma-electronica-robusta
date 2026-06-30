# AGENTS.md

## Proyecto

Plataforma de Firma Electrónica Robusta integrada con sistemas Microsoft Access.

## Objetivo

Construir una API REST y portal web responsive para que documentos PDF generados por sistemas Access puedan ser enviados a clientes para su firma electrónica.

## Reglas generales

* Priorizar código claro, mantenible y seguro.
* No implementar soluciones simuladas que comprometan seguridad.
* No guardar tokens planos en base de datos.
* No guardar claves ni secretos en código fuente.
* No llamar “firma digital” a este flujo; usar “firma electrónica”.
* Mantener separación por capas.
* Documentar endpoints nuevos.
* Agregar pruebas cuando se implemente lógica de negocio relevante.

## Stack preferido

* .NET / ASP.NET Core.
* C#.
* SQL Server / Azure SQL.
* Entity Framework Core.
* Azure Blob Storage en producción.
* FileSystem Storage solo para desarrollo local.
* Frontend responsive compatible con celulares.

## Arquitectura sugerida

Usar capas:

```text
Domain
Application
Infrastructure
Api
Web
Tests
```

## Seguridad obligatoria

* HTTPS en producción.
* Hash SHA-256 de documentos.
* Tokens seguros y con vencimiento.
* Hash de tokens en base de datos.
* Auditoría de eventos.
* Registro de IP y User Agent.
* Validación estricta de PDFs.
* Manejo seguro de errores.

## Estados de documento

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

## Primer objetivo de desarrollo

Crear el esqueleto funcional del MVP:

* Subida de PDF.
* Creación de documento.
* Cálculo de hash.
* Generación de solicitud de firma.
* Generación de link seguro.
* Consulta de estado.
* Persistencia en base de datos.
* Almacenamiento local abstracto.
* Auditoría inicial.

## No hacer todavía

* WhatsApp API.
* SMS OTP.
* Firma digital certificada.
* Biometría.
* Múltiples firmantes.
* Panel administrativo avanzado.
