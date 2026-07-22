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
        try
        {
            var especiesFlora = await _apiService.ObtenerFloraAsync();

            if (especiesFlora != null && especiesFlora.Count > 0)
            {
                listaFlora.ItemsSource = especiesFlora;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"---> Error al cargar Flora: {ex.Message}");
        }
    }

    private async void OnDetalleTapped(object sender, TappedEventArgs e)
    {
        var seleccion = e.Parameter as EspecieItem;

        if (seleccion != null)
        {
            var parametros = new Dictionary<string, object>
            {
                { "ItemSeleccionado", seleccion }
            };

            await Shell.Current.GoToAsync("DetalleItemPage", parametros);
        }
    }

    private async void OnInicioTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}