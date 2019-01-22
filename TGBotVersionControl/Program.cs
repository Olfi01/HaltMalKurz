using HaltMalKurzControl.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using ZetaIpc.Runtime.Client;
using ZetaIpc.Runtime.Server;

namespace HaltMalKurzControl
{
    class Program
    {
        private static readonly List<Node> nodes = new List<Node>();
        private static string token;
        private static TelegramBotClient Bot;

        static void Main(string[] args)
        {
            token = args[0];
            Bot = new TelegramBotClient(token);
            Bot.OnUpdate += Bot_OnUpdate;
            Bot.StartReceiving();

            string input;
            do
            {
                input = Console.ReadLine();
                switch (input.FirstWord())
                {
                    case "stop":
                        if (!input.Contains(" "))
                        {
                            Console.WriteLine("Usage: stop [node-guid]");
                            break;
                        }
                        var nodeGuid = Guid.Parse(input.Substring(input.IndexOf(" ")).Trim());
                        nodes.ForEach(x => { if (x.Guid.Equals(nodeGuid)) x.Stop(); });
                        Console.WriteLine("Stopping node with guid {0}.", nodeGuid.ToString());
                        break;
                    case "exit":
                        if (nodes.Any(x => !x.Stopped))
                        {
                            Console.WriteLine("Stopping all nodes...");
                            nodes.ForEach(x => x.Stop());
                            while (nodes.Any(x => !x.Stopped)) { }
                            Console.WriteLine("Nodes stopped.");
                        }
                        break;
                    case "nodes":
                        nodes.ForEach(x => Console.WriteLine("Node {0} ({1}): Stopped={2}", x.Process.ToString(), x.Guid.ToString(), x.Stopped));
                        if (nodes.Count < 1) Console.WriteLine("No nodes present.");
                        break;
                    default:
                        Console.WriteLine("Command does not exist.");
                        break;
                }
                Console.WriteLine();
            } while (input.FirstWord() != "exit");
            Bot.StopReceiving();
        }

        private static void Bot_OnUpdate(object sender, UpdateEventArgs e)
        {
            // update handling will be here

            nodes.ForEach(x => x.SendMessage(new IpcMessage(e.Update)));
        }
    }
}
