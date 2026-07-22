namespace SmartBiodiversity;

using SmartBiodiversity.Services;

public partial class CatalogoFaunaPage : ContentPage
{
    private readonly ApiService _apiService = new ApiService();

    public CatalogoFaunaPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarFauna();
    }

    private async Task CargarFauna()
    {
        var especiesFauna = await _apiService.ObtenerFaunaAsync();

        if (especiesFauna != null && especiesFauna.Count > 0)
        {
            listaFauna.ItemsSource = especiesFauna;
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