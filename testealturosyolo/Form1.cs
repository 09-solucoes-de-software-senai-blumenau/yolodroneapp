using Alturos.Yolo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DarrenLee.Media;
using System.Threading;
using GMap;
using GMap.NET.MapProviders;

namespace testealturosyolo
{
    public partial class Form1 : Form
    {
        Color corquadrado = Color.Red;
        Camera c;
        DateTime inicio;
        int numpessoas = 0;
        bool startou = false;
        int np = 0;
        double media = 0;
        double miliseconds = 0;
        Bitmap img = null;


        //---

        double tamanhoquadrado = 0;
        Graphics graphmap;

        public Form1()
        {
            InitializeComponent();
            try
            {
                c = new Camera();
                getinfo();
                c.OnFrameArrived += C_OnFrameArrived;
            }
            catch (Exception)
            {
                MessageBox.Show("nenhuma camera disponivel!");
            }
            
        }

        private void getinfo()
        {
            var cdv = c.GetCameraSources();
            var cresolu = c.GetSupportedResolutions();
            dataGridView1.DataSource = cdv.ToList();
            dataGridView2.DataSource = cresolu.ToList();
        }

        private void C_OnFrameArrived(object source, FrameArrivedEventArgs e)
        {
            pictureBox2.Image = e.GetFrame();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            OpenFileDialog of = new OpenFileDialog();
            of.Filter = "png|*.png|jpg|*.jpg";
            if (of.ShowDialog() == DialogResult.OK)
            {
                pictureBox2.Image = new Bitmap(of.FileName);
            }
            if (!startou)
            {
                Thread t = new Thread(analisaimg);
                t.Start();
                startou = true;
                timer2.Enabled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ColorDialog c = new ColorDialog();
            if(c.ShowDialog() == DialogResult.OK)
            {
                corquadrado = c.Color;
                panel2.BackColor = c.Color;
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }
        void analisaimg()
        {
            do
            {
                // timer1.Enabled = false;
                numpessoas = 0;

                double total = 0;

                inicio = DateTime.Now;
                var ms = new MemoryStream();
                Bitmap b = new Bitmap(pictureBox2.Image);

                b.Save(ms, ImageFormat.Jpeg);


                var configurationDetector = new ConfigurationDetector();
                var config = configurationDetector.Detect();

                using (var yoloWrapper = new YoloWrapper(config))
                {
                    var items = yoloWrapper.Detect(ms.ToArray());
                    //items[0].Type -> "Person, Car, ..."
                    //items[0].Confidence -> 0.0 (low) -> 1.0 (high)
                    //items[0].X -> bounding box
                    //items[0].Y -> bounding box
                    //items[0].Width -> bounding box
                    //items[0].Height -> bounding box
                    Pen p = new Pen(Brushes.Red, 20);

                    foreach (var item in items.Where(x => x.Type.ToLower() == "person"))
                    {


                        total += item.Confidence;
                        //desenha quadrado
                        numpessoas++;
                        for (int i2 = 0; i2 < 5; i2++)
                        {
                            for (int i = 0; i < item.Width; i++)
                            {
                                try
                                {
                                    b.SetPixel(item.X + i, item.Y + i2, corquadrado);
                                }
                                catch (Exception)
                                { }
                            }
                        }
                        for (int i2 = 0; i2 < 5; i2++)
                        {
                            for (int i = 0; i < item.Width + 5; i++)
                            {
                                try
                                {
                                    b.SetPixel(item.X + i, item.Y + i2 + item.Height, corquadrado);
                                }
                                catch (Exception)
                                { }
                            }
                        }
                        for (int i2 = 0; i2 < 5; i2++)
                        {
                            for (int i = 0; i < item.Height; i++)
                            {
                                try
                                {
                                    b.SetPixel(item.X + i2, item.Y + i, corquadrado);
                                }
                                catch (Exception)
                                { }
                            }
                        }
                        for (int i2 = 0; i2 < 5; i2++)
                        {
                            for (int i = 0; i < item.Height; i++)
                            {
                                try
                                {
                                    b.SetPixel(item.X + i2 + item.Width, item.Y + i, corquadrado);
                                }
                                catch (Exception)
                                { }
                            }
                        }



                    }
                    double a = 0;
                    a = total / Convert.ToDouble(numpessoas);
                    a = Math.Round(a, 4);
                    a = a * 100;
                    np = numpessoas;
                    media = a;

                    img = b;
                    //pictureBox1.Image = b;
                    //label2.Text = $"media %: {a}";
                    //label1.Text = $"n° pessoas: {numpessoas}";
                }

                DateTime tempoagora = DateTime.Now;
                TimeSpan ts = tempoagora - inicio;
                miliseconds = Math.Truncate(ts.TotalMilliseconds);
                //label6.Text = $"tempo processamento(ms): {Math.Truncate(ts.TotalMilliseconds)}";
                //timer1.Enabled = true;
                
            } while (true);

        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            
        }
        
        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            pictureBox1.Image = img;
            label2.Text = $"media %: {media}";
            label1.Text = $"n° pessoas: {np}";
            label6.Text = $"tempo processamento(ms): {miliseconds}";
            
        }





        //mapa--------------------------------------------------------------

        private void Form1_Load(object sender, EventArgs e)
        {

            
            graphmap = pictureBox3.CreateGraphics();
            calculatamanhoquadrado();
            timer1.Enabled = true;
        }

        
        private void button3_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            new Formalteralocaliza().ShowDialog();
            calculatamanhoquadrado();
            timer1.Enabled = true;
        }
        void calculatamanhoquadrado()
        {
            double zoom = 0;
            if (File.Exists(@"localcord.txt"))
            {
                string s = File.ReadAllText(@"localcord.txt");
                List<string> ss = s.Split('/').ToList();
                zoom = Convert.ToDouble(ss[2]);
            }
            double a = zoom * zoom;
            a = a * -5;
            double b = zoom * 205;
            double c = a + b - 2010;
            tamanhoquadrado = c;
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {

            graphmap.FillRectangle(Brushes.Transparent, 0, 0, 1000, 1000);
            graphmap.DrawImage(new Bitmap(@"imgmapa.png"), 0, 0, pictureBox3.Width, pictureBox3.Height);
            graphmap.DrawRectangle(new Pen(Brushes.Black,2), (float)Convert.ToDouble(pictureBox3.Width / 2) - (float)tamanhoquadrado/2, (float)Convert.ToDouble(pictureBox3.Height / 2) - ((float)tamanhoquadrado / 2)-20, (float)tamanhoquadrado,(float)tamanhoquadrado);
            Brush bru = new SolidBrush(Color.FromArgb(132, 0, 183, 21));
            graphmap.FillRectangle(bru, (float)Convert.ToDouble(pictureBox3.Width / 2) - (float)tamanhoquadrado / 2, (float)Convert.ToDouble(pictureBox3.Height / 2) - ((float)tamanhoquadrado / 2) - 20, (float)tamanhoquadrado, (float)tamanhoquadrado);
            graphmap.DrawImage(testealturosyolo.Properties.Resources.imgdronecima, pictureBox3.Width / 2-20, pictureBox3.Height / 2-20, 40, 40);
        }
    }
}
