using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Password_manager.Entities;

namespace Password_manager;

public partial class LoginPage : ContentPage
{	
	private readonly RequestHandler _handler;
	private IAsyncRelayCommand LoginCommand { get; }
	private IAsyncRelayCommand NavigationToRegisterPageCommand {  get; }
	public LoginPage(RequestHandler handler)
	{
		InitializeComponent();
		
		BindingContext = this;

		LoginCommand = new AsyncRelayCommand(
			execute: CheckLoginCredentials,
			canExecute: CanLogin
		);

        NavigationToRegisterPageCommand = new AsyncRelayCommand(NavigateToRegisterPage);

        UsernameEntry.TextChanged += (s, e) => LoginCommand.NotifyCanExecuteChanged();
        PasswordEntry.TextChanged += (s, e) => LoginCommand.NotifyCanExecuteChanged();

		_handler = handler;
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
		bool LoggedIn = await _handler.CheckUserAccount(UsernameEntry.Text, PasswordEntry.Text);
		try
		{

			if (LoggedIn)
			{
				await Shell.Current.GoToAsync("//MainPage");
			}
			else
			{
				UsernameBorder.Stroke = Colors.Red;
                PasswordBorder.Stroke = Colors.Red;
            }
		}
		catch (Exception ex)
		{
			Debug.WriteLine("Failed to navigate to main page" + ex);
		}
	}

	private bool CanLogin()
	{
		return !String.IsNullOrWhiteSpace(UsernameEntry.Text) && !String.IsNullOrWhiteSpace(PasswordEntry.Text);
    }
}