namespace SmartBiodiversity;

using SmartBiodiversity.Services;
using System.Reflection;

public partial class PerfilPage : ContentPage
{
    private readonly ApiService _apiService = new ApiService();

    public PerfilPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CargarPerfilUsuario();
    }

    private void CargarPerfilUsuario()
    {
        string nombre = Preferences.Default.Get("NombreUsuario", "Administrador");
        string correo = Preferences.Default.Get("CorreoUsuario", "admin@utn.edu.ec");

        if (lblNombrePerfil != null)
            lblNombrePerfil.Text = string.IsNullOrWhiteSpace(nombre) ? "Usuario" : nombre;

        if (lblCorreoPerfil != null)
            lblCorreoPerfil.Text = string.IsNullOrWhiteSpace(correo) ? "usuario@utn.edu.ec" : correo;
    }

    // ========================================================
    // GENERACIÓN Y DESCARGA SEGURA DE PDF
    // ========================================================
    private async void OnDescargarPdfClicked(object sender, EventArgs e)
    {
        if (btnDescargarPdf != null)
        {
            btnDescargarPdf.IsEnabled = false;
            btnDescargarPdf.Text = "Generando PDF...";
        }

        try
        {
            string correoUsuario = Preferences.Default.Get("CorreoUsuario", "usuario_anonimo");

            // 1. REGISTRAR EN LA BITÁCORA DE LA API
            await _apiService.RegistrarBitacoraAsync(
                correoUsuario,
                "DESCARGA_CATALOGO_PDF",
                $"El usuario {correoUsuario} descargó el catálogo de Flora y Fauna en PDF."
            );

            // 2. OBTENER ESPECIES DESDE LA API
            var especies = await _apiService.ObtenerEspeciesAsync();

            // 3. RUTA SEGURA SIN PROBLEMAS DE PERMISOS DE ANDROID
            string fileName = $"Catologo_UTN_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            // 4. GENERAR EL ARCHIVO PDF
#if ANDROID
            await GenerarPdfAndroidSeguroAsync(especies, correoUsuario, filePath);
#else
            await File.WriteAllTextAsync(filePath, "Catálogo PDF para " + correoUsuario);
#endif

            // 5. ABRIR EL PDF AUTOMÁTICAMENTE
            if (File.Exists(filePath))
            {
                await DisplayAlert("¡Catálogo Listo!", "El documento PDF se generó correctamente.", "Ver PDF");

                await Launcher.Default.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(filePath)
                });
            }
            else
            {
                await DisplayAlert("Error", "No se pudo crear el archivo PDF.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error Inesperado", $"No se pudo generar el PDF: {ex.Message}", "OK");
        }
        finally
        {
            if (btnDescargarPdf != null)
            {
                btnDescargarPdf.IsEnabled = true;
                btnDescargarPdf.Text = "?? Descargar Catálogo (PDF)";
            }
        }
    }

#if ANDROID
    // ========================================================
    // GENERADOR PDF NATIVO PROTEGIDO CONTRA ERRORES DE ANDROID
    // ========================================================
    private async Task GenerarPdfAndroidSeguroAsync(List<EspecieItem> especies, string correoUsuario, string filePath)
    {
        using var httpClient = new HttpClient();

        // PASO A: Pre-descargar todas las imágenes en memoria ANTES de abrir el PDF
        var listaEspeciesProcesadas = new List<(string Nombre, string Descripcion, byte[] ImagenBytes)>();

        if (especies != null)
        {
            foreach (var esp in especies)
            {
                string nombre = ExtraerNombre(esp);
                string descripcion = ExtraerDescripcion(esp);
                string imgUrl = ExtraerImagenUrl(esp);
                byte[] imgBytes = null;

                if (!string.IsNullOrEmpty(imgUrl))
                {
                    try
                    {
                        imgBytes = await httpClient.GetByteArrayAsync(imgUrl);
                    }
                    catch { /* Si falla una imagen, la omitimos sin crashear la app */ }
                }

                listaEspeciesProcesadas.Add((nombre, descripcion, imgBytes));
            }
        }

        // PASO B: Dibujar el PDF de forma síncrona en el hilo gráfico
        using var pdfDoc = new Android.Graphics.Pdf.PdfDocument();
        int pageWidth = 595;  // A4 Ancho
        int pageHeight = 842; // A4 Alto
        int pageNumber = 1;

        var pageInfo = new Android.Graphics.Pdf.PdfDocument.PageInfo.Builder(pageWidth, pageHeight, pageNumber).Create();
        var page = pdfDoc.StartPage(pageInfo);
        var canvas = page.Canvas;

        using var paintTitle = new Android.Graphics.Paint { TextSize = 18, Color = Android.Graphics.Color.ParseColor("#2E7D32"), FakeBoldText = true };
        using var paintSubtitle = new Android.Graphics.Paint { TextSize = 10, Color = Android.Graphics.Color.DarkGray };
        using var paintName = new Android.Graphics.Paint { TextSize = 13, Color = Android.Graphics.Color.ParseColor("#1B5E20"), FakeBoldText = true };
        using var paintText = new Android.Graphics.Paint { TextSize = 9, Color = Android.Graphics.Color.Black };
        using var paintLine = new Android.Graphics.Paint { Color = Android.Graphics.Color.ParseColor("#2E7D32"), StrokeWidth = 2 };

        int y = 45;

        // Encabezado
        canvas.DrawText("Smart Biodiversity - Campus UTN", 30, y, paintTitle);
        y += 20;
        canvas.DrawText($"Catálogo Oficial de Flora y Fauna | Descargado por: {correoUsuario}", 30, y, paintSubtitle);
        y += 12;
        canvas.DrawLine(30, y, pageWidth - 30, y, paintLine);
        y += 35;

        if (listaEspeciesProcesadas.Count > 0)
        {
            foreach (var item in listaEspeciesProcesadas)
            {
                // Salto de página si el contenido sobrepasa la hoja
                if (y + 100 > pageHeight - 40)
                {
                    pdfDoc.FinishPage(page);
                    pageNumber++;
                    pageInfo = new Android.Graphics.Pdf.PdfDocument.PageInfo.Builder(pageWidth, pageHeight, pageNumber).Create();
                    page = pdfDoc.StartPage(pageInfo);
                    canvas = page.Canvas;
                    y = 45;
                }

                int xTexto = 30;

                // Dibujar Imagen si existe
                if (item.ImagenBytes != null && item.ImagenBytes.Length > 0)
                {
                    try
                    {
                        using var bitmap = Android.Graphics.BitmapFactory.DecodeByteArray(item.ImagenBytes, 0, item.ImagenBytes.Length);
                        if (bitmap != null)
                        {
                            var rect = new Android.Graphics.Rect(30, y, 120, y + 75);
                            canvas.DrawBitmap(bitmap, null, rect, null);
                            xTexto = 135;
                        }
                    }
                    catch { }
                }

                // Dibujar Nombre
                canvas.DrawText(item.Nombre, xTexto, y + 15, paintName);

                // Dibujar Descripción en varias líneas
                int maxCaracteres = (pageWidth - xTexto - 30) / 5;
                if (maxCaracteres < 20) maxCaracteres = 20;

                int textY = y + 30;
                for (int i = 0; i < item.Descripcion.Length; i += maxCaracteres)
                {
                    int len = Math.Min(maxCaracteres, item.Descripcion.Length - i);
                    string linea = item.Descripcion.Substring(i, len);
                    canvas.DrawText(linea, xTexto, textY, paintText);
                    textY += 12;
                    if (textY > y + 75) break;
                }

                y += 90; // Separación entre elementos
            }
        }
        else
        {
            canvas.DrawText("No hay especies registradas actualmente.", 30, y, paintText);
        }

        pdfDoc.FinishPage(page);

        // Guardar archivo final
        using var fileStream = System.IO.File.Create(filePath);
        pdfDoc.WriteTo(fileStream);
        pdfDoc.Close();
    }
#endif

    // MÉTODOS AUXILIARES INTELIGENTES
    private string ExtraerNombre(object obj)
    {
        return ObtenerPropiedadTexto(obj, "Nombre", "NombreComun", "NombreCientifico", "Titulo") ?? "Especie del Campus";
    }

    private string ExtraerImagenUrl(object obj)
    {
        return ObtenerPropiedadTexto(obj, "ImagenUrl", "RutaImagen", "RutaArchivo", "Url", "Ruta") ?? "";
    }

    private string ExtraerDescripcion(object obj)
    {
        if (obj == null) return "Sin descripción.";

        string desc = ObtenerPropiedadTexto(obj, "Descripcion", "descripcion", "DescripcionEsp", "Habitat", "habitat", "Detalles", "Detalle");
        if (!string.IsNullOrWhiteSpace(desc))
            return desc;

        string mejorTexto = "";
        foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.PropertyType == typeof(string))
            {
                string nombreProp = prop.Name.ToLower();
                if (nombreProp.Contains("id") || nombreProp.Contains("imagen") || nombreProp.Contains("url") || nombreProp.Contains("nombre"))
                    continue;

                string val = prop.GetValue(obj)?.ToString();
                if (!string.IsNullOrWhiteSpace(val) && val.Length > mejorTexto.Length)
                {
                    mejorTexto = val;
                }
            }
        }

        return string.IsNullOrWhiteSpace(mejorTexto) ? "Especie registrada en el campus UTN." : mejorTexto;
    }

    private string ObtenerPropiedadTexto(object obj, params string[] nombresPropiedad)
    {
        if (obj == null) return null;
        var tipo = obj.GetType();
        foreach (var propName in nombresPropiedad)
        {
            var prop = tipo.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop != null)
            {
                var val = prop.GetValue(obj);
                if (val != null && !string.IsNullOrWhiteSpace(val.ToString()))
                    return val.ToString();
            }
        }
        return null;
    }

    // CERRAR SESIÓN
    private async void OnCerrarSesionClicked(object sender, EventArgs e)
    {
        bool confirmar = await DisplayAlert("Cerrar Sesión", "¿Deseas salir de la aplicación?", "Sí", "Cancelar");
        if (confirmar)
        {
            Preferences.Default.Remove("NombreUsuario");
            Preferences.Default.Remove("CorreoUsuario");
            Preferences.Default.Remove("AuthToken");

            if (Application.Current != null)
            {
                Application.Current.MainPage = new NavigationPage(new LoginPage());
            }
        }
    }

    // NAVEGACIÓN BARRA INFERIOR
    private async void OnInicioTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new DashboardPage());
    }

    private async void OnMapaTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MapaPage());
    }

    private async void OnIdentificarTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new IdentificarPage());
    }
}