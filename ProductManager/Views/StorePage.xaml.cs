using ProductManager.ViewModels;

namespace ProductManager.Views;

public partial class StorePage : ContentPage
{
	public StorePage(StoreViewModel vm)
	{
		InitializeComponent();

        BindingContext = vm;
    }
}