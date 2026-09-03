using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using Bcfier.Bcf.Bcf2;
using Bcfier.Data.Utils;

namespace Bcfier.Bcf.ViewModel
{
  /// <summary>
  /// ViewModel around the model <see cref="BcfFile"/>. Owns the observable issues
  /// collection, selection, text-search filter and dirty flag, raising change
  /// notifications from here rather than from the model.
  /// </summary>
  public class BcfFileVM : INotifyPropertyChanged
  {
    public BcfFile Model { get; private set; }

    private ObservableCollection<MarkupVM> _issues;
    private MarkupVM _selectedIssue;
    private string _textSearch;
    private bool _hasBeenSaved;
    private ListCollectionView _view;

    public BcfFileVM(BcfFile model)
    {
      Model = model;
      _issues = new ObservableCollection<MarkupVM>(model.Issues.Select(MarkupVM.FromModel));
      _view = new ListCollectionView(_issues);
      HasBeenSaved = true;
    }

    public static BcfFileVM FromModel(BcfFile model)
    {
      var vm = new BcfFileVM(model);
      //recompute snapshot paths (UI state) from the model's relative snapshot names
      foreach (var issue in vm.Issues)
      {
        if (issue.Model.Topic == null)
          continue;
        foreach (var view in issue.Viewpoints)
        {
          view.SnapshotPath = view.Snapshot != null
            ? Path.Combine(model.TempPath, issue.Model.Topic.Guid, view.Snapshot)
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

    public ObservableCollection<MarkupVM> Issues
    {
      get { return _issues; }
      set
      {
        _issues = value;
        _view = new ListCollectionView(value);
        NotifyPropertyChanged("Issues");
      }
    }

    public MarkupVM SelectedIssue
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
      var issue = o as MarkupVM;
      if (issue == null)
        return false;
      if (issue.Topic != null && ((issue.Topic.Title != null && issue.Topic.Title.ToLowerInvariant().Contains(TextSearch.ToLowerInvariant())) ||
          (issue.Topic.Description != null && issue.Topic.Description.ToLowerInvariant().Contains(TextSearch.ToLowerInvariant()))) ||
         issue.Comment != null && issue.Comment.Any(x => x.Comment1 != null && x.Comment1.ToLowerInvariant().Contains(TextSearch.ToLowerInvariant()))
        )
        return true;
      return false;
    }

    public void AddIssue(MarkupVM issue)
    {
      Issues.Add(issue);
      Model.Issues.Add(issue.Model);
    }

    public void RemoveIssues(IEnumerable<MarkupVM> selectetitems)
    {
      foreach (var item in selectetitems)
      {
        Utils.DeleteDirectory(Path.Combine(TempPath, item.Model.Topic.Guid));
        Issues.Remove(item);
        Model.Issues.Remove(item.Model);
      }
      HasBeenSaved = false;
    }

    public void RemoveComment(IEnumerable<CommentVM> selectetitems, MarkupVM issue)
    {
      foreach (var item in selectetitems)
      {
        issue.Comment.Remove(item);
        issue.Model.Comment.Remove(item.Model);
      }
      HasBeenSaved = false;
    }
    public void RemoveComment(CommentVM comment, MarkupVM issue)
    {
      issue.Comment.Remove(comment);
      issue.Model.Comment.Remove(comment.Model);
      HasBeenSaved = false;
    }
    public void RemoveView(ViewPointVM view, MarkupVM issue, bool delComm)
    {

      if (File.Exists(Path.Combine(TempPath, issue.Model.Topic.Guid, view.Viewpoint)))
        File.Delete(Path.Combine(TempPath, issue.Model.Topic.Guid, view.Viewpoint));
      if (File.Exists(view.SnapshotPath))
        File.Delete(view.SnapshotPath);

      var guid = view.Guid;
      issue.Viewpoints.Remove(view);
      issue.Model.Viewpoints.Remove(view.Model);
      //remove comments associated with that view
      var viewcomments = issue.Comment.Where(x => x.Viewpoint != null && x.Viewpoint.Guid == guid).ToList();

      if (!viewcomments.Any())
      {
        HasBeenSaved = false;
        return;
      }

      foreach (var viewcomm in viewcomments)
      {
        if (delComm)
        {
          issue.Comment.Remove(viewcomm);
          issue.Model.Comment.Remove(viewcomm.Model);
        }
        else
        {
          viewcomm.Viewpoint = null;
          viewcomm.Model.Viewpoint = null;
        }
      }

      HasBeenSaved = false;
    }
    public void RemoveView(IEnumerable<ViewPointVM> selectetitems, MarkupVM issue, bool delComm)
    {
      foreach (var item in selectetitems)
      {
        if (File.Exists(Path.Combine(TempPath, issue.Model.Topic.Guid, item.Viewpoint)))
          File.Delete(Path.Combine(TempPath, issue.Model.Topic.Guid, item.Viewpoint));
        if (File.Exists(item.SnapshotPath))
          File.Delete(item.SnapshotPath);

        var guid = item.Guid;
        issue.Viewpoints.Remove(item);
        issue.Model.Viewpoints.Remove(item.Model);
        //remove comments associated with that view
        var viewcomments = issue.Comment.Where(x => x.Viewpoint != null && x.Viewpoint.Guid == guid).ToList();
        foreach (var viewcomm in viewcomments)
        {
          if (delComm)
          {
            issue.Comment.Remove(viewcomm);
            issue.Model.Comment.Remove(viewcomm.Model);
          }
          else
          {
            viewcomm.Viewpoint = null;
            viewcomm.Model.Viewpoint = null;
          }
        }
      }
      HasBeenSaved = false;
    }

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
          if (!Issues.Any(x => x.Model.Topic != null && mergedIssue.Model.Topic != null && x.Model.Topic.Guid == mergedIssue.Model.Topic.Guid))
          {
            string sourceDir = Path.Combine(bcf.TempPath, mergedIssue.Model.Topic.Guid);
            string destDir = Path.Combine(TempPath, mergedIssue.Model.Topic.Guid);

            Directory.Move(sourceDir, destDir);
            //update path set for binding
            foreach (var view in mergedIssue.Viewpoints)
            {
              view.SnapshotPath = Path.Combine(TempPath, mergedIssue.Model.Topic.Guid, view.Snapshot);
            }
            Issues.Add(mergedIssue);
            Model.Issues.Add(mergedIssue.Model);
          }
          //it exists, let's loop comments and views
          else
          {
            var issue = Issues.First(x => x.Model.Topic.Guid == mergedIssue.Model.Topic.Guid);
            var newComments = mergedIssue.Comment.Where(x => issue.Comment.All(y => y.Guid != x.Guid)).ToList();
            if (newComments.Any())
              foreach (var newComment in newComments)
              {
                issue.Comment.Add(newComment);
                issue.Model.Comment.Add(newComment.Model);
              }
            //sort comments
            issue.ReplaceComments(issue.Comment.OrderByDescending(x => x.Date));
            issue.Model.Comment = issue.Model.Comment.OrderByDescending(x => x.Date).ToList();

            var newViews = mergedIssue.Viewpoints.Where(x => issue.Viewpoints.All(y => y.Guid != x.Guid)).ToList();
            if (newViews.Any())
              foreach (var newView in newViews)
              {
                //to avoid conflicts in case both contain a snapshot.png or viewpoint.bcfv
                //img to be merged
                string sourceFile = newView.SnapshotPath;
                //assign new safe name based on guid
                newView.Snapshot = newView.Guid + ".png";
                //set new temp path for binding
                newView.SnapshotPath = Path.Combine(TempPath, issue.Model.Topic.Guid, newView.Snapshot);
                //assign new safe name based on guid
                newView.Viewpoint = newView.Guid + ".bcfv";
                File.Move(sourceFile, newView.SnapshotPath);
                issue.Viewpoints.Add(newView);
                issue.Model.Viewpoints.Add(newView.Model);
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