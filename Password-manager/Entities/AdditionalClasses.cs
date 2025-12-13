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

        public long Id { get; set; }
        public string Title { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Category { get; set; }

        public bool IsDeleted { get; set; }

        public string DeleteActionText => IsDeleted ? "Delete Permanently" : "Delete";
    }

    public class NoteItem
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public string? ContentPreview => Content?.Length > 80 ? Content.Substring(0, 80) + "..." : Content;

        public bool IsDeleted { get; set; }
        public string DeleteActionText => IsDeleted ? "Delete Permanently" : "Delete";
    }
}