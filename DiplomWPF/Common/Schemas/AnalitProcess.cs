using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using DiplomWPF.Common.Mathem.Functions;
using DiplomWPF.Common.Mathem;
using DiplomWPF.Common.Helpers;
using DiplomWPF.Common.Schemas;

namespace DiplomWPF.Common
{

    public class AnalitProcess : FullAnalitSchema
    {


        private int nQ;
        private int kQ;

        public AnalitProcess(String name, Brush brush)
            : base(name, brush)
        {
        }



        private void resolveM(double r, double z)
        {
            double eps = 1e-5;

            Function fiz = new FiZFunction(beta);
            Function ir = new IRFunction(P, a);
            Boolean flag = false;
            double res = fiz.resolve(z) * fiz.resolve(r);
            for (int k = 0; k < findN; k++)
            {
                Function vkr = new VkrFunction(R, mr[k]);
                double vkres = vkr.resolve(r);
                for (int n = 0; n < findN; n++)
                {
                    Function vnz = new VnzFunction(mz[n]);
                    double vnres = vnz.resolve(z);
                    if (Math.Abs(res - beta * vkres * vnres) < eps)
                    {
                        nQ = n;
                        kQ = k;
                        flag = true;
                        //System.IO.File.AppendAllText(LOG_FILE, "we find it r=" + r + "; z=" + z +"; k=" + kQ + "; n=" + nQ + "; " + Math.Abs(res - beta * vkres * vnres) + "<" + eps + "; res=" + res + "; mrk=" + mr[kQ] + "; mzn=" + mz[nQ] + '\n');
                    }
                    if (flag) break;
                }
                if (flag) break;
            }
            if (!flag) System.IO.File.AppendAllText(LOG_FILE, "UNABLE r=" + r + "; z=" + z + "; with eps" + eps + "; res=" + res + '\n');

        }

        public override float findU(double t, double r, double z)
        {
            float res = 0;

            for (int n1 = 0; n1 < listN; n1++)
            {
                res = res + (float)(functionCkn(t, kQ, n1) * functionVkn(r, z, kQ, n1));
            }
            return res;
        }

        public override void executeAlg()
        {
            init();
            System.IO.File.WriteAllText(LOG_FILE, "");
            for (int i = 0; i <= I; i++)
            {
                for (int j = 0; j <= J; j++)
                {
                    resolveM(i * hr, j * hz);
                    for (int n = 1; n <= N; n++)
                    {
                        float res = findU(n * ht, i * hr, j * hz);
                        values[i, j, n] = res;
                        if (res > maxTemperature) maxTemperature = res;
                        if (res < minTemperature) minTemperature = res;
                        handler();
                    }
                }
            }
        }

    }
}
