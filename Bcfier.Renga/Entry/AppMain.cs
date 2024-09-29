using System;
using System.IO;
using System.Reflection;
using System.Windows;
using Renga;
using WPFLocalizeExtension.Engine;


namespace Bcfier.RengaPlugin.Entry
{
  public class AppMain : IPlugin
  {
    public bool Initialize(string plugInFolder)
    {
      var addinAssembly = Assembly.GetExecutingAssembly();

      AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
      {
        var missingName = new AssemblyName(e.Name);
        var missingPath = Path.Combine(plugInFolder, missingName.Name + ".dll");

        // If we find the DLL in the add-in folder, load and return it.
        if (File.Exists(missingPath))
          return Assembly.LoadFrom(missingPath);

        // nothing found
        return null;
      };

      var app = new Renga.Application();
      var ui = app.UI;

      var cultureName = app.GetCurrentLocale() == "ru_RU" ? "ru-RU" : "en-US";
      LocalizeDictionary.Instance.Culture = new System.Globalization.CultureInfo(cultureName);

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