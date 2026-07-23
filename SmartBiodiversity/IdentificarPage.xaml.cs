namespace SmartBiodiversity;

using SmartBiodiversity.Services;

public partial class IdentificarPage : ContentPage
{
    private readonly ApiService _apiService = new ApiService();
    private string _rutaFotoTomada = null;
    private const int LIMITE_DIARIO = 5;

    public IdentificarPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ActualizarContadorDiario();
    }

    // 1. OBTENER Y MOSTRAR CONTEO DIARIO
    private int ObtenerAportesHoy()
    {
        string claveHoy = $"Aportes_{DateTime.Today:yyyyMMdd}";
        return Preferences.Default.Get(claveHoy, 0);
    }

    private void ActualizarContadorDiario()
    {
        int enviadosHoy = ObtenerAportesHoy();
        lblContadorDiario.Text = $"{enviadosHoy}/{LIMITE_DIARIO} hoy";

        if (enviadosHoy >= LIMITE_DIARIO)
        {
            lblContadorDiario.TextColor = Colors.Red;
        }
    }

    // 2. TOMAR FOTO CON LA CÁMARA
    private async void OnTomarFotoClicked(object sender, EventArgs e)
    {
        if (ObtenerAportesHoy() >= LIMITE_DIARIO)
        {
            await DisplayAlert("Límite Alcanzado", "Has alcanzado el límite máximo de 5 fotos por día. Intenta de nuevo mañana.", "OK");
            return;
        }

        try
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                FileResult foto = await MediaPicker.Default.CapturePhotoAsync();

                if (foto != null)
                {
                    _rutaFotoTomada = foto.FullPath;

                    // Mostrar vista previa en pantalla
                    imgPrevia.Source = ImageSource.FromFile(_rutaFotoTomada);
                    imgPrevia.IsVisible = true;
                    panelPlaceholder.IsVisible = false;
                }
            }
            else
            {
                await DisplayAlert("Error", "La cámara no está disponible en este dispositivo.", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"---> Error al tomar foto: {ex.Message}");
        }
    }

    // 3. ENVIAR APORTE (SUBIDA A SUPABASE + GUARDADO EN BD)
    private async void OnEnviarAporteClicked(object sender, EventArgs e)
    {
        // Botón enviado a estado de carga
        var btnEnviar = sender as Button;

        try
        {
            // Validación 1: Verificar límite diario
            int enviadosHoy = ObtenerAportesHoy();
            if (enviadosHoy >= LIMITE_DIARIO)
            {
                await DisplayAlert("Límite Alcanzado", "Llegaste al límite de 5 aportes diarios.", "OK");
                return;
            }

            // Validación 2: Verificar foto obligatoria
            if (string.IsNullOrEmpty(_rutaFotoTomada))
            {
                await DisplayAlert("Atención", "Debes tomar una foto antes de enviar.", "OK");
                return;
            }

            // Validación 3: Verificar detalles obligatorios
            string detalles = txtDescripcion.Text?.Trim();
            if (string.IsNullOrWhiteSpace(detalles))
            {
                await DisplayAlert("Detalles Requeridos", "Debes ingresar una descripción o detalles del avistamiento para poder subirlo.", "OK");
                return;
            }

            // Cambiar UI a estado "Cargando"
            if (btnEnviar != null)
            {
                btnEnviar.IsEnabled = false;
                btnEnviar.Text = "Subiendo imagen...";
            }

            // PASO A: Subir imagen local a Supabase Storage
            var (urlPublicaImagen, errorSupabase) = await _apiService.SubirImagenSupabaseAsync(_rutaFotoTomada);

            if (string.IsNullOrEmpty(urlPublicaImagen))
            {
                await DisplayAlert("Error de Subida a Supabase", errorSupabase, "OK");
                RestablecerBotonEnviar(btnEnviar);
                return;
            }

            if (btnEnviar != null)
                btnEnviar.Text = "Guardando aporte...";

            // PASO B: Crear aporte en la API con el link generado de Supabase
            var (exito, mensaje) = await _apiService.CrearAporteAsync("Avistamiento Campus", detalles, urlPublicaImagen);

            if (exito)
            {
                // Incrementar contador diario
                string claveHoy = $"Aportes_{DateTime.Today:yyyyMMdd}";
                Preferences.Default.Set(claveHoy, enviadosHoy + 1);

                await DisplayAlert("¡Éxito!", "Tu aporte ha sido enviado correctamente para su revisión.", "OK");

                // Limpiar campos y formulario
                txtDescripcion.Text = string.Empty;
                _rutaFotoTomada = null;
                imgPrevia.IsVisible = false;
                panelPlaceholder.IsVisible = true;

                ActualizarContadorDiario();
            }
            else
            {
                await DisplayAlert("Error", $"No se pudo subir el aporte: {mensaje}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error Inesperado", ex.Message, "OK");
        }
        finally
        {
            RestablecerBotonEnviar(btnEnviar);
        }
    }

    private void RestablecerBotonEnviar(Button btn)
    {
        if (btn != null)
        {
            btn.IsEnabled = true;
            btn.Text = "ENVIAR APORTE";
        }
    }

    // NAVEGACIÓN BARRA INFERIOR
    private async void OnInicioTapped(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }

    private async void OnIdentificarTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new IdentificarPage());
    }

    private async void OnMapaTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MapaPage());
    }

    private async void OnPerfilTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PerfilPage());
    }
}