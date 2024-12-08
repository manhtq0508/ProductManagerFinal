using ProductManager.ViewModels;

namespace ProductManager.Views;

public partial class AddBillPage : ContentPage
{
	public AddBillPage(AddBillViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;
	}
}