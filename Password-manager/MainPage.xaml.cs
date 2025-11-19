using Password_manager.Entities;
using Password_manager.Templates;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Password_manager.Services;
using CommunityToolkit.Maui.Views;
using System.Threading.Tasks;


namespace Password_manager
{

    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        public IAsyncRelayCommand Cloud { get; }
        public IAsyncRelayCommand LogoutFromAccount { get; }
        public IAsyncRelayCommand SettingsCommand { get; }
        public IAsyncRelayCommand DisconnectCommand { get; }
        public IAsyncRelayCommand BackupCommand { get; }
        public IAsyncRelayCommand RestoreCommand { get; }
        private readonly IServiceProvider _services;
        private readonly RestServiceHelper _restServiceHelper;
        private readonly RestService _restService;
        public IRelayCommand<TabItem> SwitchTabCommand { get; }
        private readonly RequestHandler _handler;
        public ObservableCollection<TabItem> Tabs { get; set; }

        private TabItem _selectedTab;
        public TabItem SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (_selectedTab != value)
                {
                    _selectedTab = value;
                    OnPropertyChanged();

                    if (value != null)
                    {
                        ShowTab(value);
                    }
                }
            }
        }
        private bool _isConnectedToCloud = false;
        public bool IsConnectedToCloud { 
            get => _isConnectedToCloud;
            set 
            {
                _isConnectedToCloud = value;
                OnPropertyChanged();
            }
        }

        public MainPage(IServiceProvider services, RequestHandler handler, RestServiceHelper restServiceHelper)
        {
            InitializeComponent();

            _services = services;
            _handler = handler;
            _restServiceHelper = restServiceHelper;

            Cloud = new AsyncRelayCommand(ConnectToCloud);
            LogoutFromAccount = new AsyncRelayCommand(Logout);
            SettingsCommand = new AsyncRelayCommand(OpenSettings);
            DisconnectCommand = new AsyncRelayCommand(DisconnectFromCloud);
            BackupCommand = new AsyncRelayCommand(BackupVault);
            RestoreCommand = new AsyncRelayCommand(RestoreVault);
            SwitchTabCommand = new RelayCommand<TabItem>(tab => SelectedTab = tab);

            Tabs = new ObservableCollection<TabItem>
            {
                new TabItem { Title = "Passwords", ViewType = typeof(PasswordVaultView) },
                new TabItem { Title = "Notes", ViewType = typeof(NoteVaultView) }
            };

            BindingContext = this;

            SelectedTab = Tabs[0];
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CheckCloudConnection();
        }

        public async Task DisconnectFromCloud()
        {
            await _restServiceHelper.DeleteCloudData();

            IsConnectedToCloud = false;
        }
        public async Task BackupVault()
        {
            var isBackedUp = await _restService.BackupVault();
        }
        public async Task RestoreVault()
        {
            var isRestored = await _restService.RestoreVault();

            if (isRestored)
            {
                if(TabView.Content is PasswordVaultView passwordVaultView)
                {
                    await passwordVaultView.LoadData();
                }
            }
        }
        private async Task CheckCloudConnection()
        {
            bool cloudLinked = await _restServiceHelper.IsCloudLinked();

            if (cloudLinked)
            {
                IsConnectedToCloud = true;
            }
            else
            {
                IsConnectedToCloud = false;
            }
        }

        private async Task ConnectToCloud()
        {
            await ShowLoginView();
        }

        private async Task ShowLoginView()
        {
            var loginPopup = _services.GetService<PopupLoginView>();
            var result = await this.ShowPopupAsync(loginPopup);

            if (result?.ToString() == "navigate_to_register")
            {
                await ShowRegisterView();
            }
            else if (result?.ToString() == "login_success")
            {
                IsConnectedToCloud = true;
            }
        }

        private async Task ShowRegisterView()
        {
            var registerPopup = _services.GetService<PopupLoginView>();
            var result = await this.ShowPopupAsync(registerPopup);

            if (result?.ToString() == "navigate_to_login" || result?.ToString() == "register_success")
            {
                await ShowLoginView();
            }
        }

        private async Task Logout()
        {
            Preferences.Remove("CurrentUserId");
            Preferences.Remove("CurrentUsername");
            Preferences.Remove("IsLoggedIn");
            Preferences.Remove("IsFirstBackup");
            SecureStorage.Default.Remove("CurrentPassword");
            try
            {
                await Shell.Current.GoToAsync("//LoginPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to logout: " + ex);
            }
        }

        private async Task OpenSettings()
        {
            var settingsPage = new SettingsPage();
            await Navigation.PushModalAsync(settingsPage);
        }

        private void ShowTab(TabItem tab)
        {
            var view = _services.GetService(tab.ViewType) as View;
            TabView.Content = view;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class TabItem
    {
        public string Title { get; set; }
        public Type ViewType { get; set; }
    }
}