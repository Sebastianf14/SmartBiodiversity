namespace SmartBiodiversity;

public partial class PerfilPage : ContentPage
{
	public PerfilPage()
	{
		InitializeComponent();
	}
    private async void OnCerrarSesionClicked(object sender, EventArgs e)
    {
        bool confirmacion = await DisplayAlert("Cerrar Sesión", "¿Estás seguro de que deseas salir?", "Sí, salir", "Cancelar");

        if (confirmacion)
        {
            await Shell.Current.GoToAsync("//login");
        }
    }

    private async void OnInicioTapped(object sender, TappedEventArgs e)
    {
        // Para volver al Dashboard
        await Shell.Current.GoToAsync("..");
    }
}