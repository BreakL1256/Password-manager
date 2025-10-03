using SQLite;
using Password_manager.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Password_manager.Entities
{
    public class RequestHandler
    {
        private readonly SqliteConnectionFactory _connectionFactory;
        public RequestHandler(SqliteConnectionFactory connectionFactory) 
        { 
            _connectionFactory = connectionFactory;
        }
        public async Task GetAccountSavedData()
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();


        }

        public async Task SaveDataToAccount(PasswordItem Item)
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();
            try
            {
                await database.ExecuteAsync("INSERT INTO Accounts (Title, Username, Password) VALUES (?, ?, ?)", Item.Title, Item.Username, Item.Password);

            } catch (Exception e)
            {
                Console.WriteLine("Failed to insert data into database" + e);
            }
        }
    }
}
