using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay;
using DiplomWPF.Common.Comparators;
using System.Windows.Media;
using Microsoft.Research.DynamicDataDisplay.Charts.Navigation;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;

namespace DiplomWPF.Client.UI
{
    class ComparatorChart
    {
        private EnumerableDataSource<double> chartYDataSource = null;
        private EnumerableDataSource<double> xSrc = null;
        private CompositeDataSource dataSrc = null;

        private double[] chartX;
        private double[] chartY;

        LineAndMarker<MarkerPointsGraph> lineGraph = null;

        private SchemaComparator schemaComparator;


        private ChartPlotter plotter;

        private int globN = MainWindow.globN;

        public ComparatorChart(ChartPlotter plotterIn)
        {
            plotter = plotterIn;
        }

        public void delete()
        {
            //chartX = null;
            //chartY = null;
            //dataSrc = null;
            //chartYDataSource = null;
            //xSrc = null;
            plotter.Children.Remove(lineGraph.LineGraph);
            plotter.Children.Remove(lineGraph.MarkerGraph);
        }

        private void initilize(SchemaComparator inSchemaComparator)
        {
            schemaComparator = inSchemaComparator;
            int K = schemaComparator.numberExperiments;

            chartX = new double[K];
            chartY = new double[K];

            xSrc = new EnumerableDataSource<double>(chartX);
            xSrc.SetXMapping(x => x);
            chartYDataSource = new EnumerableDataSource<double>(chartY);
            chartYDataSource.SetYMapping(y => y);
            addGraph();
        }

        public void reinitialize(SchemaComparator inSchemaComparator)
        {
            schemaComparator = inSchemaComparator;
        }

        private void addGraph()
        {
            String name = schemaComparator.comparatorName;
            dataSrc = new CompositeDataSource(xSrc, chartYDataSource);
            lineGraph = plotter.AddLineGraph(dataSrc,
                            new Pen(schemaComparator.brush, 3),
                             new CirclePointMarker { Size = 10, Fill = Brushes.Red },
                            new PenDescription(name));

            reDrawNewValues();
            plotter.Children.Add(new CursorCoordinateGraph());
            plotter.FitToView();
        }

        private void delGraph()
        {
            //TODO find hot to remove old graphics
            plotter.RemoveUserElements();
        }

        public void reDrawNewValues()
        {
            prepareData();
            chartYDataSource.RaiseDataChanged();
        }

        public void reDrawNewProcess(SchemaComparator inSchemaComparator)
        {
            //delGraph();
            initilize(inSchemaComparator);
        }



        private void prepareData()
        {
            for (int i = 0; i < schemaComparator.numberExperiments; i++)
            {
                chartX[i] = schemaComparator.values[i, 0];
                chartY[i] = schemaComparator.values[i, 1];
            }
        }
    }
}
