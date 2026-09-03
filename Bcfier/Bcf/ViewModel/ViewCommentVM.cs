using System.Collections.ObjectModel;

namespace Bcfier.Bcf.ViewModel
{
  /// <summary>
  /// ViewModel that groups a viewpoint with its comments for the report panel.
  /// Not part of BCF. Replaces the model-side <c>ViewComment</c> grouping helper.
  /// </summary>
  public class ViewCommentVM
  {
    public ViewPointVM Viewpoint { get; set; }
    public ObservableCollection<CommentVM> Comments { get; set; }
  }
}