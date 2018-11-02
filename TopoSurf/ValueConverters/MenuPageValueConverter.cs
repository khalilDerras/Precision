using System;
using System.Diagnostics;
using System.Globalization;
using TopoSurf;
using TopoSurf.DataModels;
using TopoSurf.MenuPages;
using TopoSurf.Pages;

namespace TopoSurf
{
    public class MenuPageValueConverter : BaseValueConverter<MenuPageValueConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((Menu)value)
            {

                case Menu.Open:
                    return new Open();
                case Menu.SaveAs:
                    return new SaveAs();
                case Menu.Help:
                    return new Help();
                case Menu.Print:
                    return new Print();
                case Menu.Import:
                    return new Import();
                case Menu.Export:
                    return new Export();
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