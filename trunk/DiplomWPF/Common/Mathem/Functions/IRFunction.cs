using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiplomWPF.Common.Mathem.Functions
{
    class IRFunction:Function
    {
        double P;
        double a;

        public IRFunction(double P, double a)
        {
            this.P = P;
            this.a = a;
        }
        
        public double resolve(double param)
        {
            return P / (Math.PI * a * a) * Math.Exp(-(param * param / (a * a)));
        }
    }
    
}
