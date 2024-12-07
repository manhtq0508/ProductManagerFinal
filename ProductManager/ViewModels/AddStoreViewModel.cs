using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProductManager.Entities;
using ProductManager.Interfaces;
using ProductManager.Messages;

namespace ProductManager.ViewModels;

public partial class AddStoreViewModel : ObservableObject
{
    private IStoreRepo _storeRepo;

    [ObservableProperty]
    private string? storeId;

    [ObservableProperty]
    private string? storeName;

    [ObservableProperty]
    private string? storeAddress;

    public AddStoreViewModel(IStoreRepo storeRepo)
    {
        _storeRepo = storeRepo;
    }


    [RelayCommand]
    private async Task AddStore()
    {
        if (string.IsNullOrWhiteSpace(StoreId) || string.IsNullOrWhiteSpace(StoreName) || string.IsNullOrWhiteSpace(StoreAddress))
        {
            await Shell.Current.DisplayAlert("Error", "Missing information", "OK");
            return;
        }

        var newStore = new Store
        {
            Id = StoreId,
            Name = StoreName,
            Address = StoreAddress
        };

        try
        {
            await _storeRepo.AddStoreAsync(newStore);

            // Send a message to notify that a store has been added
            WeakReferenceMessenger.Default.Send(new StoreAddedMessage(newStore));

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception e)
        {
            await Shell.Current.DisplayAlert("Error", e.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await Shell.Current.GoToAsync("..");
    }
}
