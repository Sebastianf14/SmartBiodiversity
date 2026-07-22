namespace SmartBiodiversity;
using Microsoft.Maui.Networking;
using SmartBiodiversity.Services;

public partial class RegisterPage : ContentPage
{
    private readonly ApiService _apiService = new ApiService();

    public RegisterPage()
    {
        InitializeComponent();
    }

    private bool EsClaveSegura(string clave)
    {
        if (string.IsNullOrWhiteSpace(clave)) return false;

        // Verifica que tenga al menos 8 caracteres, una letra mayúscula y un número
        bool tieneLongitud = clave.Length >= 8;
        bool tieneMayuscula = clave.Any(char.IsUpper);
        bool tieneNumero = clave.Any(char.IsDigit);

        return tieneLongitud && tieneMayuscula && tieneNumero;
    }

    // PASO 1: Validar datos, comprobar internet y enviar código
    private async void OnEnviarCodigoClicked(object sender, EventArgs e)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            await DisplayAlert("Sin Internet", "Revisa tu conexión a internet e inténtalo de nuevo.", "OK");
            return;
        }

        if (string.IsNullOrEmpty(txtNombre.Text) || string.IsNullOrEmpty(txtEmailReg.Text))
        {
            await DisplayAlert("Atención", "Por favor llena todos los campos.", "OK");
            return;
        }

        if (!EsClaveSegura(txtPasswordReg.Text))
        {
            await DisplayAlert("Clave Insegura", "La contraseña debe tener al menos 8 caracteres, incluir una mayúscula y un número.", "Entendido");
            return;
        }

        if (txtPasswordReg.Text != txtConfirmPassword.Text)
        {
            await DisplayAlert("Error", "Las contraseñas no coinciden.", "OK");
            return;
        }

        // --- LLAMADA A LA API REAL ---
        var resultado = await _apiService.SolicitarCodigoAsync(txtEmailReg.Text);

        if (resultado.exito)
        {
            PanelRegistro.IsVisible = false;
            PanelVerificacion.IsVisible = true;
        }
        else
        {
            // Mostrará exactamente lo que falló en el backend
            await DisplayAlert("Error del Servidor", resultado.mensaje, "Entendido");
        }
    }

    // PASO 2: Comprobar el código y crear la cuenta real en la base de datos
    private async void OnVerificaryCrearClicked(object sender, EventArgs e)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            await DisplayAlert("Sin Internet", "Revisa tu conexión.", "OK");
            return;
        }

        string codigoIngresado = txtCodigoVerificacion.Text;

        if (string.IsNullOrEmpty(codigoIngresado) || codigoIngresado.Length < 6)
        {
            await DisplayAlert("Atención", "Ingresa el código completo de 6 dígitos.", "OK");
            return;
        }

        try
        {
            // 1. Verificamos el código desestructurando (exito, mensaje)
            var (codigoValido, mensajeVerif) = await _apiService.VerificarCodigoAsync(txtEmailReg.Text, codigoIngresado);

            if (codigoValido)
            {
                // 2. Si el código es válido, creamos la cuenta real
                var resultadoRegistro = await _apiService.RegistrarUsuarioAsync(
                    nombres: txtNombre.Text,
                    apellidos: "",
                    correo: txtEmailReg.Text,
                    password: txtPasswordReg.Text,
                    codigoVerif: codigoIngresado);

                if (resultadoRegistro.exito)
                {
                    await DisplayAlert("¡Bienvenido!", "Tu cuenta ha sido creada y verificada exitosamente.", "Iniciar Sesión");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Error en Registro", resultadoRegistro.mensaje, "OK");
                }
            }
            else
            {
                await DisplayAlert("Código Incorrecto", mensajeVerif, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error Inesperado", ex.Message, "OK");
        }
    }

    // PASO 3: Reenviar el código si el usuario no lo recibió
    private async void OnReenviarCodigoClicked(object sender, EventArgs e)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            await DisplayAlert("Sin Internet", "Revisa tu conexión a internet.", "OK");
            return;
        }

        var resultado = await _apiService.SolicitarCodigoAsync(txtEmailReg.Text);

        if (resultado.exito)
        {
            await DisplayAlert("Enviado", "Hemos enviado un nuevo código a tu correo.", "OK");
        }
        else
        {
            await DisplayAlert("Error", resultado.mensaje, "OK");
        }
    }

    // Vuelve a la pantalla de Login sin registrar nada
    private async void OnGoToLoginTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}