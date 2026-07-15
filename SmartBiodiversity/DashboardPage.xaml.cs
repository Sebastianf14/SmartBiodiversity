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

        // esto pone el timpo que va a durar la sesion toca probar bajandole aver si funciona
        Application.Current.Dispatcher.StartTimer(TimeSpan.FromHours(3), () =>
        {
            CerrarSesionAutomaticamente();

            // Retornar 'false' detiene el temporizador para que no se repita
            return false;
        });
    }
    private async void CerrarSesionAutomaticamente()
    {
        await DisplayAlert("Sesión Expirada", "Por seguridad, tu sesión ha caducado después de 3 horas.", "OK");
        await Shell.Current.GoToAsync("..");
    }
    private async void OnIdentificarTapped(object sender, EventArgs e)
    {
        try
        {
            // Verifica si el dispositivo tiene cámara compatible
            if (MediaPicker.Default.IsCaptureSupported)
            {
                // Abre la cámara para tomar UNA sola foto
                FileResult foto = await MediaPicker.Default.CapturePhotoAsync();

                if (foto != null)
                {
                    // por el momento aun se sabe como dar el permiso
                    await DisplayAlert("¡Excelente!", $"Foto capturada y guardada en: {foto.FullPath}", "OK");
                }
            }
            else
            {
                await DisplayAlert("Error", "Tu dispositivo no soporta la captura de fotos", "OK");
            }
        }
        catch (Exception ex)
        {
            // Por si el usuario no da permisos de cámara
            await DisplayAlert("Permiso Denegado", "No se pudo acceder a la cámara.", "OK");
        }
    }
    private async void OnVerMasFloraTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("CatalogoFloraPage");
    }

    private async void OnVerMasFaunaTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("CatalogoFaunaPage");
    }
    private async void OnMapaTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("MapaPage");
    }
    private async void OnPerfilTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("PerfilPage");
    }
}