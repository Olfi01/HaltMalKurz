﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace HaltMalKurzControl
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class IpcMessage
    {
        [JsonProperty]
        public Update Update { get; set; }
        [JsonProperty]
        public string Command { get; set; }

        public TcpMessageType Type { get => Update != null ? TcpMessageType.Update : TcpMessageType.Command; }

        public IpcMessage(Update update)
        {
            Update = update;
        }

        public IpcMessage(string command)
        {
            Command = command;
        }

        public static IpcMessage Parse(string tcpMessage)
        {
            return JsonConvert.DeserializeObject<IpcMessage>(tcpMessage);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static IpcMessage StopMessage => new IpcMessage("stop");
        public bool IsStopMessage() => Type == TcpMessageType.Command && Command == "stop";

        public static IpcMessage StoppedMessage => new IpcMessage("stopped");
        public bool IsStoppedMessage() => Type == TcpMessageType.Command && Command == "stopped";

        public enum TcpMessageType
        {
            Update,
            Command
        }
    }
}