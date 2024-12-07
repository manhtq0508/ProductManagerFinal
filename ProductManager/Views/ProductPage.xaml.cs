using ProductManager.ViewModels;

namespace ProductManager.Views;

public partial class ProductPage : ContentPage
{
	public ProductPage(ProductViewModel vm)
	{
		InitializeComponent();

        BindingContext = vm;
    }
}