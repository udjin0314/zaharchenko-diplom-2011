using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiplomWPF.Common.Mathem.Functions
{
    public class ResolveMkrFunction : Function
    {
        float P;
        float a;
        float r;
        float R;

        public ResolveMkrFunction(float P, float a, float R, float r)
        {
            this.P = P;
            this.a = a;
            this.R = R;
            this.r = r;
        }

        public float resolve(float param)
        {
            return (float)(P / (Math.PI * a * a) * Math.Exp(-(r * r / (a * a)))-MathHelper.bessel0(param*r/R));
        }
    }
}
