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
            await Bot.SendTextMessageAsync(msg.Chat.Id, $"Pong!", replyToMessageId: msg.MessageId);
        }

        [Command("/start", ProcessOnAllNodes = false, Standalone = true, Usage = "/start", Description = "Startet den Bot.", RequiredContext = CommandAttribute.Context.Private)]
        public static async Task Start(CommandContext context)
        {
            var Bot = context.Bot;
            var msg = context.Message;
            var db = context.DB;
            await Bot.SendTextMessageAsync(msg.Chat.Id, "Hallo! Schön, dass du mit mir spielen willst!");
            msg.From.FindOrCreateBotUser(db).Update(msg.From);
            db.SaveChanges();
        }

        [Command("/GA", ProcessOnAllNodes = false, Standalone = true, Usage = "/GA", Description = "Zeigt dir, ob du ein GA bist.", RequiresGlobalAdmin = true)]
        public static async Task GA(CommandContext context)
        {
            var Bot = context.Bot;
            var msg = context.Message;

            await Bot.SendTextMessageAsync(msg.Chat.Id, "Ja, du bist ein Schutzengel.");
        }
    }
}
