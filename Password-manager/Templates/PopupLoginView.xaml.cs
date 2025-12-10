using CommunityToolkit.Maui.Views;
using Password_manager.Services;
using Password_manager.Entities;
using CommunityToolkit.Mvvm.Input;

namespace Password_manager.Templates;

public partial class PopupLoginView : Popup
{
    private readonly RestService _restService;
	public IAsyncRelayCommand LoginCommand { get; }
	public IRelayCommand NavigationToRegisterPageCommand { get; }
    public PopupLoginView(RestService restService)
	{
		InitializeComponent();

        CanBeDismissedByTappingOutsideOfPopup = true;

        _restService = restService;

		LoginCommand = new AsyncRelayCommand(
			execute: LoginToCloud,
			canExecute: CanLogin);

		NavigationToRegisterPageCommand = new RelayCommand(NavigateToCloudRegister);

        EmailEntry.TextChanged += (s, e) => LoginCommand.NotifyCanExecuteChanged();
        PasswordEntry.TextChanged += (s, e) => LoginCommand.NotifyCanExecuteChanged();

        BindingContext = this;
	}

	public async Task LoginToCloud()
	{
        LoginDTO credentials = new LoginDTO()
        {
            Email = EmailEntry.Text,
            Password = PasswordEntry.Text,
        };

		bool isSuccesful = await _restService.LoginToCloudAccount(credentials);

		if (isSuccesful)
		{
			Close("login_success");
		}
		else
		{
            PasswordBorder.Stroke = Colors.Red;
        }
	}

	public void NavigateToCloudRegister()
	{
		Close("navigate_to_register");
	}


    public bool CanLogin()
	{
		return !String.IsNullOrWhiteSpace(EmailEntry.Text)
			&& !String.IsNullOrWhiteSpace(PasswordEntry.Text);
    }
}