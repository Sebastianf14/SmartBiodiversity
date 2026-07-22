namespace SmartBiodiversity;

[QueryProperty(nameof(ItemSeleccionado), "ItemSeleccionado")]
public partial class DetalleItemPage : ContentPage
{
    private EspecieItem _itemSeleccionado;

    public EspecieItem ItemSeleccionado
    {
        get => _itemSeleccionado;
        set
        {
            _itemSeleccionado = value;

            // AQUÍ ESTÁ LA MAGIA: Al asignar el BindingContext, el XAML 
            // lee automáticamente el Nombre, ImagenUrl, DescripcionLarga, etc.
            BindingContext = _itemSeleccionado;
        }
    }

    public DetalleItemPage()
	{
		InitializeComponent();
	}
    private async void OnRegresarTapped(object sender, TappedEventArgs e)
    {
        // Regresa a la pantalla anterior
        await Shell.Current.GoToAsync("..");
    }
    private async void OnInicioTapped(object sender, TappedEventArgs e)
    {
        // "../.." significa retroceder dos pantallas (Detalle -> Catálogo -> Dashboard)
        await Shell.Current.GoToAsync("../..");
    }

    private async void OnMapaTapped(object sender, TappedEventArgs e)
    {
        // Va a la página del mapa
        await Shell.Current.GoToAsync("MapaPage");
    }

    private async void OnPerfilTapped(object sender, TappedEventArgs e)
    {
        // Va a la página de perfil
        await Shell.Current.GoToAsync("PerfilPage");
    }
}