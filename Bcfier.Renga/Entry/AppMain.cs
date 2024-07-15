using System;
using System.IO;

using Renga;


namespace Bcfier.RengaPlugin.Entry
{
  public class AppMain : IPlugin
  {
    public bool Initialize(string plugInFolder)
    {
      var app = new Renga.Application();
      var ui = app.UI;

      var actionImage = ui.CreateImage();
      actionImage.LoadFromFile(Path.Combine(plugInFolder, "Assets/BCFierIcon16x16.png"));

      var action = ui.CreateAction();
      action.Icon = actionImage;
      action.ToolTip = "BCFier";

      var actionEvents = new Renga.ActionEventSource(action);
      actionEvents.Triggered += (s, e) =>
      {
        var mainCommand = new CmdMain();
        mainCommand.Execute(app);
      };

      var panelExtension = ui.CreateUIPanelExtension();
      panelExtension.AddToolButton(action);

      ui.AddExtensionToPrimaryPanel(panelExtension);

      return true;
    }

    public void Stop()
    {}
  }
}