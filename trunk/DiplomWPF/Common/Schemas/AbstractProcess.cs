﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiplomWPF.Client.UI;
using Microsoft.Research.DynamicDataDisplay;
using System.Windows.Controls;
using System.Windows.Media;

namespace DiplomWPF.Common
{
    public abstract class AbstractProcess : ICloneable
    {
        public String processName { get; set; }
        public Brush brush { get; set; }
        //   public float[, ,] values { get; set; }
        public ProcessValues values;
        public float P { get; set; }
        public float alphaR { get; set; }
        public float alphaZ { get; set; }
        public float R { get; set; }
        public float l { get; set; }

        public Boolean isExecuted { get; set; }

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

        public DiplomWPF.ProcessControl.increaseProgressBar handler;

        //graphics elements

        public Chart2D chartUZ { get; set; }
        public Chart2D chartUR { get; set; }
        public Chart2D chartUTime { get; set; }

        public AbstractProcess(String processName, Brush brush)
        {
            this.processName = processName;
            this.brush = brush;
            this.isExecuted = false;
        }

        public void reDrawNewProcess()
        {
            chartUZ.reDrawNewProcess(this);
            chartUR.reDrawNewProcess(this);
            chartUTime.reDrawNewProcess(this);

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
            this.K = alphaZ;
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
            this.K = alphaZ;
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

        public virtual void executeProcess()
        {
            //values = new float[I + 1, J + 1, N + 1];
            values = new ProcessValues(I, J, N);
        }

        public virtual void executeProcess(object parameters)
        {
            //values = new float[I + 1, J + 1, N + 1];
            values = new ProcessValues(I, J, N);
            handler = (DiplomWPF.ProcessControl.increaseProgressBar)parameters;
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
            int i = (int)(r / hr);
            int j = (int)(z / hz);
            int n = (int)(t / ht);
            return values[i, j, n];
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
