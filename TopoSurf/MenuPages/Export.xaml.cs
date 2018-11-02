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
    /// Interaction logic for Export.xaml
    /// </summary>
    public partial class Export : Page
    {
        int back;
        Canvas c;
        public Export()
        {
            InitializeComponent();
        }
        public Export(Canvas canv,int i)
        {
            InitializeComponent();
            c = canv;
            back = i;
        }


        private void saveAsAProject_Click(object sender, RoutedEventArgs e)
        {

            SaveFileDialog save = new SaveFileDialog();
            if (save.ShowDialog() == true)
            {
                var src = new Uri(save.FileName);
                util.SaveCanvas(c, 96, src.ToString().Substring(8));
                
            }
            for(int j=0;j<back;j++) this.NavigationService.GoBack();
        }
    }
}
