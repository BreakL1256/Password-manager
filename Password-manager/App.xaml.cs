//using Java.Lang;
using System.Diagnostics;
using Password_manager.Entities;
using Password_manager.Shared;
using SQLite;
using Microsoft.Maui.Storage;

namespace Password_manager
{
    public partial class App : Application
    {
        public App()
        {
            LoadTheme();
            InitializeComponent();
            

            MainPage = new AppShell();
        }

        private void LoadTheme()
        {
            var savedTheme = Preferences.Get("app_theme", "System Default");

            switch (savedTheme)
            {
                case "Light Mode":
                    Current.UserAppTheme = AppTheme.Light;
                    break;
                case "Dark Mode":
                    Current.UserAppTheme = AppTheme.Dark;
                    break;
                case "System Default":
                default:
                    Current.UserAppTheme = AppTheme.Unspecified;
                    break;
            }
        }
        private readonly SqliteConnectionFactory _connectionFactory;
        public App(SqliteConnectionFactory connectionFactory)
        {
            InitializeComponent();

            _connectionFactory = connectionFactory;

            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            await InitDatabaseAsync();
            await CheckLoginStatusAsync();
        }

        protected async Task InitDatabaseAsync()
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();
            try
            {
                await database.CreateTableAsync<ProgramDto>();
                await database.CreateTableAsync<UserAccounts>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database table creation fault: " + ex.ToString());
            }

        }

        private async Task CheckLoginStatusAsync()
        {
            await Task.Delay(100);

            bool isLoggedIn = Preferences.Get("IsLoggedIn", false);
            try
            {
                if (isLoggedIn)
                {
                    await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    await Shell.Current.GoToAsync("//LoginPage");
                }
            } catch (Exception ex) 
            {
                Debug.WriteLine("Failed to navigate to either login page or main page" + ex);
            }

        }

        //protected override Window CreateWindow(IActivationState? activationState)
        //{
        //    return new Window(new AppShell());
        //}
    }
}