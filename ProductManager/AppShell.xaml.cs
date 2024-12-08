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
            Routing.RegisterRoute(nameof(AddProductPage), typeof(AddProductPage));
            Routing.RegisterRoute(nameof(EditProductPage), typeof(EditProductPage));

            // Bill
            Routing.RegisterRoute(nameof(BillPage), typeof(BillPage));
            Routing.RegisterRoute(nameof(AddBillPage), typeof(AddBillPage));
            Routing.RegisterRoute(nameof(EditBillPage), typeof(EditBillPage));

            // BillDetail
            Routing.RegisterRoute(nameof(SelectProductInBillPage), typeof(SelectProductInBillPage));
            Routing.RegisterRoute(nameof(BillDetailPage), typeof(BillDetailPage));
        }
    }
}
