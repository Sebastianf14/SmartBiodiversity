using SmartBiodiversity;
using SmartBiodiversity.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

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

                if (response.IsSuccessStatusCode)
                {
                    string jsonString = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(jsonString);
                    var root = doc.RootElement;

                    // Extraemos el token devuelto por la API (busca "token" o "accessToken")
                    string token = "";
                    if (root.TryGetProperty("token", out var tokenProp))
                    {
                        token = tokenProp.GetString();
                    }
                    else if (root.TryGetProperty("accessToken", out var accessProp))
                    {
                        token = accessProp.GetString();
                    }

                    // GUARDAMOS EL TOKEN EN PREFERENCES
                    if (!string.IsNullOrEmpty(token))
                    {
                        Preferences.Default.Set("AuthToken", token);
                    }

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"---> Error en Login: {ex.Message}");
                return false;
            }
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
        // NUEVO MÉTODO AGREGADO PARA SOLUCIONAR EL ERROR:
        public async Task<List<EspecieItem>> ObtenerEspeciesAsync()
        {
            return await ObtenerEspeciesBaseDatosAsync();
        }
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

        // 1. SUBIR FOTO A SUPABASE STORAGE
        // ========================================================
        public async Task<(string url, string error)> SubirImagenSupabaseAsync(string rutaLocalFoto)
        {
            try
            {
                if (string.IsNullOrEmpty(rutaLocalFoto) || !File.Exists(rutaLocalFoto))
                    return (null, "El archivo de la foto no existe en el teléfono.");

                // === PON AQUÍ TUS DATOS REALES DE SUPABASE ===
                string supabaseUrl = "https://znfxeaownfgdhqonclwc.supabase.co";
                string supabaseApiKey = "sb_publishable_vBr2ON_MamjTmMJceDlnag_KfAucl9G";
                string bucketName = "especies-multimedia";
                // ============================================

                string nombreArchivo = $"aporte_{DateTime.Now:yyyyMMdd_HHmmss}_{Path.GetFileName(rutaLocalFoto)}";
                string uploadUrl = $"{supabaseUrl}/storage/v1/object/{bucketName}/{nombreArchivo}";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("apikey", supabaseApiKey);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseApiKey}");

                byte[] bytesImagen = await File.ReadAllBytesAsync(rutaLocalFoto);
                using var content = new ByteArrayContent(bytesImagen);
                content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

                var response = await client.PostAsync(uploadUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string urlPublica = $"{supabaseUrl}/storage/v1/object/public/{bucketName}/{nombreArchivo}";
                    return (urlPublica, null);
                }

                string errorRespuesta = await response.Content.ReadAsStringAsync();
                return (null, $"Error {response.StatusCode}: {errorRespuesta}");
            }
            catch (Exception ex)
            {
                return (null, $"Excepción: {ex.Message}");
            }
        }

        // ========================================================
        // 2. CREAR APORTE CON LA URL PÚBLICA DE LA IMAGEN
        // ========================================================
        // CREAR APORTE (INCLUYE TOKEN JWT DE AUTENTICACIÓN)
        public async Task<(bool exito, string mensaje)> CrearAporteAsync(string titulo, string descripcion, string urlImagenPublica)
        {
            try
            {
                // 1. Obtener el Token JWT guardado en las preferencias locales
                string token = Preferences.Default.Get("AuthToken", Preferences.Default.Get("TokenUsuario", ""));

                // 2. Adjuntar el token en el Header Authorization Bearer
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                string tituloFinal = string.IsNullOrWhiteSpace(titulo) ? "Avistamiento Campus" : titulo;
                string descFinal = descripcion ?? "";
                string rutaFinal = urlImagenPublica ?? "";

                // ========================================================
                // INTENTO 1: JSON Puro
                // ========================================================
                var payload = new
                {
                    tituloApo = tituloFinal,
                    descripcionApo = descFinal,
                    rutaArchivoApo = rutaFinal
                };

                string jsonPayload = JsonSerializer.Serialize(payload);
                using var contentJson = new StringContent(jsonPayload, Encoding.UTF8);
                contentJson.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await _httpClient.PostAsync($"{BaseUrl}/Aportes/crear", contentJson);

                // ========================================================
                // INTENTO 2: Form-Data si el controlador exige [FromForm]
                // ========================================================
                if (response.StatusCode == System.Net.HttpStatusCode.UnsupportedMediaType)
                {
                    using var formData = new MultipartFormDataContent();
                    formData.Add(new StringContent(tituloFinal), "tituloApo");
                    formData.Add(new StringContent(descFinal), "descripcionApo");
                    formData.Add(new StringContent(rutaFinal), "rutaArchivoApo");

                    response = await _httpClient.PostAsync($"{BaseUrl}/Aportes/crear", formData);
                }

                // ========================================================
                // VERIFICACIÓN DE RESPUESTA
                // ========================================================
                if (response.IsSuccessStatusCode)
                {
                    return (true, "¡Aporte registrado exitosamente!");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return (false, "Sesión expirada o no autorizada. Cierra sesión e ingresa de nuevo.");
                }

                string errorRespuesta = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(errorRespuesta))
                    errorRespuesta = $"Código HTTP {(int)response.StatusCode} ({response.ReasonPhrase})";

                return (false, errorRespuesta);
            }
            catch (Exception ex)
            {
                return (false, $"Error de conexión con el servidor: {ex.Message}");
            }

        }

        // REGISTRAR DESCARGA EN BITÁCORA (/api/Bitacora)
        public async Task<bool> RegistrarBitacoraAsync(string email, string accion, string detalle)
        {
            try
            {
                var payload = new
                {
                    idUsuarioBit = email,
                    accionBit = accion,
                    detalleBit = detalle
                };

                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/Bitacora", payload);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"---> Error en Bitácora: {ex.Message}");
                return false;
            }
        }
        // ==========================================
        // 3. FACULTADES Y ESTADÍSTICAS EXACTAS
        // ==========================================

        public async Task<List<FacultadItem>> ObtenerFacultadesAsync()
        {
            var lista = new List<FacultadItem>();
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/Facultades");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);

                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var elem in doc.RootElement.EnumerateArray())
                        {
                            string id = null;
                            string nombre = null;
                            string numero = null;

                            // Buscar ID (sea entero, GUID o string)
                            string[] propsId = new[] { "idFacultad", "id_facultad", "idFacultades", "id", "Id" };
                            foreach (var p in propsId)
                            {
                                if (elem.TryGetProperty(p, out var val))
                                {
                                    id = val.ValueKind == JsonValueKind.Number ? val.GetInt32().ToString() : val.GetString();
                                    if (!string.IsNullOrEmpty(id)) break;
                                }
                            }

                            // Buscar Nombre
                            string[] propsNombre = new[] { "nombre", "nombreFacultad", "nombre_facultad", "Nombre" };
                            foreach (var p in propsNombre)
                            {
                                if (elem.TryGetProperty(p, out var val) && val.ValueKind == JsonValueKind.String)
                                {
                                    nombre = val.GetString();
                                    if (!string.IsNullOrEmpty(nombre)) break;
                                }
                            }

                            // Buscar Número de facultad
                            string[] propsNum = new[] { "numero", "numeroFacultad", "num", "Numero" };
                            foreach (var p in propsNum)
                            {
                                if (elem.TryGetProperty(p, out var val))
                                {
                                    numero = val.ValueKind == JsonValueKind.Number ? val.GetInt32().ToString() : val.GetString();
                                    if (!string.IsNullOrEmpty(numero)) break;
                                }
                            }

                            if (!string.IsNullOrEmpty(id))
                            {
                                lista.Add(new FacultadItem
                                {
                                    IdFacultad = id,
                                    Nombre = nombre ?? "",
                                    Numero = numero ?? ""
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"---> Error al obtener facultades: {ex.Message}");
            }
            return lista;
        }

        public async Task<FacultadEspeciesStats> ObtenerEspeciesPorFacultadAsync(string idFacultad)
        {
            if (string.IsNullOrEmpty(idFacultad)) return null;

            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/Facultades/{idFacultad}/especies");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    int totalFlora = 0;
                    int totalFauna = 0;
                    string nombre = "";

                    // Extraer nombre
                    string[] propsNombre = new[] { "nombreFacultad", "nombre_facultad", "nombre" };
                    foreach (var p in propsNombre)
                    {
                        if (root.TryGetProperty(p, out var val) && val.ValueKind == JsonValueKind.String)
                        {
                            nombre = val.GetString();
                            break;
                        }
                    }

                    // Extraer Total Flora
                    string[] propsFlora = new[] { "totalFlora", "total_flora", "floraTotal" };
                    foreach (var p in propsFlora)
                    {
                        if (root.TryGetProperty(p, out var val) && val.ValueKind == JsonValueKind.Number)
                        {
                            totalFlora = val.GetInt32();
                            break;
                        }
                    }

                    // Extraer Total Fauna
                    string[] propsFauna = new[] { "totalFauna", "total_fauna", "faunaTotal" };
                    foreach (var p in propsFauna)
                    {
                        if (root.TryGetProperty(p, out var val) && val.ValueKind == JsonValueKind.Number)
                        {
                            totalFauna = val.GetInt32();
                            break;
                        }
                    }

                    // Respaldo: Si no vinieron los conteos numéricos, contar elementos dentro de los arreglos 'flora' y 'fauna'
                    if (totalFlora == 0 && root.TryGetProperty("flora", out var floraArr) && floraArr.ValueKind == JsonValueKind.Array)
                    {
                        totalFlora = floraArr.GetArrayLength();
                    }

                    if (totalFauna == 0 && root.TryGetProperty("fauna", out var faunaArr) && faunaArr.ValueKind == JsonValueKind.Array)
                    {
                        totalFauna = faunaArr.GetArrayLength();
                    }

                    return new FacultadEspeciesStats
                    {
                        NombreFacultad = nombre,
                        TotalFlora = totalFlora,
                        TotalFauna = totalFauna
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"---> Error especies facultad {idFacultad}: {ex.Message}");
            }

            return null;
        }
    }
    
}