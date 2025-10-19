using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using Password_manager.Entities;

namespace Password_manager;

public partial class RegisterPage : ContentPage
{
	private readonly RequestHandler _handler;
	public IAsyncRelayCommand NavigateBackToLoginCommand {  get; }
    public IAsyncRelayCommand RegisterCommand { get; }
    public RegisterPage(RequestHandler handler)
	{
		InitializeComponent();

		_handler = handler;

		NavigateBackToLoginCommand = new AsyncRelayCommand(NavigateToLogin);
        RegisterCommand = new AsyncRelayCommand(
			execute: RegisterNewUser,
			canExecute: CanRegister);

		UsernameEntry.TextChanged += (s, e) => RegisterCommand.NotifyCanExecuteChanged();
        PasswordEntry.TextChanged += (s, e) => RegisterCommand.NotifyCanExecuteChanged();
        ConfirmPasswordEntry.TextChanged += (s, e) => RegisterCommand.NotifyCanExecuteChanged();

		BindingContext = this;
    }

	private async Task NavigateToLogin()
	{
		try
		{
			await Shell.Current.GoToAsync("//LoginPage");
		}catch (Exception ex)
		{
			Debug.WriteLine("Failed to navigate to login page: " + ex);
		}
	}

	private bool CanRegister()
	{
		return !String.IsNullOrWhiteSpace(UsernameEntry.Text) 
			&& !String.IsNullOrWhiteSpace(PasswordEntry.Text) 
			&& !String.IsNullOrWhiteSpace(ConfirmPasswordEntry.Text);
	}

	private async Task RegisterNewUser()
	{
		string username = UsernameEntry.Text;
		string password = PasswordEntry.Text;
		string confirmPassword = ConfirmPasswordEntry.Text;

		bool DoesAccountAlreadyExist = await _handler.CheckUserAccount(username, password);

        if (password == confirmPassword && !DoesAccountAlreadyExist)
		{
			await _handler.RegisterNewUserAccount(username, password);
			try
			{
				await Shell.Current.GoToAsync("//LoginPage");
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to navigate to Login page: " + ex);
			}
		}
		else
		{
			PasswordBorder.Stroke = Colors.Red;
            ConfirmPasswordBorder.Stroke = Colors.Red;
        }
	}
}