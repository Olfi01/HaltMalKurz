using HaltMalKurzControl;
using HaltMalKurzControl.Helpers;
using HaltMalKurzControl.SQLiteFramework;
using HaltMalKurzNode.Commands;
using HaltMalKurzNode.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ZetaIpc.Runtime.Server;

namespace HaltMalKurzNode
{
    class Program
    {
        private static IpcServer _server;
        private static HaltMalKurzContext db;
        private const string version = "v0.0.1";
        private static readonly ManualResetEvent stopEvent = new ManualResetEvent(false);
        private static TelegramBotClient Bot;
        private static bool stopping = false;
        private static List<Game> games = new List<Game>();
        private static readonly List<BotCommand> commands = new List<BotCommand>();
        private static string botUsername;

        static void Main(string[] args)
        {
            int port = int.Parse(args[0]);
            string eventHandleName = args[1];
            string botToken = args[2];
            var eventWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, eventHandleName);
            db = new HaltMalKurzContext();
            _server = new IpcServer();
            _server.ReceivedRequest += ServerReceivedRequest;
            _server.Start(port);
            Bot = new TelegramBotClient(botToken);
            botUsername = Bot.GetMeAsync().Result.Username;
            eventWaitHandle.Set();

            InitCommands();

            stopEvent.WaitOne();
            stopping = true;
            do Thread.Sleep(500); while (games.Count > 0);

            Environment.Exit(0);
        }

        private static void InitCommands()
        {
            Type[] classesToSearch = { typeof(StandaloneCommands), typeof(GACommands) };

            foreach (Type t in classesToSearch)
            {
                foreach (var method in t.GetMethods())
                {
                    if (method.ReturnParameter?.ParameterType != typeof(Task)) continue;
                    var parameters = method.GetParameters();
                    if (parameters.Length != 1 || parameters[0].ParameterType != typeof(CommandContext)) continue;
                    var commandAttributes = method.GetCustomAttributes().OfType<CommandAttribute>();
                    if (commandAttributes.Count() < 1) continue;
                    Func<CommandContext, Task> action = (Func<CommandContext, Task>)method.CreateDelegate(typeof(Func<CommandContext, Task>));
                    commands.Add(new BotCommand(commandAttributes.First(), action));
                }
            }
        }

        private static void ServerReceivedRequest(object sender, ReceivedRequestEventArgs e)
        {
            if (e.Handled) return;
            IpcMessage requestMessage = JsonConvert.DeserializeObject<IpcMessage>(e.Request);
            switch (requestMessage.Type)
            {
                case IpcMessage.TcpMessageType.Command:
                    #region Handle IPC Commands
                    if (requestMessage.IsGetVersionMessage())
                    {
                        e.Response = version;
                        e.Handled = true;
                        return;
                    }
                    if (requestMessage.IsStopMessage())
                    {
                        stopEvent.Set();
                        e.Handled = true;
                        return;
                    }
                    #endregion
                    break;
                case IpcMessage.TcpMessageType.Update:
                    HandleUpdate(requestMessage.Update);
                    e.Handled = true;
                    break;
            }
        }

        // given an update from the telegram bot API, executes the action required by the update, if any
        private static void HandleUpdate(Update update)
        {
            // only thing we care about right now are messages
            if (update.Type == UpdateType.Message)
            {
                var msg = update.Message;
                // more specifically, text messages
                if (msg.Type == MessageType.Text)
                {
                    var entities = msg.Entities;
                    // we're looking through all the entities in the message, whether there's a bot command at the start of the message
                    if (entities.TryFindWithIndex(x => x.Type == MessageEntityType.BotCommand && x.Offset == 0, out var command, out int index))
                    {
                        // get the text of the command
                        string commandText = msg.EntityValues.ElementAt(index);
                        List<BotCommand> commandsToExecute;
                        // checks for all the prerequisites
                        commandsToExecute = commands.FindAll(x =>
                            (x.Command.Trigger == commandText || x.Command.Trigger + "@" + botUsername == commandText)
                            && x.Command.Standalone
                            && (x.Command.ProcessOnAllNodes || !stopping)
                            && x.Command.HasRequiredContext(msg)
                            && (msg.Text.Trim().Length > command.Length ^ x.Command.Standalone));
                        // create the context for executing the commands
                        CommandContext context = new CommandContext(Bot, msg, db);
                        // go through all commands found
                        foreach (var cmd in commandsToExecute)
                        {
                            // if the chat is a group and the command requires admin, check whether the person issueing the command is admin (or global admin, hehehe)
                            if (msg.Chat.IsGroup() && cmd.Command.RequiresAdmin)
                            {
                                var chatMember = Bot.GetChatMemberAsync(msg.Chat.Id, msg.From.Id).Result;
                                if (chatMember.Status != ChatMemberStatus.Administrator && chatMember.Status != ChatMemberStatus.Creator && !msg.From.IsGlobalAdmin(db)) continue;
                            }
                            // if global admin is required, check whether the person issueing the command is global admin
                            if (cmd.Command.RequiresGlobalAdmin && !msg.From.IsGlobalAdmin(db)) continue;
                            // either execute the command synchronously or asynchronously, depending on configuration
                            if (cmd.Command.ExecuteAsync)
                                cmd.Action.Invoke(context);
                            else
                            {
                                try
                                {
                                    cmd.Action.Invoke(context).Wait();
                                }
                                catch (Exception ex)
                                {
                                    Bot.SendTextMessageAsync(267376056, ex.ToString());
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
