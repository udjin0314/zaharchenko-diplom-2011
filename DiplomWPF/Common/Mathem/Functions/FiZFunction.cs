using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiplomWPF.Common.Mathem.Functions
{
    class FiZFunction:Function
    {
        float beta;

        public FiZFunction(float beta)
        {
            this.beta = beta;
        }

        public float resolve(float param)
        {
            return (float)(beta * Math.Exp(-beta * param)); 
        }
    }
    
}
