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
        // Verificar límite de 5 diarias
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

    // 3. ENVIAR APORTE CON VALIDACIONES OBLIGATORIAS
    private async void OnEnviarAporteClicked(object sender, EventArgs e)
    {
        // Validación 1: Verificar límite diario
        int enviadosHoy = ObtenerAportesHoy();
        if (enviadosHoy >= LIMITE_DIARIO)
        {
            await DisplayAlert("Límite Alcanzado", "Llegaste al límite de 5 aportes diarios.", "OK");
            return;
        }

        // Validación 2: Verificar que haya tomado una foto
        if (string.IsNullOrEmpty(_rutaFotoTomada))
        {
            await DisplayAlert("Atención", "Debes tomar una foto antes de enviar.", "OK");
            return;
        }

        // Validación 3: VERIFICAR QUE HAYA ESCRITO DETALLES (REQUISITO OBLIGATORIO)
        string detalles = txtDescripcion.Text?.Trim();
        if (string.IsNullOrWhiteSpace(detalles))
        {
            await DisplayAlert("Detalles Requeridos", "Debes ingresar una descripción o detalles del avistamiento para poder subirlo.", "OK");
            return;
        }

        // Si pasa todas las validaciones, enviamos a la API
        var (exito, mensaje) = await _apiService.CrearAporteAsync("Avistamiento Campus", detalles, _rutaFotoTomada);

        if (exito)
        {
            // Incrementar contador diario
            string claveHoy = $"Aportes_{DateTime.Today:yyyyMMdd}";
            Preferences.Default.Set(claveHoy, enviadosHoy + 1);

            await DisplayAlert("¡Éxito!", "Tu aporte ha sido enviado correctamente para su revisión.", "OK");

            // Limpiar campos
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