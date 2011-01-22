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
using Cloo;

namespace DiplomWPF.Client.Components
{
    /// <summary>
    /// Interaction logic for AddProcess.xaml
    /// </summary>
    public partial class AddProcessWindow : Window
    {
        private AbstractProcess process;

        private ComputePlatform platform;

        private ComputeDevice device;

        public double K { get; set; }
        public double c { get; set; }
        public double alphaZ { get; set; }
        public double l { get; set; }
        public double T { get; set; }
        public double R { get; set; }

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
                if ((bool)testCheckBox.IsChecked) process.isForTest = true;
            }
            if ("yavn" == processType)
            {
                process = new YavnSchema(processName, new SolidColorBrush(selectedColor));
                process.initializeSchema(I, 1, N);
                if (YavnSchema.isStable(K, c, alphaZ, l, T, N, R, I))
                {
                    process.additionalName = "Схема устойчива";
                }
                else
                {
                    process.additionalName = "Схема неустойчива";
                }
                if ((bool)testCheckBox.IsChecked) process.isForTest = true;
            }
            if ("analytic" == processType)
            {
                process = new AnalitProcess(processName, new SolidColorBrush(selectedColor));
                process.initializeSchema(I, J, N);
                process.isForTest = true;

            }
            if ("parpismen" == processType)
            {
                int threads = Int32.Parse(parPismTextBox.Text);
                process = new ParallelChislSchema(processName, new SolidColorBrush(selectedColor));
                process.initializeSchema(I, J, N);
                (process as ParallelChislSchema).threadsN = threads;
                process.additionalName = "Потоков " + threads;
                if ((bool)testCheckBox.IsChecked) process.isForTest = true;
            }
            if ("openCL" == processType)
            {
                process = new ChislOpenCLI(processName, new SolidColorBrush(selectedColor));
                process.initializeSchema(I, J, N);
                (process as ChislOpenCLI).Platform = platform;
                (process as ChislOpenCLI).Device = device;
                process.additionalName = platform.Name + " " + device.Name;
                if ((bool)testCheckBox.IsChecked) process.isForTest = true;
            }


            this.Close();
        }

        private void processTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = processTypeBox.SelectedValue as ComboBoxItem;
            processNameTextbox.Text = selectedItem.Content as String;
            String processType = selectedItem.Name;
            parPismRow.Height = new GridLength(0);
            openCLRow.Height = new GridLength(0);
            yavnRow.Height = new GridLength(0);
            okButton.IsEnabled = true;
            if ("parpismen" == processType) selectParPismProcessType();
            if ("openCL" == processType) selectOpenCLProcessType();
            if ("yavn" == processType) selectYavnProcessType();
        }

        private void selectYavnProcessType()
        {
            yavnRow.Height = GridLength.Auto;
        }

        private void selectParPismProcessType()
        {
            parPismRow.Height = GridLength.Auto;
        }

        private void selectOpenCLProcessType()
        {
            okButton.IsEnabled = false;
            openCLRow.Height = GridLength.Auto;
            int i = 0;
            if (platform == null)
            {
                platformBox.IsEnabled = true;
                platformBox.Items.Clear();
                foreach (ComputePlatform platformA in ComputePlatform.Platforms)
                {
                    ComboBoxItem newItem = new ComboBoxItem();
                    newItem.Content = platformA.Name;
                    newItem.Name = "platform_" + i.ToString();
                    platformBox.Items.Add(newItem);
                    if (i == 0) platformBox.SelectedItem = newItem;
                    i++;
                }
            }
        }

        private void setDevices()
        {
            deviceBoxList.Items.Clear();
            if (platform != null)
            {
                deviceBoxList.IsEnabled = true;
                int i = 0;

                foreach (ComputeDevice deviceA in platform.Devices)
                {
                    ComboBoxItem listBoxItem = new ComboBoxItem();
                    listBoxItem.Content = deviceA.Name;
                    listBoxItem.Name = "device_" + i.ToString();
                    deviceBoxList.Items.Add(listBoxItem);
                    if (i == 0) deviceBoxList.SelectedIndex = 0;
                    i++;
                    
                }
            }
            else
            {
                deviceBoxList.IsEnabled = false;
            }
        }

        private void platformBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = platformBox.SelectedValue as ComboBoxItem;
            if (selectedItem != null)
            {

                String platfirmId = selectedItem.Name;
                int id = Int32.Parse(platfirmId.Substring(9));
                if (id >= 0)
                {
                    if (platform != ComputePlatform.Platforms[id])
                    {
                        platform = ComputePlatform.Platforms[id];
                        setDevices();
                    }
                }
            }

        }

        private void deviceBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = deviceBoxList.SelectedItem as ComboBoxItem;
                if (selectedItem != null)
                {
                    String deviceId = selectedItem.Name;
                    int id = Int32.Parse(deviceId.Substring(7));
                    if (id >= 0)
                    {
                        device = platform.Devices[id];
                    }
                }
            if (device!=null) okButton.IsEnabled = true;
            
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
                       
            if (YavnSchema.isStable(K,c,alphaZ,l,T,Int32.Parse(parametrN.Text),R,Int32.Parse(parametrI.Text)))
            {
                schemaUst.Content="Схема устойчива";
            }
            else 
            {
                schemaUst.Content = "Схема неустойчива";
            }
        }
    }
}
