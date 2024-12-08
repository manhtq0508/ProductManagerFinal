using ProductManager.ViewModels;

namespace ProductManager.Views;

public partial class BillDetailPage : ContentPage
{
	public BillDetailPage(BillDetailViewModel vm)
	{
		InitializeComponent();

        BindingContext = vm;
    }
}