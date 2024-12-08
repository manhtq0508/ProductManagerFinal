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
    private IBillDetailRepo _billDetailRepo; // To calculate revenue

    [ObservableProperty]
    private ObservableCollection<Store>? stores;

    [ObservableProperty]
    private Store? selectedStore;

    [ObservableProperty]
    private long? revenue;

    public StoreViewModel(IStoreRepo storeRepo, IBillDetailRepo billDetailRepo)
    {
        _storeRepo = storeRepo;
        _billDetailRepo = billDetailRepo;

        LoadStoresAsync().Wait();
        CalculateRevenue().Wait();

        // Register to receive messages
        WeakReferenceMessenger.Default.Register<StoreAddedMessage>(this, (r, m) => OnStoreAdded(m.newStore));
        WeakReferenceMessenger.Default.Register<StoreEditedMessage>(this, async (r, m) => await OnStoreEdited(m.editedStore));
    }

    private async Task CalculateRevenue()
    {
        Revenue = await _billDetailRepo.GetTotalOfAllBillsAsync();
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

    private async Task OnStoreEdited(Store editedStore)
    {
        await LoadStoresAsync();
    }

    [RelayCommand]
    private async Task AddStore()
    {
        await Shell.Current.GoToAsync(nameof(AddStorePage));
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
        if (SelectedStore == null)
        {
            await Shell.Current.DisplayAlert("Error", "Please select a store to delete", "OK");
            return;
        }

        try
        {
            if (Stores == null)
                return;

            await _storeRepo.DeleteStoreAsync(SelectedStore);
            Stores.Remove(SelectedStore);
            await CalculateRevenue();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task ViewBills()
    {
        if (SelectedStore == null)
        {
            await Shell.Current.DisplayAlert("Error", "Please select the store", "OK");
            return;
        }

        await Shell.Current.GoToAsync($"{nameof(BillPage)}?storeId={SelectedStore?.Id}");
    }
}
