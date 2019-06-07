using Alturos.Yolo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace testealturosyolo
{
    public partial class Formtelaprocessamento : Form
    {
        double fps;
        List<Bitmap> listaframes;
        int totalframe;
        Color corquadrado;

        //----------


        double d;
        Form1 ff;
        List<detectado> detectadolist = new List<detectado>();
        public Formtelaprocessamento(List<Bitmap> listaimg, double fps, int totalframes,Color cor,Form1 f)
        {
            InitializeComponent();
            this.fps = fps;
            this.listaframes = listaimg;
            totalframe = totalframes;
            corquadrado = cor;
            ff = f;
        }

        private void Formtelaprocessamento_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            for (int i = 0; i < 31; i++)
            {
                Process.Start(@"..\..\videospliter\bin\Debug\videospliter.exe");   
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int a = -1;
            if (a == -1)
            {
                label1.Text = "processando ";
                a++;
            }
            else
            if (a == 0)
            {
                label1.Text = "processando .";
                a++;
            }else if (a == 1)
            {
                label1.Text = "processando . .";
                a++;
            }else if (a == 2)
            {
                label1.Text = "processando . . .";
                a = -1;
            }
            string[] s = Directory.GetFiles("data");
            if (s.ToList().Count()>1)
            {
                timer1.Enabled = false;
                Hide();
                new Formexiberesult(ff).Show();
                Close();
            }
        }
    }
}
