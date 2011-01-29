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

        DiplomWPF.MainWindow.increaseComparatorProgressBar handler;

        private int mode = 0;

        public Double[,] values { get; set; }

        public List<ComparatorData> compData { get; set; }

        public float value { get; set; }
        public Boolean withSameProcess = MainWindow.withSameProcess;

        public float shagExp { get; set; }

        public float r { get; set; }
        public float z { get; set; }
        public float t { get; set; }

        private int globN = MainWindow.globN;

        private bool executed = false;
        public Int32 numberExperiments { get; set; }

        public Boolean isExecuted()
        {
            return executed;
        }

        public SchemaComparator(AbstractProcess mainProc, AbstractProcess comparableProc)
        {
            this.comparatorName = mainProc.processName + " - " + comparableProc.processName;
            this.brush = comparableProc.brush;
            this.mainProc = mainProc;
            if (comparableProc != null) this.comparableProc = comparableProc;
        }

        public void initializeGraphics(ChartPlotter chartComparatorPlotter)
        {
            chartComparator = new ComparatorChart(chartComparatorPlotter);
        }

        public SchemaComparator(AbstractProcess mainProc, AbstractProcess comparableProc, Int32 numberExperiments, float shag, int mode, float r, float z, float t)
        {
            this.comparatorName = mainProc.processName + " - " + comparableProc.processName;
            this.brush = comparableProc.brush;
            this.mainProc = (AbstractProcess)mainProc.Clone();
            if (comparableProc != null) this.comparableProc = (AbstractProcess)comparableProc.Clone();
            this.shagExp = shag;
            this.r = r;
            this.z = z;
            this.t = t;
            this.numberExperiments = numberExperiments;
            this.mode = mode;

        }

        public void apply()
        {
            chartComparator.reDrawNewProcess(this);
            executed = true;
        }


        public void processSchema(int exp)
        {
            Int32 I = comparableProc.I;
            Int32 J = comparableProc.J;
            Int32 N = comparableProc.N;
            if (exp != 0)
            {


                if (mode == 0)
                {
                    I = comparableProc.getNextI(shagExp);
                    J = comparableProc.getNextJ(shagExp);
                    N = comparableProc.getNextN(shagExp);
                }
                else
                {
                    I = comparableProc.getPrevI(shagExp);
                    J = comparableProc.getPrevJ(shagExp);
                    N = comparableProc.getPrevN(shagExp);
                }
                comparableProc.values.clear();
                comparableProc.initialize(mainProc.P, mainProc.alphaR, mainProc.alphaZ, mainProc.R, mainProc.l, mainProc.K, mainProc.c, mainProc.beta, mainProc.T, N, I, J);
                //comparableProc.initializeSchema(I, J, N);
            }
            else if (withSameProcess) value = comparableProc.getPoint(r, z, t);
            if ((withSameProcess && exp != 0) || !withSameProcess)
            {
                if (withSameProcess) exp = exp - 1;
                comparableProc.executeProcess();
                //values[exp, 0] = I * J * N;
                values[exp, 0] = Math.Pow(shagExp,exp);
                values[exp, 1] = Math.Abs(value - comparableProc.getPoint(r, z, t));
                compData.Add(new ComparatorData(exp + 1, value, comparableProc.getPoint(r, z, t), (float)values[exp, 1], comparableProc.R / I, comparableProc.l / J, comparableProc.T / N, r, z, t));
                if (withSameProcess) value = comparableProc.getPoint(r, z, t);
            }
        }

        public Int32 getMaxIterations()
        {
            int maxit = 0;
            for (int i = 0; i < numberExperiments; i++)
            {
                maxit = maxit + comparableProc.getNextI(shagExp) * comparableProc.getNextJ(shagExp) * comparableProc.getNextN(shagExp);
            }
            return maxit;
        }

        public void execute()
        {
            execute(null);
        }

        public void execute(object parameters)
        {
            compData = new List<ComparatorData>();
            handler = (DiplomWPF.MainWindow.increaseComparatorProgressBar)parameters;
            values = new Double[numberExperiments + 1, 2];
            value = mainProc.getPoint(r, z, t);
            if (withSameProcess) comparableProc = mainProc;
            int points = numberExperiments;
            if (withSameProcess) points++;
            for (int i = 0; i < points; i++)
            {
                processSchema(i);
                if (handler != null) handler();
            }
        }

    }
}
