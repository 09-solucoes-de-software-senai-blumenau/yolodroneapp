using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace smartvisionaiapp
{
    public partial class FormMostralocal : Form
    {
        double latitude;
        double longitude;
        Graphics g;
        public FormMostralocal(double lat,double lng)
        {
            InitializeComponent();
            // codigo para movimentar tela com style none
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            //----------------
            latitude = lat;
            longitude = lng;
        }

        //codigo pra movimentar tela
        private const int cGrip = 16;
        private const int cCaption = 64; // o tamanho da area que o usuario pode segurar no topo

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x84)
            {
                Point pos = new Point(m.LParam.ToInt32());
                pos = this.PointToClient(pos);
                if (pos.Y < cCaption)
                {
                    m.Result = (IntPtr)2;
                    return;
                }
                if (pos.X >= this.ClientSize.Width - cGrip && pos.Y >= this.ClientSize.Height - cGrip)
                {
                    m.Result = (IntPtr)17;
                    return;
                }
            }
            base.WndProc(ref m);
        }
        //-----------------

        private void FormMostralocal_Load(object sender, EventArgs e)
        {
            gMapControl1.MapProvider = GMapProviders.GoogleSatelliteMap;
            gMapControl1.Position = new GMap.NET.PointLatLng(latitude, longitude);
            gMapControl1.Zoom = 18;
            PointLatLng p = new PointLatLng(latitude, longitude);
            GMapMarker marker = new GMarkerGoogle(p, new Bitmap(smartvisionaiapp.Properties.Resources.pessoamapa,30,60));
            GMapOverlay markers = new GMapOverlay("markers");
            markers.Markers.Add(marker);
            gMapControl1.Overlays.Add(markers);
            g = gMapControl1.CreateGraphics();
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            g.FillRectangle(Brushes.Transparent, 0, 0, 200, 200);
            g.DrawImage(smartvisionaiapp.Properties.Resources.LogoProjetoSmartVisionAI, 0, 0, 140, 130);
        }

        private void gMapControl1_Load(object sender, EventArgs e)
        {

        }

        private void gMapControl1_Load_1(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Hide();
            Close();

        }

        private void button6_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }
        bool maximizado = false;
        private void button4_Click(object sender, EventArgs e)
        {
            if (maximizado)
            {
                button4.Image = smartvisionaiapp.Properties.Resources.btnmaximizar;
                maximizado = false;
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                button4.Image = smartvisionaiapp.Properties.Resources.btndesmaximizar;
                maximizado = true;
                this.WindowState = FormWindowState.Maximized;
            }
        }

        private void verminmax_Tick(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                button4.Image = smartvisionaiapp.Properties.Resources.btndesmaximizar;
            }
            else
            {
                button4.Image = smartvisionaiapp.Properties.Resources.btnmaximizar;
            }
        }
    }
}
