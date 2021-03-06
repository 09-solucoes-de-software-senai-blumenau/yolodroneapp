﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace videospliter
{
    public partial class Form1 : Form
    {
        Analisador ia = new Analisador();
        List<detectado> detectadolist = new List<detectado>();
        private Color corquadrado = Color.Red;
        int idinstancia = 0;
        string pathgeral = @"";
        string[] quantdiv;
        bool testando = false;
        //testealturosyolo
        public Form1()
        {
            InitializeComponent();
            if (File.Exists("emteste.txt"))
            {
                testando = true;
            }
            Directory.CreateDirectory("locate");
            FileStream fil = File.Create(@"locate\naoremover.txt");
            fil.Close();
            string ss = Path.GetFullPath(Directory.GetFiles("locate").First());
            string[] corte = ss.Split('\\');
            string pathcriando = "";
            foreach (var item in corte)
            {
                if (!item.Equals("smartvisionaiapp"))
                {
                    pathcriando += item + "\\";
                }
                else
                {

                    if (testando)
                    {
                        pathcriando += @"smartvisionaiapp\bin\Debug";
                    }
                    else
                    {
                        pathcriando += @"smartvisionaiapp";
                    }
                    break;
                }
            }
            pathgeral = pathcriando;
            quantdiv = Directory.GetFiles(pathgeral + @"/imgtemp");
            string[] s = Directory.GetFiles(pathgeral+@"/ger");
            idinstancia = s.ToList().Count();
            FileStream f = File.Create(pathgeral + $@"/ger/instancia{s.ToList().Count()+1}gerfile.txt");
            f.Close();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!File.Exists(pathgeral+@"/aovivo.txt"))
            {
                int d = Convert.ToInt32(Math.Round(Convert.ToDouble(quantdiv.ToList().Count()) / 2));
                int ultimoindex = -1;
                int index = idinstancia * d;
                index++;
                for (int i = 0; i < d; i++)
                {
                    try
                    {
                        analisaframe(index + i);
                    }
                    catch (Exception)
                    { }
                    try
                    {
                        if (detectadolist.Count() > 0)
                        {
                            if (ultimoindex != detectadolist.Count() - 1)
                            {
                                exportaresult(detectadolist.Count() - 1);
                                ultimoindex = detectadolist.Count() - 1;
                            }
                        }
                    }
                    catch (Exception)
                    { }
                }

                Hide();
                Close();
            }
            else
            {
                int frameanalisar = Directory.GetFiles(pathgeral + @"/imgtemp").ToList().Count(); 
                try
                {
                    analisaframe(frameanalisar);
                }
                catch (Exception)
                { }
                try
                {
                    if (detectadolist.Count() > 0)
                    {
                       exportaresult(detectadolist.Count() - 1);
                       
                    }
                }
                catch (Exception)
                { }
                Hide();
                Close();
            }
        }
        void exportaresult(int index)
        {
            
                detectadolist[index].img.Save(pathgeral + $@"/imgdetected/imgpronta{detectadolist[index].frame}.png");
                FileStream f = File.Create(pathgeral + $@"/data/dadosimg{detectadolist[index].frame}.txt");
                f.Close();
                using (var sw = new StreamWriter(pathgeral + $@"/data/dadosimg{detectadolist[index].frame}.txt"))
                {
                    sw.Write($@"{detectadolist[index].confianca}|{detectadolist[index].quantpessoas}|{detectadolist[index].frame}|{pathgeral + $@"/imgdetected/imgpronta{detectadolist[index].frame}.png"}");
                }
            
        }
        void analisaframe(int idframe)
        {
            // timer1.Enabled = false;
            int nump = 0;

            double total = 0;
            Bitmap btm = new Bitmap(pathgeral+$@"\imgtemp\img{idframe}.png");
                List<classificacao> items = ia.analisar(pathgeral + $@"\imgtemp\img{idframe}.png",10,idinstancia);
                //items[0].Type -> "Person, Car, ..."
                //items[0].Confidence -> 0.0 (low) -> 1.0 (high)
                //items[0].X -> bounding box
                //items[0].Y -> bounding box
                //items[0].Width -> bounding box
                //items[0].Height -> bounding box
                Pen p = new Pen(Brushes.Red, 20);

                foreach (var item in items.Where(x => x.tipo.ToLower() == "pessoa" && x.porcentagem>0.87))
                {
                    total += item.porcentagem;
                    //desenha quadrado
                    nump++;
                    Graphics g = Graphics.FromImage(btm);
                    g.FillRectangle(Brushes.Red, item.x, item.y - 24, 100, 25);
                    g.DrawString(item.tipo.ToUpper(), new Font(FontFamily.GenericSansSerif, 12), Brushes.White, item.x + 5, item.y - 22);
                    for (int i2 = 0; i2 < 5; i2++)
                    {
                        for (int i = 0; i < item.width; i++)
                        {
                            try
                            {
                                btm.SetPixel(item.x + i, item.y + i2, corquadrado);
                            }
                            catch (Exception)
                            { }
                        }
                    }
                    for (int i2 = 0; i2 < 5; i2++)
                    {
                        for (int i = 0; i < item.width + 5; i++)
                        {
                            try
                            {
                                btm.SetPixel(item.x + i, item.y + i2 + item.height, corquadrado);
                            }
                            catch (Exception)
                            { }
                        }
                    }
                    for (int i2 = 0; i2 < 5; i2++)
                    {
                        for (int i = 0; i < item.height; i++)
                        {
                            try
                            {
                                btm.SetPixel(item.x + i2, item.y + i, corquadrado);
                            }
                            catch (Exception)
                            { }
                        }
                    }
                    for (int i2 = 0; i2 < 5; i2++)
                    {
                        for (int i = 0; i < item.height; i++)
                        {
                            try
                            {
                                btm.SetPixel(item.x + i2 + item.width, item.y + i, corquadrado);
                            }
                            catch (Exception)
                            { }
                        }
                    }



                }


            
            if (nump > 0)
            {
                total = total / nump;
                detectado detec = new detectado();
                detec.confianca = total;
                detec.quantpessoas = nump;
                detec.frame = idframe;
                detec.img = btm;
                pictureBox1.Image = btm;
                detectadolist.Add(detec);
                        
            }

        }
    }
}
