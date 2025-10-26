using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Password_manager.Entities
{
    class AdditionalClasses
    {
    }
    public class PasswordItem
    {
        public PasswordItem(string Title, string Username, string Password, string Category)
        {
            this.Title = Title ?? "";
            this.Username = Username;
            this.Password = Password;
            this.Category = Category ?? "General";
        }
        public string Title { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Category { get; set; }
    }
}
