using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TopoSurf.DataModels;

namespace TopoSurf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainPage page;
        public MainWindow()
        {

            InitializeComponent();
            this.DataContext = new WindowViewModel(this);
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight - 4;
            this.MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth - 4;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (MainPage.leftMargin==22 && MainPage.rightMargin==125)
            {
                MainPage.leftMargin = 88;
                MainPage.rightMargin = 155;
            }
            else
            {
                MainPage.leftMargin = 22;
                MainPage.rightMargin = 125;
            }
            
        }
    }
}
