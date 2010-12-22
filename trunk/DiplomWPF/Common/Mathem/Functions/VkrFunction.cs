using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiplomWPF.Common.Mathem.Functions
{
    class VkrFunction: Function
    {
        private double mkr;
        private double R;

        public VkrFunction(double R, double mkr)
        {
            this.mkr = mkr;
            this.R = R;
        }
        
        public double resolve(double param)
        {
            return MathHelper.bessel0(mkr/R*param);
        }
    }
}
