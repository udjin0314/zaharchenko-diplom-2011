using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiplomWPF.Common
{
    class Process
    {
        public Double[, ,] values { get; set; }
        public Double P { get; set; }
        public Double alphaR { get; set; }
        public Double alphaZ { get; set; }
        public Double R { get; set; }
        public Double L { get; set; }

        public Int32 T { get; set; }

        public Int32 N { get; set; }
        public Int32 I { get; set; }
        public Int32 J { get; set; }

        public Double ht { get; set; }
        public Double hz { get; set; }
        public Double hr { get; set; }

        public Double maxTemperature { get; set; }

        public Double minTemperature { get; set; }

        public Boolean isAnalitic { get; set; }

        public Double getPoint(Int32 i, Int32 j, Int32 n)
        {
            return values[i,j,n];
        }

    }
}
