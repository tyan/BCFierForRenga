using System;


namespace Bcfier.RengaPlugin.Entry
{
  public class CmdMain
  {
    private static bool _isRunning;
    private static ExtAppBcfier _extAppBcfier;

    public void Execute(Renga.IApplication app)
    {
      if (_isRunning && _extAppBcfier != null && _extAppBcfier.Window.IsLoaded)
      {
        _extAppBcfier.Focus();
        return;
      }

      _isRunning = true;
      _extAppBcfier = new ExtAppBcfier();
      _extAppBcfier.ShowForm(app);
    }
  }
}