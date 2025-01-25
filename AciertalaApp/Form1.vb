Imports Microsoft.Web.WebView2.WinForms
Imports Microsoft.Web.WebView2.Core
Imports Newtonsoft.Json.Linq
Imports System.Drawing.Printing
Imports System.Net.NetworkInformation
Imports System.IO

Public Class Form1
    Private mainWebView As WebView2
    Private ticketContent As String

    Private Async Sub TerminalLogin_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        mainWebView = New WebView2()
        mainWebView.Name = "mainWebView"
        mainWebView.Dock = DockStyle.Fill
        Me.Controls.Add(mainWebView)

        Await mainWebView.EnsureCoreWebView2Async()
        mainWebView.CoreWebView2.Settings.AreDevToolsEnabled = True


        AddHandler mainWebView.CoreWebView2.WebMessageReceived, AddressOf OnWebMessage


        Await InjectInterceptionScriptAsync()


        mainWebView.Source = New Uri("https://www.pe.aciertala.com/sport?sportId=1")
    End Sub

    Private Async Function InjectInterceptionScriptAsync() As Task
        Dim interceptionScript As String =
        "(function() {
            const originalFetch = window.fetch;
            const originalXHR = window.XMLHttpRequest;

            // Interceptar fetch
            window.fetch = async function(...args) {
                const response = await originalFetch.apply(this, args);
                const clonedResponse = response.clone();
                const requestUrl = args[0];
                const requestInit = args[1] || {};
                const requestBody = requestInit.body || null;

                console.log('Interceptando fetch:', { url: requestUrl, method: requestInit.method, body: requestBody });

                if (requestUrl.includes('/set-print') && requestInit.method === 'POST') {
                    clonedResponse.text().then(body => {
                        console.log('Fetch interceptado para /set-print:');
                        console.log('URL:', requestUrl);
                        console.log('Request Body:', requestBody);
                        console.log('Response Body:', body);

                        window.chrome.webview.postMessage({
                            type: 'fetch-intercept',
                            url: requestUrl,
                            method: requestInit.method,
                            requestBody: requestBody,
                            responseBody: body
                        });
                    });
                }
                return response;
            };

            // Interceptar XMLHttpRequest
            const xhrOpen = originalXHR.prototype.open;
            const xhrSend = originalXHR.prototype.send;

            originalXHR.prototype.open = function(method, url, ...rest) {
                this._url = url;
                this._method = method;
                console.log('XHR abierto:', { url: url, method: method });
                return xhrOpen.apply(this, [method, url, ...rest]);
            };

            originalXHR.prototype.send = function(body) {
                console.log('XHR enviado con body:', body);
                this.addEventListener('load', function() {
                    console.log('XHR respuesta recibida:', {
                        url: this._url,
                        method: this._method,
                        responseText: this.responseText
                    });

                    if (this._url.includes('/set-print') && this._method === 'POST') {
                        console.log('XHR interceptado para /set-print');
                        window.chrome.webview.postMessage({
                            type: 'xhr-intercept',
                            url: this._url,
                            method: this._method,
                            requestBody: body,
                            responseBody: this.responseText
                        });
                    }
                });
                return xhrSend.apply(this, [body]);
            };

            console.log('Interceptores de fetch y XHR inyectados');
        })();"

        Await mainWebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(interceptionScript)
        Debug.WriteLine("[InjectInterceptionScriptAsync] Script inyectado correctamente.")
    End Function

    Private Sub OnWebMessage(sender As Object, e As CoreWebView2WebMessageReceivedEventArgs)
        Try
            Dim rawJson As String = e.WebMessageAsJson
            Dim msgObj As JObject = JObject.Parse(rawJson)
            Dim msgType As String = msgObj("type")?.ToString()

            Select Case msgType
                Case "fetch-intercept"
                    Dim responseBody As String = msgObj("responseBody")?.ToString()
                    Debug.WriteLine("[FETCH INTERCEPT] /set-print")
                    Debug.WriteLine($"[Response Body]: {responseBody}")
                    ' -> Procesar en segundo plano
                    Task.Run(Sub() ProcessTicketResponse(responseBody))

                Case "xhr-intercept"
                    Dim responseBody As String = msgObj("responseBody")?.ToString()
                    Debug.WriteLine("[XHR INTERCEPT] /set-print")
                    Debug.WriteLine($"[Response Body]: {responseBody}")
                    ' -> Procesar en segundo plano
                    Task.Run(Sub() ProcessTicketResponse(responseBody))

                Case Else
                    Debug.WriteLine($"[JS MSG] Tipo desconocido: {rawJson}")
            End Select
        Catch ex As Exception
            Debug.WriteLine($"[OnWebMessage ERROR] {ex.Message}")
        End Try
    End Sub

    ' =========================
    ' = MÉTODO PRINCIPAL DE PROCESO E IMPRESIÓN =
    ' =========================

    Private Sub ProcessTicketResponse(responseBody As String)
        Try
            ' Log de depuración
            Debug.WriteLine("Response Body (set-print):")
            Debug.WriteLine(responseBody)

            ' Parsear JSON
            Dim parsedResponse As JObject = JObject.Parse(responseBody)
            Dim ticketData As JObject = parsedResponse("data")("ticket")

            ' Configurar PrintDocument
            Dim printDoc As New PrintDocument()
            printDoc.PrinterSettings.PrinterName = My.Settings.PrinterName
            If Not printDoc.PrinterSettings.IsValid Then
                Throw New Exception($"La impresora '{My.Settings.PrinterName}' no es válida o no está instalada.")
            End If

            ' Ajustar ancho del papel
            Dim paperWidth As Integer = If(My.Settings.PaperSize = "58 mm", 200, 300)
            Dim customPaperSize As New PaperSize("Custom", paperWidth, 3000)
            printDoc.DefaultPageSettings.PaperSize = customPaperSize

            ' Suscribirse al evento PrintPage
            AddHandler printDoc.PrintPage,
                Sub(sender As Object, e As PrintPageEventArgs)
                    Dim g As Graphics = e.Graphics
                    g.Clear(Color.White)

                    Dim y As Integer = 5
                    Dim marginLeft As Integer = If(My.Settings.PaperSize = "58 mm", 5, 10)
                    Dim pageWidthCalc As Integer = e.PageBounds.Width - marginLeft * 2

                    ' Fuentes
                    Dim fontNormal As Font = New Font("Courier New", 6, FontStyle.Regular)
                    Dim fontBold As Font = New Font("Courier New", 6, FontStyle.Bold)
                    If My.Settings.PaperSize = "80 mm" Then
                        fontNormal = New Font("Arial", 8, FontStyle.Regular)
                        fontBold = New Font("Arial", 8, FontStyle.Bold)
                    End If

                    ' Dibujar LOGO si existe
                    If Not String.IsNullOrEmpty(My.Settings.LogoPath) AndAlso File.Exists(My.Settings.LogoPath) Then
                        Dim logoImg = Image.FromFile(My.Settings.LogoPath)
                        Dim logoWidth As Integer = If(My.Settings.PaperSize = "58 mm", 140, 280)
                        Dim logoHeight As Integer = If(My.Settings.PaperSize = "58 mm", 55, 110)
                        Dim logoRect = New Rectangle((pageWidthCalc - logoWidth) \ 2, y, logoWidth, logoHeight)
                        g.DrawImage(logoImg, logoRect)
                        y += logoHeight + 10
                    End If

                    ' Lápiz para líneas
                    Dim dashPen As New Pen(Color.Black, If(My.Settings.PaperSize = "58 mm", 1, 3))
                    dashPen.DashPattern = New Single() {1, If(My.Settings.PaperSize = "58 mm", 1, 2)}

                    ' Título "Ticket: X"
                    Dim fontBigBold As New Font(fontBold.FontFamily, fontBold.Size + 5, fontBold.Style)
                    DrawBox(g, $"Ticket: {ticketData("code")}", fontBigBold, marginLeft, y, pageWidthCalc, My.Settings.PaperSize)
                    y += 50

                    ' Fecha de emisión
                    Dim checkoutTime As String = ticketData("checkout_time").ToString()
                    Dim parsedDate = DateTime.Parse(checkoutTime)
                    g.DrawString($"Fecha emisión: {parsedDate:dd/MM/yyyy HH:mm:ss}", fontNormal, Brushes.Black, marginLeft, y)
                    y += 30

                    ' Línea divisoria
                    g.DrawLine(dashPen, marginLeft, y, pageWidthCalc - marginLeft, y)
                    y += 10

                    ' Apuestas
                    Dim items = ticketData("items")
                    If items IsNot Nothing Then
                        For Each bet As JObject In items
                            ' Se usan 'event_date', 'tournament->name', 'event_name', 'market_name', 'odds_name', 'odds_value'
                            Dim gameStartDate As String = bet("event_date")?.ToString()
                            Dim league As String = bet("tournament")("name")?.ToString()

                            ' gameStartDate - league
                            Dim lines1 = WrapText($"{gameStartDate} - {league}", pageWidthCalc - 10, fontBold, g)
                            For Each line In lines1
                                g.DrawString(line, fontBold, Brushes.Black, marginLeft, y)
                                y += 15
                            Next

                            ' event_name
                            Dim lines2 = WrapText($"Match: {bet("event_name")}", pageWidthCalc - 10, fontNormal, g)
                            For Each line In lines2
                                g.DrawString(line, fontNormal, Brushes.Black, marginLeft, y)
                                y += 15
                            Next

                            ' market_name - odds_name (odds_value)
                            Dim line3 = $"{bet("market_name")} - {bet("odds_name")} ({bet("odds_value")})"
                            Dim lines3 = WrapText(line3, pageWidthCalc - 10, fontNormal, g)
                            For Each l In lines3
                                g.DrawString(l, fontNormal, Brushes.Black, marginLeft, y)
                                y += 25
                            Next
                        Next
                    End If

                    ' Línea divisoria
                    g.DrawLine(dashPen, marginLeft, y, pageWidthCalc - marginLeft, y)
                    y += 10

                    ' Totales
                    Dim stakeVal As String = ticketData("stake")?.ToString()
                    Dim totalOdds As String = ticketData("total_odds")?.ToString()
                    Dim maxWin As String = ticketData("max_win")?.ToString()
                    Dim currencyCode As String = ticketData("ticket_currency")("code")?.ToString()

                    g.DrawString($"Monto: {stakeVal} {currencyCode}", fontBold, Brushes.Black, marginLeft, y)
                    y += 20
                    g.DrawString($"Cuota Total: {totalOdds}", fontBold, Brushes.Black, marginLeft, y)
                    y += 20
                    g.DrawString($"Posible Ganancia Total: {maxWin} {currencyCode}", fontBold, Brushes.Black, marginLeft, y)
                    y += 20

                    ' Línea divisoria
                    g.DrawLine(dashPen, marginLeft, y, pageWidthCalc - marginLeft, y)
                    y += 10


                End Sub

            ' Imprimir en segundo plano, no traba la UI
            printDoc.Print()
        Catch ex As Exception
            Debug.WriteLine($"[ProcessTicketResponse ERROR] {ex.Message}")
        End Try
    End Sub


    Private Sub DrawBox(g As Graphics, text As String, font As Font, marginLeft As Integer, y As Integer, pageWidth As Integer, tipoImpresora As String)
        Dim boxWidth As Integer = pageWidth - (2 * marginLeft)
        Dim textSize As SizeF = g.MeasureString(text, font)

        If tipoImpresora = "58 mm" Then
            Dim dashPen As New Pen(Color.Black, 1)
            dashPen.DashPattern = New Single() {2, 2}

            Dim topY As Integer = y
            Dim bottomY As Integer = y + textSize.Height + 10
            Dim leftX As Integer = marginLeft
            Dim rightX As Integer = leftX + boxWidth

            g.DrawLine(dashPen, leftX, topY, rightX, topY)
            g.DrawLine(dashPen, leftX, topY, leftX, bottomY)
            g.DrawLine(dashPen, rightX, topY, rightX, bottomY)
            g.DrawLine(dashPen, leftX, bottomY, rightX, bottomY)

            Dim textX As Single = leftX + (boxWidth - textSize.Width) / 2
            Dim textY As Single = topY + (bottomY - topY - textSize.Height) / 2
            g.DrawString(text, font, Brushes.Black, textX, textY)
        Else
            Dim rectX As Integer = marginLeft
            Dim rectY As Integer = y
            Dim rectWidth As Integer = boxWidth
            Dim rectHeight As Integer = textSize.Height + 15

            g.FillRectangle(Brushes.Black, rectX, rectY, rectWidth, rectHeight)
            g.DrawString(text, font, Brushes.White, rectX + (rectWidth - textSize.Width) / 2, rectY + 5)
        End If
    End Sub

    Private Function WrapText(input As String, maxWidth As Integer, font As Font, g As Graphics) As List(Of String)
        Dim words = input.Split(" "c)
        Dim lines As New List(Of String)
        Dim currentLine = ""

        For Each word In words
            Dim testLine = If(currentLine.Length > 0, $"{currentLine} {word}", word)
            Dim size = g.MeasureString(testLine, font)

            If size.Width > maxWidth Then
                lines.Add(currentLine)
                currentLine = word
            Else
                currentLine = testLine
            End If
        Next

        If Not String.IsNullOrEmpty(currentLine) Then lines.Add(currentLine)
        Return lines
    End Function
End Class


