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
        public DateTime? DeletedAt { get; set; }

        public string DeleteActionText => IsDeleted ? "Delete Permanently" : "Delete";
        public string RemainingTime
        {
            get
            {
                if (!IsDeleted || DeletedAt == null) return string.Empty;
                var remaining = DeletedAt.Value.AddDays(5) - DateTime.UtcNow;

                if (remaining.TotalDays > 1)
                    return $"{(int)remaining.TotalDays} days left";
                if (remaining.TotalHours > 1)
                    return $"{(int)remaining.TotalHours} hours left";
                if (remaining.TotalMinutes > 0)
                    return $"{(int)remaining.TotalMinutes} mins left";

                return "Deleting soon";
            }
        }
    }

    public class NoteItem
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public string ContentPreview
        {
            get
            {
                var plainText = System.Text.RegularExpressions.Regex.Replace(
                    Content ?? "",
                    "<.*?>",
                    string.Empty
                );

                plainText = System.Net.WebUtility.HtmlDecode(plainText);

                return plainText.Length > 80
                    ? plainText.Substring(0, 80) + "..."
                    : plainText;
            }
        }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string DeleteActionText => IsDeleted ? "Delete Permanently" : "Delete";

        public string RemainingTime
        {
            get
            {
                if (!IsDeleted || DeletedAt == null) return string.Empty;
                var remaining = DeletedAt.Value.AddDays(5) - DateTime.UtcNow;

                if (remaining.TotalDays > 1)
                    return $"{(int)remaining.TotalDays} days left";
                if (remaining.TotalHours > 1)
                    return $"{(int)remaining.TotalHours} hours left";
                if (remaining.TotalMinutes > 0)
                    return $"{(int)remaining.TotalMinutes} mins left";

                return "Deleting soon";
            }
        }
    }
}