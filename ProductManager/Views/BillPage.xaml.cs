using ProductManager.ViewModels;

namespace ProductManager.Views;

public partial class BillPage : ContentPage
{
	public BillPage(BillViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;
	}
}