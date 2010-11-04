using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiplomWPF
{
    class ProcessValue
    {
        public Double r { get; set; }
        public Double z { get; set; }
        public Double t { get; set; }
        public Double u { get; set; }

        public ProcessValue(Double r, Double z, Double t)
        {
            this.r = r;
            this.z = z;
            this.t = t;
        }

        public ProcessValue(Double r, Double z, Double t, Double u)
        {
            this.r = r;
            this.z = z;
            this.t = t;
            this.u = u;
        }
    }


}
