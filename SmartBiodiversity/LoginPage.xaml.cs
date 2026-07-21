using Microsoft.Maui.Networking;
using SmartBiodiversity.Services;

namespace SmartBiodiversity;

public partial class LoginPage : ContentPage
{
    private readonly ApiService _apiService;
    public LoginPage()
	{
		InitializeComponent();
        _apiService = new ApiService();

    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        txtEmail.Text = string.Empty;
        txtPassword.Text = string.Empty;
    }
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        // 1. Revisar Internet
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            await DisplayAlert("Sin Internet", "Revisa tu conexión a internet.", "OK");
            return;
        }

        string email = txtEmail.Text;
        string password = txtPassword.Text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Atención", "Por favor ingresa tu correo y contraseña.", "OK");
            return;
        }

        // 2. Llamar a la API
        bool loginExitoso = await _apiService.LoginAsync(email, password);

        if (loginExitoso)
        {
            await Shell.Current.GoToAsync("DashboardPage");
        }
        else
        {
            await DisplayAlert("Error", "Correo o contraseña incorrecta.", "Reintentar");
        }
    }

    private async void OnGoToRegisterTapped(object sender, EventArgs e)
    {
        // Ir a la página de registro
        await Navigation.PushAsync(new RegisterPage());
    }
}