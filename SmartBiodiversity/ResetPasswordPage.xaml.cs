namespace SmartBiodiversity;

using SmartBiodiversity.Services;

public partial class ResetPasswordPage : ContentPage
{
    private readonly ApiService _apiService = new ApiService();
    private readonly string _email;
    private readonly string _token;

    public ResetPasswordPage(string email, string token)
    {
        InitializeComponent();
        _email = email;
        _token = token;
    }

    private async void OnConfirmarCambioClicked(object sender, EventArgs e)
    {
        string nuevaPass = txtNuevaPassword.Text?.Trim();
        string confirmarPass = txtConfirmarPassword.Text?.Trim();

        if (string.IsNullOrEmpty(nuevaPass) || string.IsNullOrEmpty(confirmarPass))
        {
            await DisplayAlert("Atención", "Por favor completa ambos campos.", "OK");
            return;
        }

        if (nuevaPass != confirmarPass)
        {
            await DisplayAlert("Error", "Las contraseñas no coinciden.", "OK");
            return;
        }

        // VALIDACIÓN DE 8 CARACTERES EXIGIDA POR LA API
        if (nuevaPass.Length < 8)
        {
            await DisplayAlert("Atención", "La contraseña debe tener al menos 8 caracteres.", "OK");
            return;
        }

        var resultado = await _apiService.RestablecerPasswordAsync(_email, _token, nuevaPass);

        if (resultado.exito)
        {
            await DisplayAlert("¡Éxito!", "Tu contraseña ha sido actualizada correctamente.", "Iniciar Sesión");

            // Redirigimos al Login de forma limpia
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
        else
        {
            await DisplayAlert("Error", resultado.mensaje, "OK");
        }
    }
}