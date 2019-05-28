﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace testealturosyolo
{
    public partial class Itemdetectado : UserControl
    {
        Formexiberesult ff;
        public Itemdetectado(detectado d, Formexiberesult f)
        {
            InitializeComponent();
            ff = f;
            label1.Text = $"pessoas: {d.quantpessoas}";
            int segundos = 0;
            int minutos = 0;
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
        }

        private void Itemdetectado_Load(object sender, EventArgs e)
        {

        }
    }
}
