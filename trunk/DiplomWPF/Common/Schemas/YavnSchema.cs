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
            initializeParams(P, alphaR, alphaZ, R, l, K, c, beta, T);
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
            swInit.Start();
            base.executeProcess();
            swInit.Stop(); swCompute.Start();
            executeAlg();
            swCompute.Stop();
            isExecuted = true;
        }

        public override void executeProcess(object parameters)
        {
            swInit.Start();
            base.executeProcess(parameters);
            swInit.Stop(); swCompute.Start();
            executeAlg();
            swCompute.Stop();
            isExecuted = true;
        }

        public float functionG(int i)
        {
            float r = i * hr;
            if (isForTest)
            {
                double result = 1;
                result = result * Mathem.MathHelper.bessel0(mr * r / R);
                result = result * 1 / (K*l*mz * mz) * (-alphaZ  * Math.Cos(mz * l) + alphaZ  + K*mz * Math.Sin(mz * l));
                return (float)result;
            }
            else
            {
                return (float)(P * beta  / (Math.PI * a * a) * Math.Exp(-(r * r / (a * a))));
            }
        }

        public Boolean isStable()
        {
            return 1 - 4 * K * ht / (c * hr * hr) - 2 * alphaZ * ht / (l * c) > 0;
        }

        public static Boolean isStable(double K, double c, double alphaZ, double l, double T, int N, double R, int I)
        {
            double gammaR = K * T * I * I / (N * c * R * R);
            double gamma = 1 - (2 * gammaR) - (2 * alphaZ * T / (N * l * c));
            double rty = gamma - gammaR * 2 * R * alphaZ / (I * K) * (1 + (float)1 / (2 * I));
            return rty > 0;
        }

        public override int getNextI(double koef)
        {
            double hr2 = hr / koef;
            Int32 I2 = (int)Math.Round(R / hr2);
            return I2;
        }

        public override int getNextN(double koef)
        {
            double ht2 = ht / koef;
            Int32 N2 = (int)Math.Round(T / ht2);
            return N2;
        }

        public override int getPrevI(double koef)
        {
            double hr2 =koef * hr;
            Int32 I2 = (int)Math.Round(R / hr2);
            return I2;
        }

        public override int getPrevN(double koef)
        {
            double ht2 = koef * ht;
            Int32 N2 = (int)Math.Round(T / ht2);
            return N2;
        }

        public void executeAlg()
        {
            for (int n = 0; n <= N - 1; n++)
            {
                values[0, 0, n + 1] = values[1, 0, n] * 4 * gammaR + gamma0 * values[0, 0, n] + ht / c * functionG(0);
                for (int i = 1; i < I; i++)
                {
                    values[i, 0, n + 1] = values[i + 1, 0, n] * gammaR * (1 + (float)1 / (2 * i)) + values[i, 0, n] * gamma + values[i - 1, 0, n] * gammaR * (1 - (float)1 / (2 * i)) + ht / c * functionG(i);
                }
                values[I, 0, n + 1] = values[I - 1, 0, n] *2* gammaR + values[I, 0, n] * (gamma - 2 * ht * alphaR / (hr*c) * (1 + (float)1 / (2 * I))) + ht / c * functionG(I);
                for (int i = 0; i <= I; i++)
                    for (int j = 0; j <= J; j++)
                    {
                        float val = values[i, 0, n + 1];
                        values[i, j, n + 1] = val;
                        setPoint(i * hr, j * hz, (n + 1) * ht, val);
                        if (val > maxTemperature)
                            maxTemperature = val;
                        if (val < minTemperature)
                            minTemperature = val;
                    }
                if (handler != null) handler();
            }
        }

    }
}
