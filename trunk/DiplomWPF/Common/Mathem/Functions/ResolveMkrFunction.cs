using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiplomWPF.Common.Mathem.Functions
{
    public class ResolveMkrFunction : Function
    {
        double P;
        double a;
        double r;
        double R;

        public ResolveMkrFunction(double P,double a,double R,double r)
        {
            this.P = P;
            this.a = a;
            this.R = R;
            this.r = r;
        }
        
        public double resolve(double param)
        {
            return P / (Math.PI * a * a) * Math.Exp(-(r * r / (a * a)))-MathHelper.bessel0(param*r/R);
        }
    }
}
