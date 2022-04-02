using Prism.Events;
using SimpleTCP;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Mv.Modules.P99.Service
{
    public class Epson2Cognex : IEpson2Cognex
    {
        SimpleTcpServer server;
        SimpleTcpClient client;
        private readonly IEventAggregator eventAggregator;

        public bool ServerConnected { get; private set; }

        public bool ClientConnected { get; private set; }

        public Epson2Cognex(IEventAggregator eventAggregator)
        {
            
            server = new SimpleTcpServer();
            client = new SimpleTcpClient();
            server.DataReceived += Server_DataReceived;
            client.DataReceived += Client_DataReceived;
            this.eventAggregator = eventAggregator;
            checkClientConnected();
            server.Start(6666);
     
        }

        private void Post(string message)
        {
            MessageEvent m = eventAggregator.GetEvent<MessageEvent>();
            m.Publish(message);
        }

        private bool checkClientConnected()
        {
            try
            {
                if (client.TcpClient != null && client.TcpClient.Connected)
                    return true;
                client.Connect("192.168.1.110", 5000);
                return true;
            }
            catch (Exception e)
            {
                Post($"CONNECT ERROR:{e.Message}\r{e.StackTrace}");
                return false;
            }
        }


        private void Client_DataReceived(object sender, Message e)
        {
            Post($"COGNEX TO EPSON:{e.MessageString}");
            if (server.ConnectedClientsCount > 0)
                server.Broadcast(e.Data);
        }

        private void Server_DataReceived(object sender, Message e)
        {
            Post($"EPSON TO COGNEX:{e.MessageString}");
            for (int i = 0; i < 3; i++)
            {
                if (checkClientConnected())
                    break;
                Thread.Sleep(50);
                Post($"重新连接康耐视相机...");
            }
            client.Write(e.Data);
        }
    }
}
