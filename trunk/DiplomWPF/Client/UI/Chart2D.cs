using System;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using DiplomWPF.Common;
using Microsoft.Research.DynamicDataDisplay;
using System.Windows.Media;
using Microsoft.Research.DynamicDataDisplay.Charts.Navigation;

namespace DiplomWPF.Client.UI
{
    public class Chart2D
    {
        private EnumerableDataSource<double> chartYDataSource = null;
        private EnumerableDataSource<double> xSrc = null;

        private double[] chartX;
        private double[] chartY;

        private AbstractProcess process;

        private Boolean variableZ = false;

        private ChartPlotter plotter;

        public Chart2D(ChartPlotter plotterIn, Boolean variableZFactor)
        {
            plotter = plotterIn;
            if (variableZFactor) variableZ = true;
        }

        public void initilize(AbstractProcess inProcess)
        {
            process = inProcess;
            int K = process.I;
            if (variableZ) K = process.J;
           
            chartX = new double[K+1];
            chartY = new double[K+1];

            xSrc = new EnumerableDataSource<double>(chartX);
            xSrc.SetXMapping(x => x);
            chartYDataSource = new EnumerableDataSource<double>(chartY);
            chartYDataSource.SetYMapping(y => y);
        }

        private void addGraph()
        {
            String name = process.processName+" u(r)";
            if (variableZ) name = process.processName+" u(z)";
            plotter.AddLineGraph(new CompositeDataSource(xSrc, chartYDataSource),
                            new Pen(process.brush, 3),
                            new PenDescription(name));
            reDrawNewValues(0, 0);
            plotter.Children.Add(new CursorCoordinateGraph());
            plotter.FitToView();
        }

        private void delGraph()
        {
            //TODO find hot to remove old graphics
            plotter.RemoveUserElements();
        }

        public void reDrawNewValues(int Rk, int Rn)
        {
            prepareData(Rk, Rn);
            chartYDataSource.RaiseDataChanged();
        }

        public void reDrawNewProcess(AbstractProcess processIn)
        {
            //delGraph();
            initilize(processIn);
            addGraph();
        }

        private void prepareData(int Rk, int Rn)
        {
            if (variableZ)
                for (int j = 0; j <= process.J; j++)
                {
                    chartX[j] = j * process.hz;
                    chartY[j] = process.values[Rk, j, Rn];
                }
            else
                for (int i = 0; i <= process.I; i++)
                {
                    chartX[i] = i * process.hr;
                    chartY[i] = process.values[i, Rk, Rn];
                }
        }

    }
}
