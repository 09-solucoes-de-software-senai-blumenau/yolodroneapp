﻿using Alturos.Yolo;
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
using Microsoft.Office.Interop.Excel;
using Range = Microsoft.Office.Interop.Excel.Range;
using Point = System.Drawing.Point;
using System.Text.RegularExpressions;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

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
        public static List<objtelemetria> rawtelemetrydata = new List<objtelemetria>();
        string pathprograma = "";

        //pontos detecção 
        public static List<objtelemetria> cordenadasdetec = new List<objtelemetria>();

        bool inicioumapa = false;
        //real time
        string pathcelular = "";

        string pathtelemetria = "";
        string pathvideo = "";
        
        public static Bitmap imgatual = null;

        bool concluiutelemetria = true;
        bool concluiufoto = true;
        
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
            try
            {
                File.Delete("aovivo.txt");
            }
            catch (Exception)
            { }
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
                            //File.Delete(item);
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
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
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

                convertecsv();


                Directory.CreateDirectory("imgtemp");
                Directory.CreateDirectory("ger");
                Directory.CreateDirectory("imgdetected");
                Directory.CreateDirectory("data");
                if (of.FileName.Contains(".png") || of.FileName.Contains(".jpg"))
                {
                    
                }
                else
                {
                    capture = new VideoCapture(of.FileName);
                    Mat mat = new Mat();
                    capture.Read(mat);
                    imagemvideo = mat.Bitmap;
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

                    new Formtelaprocessamento(listaframes, fps, quantidadepick, corquadrado,this).ShowDialog();


                }
                
            }
            
        }

        public void movedronesegundo(int sec)
        {
            if (!inicioumapa)
            {
                gMapControl1.MapProvider = GMapProviders.GoogleSatelliteMap;
                gMapControl1.Position = new GMap.NET.PointLatLng(latitude, longitude);
                gMapControl1.Zoom = 18;
                inicioumapa = true;
            }
            objtelemetria ob = rawtelemetrydata.FirstOrDefault(x => x.deltasegundos == sec);
            latitude = ob.lat;
            longitude = ob.log;
            dronerotacao = ob.rotacao;
            //posicionadrone(ob.lat, ob.log, 0, ob.rotacao);
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
            //funciona para uma picture box de Size: 525; 508 !!!!!
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
            gMapControl1.Overlays.Clear();
            PointLatLng p = new PointLatLng(latitude, longitude);
            GMapMarker marker = new GMarkerGoogle(p, RotateImage(new Bitmap(testealturosyolo.Properties.Resources.imgdronecima, 50, 50),(float)dronerotacao));
            GMapOverlay markers = new GMapOverlay("markers");
            markers.Markers.Add(marker);
            gMapControl1.Overlays.Add(markers);
            gMapControl1.Position = gMapControl1.Position;
            //codigo antigo
            /*graphmap.FillRectangle(Brushes.Transparent, 0, 0, 1000, 1000);
            try
            {
                graphmap.DrawImage(new Bitmap(@"imgmapa.png"), 0, 0, pictureBox3.Width, pictureBox3.Height);
            }
            catch (Exception)
            {
                graphmap.DrawImage(new Bitmap(pictureBox3.Width,pictureBox3.Height), 0, 0, pictureBox3.Width, pictureBox3.Height);
            }

            graphmap.DrawImage(RotateImage(testealturosyolo.Properties.Resources.quadradocamera, (float)dronerotacao), (float)droneX - 60, (float)droneY - 80, 120, 160);
            
            graphmap.DrawImage(RotateImage(testealturosyolo.Properties.Resources.imgdronecima,(float)dronerotacao), (float)droneX-20, (float)droneY-20, 40, 40);*/
        }

        private void button4_Click(object sender, EventArgs e)
        {
            
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                File.Delete("aovivo.txt");
            }
            catch (Exception)
            { }
            convertecsv();

            new Formexiberesult(this).Show();
        }

        void convertecsv()
        {
            //converte csv de telemetria para object-------------

            string s = Directory.GetFiles(@"..\..\").First();
            s = Path.GetFullPath(s);
            s = s.Replace("\\" + Path.GetFileName(s), "");
            pathprograma = s;

            DateTime tempoinicio = new DateTime();
            bool first = true;
            string todotexto = File.ReadAllText($@"{pathprograma}\bin\Debug\telemetria\telemetria.csv");
            Match m = Regex.Match(todotexto, $"({DateTime.Now.Year})\\/(\\d+)\\/(\\d+)\\s?(\\d+):(\\d+):([0-9\\.]+),[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,([\\-0-9\\/\\s:\\.a-zA-Z_]*),([\\-0-9\\/\\s:\\.a-zA-Z_]*),[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,[\\-0-9\\/\\s:\\.a-zA-Z_]*,([\\-0-9\\/\\s:\\.a-zA-Z_]*)", RegexOptions.Multiline);
            do
            {
                objtelemetria ov = new objtelemetria();
                ov.lat = Convert.ToDouble(m.Groups[7].Value.Replace('.', ','));
                ov.log = Convert.ToDouble(m.Groups[8].Value.Replace('.', ','));
                ov.date = new DateTime(Convert.ToInt32(m.Groups[1].Value), Convert.ToInt32(m.Groups[2].Value), Convert.ToInt32(m.Groups[3].Value), Convert.ToInt32(m.Groups[4].Value), Convert.ToInt32(m.Groups[5].Value), Convert.ToInt32(Math.Truncate(Convert.ToDouble(m.Groups[6].Value.Replace('.', ',')))));
                if (first)
                {
                    ov.deltasegundos = 0;
                    tempoinicio = ov.date;
                    first = false;
                }
                else
                {
                    TimeSpan ts = ov.date - tempoinicio;
                    ov.deltasegundos = Convert.ToInt32(Math.Truncate(ts.TotalSeconds));
                }
                string tyaw = m.Groups[9].Value.Replace('.', ',');
                double rot = Convert.ToDouble(tyaw);
                rot = rot * -1;
                ov.rotacao = rot;
                rawtelemetrydata.Add(ov);
                m = m.NextMatch();

            } while (m.Success);


            //if (rawtelemetrydata.Count() > 0)
            //{
            //    new Formalteralocaliza(rawtelemetrydata[0].lat, rawtelemetrydata[0].log).ShowDialog();
            //}

            //-----------------------------
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            List<Process> p = Process.GetProcesses().Where(x => x.ProcessName == "videospliter").ToList();
            foreach (var item in p)
            {
                item.Kill();
            }
            try
            {
                File.Delete("aovivo.txt");
            }
            catch (Exception)
            {}
            Environment.Exit(0);
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("Selecione qualquer arquivo de seu celular para sincronizar com o drone");
            string s = "";
            OpenFileDialog of = new OpenFileDialog();
            if (of.ShowDialog() == DialogResult.OK)
            {
                s = of.FileName;
            }
            pathcelular = s.Replace("\\"+Path.GetFileName(s), "");
            List<string> arq = Directory.GetFiles(pathcelular).ToList();
            List<string> arquivotelemetria = arq.Where(x => x.Contains($"DJIFlightRecord_{DateTime.Now.Year}-")).ToList();
            string mestexto = "";
            if (DateTime.Now.Month <10)
            {
                mestexto += "0";
                mestexto += DateTime.Now.Month.ToString();
                
            }
            else
            {
                mestexto = DateTime.Now.Month.ToString();
            }
            List<string> arqvideo = arq.Where(x => x.Contains($"2019_{mestexto}") && x.Contains(".mp4")).ToList();
            if(arquivotelemetria.Count>0 && arqvideo.Count > 0)
            {
                MessageBox.Show("sincronizado com sucesso!");
                pathtelemetria = arquivotelemetria[0];
                pathvideo = arqvideo[0];
                if (Directory.Exists("imgtemp"))
                {
                    string[] su = Directory.GetFiles("imgtemp");
                    if (su.ToList().Count() > 0)
                    {
                        foreach (var item in su.ToList())
                        {
                            File.Delete(item);
                        }
                    }
                }
                if (Directory.Exists("data"))
                {
                    string[] su = Directory.GetFiles("data");
                    if (su.ToList().Count() > 0)
                    {
                        foreach (var item in su.ToList())
                        {
                            File.Delete(item);
                        }
                    }
                    
                }
                if (Directory.Exists("imgdetected"))
                {
                    string[] su = Directory.GetFiles("imgdetected");
                    if (su.ToList().Count() > 0)
                    {
                        foreach (var item in su.ToList())
                        {
                            File.Delete(item);
                        }
                    }
                    Directory.Delete("imgdetected");
                }
                Directory.CreateDirectory("imgtemp");
                Directory.CreateDirectory("ger");
                Directory.CreateDirectory("imgdetected");
                Directory.CreateDirectory("data");
                FileStream f = File.Create("aovivo.txt");
                f.Close();
                timer2.Enabled = true;
                new Formexiberesult(1).Show();
            }
            else
            {
                MessageBox.Show("tente novamente!");
            }
        }

        private void timer2_Tick_1(object sender, EventArgs e)
        {
            if (concluiutelemetria)
            {
                timer3.Enabled = true;
            }
            if(concluiufoto)
            {
                timer5.Enabled = true;
            }
        }

        private void timer3_Tick_1(object sender, EventArgs e)
        {
            concluiutelemetria = false;
            timer3.Enabled = false;
            Directory.CreateDirectory("telemetria");
            do
            {
                try
                {
                    string s = Directory.GetFiles(@"..\..\").First();
                    s = Path.GetFullPath(s);
                    s = s.Replace("\\" + Path.GetFileName(s), "");
                    pathprograma = s;

                    Process p = new Process();
                    ProcessStartInfo info = new ProcessStartInfo();
                    info.FileName = "cmd.exe";
                    info.RedirectStandardInput = true;
                    info.UseShellExecute = false;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                    p.StartInfo = info;
                    p.Start();
                    using (StreamWriter sw = p.StandardInput)
                    {
                        if (sw.BaseStream.CanWrite)
                        {
                            sw.WriteLine("cd/");
                            sw.WriteLine($"cd {pathprograma}");
                            sw.WriteLine($"TXTlogToCSVtool \"{pathtelemetria}\" \"{pathprograma}\\bin\\Debug\\telemetria\\telemetria.csv\"");
                        }
                    }
                    break;
                }
                catch (Exception)
                { }
            } while (true);
            do
            {
                try
                {
                    convertecsv();
                    break;
                }
                catch (Exception)
                {}
            } while (true);
            latitude = rawtelemetrydata.Last().lat;
            longitude = rawtelemetrydata.Last().log;
            dronerotacao = rawtelemetrydata.Last().rotacao;
            concluiutelemetria = true;
            if (!inicioumapa)
            {
                gMapControl1.MapProvider = GMapProviders.GoogleSatelliteMap;
                gMapControl1.Position = new GMap.NET.PointLatLng(latitude, longitude);
                gMapControl1.Zoom = 18;
                inicioumapa = true;
            }
            timer1.Enabled = true;
            //timer4.Enabled = true;
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            this.TopMost = true;
            this.TopLevel = true;
        }

        private void timer5_Tick(object sender, EventArgs e)
        {
            concluiufoto = false;
            timer5.Enabled = false;
            capture = new VideoCapture(pathvideo);
            Mat mat = new Mat();
            capture.Read(mat);
            imagemvideo = mat.Bitmap;
            totalframe = capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount);
            Mat ma = new Mat();
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames, totalframe-1);
            capture.Read(ma);
            ma.Bitmap.Save($@"imgtemp/img{quantidadepick + 1}.png");
            quantidadepick++;
            concluiufoto = true;
            Process.Start(@"..\..\videospliter\bin\Debug\videospliter.exe");
        }
    }
}
