using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testealturosyolo
{
    public class detectado
    {
        public Bitmap img { get; set; }
        public int frame { get; set; }
        public int quantpessoas { get; set; }
        public double confianca { get; set; }
    }
}
