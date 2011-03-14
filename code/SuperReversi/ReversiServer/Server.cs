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
    public delegate void stringDelegate(Socket client, string message);

    class Server
    {
        private Socket server;
        private Socket clientWhite;
        private Socket clientBlack;

        private byte[] data = new byte[1024];
        private int size = 1024;

        public clientConnectDelegate onClientConnect;
        public stringDelegate onReceive;

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
            Socket client = server.EndAccept(iar);
            client.BeginReceive(data, 0, size, SocketFlags.None,
                    new AsyncCallback(ReceiveData), client);
            addClient(client);
            server.BeginAccept(new AsyncCallback(AcceptConn), null); //Start listening again

            onClientConnect(client);     
        }

        void addClient(Socket client)
        {
            if (clientWhite == null)
            {
                clientWhite = client;
            }
            else if (clientBlack == null)
            {
                clientBlack = client;
            }
        }

        void removeClient(Socket client)
        {
            if (clientWhite == client)
            {
                clientWhite = null;
            }
            else if (clientBlack == client)
            {
                clientBlack = null;
            }
        }

        void SendData(IAsyncResult iar)
        {
            Socket client = (Socket)iar.AsyncState;
            int sent = client.EndSend(iar);
        }

        void ReceiveData(IAsyncResult iar)
        {
            
            Socket client = (Socket)iar.AsyncState;
            try
            {    
                int recv = client.EndReceive(iar);
                if (recv == 0)
                {
                    client.Close();
                    removeClient(client);
                    return;
                }
                string receivedData = Encoding.ASCII.GetString(data, 0, recv);
                client.BeginReceive(data, 0, size, SocketFlags.None,
                    new AsyncCallback(ReceiveData), client);

                onReceive(client, receivedData);
            }
            catch (SocketException e)
            {
                System.Diagnostics.Debug.Write(e.Message);
                removeClient(client);
            }
            
        }

        public ReversiState.Pieces clientSide(Socket client)
        {
            if (clientWhite == client)
            {
                return ReversiState.Pieces.White;
            }
            return ReversiState.Pieces.Black;
        }

        public void sendGameState(Socket client, ReversiState state)
        {
            string stateString = state.serialize();
            
            sendMessage(client, "STATE:" + stateString + "\n");
            sendMessage(client, "YOU:" + (int)clientSide(client) + "\n");
        }

        public void sendGameState(ReversiState state)
        {
            if (clientWhite != null) {
                sendGameState(clientWhite, state);   
            }
            if (clientBlack != null)
            {
                sendGameState(clientWhite, state);  
            }
        }

        private void sendMessage(Socket client, string message)
        {
            byte[] byteMessage = Encoding.ASCII.GetBytes(message);

            client.BeginSend(byteMessage, 0, byteMessage.Length, SocketFlags.None,
                        new AsyncCallback(SendData), client);
        }
    }
}
