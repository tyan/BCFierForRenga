using System;
using System.IO;
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
        var path = value?.ToString();
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
          return ImagingUtils.DefaultSnapshotImage();

        return ImagingUtils.BitmapFromPath(path);
      }
      catch { return ImagingUtils.DefaultSnapshotImage(); }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
