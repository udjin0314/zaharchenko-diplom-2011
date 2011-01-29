using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiplomWPF.Common.Mathem.Functions
{
    class VnzFunction: Function
    {
        private float mnz;

        public VnzFunction(float mnz)
        {
            this.mnz = mnz;
        }

        public float resolve(float param)
        {
            return (float)(Math.Cos(param * mnz)); 
        }
    }
    
}
