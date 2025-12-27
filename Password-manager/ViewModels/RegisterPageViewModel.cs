using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using Password_manager.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace Password_manager.ViewModels
{
    public partial class RegisterPageViewModel: ObservableObject
    {
        private readonly RequestHandler _handler;
        private readonly ILogger<RegisterPageViewModel> _logger;
        public IAsyncRelayCommand NavigateBackToLoginCommand { get; }
        public IAsyncRelayCommand RegisterCommand { get; }


        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string confirmPassword = string.Empty;

        [ObservableProperty]
        private Color usernameBorderColor = Colors.Gray;

        [ObservableProperty]
        private Color passwordBorderColor = Colors.Gray;

        [ObservableProperty]
        private Color confirmPasswordBorderColor = Colors.Gray;

        [ObservableProperty]
        private bool isLoading = false;

        public Func<string, Task>? NavigateToPage { get; set; }
        public RegisterPageViewModel(RequestHandler handler, ILogger<RegisterPageViewModel> logger)
        {
            _handler = handler;
            _logger = logger;

            NavigateBackToLoginCommand = new AsyncRelayCommand(NavigateToLogin);
            RegisterCommand = new AsyncRelayCommand(
                execute: RegisterNewUser,
                canExecute: CanRegister);

        }

        private async Task NavigateToLogin()
        {  
            if(NavigateToPage != null)
            {
                await NavigateToPage.Invoke("//LoginPage");

            }
        }

        private bool CanRegister()
        {
            return !String.IsNullOrWhiteSpace(Username)
                && !String.IsNullOrWhiteSpace(Password)
                && !String.IsNullOrWhiteSpace(ConfirmPassword)
                && !IsLoading;
        }

        private async Task RegisterNewUser()
        {
            IsLoading = true;
            ResetBorderColor();


            string username = Username;
            string password = Password;
            string confirmPassword = ConfirmPassword;
            try
            {
                bool DoesAccountAlreadyExist = await _handler.CheckUserAccount(username, password);

                if (password == confirmPassword && !DoesAccountAlreadyExist)
                {
                    await _handler.RegisterNewUserAccount(username, password);

                    ResetTextFields();

                    if (NavigateToPage != null)
                    {
                        await NavigateToPage.Invoke("//LoginPage");
                    }
                }
                else
                {
                    UsernameBorderColor = Colors.Red;
                    PasswordBorderColor  = Colors.Red;
                    ConfirmPasswordBorderColor = Colors.Red;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to navigate to Login page: {ex}", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ResetBorderColor()
        {
            UsernameBorderColor = Colors.Gray;
            PasswordBorderColor = Colors.Gray;
            ConfirmPasswordBorderColor = Colors.Gray;
        }

        private void ResetTextFields()
        {
            Username = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
        }

        partial void OnUsernameChanged(string value)
        {
            RegisterCommand.NotifyCanExecuteChanged();
        }

        partial void OnPasswordChanged(string value)
        {
            RegisterCommand.NotifyCanExecuteChanged();
        }

        partial void OnConfirmPasswordChanged(string value)
        {
            RegisterCommand.NotifyCanExecuteChanged();
        }

        partial void OnIsLoadingChanged(bool value)
        {
            RegisterCommand.NotifyCanExecuteChanged();
        }
    }
}
