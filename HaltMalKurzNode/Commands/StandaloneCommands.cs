using HaltMalKurzControl.Helpers;
using HaltMalKurzControl.SQLiteFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaltMalKurzNode.Commands
{
    public class StandaloneCommands
    {
        [Command("/ping", ProcessOnAllNodes = false, Standalone = true, Usage = "/ping", Description = "Pingt den Bot.")]
        public static async Task Ping(CommandContext context)
        {
            var Bot = context.Bot;
            var msg = context.Message;
            await Bot.SendTextMessageAsync(msg.Chat.Id, $"Pong!\nTime to receive message: {(DateTime.Now - msg.Date).TotalSeconds} seconds.");
        }

        [Command("/start", ProcessOnAllNodes = false, Standalone = true, Usage = "/start", Description = "Startet den Bot.", RequiredContext = CommandAttribute.Context.Private)]
        public static async Task Start(CommandContext context)
        {
            var Bot = context.Bot;
            var msg = context.Message;
            var db = context.DB;
            if (db.Users.Find(msg.From.Id) == null) db.Users.Add(BotUser.FromUser(msg.From));
            db.Users.Find(msg.From.Id).Update(msg.From);
            await db.SaveChangesAsync();
            await Bot.SendTextMessageAsync(msg.Chat.Id, "Hallo! Schön, dass du mit mir spielen willst!");
        }
    }
}
