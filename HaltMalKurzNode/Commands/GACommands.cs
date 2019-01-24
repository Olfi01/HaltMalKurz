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
    }
}
