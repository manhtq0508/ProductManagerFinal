using ProductManager.ViewModels;

namespace ProductManager.Views;

public partial class EditProductPage : ContentPage
{
	public EditProductPage(EditProductViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;
	}
}