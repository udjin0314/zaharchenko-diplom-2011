using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiplomWPF.Common.Mathem.Functions
{
    public class MkrFunction:Function
    {
        double alphar;
        double K;
        
        public double resolve(double param)
        {
            return MathHelper.bessel1(param) + alphar / K * MathHelper.bessel0(param); 
        }
    }
}
