namespace SmartBiodiversity;

using SmartBiodiversity.Services;

public partial class MapaPage : ContentPage
{
    private readonly ApiService _apiService = new ApiService();

    public MapaPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CargarMapaLeaflet();
        CargarEstadisticasBiodiversidad();
    }

    // 1. CARGAR MAPA INTERACTIVO DE LEAFLET.JS CON OPENSTREETMAP
    private void CargarMapaLeaflet()
    {
        // Utilizamos un string literal (@"") para poder escribir HTML y JS libremente.
        // Usamos comillas simples (') o backticks (`) en el HTML/JS para evitar conflictos con C#.
        string htmlContent = @"
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset='utf-8' />
        <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no' />
        <link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css' />
        <script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
        <style>
            html, body, #map { height: 100%; width: 100%; margin: 0; padding: 0; font-family: Arial, Helvetica, sans-serif; }
            .leaflet-popup-content-wrapper { border-radius: 12px; box-shadow: 0 4px 15px rgba(0,0,0,0.15); }
            
            /* Estilos del Popup */
            .popup-title { font-weight: bold; color: #16a34a; margin-bottom: 5px; font-size: 14px; }
            .popup-desc { font-size: 12px; color: #6b7280; margin-bottom: 8px; }
            .popup-stats { font-size: 12px; color: #111827; }
            
            /* Estilos del Marcador Dinámico */
            .custom-marker {
                background: #22c55e;
                color: white;
                border-radius: 50%;
                width: 34px;
                height: 34px;
                display: flex;
                align-items: center;
                justify-content: center;
                font-weight: bold;
                font-size: 14px;
                border: 3px solid white;
                box-shadow: 0 2px 8px rgba(0,0,0,0.3);
            }
        </style>
    </head>
    <body>
        <div id='map'></div>
        <script>
            // Coordenadas del Campus El Olivo - UTN (Ibarra, Ecuador)
            var latUTN = 0.35825;
            var lngUTN = -78.11180;

            var map = L.map('map', { zoomControl: false }).setView([latUTN, lngUTN], 17);

            L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
                maxZoom: 19,
                attribution: '© OpenStreetMap'
            }).addTo(map);

            // Integración de la API
            const API_BASE = 'https://smartbiodiversityapi.onrender.com';

            async function loadFacultades() {
                try {
                    const resp = await fetch(`${API_BASE}/api/Facultades`);
                    if (!resp.ok) throw new Error(`Error HTTP: ${resp.status}`);

                    const facultades = await resp.json();
                    let markers = [];

                    facultades.forEach(fac => {
                        // Crear el ícono personalizado con el número de facultad
                        const icon = L.divIcon({
                            className: '',
                            html: `<div class='custom-marker'>${fac.numero || '•'}</div>`,
                            iconSize: [34, 34],
                            iconAnchor: [17, 17],
                            popupAnchor: [0, -17]
                        });

                        // Agregar marcador al mapa utilizando latitud y longitud de la API
                        const marker = L.marker([fac.latitud, fac.longitud], { icon }).addTo(map);

                        // Agregar el Popup con los datos extraídos
                        marker.bindPopup(`
                            <div class='popup-title'>${fac.nombre}</div>
                            <div class='popup-desc'>${fac.descripcion || 'Sin descripción'}</div>
                            <div class='popup-stats'>
                                <strong>${fac.totalEspecies || 0}</strong> especies registradas
                            </div>
                        `);

                        markers.push(marker);
                    });

                    // Opcional: Centrar el mapa automáticamente para que todas las facultades se vean
                    if (markers.length > 0) {
                        var group = L.featureGroup(markers);
                        map.fitBounds(group.getBounds().pad(0.2));
                    }

                } catch (err) {
                    console.error('Error cargando facultades desde la API:', err);
                }
            }

            // Ejecutar la función al cargar el script
            loadFacultades();
        </script>
    </body>
    </html>";

        webMapView.Source = new HtmlWebViewSource
        {
            Html = htmlContent
        };
    }

    // 2. OBTENER FLORA Y FAUNA DE LA API Y MOSTRAR CONTEO EN NÚMEROS
    private async void CargarEstadisticasBiodiversidad()
    {
        try
        {
            // 1. Cargar totales globales superiores
            var listaFlora = await _apiService.ObtenerFloraAsync();
            var listaFauna = await _apiService.ObtenerFaunaAsync();

            int totalFlora = listaFlora != null ? listaFlora.Count : 0;
            int totalFauna = listaFauna != null ? listaFauna.Count : 0;

            lblTotalFlora.Text = $"{totalFlora} especies";
            lblTotalFauna.Text = $"{totalFauna} especies";

            // 2. Obtener la lista de facultades desde la API
            var facultades = await _apiService.ObtenerFacultadesAsync();

            if (facultades != null && facultades.Count > 0)
            {
                foreach (var fac in facultades)
                {
                    // Consultar datos reales de la facultad actual
                    var stats = await _apiService.ObtenerEspeciesPorFacultadAsync(fac.IdFacultad);

                    if (stats != null)
                    {
                        string nombre = fac.Nombre?.ToUpper() ?? "";
                        string num = fac.Numero ?? "";

                        // Match dinámico por Nombre Completo, Siglas o Número de posición:

                        // 1. FICA
                        if (nombre.Contains("FICA") || nombre.Contains("INGENIER") || nombre.Contains("APLICADAS") || num == "1")
                        {
                            lblFicaFlora.Text = stats.TotalFlora.ToString();
                            lblFicaFauna.Text = stats.TotalFauna.ToString();
                        }
                        // 2. FICAYA
                        else if (nombre.Contains("FICAYA") || nombre.Contains("AGRO") || nombre.Contains("AMBIENT") || num == "2")
                        {
                            lblFicayaFlora.Text = stats.TotalFlora.ToString();
                            lblFicayaFauna.Text = stats.TotalFauna.ToString();
                        }
                        // 3. FECYT
                        else if (nombre.Contains("FECYT") || nombre.Contains("EDUCAC") || nombre.Contains("TECNOLOG") || num == "3")
                        {
                            lblFecytFlora.Text = stats.TotalFlora.ToString();
                            lblFecytFauna.Text = stats.TotalFauna.ToString();
                        }
                        // 4. FACAE
                        else if (nombre.Contains("FACAE") || nombre.Contains("ADMINISTRA") || nombre.Contains("ECONOMIC") || num == "4")
                        {
                            lblFacaeFlora.Text = stats.TotalFlora.ToString();
                            lblFacaeFauna.Text = stats.TotalFauna.ToString();
                        }
                        // 5. FACS
                        else if (nombre.Contains("FACS") || nombre.Contains("SALUD") || num == "5")
                        {
                            lblFacsFlora.Text = stats.TotalFlora.ToString();
                            lblFacsFauna.Text = stats.TotalFauna.ToString();
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"---> Error al cargar estadísticas: {ex.Message}");
        }
    }

    // NAVEGACIÓN BARRA INFERIOR
    private async void OnInicioTapped(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
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