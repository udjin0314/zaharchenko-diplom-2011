using System;
using System.Windows;
using DiplomWPF.Common;

using WPFChart3D;
using DiplomWPF.Client;
using DiplomWPF.Client.UI;
using System.Windows.Media;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Input;
using System.Windows.Controls;
using DiplomWPF.Client.Components;

namespace DiplomWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //Chart2D chartUZ;
        //Chart2D chartUR;

        private List<AbstractProcess> processes;

        private List<ProcessControl> processControls;

        private AbstractProcess paramProcess;

        private BackgroundWorker backgroundWorker;

        

        public MainWindow()
        {
            
            
            InitializeComponent();
            initBackgroundWorker();
            processes = new List<AbstractProcess>();
            processControls = new List<ProcessControl>();
            ChislProcess pismenProc = new ChislProcess("Писмена Рекфорда", Brushes.Magenta);
            ProcessControl prc = new ProcessControl(pismenProc);
            processesGrid.Children.Add(prc);
            processControls.Add(prc);
            processes.Add(pismenProc);
            initializeGraphics();
            //chartUZ = new Chart2D(chartUZPlotter, true);
            //chartUR = new Chart2D(chartURPlotter, false);
            //graphURZ = new Graph3D(mainViewport);
            // selection rect

        }

        public void initBackgroundWorker()
        {
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;
            
        }

        public void deleteProcessControl(ProcessControl processCtrl)
        {
            processesGrid.Children.Remove(processCtrl);
            processControls.Remove(processCtrl);
        }

        private void initializeGraphics()
        {
            foreach (AbstractProcess process in processes)
            {
                process.initializeGraphics(chartUZPlotter, chartURPlotter, mainViewport);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Prepare data in arrays

        }


        public void OnViewportMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs args)
        {
            if (paramProcess != null) paramProcess.graphURZ.OnMouseDown(sender, args);
        }

        public void OnViewportMouseMove(object sender, System.Windows.Input.MouseEventArgs args)
        {
            if (paramProcess != null) paramProcess.graphURZ.OnMouseMove(sender, args);
        }

        public void OnViewportMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs args)
        {
            if (paramProcess != null) paramProcess.graphURZ.OnMouseUp(sender, args);
        }

        // zoom in 3d display
        public void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs args)
        {
            if (paramProcess != null) paramProcess.graphURZ.OnKeyDown(sender, args);
        }

        private void parametersExecuteButton_Click(object sender, RoutedEventArgs e)
        {

            //if (process == null) process = new ChislProcess();
            Double alphaR = Double.Parse(parametrAlphaR.Text);
            Double P = Double.Parse(parametrP.Text);
            Double alphaZ = Double.Parse(parametrAlphaZ.Text);
            Double R = Double.Parse(parametrR.Text);
            Double l = Double.Parse(parametrL.Text);
            Int32 T = Int32.Parse(parametrExTime.Text);
            Double c = Double.Parse(parametrC.Text);
            Double beta = Double.Parse(parametrBeta.Text);
            Double K = Double.Parse(parametrK.Text);
            Int32 I = Int32.Parse(parametrI.Text);
            Int32 J = Int32.Parse(parametrJ.Text);
            Int32 N = Int32.Parse(parametrT.Text);


            foreach (AbstractProcess process in processes)
            {
                
                process.initialize(P, alphaR, alphaZ, R, l, K, c, beta, T, N, I, J);
                process.executeProcess();
                paramProcess=process;
            }
            processApply();


        }

        public void prepareChartUZ()
        {
            chartUZTimeSlider.Maximum = paramProcess.N;
            chartUZTimeSlider.TickFrequency = 1;
            chartUZTimeSlider.IsEnabled = true;
            chartUZRSlider.Maximum = paramProcess.I;
            chartUZRSlider.TickFrequency = 1;
            chartUZRSlider.IsEnabled = true;
        }

        public void prepareChartUR()
        {
            chartURTimeSlider.Maximum = paramProcess.N;
            chartURTimeSlider.TickFrequency = 1;
            chartURTimeSlider.IsEnabled = true;
            chartURZSlider.Maximum = paramProcess.J;
            chartURZSlider.TickFrequency = 1;
            chartURZSlider.IsEnabled = true;
        }

        public void prepareGraphURZ()
        {
            graphURZTimeSlider.Maximum = paramProcess.N;
            graphURZTimeSlider.TickFrequency = 1;
            graphURZTimeSlider.IsEnabled = true;

            graphURZZSlider.Maximum = paramProcess.J;
            graphURZZSlider.TickFrequency = 1;
            graphURZZSlider.IsEnabled = true;
        }


        //TODO make it foreach
        private void prepareTempLegend()
        {
            tempLegend.Header = paramProcess.processName;
            tempLegend.Visibility = System.Windows.Visibility.Visible;
            MaxTLabel.Content = Math.Round(paramProcess.maxTemperature, 3);
            MinTLabel.Content = Math.Round(paramProcess.minTemperature, 3);
            minTRect.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 255));
            maxTRect.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        }

        //TODO make it foreach
        private void processApply()
        {
            foreach (AbstractProcess process in processes)
                process.reDrawNewProcess();
            prepareChartUR();
            prepareChartUZ();
            prepareGraphURZ();
            prepareTempLegend();

        }


        //TODO make it foreach
        private void graphURZTimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            Int32 zn = (int)graphURZZSlider.Value;
            if (zn < 0) zn = 0;

            Double len = paramProcess.hz * (zn);
            graphURZZLabel.Content = len.ToString() + " мм";

            Int32 tn = (int)graphURZTimeSlider.Value;
            if (tn < 0) tn = 0;
            Double time = paramProcess.ht * (tn);
            graphURZTimeLabel.Content = time.ToString() + " c";
            foreach (AbstractProcess process in processes)
                process.graphURZ.reDrawNewValues(tn, zn);

        }

        private void chartUZ_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Int32 rn = (int)chartUZRSlider.Value;
            if (rn < 0) rn = 0;
            Int32 tn = (int)chartUZTimeSlider.Value;
            if (tn < 0) tn = 0;
            Double radius = paramProcess.hr * (rn);
            chartUZRLabel.Content = radius.ToString() + " мм";

            Double time = paramProcess.ht * (tn);
            chartUZTimeLabel.Content = time.ToString() + " c";

            foreach (AbstractProcess process in processes)
                process.chartUZ.reDrawNewValues(rn, tn);

        }

        private void chartUR_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Int32 zn = (int)chartURZSlider.Value;
            if (zn < 0) zn = 0;
            Int32 tn = (int)chartURTimeSlider.Value;
            if (tn < 0) tn = 0;
            Double len = paramProcess.hz * (zn);
            chartURZLabel.Content = len.ToString() + " мм";

            Double time = paramProcess.ht * (tn);
            chartURTimeLabel.Content = time.ToString() + " c";
            foreach (AbstractProcess process in processes)
                process.chartUR.reDrawNewValues(zn, tn);
        }

        private void addNewProcess(AbstractProcess process)
        {
            ProcessControl prc = new ProcessControl(process);
            processesGrid.Children.Add(prc);
        }

        private void addProcessLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //TODO open Form
            AddProcessWindow newProcessWindow = new AddProcessWindow();
            newProcessWindow.ShowDialog();
            addNewProcess(newProcessWindow.getProcess());
        }
    }

}
