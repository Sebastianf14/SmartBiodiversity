namespace SmartBiodiversity;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
	}
    protected override void OnAppearing()
    {
        base.OnAppearing();
        txtEmail.Text = string.Empty;
        txtPassword.Text = string.Empty;
    }
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string email = txtEmail.Text;
        string password = txtPassword.Text;

        // Validación simple (Luego la conectaremos a tu PostgreSQL)
        if (email == "admin@utn.edu.ec" && password == "1234")
        {
            // Navegar al Dashboard
            await Shell.Current.GoToAsync("DashboardPage");
        }
        else
        {
            // Mostrar mensaje de error si fallan las credenciales
            await DisplayAlert("Error", "Correo o contraseña incorrecta", "Reintentar");
        }
    }

    private async void OnGoToRegisterTapped(object sender, EventArgs e)
    {
        // Ir a la página de registro
        await Navigation.PushAsync(new RegisterPage());
    }
}