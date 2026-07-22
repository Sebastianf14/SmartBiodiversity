using System.Text.Json;
using System.Net.Http.Json;
using System.Text;
using SmartBiodiversity.Models;
using SmartBiodiversity;

namespace SmartBiodiversity.Services
{
    public class ApiService
    {
        private readonly string BaseUrl = "https://smartbiodiversityapi.onrender.com/api";
        private readonly HttpClient _httpClient;

        public ApiService()
        {
            _httpClient = new HttpClient();
        }

        // ==========================================
        // 1. AUTENTICACIÓN Y REGISTRO
        // ==========================================

        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                var payload = new { email = email, password = password };
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/Auth/login", payload);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<(bool exito, string mensaje)> SolicitarCodigoAsync(string email)
        {
            try
            {
                var payload = new { email = email };
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/Auth/send-verification-code", payload);

                if (response.IsSuccessStatusCode) return (true, "OK");
                string error = await response.Content.ReadAsStringAsync();
                return (false, $"Código {(int)response.StatusCode}: {error}");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<bool> VerificarCodigoAsync(string email, string codigo)
        {
            try
            {
                var payload = new { email = email, codigo = codigo };
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/Auth/verify-code", payload);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<(bool exito, string mensaje)> RegistrarUsuarioAsync(string nombres, string apellidos, string correo, string password, string codigoVerif)
        {
            try
            {
                string apellidoValido = string.IsNullOrWhiteSpace(apellidos) ? "N/A" : apellidos;
                var payload = new
                {
                    id_usuario = Guid.NewGuid().ToString(),
                    id_rolesusu = "1",
                    nombres = nombres,
                    apellidos = apellidoValido,
                    correo = correo,
                    password = password,
                    codigoVerificacion = codigoVerif,
                    estado = "ACTIVO",
                    fecha_registro = DateTime.Now.ToString("yyyy-MM-dd"),
                    intentos_fallidos = 0
                };

                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/Auth/register", payload);
                if (response.IsSuccessStatusCode) return (true, "OK");

                string error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        // ==========================================
        // 2. ESPECIES Y MULTIMEDIA
        // ==========================================

        public async Task<List<CategoriaItem>> ObtenerCategoriasAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/Categorias");
                if (response.IsSuccessStatusCode)
                {
                    var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return await response.Content.ReadFromJsonAsync<List<CategoriaItem>>(opciones) ?? new List<CategoriaItem>();
                }
            }
            catch { }
            return new List<CategoriaItem>();
        }

        // OBTENER SÓLO FLORA
        public async Task<List<EspecieItem>> ObtenerFloraAsync()
        {
            var todas = await ObtenerEspeciesBaseDatosAsync();
            if (todas == null || todas.Count == 0) return new List<EspecieItem>();

            var categorias = await ObtenerCategoriasAsync();
            var catFlora = categorias.FirstOrDefault(c =>
                !string.IsNullOrEmpty(c.Nombre) &&
                (c.Nombre.ToLower().Contains("flora") || c.Nombre.ToLower().Contains("planta")));

            if (catFlora != null)
            {
                var filtradas = todas.Where(e =>
                    !string.IsNullOrEmpty(e.CategoriaId) &&
                    e.CategoriaId.Equals(catFlora.Id, StringComparison.OrdinalIgnoreCase)).ToList();

                if (filtradas.Count > 0) return filtradas;
            }

            // Filtro por la ruta de la imagen en Supabase (/Flora/) o el nombre común
            var filtradasPorRuta = todas.Where(e =>
                (!string.IsNullOrEmpty(e.ImagenUrl) && e.ImagenUrl.ToLower().Contains("/flora/")) ||
                (!string.IsNullOrEmpty(e.Nombre) && (e.Nombre.ToLower().Contains("orquídea") || e.Nombre.ToLower().Contains("árbol") || e.Nombre.ToLower().Contains("planta") || e.Nombre.ToLower().Contains("flor")))
            ).ToList();

            return filtradasPorRuta;
        }

        // OBTENER SÓLO FAUNA
        public async Task<List<EspecieItem>> ObtenerFaunaAsync()
        {
            var todas = await ObtenerEspeciesBaseDatosAsync();
            if (todas == null || todas.Count == 0) return new List<EspecieItem>();

            var categorias = await ObtenerCategoriasAsync();
            var catFauna = categorias.FirstOrDefault(c =>
                !string.IsNullOrEmpty(c.Nombre) &&
                (c.Nombre.ToLower().Contains("fauna") || c.Nombre.ToLower().Contains("animal")));

            if (catFauna != null)
            {
                var filtradas = todas.Where(e =>
                    !string.IsNullOrEmpty(e.CategoriaId) &&
                    e.CategoriaId.Equals(catFauna.Id, StringComparison.OrdinalIgnoreCase)).ToList();

                if (filtradas.Count > 0) return filtradas;
            }

            // Filtro por la ruta de la imagen en Supabase (/Fauna/) o el nombre común
            var filtradasPorRuta = todas.Where(e =>
                (!string.IsNullOrEmpty(e.ImagenUrl) && e.ImagenUrl.ToLower().Contains("/fauna/")) ||
                (!string.IsNullOrEmpty(e.Nombre) && (e.Nombre.ToLower().Contains("sapo") || e.Nombre.ToLower().Contains("sebra") || e.Nombre.ToLower().Contains("perro") || e.Nombre.ToLower().Contains("zorro") || e.Nombre.ToLower().Contains("ave") || e.Nombre.ToLower().Contains("picaflor")))
            ).ToList();

            return filtradasPorRuta;
        }

        // MÉTODO PRINCIPAL QUE CONSUME LAS ESPECIES
        private async Task<List<EspecieItem>> ObtenerEspeciesBaseDatosAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/Especies");

                if (response.IsSuccessStatusCode)
                {
                    var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var especies = await response.Content.ReadFromJsonAsync<List<EspecieItem>>(opciones);

                    if (especies != null && especies.Count > 0)
                    {
                        // Traemos el mapa global de fotos por si acaso
                        var mapaFotos = await ObtenerTodasLasMultimediasAsync();

                        foreach (var esp in especies)
                        {
                            // 1. Intentamos consultar la foto por ID individual
                            string urlFoto = null;
                            if (!string.IsNullOrEmpty(esp.Id))
                            {
                                urlFoto = await ObtenerImagenEspecieAsync(esp.Id);
                            }

                            // 2. Respaldo: si falló, buscamos el ID en el mapa global de la tabla multimedia
                            if (string.IsNullOrEmpty(urlFoto) && !string.IsNullOrEmpty(esp.Id) && mapaFotos.ContainsKey(esp.Id))
                            {
                                urlFoto = mapaFotos[esp.Id];
                            }

                            // 3. Asignamos la imagen encontrada
                            if (!string.IsNullOrEmpty(urlFoto))
                            {
                                esp.ImagenUrl = urlFoto;
                            }
                        }
                        return especies;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"---> Error especies: {ex.Message}");
            }

            return new List<EspecieItem>();
        }

        // EXTRACTOR MULTIMEDIA AJUSTADO A PostgreSQL (RutaArchivoMul)
        // BUSCA MULTIMEDIA POR ID
        private async Task<string> ObtenerImagenEspecieAsync(string especieId)
        {
            if (string.IsNullOrEmpty(especieId)) return null;

            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/Multimedias/{especieId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);

                    string ExtraerRuta(JsonElement elem)
                    {
                        // Nombres exactos de PostgreSQL (ruta_archivomul)
                        string[] propiedades = new[] {
                            "ruta_archivomul", "rutaArchivoMul", "RutaArchivoMul",
                            "ruta_archivo", "rutaArchivo", "RutaArchivo",
                            "ruta", "Ruta", "url", "Url"
                        };

                        foreach (var prop in propiedades)
                        {
                            if (elem.TryGetProperty(prop, out var val) && val.ValueKind == JsonValueKind.String)
                            {
                                string str = val.GetString();
                                if (!string.IsNullOrWhiteSpace(str)) return str;
                            }
                        }

                        // Detector comodín de enlaces de Supabase
                        foreach (var prop in elem.EnumerateObject())
                        {
                            if (prop.Value.ValueKind == JsonValueKind.String)
                            {
                                string strVal = prop.Value.GetString();
                                if (!string.IsNullOrEmpty(strVal) && (strVal.StartsWith("http") || strVal.Contains("/storage/")))
                                {
                                    return strVal;
                                }
                            }
                        }
                        return null;
                    }

                    if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
                    {
                        return ExtraerRuta(doc.RootElement[0]);
                    }
                    else if (doc.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        return ExtraerRuta(doc.RootElement);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"---> Error multimedia individual: {ex.Message}");
            }

            return null;
        }
        // MAPA GLOBAL DE RESPALDO (Lee la tabla Multimedia completa)
        private async Task<Dictionary<string, string>> ObtenerTodasLasMultimediasAsync()
        {
            var mapa = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/Multimedias");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);

                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var elem in doc.RootElement.EnumerateArray())
                        {
                            string idEspecie = null;
                            string ruta = null;

                            // Nombres de la clave foránea en SQL (id_especiesmul)
                            string[] propsId = new[] { "id_especiesmul", "idEspeciesMul", "idEspeciesmul", "especieId" };
                            foreach (var p in propsId)
                            {
                                if (elem.TryGetProperty(p, out var val) && val.ValueKind == JsonValueKind.String)
                                {
                                    idEspecie = val.GetString();
                                    break;
                                }
                            }

                            // Nombres de la ruta en SQL (ruta_archivomul)
                            string[] propsRuta = new[] { "ruta_archivomul", "rutaArchivoMul", "RutaArchivoMul", "rutaArchivo", "url" };
                            foreach (var p in propsRuta)
                            {
                                if (elem.TryGetProperty(p, out var val) && val.ValueKind == JsonValueKind.String)
                                {
                                    ruta = val.GetString();
                                    break;
                                }
                            }

                            if (!string.IsNullOrEmpty(idEspecie) && !string.IsNullOrEmpty(ruta))
                            {
                                mapa[idEspecie] = ruta;
                            }
                        }
                    }
                }
            }
            catch { }

            return mapa;
        }
    }
}