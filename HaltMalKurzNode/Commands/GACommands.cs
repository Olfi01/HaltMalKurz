using HaltMalKurzControl.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaltMalKurzNode.Commands
{
    public class GACommands
    {
        #region SQLite
        [Command("/sqlite", Usage = "/sqlite [query]", Description = "Executes an sqlite query against the database.", ProcessOnAllNodes = false, RequiresGlobalAdmin = true, Standalone = false)]
        public static async Task SQLite(CommandContext context)
        {
            var Bot = context.Bot;
            var msg = context.Message;
            string query = string.Join(" ", context.Args);
            using (var conn = new SQLiteConnection(ConfigurationManager.ConnectionStrings["HaltMalKurzContext"].ConnectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    using (var comm = new SQLiteCommand(query, conn, trans))
                    {
                        using (var reader = comm.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                await Bot.SendTextMessageAsync(msg.Chat.Id, $"Query finished. {reader.RecordsAffected} records affected.", replyToMessageId: msg.MessageId);
                                trans.Commit();
                                return;
                            }
                            else
                            {
                                string response = "";
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    response += $" {reader.GetName(i)} ({reader.GetFieldType(i).Name}) |";
                                }
                                while (reader.Read())
                                {
                                    response += "\n";
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        response += $" {reader[i]} |";
                                    }
                                }
                                await Bot.SendTextMessageAsync(msg.Chat.Id, response, replyToMessageId: msg.MessageId);
                            }
                        }
                    }
                    trans.Commit();
                }
                conn.Close();
            }
        }
        #endregion

        [Command("/promote", Description = "Promotes someone to GA.", ProcessOnAllNodes = false, RequiresGlobalAdmin = true, Standalone = true, Usage = "/promote (reply to user)")]
        public static async Task Promote(CommandContext context)
        {
            var Bot = context.Bot;
            var msg = context.Message;
            var db = context.DB;
            if (msg.ReplyToMessage == null) return;
            var toPromote = msg.ReplyToMessage.From;
            if (toPromote.IsGlobalAdmin(db))
            {
                await Bot.SendTextMessageAsync(msg.Chat.Id, "Diese Person ist bereits GA!");
                return;
            }
            toPromote.FindOrCreateBotUser(db).IsGlobalAdmin = true;
            await db.SaveChangesAsync();
            await Bot.SendTextMessageAsync(msg.Chat.Id, $"{ toPromote.FullName() } wurde zum GA ernannt!");
        }
    }
}
