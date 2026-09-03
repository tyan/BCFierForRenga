using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Bcfier.Bcf.Bcf2;

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
      return new BcfFileVM(model);
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