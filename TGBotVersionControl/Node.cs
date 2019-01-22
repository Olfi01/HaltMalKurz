using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ZetaIpc.Runtime.Client;
using ZetaIpc.Runtime.Helper;
using ZetaIpc.Runtime.Server;

namespace HaltMalKurzControl
{
    internal class Node
    {
        public Process Process { get; }
        public Guid Guid { get; }
        private readonly IpcClient _client = new IpcClient();
        private readonly int clientPort;
        private readonly ManualResetEvent clientInitialized = new ManualResetEvent(false);
        private string EventHandleName { get => Guid.ToString() + ":started"; }
        public bool Stopped { get; set; } = false;

        public Node(ProcessStartInfo psi)
        {
            Guid = Guid.NewGuid();
            clientPort = FreePortHelper.GetFreePort();
            psi.Arguments = $"{clientPort} {EventHandleName}";
            Process = Process.Start(psi);
            Task.Run((Action)StartClient);
        }

        private void StartClient()
        {
            EventWaitHandle eventWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, EventHandleName);
            eventWaitHandle.WaitOne();
            _client.Initialize(clientPort);
            clientInitialized.Set();
        }

        public string SendMessage(IpcMessage message)
        {
            clientInitialized.WaitOne();
            return _client.Send(message.ToString());
        }

        public void Stop()
        {
            SendMessage(IpcMessage.StopMessage);
            Task.Run(() => { Process.WaitForExit(); Stopped = true; });
        }
    }
}
