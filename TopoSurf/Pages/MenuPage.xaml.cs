using System;
using System.Windows;
using System.Windows.Controls;
using TopoSurf.MenuPages;
using TopoSurf.ViewModel;
using System.Windows.Media;
using Microsoft.Win32;
using System.Windows.Media.Imaging;



namespace TopoSurf.Pages
{
    /// <summary>
    /// Interaction logic for MenuPage.xaml
    /// </summary>
    public partial class MenuPage : Page
    {
        Canvas ca;
        ImageBrush ib = new ImageBrush();
        Image imag;
        int i=1;
        public MenuPage(Image c,Canvas canvas1)
        {
            InitializeComponent();
            imag = c;
            ca = canvas1;
            this.DataContext = new MenuPageModel(this);
        }
        public MenuPage()
        {
            InitializeComponent();
            this.DataContext = new MenuPageModel(this);
        }
        #region MenuPage buttons
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            for(int j=0;j<i;j++) this.NavigationService.GoBack();

        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            i++;
            this.NavigationService.Navigate(new MainPage());
            this.NavigationService.RemoveBackEntry();
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            i++;
            MenuFrame.NavigationService.Navigate(new SaveAs(i));
        }

        private void ImportBtn_Click(object sender, RoutedEventArgs e)
        {
            i++;
            MenuFrame.NavigationService.Navigate(new Import(i,imag));
        }

        private void OpenBtn_Click(object sender, RoutedEventArgs e)
        {
            i++;
            MenuFrame.NavigationService.Navigate(new Open(i));
        }

        private void PrintBtn_Click(object sender, RoutedEventArgs e)
        {
            i++;
            MenuFrame.NavigationService.Navigate(new Print(ca,i));

        }

        private void HelpBtn_Click(object sender, RoutedEventArgs e)
        {
            i++;
            MenuFrame.NavigationService.Navigate(new Help());
        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            i++;
            MenuFrame.NavigationService.Navigate(new Export(ca,i));
        }

        
        #endregion
        
    }

}
