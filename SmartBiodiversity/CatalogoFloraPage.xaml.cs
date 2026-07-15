namespace SmartBiodiversity;


public class EspecieItem
{
    public string Nombre { get; set; }
    public string DescripcionCorta { get; set; }
    public string DescripcionLarga { get; set; }
    public string Tamano { get; set; }
    public string Dieta { get; set; }
    public string Estado { get; set; }
}
public partial class CatalogoFloraPage : ContentPage
{
    public class PlantaItem
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        // public string ImagenUrl { get; set; } // ojo base
    }
    public CatalogoFloraPage()
	{
		InitializeComponent();
        CargarDatosSimulados();
    }
    private void CargarDatosSimulados()
    {
        var especiesFalsas = new List<EspecieItem>
    {
        new EspecieItem
        {
            Nombre = "Quishuar (Árbol)",
            DescripcionCorta = "Árbol nativo andino",
            DescripcionLarga = "El Quishuar es un árbol nativo de los Andes, muy común en las áreas verdes del campus El Olivo. Es reconocido por sus hojas verde grisáceas y su gran resistencia a las bajas temperaturas. Es fundamental para la conservación del suelo.",
            Tamano = "Hasta 8m de altura",
            Dieta = "Nutrientes del suelo y luz solar",
            Estado = "Preocupación Menor (LC)"
        },
        new EspecieItem
        {
            Nombre = "Orquídea de altura",
            DescripcionCorta = "Flor endémica",
            DescripcionLarga = "Especie de orquídea adaptada a los climas andinos. Florece durante la época de lluvias mostrando pétalos vibrantes. Suele encontrarse cerca de zonas húmedas.",
            Tamano = "15-30 cm",
            Dieta = "Fotosíntesis",
            Estado = "Vulnerable (VU)"
        },
        new EspecieItem
        {
            Nombre = "Helecho Arbóreo",
            DescripcionCorta = "Planta prehistórica",
            DescripcionLarga = "Planta fascinante con un tronco grueso y hojas frondosas que parecen plumas gigantes. Prefiere la sombra y aporta mucha humedad al ecosistema local.",
            Tamano = "2 a 5 metros",
            Dieta = "Fotosíntesis y humedad ambiental",
            Estado = "Casi Amenazado (NT)"
        }
    };

        listaFlora.ItemsSource = especiesFalsas;
    }

    private async void OnDetalleTapped(object sender, TappedEventArgs e)
    {
        // Atrapamos el elemento específico al que le dieron clic (ej. El Picaflor)
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
        // En la navegación de MAUI, ".." significa "volver a la página anterior".
        await Shell.Current.GoToAsync("..");
    }
    
}