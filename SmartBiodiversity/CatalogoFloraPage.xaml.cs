namespace SmartBiodiversity;
using System.Text.Json.Serialization;
using SmartBiodiversity.Services;
public class EspecieItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    // Mapeamos 'nombreComun' de SQL al campo 'Nombre' de tu XAML
    [JsonPropertyName("nombreComun")]
    public string Nombre { get; set; }

    [JsonPropertyName("nombreCientifico")]
    public string NombreCientifico { get; set; }

    [JsonPropertyName("descripcion")]
    public string DescripcionLarga { get; set; }

    [JsonPropertyName("habitat")]
    public string DescripcionCorta { get; set; }

    [JsonPropertyName("categoriaId")]
    public string CategoriaId { get; set; }

    public string Tamano { get; set; } = "N/A";
    public string Dieta { get; set; } = "N/A";
    public string Estado { get; set; } = "N/A";

    // URL de la imagen (usará el ícono por defecto si la especie aún no tiene foto en BD)
    public string ImagenUrl { get; set; } = "flora_icono.png";
}
public partial class CatalogoFloraPage : ContentPage
{
    private readonly ApiService _apiService = new ApiService();

    public CatalogoFloraPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarFlora();
    }

    private async Task CargarFlora()
    {
        var flora = await _apiService.ObtenerFloraAsync();
        listaFlora.ItemsSource = flora;
    }

    // IR A DETALLE DE LA ESPECIE SELECCIONADA
    private async void OnDetalleTapped(object sender, EventArgs e)
    {
        try
        {
            EspecieItem especieSeleccionada = null;

            // 1. Intento por CommandParameter del GestureRecognizer
            if (sender is TapGestureRecognizer tap && tap.CommandParameter is EspecieItem itemTap)
            {
                especieSeleccionada = itemTap;
            }
            // 2. Intento por BindingContext del objeto asignado al Gesture
            else if (sender is TapGestureRecognizer tapGesture && tapGesture.Parent is BindableObject parentObj)
            {
                especieSeleccionada = parentObj.BindingContext as EspecieItem;
            }
            // 3. Intento por BindingContext del elemento visual directo (Border/Grid)
            else if (sender is BindableObject bindable)
            {
                especieSeleccionada = bindable.BindingContext as EspecieItem;
            }

            // Si encontramos la especie, navegamos a la pantalla de detalle
            if (especieSeleccionada != null)
            {
                await Navigation.PushAsync(new DetalleItemPage(especieSeleccionada));
            }
            else
            {
                await DisplayAlert("Atención", "No se pudo obtener la información de este elemento.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error de Navegación", $"Ocurrió un detalle al abrir: {ex.Message}", "OK");
        }
    }

    // NAVEGACIÓN BARRA INFERIOR
    private async void OnInicioTapped(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }

    private async void OnMapaTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MapaPage());
    }

    private async void OnIdentificarTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new IdentificarPage());
    }

    private async void OnPerfilTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PerfilPage());
    }
}