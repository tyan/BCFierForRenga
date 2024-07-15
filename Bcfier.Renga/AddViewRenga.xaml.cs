using System.IO;
using System.Windows;
using System.Windows.Controls;
using Bcfier.Bcf.Bcf2;
using Renga;

namespace Bcfier.RengaPlugin
{
  /// <summary>
  /// Interaction logic for AddViewRenga.xaml
  /// </summary>
  public partial class AddViewRenga : Window
  {

    public AddViewRenga(Markup issue, string bcfTempFolder, Renga.IApplication app)
    {
      try
      {
        this.InitializeComponent();
        AddViewControl.Issue = issue;
        AddViewControl.TempFolder = bcfTempFolder;
        AddViewControl.TextBlockInfo.Text = "3D/2D information of the current view will be included in the viewpoint";

        GetRengaSnapshot(app);
      }
      catch (System.Exception ex1)
      {
        app.UI.ShowMessageBox(Renga.MessageIcon.MessageIcon_Error, "Error!", ex1.Message);
      }
    }

    private void GetRengaSnapshot(Renga.IApplication app)
    {
      try
      {
        string tempImg = Path.Combine(Path.GetTempPath(), "BCFier", Path.GetTempFileName() + ".png");

        var service = app.ActiveView as Renga.IScreenshotService;
        
        var settings = service.CreateSettings();
        settings.Width = 960;
        settings.Height = 540;

        var image = service.MakeScreenshot(settings);
        image.SaveToFile(tempImg, Renga.ImageFormat.ImageFormat_PNG);

        AddViewControl.AddViewpoint(tempImg);

        File.Delete(tempImg);
      }
      catch (System.Exception ex1)
      {
        app.UI.ShowMessageBox(Renga.MessageIcon.MessageIcon_Error, "Error!", ex1.Message);
      }
    }    
  }
}