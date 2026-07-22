namespace SmartBiodiversity;

using SmartBiodiversity.Models;
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
        var fauna = await _apiService.ObtenerFaunaAsync();
        listaFauna.ItemsSource = fauna;
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