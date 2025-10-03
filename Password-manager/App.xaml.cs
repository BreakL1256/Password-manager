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

            InitDatabaseAsync();
        }

        protected async void InitDatabaseAsync()
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();
            try
            {
                await database.CreateTableAsync<ProgramDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database table creation fault: " + ex.ToString());
            }

        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}