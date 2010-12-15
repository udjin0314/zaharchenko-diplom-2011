using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiplomWPF.Client.UI;
using System.Windows.Media;
using Microsoft.Research.DynamicDataDisplay;

namespace DiplomWPF.Common.Comparators
{
    class SchemaComparator
    {
        public String comparatorName { get; set; }
        public Brush brush { get; set; }
        public ComparatorChart chartComparator { get; set; }
        public AbstractProcess mainProc { get; set; }
        public AbstractProcess comparableProc { get; set; }

        private int mode = 0;

        public Double[,] values { get; set; }

        public Int32 maxSchemaSize { get; set; }
        public Int32 minSchemaSize { get; set; }
        public Int32 shag { get; set; }

        public float r { get; set; }
        public float z { get; set; }
        public float t { get; set; }

        private int globN = MainWindow.globN;

        private bool executed = false;
        public Int32 pointsN { get; set; }

        public Boolean isExecuted()
        {
            return executed;
        }

        public SchemaComparator(AbstractProcess mainProc, AbstractProcess comparableProc)
        {
            this.comparatorName = mainProc.processName + " - " + comparableProc.processName;
            this.brush = comparableProc.brush;
            this.mainProc = mainProc;
            this.comparableProc = comparableProc;
        }

        public void initializeGraphics(ChartPlotter chartComparatorPlotter)
        {
            chartComparator = new ComparatorChart(chartComparatorPlotter);
        }

        public SchemaComparator(AbstractProcess mainProc, AbstractProcess comparableProc, Int32 minSchemaSize, Int32 maxSchemaSize, Int32 shag, float r, float z, float t, int mode)
        {
            this.comparatorName = mainProc.processName + " - " + comparableProc.processName;
            this.brush = comparableProc.brush;
            this.mainProc = (AbstractProcess)mainProc.Clone();
            this.comparableProc = (AbstractProcess)comparableProc.Clone();
            this.minSchemaSize = minSchemaSize;
            this.maxSchemaSize = maxSchemaSize;
            this.shag = shag;
            this.r = r;
            this.z = z;
            this.t = t;
            this.mode = mode;
            this.pointsN = (Int32)(Math.Round((double)(maxSchemaSize - minSchemaSize)) / shag);

        }

        public void apply()
        {
            chartComparator.reDrawNewProcess(this);
            executed = true;
        }

        public void processSchemaParam(Int32 schemParameter, int i)
        {
            if (mode == 0) comparableProc.initializeSchema(schemParameter, comparableProc.J, comparableProc.N);
            if (mode == 1) comparableProc.initializeSchema(comparableProc.I, schemParameter, comparableProc.N);
            if (mode == 2) comparableProc.initializeSchema(comparableProc.I, comparableProc.J, schemParameter);
            comparableProc.executeProcess();
            values[i, 0] = schemParameter;
            values[i, 1] = Math.Abs(mainProc.getPoint(r, z, t) - comparableProc.getPoint(r, z, t));
        }

        public void execute()
        {
            if (!mainProc.isExecuted) mainProc.executeProcess();

            values = new Double[pointsN+1, 2];
            for (int i = 0; i <= pointsN; i++)
            {
                Int32 schemParameter = i * shag+minSchemaSize;
                processSchemaParam(schemParameter, i);
            }
        }

        public void execute(object parameters)
        {
            DiplomWPF.MainWindow.increaseComparatorProgressBar handler = (DiplomWPF.MainWindow.increaseComparatorProgressBar)parameters;
            if (!mainProc.isExecuted) mainProc.executeProcess();
            values = new Double[pointsN + 1, 2];
            for (int i = 0; i <= pointsN; i++)
            {
                Int32 schemParameter = i * shag + minSchemaSize;
                processSchemaParam(schemParameter, i);
                handler();
            }
        }

    }
}
