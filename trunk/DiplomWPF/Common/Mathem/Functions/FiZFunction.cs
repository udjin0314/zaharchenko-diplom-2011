using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiplomWPF.Common.Mathem.Functions
{
    class FiZFunction:Function
    {
        double beta;

        public FiZFunction(double beta)
        {
            this.beta = beta;
        }
        
        public double resolve(double param)
        {
            return beta * Math.Exp(-beta * param); 
        }
    }
    
}
