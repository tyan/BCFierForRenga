using System.Reflection;
using System;
using System.Windows;
using Bcfier.Data.Utils;

namespace Bcfier.RengaPlugin.Entry
{
  public class ExtAppBcfier
  {
    // ModelessForm instance  
    public RengaWindow Window;

    #region public methods
    public void ShowForm(Renga.IApplication app)
    {
      try
      {
        // If we do not have a dialog yet, create and show it  
        if (Window != null) return;

        // A new handler to handle request posting by the dialog  
        var handler = new ExtEvntOpenView();

        // We give the objects to the new dialog;  
        // The dialog becomes the owner responsible for disposing them, eventually.
        Window = new RengaWindow(app, handler);
        Window.Show();
      }
      catch (Exception ex)
      {
        Utils.ShowErrorMessageBox("Unknown error.", ex);
      }
    }

    public void Focus()
    {
      if (Window == null) return;
      Window.Activate();
      Window.WindowState = WindowState.Normal;
    }
    #endregion
  }

}