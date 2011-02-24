using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ReversiClient
{
    public delegate void stringDelegate(string text);

    public partial class ClientForm : Form
    {
        Client client;
        
        public ClientForm()
        {
            InitializeComponent();

            client = new Client("127.0.0.1");
            client.serverEvent = new stringDelegate(this.serverLog);
            listBox1.Items.Add("hej");
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
                listBox1.Items.Add(text);
            } 
        }
    }
}
