﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

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
    private ExtEvntOpenView _Handler;
    private Renga.IApplication _App;

    public RengaWindow(Renga.IApplication app, ExtEvntOpenView handler)
    {
      InitializeComponent();
      Bcfier.LabelVersion.Content = "BCFier for Renga " +
                         System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
      _Handler = handler;
      _App = app;

      var applicationEvents = new Renga.ApplicationEventSource(_App);
      applicationEvents.BeforeProjectClose += (eventArgs) =>
      {
        if (Bcfier.TryCloseAllBcfs())
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
        if (Bcfier.SelectedBcf() == null)
          return;
        
        var view = e.Parameter as ViewPoint;
        if (view == null)
          return;
        
        _Handler.v = view.VisInfo;
        _Handler.Execute(_App);
      }
      catch (System.Exception ex)
      {
        Utils.ShowErrorMessageBox(LocValueGetter.Get("FailedViewOpening"), ex);
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
        if (Bcfier.SelectedBcf() == null)
          return;
        
        var issue = e.Parameter as Markup;
        if (issue == null)
        {
          MessageBox.Show(LocValueGetter.Get("NoIssue"), LocValueGetter.Get("Error"));
          return;
        }

        var rengaActiveView = _App.ActiveView;
        if (rengaActiveView.Type != Renga.ViewType.ViewType_View3D)
        {
          MessageBox.Show(LocValueGetter.Get("UnsupportedView"), LocValueGetter.Get("Info"));
          return;
        }

        var dialog = new AddViewRenga(
          issue, 
          Bcfier.SelectedBcf().TempPath, 
          _App.ActiveView as Renga.IScreenshotService);
        dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        dialog.ShowDialog();
        
        if (dialog.DialogResult.HasValue && dialog.DialogResult.Value)
        {
          //generate and set the visinfo
          issue.Viewpoints.Last().VisInfo = RengaView.GenerateViewpoint(_App);
          Bcfier.SelectedBcf().HasBeenSaved = false;
        }

      }
      catch (System.Exception ex)
      {
        Utils.ShowErrorMessageBox(LocValueGetter.Get("AddViewError"), ex);
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
      e.Cancel = !Bcfier.TryCloseAllBcfs();
    }
    #endregion

    //stats
    private void RengaWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
    }

  }
}