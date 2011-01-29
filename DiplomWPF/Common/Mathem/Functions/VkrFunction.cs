using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiplomWPF.Common.Mathem.Functions
{
    class VkrFunction: Function
    {
        private float mkr;
        private float R;

        public VkrFunction(float R, float mkr)
        {
            this.mkr = mkr;
            this.R = R;
        }

        public float resolve(float param)
        {
            return (float)MathHelper.bessel0(mkr / R * param);
        }
    }
}
