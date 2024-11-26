using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using Bcfier.Api;
using Bcfier.Bcf;
using Bcfier.Bcf.Bcf2;
using Bcfier.Data.Utils;
using Bcfier.Windows;
using Bcfier.Data;
using Version = System.Version;

namespace Bcfier.UserControls
{
  /// <summary>
  /// Main panel UI and logic that need to be used by all modules
  /// </summary>
  public partial class BcfierPanel : UserControl
  {
    //my data context
    private readonly BcfContainer _bcf = new BcfContainer();

    public BcfierPanel()
    {
      InitializeComponent();
      DataContext = _bcf;
      _bcf.UpdateDropdowns();
      //top menu buttons and events
      NewBcfBtn.Click += delegate { _bcf.NewFile(); OnAddIssue(null, null); };
      OpenBcfBtn.Click += delegate { _bcf.OpenFile(); _bcf.UpdateDropdowns(); };
      //OpenProjectBtn.Click += OnOpenWebProject;
      SaveBcfBtn.Click += delegate { _bcf.SaveFile(SelectedBcf()); };
      MergeBcfBtn.Click += delegate { _bcf.MergeFiles(SelectedBcf()); };
      SettingsBtn.Click += delegate
      {
        var s = new Settings();
        s.ShowDialog();
        //update bcfs with new statuses and types
        if (s.DialogResult.HasValue && s.DialogResult.Value)
        {
          _bcf.UpdateDropdowns();
        }

      };
      HelpBtn.Click += HelpBtnOnClick;
    }


    private bool AskAndSaveBcf(BcfFile bcf)
    {
      if (bcf.HasBeenSaved || !bcf.Issues.Any())
        return true;

      var answer = MessageBox.Show(bcf.Filename + " has been modified.\nDo you want to save changes?", "Save Report?",
      MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
      if (answer == MessageBoxResult.Yes)
      {
        _bcf.SaveFile(bcf);
        return true;
      }
      else if (answer == MessageBoxResult.No)
        return true;
      else
        return false;
    }

    #region commands
    private void OnDeleteIssues(object sender, ExecutedRoutedEventArgs e)
    {
      try
      {
        if (SelectedBcf() == null)
          return;

        var selItems = e.Parameter as IList;
        var issues = selItems.Cast<Markup>().ToList();
        if (!issues.Any())
        {
          Utils.ShowInfoMessageBox("No Issue selected.");
          return;
        }

        //Are you sure you want to delete comments? Number of comments to delete: 8

        var deleteIssuesCaption = "Delete issues";
        var deleteIssuesMessage = String.Format("Are you sure you want to delete issues?\nNumber of issues to delete: {0}", issues.Count);
        var answer = MessageBox.Show(deleteIssuesMessage, deleteIssuesCaption, MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (answer == MessageBoxResult.No)
          return;

        SelectedBcf().RemoveIssues(issues);

      }
      catch (System.Exception ex)
      {
        // Log exception
        Utils.ShowErrorMessageBox("Delete issue error.", ex);
      }
    }

    private void OnAddComment(object sender, ExecutedRoutedEventArgs e)
    {
      try
      {
        if (SelectedBcf() == null)
          return;
        var values = (object[])e.Parameter;
        var view = values[0] as ViewPoint;
        var issue = values[1] as Markup;
        var content = values[2].ToString();
        //var status = (values[3] == null) ? "" : values[3].ToString();
        //var verbalStatus = values[4].ToString();
        if (issue == null)
        {
          MessageBox.Show("No Issue selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
          return;
        }

        Comment c = new Comment();
        c.Guid = Guid.NewGuid().ToString();
        c.Comment1 = content;
        //c.Topic = new CommentTopic();
        //c.Topic.Guid = issue.Topic.Guid;
        c.Date = DateTime.Now;
        //c.VerbalStatus = verbalStatus;
        //c.Status = status;
        c.Author = Utils.GetUsername();

        c.Viewpoint = new CommentViewpoint();
        c.Viewpoint.Guid = (view != null) ? view.Guid : "";

        issue.Comment.Add(c);

        SelectedBcf().HasBeenSaved = false;
      }
      catch (System.Exception ex)
      {
        // Log exception
        Utils.ShowErrorMessageBox("Add comment error.", ex);
      }
    }
    private void OnDeleteComment(object sender, ExecutedRoutedEventArgs e)
    {
      try
      {
        if (SelectedBcf() == null)
          return;
        var values = (object[])e.Parameter;
        var comment = values[0] as Comment;
        //  var comments = selItems.Cast<Comment>().ToList();
        var issue = (Markup)values[1];
        if (issue == null)
        {
          MessageBox.Show("No Issue selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
          return;
        }
        if (comment == null)
        {
          MessageBox.Show("No Comment selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
          return;
        }
        MessageBoxResult answer = MessageBox.Show(
          "Are you sure you want to\nDelete this comment?",
           "Delete Comment?", MessageBoxButton.YesNo, MessageBoxImage.Question);
        //MessageBoxResult answer = MessageBox.Show(
        //  String.Format("Are you sure you want to\nDelete {0} Comment{1}?", comments.Count, (comments.Count > 1) ? "s" : ""),
        //   "Delete Issue?", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (answer == MessageBoxResult.No)
          return;

        SelectedBcf().RemoveComment(comment, issue);
      }
      catch (System.Exception ex)
      {
        // Log exception
        Utils.ShowErrorMessageBox("Delete comment error.", ex);
      }
    }

    private void OnDeleteView(object sender, ExecutedRoutedEventArgs e)
    {
      try
      {

        if (SelectedBcf() == null)
          return;
        var values = (object[])e.Parameter;
        var view = values[0] as ViewPoint;
        var issue = (Markup)values[1];
        if (issue == null)
        {
          MessageBox.Show("No Issue selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
          return;
        }
        if (view == null)
        {
          MessageBox.Show("No View selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
          return;
        }
        var delComm = true;

        MessageBoxResult answer = MessageBox.Show("Do you also want to delete the comments linked to the selected viewpoint?",
           "Delete Viewpoint's Comments?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

        if (answer == MessageBoxResult.Cancel)
          return;
        if (answer == MessageBoxResult.No)
          delComm = false;

        SelectedBcf().RemoveView(view, issue, delComm);
      }
      catch (System.Exception ex)
      {
        // Log exception
        Utils.ShowErrorMessageBox("Delete view error.", ex);
      }
    }
    private void OnAddIssue(object sender, ExecutedRoutedEventArgs e)
    {
      try
      {

        if (SelectedBcf() == null)
          return;

        var issue = new Markup(DateTime.Now);
        SelectedBcf().Issues.Add(issue);
        SelectedBcf().SelectedIssue = issue;

      }
      catch (System.Exception ex)
      {
        Utils.ShowErrorMessageBox("Add issue error.", ex);
      }
    }
    private void HasIssueSelected(object sender, CanExecuteRoutedEventArgs e)
    {
      if (SelectedBcf().SelectedIssue != null)
        e.CanExecute = true;
      else
        e.CanExecute = false;

    }
    private void OnOpenSnapshot(object sender, ExecutedRoutedEventArgs e)
    {
      try
      {
        var view = e.Parameter as ViewPoint;
        if (view == null || !File.Exists(view.SnapshotPath))
        {
          Utils.ShowErrorMessageBox("The selected Snapshot does not exist");
          return;
        }
        Process.Start(view.SnapshotPath);
      }
      catch (System.Exception ex)
      {
        Utils.ShowErrorMessageBox("Open snapshot error.", ex);
      }
    }

    // TODO: remove this
    private void OnOpenComponents(object sender, ExecutedRoutedEventArgs e)
    {
      try
      {
        var view = e.Parameter as ViewPoint;
        if (view == null)
        {
          MessageBox.Show("The selected ViewPoint is null", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
          return;
        }
        var dialog = new ComponentsList(view.VisInfo.Components);
        dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        dialog.Show();
      }
      catch (System.Exception)
      {
      }
    }
    private void OnCloseBcf(object sender, ExecutedRoutedEventArgs e)
    {
      try
      {
        var guid = Guid.Parse(e.Parameter.ToString());
        var bcfs = _bcf.BcfFiles.Where(x => x.Id.Equals(guid));
        if (!bcfs.Any())
          return;
        var bcf = bcfs.First();

        if (AskAndSaveBcf(bcf))
          _bcf.CloseFile(bcf);
      }
      catch (System.Exception ex)
      {
        Utils.ShowErrorMessageBox("Close BCF error.", ex);
      }
    }

    private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      var target = e.Source as Control;

      if (target != null)
      {
        e.CanExecute = true;
      }
      else
      {
        e.CanExecute = false;
      }
    }
    #endregion
    #region events 

    private void OnOpenWebProject(object sender, RoutedEventArgs routedEventArgs)
    {

    }

    public void BcfFileClicked(string path)
    {
      _bcf.OpenFile(path);
    }
    //prompt to save bcf
    //delete temp folders
    public bool TryCloseAllBcfs()
    {
      foreach (var bcf in _bcf.BcfFiles)
        // Something was not saved and user has discarded saving
        if (!AskAndSaveBcf(bcf))
          return false;

      _bcf.CloseAllFiles();
      return true;
    }
    private void HelpBtnOnClick(object sender, RoutedEventArgs routedEventArgs)
    {
      const string url = "https://github.com/tyan/BCFierForRenga/blob/master/USERGUIDE.md";
      try
      {
        Process.Start(url);
      }
      catch (Win32Exception)
      {
        Process.Start("IExplore.exe", url);
      }
    }

    #endregion

    #region drag&drop
    private void Window_DragEnter(object sender, DragEventArgs e)
    {
      whitespace.Visibility = Visibility.Visible;
    }
    private void Window_DragLeave(object sender, DragEventArgs e)
    {
      whitespace.Visibility = Visibility.Hidden;
    }
    private void Window_Drop(object sender, DragEventArgs e)
    {
      try
      {
        whitespace.Visibility = Visibility.Hidden;
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
          var files = (string[])e.Data.GetData(DataFormats.FileDrop);
          foreach (var f in files)
          {
            if (File.Exists(f))
              _bcf.OpenFile(f);
          }
        }
      }
      catch (System.Exception ex)
      {
        Utils.ShowErrorMessageBox("Open BCF error.", ex);
      }
    }
    private void Window_DragOver(object sender, DragEventArgs e)
    {
      try
      {
        var dropEnabled = true;

        if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
        {
          var filenames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
          if (filenames.Any(x => Path.GetExtension(x).ToUpperInvariant() != BcfSerializer.FileExtension.ToUpperInvariant()))
            dropEnabled = false;
        }
        else
          dropEnabled = false;

        if (!dropEnabled)
        {
          e.Effects = DragDropEffects.None;
          e.Handled = true;
        }
      }
      catch (System.Exception)
      {
      }
    }
    #endregion
    #region shortcuts
    public BcfFile SelectedBcf()
    {
      if (BcfTabControl.SelectedIndex == -1 || _bcf.BcfFiles.Count <= BcfTabControl.SelectedIndex)
        return null;
      return _bcf.BcfFiles.ElementAt(BcfTabControl.SelectedIndex);
    }
    #endregion


  }
}
