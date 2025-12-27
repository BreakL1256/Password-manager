using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using Password_manager.Services;

namespace Password_manager.ViewModels
{
    public partial class LoginPageViewModel: ObservableObject
    {
        private readonly RequestHandler _handler;
        private readonly ILogger<LoginPageViewModel> _logger;

        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private Color usernameBorderColor = Colors.Gray;

        [ObservableProperty]
        private Color passwordBorderColor = Colors.Gray;

        [ObservableProperty]
        private bool isLoading = false;

        public IAsyncRelayCommand LoginCommand { get; }
        public IAsyncRelayCommand NavigationToRegisterPageCommand { get; }
        public Func<string, Task>? NavigateToPage { get; set; }
        public LoginPageViewModel(RequestHandler handler, ILogger<LoginPageViewModel> logger)
        {
            LoginCommand = new AsyncRelayCommand(
                execute: CheckLoginCredentials,
                canExecute: CanLogin
            );

            NavigationToRegisterPageCommand = new AsyncRelayCommand(NavigateToRegisterPage);

            _handler = handler;
            _logger = logger;
        }

        private async Task NavigateToRegisterPage()
        {
            if(NavigateToPage != null)
            {
                await NavigateToPage.Invoke("//RegisterPage");
            }
        }

        private async Task CheckLoginCredentials()
        {
            IsLoading = true;
            ResetBorderColors();

            string username = Username;
            string password = Password;


            bool accountExists = await _handler.CheckUserAccount(username, password);

            try
            {
                if (accountExists)
                {
                    long UserId = await _handler.GetUserAccountId(username);
                    Preferences.Set("CurrentUserId", UserId);
                    Preferences.Set("CurrentUsername", username);
                    Preferences.Set("IsLoggedIn", true);
                    await SecureStorage.Default.SetAsync("CurrentPassword", password);
                    
                    ResetTextFields();

                    if(NavigateToPage != null)
                    {
                        await NavigateToPage.Invoke("//MainPage");
                    }
                }
                else
                {
                    UsernameBorderColor = Colors.Red;
                    PasswordBorderColor = Colors.Red;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to navigate to main page: {ex}", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ResetBorderColors()
        {
            UsernameBorderColor = Colors.Gray;
            PasswordBorderColor = Colors.Gray;
        }

        private void ResetTextFields()
        {
            Username = string.Empty;
            Password = string.Empty;
        }

        partial void OnUsernameChanged(string value)
        {
            LoginCommand.NotifyCanExecuteChanged();
        }

        partial void OnPasswordChanged(string value)
        {
            LoginCommand.NotifyCanExecuteChanged();
        }

        partial void OnIsLoadingChanged(bool value)
        {
            LoginCommand.NotifyCanExecuteChanged();
        }
        private bool CanLogin()
        {
            return !String.IsNullOrWhiteSpace(Username) && !String.IsNullOrWhiteSpace(Password) && !IsLoading;
        }
    }
}
