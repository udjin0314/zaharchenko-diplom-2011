using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiplomWPF.Common.Mathem.Functions;

namespace DiplomWPF.Common.Mathem
{
    public class Dihotomy
    {
        private float epsilon = 1e-8f;
        private Function func;

        public Dihotomy(Function func)
        {
            this.func = func;
        }
        public Dihotomy(Function func, float epsilon)
        {
            this.func = func;
            this.epsilon = epsilon;
        }

        public double resolve(float a, float b)
        {
            float c;
            //if (func.resolve(a) * func.resolve(b) > 0) 
            while (b - a > epsilon)
            {
                c = (a + b) / 2;
                if (func.resolve(b) * func.resolve(c) < 0)
                    a = c;
                else
                    b = c;
            }
            return (a + b) / 2;
        }
    }
}
