using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProductManager.Views;

namespace ProductManager.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private string USERNAME = "admin";
    private string PASSWORD = "admin";

    [ObservableProperty]
    private bool showLoggingInScreen = false;

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

        if (Preferences.ContainsKey("username"))
        {
            USERNAME = Preferences.Get("username", "admin");
        }

        if (Preferences.ContainsKey("password"))
        {
            PASSWORD = Preferences.Get("password", "admin");
        }

        try
        {
            if (Username == USERNAME && Password == PASSWORD)
            {
                ShowLoggingInScreen = true;

                // IMPORTANT: This line is used prevent UI from blocking when navigating to MainPage
                // Remove it will make LoggingInScreen not showing
                await Task.Delay(100);

                await Shell.Current.GoToAsync($"//{nameof(MainPage)}");

                ShowLoggingInScreen = false;
                Password = "";
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

    [RelayCommand]
    private async Task ForgotCredentials()
    {
        await Shell.Current.GoToAsync(nameof(ChangeCredentialsPage));
    }
}
