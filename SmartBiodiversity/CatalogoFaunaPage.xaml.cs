namespace SmartBiodiversity;

public partial class CatalogoFaunaPage : ContentPage
{
    public class PlantaItem
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        // public string ImagenUrl { get; set; } // se usa cuando se conecte la base ojo
    }
    public CatalogoFaunaPage()
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
            Nombre = "Perro 'Negrita'",
            DescripcionCorta = "Mascota comunitaria",
            DescripcionLarga = "Negrita es una de las perritas comunitarias más queridas por los estudiantes. Suele descansar cerca de las facultades y es muy amigable. Siempre está atenta a quienes le invitan un bocadillo.",
            Tamano = "Mediano (15-20 kg)",
            Dieta = "Croquetas y bocadillos de estudiantes",
            Estado = "Fuera de peligro (Mascota comunitaria)"
        },
        new EspecieItem
        {
            Nombre = "Zorro de las montañas",
            DescripcionCorta = "Cánido silvestre andino",
            DescripcionLarga = "También conocido como zorro andino o culpeo. Ocasionalmente avistado en los límites menos transitados del campus al atardecer. Es escurridizo y mantiene el equilibrio ecológico controlando plagas.",
            Tamano = "70-90 cm de largo",
            Dieta = "Pequeños roedores, insectos y frutos",
            Estado = "Preocupación Menor (LC)"
        },
        new EspecieItem
        {
            Nombre = "Mariposas",
            DescripcionCorta = "Polinizadores locales",
            DescripcionLarga = "Diversas especies de mariposas diurnas que revolotean por los jardines del campus, especialmente durante los meses más cálidos y soleados. Son indicadores de una buena salud ambiental.",
            Tamano = "Variable (3-10 cm de envergadura)",
            Dieta = "Néctar de las flores (Flora local)",
            Estado = "No evaluado"
        }
    };
        listaFauna.ItemsSource = especiesFalsas;
    }
    private async void OnDetalleTapped(object sender, TappedEventArgs e)
    {
        // Atrapamos el elemento específico al que le dieron clic
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