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

        public ProcessControl(AbstractProcess processIn)
        {
            this.process = processIn;
            InitializeComponent();
            processGroupBox.Header = process.processName;

        }
    }
}
