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
        List<Itemdetectado> detecitem = new List<Itemdetectado>();
        string[] baseimg;
        Graphics gbarra;
        bool arastando = false;
        int frameolhando = 0;
        bool olhandoframe = false;
        bool pausou = false;
        public Form1 ff;
        public Formexiberesult(Form1 f)
        {
            InitializeComponent();
            ff = f;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            
        }

        public void clicoudeteccao(int frame)
        {
            frameolhando = frame;
            olhandoframe = true;
            if (ind > 2)
            {
                ind = frame - 2;
            }
            else
            {
                ind = 0;
            }
        }

        private void Formexiberesult_Load(object sender, EventArgs e)
        {
            gbarra = barrarep.CreateGraphics();
            string[] s = Directory.GetFiles("data");
                    baseimg = Directory.GetFiles("imgtemp");
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
                    foreach (var item in detectadolist.OrderBy(x=>x.frame))
                    {
                        tableLayoutPanel1.Controls.Add(new Itemdetectado(item, this));
                        detecitem.Add(new Itemdetectado(item, this));
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
                        foreach (var item in detectadolist.OrderBy(x => x.frame))
                        {
                            tableLayoutPanel1.Controls.Add(new Itemdetectado(item, this));
                            detecitem.Add(new Itemdetectado(item, this));
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
                //----------
                //move barra rep sozinho
                int widthbarra = barrarep.Width;
                if (!arastando)
                {
                    int d = baseimg.Count();
                    widthbarra = barrarep.Width;
                    int xinicio = barrarep.Location.X;

                    double porcent = Convert.ToDouble(ind) / Convert.ToDouble(d);
                    porcent = porcent * widthbarra;
                    controlerep.Location = new Point(xinicio + Convert.ToInt32(Math.Round(porcent)), controlerep.Location.Y);

                }
                //---------------


                int especura = 0;
                int dsegundos = baseimg.Count();

                especura = widthbarra / dsegundos;
                foreach (var item in detectadolist.OrderBy(x => x.frame))
                {
                    double porcent = Convert.ToDouble(item.frame) / Convert.ToDouble(dsegundos);
                    porcent = porcent * widthbarra;
                    gbarra.FillRectangle(Brushes.Red, (float)(porcent - (especura / 2)), 0, especura, 20);
                }

                if (ind == frameolhando + 2)
                {
                    if (olhandoframe)
                    {
                        if (frameolhando > 2)
                        {
                            ind = frameolhando - 2;
                        }
                    }
                }
                
                    ff.movedronesegundo(ind);
                    
                
                ind++;
                
            }
            else
            {
                ind = 0;
                if (olhandoframe)
                {
                    if (frameolhando > 2)
                    {
                        ind = frameolhando - 2;
                    }
                }
            }
        }

        private void barrarep_MouseDown(object sender, MouseEventArgs e)
        {
            //arastando = true;
            
            double especura = 0;
            int dsegundos = baseimg.Count();
            int widthbarra = barrarep.Width;

            especura = Convert.ToDouble(widthbarra) / Convert.ToDouble(dsegundos);
            ind = Convert.ToInt32(Math.Truncate(Convert.ToDouble(e.X) / especura));
            timer2.Enabled = false;
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
                    segundos = ind;
                }
                label2.Text = $"{minutos}:{segundos}";
            }
            catch (Exception)
            { }
            
            if (!arastando)
            {
                int d = baseimg.Count();
                widthbarra = barrarep.Width;
                int xinicio = barrarep.Location.X;

                double porcent = Convert.ToDouble(ind) / Convert.ToDouble(d);
                porcent = porcent * widthbarra;
                controlerep.Location = new Point(xinicio + Convert.ToInt32(Math.Round(porcent)), controlerep.Location.Y);

            }
            pausou = true;
            button2.Image = testealturosyolo.Properties.Resources.play;

            int especuraa = 0;
            int dsegundoss = baseimg.Count();

            especuraa = widthbarra / dsegundoss;
            foreach (var item in detectadolist.OrderBy(x => x.frame))
            {
                double porcent = Convert.ToDouble(item.frame) / Convert.ToDouble(dsegundoss);
                porcent = porcent * widthbarra;
                gbarra.FillRectangle(Brushes.Red, (float)(porcent - (especuraa / 2)), 0, especuraa, 20);
            }
            olhandoframe = false;
            frameolhando = 0;
        }

        private void controlerep_MouseDown(object sender, MouseEventArgs e)
        {
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (pausou)
            {
                timer2.Enabled = true;
                pausou = false;
                button2.Image = testealturosyolo.Properties.Resources.pause;
            }
            else
            {
                timer2.Enabled = false;
                pausou = true;
                button2.Image = testealturosyolo.Properties.Resources.play;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            olhandoframe = false;
            frameolhando = 0;
            ind = 0;
        }
    }
}
