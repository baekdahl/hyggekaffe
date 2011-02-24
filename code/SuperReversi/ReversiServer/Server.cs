using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace SuperReversi
{
    public delegate void clientConnectDelegate(Socket client); 

    class Server
    {
        private Socket server;
        private List<Socket> clients = new List<Socket>();
        private byte[] data = new byte[1024];
        private int size = 1024;

        public clientConnectDelegate onClientConnect; 

        public Server()
        {
            server = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint iep = new IPEndPoint(IPAddress.Any, 3000);
            server.Bind(iep);
            server.Listen(5);
            server.BeginAccept(new AsyncCallback(AcceptConn), server);
        }

        void AcceptConn(IAsyncResult iar)
        {
            Socket oldserver = (Socket)iar.AsyncState;
            Socket client = oldserver.EndAccept(iar);
            clients.Add(client);

            string stringData = "Welcome to my server";
            byte[] message1 = Encoding.ASCII.GetBytes(stringData);
            onClientConnect(client);

            client.BeginSend(message1, 0, message1.Length, SocketFlags.None,
                        new AsyncCallback(SendData), client);
        }

        void SendData(IAsyncResult iar)
        {
            Socket client = (Socket)iar.AsyncState;
            int sent = client.EndSend(iar);
            client.BeginReceive(data, 0, size, SocketFlags.None,
                        new AsyncCallback(ReceiveData), client);
        }

        void ReceiveData(IAsyncResult iar)
        {
            Socket client = (Socket)iar.AsyncState;
            int recv = client.EndReceive(iar);
            if (recv == 0)
            {
                client.Close();
                clients.Remove(client);
                server.BeginAccept(new AsyncCallback(AcceptConn), server);
                return;
            }
            string receivedData = Encoding.ASCII.GetString(data, 0, recv);



            byte[] message2 = Encoding.ASCII.GetBytes(receivedData);
            client.BeginSend(message2, 0, message2.Length, SocketFlags.None,
                         new AsyncCallback(SendData), client);
        }

        public void sendGameState(Socket client)
        {
            sendMessage(client, "STATE");
        }

        private void sendMessage(Socket client, string message)
        {
            byte[] byteMessage = Encoding.ASCII.GetBytes(message);

            client.BeginSend(byteMessage, 0, byteMessage.Length, SocketFlags.None,
                        new AsyncCallback(SendData), client);
        }
    }
}
