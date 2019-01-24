using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Telegram.Bot.Types;

namespace HaltMalKurzControl.SQLiteFramework
{
    public class BotUser
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get => string.Join(" ", FirstName, LastName); }
        public string Username { get; set; }
        public string LanguageCode { get; set; }
        public int GamesPlayed { get; set; }
        public bool IsGlobalAdmin { get; set; }

        public static BotUser FromUser(User from)
        {
            return new BotUser
            {
                Id = from.Id,
                FirstName = from.FirstName,
                LastName = from.LastName,
                Username = from.Username,
                LanguageCode = from.LanguageCode,
                IsGlobalAdmin = (from.Id == 267376056)
            };
        }
    }
}
