using Bcfier.Bcf;
using Bcfier.Bcf.Bcf2;
using Bcfier.Bcf.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Tests
{
  public class TestViewModel
  {
    private static Markup BuildMarkupWithViewpoint(string viewpointGuid)
    {
      var markup = new Markup(DateTime.Now);
      if (viewpointGuid != null)
      {
        markup.Viewpoints.Add(new ViewPoint(false) { Guid = viewpointGuid });
      }
      return markup;
    }

    private static Comment BuildComment(string guid, string viewpointGuid)
    {
      return new Comment
      {
        Guid = guid,
        Comment1 = "c",
        Date = DateTime.Now,
        Viewpoint = viewpointGuid == null ? null : new CommentViewpoint { Guid = viewpointGuid }
      };
    }

    [Test]
    public void markup_vm_maps_children_from_model()
    {
      // given
      var model = BuildMarkupWithViewpoint("vp-1");
      model.Viewpoints[0].Snapshot = "snap.png";
      model.Topic.Title = "T";
      model.Comment.Add(BuildComment("c1", "vp-1"));

      // when
      var vm = MarkupVM.FromModel(model);

      // then
      Assert.That(vm.Model, Is.SameAs(model));
      Assert.That(vm.Topic.Title, Is.EqualTo("T"));
      Assert.That(vm.Viewpoints, Has.Count.EqualTo(1));
      Assert.That(vm.Viewpoints[0].Snapshot, Is.EqualTo("snap.png"));
      Assert.That(vm.Comment, Has.Count.EqualTo(1));
      Assert.That(vm.Comment[0].Model, Is.SameAs(model.Comment[0]));
    }

    [Test]
    public void markup_vm_viewcomments_groups_comments_by_viewpoint()
    {
      // given
      var model = BuildMarkupWithViewpoint("vp-1");
      model.Comment.Add(BuildComment("c1", "vp-1"));
      model.Comment.Add(BuildComment("c2", "other-vp"));
      var vm = MarkupVM.FromModel(model);

      // when
      var groups = vm.ViewComments;

      // then: one group for vp-1 (its comment) + one group for unlinked comments
      Assert.That(groups, Has.Count.EqualTo(2));
      var vpGroup = groups.First(g => g.Viewpoint != null);
      Assert.That(vpGroup.Viewpoint.Guid, Is.EqualTo("vp-1"));
      Assert.That(vpGroup.Comments.Select(c => c.Guid), Is.EqualTo(new[] { "c1" }));
      var emptyGroup = groups.First(g => g.Viewpoint == null);
      Assert.That(emptyGroup.Comments.Select(c => c.Guid), Is.EqualTo(new[] { "c2" }));
    }

    [Test]
    public void markup_vm_raises_viewcomments_changed_when_comment_added()
    {
      // given
      var vm = MarkupVM.FromModel(BuildMarkupWithViewpoint("vp-1"));
      var changed = new List<string>();
      vm.PropertyChanged += (s, e) => changed.Add(e.PropertyName);

      // when
      vm.Comment.Add(CommentVM.FromModel(BuildComment("c1", "vp-1")));

      // then
      Assert.That(changed, Does.Contain("ViewComments"));
    }

    [Test]
    public void markup_vm_raises_viewcomments_changed_when_viewpoint_added()
    {
      // given
      var vm = MarkupVM.FromModel(BuildMarkupWithViewpoint(null));
      var changed = new List<string>();
      vm.PropertyChanged += (s, e) => changed.Add(e.PropertyName);

      // when
      vm.Viewpoints.Add(ViewPointVM.FromModel(new ViewPoint(false) { Guid = "vp-new" }));

      // then
      Assert.That(changed, Does.Contain("ViewComments"));
    }

    [Test]
    public void topic_vm_proxies_scalars_and_option_collections()
    {
      // given
      var vm = TopicVM.FromModel(new Topic());
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
      var vm = ViewPointVM.FromModel(new ViewPoint(false));
      var changed = new List<string>();
      vm.PropertyChanged += (s, e) => changed.Add(e.PropertyName);

      // when
      vm.SnapshotPath = @"C:\temp\snap.png";

      // then
      Assert.That(vm.SnapshotPath, Is.EqualTo(@"C:\temp\snap.png"));
      Assert.That(changed, Contains.Item("SnapshotPath"));
    }

    [Test]
    public void bcf_file_vm_from_model_maps_issues_and_raises_selection_changed()
    {
      // given
      var model = new BcfFile();
      model.Issues.Add(new Markup(DateTime.Now));
      var vm = BcfFileVM.FromModel(model);
      var changed = new List<string>();
      vm.PropertyChanged += (s, e) => changed.Add(e.PropertyName);

      // then
      Assert.That(vm.Issues, Has.Count.EqualTo(1));
      Assert.That(vm.Issues[0].Model, Is.SameAs(model.Issues[0]));

      // when
      vm.SelectedIssue = vm.Issues[0];
      vm.HasBeenSaved = false;

      // then
      Assert.That(changed, Contains.Item("SelectedIssue"));
      Assert.That(changed, Contains.Item("HasBeenSaved"));
    }
  }
}