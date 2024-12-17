using System;
using System.IO;
using System.Windows;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Threading;
using Bcfier.Bcf.Bcf2;
using WPFLocalizeExtension.Engine;
using Bcfier.Data.Utils;
using Bcfier.Localization;


namespace Bcfier.Win
{

  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();

      LocalizeDictionary.Instance.Culture = new System.Globalization.CultureInfo("en-US");

      Bcfier.LabelVersion.Content = "BCFier for Windows " +
                   System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

      string[] args = Environment.GetCommandLineArgs();
      if (args.Length > 1 && File.Exists(args[1]))
      {
        Bcfier.Dispatcher.BeginInvoke(DispatcherPriority.Background,
             new Action(() => Bcfier.BcfFileClicked(args[1])));
      }
    }

    /// <summary>
    /// passing event to the user control
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Window_Closing(object sender, CancelEventArgs e)
    {
      e.Cancel = !Bcfier.TryCloseAllBcfs();
    }

    #region commands


    private void OnAddView(object sender, ExecutedRoutedEventArgs e)
    {
      try
      {

        if (Bcfier.SelectedBcf() == null)
          return;
        var issue = e.Parameter as Markup;
        if (issue == null)
        {
          MessageBox.Show(LocValueGetter.Get("NoIssue"), LocValueGetter.Get("Error"), MessageBoxButton.OK, MessageBoxImage.Error);
          return;
        }

        var addView = new AddView(issue, Bcfier.SelectedBcf().TempPath);
        addView.ShowDialog();
        if (addView.DialogResult.HasValue && addView.DialogResult.Value)
          Bcfier.SelectedBcf().HasBeenSaved = false;

      }
      catch (System.Exception ex)
      {
        Utils.ShowErrorMessageBox(LocValueGetter.Get("AddViewError"), ex);
      }
    }
    #endregion

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
      //Task.Run(() =>
      //{
      //  StatHat.Post.EzCounter(@"hello@teocomi.com", "BCFierWinStart", 1);
      //});
    }
  }
}