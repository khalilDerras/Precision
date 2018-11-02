using Microsoft.Win32;
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
    /// Interaction logic for Import.xaml
    /// </summary>
    public partial class Import : Page
    {
        int back;
        Image imag;
        public Import()
        {
            InitializeComponent();
        }
        public Import(int i,Image imagee)
        {
            InitializeComponent();
            back = i;
            imag = imagee;
        }
        private void Importimage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.bmp) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.bmp";
            if (d.ShowDialog() == true)
            {

                var src = new Uri(d.FileName);
                imag.Source = new BitmapImage(src);

            }
            for (int j = 0; j < back; j++) this.NavigationService.GoBack();
        }

        private void Blank_Click(object sender, RoutedEventArgs e)
        {
            imag.Source = null;
            for (int j = 0; j < back; j++) this.NavigationService.GoBack();
        }
    }
}
