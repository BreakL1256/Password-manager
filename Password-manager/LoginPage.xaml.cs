using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Password_manager.Entities;
using Microsoft.Maui.Storage;

namespace Password_manager;

public partial class LoginPage : ContentPage
{	
	private readonly RequestHandler _handler;
	public IAsyncRelayCommand LoginCommand { get; }
	public IAsyncRelayCommand NavigationToRegisterPageCommand {  get; }
	public LoginPage(RequestHandler handler)
	{
		InitializeComponent();
		

		LoginCommand = new AsyncRelayCommand(
			execute: CheckLoginCredentials,
			canExecute: CanLogin
		);

        NavigationToRegisterPageCommand = new AsyncRelayCommand(NavigateToRegisterPage);

        UsernameEntry.TextChanged += (s, e) => LoginCommand.NotifyCanExecuteChanged();
        PasswordEntry.TextChanged += (s, e) => LoginCommand.NotifyCanExecuteChanged();

		_handler = handler;

		BindingContext = this;
    }

	private async Task NavigateToRegisterPage()
	{
		try
		{
			await Shell.Current.GoToAsync("//RegisterPage");
		}
		catch (Exception ex)
		{
			Debug.WriteLine("Failed to navigate to register page" + ex);
		}

	}

	private async Task CheckLoginCredentials()
	{
		string username = UsernameEntry.Text;
		string password = PasswordEntry.Text;

        bool accountExists = await _handler.CheckUserAccount(username, password);

        if (accountExists)
		{
			try
			{
				int UserId = await _handler.GetUserAccountId(username);
				Preferences.Set("CurrentUserId", UserId);
				Preferences.Set("CurrentUsername", username);
				Preferences.Set("IsLoggedIn", true);
				await SecureStorage.Default.SetAsync("CurrentPassword", password);

                await Shell.Current.GoToAsync("//MainPage");
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to navigate to main page: " + ex);
			}
		}
		else
		{
			UsernameBorder.Stroke = Colors.Red;
            PasswordBorder.Stroke = Colors.Red;
        }
	}

	private bool CanLogin()
	{
		return !String.IsNullOrWhiteSpace(UsernameEntry.Text) && !String.IsNullOrWhiteSpace(PasswordEntry.Text);
    }
}