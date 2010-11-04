using System;
using System.Windows;
using DiplomWPF.Common;

using WPFChart3D;
using DiplomWPF.Client;
using DiplomWPF.Client.UI;
using System.Windows.Media;

namespace DiplomWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Chart2D chartUZ;
        Chart2D chartUR;
        Graph3D graphURZ;

        // transform class object for rotate the 3d model
        public TransformMatrix m_transformMatrix = new TransformMatrix();

        // ***************************** 3d chart ***************************
        private Chart3D m_3dChart;       // data for 3d chart
        public int m_nChartModelIndex = -1;         // model index in the Viewport3d
        public int m_nSurfaceChartGridNo = 100;     // surface chart grid no. in each axis
        public int m_nScatterPlotDataNo = 5000;     // total data number of the scatter plot

        private Process process;

        // ***************************** selection rect ***************************
        ViewportRect m_selectRect = new ViewportRect();
        public int m_nRectModelIndex = -1;

        public MainWindow()
        {
            InitializeComponent();
            chartUZ = new Chart2D(chartUZPlotter, true);
            chartUR = new Chart2D(chartURPlotter, false);
            graphURZ = new Graph3D(mainViewport);
            // selection rect

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Prepare data in arrays

        }

        public void OnViewportMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs args)
        {
            graphURZ.OnMouseDown(sender, args);
        }

        public void OnViewportMouseMove(object sender, System.Windows.Input.MouseEventArgs args)
        {
            graphURZ.OnMouseMove(sender, args);
        }

        public void OnViewportMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs args)
        {
            graphURZ.OnMouseUp(sender, args);
        }

        // zoom in 3d display
        public void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs args)
        {
            graphURZ.OnKeyDown(sender, args);
        }

        private void parametersExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            if (process == null) process = new Process();
            process.alphaR = Double.Parse(parametrAlphaR.Text);
            process.P = Double.Parse(parametrP.Text);
            process.alphaZ = Double.Parse(parametrAlphaZ.Text);
            process.R = Double.Parse(parametrR.Text);
            process.L = Double.Parse(parametrL.Text);
            process.T = Int32.Parse(parametrExTime.Text);

            process.I = Int32.Parse(parametrI.Text);
            process.J = Int32.Parse(parametrJ.Text);
            process.N = Int32.Parse(parametrT.Text);

            DataProcessProviderClient dataProcessProviderClient = new DataProcessProviderClient();
            process = dataProcessProviderClient.getProcessValues(process);
            processApply();


        }

        public void prepareChartUZ()
        {
            chartUZTimeSlider.Maximum = process.N;
            chartUZTimeSlider.TickFrequency = 1;
            chartUZTimeSlider.IsEnabled = true;
            chartUZRSlider.Maximum = process.I;
            chartUZRSlider.TickFrequency = 1;
            chartUZRSlider.IsEnabled = true;
        }

        public void prepareChartUR()
        {
            chartURTimeSlider.Maximum = process.N;
            chartURTimeSlider.TickFrequency = 1;
            chartURTimeSlider.IsEnabled = true;
            chartURZSlider.Maximum = process.J;
            chartURZSlider.TickFrequency = 1;
            chartURZSlider.IsEnabled = true;
        }

        public void prepareGraphURZ()
        {
            graphURZTimeSlider.Maximum = process.N;
            graphURZTimeSlider.TickFrequency = 1;
            graphURZTimeSlider.IsEnabled = true;

            graphURZZSlider.Maximum = process.J;
            graphURZZSlider.TickFrequency = 1;
            graphURZZSlider.IsEnabled = true;
        }

        private void prepareTempLegend()
        {
            MaxTLabel.Content = Math.Round(process.maxTemperature,3);
            MinTLabel.Content = Math.Round(process.minTemperature,3);
            minTRect.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 255));
            maxTRect.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        }

        private void processApply()
        {
            if (process != null && process.values != null)
            {
                chartUZ.reDrawNewProcess(process);
                prepareChartUZ();

                chartUR.reDrawNewProcess(process);
                prepareChartUR();

                graphURZ.reDrawNewProcess(process);
                prepareGraphURZ();

                prepareTempLegend();
                
            }
        }



        private void graphURZTimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            Int32 zn = (int)graphURZZSlider.Value - 1;
            if (zn < 0) zn = 0;

            Double len = process.hz * (zn + 1);
            graphURZZLabel.Content = len.ToString() + " мм";
            
            Int32 tn = (int)graphURZTimeSlider.Value - 1;
            if (tn < 0) tn = 0;
            Double time = process.ht * (tn + 1);
            graphURZTimeLabel.Content = time.ToString() + " c";

            graphURZ.reDrawNewValues(tn,zn);
        }

        private void chartUZ_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Int32 rn = (int)chartUZRSlider.Value - 1;
            if (rn < 0) rn = 0;
            Int32 tn = (int)chartUZTimeSlider.Value - 1;
            if (tn < 0) tn = 0;
            Double radius = process.hr * (rn + 1);
            chartUZRLabel.Content = radius.ToString() + " мм";

            Double time = process.ht * (tn + 1);
            chartUZTimeLabel.Content = time.ToString() + " c";

            chartUZ.reDrawNewValues(rn, tn);
        }

        private void chartUR_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Int32 zn = (int)chartURZSlider.Value - 1;
            if (zn < 0) zn = 0;
            Int32 tn = (int)chartURTimeSlider.Value - 1;
            if (tn < 0) tn = 0;
            Double len = process.hz * (zn + 1);
            chartURZLabel.Content = len.ToString() + " мм";

            Double time = process.ht * (tn + 1);
            chartURTimeLabel.Content = time.ToString() + " c";

            chartUR.reDrawNewValues(zn, tn);
        }
    }

}
