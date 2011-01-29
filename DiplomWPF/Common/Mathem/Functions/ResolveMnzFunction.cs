using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiplomWPF.Common.Mathem.Functions
{
    public class ResolveMnzFunction : Function
    {
        private float beta;
        private float z;

        public ResolveMnzFunction(float beta, float z)
        {
            this.beta = beta;
            this.z = z;
        }

        public float resolve(float param)
        {
            return (float)(beta * Math.Exp(-beta * z) - Math.Cos(param * z));
        }
    }
}
