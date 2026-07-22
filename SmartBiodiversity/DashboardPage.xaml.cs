using Microsoft.Maui.Storage;
namespace SmartBiodiversity;

public partial class DashboardPage : ContentPage
{
	public DashboardPage()
	{
        InitializeComponent();
    }
    protected override bool OnBackButtonPressed()
    {
        return true;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Leemos el nombre guardado al iniciar sesión
        string nombreGuardado = Preferences.Default.Get("NombreUsuario", "USUARIO");

        if (lblNombreUsuario != null)
            lblNombreUsuario.Text = nombreGuardado.ToUpper();

        if (lblSaludo != null)
            lblSaludo.Text = $"HOLA, {nombreGuardado.ToUpper()}!";
    }
    
    
    // NAVEGACIÓN A FLORA
    private async void OnVerMasFloraTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CatalogoFloraPage());
    }

    // NAVEGACIÓN A FAUNA
    private async void OnVerMasFaunaTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CatalogoFaunaPage());
    }
    // NAVEGACIÓN A MAPA
    private async void OnMapaTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MapaPage());
    }

    // NAVEGACIÓN A IDENTIFICAR
    private async void OnIdentificarTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new IdentificarPage());
    }

    // NAVEGACIÓN A PERFIL
    private async void OnPerfilTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PerfilPage());
    }
}