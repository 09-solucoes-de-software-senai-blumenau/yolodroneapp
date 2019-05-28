using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace testealturosyolo
{
    public partial class Formexiberesult : Form
    {
        List<Bitmap> listaimg = new List<Bitmap>();
        List<detectado> detectadolist = new List<detectado>();
        public Formexiberesult()
        {
            InitializeComponent();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            
        }

        public void clicoudeteccao(int frame)
        {

        }

        private void Formexiberesult_Load(object sender, EventArgs e)
        {
            
                    string[] s = Directory.GetFiles("data");
                    string[] baseimg = Directory.GetFiles("imgtemp");
                    string[] s2 = Directory.GetFiles("imgdetected");
            int segundos = 0;
            int minutos = 0;
            if (baseimg.Count() > 60)
            {
                minutos = Convert.ToInt32(Math.Truncate(Convert.ToDouble(baseimg.Count()) / 60));
                segundos = baseimg.Count() - (minutos * 60);
            }
            else
            {
                segundos = baseimg.Count();
            }
            label3.Text = $"{minutos}:{segundos}";
            foreach (var item in baseimg.ToList())
                    {
                        listaimg.Add(new Bitmap(item));
                    }
                    foreach (var item in s.ToList())
                    {
                        string raw = "";
                        raw = File.ReadAllText(item);
                        string[] data = raw.Split('|');
                        detectado dt = new detectado();
                        dt.confianca = Convert.ToDouble(data[0]);
                        dt.quantpessoas = Convert.ToInt32(data[1]);
                        dt.frame = Convert.ToInt32(data[2]);
                        dt.img = new Bitmap(s2.ToList().FirstOrDefault(x => x.Contains($"{dt.frame}")));
                        detectadolist.Add(dt);
                    }
                    foreach (var item in detectadolist)
                    {
                        tableLayoutPanel1.Controls.Add(new Itemdetectado(item, this));
                        listaimg[item.frame - 1] = item.img;
                    }
            
            
            timer1.Enabled = true;
            timer2.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
                try
                {
                    List<detectado> dlistcomp = new List<detectado>();
                    string[] s = Directory.GetFiles("data");
                    string[] s2 = Directory.GetFiles("imgdetected");
                    foreach (var item in s.ToList())
                    {
                        string raw = "";
                        raw = File.ReadAllText(item);
                        string[] data = raw.Split('|');
                        detectado dt = new detectado();
                        dt.confianca = Convert.ToDouble(data[0]);
                        dt.quantpessoas = Convert.ToInt32(data[1]);
                        dt.frame = Convert.ToInt32(data[2]);
                        dt.img = new Bitmap(s2.ToList().FirstOrDefault(x => x.Contains($"{dt.frame}")));
                        dlistcomp.Add(dt);
                    }
                    if (dlistcomp.Count() > detectadolist.Count())
                    {
                        detectadolist = dlistcomp;
                        tableLayoutPanel1.Controls.Clear();
                        foreach (var item in detectadolist)
                        {
                            tableLayoutPanel1.Controls.Add(new Itemdetectado(item, this));
                            listaimg[item.frame - 1] = item.img;
                        }

                    }
           
                }
                catch (Exception)
                  {}
            
            
        }
        int ind=0;
        private void timer2_Tick(object sender, EventArgs e)
        {
            if (ind < listaimg.Count())
            {
                try
                {
                    pictureBox1.Image = listaimg[ind];
                    int segundos = 0;
                    int minutos = 0;
                    if (ind > 60)
                    {
                        minutos = Convert.ToInt32(Math.Truncate(Convert.ToDouble(ind) / 60));
                        segundos = ind - (minutos * 60);
                    }
                    else
                    {
                        segundos =ind;
                    }
                    label2.Text = $"{minutos}:{segundos}";
                }
                catch (Exception)
                {}
                ind++;
            }
            else
            {
                ind = 0;
            }
        }
    }
}
