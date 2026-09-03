using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Bcfier.Bcf.Bcf2;

namespace Bcfier.Bcf.ViewModel
{
  /// <summary>
  /// ViewModel around the model <see cref="Markup"/>. Owns observable collections
  /// of viewpoints and comments and the computed <see cref="ViewComments"/> grouping,
  /// raising change notifications from here rather than from the model.
  /// </summary>
  public class MarkupVM : INotifyPropertyChanged
  {
    public Markup Model { get; private set; }

    public TopicVM Topic { get; private set; }
    public ObservableCollection<ViewPointVM> Viewpoints { get; private set; }
    public ObservableCollection<CommentVM> Comment { get; private set; }

    public MarkupVM(Markup model)
    {
      Model = model;
      Topic = model.Topic != null ? TopicVM.FromModel(model.Topic) : null;
      Viewpoints = new ObservableCollection<ViewPointVM>(
        (model.Viewpoints ?? new ObservableCollection<ViewPoint>()).Select(ViewPointVM.FromModel));
      Comment = new ObservableCollection<CommentVM>(
        (model.Comment ?? new ObservableCollection<Comment>()).Select(CommentVM.FromModel));

      //when Views or comments change refresh the ViewComments grouping
      Viewpoints.CollectionChanged += OnChildrenChanged;
      Comment.CollectionChanged += OnChildrenChanged;
    }

    public static MarkupVM FromModel(Markup model)
    {
      return new MarkupVM(model);
    }

    private void OnChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      NotifyPropertyChanged("ViewComments");
    }

    /// <summary>
    /// Replaces the comments while keeping the same collection instance so that
    /// change notifications keep flowing.
    /// </summary>
    public void ReplaceComments(IEnumerable<CommentVM> comments)
    {
      Comment.Clear();
      foreach (var comment in comments)
        Comment.Add(comment);
    }

    /// <summary>
    /// Generates ViewCommentVM objects from Viewpoints and Comments dynamically.
    /// </summary>
    public ObservableCollection<ViewCommentVM> ViewComments
    {
      get
      {
        var viewCommentsField = new ObservableCollection<ViewCommentVM>();
        foreach (var viewpoint in Viewpoints)
        {
          var vc = new ViewCommentVM
          {
            Viewpoint = viewpoint,
            Comments = new ObservableCollection<CommentVM>(Comment.Where(x => x.Viewpoint != null && x.Viewpoint.Guid == viewpoint.Guid))
          };
          viewCommentsField.Add(vc);
        }
        var vcEmpty = new ViewCommentVM
        {
          Comments =
            new ObservableCollection<CommentVM>(Comment.Where(x => !Viewpoints.Any(v => x.Viewpoint != null && v.Guid == x.Viewpoint.Guid)))
        };
        viewCommentsField.Add(vcEmpty);
        return viewCommentsField;
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