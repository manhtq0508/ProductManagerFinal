using ProductManager.Views;

namespace ProductManager
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            // Store
            Routing.RegisterRoute(nameof(StorePage), typeof(StorePage));
            Routing.RegisterRoute(nameof(AddStorePage), typeof(AddStorePage));
            Routing.RegisterRoute(nameof(EditStorePage), typeof(EditStorePage));

            // Product
            Routing.RegisterRoute(nameof(ProductPage), typeof(ProductPage));
        }
    }
}
