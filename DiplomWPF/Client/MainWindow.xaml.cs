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

        public AbstractProcess paramProcess { get; set; }

        private BackgroundWorker backgroundWorker;

        public static int globN = 100;



        public MainWindow()
        {


            InitializeComponent();
            initBackgroundWorker();
            processes = new List<AbstractProcess>();
            processControls = new List<ProcessControl>();

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

        public void initSliders()
        {
            setSliderParam(graphURZZSlider);
            setSliderParam(graphURZTimeSlider);
            setSliderParam(chartURTimeSlider);
            setSliderParam(chartURZSlider);
            setSliderParam(chartUZRSlider);
            setSliderParam(chartUZTimeSlider);

        }

        public void setSliderParam(Slider slider)
        {
            slider.Maximum = globN;
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

        public void initializeProcessParams(AbstractProcess proc)
        {
            Double alphaR = Double.Parse(parametrAlphaR.Text);
            Double P = Double.Parse(parametrP.Text);
            Double alphaZ = Double.Parse(parametrAlphaZ.Text);
            Double R = Double.Parse(parametrR.Text);
            Double l = Double.Parse(parametrL.Text);
            Int32 T = Int32.Parse(parametrExTime.Text);
            Double c = Double.Parse(parametrC.Text);
            Double beta = Double.Parse(parametrBeta.Text);
            Double K = Double.Parse(parametrK.Text);
            proc.initializeParams(P, alphaR, alphaZ, R, l, K, c, beta, T);
        }


        //TODO deprecated
        private void parametersExecuteButton_Click(object sender, RoutedEventArgs e)
        {

            //if (process == null) process = new ChislProcess();
            Double alphaR = Double.Parse(parametrAlphaR.Text);
            Double P = Double.Parse(parametrP.Text);
            Double alphaZ = Double.Parse(parametrAlphaZ.Text);
            Double R = Double.Parse(parametrR.Text);
            Double l = Double.Parse(parametrL.Text);
            Double T = Double.Parse(parametrExTime.Text);
            Double c = Double.Parse(parametrC.Text);
            Double beta = Double.Parse(parametrBeta.Text);
            Double K = Double.Parse(parametrK.Text);

            foreach (AbstractProcess process in processes)
            {

                process.initializeParams(P, alphaR, alphaZ, R, l, K, c, beta, T);
                process.executeProcess();
            }
            processApply();

        }

        public void resetProcessControls()
        {
            foreach (ProcessControl proCtrl in processControls)
            {
                proCtrl.processGroupBox.BorderBrush = null;
                proCtrl.processGroupBox.BorderThickness = new Thickness(0);
            }
        }




        //TODO make it foreach
        public void prepareTempLegend()
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
            //prepareChartUR();
            //prepareChartUZ();
            //prepareGraphURZ();
            

        }


        //TODO make it foreach
        private void graphURZTimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            Int32 zn = (int)graphURZZSlider.Value;
            if (zn < 0) zn = 0;

            Double len = Double.Parse(parametrL.Text) * (zn) / globN;
            graphURZZLabel.Content = len.ToString() + " мм";

            Int32 tn = (int)graphURZTimeSlider.Value;
            if (tn < 0) tn = 0;
            Double time = Double.Parse(parametrExTime.Text) * (tn) / globN;
            graphURZTimeLabel.Content = time.ToString() + " c";
            foreach (AbstractProcess process in getActiveProcesses())
                process.graphURZ.reDrawNewValues(len, time);

        }

        private List<AbstractProcess> getActiveProcesses()
        {
            List<AbstractProcess> processes = new List<AbstractProcess>();
            foreach (ProcessControl processCtrl in processControls)
                if (processCtrl.process.isExecuted) processes.Add(processCtrl.process);
            return processes;
        }

        private void chartUZ_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Int32 rn = (int)chartUZRSlider.Value;
            if (rn < 0) rn = 0;
            Int32 tn = (int)chartUZTimeSlider.Value;
            if (tn < 0) tn = 0;
            Double radius = Double.Parse(parametrR.Text) * (rn) / globN;
            chartUZRLabel.Content = radius.ToString() + " мм";

            Double time = Double.Parse(parametrExTime.Text) * (tn) / globN;
            chartUZTimeLabel.Content = time.ToString() + " c";

            foreach (AbstractProcess process in getActiveProcesses())
                process.chartUZ.reDrawNewValues(radius, time);

        }

        private void chartUR_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Int32 zn = (int)chartURZSlider.Value;
            if (zn < 0) zn = 0;
            Int32 tn = (int)chartURTimeSlider.Value;
            if (tn < 0) tn = 0;
            Double len = Double.Parse(parametrL.Text) * (zn) / globN;
            chartURZLabel.Content = len.ToString() + " мм";

            Double time = Double.Parse(parametrExTime.Text) * (tn) / globN;
            chartURTimeLabel.Content = time.ToString() + " c";
            foreach (AbstractProcess process in getActiveProcesses())
                process.chartUR.reDrawNewValues(len, time);
        }

        private void addNewProcess(AbstractProcess process)
        {
            initializeProcessParams(process);
            process.initializeGraphics(chartUZPlotter, chartURPlotter, mainViewport);
            ProcessControl prc = new ProcessControl(process);
            processControls.Add(prc);
            drawProcessesToGrid();
            setProcess(prc);
            
        }

        private void drawProcessesToGrid()
        {
            processesGrid.RowDefinitions.Clear();
            processesGrid.Children.Clear();
            foreach (ProcessControl prCtrl in processControls)
            {

                RowDefinition rowDef1 = new RowDefinition();
                rowDef1.Height = GridLength.Auto;
                processesGrid.RowDefinitions.Add(rowDef1);
                Grid.SetRow(prCtrl, processesGrid.RowDefinitions.Count - 1);
                Grid.SetColumn(prCtrl, 0);
                processesGrid.Children.Add(prCtrl);
            }
        }

        private void addProcessLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //TODO open Form
            AddProcessWindow newProcessWindow = new AddProcessWindow();
            newProcessWindow.ShowDialog();
            if (newProcessWindow.getProcess() != null) addNewProcess(newProcessWindow.getProcess());
        }

        public void setProcess(ProcessControl processCtrl)
        {
            paramProcess = processCtrl.process;
            resetProcessControls();
            processCtrl.processGroupBox.BorderBrush = Brushes.DarkGreen;
            processCtrl.processGroupBox.BorderThickness = new Thickness(2);
            
        }
    }

}
