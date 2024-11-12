using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Globalization;
using Bcfier.Data.Utils;

namespace Bcfier.Data.ValueConverters
{
  /// <summary>
  /// This avoids issues when deleting an image that is loaded by the UI
  /// </summary>
  [ValueConversion(typeof(String), typeof(BitmapImage))]
  public class PathToImageConv : IValueConverter
  {

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      try
      {
        if (value == null)
          return null;

        var path = value.ToString();
        return !string.IsNullOrEmpty(path) ? ImagingUtils.BitmapFromPath(path) : null;
      }
      catch { return null; }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
