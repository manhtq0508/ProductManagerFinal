using ProductManager.ViewModels;

namespace ProductManager.Views;

public partial class AddProductPage : ContentPage
{
	public AddProductPage(AddProductViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;
	}
}