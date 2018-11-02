using System;
using System.Diagnostics;
using System.Globalization;
using TopoSurf;
using TopoSurf.DataModels;
using TopoSurf.Pages;

namespace TopoSurf
{
    public class ApplicationPageValueConverter : BaseValueConverter<ApplicationPageValueConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((AppilicationPage)value)
            {
                
                case AppilicationPage.Main:
                    return new MainPage();
                case AppilicationPage.Menu:
                    return new MenuPage();
                default:
                    Debugger.Break();
                    return null;
            }
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
