using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace SuperReversi
{
    public partial class GameForm : Form
    {
        ReversiState gameState;
        Server server;

        public GameForm()
        {
            InitializeComponent();
            gameState = ReversiState.initial();
            server = new Server();
            server.onClientConnect = this.clientConnect;
            server.onReceive = this.receiveData;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            foreach(ReversiState.Piece piece in gameState.getPieces()) {
                Rectangle myRectangle = pieceRectangle(piece.position);
                g.FillEllipse(new SolidBrush(piece.type == ReversiState.Pieces.White ? Color.White : Color.Black), myRectangle);  
            }
            
        }

        private Rectangle pieceRectangle(Point position)
        {
            float height = pictureBox1.Height / 8 - 1;
            float width = pictureBox1.Width / 8 - 1;

            return new Rectangle((int)Math.Floor(position.X * (width+1) + 1), (int)Math.Floor(position.Y * (height+1) + 1), (int)width, (int)height); 
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            float height = pictureBox1.Height / 8 - 1;
            float width = pictureBox1.Width / 8 - 1;
            
            Point click = pictureBox1.PointToClient(new Point(Cursor.Position.X, Cursor.Position.Y));
            Debug.Write("Mouse click:" + click.X + " , " + click.Y + "\n");

            Point gamePos = new Point((int)Math.Floor(click.X / (width+1)), (int)Math.Floor(click.Y / (height+1)));
            if (gamePos.X < 0) gamePos.X = 0;
            else if (gamePos.X > 7) gamePos.X = 7;

            if (gamePos.Y < 0) gamePos.Y = 0;
            else if (gamePos.Y > 7) gamePos.Y = 7;

            makeMove(gamePos);
        }

        public void makeMove(Point position, ReversiState.Pieces type = ReversiState.Pieces.None)
        {
            gameState = gameState.placePiece(position, type);
            server.sendGameState(gameState);
            pictureBox1.Invalidate();
        }

        public void clientConnect(System.Net.Sockets.Socket client)
        {
            if (InvokeRequired)
            {
                System.Diagnostics.Debug.WriteLine("not in the right thread");
                Invoke(new clientConnectDelegate(clientConnect), client);
            }
            else
            {
                server.sendGameState(client, gameState);
            }
        }

        public void receiveData(System.Net.Sockets.Socket client, string message)
        {
            if (InvokeRequired)
            {
                Invoke(new stringDelegate(receiveData), new object[] {client, message});
            }
            else
            {
                if (message.IndexOf("\n") != -1)
                {
                    foreach (string msg in message.Split('\n'))
                    {
                        receiveData(client, msg);
                    }
                    return;
  
                } else if (message.IndexOf("MOVE") != -1) {
                    message = message.Substring(message.IndexOf("MOVE:") + 5);

                    string[] messageArray = message.Split(',');

                    makeMove(new Point(Int32.Parse(messageArray[0]), Int32.Parse(messageArray[1])), (ReversiState.Pieces)Enum.Parse(typeof(ReversiState.Pieces), messageArray[2]));
                }
            }
        }

    }
}
