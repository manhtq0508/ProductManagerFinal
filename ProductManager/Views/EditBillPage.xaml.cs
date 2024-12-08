using ProductManager.ViewModels;

namespace ProductManager.Views;

public partial class EditBillPage : ContentPage
{
	public EditBillPage(EditBillViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;
	}
}