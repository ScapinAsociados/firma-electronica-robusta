Attribute VB_Name = "FirmaElectronicaPruebas"
Option Compare Database
Option Explicit

Private Const TABLA_COMPROBANTES As String = "ComprobantesFirmaPrueba"
Private Const TABLA_LOG As String = "FirmaElectronicaLogPrueba"
Private Const DB_OPEN_DYNASET As Long = 2
Private Const DB_OPEN_SNAPSHOT As Long = 4
Private Const DB_FAIL_ON_ERROR As Long = 128

Public Sub EnviarComprobanteFirmaPrueba(ByVal idComprobante As Long)
    On Error GoTo EH

    Dim db As Object
    Dim rs As Object
    Dim r As FirmaCrearDocumentoResult

    Set db = CurrentDb
    Set rs = db.OpenRecordset("SELECT * FROM " & TABLA_COMPROBANTES & " WHERE Id=" & CStr(idComprobante), DB_OPEN_DYNASET)

    If rs.EOF Then
        MsgBox "No existe el comprobante de prueba Id=" & CStr(idComprobante), vbExclamation
        GoTo CleanUp
    End If

    r = CrearSolicitudFirmaPdf( _
        Nz(rs!RutaPdf, ""), _
        Nz(rs!TipoDocumento, "comprobante"), _
        Nz(rs!NombreFirmante, ""), _
        Nz(rs!EmailFirmante, ""), _
        Nz(rs!CuitDniFirmante, ""), _
        Nz(rs!CelularFirmante, ""), _
        Nz(rs!IdSistemaOrigen, "ACCESS"), _
        Nz(rs!IdComprobanteOrigen, CStr(idComprobante)), _
        "Access Pruebas.accdb")

    rs.Edit
    rs!UltimoErrorFirma = Null
    If r.Ok Then
        rs!IdFirmaWeb = r.IdDocumento
        rs!EstadoFirma = r.Estado
        rs!UrlFirma = NormalizarUrlFirmaLocal(r.UrlFirma)
        rs!FechaEnvioFirma = Now
        rs!HashOriginal = r.HashOriginal
        RegistrarLog "crear_documento", idComprobante, r.IdDocumento, r.Estado, "Documento enviado correctamente.", r.RawResponse
        MsgBox "Documento enviado. Se genero la URL de firma.", vbInformation
    Else
        rs!UltimoErrorFirma = r.ErrorMessage
        RegistrarLog "crear_documento_error", idComprobante, "", "", r.ErrorMessage, r.RawResponse
        MsgBox "No se pudo enviar el documento: " & r.ErrorMessage, vbExclamation
    End If
    rs.Update

CleanUp:
    On Error Resume Next
    rs.Close
    Set rs = Nothing
    Set db = Nothing
    Exit Sub

EH:
    RegistrarLog "crear_documento_excepcion", idComprobante, "", "", Err.Number & " - " & Err.Description, ""
    MsgBox Err.Number & " - " & Err.Description, vbCritical
    Resume CleanUp
End Sub

Public Sub ConsultarEstadoComprobanteFirmaPrueba(ByVal idComprobante As Long)
    On Error GoTo EH

    Dim db As Object
    Dim rs As Object
    Dim r As FirmaEstadoResult

    Set db = CurrentDb
    Set rs = db.OpenRecordset("SELECT * FROM " & TABLA_COMPROBANTES & " WHERE Id=" & CStr(idComprobante), DB_OPEN_DYNASET)

    If rs.EOF Then
        MsgBox "No existe el comprobante de prueba Id=" & CStr(idComprobante), vbExclamation
        GoTo CleanUp
    End If

    If Len(Nz(rs!IdFirmaWeb, "")) = 0 Then
        MsgBox "El comprobante todavia no tiene IdFirmaWeb.", vbExclamation
        GoTo CleanUp
    End If

    r = ConsultarEstadoFirma(Nz(rs!IdFirmaWeb, ""))

    rs.Edit
    If r.Ok Then
        rs!EstadoFirma = r.Estado
        rs!UltimoErrorFirma = Nz(r.UltimoError, "")
        If Len(r.FechaFirma) > 0 Then rs!FechaFirma = CDate(Replace(Left$(r.FechaFirma, 19), "T", " "))
        RegistrarLog "consultar_estado", idComprobante, r.IdDocumento, r.Estado, "Estado actualizado.", r.RawResponse
        MsgBox "Estado actual: " & r.Estado, vbInformation
    Else
        rs!UltimoErrorFirma = r.ErrorMessage
        RegistrarLog "consultar_estado_error", idComprobante, Nz(rs!IdFirmaWeb, ""), "", r.ErrorMessage, r.RawResponse
        MsgBox "No se pudo consultar el estado: " & r.ErrorMessage, vbExclamation
    End If
    rs.Update

CleanUp:
    On Error Resume Next
    rs.Close
    Set rs = Nothing
    Set db = Nothing
    Exit Sub

EH:
    RegistrarLog "consultar_estado_excepcion", idComprobante, "", "", Err.Number & " - " & Err.Description, ""
    MsgBox Err.Number & " - " & Err.Description, vbCritical
    Resume CleanUp
End Sub

Public Sub AbrirUrlFirmaComprobantePrueba(ByVal idComprobante As Long)
    On Error GoTo EH

    Dim rs As Object
    Set rs = CurrentDb.OpenRecordset("SELECT UrlFirma FROM " & TABLA_COMPROBANTES & " WHERE Id=" & CStr(idComprobante), DB_OPEN_SNAPSHOT)

    If rs.EOF Or Len(Nz(rs!UrlFirma, "")) = 0 Then
        MsgBox "El comprobante no tiene URL de firma.", vbExclamation
    Else
        AbrirUrlFirma Nz(rs!UrlFirma, "")
    End If

CleanUp:
    On Error Resume Next
    rs.Close
    Set rs = Nothing
    Exit Sub

EH:
    MsgBox Err.Number & " - " & Err.Description, vbCritical
    Resume CleanUp
End Sub

Public Sub DescargarFirmaImagenComprobantePrueba(ByVal idComprobante As Long)
    On Error GoTo EH

    Dim db As Object
    Dim rs As Object
    Dim bytes() As Byte
    Dim ok As Boolean
    Dim statusCode As Long
    Dim errorMessage As String
    Dim outputDir As String
    Dim outputPath As String

    Set db = CurrentDb
    Set rs = db.OpenRecordset("SELECT * FROM " & TABLA_COMPROBANTES & " WHERE Id=" & CStr(idComprobante), DB_OPEN_DYNASET)

    If rs.EOF Then
        MsgBox "No existe el comprobante de prueba Id=" & CStr(idComprobante), vbExclamation
        GoTo CleanUp
    End If

    If Len(Nz(rs!IdFirmaWeb, "")) = 0 Then
        MsgBox "El comprobante todavia no tiene IdFirmaWeb.", vbExclamation
        GoTo CleanUp
    End If

    ok = DescargarFirmaImagenBytes(Nz(rs!IdFirmaWeb, ""), bytes, statusCode, errorMessage)

    rs.Edit
    If ok Then
        outputDir = CurrentProject.Path & "\firmas-descargadas"
        EnsureFolder outputDir
        outputPath = outputDir & "\" & Nz(rs!IdFirmaWeb, "") & ".png"
        GuardarBytesEnArchivo bytes, outputPath

        rs!FirmaImagen.AppendChunk bytes
        rs!FirmaImagenRutaLocal = outputPath
        rs!FechaDescargaFirma = Now
        rs!UltimoErrorFirma = Null
        RegistrarLog "descargar_firma_imagen", idComprobante, Nz(rs!IdFirmaWeb, ""), Nz(rs!EstadoFirma, ""), "Imagen de firma descargada.", outputPath
        MsgBox "Imagen de firma descargada y guardada en la tabla.", vbInformation
    Else
        rs!UltimoErrorFirma = errorMessage
        RegistrarLog "descargar_firma_imagen_error", idComprobante, Nz(rs!IdFirmaWeb, ""), Nz(rs!EstadoFirma, ""), errorMessage, ""
        MsgBox "No se pudo descargar la firma: " & errorMessage, vbExclamation
    End If
    rs.Update

CleanUp:
    On Error Resume Next
    rs.Close
    Set rs = Nothing
    Set db = Nothing
    Exit Sub

EH:
    RegistrarLog "descargar_firma_imagen_excepcion", idComprobante, "", "", Err.Number & " - " & Err.Description, ""
    MsgBox Err.Number & " - " & Err.Description, vbCritical
    Resume CleanUp
End Sub

Public Sub ProbarFlujoCompletoConPrimerRegistro()
    EnviarComprobanteFirmaPrueba 1
    AbrirUrlFirmaComprobantePrueba 1
End Sub

Private Sub EnsureFolder(ByVal folderPath As String)
    If Len(Dir$(folderPath, vbDirectory)) = 0 Then
        MkDir folderPath
    End If
End Sub

Private Sub RegistrarLog( _
    ByVal operacion As String, _
    ByVal idComprobante As Long, _
    ByVal idDocumento As String, _
    ByVal estado As String, _
    ByVal mensaje As String, _
    ByVal rawResponse As String)

    On Error Resume Next

    Dim db As Object
    Dim sql As String

    Set db = CurrentDb
    sql = "INSERT INTO " & TABLA_LOG & " (FechaHora, Operacion, IdComprobante, IdDocumento, Estado, Mensaje, RawResponse) VALUES (" & _
        "#" & Format$(Now, "yyyy-mm-dd hh:nn:ss") & "#, " & _
        SqlText(operacion) & ", " & _
        CStr(idComprobante) & ", " & _
        SqlText(idDocumento) & ", " & _
        SqlText(estado) & ", " & _
        SqlText(mensaje) & ", " & _
        SqlText(rawResponse) & ")"

    db.Execute sql, DB_FAIL_ON_ERROR
End Sub

Private Function SqlText(ByVal value As String) As String
    If Len(value) = 0 Then
        SqlText = "Null"
    Else
        SqlText = "'" & Replace(value, "'", "''") & "'"
    End If
End Function
