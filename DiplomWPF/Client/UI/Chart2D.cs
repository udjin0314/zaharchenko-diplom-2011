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
        private EnumerableDataSource<float> chartYDataSource = null;
        private EnumerableDataSource<float> xSrc = null;
        private CompositeDataSource dataSrc = null;

        private float[] chartX;
        private float[] chartY;

        LineGraph lineGraph = null;

        private AbstractProcess process;

        private int mode = 0;

        private ChartPlotter plotter;

        private int globN = MainWindow.globN;

        public Chart2D(ChartPlotter plotterIn, int mode)
        {
            plotter = plotterIn;
            this.mode = mode;
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

            chartX = new float[K + 1];
            chartY = new float[K + 1];

            xSrc = new EnumerableDataSource<float>(chartX);
            xSrc.SetXMapping(x => x);
            chartYDataSource = new EnumerableDataSource<float>(chartY);
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
            if (mode == 1) name = process.processName + " u(z)";
            if (mode == 2) name = process.processName + " u(t)";
            dataSrc = new CompositeDataSource(xSrc, chartYDataSource);
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
            if (mode == 1)
            {
                int rki = (int)Math.Round(Rk / process.hr);
                for (int j = 0; j <= globN; j++)
                {
                    chartX[j] = j * process.l / globN;
                    int jint = j * process.J / globN;

                    float diff = (float)globN / process.J;
                    float z = process.values[rki, jint, rni];
                    if ((jint != process.J))
                    {
                        float k = (float)((chartX[j] - jint * process.hz) / process.hz * (process.values[rki, jint + 1, rni] - process.values[rki, jint, rni]));
                        z += k;
                    }

                    chartY[j] = z;
                }
            }
            else if (mode == 2)
            {
                rni = (int)Math.Round(Rk / process.hz);
                int rki = (int)Math.Round(Rn / process.hr);
                for (int t = 0; t <= globN; t++)
                {
                    chartX[t] = t * process.T / globN;
                    int tint = t * process.N / globN;

                    float z = process.values[rki, rni, tint];
                    if ((tint != process.N))
                    {
                        float k = (float)((chartX[t] - tint * process.ht) / process.ht * (process.values[rki, rni, tint + 1] - process.values[rki, rni, tint]));
                        z += k;
                    }

                    chartY[t] = z;
                }
            }
            else
            {
                int rki = (int)Math.Round(Rk / process.hz);
                for (int i = 0; i <= globN; i++)
                {
                    chartX[i] = i * process.R / globN;
                    int jint = i * process.I / globN;

                    float z = process.values[jint, rki, rni];
                    if ((jint != process.I))
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
