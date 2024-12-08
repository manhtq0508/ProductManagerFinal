using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProductManager.Entities;
using ProductManager.Interfaces;
using ProductManager.Messages;

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

    // Call when currentStoreId is changed
    partial void OnStoreIdChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            LoadStoreAsync(value);
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

            WeakReferenceMessenger.Default.Send(new StoreEditedMessage());

            await Shell.Current.GoToAsync("..");
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
