namespace SmartBiodiversity;

using SmartBiodiversity.Services;

public partial class ForgotPasswordPage : ContentPage
{
    private readonly ApiService _apiService = new ApiService();

    public ForgotPasswordPage()
    {
        InitializeComponent();
    }

    // PASO 1: Solicitar el código
    private async void OnEnviarCodigoClicked(object sender, EventArgs e)
    {
        string email = txtCorreoRecuperacion.Text?.Trim();

        if (string.IsNullOrEmpty(email))
        {
            await DisplayAlert("Atención", "Por favor ingresa tu correo electrónico.", "OK");
            return;
        }

        var resultado = await _apiService.SolicitarCodigoOlvidePasswordAsync(email);

        if (resultado.exito)
        {
            await DisplayAlert("Código Enviado", "Hemos enviado un código de verificación a tu correo.", "OK");
            PanelCodigoVerificacion.IsVisible = true;
        }
        else
        {
            await DisplayAlert("Error", "No se pudo enviar el código. Revisa el correo ingresado.", "OK");
        }
    }

    // PASO 2: Reenviar código
    private async void OnReenviarCodigoClicked(object sender, EventArgs e)
    {
        string email = txtCorreoRecuperacion.Text?.Trim();

        if (string.IsNullOrEmpty(email))
        {
            await DisplayAlert("Atención", "Ingresa tu correo primero.", "OK");
            return;
        }

        var resultado = await _apiService.SolicitarCodigoOlvidePasswordAsync(email);

        if (resultado.exito)
        {
            await DisplayAlert("Reenviado", "Se ha enviado un nuevo código a tu correo.", "OK");
        }
        else
        {
            await DisplayAlert("Error", "No se pudo reenviar el código.", "OK");
        }
    }

    // PASO 3: Verificar el código
    private async void OnVerificarCodigoClicked(object sender, EventArgs e)
    {
        string email = txtCorreoRecuperacion.Text?.Trim();
        string codigo = txtCodigoVerificacion.Text?.Trim();

        if (string.IsNullOrEmpty(codigo) || codigo.Length < 6)
        {
            await DisplayAlert("Atención", "Por favor ingresa el código completo de 6 dígitos.", "OK");
            return;
        }

        // El código recibido por correo es el token de restablecimiento.
        // Navegamos a la pantalla de Nueva Contraseña para aplicar el cambio en /api/Auth/reset-password
        await Navigation.PushAsync(new ResetPasswordPage(email, codigo));
    }

    private async void OnVolverLoginTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}