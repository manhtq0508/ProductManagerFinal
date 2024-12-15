using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ProductManager.ViewModels;

public partial class ChangeCredentialsViewModel : ObservableObject
{
    [ObservableProperty]
    private string privateKey = "";

    [ObservableProperty]
    private string newUsername = "";

    [ObservableProperty]
    private string newPassword = "";

    [ObservableProperty]
    private string reEnterPassword = "";

    [RelayCommand]
    private async Task ChangePassword()
    {
        if (PrivateKey != "VK7JG-NPHTM-C97JM-9MPGT-3V66T")
        {
            await Shell.Current.DisplayAlert("Error", "Private key is incorrect", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(NewPassword))
        {
            await Shell.Current.DisplayAlert("Error", "Missing information", "OK");
            return;
        }

        if (NewPassword != ReEnterPassword)
        {
            await Shell.Current.DisplayAlert("Error", "Passwords do not match", "OK");
            return;
        }

        Preferences.Set("username", NewUsername);
        Preferences.Set("password", NewPassword);
        await Shell.Current.DisplayAlert("Success", "Credentials changed successfully", "OK");
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await Shell.Current.GoToAsync("..");
    }
}
