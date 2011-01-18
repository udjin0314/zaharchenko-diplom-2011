using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using DiplomWPF.Common.Mathem.Functions;
using DiplomWPF.Common.Mathem;
using DiplomWPF.Common.Helpers;

namespace DiplomWPF.Common.Schemas
{
    public class FullAnalitSchema : AbstractProcess
    {
        public static String LOG_FILE = "log.txt";

        private int k = 0;
        private int n = 0;

        protected List<double> mr = new List<double>();
        protected List<double> mz = new List<double>();

        protected String mrFile = "mr";
        protected String mzFile = "mz";

        protected Int32 listN = 2;

        protected Int32 findN = 2;

        public FullAnalitSchema(String name, Brush brush)
            : base(name, brush)
        {
        }

        public override void initialize(float P, float alphaR, float alphaZ, float R, float l, float K, float c, float beta, float T, Int32 N, Int32 I, Int32 J)
        {
            base.initialize(P, alphaR, alphaZ, R, l, K, c, beta, T, N, I, J);
            progressBarMax = N * J * I;
        }

        public override void initializeParams(float P, float alphaR, float alphaZ, float R, float l, float K, float c, float beta, float T)
        {
            base.initializeParams(P, alphaR, alphaZ, R, l, K, c, beta, T);
            progressBarMax = N * J * I;
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




        protected double resolveLkn(int k, int n)
        {
            return -(mr[k] * mr[k] / (R * R) + mz[n] * mz[n]);
        }

        protected double resolveDkn(int k, int n)
        {
            return resolveDjk(k) * resolveDcn(n);
        }

        protected double resolveDjk(int k)
        {
            return a * a / (R * R * Math.Pow(MathHelper.bessel0(mr[k]), 2)) * Math.Exp(-Math.Pow(a * mr[k] / (2 * R), 2));
        }

        protected double resolveDcn(int n)
        {
            /*if (n == 0) return (1 - Math.Exp(-beta * l));
            else */
            return 4 * beta * mz[n] * Math.Exp(-beta * l) * (mz[n] * Math.Sin(mz[n] * l) - beta * Math.Cos(mz[n] * l) + beta * Math.Exp(beta * l)) / ((2 * l * mz[n] + Math.Sin(2 * mz[n] * l)) * (beta * beta + mz[n] * mz[n]));
        }

        protected double functionCkn(double t, int k, int n)
        {
            double dkn = resolveDkn(k, n);
            double lkn = resolveLkn(k, n);
            return dkn / (lkn * K) * (Math.Exp(K * lkn * t / c) - 1);
        }

        protected double functionVkn(double r, double z, int k, int n)
        {
            /* if (n == 0) return MathHelper.bessel0(mr[k] * r / R);
             else */
            return MathHelper.bessel0(mr[k] * r / R) * (K * mz[n] / alphaZ * Math.Cos(mz[n] * z) + Math.Sin(mz[n] * z));
        }


        public void preapareMr()
        {
            if (System.IO.File.Exists(mrFile))
            {
                CommonHelper.readAllLines(mrFile, mr);
                if (mr.Count >= listN) return;
            }

            Function mrf = new MkrFunction(alphaR, K, R);
            mr = CommonHelper.findM(mrFile, findN, mrf, 1e-5);

        }



        public void preapareMz()
        {
            if (System.IO.File.Exists(mzFile))
            {
                CommonHelper.readAllLines(mzFile, mz);
                if (mz.Count >= listN) return;
            }
            Function mnf = new MnzFunction(l, alphaZ, K);
            mz = CommonHelper.findM(mzFile, findN, mnf, 1e-5/*,1*/);


        }


        public virtual void init()
        {
            mrFile = mrFile + "_aR" + alphaR + "K" + K + "R" + R + ".txt";
            mzFile = mzFile + "_aZ" + alphaZ + "K" + K + "l" + l + ".txt";
            preapareMr();
            preapareMz();
        }

        public virtual float findU(double t, double r, double z)
        {
            float res = 0;
            for (int k1 = 0; k1 < listN; k1++)
                for (int n1 = 0; n1 < listN; n1++)
                {
                    res = res + (float)(functionCkn(n * ht, k1, n1) * functionVkn(r, z, k1, n1));
                }
            return res;
        }

        public virtual void executeAlg()
        {
            init();
            for (int n = 0; n <= N - 1; n++)
            {
                for (int j = 0; j <= J; j++)
                {
                    for (int i = 0; i <= I; i++)
                    {
                        float res = findU(n * ht, i * hr, j * hz);
                        values[i, j, n + 1] = res;
                        if (res > maxTemperature) maxTemperature = res;
                        if (res < minTemperature) minTemperature = res;
                        handler();
                    }
                }
            }
        }

    }

}
