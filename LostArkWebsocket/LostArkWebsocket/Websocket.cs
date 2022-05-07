using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace LostArkWebsocket
{

    public class LostArkWebsocket
    {
        WebSocketServer listener;
        public static List<WsBehav> clients = new List<WsBehav>();
        bool started = false;

        public LostArkWebsocket()
        {

        }


        public void SendData(string data)
        {
            if (!started)
                return;
            if (clients.Count == 0)
                return;
            clients[0].SendToAll(data);
        }
    
        public int Start()
        {
            if (started)
                return -1;
            started = true;
            int port = LostArkWebsocket.FreeTcpPort();

            listener = new WebSocketServer(port);
            listener.AddWebSocketService<WsBehav>("/data");
            listener.Start();
            return port; 
        }

        private static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
    }
    public class WsBehav : WebSocketBehavior
    {
        protected override void OnOpen()
        {
            base.OnOpen();
            LostArkWebsocket.clients.Add(this);
        }
        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);

        }
        public void SendToAll(string data)
        {
            Sessions.Broadcast(data);
        }
    }

}
