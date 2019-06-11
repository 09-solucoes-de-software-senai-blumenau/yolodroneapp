using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace smartvisionaiapp
{
    public partial class Itemdetectado : UserControl
    {
        Formexiberesult ff;
        objtelemetria o;
        int framed = 0;
        public Itemdetectado(detectado d, Formexiberesult f)
        {
            InitializeComponent();
            ff = f;
            label1.Text = $"pessoas: {d.quantpessoas}";
            int segundos = 0;
            int minutos = 0;
            framed = d.frame;
            if (d.frame > 60)
            {
                minutos = Convert.ToInt32(Math.Truncate(Convert.ToDouble(d.frame) / 60));
                segundos = d.frame - (minutos * 60);
            }
            else
            {
                segundos = d.frame;
            }
            label2.Text = $"tempo: {minutos}:{segundos}";
            o = Form1.rawtelemetrydata.FirstOrDefault(x => x.deltasegundos == framed);
            label3.Text = $"lat: {o.lat}";
            label4.Text = $"lng: {o.log}";
        }

       
        private void Itemdetectado_Load(object sender, EventArgs e)
        {

        }

        private void Itemdetectado_Click(object sender, EventArgs e)
        {
            ff.clicoudeteccao(framed-1);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            new FormMostralocal(o.lat, o.log).Show();
        }
    }
}
