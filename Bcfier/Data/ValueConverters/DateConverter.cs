using System;
using System.Globalization;
using System.Windows.Data;
using WPFLocalizeExtension.Engine;


namespace Bcfier.Data.ValueConverters
{
  /// <summary>
  /// Converts a date to relative
  /// </summary>
  [ValueConversion(typeof(DateTime), typeof(String))]
  public class DateConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null)
        return "";

      try
      {
        var dateTime = System.Convert.ToDateTime(value.ToString());
        var timeSpan = DateTime.Now.Subtract(dateTime);
        if (timeSpan.Days < 1 && DateTime.Now.Date == dateTime.Date)
          return dateTime.ToString("t", LocalizeDictionary.CurrentCulture);
        else
          return dateTime.ToString("g", LocalizeDictionary.CurrentCulture);
      }
      catch (InvalidCastException)
      {
        return "";
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }


  }
}
