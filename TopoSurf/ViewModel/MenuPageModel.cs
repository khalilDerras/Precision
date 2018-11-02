using System.Windows;
using System.Windows.Input;
using TopoSurf.DataModels;
using TopoSurf.Pages;

namespace TopoSurf.ViewModel
{
    class MenuPageModel : BaseViewModel
    {
        private MenuPage menuPage;

        public Menu _currentMenuPage = Menu.SaveAs;


        public Menu CurrentMenuPage
        {
            get
            {
                return _currentMenuPage;
            }
            set
            {
                _currentMenuPage = value;
                OnPropertyChanged("CurrentMenuPage");
            }
        }

        public MenuPageModel(MenuPage menuPage)
        {
            this.menuPage = menuPage;
        }
    }
}
