using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Drawing;

namespace SuperReversi
{
    class Client
    {
        
        private Socket client;
        private byte[] data = new byte[1024];
        private int size = 1024;


        public stringDelegate serverEvent;

        public Client(string ip)
        {
            client = new Socket(AddressFamily.InterNetwork,
                            SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint iep = new IPEndPoint(IPAddress.Parse(ip), 3000);
            client.BeginConnect(iep, new AsyncCallback(Connected), client);

        }

        void Connected(IAsyncResult iar)
        {
            client = (Socket)iar.AsyncState;
            try
            {
                client.EndConnect(iar);
                serverEvent("Connected to: " + client.RemoteEndPoint.ToString());
                client.BeginReceive(data, 0, size, SocketFlags.None,
                              new AsyncCallback(ReceiveData), client);
            }
            catch (SocketException)
            {
                serverEvent("Error connecting");
            }
        }

        public void sendMove(ReversiState.Pieces type, Point position)
        {
            string moveString = "MOVE:";
            moveString += position.X + "," + position.Y + "," + (int)type + "\n";

            sendMessage(moveString);
        }

        void ReceiveData(IAsyncResult iar)
        {
            Socket remote = (Socket)iar.AsyncState;
            int recv = remote.EndReceive(iar);
            string stringData = Encoding.ASCII.GetString(data, 0, recv);
            remote.BeginReceive(data, 0, size, SocketFlags.None,
                          new AsyncCallback(ReceiveData), remote);
            serverEvent(stringData);
            
        }

        private void sendMessage(string message)
        {
            byte[] byteMessage = Encoding.ASCII.GetBytes(message);

            client.BeginSend(byteMessage, 0, byteMessage.Length, SocketFlags.None,
                        new AsyncCallback(SendData), client);
        }

        void SendData(IAsyncResult iar)
        {
            Socket remote = (Socket)iar.AsyncState;
            int sent = remote.EndSend(iar);
        }
    }
}
