using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiplomWPF.Common.Mathem
{
    public static class MathHelper
    {
        public static Double bessel0(Double x)
        {
            Double result = 0;
            if (x >= -3 && x <= 3)
            {
                result = (double)1 - 2.2499997 * Math.Pow(x / 3, 2) + 1.2656208 * Math.Pow(x / 3, 4)
                    - 0.3163866 * Math.Pow(x / 3, 6) + 0.0444479 * Math.Pow(x / 3, 8) - 0.0039444 * Math.Pow(x / 3, 10)
                    + 0.00021 * Math.Pow(x / 3, 12);
            }
            else
            {
                Double f0 = 0.79788456 - 0.00000077 * 3 / x - 0.0055274 * Math.Pow(3 / x, 2) - 0.00009512 * Math.Pow(3 / x, 3)
                    + 0.00137237 * Math.Pow(3 / x, 4) - 0.00072805 * Math.Pow(3 / x, 5) + 0.00014476 * Math.Pow(3 / x, 6);
                Double Q0 = x - 0.78539816 - 0.04166397 * 3 / x - 0.00003954 * Math.Pow(3 / x, 2)
                    + 0.00262573 * Math.Pow(3 / x, 3) - 0.00054125 * Math.Pow(3 / x, 4) - 0.00029333 * Math.Pow(3 / x, 5)
                        + 0.00013558 * Math.Pow(3 / x, 6);
                result = Math.Pow(x, -(double)1 / 2) * f0 * Math.Cos(Q0);
            }
            return result;
        }

        public static Double bessel1(Double x)
        {
            Double result = 0;
            if (x >= -3 && x <= 3)
            {
                result = (double)1/2 - 0.56249985 * Math.Pow(x / 3, 2) + 0.21093573 * Math.Pow(x / 3, 4)
                    - 0.03954289 * Math.Pow(x / 3, 6) + 0.00443319 * Math.Pow(x / 3, 8) - 0.00031761 * Math.Pow(x / 3, 10)
                    + 0.00001109 * Math.Pow(x / 3, 12);
            }
            else
            {
                Double f1 = 0.79788456 + 0.00000156 * 3 / x + 0.01659667 * Math.Pow(3 / x, 2) + 0.00017105 * Math.Pow(3 / x, 3)
                    - 0.00249511 * Math.Pow(3 / x, 4) + 0.00113653 * Math.Pow(3 / x, 5) - 0.00020033 * Math.Pow(3 / x, 6);
                Double Q1 = x - 2.35619449 + 0.12499612 * 3 / x + 0.00005650 * Math.Pow(3 / x, 2)
                    - 0.00637879 * Math.Pow(3 / x, 3) + 0.00074348 * Math.Pow(3 / x, 4) + 0.00079824 * Math.Pow(3 / x, 5)
                        - 0.00029166 * Math.Pow(3 / x, 6);
                result = Math.Pow(x, -(double)1 / 2) * f1 * Math.Cos(Q1);
            }
            return result;
        }
    }
}
