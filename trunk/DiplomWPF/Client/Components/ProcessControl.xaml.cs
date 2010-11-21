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

namespace DiplomWPF
{
    /// <summary>
    /// Interaction logic for ProcessControl.xaml
    /// </summary>
    public partial class ProcessControl : UserControl
    {
        public AbstractProcess process { get; set; }
        public MainWindow parentWindow; 


        public ProcessControl(AbstractProcess processIn)
        {
            this.process = processIn;
            InitializeComponent();
            processGroupBox.Header = process.processName;
            progressBar.Maximum = process.N;

        }

        private void selectProcess_Unchecked(object sender, RoutedEventArgs e)
        {
            process.isVisible = false;
        }

        private void selectProcess_Checked(object sender, RoutedEventArgs e)
        {
            process.isVisible = true;
        }

        private void label1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //TODO open parameters
        }

        private void setProcessButton_Click(object sender, RoutedEventArgs e)
        {
            process.executeProcess();
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
    }
}
