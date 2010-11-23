﻿using System;
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
                process.initializeSchema(N, I, J);
            }
            if ("yavn" == processType)
            {
                process = new YavnSchema(processName, new SolidColorBrush(selectedColor));
                process.initializeSchema(N, I, J);
            }
            this.Close();
        }
    }
}
