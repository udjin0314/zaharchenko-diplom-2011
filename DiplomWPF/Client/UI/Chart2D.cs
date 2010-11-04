using System;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using DiplomWPF.Common;
using Microsoft.Research.DynamicDataDisplay;
using System.Windows.Media;
using Microsoft.Research.DynamicDataDisplay.Charts.Navigation;

namespace DiplomWPF.Client.UI
{
    class Chart2D
    {
        private EnumerableDataSource<double> chartYDataSource = null;
        private EnumerableDataSource<double> xSrc = null;

        private double[] chartX;
        private double[] chartY;

        private Process process;

        private Boolean variableZ = false;

        private ChartPlotter plotter;

        public Chart2D(ChartPlotter plotterIn, Boolean variableZFactor)
        {
            plotter = plotterIn;
            if (variableZFactor) variableZ = true;
        }
        
        public void initilize(Process inProcess)
        {
            process = inProcess;
            int K = process.I;
            if (variableZ) K = process.J;
           
            chartX = new double[K];
            chartY = new double[K];

            xSrc = new EnumerableDataSource<double>(chartX);
            xSrc.SetXMapping(x => x);
            chartYDataSource = new EnumerableDataSource<double>(chartY);
            chartYDataSource.SetYMapping(y => y);
        }

        private void addGraph()
        {
            String name = "u(r)";
            if (variableZ) name = "u(z)";
            plotter.AddLineGraph(new CompositeDataSource(xSrc, chartYDataSource),
                            new Pen(Brushes.Magenta, 3),
                            new PenDescription(name));
            reDrawNewValues(0, 0);
            plotter.Children.Add(new CursorCoordinateGraph());
            plotter.FitToView();
        }

        private void delGraph()
        {
        }

        public void reDrawNewValues(int Rk, int Rn)
        {
            prepareData(Rk, Rn);
            chartYDataSource.RaiseDataChanged();
        }

        public void reDrawNewProcess(Process processIn)
        {
            delGraph();
            initilize(processIn);
            addGraph();
        }

        private void prepareData(int Rk, int Rn)
        {
            if (variableZ)
                for (int j = 0; j < process.J; j++)
                {
                    chartX[j] = j * process.hz;
                    chartY[j] = process.values[Rk, j, Rn];
                }
            else
                for (int i = 0; i < process.I; i++)
                {
                    chartX[i] = i * process.hr;
                    chartY[i] = process.values[i, Rk, Rn];
                }
        }

    }
}
