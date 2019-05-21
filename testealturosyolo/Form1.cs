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

namespace testealturosyolo
{
    public partial class Form1 : Form
    {
        Color corquadrado = Color.Red;
        Camera c;
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
            double total = 0;
            if (of.ShowDialog() == DialogResult.OK)
            {
                var ms = new MemoryStream();
                Bitmap b = new Bitmap(of.FileName);
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
                    label1.Text = $"n° pessoas: {items.ToList().Count()}";
                    foreach (var item in items)
                    {
                        total += item.Confidence;
                        if (item.Type.ToLower() == "person")
                        {
                            //desenha quadrado

                            for (int i2 = 0; i2 < 5; i2++)
                            {
                                for (int i = 0; i < item.Width; i++)
                                {
                                    try
                                    {
                                        b.SetPixel(item.X + i, item.Y + i2, corquadrado);
                                    }
                                    catch (Exception)
                                    {}
                                }
                            }
                            for (int i2 = 0; i2 < 5; i2++)
                            {
                                for (int i = 0; i < item.Width+5; i++)
                                {
                                    try
                                    {
                                        b.SetPixel(item.X + i, item.Y + i2 + item.Height, corquadrado);
                                    }
                                    catch (Exception)
                                    {}
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
                                    {}
                                }
                            }

                        }

                    }
                    double a = 0;
                    a = total / Convert.ToDouble(items.ToList().Count());
                    a = Math.Round(a, 4);
                    a = a * 100;
                    label2.Text = $"media %: {a}";
                    pictureBox1.Image = b;
                }
                
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
    }
}
