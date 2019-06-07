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

namespace testealturosyolo
{
    public partial class FormMostralocal : Form
    {
        double latitude;
        double longitude;
        Graphics g;
        public FormMostralocal(double lat,double lng)
        {
            InitializeComponent();
            latitude = lat;
            longitude = lng;
        }

        private void FormMostralocal_Load(object sender, EventArgs e)
        {
            gMapControl1.MapProvider = GMapProviders.GoogleSatelliteMap;
            gMapControl1.Position = new GMap.NET.PointLatLng(latitude, longitude);
            gMapControl1.Zoom = 18;
            PointLatLng p = new PointLatLng(latitude, longitude);
            GMapMarker marker = new GMarkerGoogle(p, new Bitmap(testealturosyolo.Properties.Resources.pessoamapa,30,60));
            GMapOverlay markers = new GMapOverlay("markers");
            markers.Markers.Add(marker);
            gMapControl1.Overlays.Add(markers);
            g = gMapControl1.CreateGraphics();
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            g.FillRectangle(Brushes.Transparent, 0, 0, 200, 200);
            g.DrawImage(testealturosyolo.Properties.Resources.LogoProjetoSmartVisionAI, 0, 0, 140, 130);
        }
    }
}
