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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace TopoSurf.MenuPages
{
    /// <summary>
    /// Interaction logic for Open.xaml
    /// </summary>
    public partial class Open : Page
    {
        int back;
        public Open()
        {
            InitializeComponent();
        }
        public Open(int i)
        {
            InitializeComponent();
            back = i;
        }

        private void loadAFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            if (open.ShowDialog() == true)
            {
                var src = new Uri(open.FileName);
                MainWindow.page.LoadThings(src.ToString().Substring(8));
            }
            for (int j = 0; j < back; j++) this.NavigationService.GoBack();
        }
    }
}
