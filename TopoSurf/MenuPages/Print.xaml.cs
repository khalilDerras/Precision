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

namespace TopoSurf.MenuPages
{
    /// <summary>
    /// Interaction logic for Print.xaml
    /// </summary>
    public partial class Print : Page
    {
        int back;
        Canvas c;
        public Print()
        {
            InitializeComponent();
        }
        public Print(Canvas Can,int i)
        {
            InitializeComponent();
            c = Can;
            back = i;
        }
        private void _2DPrint_Click(object sender, RoutedEventArgs e)
        {
            
            util.PrintCanvas( c , 96);
            for(int j=0;j<back;j++) this.NavigationService.GoBack();
        }    }
}
