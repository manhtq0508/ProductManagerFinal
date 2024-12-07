using ProductManager.ViewModels;

namespace ProductManager.Views;

public partial class AddStorePage : ContentPage
{
	public AddStorePage(AddStoreViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;
	}
}