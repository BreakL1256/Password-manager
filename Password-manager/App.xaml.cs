//using Java.Lang;
using Password_manager.Entities;
using Password_manager.Shared;
using SQLite;

namespace Password_manager
{
    public partial class App : Application
    {
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

            bool isLoggedIn = false;

            if (isLoggedIn)
            {
                await Shell.Current.GoToAsync("//MainPage");
            } else
            {
                await Shell.Current.GoToAsync("//LoginPage");
            }
        }

        //protected override Window CreateWindow(IActivationState? activationState)
        //{
        //    return new Window(new AppShell());
        //}
    }
}