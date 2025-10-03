using SQLite;
using Password_manager.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Password_manager.Entities
{
    public class RequestHandler
    {
        private readonly SqliteConnectionFactory _connectionFactory;
        public RequestHandler(SqliteConnectionFactory connectionFactory) 
        { 
            _connectionFactory = connectionFactory;
        }
        public async Task<List<PasswordItem>> GetAccountSavedData()
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();
   
            try
            {
                var dtos = await database.QueryAsync<ProgramDto>("SELECT * FROM Accounts;");

                var result =  dtos.Select(dto => new PasswordItem(
                    dto.Title,
                    dto.Username,
                    dto.Password
                )).ToList();

                Debug.WriteLine($"inputs in the db {result.Count}");

                foreach (var item in result)
                {
                    Debug.WriteLine($"Title: {item.Title}, Username: {item.Username}");
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to fetch data: " + ex);
                return new List<PasswordItem>();
            }
        }

        public async Task SaveDataToAccount(PasswordItem Item)
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();
            try
            { 
                await database.ExecuteAsync("INSERT INTO Accounts (Title, Username, Password) VALUES (?, ?, ?);", Item.Title, Item.Username, Item.Password);

            } catch (Exception e)
            {
                Console.WriteLine("Failed to insert data into database" + e);
            }
        }
    }
}
