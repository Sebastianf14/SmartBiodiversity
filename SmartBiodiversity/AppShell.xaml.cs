namespace SmartBiodiversity
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            // Registramos las rutas para que la app sepa cómo llegar al Dashboard
            Routing.RegisterRoute("DashboardPage", typeof(DashboardPage));
            Routing.RegisterRoute("CatalogoFloraPage", typeof(CatalogoFloraPage));
            Routing.RegisterRoute("CatalogoFaunaPage", typeof(CatalogoFaunaPage));
            Routing.RegisterRoute("PerfilPage", typeof(PerfilPage));
            Routing.RegisterRoute("MapaPage", typeof(MapaPage));
            Routing.RegisterRoute("DetalleItemPage", typeof(DetalleItemPage));
        }
    }
}
