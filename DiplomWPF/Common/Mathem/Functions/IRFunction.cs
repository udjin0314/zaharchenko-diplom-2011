using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiplomWPF.Common.Mathem.Functions
{
    class IRFunction:Function
    {
        float P;
        float a;

        public IRFunction(float P, float a)
        {
            this.P = P;
            this.a = a;
        }

        public float resolve(float param)
        {
            return (float)(P / (Math.PI * a * a) * Math.Exp(-(param * param / (a * a))));
        }
    }
    
}
