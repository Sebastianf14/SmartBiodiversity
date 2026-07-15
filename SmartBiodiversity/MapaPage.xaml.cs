namespace SmartBiodiversity;

public class AuditorioItem
{
    public string Numero { get; set; }
    public string Facultad { get; set; }
    public string Nombre { get; set; }
    public string Capacidad { get; set; }
}
public partial class MapaPage : ContentPage
{
    double currentScale = 1;
    double startScale = 1;
    
    public MapaPage()
	{
		InitializeComponent();
        CargarTablaAuditorios();
    }
    private void CargarTablaAuditorios()
    {
        var auditorios = new List<AuditorioItem>
            {
                new AuditorioItem { Numero = "1", Facultad = "AUDITORIO", Nombre = "AGUSTÍN CUEVA", Capacidad = "470" },
                new AuditorioItem { Numero = "2", Facultad = "EDUCACIÓN FÍSICA", Nombre = "POLIDEPORTIVO", Capacidad = "2000" },
                new AuditorioItem { Numero = "3", Facultad = "FICA", Nombre = "AULA VIRTUAL FICA", Capacidad = "144" },
                new AuditorioItem { Numero = "4", Facultad = "FICAYA", Nombre = "AULA VIRTUAL FICAYA", Capacidad = "100" },
                new AuditorioItem { Numero = "5", Facultad = "FACAE", Nombre = "AULA VIRTUAL FACAE", Capacidad = "85" },
                new AuditorioItem { Numero = "6", Facultad = "FACAE", Nombre = "LABORATORIO COMPUTACIÓN", Capacidad = "40" },
                new AuditorioItem { Numero = "7", Facultad = "POSTGRADO AUDITORIO", Nombre = "-", Capacidad = "200" },
                new AuditorioItem { Numero = "8", Facultad = "EDIFICIO PRINCIPAL", Nombre = "JOSÉ MARTI", Capacidad = "80" },
                new AuditorioItem { Numero = "9", Facultad = "CIENCIAS DE LA SALUD CCSS", Nombre = "AULA VIRTUAL CCSS", Capacidad = "20" },
                new AuditorioItem { Numero = "10", Facultad = "FACAE Y FECYT", Nombre = "5 AULAS LAS MÁS AMPLIAS", Capacidad = "40 c/u" }
            };

        listaAuditorios.ItemsSource = auditorios;
    }

    private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
    {
        if (e.Status == GestureStatus.Started)
        {
            startScale = imgMapa.Scale;
        }
        if (e.Status == GestureStatus.Running)
        {

            currentScale += (e.Scale - 1) * startScale;
            currentScale = Math.Max(1, currentScale); // Evita que se haga más pequeña que el original
            currentScale = Math.Min(currentScale, 4); // Límite de zoom máximo (4x)

            imgMapa.Scale = currentScale;
        }
    }

    private async void OnInicioTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(".."); // Vuelve al Dashboard
    }

    private async void OnPerfilTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("PerfilPage");
    }
}