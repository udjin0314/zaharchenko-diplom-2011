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
        private CompositeDataSource dataSrc=null;

        private double[] chartX;
        private double[] chartY;

        LineGraph lineGraph=null;

        private AbstractProcess process;

        private Boolean variableZ = false;

        private ChartPlotter plotter;

        private int globN = MainWindow.globN;

        public Chart2D(ChartPlotter plotterIn, Boolean variableZFactor)
        {
            plotter = plotterIn;
            if (variableZFactor) variableZ = true;
        }

        public void delete()
        {
            //chartX = null;
            //chartY = null;
            //dataSrc = null;
            //chartYDataSource = null;
            //xSrc = null;
            plotter.Children.Remove(lineGraph);
        }

        private void initilize(AbstractProcess inProcess)
        {
            process = inProcess;
            int K = globN;

            chartX = new double[K + 1];
            chartY = new double[K + 1];

            xSrc = new EnumerableDataSource<double>(chartX);
            xSrc.SetXMapping(x => x);
            chartYDataSource = new EnumerableDataSource<double>(chartY);
            chartYDataSource.SetYMapping(y => y);
            addGraph();
        }

        public void reinitialize(AbstractProcess inProcess)
        {
            process = inProcess;
        }

        private void addGraph()
        {
            String name = process.processName + " u(r)";
            if (variableZ) name = process.processName + " u(z)";
            dataSrc =new CompositeDataSource(xSrc, chartYDataSource);
            lineGraph = plotter.AddLineGraph(dataSrc,
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

        public void reDrawNewValues(double Rk, double Rn)
        {
            prepareData(Rk, Rn);
            chartYDataSource.RaiseDataChanged();
        }

        public void reDrawNewProcess(AbstractProcess processIn)
        {
            //delGraph();
            if (chartX == null) initilize(processIn);
            else reinitialize(processIn);
        }

        private void prepareData(double Rk, double Rn)
        {
            int rni = (int)Math.Round(Rn / process.ht);
            if (variableZ)
            {
                int rki = (int)Math.Round(Rk / process.hr);
                for (int j = 0; j <= globN; j++)
                {
                    chartX[j] = j * process.l / globN;
                    int jint = j * process.J / globN;

                    int diff = globN / process.J;
                    double z = process.values[rki, jint, rni];
                    if ((jint != process.J) && diff > 1)
                    {
                        float k = (float)((chartX[j] - jint * process.hz) / process.hz * (process.values[rki, jint + 1, rni] - process.values[rki, jint, rni]));
                        z += k;
                    }

                    chartY[j] = z;
                }
            }
            else
            {
                int rki = (int)Math.Round(Rk / process.hz);
                for (int i = 0; i <= globN; i++)
                {
                    chartX[i] = i * process.R / globN;
                    int jint = i * process.I / globN;

                    int diff = globN / process.I;
                    double z = process.values[jint, rki, rni];
                    if ((jint != process.I) && diff > 1)
                    {
                        float k = (float)((chartX[i] - jint * process.hr) / process.hr * (process.values[jint + 1, rki, rni] - process.values[jint, rki, rni]));
                        z += k;
                    }

                    chartY[i] = z;
                }
            }
        }

    }
}
