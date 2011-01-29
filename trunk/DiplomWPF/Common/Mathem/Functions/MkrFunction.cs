using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiplomWPF.Common.Mathem.Functions
{
    public class MkrFunction : Function
    {
        float alphar;
        float K;
        float R;

        public MkrFunction(float alphar, float K, float R)
        {
            this.alphar = alphar;
            this.K = K;
            this.R = R;
        }

        public float resolve(float param)
        {
            return (float)(alphar * MathHelper.bessel0(param) / K - param * MathHelper.bessel1(param));
        }
    }
}
