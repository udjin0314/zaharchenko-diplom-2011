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
using DiplomWPF.Common.Comparators;
using DiplomWPF.Common.Helpers;
using System.Windows.Threading;

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

        private Boolean compExecute = false;

        private DThreadPool comparatorsPool;

        private List<SchemaComparator> comparators;

        DispatcherTimer _timer;

        private int comparatorsValue = 0;

        public MainWindow()
        {


            InitializeComponent();
            initBackgroundWorker();
            processes = new List<AbstractProcess>();
            processControls = new List<ProcessControl>();

            initializeGraphics();

            _timer = new DispatcherTimer();
            _timer.Tick += new EventHandler(delegate(object s, EventArgs a)
            {
                checkThreads();
                foreach (ProcessControl prcCtrl in processControls)
                {
                    prcCtrl.processTimer();
                }
            });
            _timer.Interval = TimeSpan.FromMilliseconds(100);

            // Запуск таймера
            _timer.Start();
            //chartUZ = new Chart2D(chartUZPlotter, true);
            //chartUR = new Chart2D(chartURPlotter, false);
            //graphURZ = new Graph3D(mainViewport);
            // selection rect

        }

        public delegate void increaseComparatorProgressBar();

        public void increaseComparatorProgressBarMethod()
        {
            comparatorsValue++;
        }

        private void checkThreads()
        {
            if (comparators != null && comparatorsPool != null && !comparatorsPool.isClean() && comparators.Count > 0)
            {
                if (!compExecute)
                {
                    if (comparatorsPool.allThreadsCompleted())
                    {
                        foreach (SchemaComparator comparator in comparators)
                        {
                            comparator.apply();
                            comparatorsValue = 0;
                            comparatorProgressBar.Value = 0;
                            applyButton.IsEnabled = true;
                            compExecute = true;
                        }
                    }
                    else comparatorProgressBar.Value = comparatorsValue;
                }
            }
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
            setSliderParam(chartUTimeRSlider);
            setSliderParam(chartUTimeZSlider);

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
            if (processCtrl.process == paramProcess && processCtrl.process.isExecuted)
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
                process.initializeGraphics(chartUZPlotter, chartURPlotter, chartUTimePlotter);
            }
            graphURZ = new Graph3D(mainViewport);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Prepare data in arrays

        }


        public void OnViewportMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs args)
        {
            if (paramProcess != null && paramProcess.isExecuted) graphURZ.OnMouseDown(sender, args);
        }

        public void OnViewportMouseMove(object sender, System.Windows.Input.MouseEventArgs args)
        {
            if (paramProcess != null && paramProcess.isExecuted) graphURZ.OnMouseMove(sender, args);
        }

        public void OnViewportMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs args)
        {
            if (paramProcess != null && paramProcess.isExecuted) graphURZ.OnMouseUp(sender, args);
        }

        // zoom in 3d display
        public void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs args)
        {
            if (paramProcess != null && paramProcess.isExecuted) graphURZ.OnKeyDown(sender, args);
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

        public void chartUTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Int32 zn = (int)chartUTimeZSlider.Value;
            if (zn < 0) zn = 0;
            Int32 rn = (int)chartUTimeRSlider.Value;
            if (rn < 0) rn = 0;
            Double len = Double.Parse(parametrL.Text) * (zn) / globN;
            chartUTimeZLabel.Content = len.ToString() + " мм";

            Double radius = Double.Parse(parametrR.Text) * (rn) / globN;
            chartUTimeRLabel.Content = radius.ToString() + " мм";
            foreach (AbstractProcess process in getActiveProcesses())
                process.chartUTime.reDrawNewValues(len, radius);
        }

        private void addNewProcess(AbstractProcess process)
        {
            initializeProcessParams(process);
            process.initializeGraphics(chartUZPlotter, chartURPlotter, chartUTimePlotter);
            ProcessControl prc = new ProcessControl(process);
            processControls.Add(prc);
            drawProcessesToGrid();
            setProcess(prc);
            chartUR_ValueChanged(null, null);
            chartUZ_ValueChanged(null, null);
            chartUTime_ValueChanged(null, null);

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

        private void applyButton_Click(object sender, RoutedEventArgs e)
        {
            applyButton.IsEnabled = false;
            compExecute = false;
            float r = (float)Double.Parse(approxRParam.Text);
            float z = (float)Double.Parse(approxZParam.Text);
            float t = (float)Double.Parse(approxTParam.Text);
            Int32 maxSize = Int32.Parse(approxMaxSParam.Text);
            Int32 minSize = Int32.Parse(approxMinSParam.Text);
            Int32 shag = Int32.Parse(shagTextBox.Text);
            int mode = 0;
            if (radioButtonR.IsChecked == true) mode = 0;
            if (radioButtonZ.IsChecked == true) mode = 1;
            if (radioButtonT.IsChecked == true) mode = 2;
            if (comparators!=null) foreach (SchemaComparator comparator in comparators)
            {
                comparator.chartComparator.delete();
            }
            comparatorsPool = new DThreadPool();
            comparators = new List<SchemaComparator>();
            comparatorProgressBar.Maximum = (maxSize-minSize)/shag * processControls.Count - 1;
            foreach (ProcessControl prCtrl in processControls)
            {
                AbstractProcess process = prCtrl.process;
                if (process != paramProcess)
                {
                    SchemaComparator comparator = new SchemaComparator(paramProcess, process, minSize, maxSize, shag, r, z, t, mode);
                    comparator.initializeGraphics(comparatorChartPlotter);
                    comparators.Add(comparator);
                    Thread thread = new Thread(new ParameterizedThreadStart(comparator.execute));
                    thread.Name = comparator.comparatorName;
                    comparatorsPool.addThread(thread);
                    increaseComparatorProgressBar handler = increaseComparatorProgressBarMethod;
                    thread.Start(handler);
                }

            }

        }
    }

}
