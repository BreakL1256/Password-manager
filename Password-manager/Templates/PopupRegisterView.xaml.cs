using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Password_manager.Services;
using Password_manager.Entities;

namespace Password_manager.Templates;

public partial class PopupRegisterView : Popup
{
	private readonly RestService _restService;
	public IAsyncRelayCommand RegisterCommand { get; }
	public IRelayCommand NavigateBackToLoginCommand { get; }

    public PopupRegisterView(RestService restService)
	{
		InitializeComponent();

        CanBeDismissedByTappingOutsideOfPopup = true;

        _restService = restService;

		RegisterCommand = new AsyncRelayCommand(
			execute: RegisterToCloud,
			canExecute: CanRegister);
		NavigateBackToLoginCommand = new RelayCommand(NavigateToCloudLogin);

        EmailEntry.TextChanged += (s, e) => RegisterCommand.NotifyCanExecuteChanged();
        PasswordEntry.TextChanged += (s, e) => RegisterCommand.NotifyCanExecuteChanged();
        ConfirmPasswordEntry.TextChanged += (s, e) => RegisterCommand.NotifyCanExecuteChanged();

        BindingContext = this;
	}

	public async Task RegisterToCloud()
	{
		LoginDTO credentials = new LoginDTO()
		{
			Email = EmailEntry.Text,
			Password = PasswordEntry.Text,
		};

		bool isSuccesful = await _restService.RegisterNewCloudAccount(credentials);

		if (isSuccesful)
		{
			Close("register_success");
		}
		else
		{
            PasswordBorder.Stroke = Colors.Red;
            ConfirmPasswordBorder.Stroke = Colors.Red;
        }

	}

    public void NavigateToCloudLogin()
    {
		Close("navigate_to_login");
    }

	private bool CanRegister()
	{
        return !String.IsNullOrWhiteSpace(EmailEntry.Text)
			&& !String.IsNullOrWhiteSpace(PasswordEntry.Text)
			&& !String.IsNullOrWhiteSpace(ConfirmPasswordEntry.Text);
    }
}