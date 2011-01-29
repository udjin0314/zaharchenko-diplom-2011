using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiplomWPF.Common.Comparators
{
    public class ComparatorData
    {
        public Int32 exp { get; set; }
        public float value { get; set; }
        public float compValue { get; set; }
        public float diff { get; set; }
        public float hr { get; set; }
        public float hz { get; set; }
        public float ht { get; set; }
        public float r { get; set; }
        public float z { get; set; }
        public float t { get; set; }

        public ComparatorData(Int32 exp, float value, float compValue, float diff, float hr, float hz, float ht, float r, float z, float t)
        {
            this.exp = exp;
            this.value = value;
            this.compValue = compValue;
            this.diff = diff;
            this.hr = hr;
            this.hz = hz;
            this.ht = ht;
            this.r = r;
            this.z = z;
            this.t = t;
        }

    }
}
