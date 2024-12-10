using ProductManager.ViewModels;

namespace ProductManager.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage(LoginViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;
	}
}