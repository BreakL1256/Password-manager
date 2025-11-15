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


namespace Password_manager
{

    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        public IAsyncRelayCommand Cloud { get; }
        public IAsyncRelayCommand LogoutFromAccount { get; }
        public IAsyncRelayCommand<PasswordItem> DeleteCommand { get; }
        public IAsyncRelayCommand SettingsCommand { get; }

        private readonly IServiceProvider _services;

        private readonly RequestHandler _handler;

        private PasswordItem? _selectedPassword;
        public ObservableCollection<PasswordItem> PasswordList { get; set; } = new ObservableCollection<PasswordItem>();
        public MainPage(IServiceProvider services, RequestHandler handler)
        {
            InitializeComponent();

            _services = services;
            _handler = handler;

            Cloud = new AsyncRelayCommand(StoreOnCloud);
            LogoutFromAccount = new AsyncRelayCommand(Logout);
            DeleteCommand = new AsyncRelayCommand<PasswordItem>(DeleteSelectedData);
            SettingsCommand = new AsyncRelayCommand(OpenSettings);
            BindingContext = this;
        }

        private async Task StoreOnCloud()
        {

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

        private async Task DeleteSelectedData(PasswordItem? Item)
        {
            if (Item == null)
            {
                return;
            }

            await _handler.DeleteDataFromAccount(Item);

            await LoadData();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadData();
        }

        private async Task LoadData()
        {
            var data = await _handler.GetAccountSavedData();

            PasswordList.Clear();

            foreach (var item in data)
            {
                PasswordList.Add(item);
            }

        }

        private void OnShowAddView(object sender, EventArgs e)
        {
            var view = _services.GetService<AddNewDataView>();
            view.OnDataAdded = async () => await LoadData();
            DynamicContentView.Content = view;
        }

        private void OnShowDataView(object sender, TappedEventArgs e)
        {
            if (e.Parameter is PasswordItem _selectedPassword)
            {
                DynamicContentView.Content = new ViewDataView(_selectedPassword);
            }
        }

        public PasswordItem? SelectedPassword
        {
            get => _selectedPassword;
            set
            {
                _selectedPassword = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}