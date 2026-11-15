using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Bcfier.Bcf.Model;

namespace Bcfier.Bcf.ViewModel
{
  /// <summary>
  /// ViewModel around the model <see cref="BcfIssue"/>. Owns observable collections
  /// of viewpoints and comments and the computed <see cref="ViewComments"/> grouping,
  /// raising change notifications from here rather than from the model.
  /// </summary>
  public class IssueVM : INotifyPropertyChanged
  {
    public BcfIssue Model { get; private set; }

    public TopicVM Topic { get; private set; }
    public ObservableCollectionVM<ViewPointVM, BcfViewpoint> Viewpoints { get; private set; }
    public ObservableCollectionVM<CommentVM, BcfComment> Comment { get; private set; }

    public IssueVM(BcfIssue model)
    {
      Model = model;
      Topic = TopicVM.FromModel(model.Topic);
      Viewpoints = new ObservableCollectionVM<ViewPointVM, BcfViewpoint>(model.Viewpoints, ViewPointVM.FromModel, v => v.Model);
      Comment = new ObservableCollectionVM<CommentVM, BcfComment>(model.Comments, CommentVM.FromModel, c => c.Model);

      //when Views or comments change refresh the ViewComments grouping
      Viewpoints.CollectionChanged += OnViewCommentsChanged;
      Comment.CollectionChanged += OnViewCommentsChanged;
    }

    public static IssueVM FromModel(BcfIssue model)
    {
      return new IssueVM(model);
    }

    private void OnViewCommentsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      NotifyPropertyChanged("ViewComments");
    }

    /// <summary>
    /// Generates ViewCommentVM objects from Viewpoints and Comments dynamically.
    /// </summary>
    public ObservableCollection<ViewCommentsVM> ViewComments
    {
      get
      {
        var result = new ObservableCollection<ViewCommentsVM>();

        foreach (var viewpoint in Viewpoints)
        {
          var linkedComments = Comment.Where(
            x => x.ViewpointGuid.HasValue && x.ViewpointGuid.Value == viewpoint.Guid);
          var linkedCommentsCollection = new ObservableCollection<CommentVM>(linkedComments);
          var viewPointCommentsVM = new ViewCommentsVM(viewpoint, linkedCommentsCollection);

          result.Add(viewPointCommentsVM);
        }

        var unlinkedComments = Comment.Where(
          x => !x.ViewpointGuid.HasValue || !Viewpoints.Any(v => v.Guid == x.ViewpointGuid.Value));
        var unlinkedCommentsCollection = new ObservableCollection<CommentVM>(unlinkedComments);
        result.Add(new ViewCommentsVM(null, unlinkedCommentsCollection));

        return result;
      }
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