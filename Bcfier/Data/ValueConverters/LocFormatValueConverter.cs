using System;
using System.Windows.Data;
using System.Globalization;
using Bcfier.Localization;

namespace Bcfier.Data.ValueConverters
{
  /// <summary>
  /// Returna  string formatted to have a number and a plural
  /// using %0% and %s% as wildcards
  /// </summary>
  [ValueConversion(typeof(Int16), typeof(String))]
  public class LocFormatValueConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var format = LocValueGetter.Get(parameter.ToString());
      if (format == null || value == null)
        return string.Empty;
      else
        return string.Format(LocValueGetter.Get(parameter.ToString()), value);
      //var count = (int)value;
      //var plural = (count == 1) ? "" : "s";

      //var text = "";
      //if (parameter != null)
      //  text = parameter.ToString();
      //text = text.Replace("%0%", count.ToString());
      //text = text.Replace("%s%", plural);


      //return text;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
