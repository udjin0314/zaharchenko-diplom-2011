using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace DiplomWPF.Common
{
    class YavnSchema : AbstractProcess
    {
        public static String FILE_NAME = "log.txt";

        private float gamma0 = 0;
        private float gamma = 0;
        private float gammaR = 0;

        public YavnSchema(String name, Brush brush)
            : base(name, brush)
        {
        }

        public override void initialize(float P, float alphaR, float alphaZ, float R, float l, float K, float c, float beta, float T, Int32 N, Int32 I, Int32 J)
        {
            base.initialize(P, alphaR, alphaZ, R, l, K, c, beta, T, N, I, J);
            gammaR = K * ht / (c * hr * hr);
            gamma = 1 - (2 * gammaR) - (2 * alphaZ * ht / (l * c));
            gamma0 = 1 - (4 * gammaR) - (2 * alphaZ * ht / (l * c));
        }

        public override void initializeParams(float P, float alphaR, float alphaZ, float R, float l, float K, float c, float beta, float T)
        {
            base.initializeParams(P, alphaR, alphaZ, R, l, K, c, beta, T);
            gammaR = K * ht / (c * hr * hr);
            gamma = 1 - (2 * gammaR) - (2 * alphaZ * ht / (l * c));
            gamma0 = 1 - (4 * gammaR) - (2 * alphaZ * ht / (l * c));
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

        float functionG(int i)
        {
            float r = i * hr;
            float res = (float)(P * beta * ht / (Math.PI * a * a * c) * Math.Exp(-(r * r / (a * a))));
            return res;
        }

        public void executeAlg()
        {
            float[] ilayer = new float[I + 1];
            for (int n = 0; n <= N - 1; n++)
            {
                ilayer[0] = values[1, 0, n] * 4 * gammaR + gamma0 * values[0, 0, n] + functionG(0);
                for (int i = 1; i < I; i++)
                {
                    ilayer[i] = values[i + 1, 0, n] * gammaR * (1 + (float)1 / (2 * i)) + values[i, 0, n] * gamma + values[i - 1, 0, n] * gammaR * (1 - (float)1 / (2 * i)) + functionG(i);
                }
                ilayer[I] = values[I - 1, 0, n] * gammaR + values[I, 0, n] * (gamma - gammaR * 2 * hr * alphaZ / K * (1 + (float)1 / (2 * I)));
                for (int i = 0; i <= I; i++)
                    for (int j = 0; j <= J; j++)
                    {
                        values[i, j, n + 1] = ilayer[i];
                        setPoint(i * hr, j * hz, (n + 1) * ht, ilayer[i]);
                        if (ilayer[i] > maxTemperature)
                            maxTemperature = ilayer[i];
                        if (ilayer[i] < minTemperature)
                            minTemperature = ilayer[i];
                    }
                if (handler != null) handler();
            }
        }

    }
}
