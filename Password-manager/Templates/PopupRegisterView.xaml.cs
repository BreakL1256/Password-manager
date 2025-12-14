using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics;
using Password_manager.Services;
using Password_manager.Entities;
using System.Threading.Tasks;

namespace Password_manager.Templates;

public partial class PopupRegisterView : Popup
{
    private readonly RestService _restService;
    private readonly RequestHandler _requestHandler;

    public IAsyncRelayCommand RegisterCommand { get; }
    public IRelayCommand NavigateBackToLoginCommand { get; }

    public PopupRegisterView(RestService restService, RequestHandler requestHandler)
    {
        InitializeComponent();

        CanBeDismissedByTappingOutsideOfPopup = true;

        _restService = restService;
        _requestHandler = requestHandler;

        RegisterCommand = new AsyncRelayCommand(
            execute: RegisterUser,
            canExecute: CanRegister);

        NavigateBackToLoginCommand = new RelayCommand(NavigateToCloudLogin);

        EmailEntry.TextChanged += (s, e) => RegisterCommand.NotifyCanExecuteChanged();
        PasswordEntry.TextChanged += (s, e) => RegisterCommand.NotifyCanExecuteChanged();
        ConfirmPasswordEntry.TextChanged += (s, e) => RegisterCommand.NotifyCanExecuteChanged();

        BindingContext = this;
    }

    public async Task RegisterUser()
    {
        PasswordBorder.Stroke = Colors.Transparent;
        ConfirmPasswordBorder.Stroke = Colors.Transparent;

        if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            ConfirmPasswordBorder.Stroke = Colors.Red;
            return;
        }

        LoginDTO credentials = new LoginDTO()
        {
            Email = EmailEntry.Text,
            Password = PasswordEntry.Text,
        };

        bool isCloudSuccess = await _restService.RegisterNewCloudAccount(credentials);

        if (isCloudSuccess)
        {
            try
            {
                await _requestHandler.RegisterNewUserAccount(credentials.Email, credentials.Password);
                Close("register_success");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Local Registration Failed: {ex.Message}");
                PasswordBorder.Stroke = Colors.Red;
            }
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
        return !string.IsNullOrWhiteSpace(EmailEntry.Text)
            && !string.IsNullOrWhiteSpace(PasswordEntry.Text)
            && !string.IsNullOrWhiteSpace(ConfirmPasswordEntry.Text);
    }
}