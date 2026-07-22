namespace SmartBiodiversity;

using SmartBiodiversity.Models;

public partial class DetalleItemPage : ContentPage
{
    public DetalleItemPage()
    {
        InitializeComponent();
    }

    public DetalleItemPage(EspecieItem especie) : this()
    {
        BindingContext = especie;
        CargarDescripcionAuto(especie);
    }

    private void CargarDescripcionAuto(EspecieItem especie)
    {
        if (especie == null) return;

        string descripcionEncontrada = null;

        // Inspección inteligente de propiedades por si cambia el nombre del atributo en el modelo
        var propiedades = typeof(EspecieItem).GetProperties();
        foreach (var prop in propiedades)
        {
            string nombreProp = prop.Name.ToLower();
            if (nombreProp.Contains("descrip") || nombreProp.Contains("detalle") || nombreProp.Contains("info"))
            {
                var valor = prop.GetValue(especie)?.ToString();
                if (!string.IsNullOrWhiteSpace(valor))
                {
                    descripcionEncontrada = valor;
                    break;
                }
            }
        }

        // Si no se encontró texto o el campo está NULL en la BD, asignamos un texto claro por defecto
        if (string.IsNullOrWhiteSpace(descripcionEncontrada))
        {
            string nombreEspecie = string.IsNullOrWhiteSpace(especie.Nombre) ? "esta especie" : especie.Nombre;
            descripcionEncontrada = $"Especie de {nombreEspecie} registrada dentro del campus El Olivo de la Universidad Técnica del Norte.";
        }

        if (lblDescripcion != null)
        {
            lblDescripcion.Text = descripcionEncontrada;
        }
    }

    private async void OnRegresarTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnInicioTapped(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }
    private async void OnIdentificarTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new IdentificarPage());
    }
    private async void OnMapaTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MapaPage());
    }

    private async void OnPerfilTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PerfilPage());
    }
}