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
using DiplomWPF.Common.Comparators;

namespace DiplomWPF.Client.Components
{
    /// <summary>
    /// Interaction logic for ComparatorDataWindow.xaml
    /// </summary>
    public partial class ComparatorDataWindow : Window
    {

        public List<ComparatorData> compData { get; set; }

        public ComparatorDataWindow(List<ComparatorData> compData)
        {
            InitializeComponent();
            this.compData = compData;
            initData();
        }

        public void initData()
        {
            System.Collections.ObjectModel.ObservableCollection<ComparatorData> coll = new System.Collections.ObjectModel.ObservableCollection<ComparatorData>();
            foreach(ComparatorData cd in compData)
            {
                coll.Add(cd);
            }
            dataGrid.ItemsSource = coll;
        }
    }
}
