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

        // 1. AUTENTICACIÓN Y REGISTRO

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

        // VERIFICAR CÓDIGO (MEJORADO PARA MOSTRAR LA RESPUESTA REAL DE LA API)
        public async Task<(bool exito, string mensaje)> VerificarCodigoAsync(string email, string codigo)
        {
            try
            {
                var payload = new { email = email, codigo = codigo };
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/Auth/verify-code", payload);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "OK");
                }
                else
                {
                    string errorServidor = await response.Content.ReadAsStringAsync();
                    return (false, $"HTTP {(int)response.StatusCode}: {errorServidor}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error de red: {ex.Message}");
            }
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

        // SOLICITAR CÓDIGO DE OLVIDÉ CONTRASEÑA (/api/Auth/forgot-password)
        public async Task<(bool exito, string mensaje)> SolicitarCodigoOlvidePasswordAsync(string email)
        {
            try
            {
                var payload = new { email = email };
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/Auth/forgot-password", payload);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "OK");
                }
                string error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // RESTABLECER CONTRASEÑA CON EL CÓDIGO/TOKEN (/api/Auth/reset-password)
        // RESTABLECER CONTRASEÑA (/api/Auth/reset-password)
        // RESTABLECER CONTRASEÑA (/api/Auth/reset-password)
        public async Task<(bool exito, string mensaje)> RestablecerPasswordAsync(string email, string token, string nuevaPassword)
        {
            try
            {
                // JSON formateado exactamente con las propiedades que valida el DTO de ASP.NET Core
                var payload = new
                {
                    Email = email,
                    Token = token,
                    NewPassword = nuevaPassword
                };

                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/Auth/reset-password", payload);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Contraseña actualizada exitosamente.");
                }

                string errorRespuesta = await response.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(errorRespuesta) ? "Error al procesar la solicitud." : errorRespuesta);
            }
            catch (Exception ex)
            {
                return (false, $"Error de conexión con el servidor: {ex.Message}");
            }
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

            var filtradasPorRuta = todas.Where(e =>
                (!string.IsNullOrEmpty(e.ImagenUrl) && e.ImagenUrl.ToLower().Contains("/flora/")) ||
                (!string.IsNullOrEmpty(e.Nombre) && (e.Nombre.ToLower().Contains("orquídea") || e.Nombre.ToLower().Contains("árbol") || e.Nombre.ToLower().Contains("planta") || e.Nombre.ToLower().Contains("flor")))
            ).ToList();

            return filtradasPorRuta;
        }

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

            var filtradasPorRuta = todas.Where(e =>
                (!string.IsNullOrEmpty(e.ImagenUrl) && e.ImagenUrl.ToLower().Contains("/fauna/")) ||
                (!string.IsNullOrEmpty(e.Nombre) && (e.Nombre.ToLower().Contains("sapo") || e.Nombre.ToLower().Contains("sebra") || e.Nombre.ToLower().Contains("perro") || e.Nombre.ToLower().Contains("zorro") || e.Nombre.ToLower().Contains("ave") || e.Nombre.ToLower().Contains("picaflor")))
            ).ToList();

            return filtradasPorRuta;
        }

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
                        var mapaFotos = await ObtenerTodasLasMultimediasAsync();

                        foreach (var esp in especies)
                        {
                            string urlFoto = null;
                            if (!string.IsNullOrEmpty(esp.Id))
                            {
                                urlFoto = await ObtenerImagenEspecieAsync(esp.Id);
                            }

                            if (string.IsNullOrEmpty(urlFoto) && !string.IsNullOrEmpty(esp.Id) && mapaFotos.ContainsKey(esp.Id))
                            {
                                urlFoto = mapaFotos[esp.Id];
                            }

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

                            string[] propsId = new[] { "id_especiesmul", "idEspeciesMul", "idEspeciesmul", "especieId" };
                            foreach (var p in propsId)
                            {
                                if (elem.TryGetProperty(p, out var val) && val.ValueKind == JsonValueKind.String)
                                {
                                    idEspecie = val.GetString();
                                    break;
                                }
                            }

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
        // CREAR APORTE DE AVISTAMIENTO (/api/Aportes/crear)
        public async Task<(bool exito, string mensaje)> CrearAporteAsync(string titulo, string descripcion, string rutaImagen)
        {
            try
            {
                var payload = new
                {
                    tituloApo = string.IsNullOrWhiteSpace(titulo) ? "Avistamiento" : titulo,
                    descripcionApo = descripcion,
                    rutaArchivoApo = rutaImagen ?? ""
                };

                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/Aportes/crear", payload);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "¡Aporte registrado exitosamente!");
                }

                string error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }
            catch (Exception ex)
            {
                return (false, $"Error al conectar con el servidor: {ex.Message}");
            }
        }
    }
}