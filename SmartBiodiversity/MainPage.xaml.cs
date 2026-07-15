namespace SmartBiodiversity
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            await DisplayAlert("¡Excelente!", "El botón de iniciar sesión funciona", "OK");
        }

        private async void OnGoToRegisterTapped(object sender, EventArgs e)
        {

            await DisplayAlert("Navegación", "Aquí iremos a la pantalla de Registro", "OK");
        }
    }

}
