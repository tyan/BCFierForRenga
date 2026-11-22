using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using Bcfier.Bcf.Model;
using Bcfier.Data.Utils;

namespace Bcfier.Bcf.ViewModel
{
  /// <summary>
  /// ViewModel around the model <see cref="BcfProject"/>. Owns the observable issues
  /// collection, selection, text-search filter and dirty flag, raising change
  /// notifications from here rather than from the model.
  /// </summary>
  public class BcfFileVM : INotifyPropertyChanged
  {
    public BcfProject Model { get; private set; }

    private ObservableCollectionVM<IssueVM, BcfIssue> _issues;
    private IssueVM _selectedIssue;
    private string _textSearch;
    private bool _hasBeenSaved;
    private ListCollectionView _view;

    public BcfFileVM(BcfProject model)
    {
      Model = model;
      _issues = new ObservableCollectionVM<IssueVM, BcfIssue>(model.Issues, IssueVM.FromModel, m => m.Model);
      _view = new ListCollectionView(_issues);
      HasBeenSaved = true;
    }

    public static BcfFileVM FromModel(BcfProject model)
    {
      var vm = new BcfFileVM(model);
      //recompute snapshot paths (UI state) from the model's relative snapshot names
      foreach (var issue in vm.Issues)
      {
        foreach (var view in issue.Viewpoints)
        {
          view.SnapshotPath = view.SnapshotFileName != null
            ? Path.Combine(model.TempPath, issue.Model.Topic.Guid, view.SnapshotFileName)
            : null;
        }
      }
      return vm;
    }

    public bool HasBeenSaved
    {
      get { return _hasBeenSaved; }
      set
      {
        _hasBeenSaved = value;
        NotifyPropertyChanged("HasBeenSaved");
      }
    }

    public Guid Id
    {
      get { return Model.Id; }
      set { Model.Id = value; NotifyPropertyChanged("Id"); }
    }

    public string Filename
    {
      get { return Model.Filename; }
      set { Model.Filename = value; NotifyPropertyChanged("Filename"); }
    }

    public string Fullname
    {
      get { return Model.Fullname; }
      set { Model.Fullname = value; NotifyPropertyChanged("Fullname"); }
    }

    public Guid ProjectId
    {
      get { return Model.ProjectId; }
      set { Model.ProjectId = value; }
    }

    public string ProjectName
    {
      get { return Model.ProjectName; }
      set { Model.ProjectName = value; }
    }

    public string TempPath
    {
      get { return Model.TempPath; }
      set { Model.TempPath = value; }
    }

    public ObservableCollectionVM<IssueVM, BcfIssue> Issues
    {
      get { return _issues; }
      set
      {
        _issues = value;
        _view = new ListCollectionView(value);
        NotifyPropertyChanged("Issues");
      }
    }

    public IssueVM SelectedIssue
    {
      get { return _selectedIssue; }
      set
      {
        _selectedIssue = value;
        NotifyPropertyChanged("SelectedIssue");
      }
    }

    public ICollectionView View
    {
      get { return _view; }
    }

    public string TextSearch
    {
      get { return _textSearch; }
      set
      {
        _textSearch = value;
        NotifyPropertyChanged("TextSearch");

        if (String.IsNullOrEmpty(value))
          View.Filter = null;
        else
          View.Filter = Filter;
      }
    }

    private bool Filter(object o)
    {
      var issue = o as IssueVM;
      if (issue == null)
        return false;

      var text = TextSearch?.ToLowerInvariant();
      if (string.IsNullOrEmpty(text))
        return true;

      Func<string, bool> containsText = item => item.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;

      if (containsText(issue.Topic.Title) || containsText(issue.Topic.Description))
        return true;

      if (issue.Comment != null && issue.Comment.Any(x => containsText(x.Comment)))
        return true;

      return false;
    }

    public void AddIssue(IssueVM issue)
    {
      Issues.Add(issue);
    }

    public void RemoveIssues(IEnumerable<IssueVM> selectetitems)
    {
      foreach (var item in selectetitems)
      {
        Utils.DeleteDirectory(Path.Combine(TempPath, item.Model.Topic.Guid));
        Issues.Remove(item);
      }
      HasBeenSaved = false;
    }

    public void RemoveComment(IEnumerable<CommentVM> selectetitems, IssueVM issue)
    {
      foreach (var item in selectetitems)
        issue.Comment.Remove(item);
      HasBeenSaved = false;
    }

    public void RemoveComment(CommentVM comment, IssueVM issue)
    {
      RemoveComment(new[] { comment }, issue);
    }

    public void RemoveView(ViewPointVM view, IssueVM issue, bool delComm)
    {
      RemoveView(new[]{ view}, issue, delComm);
    }

    public void RemoveView(IEnumerable<ViewPointVM> selectetitems, IssueVM issue, bool delComm)
    {
      foreach (var item in selectetitems)
      {
        if (item.ViewpointFileName != null &&
            File.Exists(Path.Combine(TempPath, issue.Model.Topic.Guid, item.ViewpointFileName)))
          File.Delete(Path.Combine(TempPath, issue.Model.Topic.Guid, item.ViewpointFileName));
        if (File.Exists(item.SnapshotPath))
          File.Delete(item.SnapshotPath);

        var guid = item.Guid;
        issue.Viewpoints.Remove(item);
        //remove comments associated with that view
        var viewcomments = issue.Comment.Where(x => x.ViewpointGuid.HasValue && x.ViewpointGuid.Value == guid).ToList();

        if (!viewcomments.Any())
          continue;

        foreach (var viewcomm in viewcomments)
        {
          if (delComm)
            issue.Comment.Remove(viewcomm);
          else
            viewcomm.ViewpointGuid = null;
        }
      }
      HasBeenSaved = false;
    }

    // While there are no events in BCFFile so Merge should be maid over VM entities to notify UI elements.
    public void MergeBcfFile(IEnumerable<BcfFileVM> bcfFiles)
    {
      // TODO: create directory with synchroniously with new file
      // See: https://github.com/tyan/BCFierForRenga/issues/55
      if (!Directory.Exists(TempPath))
        return;

      foreach (var bcf in bcfFiles)
      {
        foreach (var mergedIssue in bcf.Issues)
        {
          //it's a new issue
          if (!Issues.Any(x => x.Model.Topic.Guid == mergedIssue.Model.Topic.Guid))
          {
            string sourceDir = Path.Combine(bcf.TempPath, mergedIssue.Model.Topic.Guid);
            string destDir = Path.Combine(TempPath, mergedIssue.Model.Topic.Guid);

            Directory.Move(sourceDir, destDir);
            //update path set for binding
            foreach (var view in mergedIssue.Viewpoints)
            {
              view.SnapshotPath = Path.Combine(TempPath, mergedIssue.Model.Topic.Guid, view.SnapshotFileName);
            }
            Issues.Add(mergedIssue);
          }
          //it exists, let's loop comments and views
          else
          {
            var issue = Issues.First(x => x.Model.Topic.Guid == mergedIssue.Model.Topic.Guid);
            var newComments = mergedIssue.Comment.Where(x => issue.Comment.All(y => y.Guid != x.Guid)).ToList();
            if (newComments.Any())
            {
              var allComments = issue.Comment.Concat(newComments)
                .OrderByDescending(x => x.Date)
                .ToList();

              issue.Comment.Clear();

              foreach (var comment in allComments)
                issue.Comment.Add(comment);
            }
              
            var newViews = mergedIssue.Viewpoints.Where(x => issue.Viewpoints.All(y => y.Guid != x.Guid)).ToList();
            if (newViews.Any())
              foreach (var newView in newViews)
              {
                //to avoid conflicts in case both contain a snapshot.png or viewpoint.bcfv
                //img to be merged
                string sourceFile = newView.SnapshotPath;
                //assign new safe name based on guid
                newView.SnapshotFileName = newView.Guid + ".png";
                //set new temp path for binding
                newView.SnapshotPath = Path.Combine(TempPath, issue.Model.Topic.Guid, newView.SnapshotFileName);
                //assign new safe name based on guid
                newView.ViewpointFileName = newView.Guid + ".bcfv";
                if (!string.IsNullOrEmpty(sourceFile))
                  File.Move(sourceFile, newView.SnapshotPath);
                issue.Viewpoints.Add(newView);
              }
          }
        }
        Utils.DeleteDirectory(bcf.TempPath);
      }
      HasBeenSaved = false;
    }

    [field: NonSerialized]
    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged(String info)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(info));
      }
    }
  }
}
