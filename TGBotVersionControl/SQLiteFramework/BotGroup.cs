using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaltMalKurzControl.SQLiteFramework
{
    [Table("groups")]
    public class BotGroup
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
        public string Title { get; set; }
        public string Username { get; set; }
        public bool IsSuperGroup { get; set; }
        public int GamesPlayed { get; set; }
    }
}
