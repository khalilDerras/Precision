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
    /// Interaction logic for SaveAs.xaml
    /// </summary>
    public partial class SaveAs : Page
    {
        int back;
        public SaveAs()
        {
            InitializeComponent();
        }
        public SaveAs(int i)
        {
            InitializeComponent();
            back = i;
        }

        #region  Save as buttons
        private void saveAsAProject_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog open = new SaveFileDialog();
            if (open.ShowDialog() == true)
            {
                var src = new Uri(open.FileName);
                MainWindow.page.SaveThings(src.ToString().Substring(8));
            }
            for (int j = 0; j < back; j++) this.NavigationService.GoBack();
        }
        
        #endregion
    }
}
