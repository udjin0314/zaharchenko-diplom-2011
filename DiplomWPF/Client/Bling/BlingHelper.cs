using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bling.Graphics;
using Bling.Core;

namespace DiplomWPF.Client.Bling
{
    class BlingHelper
    {
        public static readonly PSurface ProcessSurface = new PSurface((PointBl p) =>
        {
            PointBl sin = p.SinU;
            PointBl cos = p.CosU;
            return new Point3DBl(sin[0] * cos[1], sin[0] * sin[1], cos[0]);
        });
    }
}
