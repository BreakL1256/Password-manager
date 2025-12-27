using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Microsoft.Extensions.Logging;
using Password_manager.Entities;
using Password_manager.Services;
using Password_manager.Templates;

namespace Password_manager.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        public IAsyncRelayCommand Cloud { get; }
        public IAsyncRelayCommand LogoutFromAccount { get; }
        public IAsyncRelayCommand SettingsCommand { get; }
        public IAsyncRelayCommand DisconnectCommand { get; }
        public IAsyncRelayCommand BackupCommand { get; }
        public IAsyncRelayCommand RestoreCommand { get; }
        public IRelayCommand<TabItem> SwitchTabCommand { get; }

        private readonly IServiceProvider _services;
        private readonly RestServiceHelper _restServiceHelper;
        private readonly RestService _restService;
        private readonly ILogger<MainPage> _logger;
        private readonly RequestHandler _handler;

        [ObservableProperty]
        private ObservableCollection<TabItem> tabs;

        [ObservableProperty]
        private TabItem selectedTab;

        [ObservableProperty]
        private bool isConnectedToCloud;

        // UI callbacks
        public Action<Page>? OpenSettingsPage { get; set; }
        public Func<Task<object?>>? ShowLoginPopup { get; set; }
        public Func<PopupRegisterView, Task<object?>>? ShowRegisterPopup { get; set; }
        public Action<View>? SetTabContent { get; set; }
        public Func<string, Task> NavigateToPage { get; set; }

        public MainPageViewModel(IServiceProvider services, RequestHandler handler,
            RestServiceHelper restServiceHelper, ILogger<MainPage> logger, RestService restService)
        {
            _services = services;
            _handler = handler;
            _restServiceHelper = restServiceHelper;
            _logger = logger;
            _restService = restService;

            Tabs = new ObservableCollection<TabItem>
            {
                new TabItem { Title = "Passwords", ViewType = typeof(PasswordVaultView) },
                new TabItem { Title = "Notes", ViewType = typeof(NoteVaultView) }
            };

            SelectedTab = Tabs[0];
            Cloud = new AsyncRelayCommand(ConnectToCloud);
            LogoutFromAccount = new AsyncRelayCommand(Logout);
            SettingsCommand = new AsyncRelayCommand(OpenSettings);
            DisconnectCommand = new AsyncRelayCommand(DisconnectFromCloud);
            BackupCommand = new AsyncRelayCommand(BackupVault);
            RestoreCommand = new AsyncRelayCommand(RestoreVault);
            SwitchTabCommand = new RelayCommand<TabItem>(tab =>
            {
                SelectedTab = tab;
                ShowTab(tab);
            });
        }


        public async Task DisconnectFromCloud()
        {
            await _restServiceHelper.DeleteCloudData();
            IsConnectedToCloud = false;
        }

        public async Task BackupVault()
        {
            var credentials = await _restServiceHelper.GetCloudCredentials();
            if (credentials != null)
            {
                await _restService.LoginToCloudAccount(credentials);
                await _restService.BackupVault();
            }
        }

        public async Task RestoreVault()
        {
            var credentials = await _restServiceHelper.GetCloudCredentials();
            if (credentials != null)
            {
                await _restService.LoginToCloudAccount(credentials);
                bool restored = await _restService.RestoreVault();
                if (restored && SelectedTab?.ViewType == typeof(PasswordVaultView))
                {
                    var view = _services.GetService(typeof(PasswordVaultView)) as PasswordVaultView;
                    if (view != null)
                        await view.LoadData();
                }
            }
        }

        public async Task CheckCloudConnection()
        {
            IsConnectedToCloud = await _restServiceHelper.IsCloudLinked();
        }

        private async Task ConnectToCloud()
        {
            var loginPopup = _services.GetService<PopupLoginView>();
            var result = await ShowLoginPopup?.Invoke();
            if (result?.ToString() == "navigate_to_register")
            {
                var registerPopup = _services.GetService<PopupRegisterView>();
                await ShowRegisterPopup?.Invoke(registerPopup);
            }
            else if (result?.ToString() == "login_success")
            {
                IsConnectedToCloud = true;
            }
        }

        private async Task OpenSettings()
        {
            var page = new SettingsPage();
            OpenSettingsPage?.Invoke(page);
        }

        private async Task Logout()
        {
            Preferences.Remove("CurrentUserId");
            Preferences.Remove("CurrentUsername");
            Preferences.Remove("IsLoggedIn");
            Preferences.Remove("IsFirstBackup");
            SecureStorage.Default.Remove("CurrentPassword");

            if (NavigateToPage != null)
            {
                await NavigateToPage.Invoke("//LoginPage");
            }

        }

        private void ShowTab(TabItem tab)
        {
            var view = _services.GetService(tab.ViewType) as View;
            if (view != null)
                SetTabContent?.Invoke(view);
        }
    }

    public class TabItem
    {
        public string Title { get; set; }
        public Type ViewType { get; set; }
    }
}
