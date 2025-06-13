using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ComputerClub.Admin
{
    /// <summary>
    /// Логика взаимодействия для ChartDetailWindow.xaml
    /// </summary>
    public partial class ChartDetailWindow : Window
    {
        public ChartDetailWindow()
        {
            InitializeComponent();
        }
        public ChartDetailWindow(PlotModel plotModel)
        {
            InitializeComponent();

            ChartPlotView.Model = plotModel;
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
