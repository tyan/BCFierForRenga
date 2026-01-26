using System.Reflection;
using System;
using System.Windows;
using Bcfier.Data.Utils;
using Bcfier.Localization;

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

        // We give the objects to the new dialog;  
        // The dialog becomes the owner responsible for disposing them, eventually.
        Window = new RengaWindow(app);
        Window.Show();
      }
      catch (Exception ex)
      {
        Utils.ShowErrorMessageBox(LocValueGetter.Get("UnknownError"), ex);
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