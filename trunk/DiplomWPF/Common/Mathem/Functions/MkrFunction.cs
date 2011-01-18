using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiplomWPF.Common.Mathem.Functions
{
    public class MkrFunction : Function
    {
        double alphar;
        double K;
        double R;

        public MkrFunction(double alphar, double K, double R)
        {
            this.alphar = alphar;
            this.K = K;
            this.R = R;
        }

        public double resolve(double param)
        {
            return alphar * R / K * MathHelper.bessel0(param) - param * MathHelper.bessel1(param);
        }
    }
}
