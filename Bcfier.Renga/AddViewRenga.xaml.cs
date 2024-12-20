﻿using System.IO;
using System.Windows;
using System.Windows.Controls;
using Bcfier.Bcf.Bcf2;
using Bcfier.Data.Utils;
using Renga;

namespace Bcfier.RengaPlugin
{
  /// <summary>
  /// Interaction logic for AddViewRenga.xaml
  /// </summary>
  public partial class AddViewRenga : Window
  {
    public AddViewRenga(Markup issue, string bcfTempFolder, Renga.IScreenshotService service)
    {
      this.InitializeComponent();
      AddViewControl.Issue = issue;
      AddViewControl.TempFolder = bcfTempFolder;

      var tempImg = Path.Combine(Path.GetTempPath(), "BCFier", Path.GetTempFileName() + ".png");
      var settings = service.CreateSettings();
      settings.Width = 960;
      settings.Height = 540;

      var image = service.MakeScreenshot(settings);
      try
      {
        image.SaveToFile(tempImg, Renga.ImageFormat.ImageFormat_PNG);

        AddViewControl.SnapshotImg.Source = ImagingUtils.ImageSourceFromPath(tempImg);
      }
      finally
      {
        File.Delete(tempImg);
      }      
    }
  }
}