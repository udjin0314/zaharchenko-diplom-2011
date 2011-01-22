using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiplomWPF.Client.UI;
using Microsoft.Research.DynamicDataDisplay;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using DiplomWPF.Common.Helpers;
using DiplomWPF.Common.Mathem.Functions;

namespace DiplomWPF.Common
{
    public abstract class AbstractProcess : ICloneable
    {
        public String processName { get; set; }

        public String additionalName { get; set; }

        public Brush brush { get; set; }
        //   public float[, ,] values { get; set; }
        public ProcessValues values;
        public float P { get; set; }
        public float alphaR { get; set; }
        public float alphaZ { get; set; }
        public float R { get; set; }
        public float l { get; set; }

        public Int32 Npv { get; set; }
        public Int32 Ipv { get; set; }
        public Int32 Jpv { get; set; }

        public Boolean isExecuted { get; set; }
        public Boolean isForTest { get; set; }

        protected double mz;
        protected double mr;

        protected double eps { get; set; }

        public float a { get; set; }
        public float K { get; set; }

        public float c { get; set; }

        public float beta { get; set; }

        public float T { get; set; }

        public Int32 N { get; set; }
        public Int32 I { get; set; }
        public Int32 J { get; set; }
        public Int32 progressBarMax { get; set; }

        public float ht { get; set; }
        public float hz { get; set; }
        public float hr { get; set; }

        public float maxTemperature { get; set; }

        public float minTemperature { get; set; }

        protected List<double> mrArr = new List<double>();
        protected List<double> mzArr = new List<double>();

        public DiplomWPF.ProcessControl.increaseProgressBar handler;

        //graphics elements

        protected Int32 findN = 2;

        public Chart2D chartUZ { get; set; }
        public Chart2D chartUR { get; set; }
        public Chart2D chartUTime { get; set; }

        public Stopwatch swInit { get; set; }
        public Stopwatch swCompute { get; set; }

        protected String mrFile = "mr";
        protected String mzFile = "mz";

        public AbstractProcess(String processName, Brush brush)
        {
            this.processName = processName;
            this.brush = brush;
            this.isExecuted = false;
            this.Npv = 100;
            this.Ipv = 100;
            this.Jpv = 100;
            swInit = new Stopwatch();
            isForTest = false;
            eps = 1e-5;
            swCompute = new Stopwatch();
        }

        public void reDrawNewProcess()
        {
            chartUZ.reDrawNewProcess(this);
            chartUR.reDrawNewProcess(this);
            chartUTime.reDrawNewProcess(this);

        }

        public virtual void init()
        {
            if (isForTest)
            {
                mrFile = mrFile + "_aR" + alphaR + "K" + K + "R" + R + ".txt";
                mzFile = mzFile + "_aZ" + alphaZ + "K" + K + "l" + l + ".txt";
                preapareMr();
                preapareMz();
            }
        }

        public void preapareMr()
        {
            if (System.IO.File.Exists(mrFile))
            {
                CommonHelper.readAllLines(mrFile, mrArr);
                if (mrArr.Count >= findN)
                {
                    mr = mrArr[1];
                    return;
                }
            }

            Function mrf = new MkrFunction(alphaR, K, R);
            mrArr = CommonHelper.findM(mrFile, findN, mrf, eps);
            mr = mrArr[1];
        }



        public void preapareMz()
        {
            if (System.IO.File.Exists(mzFile))
            {
                CommonHelper.readAllLines(mzFile, mzArr);
                if (mzArr.Count >= findN)
                {
                    mz = mzArr[1];
                    return;
                }
            }
            Function mnf = new MnzFunction(l, alphaZ, K);
            mzArr = CommonHelper.findM(mzFile, findN, mnf, eps);
            mz = mzArr[1];

        }


        public void initializeGraphics(ChartPlotter chartUZPlotter, ChartPlotter chartURPlotter, ChartPlotter chartUTimePlotter)
        {
            chartUZ = new Chart2D(chartUZPlotter, 1);
            chartUR = new Chart2D(chartURPlotter, 0);
            chartUTime = new Chart2D(chartUTimePlotter, 2);
        }

        public virtual void initialize(float P, float alphaR, float alphaZ, float R, float l, float K, float c, float beta, float T, Int32 N, Int32 I, Int32 J)
        {
            this.P = P;
            this.alphaR = alphaR;
            this.alphaZ = alphaZ;
            this.R = R;
            this.l = l;
            this.K = K;
            this.c = c;
            this.beta = beta;
            this.T = T;
            this.N = N;
            this.I = I;
            this.J = J;
            this.maxTemperature = 0;
            this.minTemperature = Int32.MaxValue;
            this.a = 0.15F * R;
            this.hr = R / I;
            this.hz = l / J;
            this.ht = T / N;
            progressBarMax = N;
        }

        public virtual void initializeParams(float P, float alphaR, float alphaZ, float R, float l, float K, float c, float beta, float T)
        {
            this.P = P;
            this.alphaR = alphaR;
            this.alphaZ = alphaZ;
            this.R = R;
            this.l = l;
            this.K = K;
            this.c = c;
            this.beta = beta;
            this.T = T;
            this.maxTemperature = 0;
            this.minTemperature = Int32.MaxValue;
            this.a = 0.15F * R;
            this.hr = R / I;
            this.hz = l / J;
            this.ht = T / N;
            progressBarMax = N;
        }

        public virtual void initializeSchema(Int32 I, Int32 J, Int32 N)
        {
            this.N = N;
            this.I = I;
            this.J = J;
            this.isExecuted = false;
            this.hr = R / I;
            this.hz = l / J;
            this.ht = T / N;
            progressBarMax = N;

        }

        protected float functionG(float r, float z)
        {
            if (!isForTest)
            {
                float res = (float)(P * beta / (Math.PI * a * a) * Math.Exp(-(beta * z + (r * r / (a * a)))));
                return res;
            }
            else
            {
                double result = 1;
                result = result * Mathem.MathHelper.bessel0(mr * r / R);
                result = result * (Math.Cos(mz * z) + alphaZ / (K * mz) * Math.Sin(mz * z));
                return (float)result;
            }
        }

        public virtual void executeProcess()
        {
            initValues();
        }

        public virtual void executeProcess(object parameters)
        {
            initValues();
            handler = (DiplomWPF.ProcessControl.increaseProgressBar)parameters;
        }

        public void initValues()
        {
            values = new ProcessValues(I, J, N);
            init();
        }

        public virtual void delete()
        {
            chartUR.delete();
            chartUZ.delete();
            chartUTime.delete();
            chartUR = null;
            chartUZ = null;
            chartUTime = null;
        }

        public float getPoint(float r, float z, float t)
        {
            int i = (int)Math.Round(r * I / R);
            int j = (int)Math.Round(z * J / l);
            int n = (int)Math.Round(t * N / T);
            return values[i, j, n];
        }

        public void setPoint(float r, float z, float t, float value)
        {
            int i = (int)Math.Round(r * I / R);
            int j = (int)Math.Round(z * J / l);
            int n = (int)Math.Round(t * N / T);
            values[i, j, n] = value;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
