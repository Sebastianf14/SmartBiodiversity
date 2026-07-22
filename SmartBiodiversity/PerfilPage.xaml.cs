namespace SmartBiodiversity;

public partial class PerfilPage : ContentPage
{
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
        // Obtenemos el nombre y correo guardados en el login/registro
        string nombre = Preferences.Default.Get("NombreUsuario", "Administrador");
        string correo = Preferences.Default.Get("CorreoUsuario", "admin@utn.edu.ec");

        if (lblNombrePerfil != null)
            lblNombrePerfil.Text = string.IsNullOrWhiteSpace(nombre) ? "Usuario" : nombre;

        if (lblCorreoPerfil != null)
            lblCorreoPerfil.Text = string.IsNullOrWhiteSpace(correo) ? "usuario@utn.edu.ec" : correo;
    }

    private async void OnCerrarSesionClicked(object sender, EventArgs e)
    {
        bool confirmar = await DisplayAlert("Cerrar Sesión", "¿Deseas cerrar sesión?", "Sí", "Cancelar");
        if (confirmar)
        {
            // 1. Borramos los datos de sesión guardados
            Preferences.Default.Remove("NombreUsuario");
            Preferences.Default.Remove("CorreoUsuario");

            // 2. Establecemos LoginPage como la nueva página principal
            if (Application.Current != null)
            {
                Application.Current.MainPage = new NavigationPage(new LoginPage());
            }
        }
    }

    // 1. INICIO -> Abre directamente la pantalla "HOLA, BSFERNANDEZI!"
    private async void OnInicioTapped(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new DashboardPage());
        }
        catch
        {
            await Shell.Current.GoToAsync("//DashboardPage");
        }
    }

    // NAVEGACIÓN A MAPA
    private async void OnMapaTapped(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("//MapaPage");
        }
        catch
        {
            await Navigation.PushAsync(new MapaPage());
        }
    }

    // NAVEGACIÓN A IDENTIFICAR
    private async void OnIdentificarTapped(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("//IdentificarPage");
        }
        catch
        {
            await Navigation.PushAsync(new IdentificarPage());
        }
    }
}