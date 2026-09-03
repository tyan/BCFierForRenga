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
    public ObservableCollectionVM<ViewPointVM, ViewPoint> Viewpoints { get; private set; }
    public ObservableCollectionVM<CommentVM, Comment> Comment { get; private set; }

    public MarkupVM(Markup model)
    {
      Model = model;
      Topic = model.Topic != null ? TopicVM.FromModel(model.Topic) : null;
// The synced collections need a real model collection to mirror into. If the incoming
      // model has null collections (only possible via the parameterless Markup ctor), attach
      // an empty one to the model; serialization-wise empty == absent, so the write is idempotent.
      var modelViewpoints = model.Viewpoints ?? (model.Viewpoints = new List<ViewPoint>());
      var modelComments = model.Comment ?? (model.Comment = new List<Comment>());
      Viewpoints = new ObservableCollectionVM<ViewPointVM, ViewPoint>(modelViewpoints, ViewPointVM.FromModel, v => v.Model);
      Comment = new ObservableCollectionVM<CommentVM, Comment>(modelComments, CommentVM.FromModel, c => c.Model);

      //when Views or comments change refresh the ViewComments grouping
      Viewpoints.CollectionChanged += OnViewCommentsChanged;
      Comment.CollectionChanged += OnViewCommentsChanged;
    }

    public static MarkupVM FromModel(Markup model)
    {
      return new MarkupVM(model);
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
            x => x.Viewpoint != null && x.Viewpoint.Guid == viewpoint.Guid);
          var linkedCommentsCollection = new ObservableCollection<CommentVM>(linkedComments);
          var viewPointCommentsVM = new ViewCommentsVM(viewpoint, linkedCommentsCollection);

          result.Add(viewPointCommentsVM);
        }

        var unlinkedComments = Comment.Where(
          x => !Viewpoints.Any(v => x.Viewpoint != null && v.Guid == x.Viewpoint.Guid));
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
