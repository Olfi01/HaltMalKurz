﻿using HaltMalKurzControl.Helpers;
using HaltMalKurzControl.SQLiteFramework;
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
        private const string repoContainingDir = "C:\\Olfi01\\HaltMalKurz\\repo\\";
        private const string nodesDir = "C:\\Olfi01\\HaltMalKurz\\nodes\\";
        private const string repoDir = repoContainingDir + "HaltMalKurz\\";
        private const string singleNodeDir = repoDir + "HaltMalKurzNode\\bin\\Release\\";
        private const string nodeExecutableName = "HaltMalKurzNode.exe";
        private static readonly List<Node> nodes = new List<Node>();
        private static string token;
        private static TelegramBotClient Bot;
        private static HaltMalKurzContext db;

        static void Main(string[] args)
        {
            #region Init
            token = args[0];
            Bot = new TelegramBotClient(token);
            Bot.OnUpdate += Bot_OnUpdate;
            Bot.StartReceiving();

            db = new HaltMalKurzContext();
            Directory.CreateDirectory(repoContainingDir);
            Directory.CreateDirectory(nodesDir);
            #endregion

            StartNewNode(x => Console.WriteLine(x));

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

            #region Tidy up
            Bot.StopReceiving();
            db.Dispose();
            ClearRecursively(Directory.CreateDirectory(nodesDir));
            #endregion
        }

        private static void ClearRecursively(DirectoryInfo directoryInfo)
        {
            foreach (var dir in directoryInfo.EnumerateDirectories())
            {
                ClearRecursively(dir);
                dir.Delete();
            }
            foreach (var file in directoryInfo.EnumerateFiles())
            {
                file.Delete();
            }
        }

        private static void Bot_OnUpdate(object sender, UpdateEventArgs e)
        {
            if (e.Update.Type == UpdateType.CallbackQuery && e.Update.CallbackQuery.Data == "update")
            {
                if (!e.Update.CallbackQuery.From.IsGlobalAdmin(db))
                {
                    Bot.AnswerCallbackQueryAsync(e.Update.CallbackQuery.Id, "You are not authorized to do this!");
                    return;
                }

                // update routine here

                return;
            }

            nodes.ForEach(x => x.SendMessage(new IpcMessage(e.Update)));
        }

        private static void StartNewNode(Action<string> outputHandler)
        {
            ProcessStartInfo psi = new ProcessStartInfo("first.cmd")
            {
                WorkingDirectory = repoContainingDir,
                UseShellExecute = false
            };
            Process.Start(psi).WaitForExit();

            outputHandler.Invoke("Building...");
            outputHandler.Invoke("");
            psi = new ProcessStartInfo("build.cmd")
            {
                WorkingDirectory = repoContainingDir,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            Process buildProcess = Process.Start(psi);
            buildProcess.StandardOutput.ReadLineAsync().ContinueWith(x => PrintAndContinue(x, buildProcess.StandardOutput, outputHandler));
            buildProcess.WaitForExit();

            outputHandler.Invoke("Copying to directory...");
            outputHandler.Invoke("");
            Guid guid = Guid.NewGuid();
            var targetDir = Directory.CreateDirectory(Path.Combine(nodesDir, guid.ToString()));
            CopyDirectory(Directory.CreateDirectory(singleNodeDir), targetDir);

            psi = new ProcessStartInfo(Path.Combine(targetDir.FullName, nodeExecutableName))
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = targetDir.FullName
            };

            nodes.Add(new Node(psi, guid, token));
        }

        private static void CopyDirectory(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (var dir in source.EnumerateDirectories())
            {
                CopyDirectory(dir, target.CreateSubdirectory(dir.Name));
            }
            foreach (var file in source.EnumerateFiles())
            {
                file.CopyTo(Path.Combine(target.FullName, file.Name));
            }
        }

        private static void PrintAndContinue(Task<string> line, StreamReader standardOutput, Action<string> outputHandler)
        {
            outputHandler.Invoke(line.Result);
            standardOutput.ReadLineAsync().ContinueWith(x => PrintAndContinue(x, standardOutput, outputHandler));
        }
    }
}
