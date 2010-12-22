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
using System.Windows.Shapes;
using DiplomWPF.Common;
using DiplomWPF.Common.Schemas;

namespace DiplomWPF.Client.Components
{
    /// <summary>
    /// Interaction logic for AddProcess.xaml
    /// </summary>
    public partial class AddProcessWindow : Window
    {
        private AbstractProcess process;

        public AddProcessWindow()
        {
            InitializeComponent();
        }

        public AbstractProcess getProcess()
        {
            return process;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem selectedItem = processTypeBox.SelectedValue as ComboBoxItem;
            String processType = selectedItem.Name;
            Color selectedColor = colorPicker.SelectedColor;
            String processName = processNameTextbox.Text;
            Int32 N = Int32.Parse(parametrN.Text);
            Int32 I = Int32.Parse(parametrI.Text);
            Int32 J = Int32.Parse(parametrJ.Text);
            if ("pismen" == processType)
            {
                process = new ChislProcess(processName, new SolidColorBrush(selectedColor));
                process.initializeSchema(I, J, N);
            }
            if ("yavn" == processType)
            {
                process = new YavnSchema(processName, new SolidColorBrush(selectedColor));
                process.initializeSchema(I, 1, N);
            }
            if ("analytic" == processType)
            {
                process = new AnalitProcess(processName, new SolidColorBrush(selectedColor));
                process.initializeSchema(I, J, N);
            }
            if ("parpismen" == processType)
            {
                process = new ParallelChislSchema(processName, new SolidColorBrush(selectedColor));
                process.initializeSchema(I, J, N);
            }
            if ("fullAnalytic" == processType)
            {
                process = new FullAnalitSchema(processName, new SolidColorBrush(selectedColor));
                process.initializeSchema(I, J, N);
            }
            if ("CLI" == processType)
            {
                process = new ChislOpenCLI(processName, new SolidColorBrush(selectedColor));
                process.initializeSchema(I, J, N);
            }

            this.Close();
        }
    }
}
