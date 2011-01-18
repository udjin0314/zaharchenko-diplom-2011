using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiplomWPF.Common.Mathem.Functions
{
    class VnzFunction: Function
    {
        private double mnz;

        public VnzFunction(double mnz)
        {
            this.mnz = mnz;
        }
        
        public double resolve(double param)
        {
            return Math.Cos(param * mnz); 
        }
    }
    
}
