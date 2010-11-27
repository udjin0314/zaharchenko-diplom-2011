using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiplomWPF.Client.UI;
using Microsoft.Research.DynamicDataDisplay;
using System.Windows.Controls;
using System.Windows.Media;

namespace DiplomWPF.Common
{
    public abstract class AbstractProcess
    {
        public String processName { get; set; }
        public Brush brush { get; set; }
        public Double[, ,] values { get; set; }
        public Double P { get; set; }
        public Double alphaR { get; set; }
        public Double alphaZ { get; set; }
        public Double R { get; set; }
        public Double l { get; set; }

        public Boolean isExecuted { get; private set; }

        public Double a { get; set; }
        public Double K { get; set; }

        public Double c { get; set; }

        public Double beta { get; set; }

        public Double T { get; set; }

        public Int32 N { get; set; }
        public Int32 I { get; set; }
        public Int32 J { get; set; }

        public Double ht { get; set; }
        public Double hz { get; set; }
        public Double hr { get; set; }

        public Double maxTemperature { get; set; }

        public Double minTemperature { get; set; }

        //graphics elements

        public Chart2D chartUZ { get; set; }
        public Chart2D chartUR { get; set; }

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
            
        }


        public void initializeGraphics(ChartPlotter chartUZPlotter, ChartPlotter chartURPlotter, Viewport3D viewport)
        {
            chartUZ = new Chart2D(chartUZPlotter, true);
            chartUR = new Chart2D(chartURPlotter, false);
        }

        public virtual void initialize(Double P, Double alphaR, Double alphaZ, Double R, Double l, Double K, Double c, Double beta, Double T, Int32 N, Int32 I, Int32 J)
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
            this.a = 0.15 * R;
            this.hr = R / I;
            this.hz = l / J;
            this.ht = T / N;
            values = new double[I + 1, J + 1, N + 1];
        }

        public virtual void initializeParams(Double P, Double alphaR, Double alphaZ, Double R, Double l, Double K, Double c, Double beta, Double T)
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
            this.a = 0.15 * R;
            this.hr = R / I;
            this.hz = l / J;
            this.ht = T / N;
            values = new double[I + 1, J + 1, N + 1];
        }

        public virtual void initializeSchema(Int32 N, Int32 I, Int32 J)
        {
            this.N = N;
            this.I = I;
            this.J = J;
        }

        public virtual void executeProcess()
        {
            isExecuted = true;
        }

        public virtual void delete()
        {
            chartUR.delete();
            chartUZ.delete();
            chartUR = null;
            chartUZ = null;
        }
    }
}
