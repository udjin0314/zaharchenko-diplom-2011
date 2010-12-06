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
using System.Windows.Media.Media3D;

namespace DiplomWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //Chart2D chartUZ;
        //Chart2D chartUR;

        public Graph3D graphURZ { get; set; }

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
            processCtrl.process.delete();
            if (processCtrl.process == paramProcess)
            {
                ModelVisual3D m = (ModelVisual3D)mainViewport.Children[graphURZ.getModelNumber()];
                mainViewport.Children.Remove(m);
                paramProcess = null;
            }
            processCtrl.process = null;
        }

        private void initializeGraphics()
        {
            foreach (AbstractProcess process in processes)
            {
                process.initializeGraphics(chartUZPlotter, chartURPlotter, mainViewport);
            }
            graphURZ = new Graph3D(mainViewport);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Prepare data in arrays

        }


        public void OnViewportMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs args)
        {
            if (paramProcess != null) graphURZ.OnMouseDown(sender, args);
        }

        public void OnViewportMouseMove(object sender, System.Windows.Input.MouseEventArgs args)
        {
            if (paramProcess != null) graphURZ.OnMouseMove(sender, args);
        }

        public void OnViewportMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs args)
        {
            if (paramProcess != null) graphURZ.OnMouseUp(sender, args);
        }

        // zoom in 3d display
        public void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs args)
        {
            if (paramProcess != null) graphURZ.OnKeyDown(sender, args);
        }

        public void initializeProcessParams(AbstractProcess proc)
        {
            float alphaR = (float)Double.Parse(parametrAlphaR.Text);
            float P = (float)Double.Parse(parametrP.Text);
            float alphaZ = (float)Double.Parse(parametrAlphaZ.Text);
            float R = (float)Double.Parse(parametrR.Text);
            float l = (float)Double.Parse(parametrL.Text);
            float T = (float)Double.Parse(parametrExTime.Text);
            float c = (float)Double.Parse(parametrC.Text);
            float beta = (float)Double.Parse(parametrBeta.Text);
            float K = (float)Double.Parse(parametrK.Text);
            proc.initializeParams(P, alphaR, alphaZ, R, l, K, c, beta, T);
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
            if (paramProcess.isExecuted)
            {
                tempLegend.Header = paramProcess.processName;
                tempLegend.Visibility = System.Windows.Visibility.Visible;
                MaxTLabel.Content = Math.Round(paramProcess.maxTemperature, 3);
                MinTLabel.Content = Math.Round(paramProcess.minTemperature, 3);
                minTRect.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 255));
                maxTRect.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                graphURZ.reDrawNewProcess(paramProcess);
            }
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
            graphURZ.reDrawNewValues(len, time);

        }

        private List<AbstractProcess> getActiveProcesses()
        {
            List<AbstractProcess> processes = new List<AbstractProcess>();
            foreach (ProcessControl processCtrl in processControls)
                if (processCtrl.process.isExecuted) processes.Add(processCtrl.process);
            return processes;
        }

        public void chartUZ_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
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

        public void chartUR_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
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
            chartUR_ValueChanged(null, null);
            chartUZ_ValueChanged(null, null);
            
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
            
            if (paramProcess.isExecuted)
            {
                graphURZ.reDrawNewProcess(paramProcess);
                graphURZTimeSlider_ValueChanged(null, null);
            }
            resetProcessControls();
            processCtrl.processGroupBox.BorderBrush = Brushes.DarkGreen;
            processCtrl.processGroupBox.BorderThickness = new Thickness(2);
            prepareTempLegend();
            
        }
    }

}
