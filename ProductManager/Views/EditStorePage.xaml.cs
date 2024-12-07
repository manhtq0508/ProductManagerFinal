using ProductManager.ViewModels;

namespace ProductManager.Views;

public partial class EditStorePage : ContentPage
{
	public EditStorePage(EditStoreViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;
	}
}