using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiplomWPF.Common.Mathem.Functions
{
    public class MnzFunction : Function
    {
        private double l;
        private double alphaz;
        private double K;

        public MnzFunction(double l,double alphaz,double K)
        {
            this.l = l;
            this.alphaz = alphaz;
            this.K = K;
        }
        
        public double resolve(double param)
        {
            return Math.Cos(param * l) * 2 * alphaz / K * param - Math.Sin(param * l) * (param * param - alphaz / K * alphaz / K);
        }
    }
}
