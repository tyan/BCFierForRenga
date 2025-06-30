using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Renga;
using Bcfier.Bcf.Bcf2;
using Bcfier.RengaPlugin.Entry;
using System.ComponentModel;
using System.Threading.Tasks;
using Component = Bcfier.Bcf.Bcf2.Component;
using Point = Bcfier.Bcf.Bcf2.Point;
using Bcfier.Data.Utils;
using Bcfier.RengaPlugin.Data;
using Bcfier.Localization;


namespace Bcfier.RengaPlugin
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class RengaWindow : Window
  {
    private Renga.IApplication _App;

    public RengaWindow(Renga.IApplication app)
    {
      InitializeComponent();
      m_panel.LabelVersion.Content = "BCFier for Renga " +
                         System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
      
      _App = app;

      var applicationEvents = new Renga.ApplicationEventSource(_App);
      applicationEvents.BeforeProjectClose += (eventArgs) =>
      {
        if (m_panel.TryCloseAllBcfs())
          Close();
        else
          eventArgs.Prevent();
      };
    }

    #region commands
    /// <summary>
    /// Raises the External Event to accomplish a transaction in a modeless window
    /// http://help.autodesk.com/view/RVT/2014/ENU/?guid=GUID-0A0D656E-5C44-49E8-A891-6C29F88E35C0
    /// http://matteocominetti.com/starting-a-transaction-from-an-external-application-running-outside-of-api-context-is-not-allowed/
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnOpenView(object sender, ExecutedRoutedEventArgs e)
    {
      try
      {
        if (m_panel.SelectedBcf() == null)
          return;
        
        var view = e.Parameter as ViewPoint;
        if (view == null)
          return;

        ExtEvntOpenView.Execute(_App, view.VisInfo);
      }
      catch (System.Exception ex)
      {
        Bcfier.Data.Utils.Utils.ShowErrorMessageBox(LocValueGetter.Get("FailedViewOpening"), ex);
      }
    }
    /// <summary>
    /// Same as in the windows app, but here we generate a VisInfo that is attached to the view
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnAddView(object sender, ExecutedRoutedEventArgs e)
    {
      try
      {
        if (m_panel.SelectedBcf() == null)
          return;
        
        var issue = e.Parameter as Markup;
        if (issue == null)
        {
          MessageBox.Show(LocValueGetter.Get("NoIssue"), LocValueGetter.Get("Error"));
          return;
        }

        var supportedViews = new List<ViewType> { ViewType.ViewType_View3D, ViewType.ViewType_Assembly, ViewType.ViewType_Drawing };
        var rengaActiveView = _App.ActiveView;
        if (!supportedViews.Contains(rengaActiveView.Type))
        {
          MessageBox.Show(LocValueGetter.Get("UnsupportedView"), LocValueGetter.Get("Info"));
          return;
        }

        var dialog = new AddViewRenga(
          issue,
          m_panel.SelectedBcf().TempPath, 
          _App.ActiveView as Renga.IScreenshotService);
        dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        dialog.ShowDialog();
        
        if (dialog.DialogResult.HasValue && dialog.DialogResult.Value)
        {
          //generate and set the visinfo
          issue.Viewpoints.Last().VisInfo = RengaView.GenerateViewpoint(_App);
          m_panel.SelectedBcf().HasBeenSaved = false;
        }

      }
      catch (System.Exception ex)
      {
        Bcfier.Data.Utils.Utils.ShowErrorMessageBox(LocValueGetter.Get("AddViewError"), ex);
      }
    }
    #endregion

    #region private methods

    /// <summary>
    /// passing event to the user control
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Window_Closing(object sender, CancelEventArgs e)
    {
      e.Cancel = !m_panel.TryCloseAllBcfs();
    }
    #endregion

    //stats
    private void RengaWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
    }

  }
}