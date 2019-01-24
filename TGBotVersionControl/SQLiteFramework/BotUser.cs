using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
