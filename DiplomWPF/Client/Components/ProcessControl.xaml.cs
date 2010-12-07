using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DiplomWPF.Common;
using DiplomWPF.Common.Helpers;
using System.Threading;

namespace DiplomWPF
{
    /// <summary>
    /// Interaction logic for ProcessControl.xaml
    /// </summary>
    public partial class ProcessControl : UserControl
    {
        public AbstractProcess process { get; set; }
        public MainWindow parentWindow;
        private int progressBarValue = 0;

        DThreadPool pool;


        public ProcessControl(AbstractProcess processIn)
        {
            this.process = processIn;
            InitializeComponent();
            processGroupBox.Header = process.processName;
            progressBar.Maximum = process.N;

        }

        public void processTimer()
        {
            if (process != null && pool != null && !pool.isClean())
            {
                if (pool.allThreadsCompleted())
                {
                    progressBar.Value = 0;
                    pool.Clear();
                    process.reDrawNewProcess();
                    parentWindow.chartUR_ValueChanged(null, null);
                    parentWindow.chartUZ_ValueChanged(null, null);
                    if (parentWindow.paramProcess == process)
                    {
                        //process.reDrawViewport();
                        parentWindow.prepareTempLegend();
                    }
                    
                }
                else progressBar.Value = progressBarValue;
            }

        }

        public delegate void increaseProgressBar();

        public void increaseProgressBarMethod()
        {
            progressBarValue++;
        }

        private void label1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void setProcessButton_Click(object sender, RoutedEventArgs e)
        {
            pool = new DThreadPool();
            setProcessButton.IsEnabled = false;
            parentWindow.initializeProcessParams(process);
            progressBar.Maximum = process.N;

            Thread thread = new Thread(new ParameterizedThreadStart(process.executeProcess));
            pool.addThread(thread);
            increaseProgressBar handler = increaseProgressBarMethod;
            thread.Start(handler);

            //process.executeProcess();

        }

        private void closeButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //TODO deleteProcessControl
            parentWindow.deleteProcessControl(this);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            parentWindow = MainWindow.GetWindow(this) as MainWindow;
        }

        private void processGroupBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            parentWindow.setProcess(this);
        }

        private void closeButton_MouseLeave(object sender, MouseEventArgs e)
        {
            closeButton.Height = 15;
            closeButton.Width = 15;
        }

        private void closeButton_MouseEnter(object sender, MouseEventArgs e)
        {
            closeButton.Height = 18;
            closeButton.Width = 18;
        }
    }
}
