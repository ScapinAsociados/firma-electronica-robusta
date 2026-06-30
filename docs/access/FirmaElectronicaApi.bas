Attribute VB_Name = "FirmaElectronicaApi"
Option Compare Database
Option Explicit

Private Const FIRMA_API_BASE_URL As String = "http://localhost:5186"
Private Const FIRMA_API_KEY As String = "dev-firma-electronica-api-key"

Public Type FirmaCrearDocumentoResult
    Ok As Boolean
    StatusCode As Long
    IdDocumento As String
    Estado As String
    UrlFirma As String
    HashOriginal As String
    FechaVencimiento As String
    ErrorMessage As String
    RawResponse As String
End Type

Public Type FirmaEstadoResult
    Ok As Boolean
    StatusCode As Long
    IdDocumento As String
    Estado As String
    FechaAlta As String
    FechaVencimiento As String
    FechaFirma As String
    UltimoError As String
    ErrorMessage As String
    RawResponse As String
End Type

Public Function CrearSolicitudFirmaPdf( _
    ByVal pdfPath As String, _
    ByVal tipoDocumento As String, _
    ByVal nombreFirmante As String, _
    ByVal emailFirmante As String, _
    Optional ByVal cuitDniFirmante As String = "", _
    Optional ByVal celularFirmante As String = "", _
    Optional ByVal idSistemaOrigen As String = "ACCESS", _
    Optional ByVal idComprobanteOrigen As String = "", _
    Optional ByVal creadoPor As String = "Microsoft Access") As FirmaCrearDocumentoResult

    On Error GoTo EH

    Dim result As FirmaCrearDocumentoResult
    Dim boundary As String
    Dim body() As Byte
    Dim http As Object

    If Len(Dir$(pdfPath)) = 0 Then
        result.Ok = False
        result.ErrorMessage = "No existe el PDF: " & pdfPath
        CrearSolicitudFirmaPdf = result
        Exit Function
    End If

    boundary = NewMultipartBoundary()
    body = BuildMultipartBody(boundary, pdfPath, tipoDocumento, nombreFirmante, emailFirmante, cuitDniFirmante, celularFirmante, idSistemaOrigen, idComprobanteOrigen, creadoPor)

    Set http = CreateObject("WinHttp.WinHttpRequest.5.1")
    http.Open "POST", FIRMA_API_BASE_URL & "/api/documentos", False
    http.SetRequestHeader "X-API-Key", FIRMA_API_KEY
    http.SetRequestHeader "Content-Type", "multipart/form-data; boundary=" & boundary
    http.Send body

    result.StatusCode = http.Status
    result.RawResponse = CStr(http.ResponseText)
    result.Ok = (http.Status >= 200 And http.Status < 300)

    If result.Ok Then
        result.IdDocumento = JsonValue(result.RawResponse, "idDocumento")
        result.Estado = JsonValue(result.RawResponse, "estado")
        result.UrlFirma = JsonValue(result.RawResponse, "urlFirma")
        result.HashOriginal = JsonValue(result.RawResponse, "hashOriginal")
        result.FechaVencimiento = JsonValue(result.RawResponse, "fechaVencimiento")
    Else
        result.ErrorMessage = JsonValue(result.RawResponse, "error")
        If Len(result.ErrorMessage) = 0 Then result.ErrorMessage = "HTTP " & CStr(http.Status) & ": " & CStr(http.ResponseText)
    End If

    CrearSolicitudFirmaPdf = result
    Exit Function

EH:
    result.Ok = False
    result.ErrorMessage = Err.Number & " - " & Err.Description
    CrearSolicitudFirmaPdf = result
End Function

Public Function ConsultarEstadoFirma(ByVal idDocumento As String) As FirmaEstadoResult
    On Error GoTo EH

    Dim result As FirmaEstadoResult
    Dim http As Object

    Set http = CreateObject("WinHttp.WinHttpRequest.5.1")
    http.Open "GET", FIRMA_API_BASE_URL & "/api/documentos/" & UrlEncode(idDocumento) & "/estado", False
    http.SetRequestHeader "X-API-Key", FIRMA_API_KEY
    http.Send

    result.StatusCode = http.Status
    result.RawResponse = CStr(http.ResponseText)
    result.Ok = (http.Status >= 200 And http.Status < 300)

    If result.Ok Then
        result.IdDocumento = JsonValue(result.RawResponse, "idDocumento")
        result.Estado = JsonValue(result.RawResponse, "estado")
        result.FechaAlta = JsonValue(result.RawResponse, "fechaAlta")
        result.FechaVencimiento = JsonValue(result.RawResponse, "fechaVencimiento")
        result.FechaFirma = JsonValue(result.RawResponse, "fechaFirma")
        result.UltimoError = JsonValue(result.RawResponse, "ultimoError")
    Else
        result.ErrorMessage = JsonValue(result.RawResponse, "error")
        If Len(result.ErrorMessage) = 0 Then result.ErrorMessage = "HTTP " & CStr(http.Status) & ": " & CStr(http.ResponseText)
    End If

    ConsultarEstadoFirma = result
    Exit Function

EH:
    result.Ok = False
    result.ErrorMessage = Err.Number & " - " & Err.Description
    ConsultarEstadoFirma = result
End Function

Public Sub AbrirUrlFirma(ByVal urlFirma As String)
    If Len(urlFirma) = 0 Then Exit Sub
    Application.FollowHyperlink NormalizarUrlFirmaLocal(urlFirma)
End Sub

Public Function DescargarFirmaImagenBytes( _
    ByVal idDocumento As String, _
    ByRef bytes() As Byte, _
    ByRef statusCode As Long, _
    ByRef errorMessage As String) As Boolean

    On Error GoTo EH

    Dim http As Object
    Set http = CreateObject("WinHttp.WinHttpRequest.5.1")

    http.Open "GET", FIRMA_API_BASE_URL & "/api/documentos/" & UrlEncode(idDocumento) & "/firma-imagen", False
    http.SetRequestHeader "X-API-Key", FIRMA_API_KEY
    http.Send

    statusCode = http.Status
    If http.Status >= 200 And http.Status < 300 Then
        bytes = http.ResponseBody
        DescargarFirmaImagenBytes = True
    Else
        errorMessage = JsonValue(CStr(http.ResponseText), "error")
        If Len(errorMessage) = 0 Then errorMessage = "HTTP " & CStr(http.Status) & ": " & CStr(http.ResponseText)
        DescargarFirmaImagenBytes = False
    End If

    Exit Function

EH:
    statusCode = 0
    errorMessage = Err.Number & " - " & Err.Description
    DescargarFirmaImagenBytes = False
End Function

Public Sub GuardarBytesEnArchivo(ByRef bytes() As Byte, ByVal filePath As String)
    Dim stream As Object
    Set stream = CreateObject("ADODB.Stream")
    stream.Type = 1
    stream.Open
    stream.Write bytes
    stream.SaveToFile filePath, 2
    stream.Close
End Sub

Public Function NormalizarUrlFirmaLocal(ByVal urlFirma As String) As String
    Dim result As String
    result = urlFirma
    result = Replace(result, "https://localhost:5001", FIRMA_API_BASE_URL, 1, -1, vbTextCompare)
    result = Replace(result, "http://localhost:5001", FIRMA_API_BASE_URL, 1, -1, vbTextCompare)
    NormalizarUrlFirmaLocal = result
End Function

Private Function BuildMultipartBody( _
    ByVal boundary As String, _
    ByVal pdfPath As String, _
    ByVal tipoDocumento As String, _
    ByVal nombreFirmante As String, _
    ByVal emailFirmante As String, _
    ByVal cuitDniFirmante As String, _
    ByVal celularFirmante As String, _
    ByVal idSistemaOrigen As String, _
    ByVal idComprobanteOrigen As String, _
    ByVal creadoPor As String) As Byte()

    Dim stream As Object
    Set stream = CreateObject("ADODB.Stream")
    stream.Type = 1
    stream.Open

    AppendText stream, "--" & boundary & vbCrLf
    AppendFormField stream, "tipoDocumento", tipoDocumento, boundary
    AppendFormField stream, "nombreFirmante", nombreFirmante, boundary
    AppendFormField stream, "emailFirmante", emailFirmante, boundary
    AppendFormField stream, "cuitDniFirmante", cuitDniFirmante, boundary
    AppendFormField stream, "celularFirmante", celularFirmante, boundary
    AppendFormField stream, "idSistemaOrigen", idSistemaOrigen, boundary
    AppendFormField stream, "idComprobanteOrigen", idComprobanteOrigen, boundary
    AppendFormField stream, "creadoPor", creadoPor, boundary

    AppendText stream, "Content-Disposition: form-data; name=""archivo""; filename=""" & FileNameOnly(pdfPath) & """" & vbCrLf
    AppendText stream, "Content-Type: application/pdf" & vbCrLf & vbCrLf
    AppendFile stream, pdfPath
    AppendText stream, vbCrLf & "--" & boundary & "--" & vbCrLf

    stream.Position = 0
    BuildMultipartBody = stream.Read
    stream.Close
End Function

Private Sub AppendFormField(ByVal stream As Object, ByVal name As String, ByVal value As String, ByVal boundary As String)
    AppendText stream, "Content-Disposition: form-data; name=""" & name & """" & vbCrLf & vbCrLf
    AppendText stream, value & vbCrLf
    AppendText stream, "--" & boundary & vbCrLf
End Sub

Private Sub AppendText(ByVal stream As Object, ByVal text As String)
    Dim textStream As Object
    Set textStream = CreateObject("ADODB.Stream")
    textStream.Type = 2
    textStream.Charset = "utf-8"
    textStream.Open
    textStream.WriteText text
    textStream.Position = 0
    textStream.Type = 1
    textStream.Position = 3
    stream.Write textStream.Read
    textStream.Close
End Sub

Private Sub AppendFile(ByVal stream As Object, ByVal filePath As String)
    Dim fileStream As Object
    Set fileStream = CreateObject("ADODB.Stream")
    fileStream.Type = 1
    fileStream.Open
    fileStream.LoadFromFile filePath
    stream.Write fileStream.Read
    fileStream.Close
End Sub

Private Function FileNameOnly(ByVal filePath As String) As String
    FileNameOnly = Mid$(filePath, InStrRev(filePath, "\") + 1)
End Function

Private Function NewMultipartBoundary() As String
    Randomize
    NewMultipartBoundary = "----FirmaElectronicaBoundary" & Format$(Now, "yyyymmddhhnnss") & CStr(CLng(Rnd() * 1000000))
End Function

Private Function JsonValue(ByVal json As String, ByVal propertyName As String) As String
    Dim pattern As String
    Dim startPos As Long
    Dim valueStart As Long
    Dim valueEnd As Long
    Dim ch As String

    pattern = """" & propertyName & """:"
    startPos = InStr(1, json, pattern, vbTextCompare)
    If startPos = 0 Then
        JsonValue = ""
        Exit Function
    End If

    valueStart = startPos + Len(pattern)
    Do While valueStart <= Len(json) And Mid$(json, valueStart, 1) = " "
        valueStart = valueStart + 1
    Loop

    ch = Mid$(json, valueStart, 1)
    If ch = """" Then
        valueStart = valueStart + 1
        valueEnd = valueStart
        Do While valueEnd <= Len(json)
            If Mid$(json, valueEnd, 1) = """" And Mid$(json, valueEnd - 1, 1) <> "\" Then Exit Do
            valueEnd = valueEnd + 1
        Loop
        JsonValue = Replace(Mid$(json, valueStart, valueEnd - valueStart), "\""", """")
    ElseIf Mid$(json, valueStart, 4) = "null" Then
        JsonValue = ""
    Else
        valueEnd = valueStart
        Do While valueEnd <= Len(json)
            ch = Mid$(json, valueEnd, 1)
            If ch = "," Or ch = "}" Then Exit Do
            valueEnd = valueEnd + 1
        Loop
        JsonValue = Trim$(Mid$(json, valueStart, valueEnd - valueStart))
    End If
End Function

Private Function UrlEncode(ByVal value As String) As String
    Dim i As Long
    Dim ch As String
    Dim code As Integer
    Dim output As String

    For i = 1 To Len(value)
        ch = Mid$(value, i, 1)
        code = AscW(ch)
        If (code >= 48 And code <= 57) Or (code >= 65 And code <= 90) Or (code >= 97 And code <= 122) Or ch = "-" Or ch = "_" Or ch = "." Then
            output = output & ch
        Else
            output = output & "%" & Right$("0" & Hex$(code), 2)
        End If
    Next i

    UrlEncode = output
End Function
