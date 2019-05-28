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

        List<detectado> detectadolist = new List<detectado>();
        public Formtelaprocessamento(List<Bitmap> listaimg, double fps, int totalframes,Color cor)
        {
            InitializeComponent();
            this.fps = fps;
            this.listaframes = listaimg;
            totalframe = totalframes;
            corquadrado = cor;
            
        }

        private void Formtelaprocessamento_Load(object sender, EventArgs e)
        {
            d = totalframe / 4;
            timer1.Enabled = true;
            for (int i = 0; i < Convert.ToInt32(Math.Round(d)); i++)
            {
                
                    Process.Start(@"C:\Users\Administrador\Desktop\FAGUNDES\projetosc#\REPOSITORIOS\yolodroneapp\testealturosyolo\videospliter\bin\Debug\videospliter.exe");
                
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
                Hide();
                new Formexiberesult().Show();
                Close();
            }
        }
    }
}
