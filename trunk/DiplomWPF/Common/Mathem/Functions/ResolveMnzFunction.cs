using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiplomWPF.Common.Mathem.Functions
{
    public class ResolveMnzFunction : Function
    {
        private double beta;
        private double z;

        public ResolveMnzFunction(double beta,double z)
        {
            this.beta = beta;
            this.z = z;
        }

        public double resolve(double param)
        {
            return beta * Math.Exp(-beta * z) - Math.Cos(param * z);
        }
    }
}
