using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProductManager.Entities;
using ProductManager.Interfaces;

namespace ProductManager.ViewModels;

[QueryProperty(nameof(StoreId), "storeId")]
public partial class EditStoreViewModel : ObservableObject
{
    private IStoreRepo _storeRepo;

    [ObservableProperty]
    private Store? storeNeedEdited;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StoreNeedEdited))]
    private string? storeId;

    public EditStoreViewModel(IStoreRepo storeRepo)
    {
        _storeRepo = storeRepo;
    }

    // Call when storeId is changed
    partial void OnStoreIdChanged(string? oldValue, string? newValue)
    {
        if (!string.IsNullOrEmpty(newValue))
        {
            LoadStoreAsync(newValue);
        }
    }

    private async void LoadStoreAsync(string id)
    {
        try
        {
            StoreNeedEdited = await _storeRepo.GetStoreByIdAsync(id);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task SaveStoreAsync()
    {
        if (StoreNeedEdited == null)
            return;

        try
        {
            await _storeRepo.UpdateStoreAsync(StoreNeedEdited);
            await Shell.Current.GoToAsync("..");

            StoreNeedEdited = null;
            StoreId = null;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await Shell.Current.GoToAsync("..");
    }
}
