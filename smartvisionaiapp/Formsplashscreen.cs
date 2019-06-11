using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace smartvisionaiapp
{
    public partial class Formsplashscreen : Form
    {
        int pos = 0;
        string[] msgs = { "Carregando . . .", "Verificando Instalação . . .", "Verificando Programas . . .", "Iniciando App . . ." };
        bool instalacaook = true;
        bool programasok = false;

        public Formsplashscreen()
        {
            InitializeComponent();
            this.TransparencyKey = Color.DimGray;
            
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (pos < 3)
            {
                pos++;
                label1.Text = msgs[pos];
                progressBar1.Value = (pos+1) * 25;
            }
            else
            {
                if(programasok)
                {
                    if (instalacaook)
                    {
                        timer1.Enabled = false;
                        new Form1().Show();
                        Hide();
                        
                    }
                }
                else
                {
                    timer1.Enabled = false;
                    Hide();
                    MessageBox.Show("O PROGRAMA JÁ ESTÁ ABERTO EM SEU COMPUTADOR!!!", "AVISO!");
                    Close();
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            Process[] p = Process.GetProcesses().Where(x => x.ProcessName == "smartvisionaiapp").ToArray();
            if (p.Count()<=1)
            {
                programasok = true;
            }
        }
    }
}
