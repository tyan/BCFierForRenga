using Bcfier.Bcf;
using Bcfier.Bcf.Model;
using Bcfier.Bcf.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Tests
{
  public class TestViewModel
  {
    private static readonly Guid vp1Id = Guid.Parse("3cc48663-ca13-4866-b90f-72bf53dabb9e");
    private static readonly Guid vp2Id = Guid.Parse("d917f96a-d460-4a15-9541-469b64229887");
    private static readonly Guid comment1Id = Guid.Parse("d6c8d1de-e197-4657-b7ae-6ea03b7f4a00");
    private static readonly Guid comment2Id = Guid.Parse("ba48f036-489c-4d4e-9340-8efd18f62b57");

    private string _baseTemp;

    [SetUp]
    public void SetUp()
    {
      _baseTemp = Path.Combine(Path.GetTempPath(), "BCFierRemoveTests", Guid.NewGuid().ToString());
      Directory.CreateDirectory(_baseTemp);
    }

    [TearDown]
    public void TearDown()
    {
      if (Directory.Exists(_baseTemp))
        Directory.Delete(_baseTemp, true);
    }

    private static BcfIssue BuildIssueWithViewpoint(Guid? viewpointGuid)
    {
      var issue = new BcfIssue(DateTime.Now);
      if (viewpointGuid.HasValue)
      {
        issue.Viewpoints.Add(new BcfViewpoint(false) { Guid = viewpointGuid.Value });
      }
      return issue;
    }

    private static BcfComment BuildComment(Guid guid, string comment, Guid? viewpointGuid)
    {
      return new BcfComment
      {
        Guid = guid,
        Comment = comment,
        Date = DateTime.Now,
        ViewpointGuid = viewpointGuid
      };
    }

    private static BcfFileVM CreateBcf(string tempPath)
    {
      Directory.CreateDirectory(tempPath);
      return BcfFileVM.FromModel(new BcfProject { TempPath = tempPath });
    }

    private static IssueVM AddIssue(BcfFileVM bcf, Guid topicGuid)
    {
      var issue = new BcfIssue(DateTime.Now);
      issue.Topic.Guid = topicGuid.ToString();
      var vm = IssueVM.FromModel(issue);
      bcf.Issues.Add(vm);
      return vm;
    }

    [Test]
    public void issue_vm_maps_fields_from_model()
    {
      // given
      var model = BuildIssueWithViewpoint(vp1Id);
      model.Viewpoints[0].SnapshotFileName = "snap.png";
      model.Topic.Title = "Title";
      model.Comments.Add(BuildComment(comment1Id, "Comment string", vp1Id));

      // when
      var vm = IssueVM.FromModel(model);

      // then
      Assert.That(vm.Model, Is.SameAs(model));
      Assert.That(vm.Topic.Title, Is.EqualTo("Title"));
      Assert.That(vm.Viewpoints, Has.Count.EqualTo(1));
      Assert.That(vm.Viewpoints[0].SnapshotFileName, Is.EqualTo("snap.png"));
      Assert.That(vm.Comment, Has.Count.EqualTo(1));
      Assert.That(vm.Comment[0].Model.Comment, Is.SameAs(model.Comments[0].Comment));
    }

    [Test]
    public void issue_vm_viewcomments_groups_comments_by_viewpoint()
    {
      // given
      var model = BuildIssueWithViewpoint(vp1Id);
      model.Comments.Add(BuildComment(comment1Id, "Comment1", vp1Id));
      model.Comments.Add(BuildComment(comment2Id, "Comment2", null));
      var vm = IssueVM.FromModel(model);

      // when
      var groups = vm.ViewComments;

      // then
      // one group for vp1 (its comment) + one group for unlinked comments
      Assert.That(groups, Has.Count.EqualTo(2));
      // check viewpoint group
      var vpGroup = groups.First(g => g.Viewpoint != null);
      Assert.That(vpGroup.Viewpoint.Guid, Is.EqualTo(vp1Id));
      Assert.That(vpGroup.Comments.Select(c => c.Guid), Is.EqualTo(new[] { comment1Id }));
      // check root group
      var emptyGroup = groups.First(g => g.Viewpoint == null);
      Assert.That(emptyGroup.Comments.Select(c => c.Guid), Is.EqualTo(new[] { comment2Id }));
    }

    [Test]
    public void issue_vm_raises_viewcomments_changed_when_comment_added()
    {
      // given
      var vm = IssueVM.FromModel(BuildIssueWithViewpoint(vp1Id));
      var changedProperty = "";
      vm.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

      // when
      vm.Comment.Add(CommentVM.FromModel(BuildComment(comment1Id, "comment", vp1Id)));

      // then
      Assert.That(changedProperty, Is.EqualTo("ViewComments"));
    }

    [Test]
    public void issue_vm_raises_viewcomments_changed_when_viewpoint_added()
    {
      // given
      var vm = IssueVM.FromModel(BuildIssueWithViewpoint(null));
      var changedProperty = "";
      vm.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

      // when
      vm.Viewpoints.Add(ViewPointVM.FromModel(new BcfViewpoint(false) { Guid = vp1Id }));

      // then
      Assert.That(changedProperty, Is.EqualTo("ViewComments"));
    }

    [Test]
    public void issue_vm_viewpoints_add_mirrors_to_model()
    {
      // given
      var model = BuildIssueWithViewpoint(vp1Id);
      var vm = IssueVM.FromModel(model);
      var viewPoint = ViewPointVM.FromModel(new BcfViewpoint(false) { Guid = vp2Id });

      // when
      vm.Viewpoints.Add(viewPoint);

      // then
      Assert.That(vm.Viewpoints, Has.Count.EqualTo(2));
      Assert.That(vm.Model.Viewpoints, Has.Count.EqualTo(2));
      Assert.That(vm.Model.Viewpoints.Last(), Is.SameAs(viewPoint.Model));
    }

    [Test]
    public void issue_vm_comment_remove_mirrors_to_model()
    {
      // given
      var model = BuildIssueWithViewpoint(vp1Id);
      model.Comments.Add(BuildComment(comment1Id, "Comment1", vp1Id));
      var vm = IssueVM.FromModel(model);
      var comment = vm.Comment[0];

      // when
      vm.Comment.Remove(comment);

      // then
      Assert.That(vm.Comment, Is.Empty);
      Assert.That(vm.Model.Comments, Is.Empty);
    }

    [Test]
    public void issue_vm_collections_clear_mirrors_to_model()
    {
      // given
      var model = BuildIssueWithViewpoint(vp1Id);
      model.Comments.Add(BuildComment(comment1Id, "Comment1", vp1Id));
      model.Comments.Add(BuildComment(comment2Id, "Comment2", null));
      var vm = IssueVM.FromModel(model);

      // when
      vm.Comment.Clear();

      // then
      Assert.That(vm.Comment, Is.Empty);
      Assert.That(vm.Model.Comments, Is.Empty);
    }

    [Test]
    public void topic_vm_proxies_scalars_and_option_collections()
    {
      // given
      var vm = TopicVM.FromModel(new BcfTopic());
      vm.TopicStatusesCollection.Clear();
      vm.TopicTypesCollection.Clear();
      vm.TopicStatusesCollection.Add("Open");
      vm.TopicStatusesCollection.Add("Closed");
      vm.TopicTypesCollection.Add("Clash");

      // when
      vm.Title = "NewTitle";

      // then
      Assert.That(vm.Model.Title, Is.EqualTo("NewTitle"));
      Assert.That(vm.TopicStatusesCollection, Is.EquivalentTo(new[] { "Open", "Closed" }));
      Assert.That(vm.TopicTypesCollection, Is.EquivalentTo(new[] { "Clash" }));
    }

    [Test]
    public void viewpoint_vm_snapshot_path_raises_property_changed()
    {
      // given
      var vm = ViewPointVM.FromModel(new BcfViewpoint(false));
      var changedProperty = "";
      vm.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

      // when
      vm.SnapshotPath = @"C:\temp\snap.png";

      // then
      Assert.That(changedProperty, Is.EqualTo("SnapshotPath"));
    }

    [Test]
    public void bcf_file_vm_maps_issues_from_model()
    {
      // given
      var model = new BcfProject();
      model.Issues.Add(new BcfIssue(DateTime.Now));

      // when
      var vm = BcfFileVM.FromModel(model);
      
      // then
      Assert.That(vm.Issues, Has.Count.EqualTo(1));
      Assert.That(vm.Issues[0].Model, Is.SameAs(model.Issues[0]));
    }

    [Test]
    public void bcf_file_vm_raises_selection_changed()
    {
      // given
      var model = new BcfProject();
      model.Issues.Add(new BcfIssue(DateTime.Now));
      var vm = BcfFileVM.FromModel(model);
      var changedProperty = "";
      vm.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

      // when
      vm.SelectedIssue = vm.Issues[0];

      // then
      Assert.That(changedProperty, Is.EqualTo("SelectedIssue"));
    }

    [Test]
    public void bcf_file_vm_issues_add_mirrors_to_model()
    {
      // given
      var model = new BcfProject();
      var vm = BcfFileVM.FromModel(model);
      var issue = new IssueVM(new BcfIssue(DateTime.Now));

      // when
      vm.Issues.Add(issue);

      // then
      Assert.That(vm.Issues, Has.Count.EqualTo(1));
      Assert.That(vm.Model.Issues, Has.Count.EqualTo(1));
      Assert.That(vm.Model.Issues[0], Is.SameAs(issue.Model));
    }

    [Test]
    public void bcf_file_vm_raises_saved_event()
    {
      // given
      var model = new BcfProject();
      var vm = BcfFileVM.FromModel(model);

      var changedProperty = "";
      vm.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

      // when
      vm.HasBeenSaved = false;

      // then
      Assert.That(changedProperty, Is.EqualTo("HasBeenSaved"));
    }

    [Test]
    public void remove_comment_removes_from_vm_and_model()
    {
      // given
      var topicGuid = Guid.NewGuid();
      var toKeepCommentId = comment1Id;
      var toRemoveCommentId = comment2Id;
      var bcf = CreateBcf(Path.Combine(_baseTemp, "target"));
      var issue = AddIssue(bcf, topicGuid);
      var toKeepComment = CommentVM.FromModel(BuildComment(toKeepCommentId, "keep", null));
      var toRemoveComment = CommentVM.FromModel(BuildComment(toRemoveCommentId, "remove", null));
      issue.Comment.Add(toKeepComment);
      issue.Comment.Add(toRemoveComment);

      // when
      bcf.RemoveComment(toRemoveComment, issue);

      // then
      Assert.That(issue.Comment.Select(c => c.Guid), Is.EqualTo(new[] { toKeepCommentId }));
      Assert.That(issue.Model.Comments.Select(c => c.Guid), Is.EqualTo(new[] { toKeepCommentId }));
      Assert.That(bcf.HasBeenSaved, Is.False);
    }

    [Test]
    public void remove_comment_removes_multiple_comments()
    {
      // given
      var topicGuid = Guid.NewGuid();
      var bcf = CreateBcf(Path.Combine(_baseTemp, "target"));
      var issue = AddIssue(bcf, topicGuid);
      issue.Comment.Add(CommentVM.FromModel(BuildComment(comment1Id, "one", null)));
      issue.Comment.Add(CommentVM.FromModel(BuildComment(comment2Id, "two", null)));

      // when
      bcf.RemoveComment(issue.Comment.ToList(), issue);

      // then
      Assert.That(issue.Comment, Is.Empty);
      Assert.That(issue.Model.Comments, Is.Empty);
      Assert.That(bcf.HasBeenSaved, Is.False);
    }

    [Test]
    public void remove_view_removes_view_and_deletes_files()
    {
      // given
      var topicGuid = Guid.NewGuid();
      var targetTemp = Path.Combine(_baseTemp, "target");
      var bcf = CreateBcf(targetTemp);
      var issue = AddIssue(bcf, topicGuid);

      var view = new BcfViewpoint(false);
      var issueDir = Path.Combine(targetTemp, topicGuid.ToString());
      Directory.CreateDirectory(issueDir);

      var viewpointFile = Path.Combine(issueDir, view.ViewpointFileName);
      File.WriteAllText(viewpointFile, "view");

      var snapshotFile = Path.Combine(_baseTemp, view.SnapshotFileName);
      File.WriteAllText(snapshotFile, "img");

      var viewVm = ViewPointVM.FromModel(view);
      viewVm.SnapshotPath = snapshotFile;
      issue.Viewpoints.Add(viewVm);

      // when
      bcf.RemoveView(viewVm, issue, false);

      // then
      Assert.That(issue.Viewpoints, Is.Empty);
      Assert.That(issue.Model.Viewpoints, Is.Empty);
      Assert.That(File.Exists(viewpointFile), Is.False);
      Assert.That(File.Exists(snapshotFile), Is.False);
      Assert.That(bcf.HasBeenSaved, Is.False);
    }

    [Test]
    public void remove_view_with_delcomm_true_removes_linked_comments()
    {
      // given
      var topicGuid = Guid.NewGuid();
      var bcf = CreateBcf(Path.Combine(_baseTemp, "target"));
      var issue = AddIssue(bcf, topicGuid);

      var view = new BcfViewpoint(false);
      issue.Viewpoints.Add(ViewPointVM.FromModel(view));
      issue.Comment.Add(CommentVM.FromModel(BuildComment(comment1Id, "linked", view.Guid)));

      // when
      bcf.RemoveView(issue.Viewpoints[0], issue, true);

      // then
      Assert.That(issue.Viewpoints, Is.Empty);
      Assert.That(issue.Comment, Is.Empty);
      Assert.That(issue.Model.Comments, Is.Empty);
    }

    [Test]
    public void remove_view_with_delcomm_false_detaches_linked_comments()
    {
      // given
      var topicGuid = Guid.NewGuid();
      var bcf = CreateBcf(Path.Combine(_baseTemp, "target"));
      var issue = AddIssue(bcf, topicGuid);

      var view = new BcfViewpoint(false);
      issue.Viewpoints.Add(ViewPointVM.FromModel(view));
      issue.Comment.Add(CommentVM.FromModel(BuildComment(comment1Id, "linked", view.Guid)));

      // when
      bcf.RemoveView(issue.Viewpoints[0], issue, false);

      // then
      Assert.That(issue.Viewpoints, Is.Empty);
      Assert.That(issue.Comment, Has.Count.EqualTo(1));
      Assert.That(issue.Comment[0].ViewpointGuid, Is.Null);
      Assert.That(issue.Model.Comments.Single().ViewpointGuid, Is.Null);
    }

    [Test]
    public void remove_view_leaves_comments_of_other_views_intact()
    {
      // given
      var topicGuid = Guid.NewGuid();
      var bcf = CreateBcf(Path.Combine(_baseTemp, "target"));
      var issue = AddIssue(bcf, topicGuid);

      var keptView = new BcfViewpoint(false);
      var removedView = new BcfViewpoint(false);
      issue.Viewpoints.Add(ViewPointVM.FromModel(keptView));
      var removedViewVm = ViewPointVM.FromModel(removedView);
      issue.Viewpoints.Add(removedViewVm);
      issue.Comment.Add(CommentVM.FromModel(BuildComment(comment1Id, "linked", keptView.Guid)));

      // when
      bcf.RemoveView(removedViewVm, issue, true);

      // then
      Assert.That(issue.Viewpoints, Has.Count.EqualTo(1));
      Assert.That(issue.Viewpoints[0].Guid, Is.EqualTo(keptView.Guid));
      Assert.That(issue.Comment, Has.Count.EqualTo(1));
      Assert.That(issue.Comment[0].ViewpointGuid, Is.EqualTo(keptView.Guid));
    }

    [Test]
    public void remove_view_without_files_on_disk_does_not_throw()
    {
      // given
      var topicGuid = Guid.NewGuid();
      var bcf = CreateBcf(Path.Combine(_baseTemp, "target"));
      var issue = AddIssue(bcf, topicGuid);
      var view = new BcfViewpoint(false);
      issue.Viewpoints.Add(ViewPointVM.FromModel(view));

      // when / then
      Assert.That(() => bcf.RemoveView(issue.Viewpoints[0], issue, false), Throws.Nothing);
      Assert.That(issue.Viewpoints, Is.Empty);
      Assert.That(issue.Model.Viewpoints, Is.Empty);
    }
  }
}