using ProductManager.ViewModels;

namespace ProductManager.Views;

public partial class SelectProductInBillPage : ContentPage
{
	public SelectProductInBillPage(SelectProductInBillViewModel vm)
	{
		InitializeComponent();

        BindingContext = vm;
    }
}