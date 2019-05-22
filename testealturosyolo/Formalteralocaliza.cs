using GMap.NET.MapProviders;
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
    public partial class Formalteralocaliza : Form
    {
        
        double latitude = 0;
        double longitude = 0;
        double zoom = 2;
        public Formalteralocaliza()
        {
            InitializeComponent();
            
            if (File.Exists(@"localcord.txt"))
            {
                string s = File.ReadAllText(@"localcord.txt");
                List<string> ss = s.Split('/').ToList();
                latitude = Convert.ToDouble(ss[0]);
                longitude = Convert.ToDouble(ss[1]);
                zoom = Convert.ToDouble(ss[2]);
            }
        }
        Form1 ff;
        public Formalteralocaliza(Form1 f)
        {
            InitializeComponent();
            ff = f;
            button1.Visible = false;
            button3.Visible = true;
            if (File.Exists(@"localcord.txt"))
            {
                string s = File.ReadAllText(@"localcord.txt");
                List<string> ss = s.Split('/').ToList();
                latitude = Convert.ToDouble(ss[0]);
                longitude = Convert.ToDouble(ss[1]);
                zoom = Convert.ToDouble(ss[2]);
            }
        }

        private void Formalteralocaliza_Load(object sender, EventArgs e)
        {
            gMapControl1.MapProvider = GMapProviders.GoogleMap;
            gMapControl1.Position = new GMap.NET.PointLatLng(latitude,longitude);
            gMapControl1.Zoom = zoom;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (gMapControl1.Zoom >= 18 && gMapControl1.Zoom <= 20)
            {
                FileStream f = File.Create(@"localcord.txt");
                f.Close();
                using (var sw = new StreamWriter(@"localcord.txt"))
                {
                    sw.WriteLine($"{gMapControl1.Position.Lat}/{gMapControl1.Position.Lng}/{gMapControl1.Zoom}");
                }
                button1.Visible = false;
                button2.Visible = false;
                gMapControl1.MapProvider = GMapProviders.GoogleSatelliteMap;
                timer1.Enabled = true;
            }
            else
            {
                MessageBox.Show("o tamabho de área que você escolheu é invalido");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Hide();
            Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                File.Delete(@"imgmapa.png");
                timer1.Enabled = false;
                Bitmap b = new Bitmap(gMapControl1.Width, gMapControl1.Height);
                Graphics g = Graphics.FromImage(b);
                g.CopyFromScreen(this.Bounds.X, this.Bounds.Y, 0, 0, this.Bounds.Size);
                b.Save(@"imgmapa.png");
                Hide();
                Close();
            }
            catch (Exception)
            {}
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void gMapControl1_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            ff.posicionadrone(gMapControl1.Position.Lat, gMapControl1.Position.Lng, 10, 90);
            Hide();
            Close();
        }
    }
}
