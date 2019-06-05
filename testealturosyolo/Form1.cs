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
using System.Drawing.Drawing2D;
using System.Diagnostics;
using Emgu.CV;
using Emgu.CV.Structure;

namespace testealturosyolo
{
    public partial class Form1 : Form
    {
        Color corquadrado = Color.Red;
        DateTime inicio;
        
        
        int numpessoas = 0;
        bool startou = false;
        int np = 0;
        double media = 0;
        double miliseconds = 0;
        Bitmap img = null;


        //---

        
        Graphics graphmap;

        //---
        //variaveis de posicionamento do drone
        double zoomcalculo = 0;
        double latitude = 0;
        double longitude = 0;

        double droneX = 262;
        double droneY = 254;

        double dronelatitude = 0;
        double dronelongitude = 0;
        double dronealtura = 0;
        double dronerotacao = 0;

        int quantidadepick = 0;

        //video -------------
        double totalframe;
        public static double fps;
        int frame;
        VideoCapture capture;
        Bitmap imagemvideo = new Bitmap(100,100);

        //analise com thread----
        List<detectado> detectadolist = new List<detectado>();
        public static List<Bitmap> listaframes = new List<Bitmap>();

        //telemetry
        List<string> rawtelemetrydata = new List<string>();

        public Form1()
        {
            InitializeComponent();

            //Process p = Process.GetProcessesByName("testealturosyolo").First();
            //if ( true)
            //{
            //    MessageBox.Show("aplicativo já aberto!!");
            //    Environment.Exit(0);
            //}
            List<Process> p = Process.GetProcesses().Where(x => x.ProcessName == "videospliter").ToList();
            foreach (var item in p)
            {
                item.Kill();
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            if (Directory.Exists("ger"))
            {
                string[] s = Directory.GetFiles("ger");
                if (s.ToList().Count() > 0)
                {
                    foreach (var item in s.ToList())
                    {
                        File.Delete(item);
                    }
                }
                Directory.Delete("ger");
            }
            if (Directory.Exists("imgdetected"))
            {
                string[] s = Directory.GetFiles("imgdetected");
                if (s.ToList().Count() > 0)
                {
                    foreach (var item in s.ToList())
                    {
                        File.Delete(item);
                    }
                }
                Directory.Delete("imgdetected");
            }
            if (Directory.Exists("data"))
            {
                string[] s = Directory.GetFiles("data");
                if (s.ToList().Count() > 0)
                {
                    foreach (var item in s.ToList())
                    {
                        File.Delete(item);
                    }
                }
                Directory.Delete("data");
            }
            if (Directory.Exists("imgtemp"))
            {
                string[] s = Directory.GetFiles("imgtemp");
                if (s.ToList().Count() > 0)
                {
                    foreach (var item in s.ToList())
                    {
                        File.Delete(item);
                    }
                }
                Directory.Delete("imgtemp");
            }
            Directory.CreateDirectory("imgtemp");
            Directory.CreateDirectory("ger");
            Directory.CreateDirectory("imgdetected");
            Directory.CreateDirectory("data");
            OpenFileDialog of = new OpenFileDialog();
            //seleciona telemetria --------------------------------------------
            //https://phantompilots.com/threads/tool-win-offline-txt-flightrecord-to-csv-converter.70428/
            //TXTlogToCSVtool "C:\temp\inputFile.txt" "C:\temp\outputFile.csv"
            OpenFileDialog of2 = new OpenFileDialog();
            of2.Filter = "TXT|*.txt";
            if (of2.ShowDialog() == DialogResult.OK)
            {
                string pathprograma = "";
                string s = Directory.GetFiles(@"..\..\").First();
                s = Path.GetFullPath(s);
                s = s.Replace("\\" + Path.GetFileName(s), "");
                pathprograma = s;

                if (Directory.Exists("telemetria"))
                {
                    string[] ss = Directory.GetFiles("telemetria");
                    if (ss.ToList().Count() > 0)
                    {
                        foreach (var item in ss.ToList())
                        {
                            File.Delete(item);
                        }
                    }

                }

                Directory.CreateDirectory("telemetria");
                try
                {
                    Process p = new Process();
                    ProcessStartInfo info = new ProcessStartInfo();
                    info.FileName = "cmd.exe";
                    info.RedirectStandardInput = true;
                    info.UseShellExecute = false;

                    p.StartInfo = info;
                    p.Start();
                    using (StreamWriter sw = p.StandardInput)
                    {
                        if (sw.BaseStream.CanWrite)
                        {
                            sw.WriteLine("cd/");
                            sw.WriteLine($"cd {pathprograma}");
                            sw.WriteLine($"TXTlogToCSVtool \"{of2.FileName}\" \"{pathprograma}\\bin\\Debug\\telemetria\\telemetria.csv\"");
                        }
                    }

                }
                catch (Exception)
                { }

            }
            //---------------------------


            if (of.ShowDialog() == DialogResult.OK)
            {
                //converte csv de telemetria para string-------------

                /*try
                {
                    var app = new Microsoft.Office.Interop.Excel.Application();
                    var book = app.Workbooks.Open(@"C:\Users\Administrador\Desktop\FAGUNDES\projetosc#\atividadeimportexportdata\exportacoes\arquivocsv.csv");
                    Worksheet sheet = book.Worksheets[1];

                    var row = 1;

                    while (true)
                    {
                        string s = "";
                        s = ((Range)sheet.Cells[row, 1]).Text;
                        List<string> sl = s.Split(';').ToList();
                        foreach (var item in sl)
                        {
                            Console.Write($"| {item} |");
                        }
                        Console.WriteLine(" ");
                        row++;
                        if (s.Length == 0)
                        {
                            break;
                        }
                    }
                    Console.WriteLine("importado csv");

                }
                catch (Exception)
                { }*/


                //
                Directory.CreateDirectory("imgtemp");
                Directory.CreateDirectory("ger");
                Directory.CreateDirectory("imgdetected");
                Directory.CreateDirectory("data");
                //List<string> pathsfiles = new List<string>();
                //try
                //{
                //    pathsfiles = Directory.GetFiles("C:\\Users\\Administrador\\AppData\\Local\\Microsoft\\Windows\\INetCache\\IE\\Y2ZHAK3M\\").ToList();
                //}
                //catch (Exception)
                //{
                //    pathsfiles = Directory.GetFiles("C:\\Users\\Administrador\\AppData\\Local\\Microsoft\\Windows\\INetCache\\IE\\FNXD0NZ6\\").ToList();
                //}
                //pathsfiles = pathsfiles.Where(x => x.Contains(".png") || x.Contains(".jpg")).ToList();

                //pictureBox2.Image = new Bitmap(pathsfiles[0]);
                if (of.FileName.Contains(".png") || of.FileName.Contains(".jpg"))
                {
                    
                }
                else
                {
                    capture = new VideoCapture(of.FileName);
                    Mat m = new Mat();
                    capture.Read(m);
                    imagemvideo = m.Bitmap;
                    totalframe = capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount);
                    fps = capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps);
                    //Thread video = new Thread(playvideo);
                    //video.Start();
                    //timer3.Enabled = true;
                    
                    
                    Mat ma = new Mat();
                    double b = 29;
                    if (fps > 40)
                    {
                        b = 59;
                    }
                    
                        while (frame < totalframe-b)
                        {
                            if (fps > 40)
                            {
                                frame +=59;
                            }
                            else
                            {
                                frame += 29;
                            }
                            capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames, frame);
                            capture.Read(ma);
                            Thread.Sleep(1);
                            //listaframes.Add(ma.Bitmap);
                            ma.Bitmap.Save($@"imgtemp/img{quantidadepick+1}.png");
                        quantidadepick++;   
                        }

                    new Formtelaprocessamento(listaframes, fps, quantidadepick, corquadrado).ShowDialog();


                }
                
            }
            
        }
        void playvideo()
        {
            Mat m = new Mat();
            while(frame< totalframe)
            {
                frame++;
                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames, frame);
                capture.Read(m);
                imagemvideo = m.Bitmap;
                Thread.Sleep(100);
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

        


       
        private void timer1_Tick(object sender, EventArgs e)
        {
            
        }
        
        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            
            
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
                zoomcalculo = zoom;
                latitude = Convert.ToDouble(ss[0]);
                //18
                //1.391992254E-03
                //19
                //6.983899971E-04
                //20
                //3.503914119E-04
                longitude = Convert.ToDouble(ss[1]);
                //18
                //1.657605171E-03
                //19
                //8.261203767E-04
                //20
                //4.144012929E-04
            }
            
        }

        public void posicionadrone(double lat,double longi, double altura,double rotacao)
        {
            dronelatitude = lat;
            dronelongitude = longi;
            dronealtura = altura;
            dronerotacao = rotacao;

            double a = latitude - (dronelatitude);
            double b = longitude - (dronelongitude);
            if (zoomcalculo == 20)
            {
                if (b > 0)
                {
                    double calculo = (4.144012929E-04 * 100000000) / (b* 100000000);
                    double deltacalculo = 1 / calculo;
                    droneX = (pictureBox3.Width / 2) - (deltacalculo * (pictureBox3.Width / 2));
                    
                }
                else if(b<0)
                {
                    double calculo = (-4.144012929E-04 * 100000000) / (b * 100000000);
                    double deltacalculo = 1 / calculo;
                    droneX = (pictureBox3.Width / 2) + (deltacalculo * (pictureBox3.Width / 2));
                    
                }
                else if (b == 0)
                {
                    droneX = pictureBox3.Width / 2;
                }

                if (a > 0)
                {
                    double calculo = (3.503914119E-04 * 100000000) / (a * 100000000);
                    double deltacalculo = 1 / calculo;
                    droneY = (pictureBox3.Height / 2) + (deltacalculo * (pictureBox3.Height / 2));
                    
                }
                else if (a < 0)
                {
                    double calculo = (-3.503914119E-04 * 100000000) / (a * 100000000);
                    double deltacalculo = 1 / calculo;
                    droneY = (pictureBox3.Height / 2) - (deltacalculo * (pictureBox3.Height / 2));
                    
                }
                else if (a == 0)
                {
                    droneY = pictureBox3.Height / 2;
                }
            }
            else if(zoomcalculo == 19)
            {
                if (b > 0)
                {
                    double calculo = (8.261203767E-04 * 100000000) / (b * 100000000);
                    double deltacalculo = 1 / calculo;
                    
                    droneX = (pictureBox3.Width / 2) - (deltacalculo*(pictureBox3.Width / 2));
                    
                }
                else if (b < 0)
                {
                    double calculo = (-8.261203767E-04 * 100000000) / (b * 100000000);
                    double deltacalculo = 1 / calculo;

                    droneX = (pictureBox3.Width / 2) + (deltacalculo * (pictureBox3.Width / 2));
                    
                }
                else if (b == 0)
                {
                    droneX = pictureBox3.Width / 2;
                }


                if (a > 0)
                {
                    double calculo = (6.983899971E-04 * 100000000) / (a * 100000000);
                    double deltacalculo = 1 / calculo;

                    droneY = (pictureBox3.Height / 2) + (deltacalculo * (pictureBox3.Height / 2));
                    
                }
                else if (a < 0)
                {
                    double calculo = (-6.983899971E-04 * 100000000) / (a * 100000000);
                    double deltacalculo = 1 / calculo;

                    droneY = (pictureBox3.Height / 2) - (deltacalculo * (pictureBox3.Height / 2));
                    
                }
                else if (a == 0)
                {
                    droneY = pictureBox3.Height / 2;
                }
            }
            else if (zoomcalculo == 18)
            {
                if (b > 0)
                {
                    double calculo = (1.657605171E-03 * 100000000) / (b * 100000000);
                    double deltacalculo = 1 / calculo;
                    droneX = (pictureBox3.Width / 2) - (deltacalculo * (pictureBox3.Width / 2));
                    
                }
                else if (b < 0)
                {
                    double calculo = (-1.657605171E-03 * 100000000) / (b * 100000000);
                    double deltacalculo = 1 / calculo;
                    droneX = (pictureBox3.Width / 2) + (deltacalculo * (pictureBox3.Width / 2));
                   
                }
                else if (b == 0)
                {
                    droneX = pictureBox3.Width / 2;
                }


                if (a > 0)
                {
                    double calculo = (1.391992254E-03 * 100000000) / (a * 100000000);
                    double deltacalculo = 1 / calculo;
                    droneY = (pictureBox3.Height / 2) + (deltacalculo * (pictureBox3.Height / 2));

                }
                else if (a < 0)
                {
                    double calculo = (-1.391992254E-03 * 100000000) / (a * 100000000);
                    double deltacalculo = 1 / calculo;
                    droneY = (pictureBox3.Height / 2) - (deltacalculo * (pictureBox3.Height / 2));

                }
                else if (a == 0)
                {
                    droneY = pictureBox3.Height / 2;
                }
            }
        }

        private Bitmap RotateImage(Bitmap b, float angle)
        {
            //Create a new empty bitmap to hold rotated image.
            Bitmap returnBitmap = new Bitmap(b.Width, b.Height);
            //Make a graphics object from the empty bitmap.
            Graphics g = Graphics.FromImage(returnBitmap);
            //move rotation point to center of image.
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.TranslateTransform((float)b.Width / 2, (float)b.Height / 2);
            //Rotate.        
            g.RotateTransform(angle);
            //Move image back.
            g.TranslateTransform(-(float)b.Width / 2, -(float)b.Height / 2);
            //Draw passed in image onto graphics object.
            g.DrawImage(b, new Point(0, 0));
            return returnBitmap;
        }
        private void timer1_Tick_1(object sender, EventArgs e)
        {
           
            graphmap.FillRectangle(Brushes.Transparent, 0, 0, 1000, 1000);
            try
            {
                graphmap.DrawImage(new Bitmap(@"imgmapa.png"), 0, 0, pictureBox3.Width, pictureBox3.Height);
            }
            catch (Exception)
            {
                graphmap.DrawImage(new Bitmap(pictureBox3.Width,pictureBox3.Height), 0, 0, pictureBox3.Width, pictureBox3.Height);
            }

            graphmap.DrawImage(RotateImage(testealturosyolo.Properties.Resources.quadradocamera, (float)dronerotacao), (float)droneX - 60, (float)droneY - 80, 120, 160);
            
            graphmap.DrawImage(RotateImage(testealturosyolo.Properties.Resources.imgdronecima,(float)dronerotacao), (float)droneX-20, (float)droneY-20, 40, 40);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            new Formalteralocaliza(this).ShowDialog();
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            new Formexiberesult().Show();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            List<Process> p = Process.GetProcesses().Where(x => x.ProcessName == "videospliter").ToList();
            foreach (var item in p)
            {
                item.Kill();
            }
        }

        
    }
}
