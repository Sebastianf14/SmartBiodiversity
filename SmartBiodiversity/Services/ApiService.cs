using System.Text.Json;
using System.Net.Http.Json;
using System.Text;

namespace SmartBiodiversity.Services
{
    public class ApiService
    {
        // Esta URL cambiará cuando tu compañero publique la API en la nube
        // Por ahora, es un ejemplo de cómo se verá
        private readonly string BaseUrl = "https://smartbiodiversityapi.onrender.com/api";
        private readonly HttpClient _httpClient;

        public ApiService()
        {
            _httpClient = new HttpClient();
        }

        // 1. LOGIN
        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                // El backend espera "email" y "password"
                var payload = new { email = email, password = password };
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/Auth/login", payload);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de conexión: {ex.Message}");
                return false;
            }
        }

        // 2. ENVIAR CÓDIGO DE VERIFICACIÓN
        public async Task<(bool exito, string mensaje)> SolicitarCodigoAsync(string email)
        {
            try
            {
                var payload = new { email = email };
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/Auth/send-verification-code", payload);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "OK");
                }
                else
                {
                    // Atrapamos el texto de error que nos manda el servidor
                    string errorDelServidor = await response.Content.ReadAsStringAsync();
                    return (false, $"Código {(int)response.StatusCode}: {errorDelServidor}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Fallo de red o Render dormido: {ex.Message}");
            }
        }

        // 3. VERIFICAR CÓDIGO
        public async Task<bool> VerificarCodigoAsync(string email, string codigo)
        {
            try
            {
                // El backend espera "email" y "codigo"
                var payload = new { email = email, codigo = codigo };
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/Auth/verify-code", payload);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // 4. REGISTRAR USUARIO
        public async Task<bool> RegistrarUsuarioAsync(string nombres, string apellidos, string correo, string password, string codigoVerif)
        {
            try
            {
                // Aquí el backend espera "correo" y "codigoVerificacion"
                var payload = new
                {
                    nombres = nombres,
                    apellidos = apellidos,
                    correo = correo,
                    password = password,
                    codigoVerificacion = codigoVerif
                };

                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/Auth/register", payload);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        // Método que usaremos para pedir la lista de Flora
        public async Task<List<EspecieItem>> ObtenerFloraAsync()
        {
            try
            {
                // Aquí vamos a la URL de la API (ej: .../api/flora)
                var respuesta = await _httpClient.GetAsync($"{BaseUrl}/flora");

                if (respuesta.IsSuccessStatusCode)
                {
                    // Convertimos el JSON que manda la API a nuestra lista de C#
                    var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var flora = await respuesta.Content.ReadFromJsonAsync<List<EspecieItem>>(opciones);
                    return flora ?? new List<EspecieItem>();
                }
                return new List<EspecieItem>();
            }
            catch (Exception ex)
            {
                // Si no hay internet o la API falla
                Console.WriteLine($"Error al conectar con la API: {ex.Message}");
                return new List<EspecieItem>();
            }
        }
    }
}
