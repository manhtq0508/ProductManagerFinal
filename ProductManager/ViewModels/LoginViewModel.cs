using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProductManager.Views;

namespace ProductManager.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private const string USERNAME = "admin";
    private const string PASSWORD = "admin";

    [ObservableProperty]
    private bool showLogingInScreen = false;

    [ObservableProperty]
    private string? username;

    [ObservableProperty]
    private string? password;

    [RelayCommand]
    private async Task Login()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            await Shell.Current.DisplayAlert("Error", "Username and password are required", "OK");
            return;
        }

        try
        {
            if (Username == USERNAME && Password == PASSWORD)
            {
                ShowLogingInScreen = true;

                // IMPORTANT: This line is used prevent UI from blocking when navigating to MainPage
                // Remove it will make LogingInScreen not showing
                await Task.Delay(100);

                await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Invalid username or password", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
