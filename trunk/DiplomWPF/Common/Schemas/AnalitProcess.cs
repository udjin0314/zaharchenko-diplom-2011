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

    public class AnalitProcess : AbstractProcess
    {
        public AnalitProcess(String name, Brush brush)
            : base(name, brush)
        {
            isForTest = true;
        }

        public override void executeProcess()
        {
            //execute();
            swInit.Start();
            base.executeProcess();
            swInit.Stop(); swCompute.Start();
            executeAlg();
            swCompute.Stop();
            isExecuted = true;

        }

        public override void executeProcess(object parameters)
        {
            //execute();
            swInit.Start();
            base.executeProcess(parameters);
            swInit.Stop(); swCompute.Start();
            executeAlg();
            swCompute.Stop();
            isExecuted = true;


        }

        public float findU(double t, double r, double z)
        {
            float a1 = 1;
            double result = 1;
            result = a1 * a1 * R * R / (K * (mr * mr + R * R * mz * mz));
            result = result * (1 - Math.Exp(-K * (mr * mr + R * R * mz * mz) * t / (c * R * R)));
            result = result * functionG((float)r, (float)z);
            return (float)result;
        }

        public void executeAlg()
        {
            //init();
            for (int i = 0; i <= I; i++)
            {
                for (int j = 0; j <= J; j++)
                {
                    for (int n = 1; n <= N; n++)
                    {
                        float res = findU(n * ht, i * hr, j * hz);
                        setPoint(i * hr, j * hz, n * ht, res);
                        //values[i, j, n] = res;
                        if (res > maxTemperature) maxTemperature = res;
                        if (res < minTemperature) minTemperature = res;
                        handler();
                    }
                }
            }
        }

    }
}
