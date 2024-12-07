using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProductManager.Entities;
using ProductManager.Interfaces;
using ProductManager.Messages;
using ProductManager.Views;
using System.Collections.ObjectModel;

namespace ProductManager.ViewModels;

public partial class StoreViewModel : ObservableObject
{
    private IStoreRepo _storeRepo;

    [ObservableProperty]
    private ObservableCollection<Store>? stores;

    [ObservableProperty]
    private Store? selectedStore;

    public StoreViewModel(IStoreRepo storeRepo)
    {
        _storeRepo = storeRepo;
        LoadStoresAsync().Wait();

        // Register to receive messages
        WeakReferenceMessenger.Default.Register<StoreAddedMessage>(this, (r, m) => OnStoreAdded(m.newStore));
    }

    private async Task LoadStoresAsync()
    {
        if (Stores == null)
            Stores = new ObservableCollection<Store>();
        else
            Stores.Clear();

        foreach (var s in await _storeRepo.GetStoresAsync())
        {
            Stores.Add(s);
        }
    }

    private void OnStoreAdded(Store newStore)
    {
        if (Stores == null)
            Stores = new ObservableCollection<Store>();

        Stores.Add(newStore);
    }

    [RelayCommand]
    private async Task AddStore()
    {
        await Shell.Current.GoToAsync(nameof(AddStorePage));
        await LoadStoresAsync();
    }

    [RelayCommand]
    private async Task EditStore()
    {
        if (SelectedStore == null)
        {
            await Shell.Current.DisplayAlert("Error", "Please select a store to edit", "OK");
            return;
        }

        await Shell.Current.GoToAsync($"{nameof(EditStorePage)}?storeId={SelectedStore.Id}");
    }

    [RelayCommand]
    private async Task DeleteStore()
    {
    }

    [RelayCommand]
    private async Task ViewBills()
    {

    }
}
