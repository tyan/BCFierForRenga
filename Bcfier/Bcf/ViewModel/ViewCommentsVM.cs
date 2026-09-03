using System.Collections.ObjectModel;

namespace Bcfier.Bcf.ViewModel
{
  /// <summary>
  /// ViewModel that groups a viewpoint with its comments for the report panel.
  /// Not part of BCF. Replaces the model-side <c>ViewComment</c> grouping helper.
  /// </summary>
  public class ViewCommentsVM
  {
    public ViewCommentsVM(ViewPointVM viewpoint, ObservableCollection<CommentVM> comments)
    {
      Viewpoint = viewpoint;
      Comments = comments;
    }

    public ViewPointVM Viewpoint { get; }
    public ObservableCollection<CommentVM> Comments { get; }
  }
}
