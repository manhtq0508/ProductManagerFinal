using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ProductManager.Messages;

namespace ProductManager.CombineData;

public partial class ProductInBill : ObservableObject
{
    [ObservableProperty]
    private string id = "";

    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private int price;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Total))]
    private int quantity;

    [ObservableProperty]
    private long total;

    partial void OnQuantityChanged(int value)
    {
        Total = Price * Quantity;

        WeakReferenceMessenger.Default.Send<QuantityChangedMessage>();
    }
}
