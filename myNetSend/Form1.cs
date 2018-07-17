using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace myNetSend
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                NetSend.SendMessage(tbDstName.Text, tbSrc.Text, tbMsg.Text);
                MessageBox.Show("Success!", "information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }).Start();
        }

    }
}
