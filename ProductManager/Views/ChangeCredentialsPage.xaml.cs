using ProductManager.ViewModels;

namespace ProductManager.Views;

public partial class ChangeCredentialsPage : ContentPage
{
	public ChangeCredentialsPage(ChangeCredentialsViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;
	}
}