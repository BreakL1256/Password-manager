using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Password_manager.Entities;

namespace Password_manager;

public partial class LoginPage : ContentPage
{	
	private readonly RequestHandler _handler;
	private IAsyncRelayCommand LoginCommand { get; }
	public LoginPage(RequestHandler handler)
	{
		InitializeComponent();
		
		BindingContext = this;

		LoginCommand = new AsyncRelayCommand(
			execute: CheckLoginCredentials,
			canExecute: CanLogin
		);

        UsernameEntry.TextChanged += (s, e) => LoginCommand.NotifyCanExecuteChanged();
        PasswordEntry.TextChanged += (s, e) => LoginCommand.NotifyCanExecuteChanged();

		_handler = handler;
    }

	private async Task CheckLoginCredentials()
	{
		try
		{
			bool LoggedIn = await _handler.CheckUserAccount(UsernameEntry.Text, PasswordEntry.Text);

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
			Debug.WriteLine("Failed to execute user check" + ex);
		}
	}

	private bool CanLogin()
	{
		return !String.IsNullOrWhiteSpace(UsernameEntry.Text) && !String.IsNullOrWhiteSpace(PasswordEntry.Text);

    }
}