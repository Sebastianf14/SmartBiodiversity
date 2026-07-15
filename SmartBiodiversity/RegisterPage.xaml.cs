namespace SmartBiodiversity;

public partial class RegisterPage : ContentPage
{
	public RegisterPage()
	{
		InitializeComponent();
	}
    // Simula la creación de la cuenta
    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        // Validamos que llenen los campos
        if (string.IsNullOrEmpty(txtNombre.Text) || string.IsNullOrEmpty(txtEmailReg.Text))
        {
            await DisplayAlert("Atención", "Por favor llena todos los campos", "OK");
            return;
        }

        // Validamos que las contraseñas coincidan
        if (txtPasswordReg.Text != txtConfirmPassword.Text)
        {
            await DisplayAlert("Error", "Las contraseñas no coinciden", "OK");
            return;
        }

        // Simulamos éxito y volvemos a la pantalla anterior
        await DisplayAlert("Éxito", "Cuenta creada correctamente", "Empezar");
        await Navigation.PopAsync();
    }

    // Vuelve a la pantalla de Login sin registrar nada
    private async void OnGoToLoginTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}