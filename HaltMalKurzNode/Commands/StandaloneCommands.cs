using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaltMalKurzNode.Commands
{
    public class StandaloneCommands
    {
        [Command("/ping", ProcessOnAllNodes = false, Standalone = true, Usage = "/ping", Description = "Pings the bot.")]
        public static async Task Ping(CommandContext context)
        {
            var Bot = context.Bot;
            var msg = context.Message;
            await Bot.SendTextMessageAsync(msg.Chat.Id, $"Pong!\nTime to receive message: {(DateTime.Now - msg.Date).TotalSeconds} seconds.");
        }
    }
}
