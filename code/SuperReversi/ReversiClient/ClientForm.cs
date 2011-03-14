using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace SuperReversi
{
    public delegate void stringDelegate(string text);

    public partial class ClientForm : Form
    {
        Client client;
        ReversiState gameState;
        ReversiState.Pieces whoAmI;
        Random random;
        AI ai;
       
        public ClientForm()
        {
            InitializeComponent();

            client = new Client("127.0.0.1");
            client.serverEvent = new stringDelegate(this.serverLog);

            gameState = new ReversiState();
            random = new Random();
        }

        public void serverLog(string text) 
        { 
            if(InvokeRequired) 
            { 
                System.Diagnostics.Debug.WriteLine("not in the right thread");
                Invoke(new stringDelegate(serverLog), text); 
            } 
            else 
            {
                if (text.IndexOf("\n") != -1)
                {
                    foreach(string message in text.Split('\n')) {
                        serverLog(message);
                    }
                    return;
                }
                
                if (text.IndexOf("STATE:") != -1)
                {
                    gameState = ReversiState.unserialize(text);
                }
                else if (text.IndexOf("YOU") != -1)
                {
                    whoAmI = (ReversiState.Pieces)Enum.Parse(typeof(ReversiState.Pieces), text.Substring(text.IndexOf("YOU")+4));
                    if (whoAmI == gameState.turn)
                    {
                        client.sendMove(whoAmI, new AI(gameState).getBestMove());
                    }
                }
                textBox1.Text += text + "\n";


            } 
        }
    }
}
