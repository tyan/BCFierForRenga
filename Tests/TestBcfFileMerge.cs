using Bcfier.Bcf;
using Bcfier.Bcf.Bcf2;
using Bcfier.Bcf.ViewModel;
using System;
using System.IO;
using System.Linq;

namespace Tests
{
  public class TestBcfFileMerge
  {
    private string _baseTemp;

    [SetUp]
    public void SetUp()
    {
      _baseTemp = Path.Combine(Path.GetTempPath(), "BCFierMergeTests", Guid.NewGuid().ToString());
      Directory.CreateDirectory(_baseTemp);
    }

    [TearDown]
    public void TearDown()
    {
      if (Directory.Exists(_baseTemp))
        Directory.Delete(_baseTemp, true);
    }

    private static BcfFileVM CreateBcf(string tempPath, bool createDirectory = true)
    {
      if (createDirectory)
        Directory.CreateDirectory(tempPath);
      return BcfFileVM.FromModel(new BcfFile { TempPath = tempPath });
    }

    private (BcfFileVM Target, BcfFileVM Source, string TargetTemp, string SourceTemp) CreateTargetAndSource()
    {
      var targetTemp = Path.Combine(_baseTemp, "target");
      var sourceTemp = Path.Combine(_baseTemp, "source");
      return (CreateBcf(targetTemp), CreateBcf(sourceTemp), targetTemp, sourceTemp);
    }

    private static MarkupVM AddIssue(BcfFileVM bcf, Guid topicGuid)
    {
      var markup = new Markup(DateTime.Now);
      markup.Topic.Guid = topicGuid.ToString();
      var vm = MarkupVM.FromModel(markup);
      bcf.Issues.Add(vm);
      return vm;
    }

    private static Comment BuildComment(Guid guid, string text, DateTime date)
    {
      return new Comment { Guid = guid.ToString(), Comment1 = text, Date = date };
    }

    private static ViewPointVM AddView(BcfFileVM bcf, MarkupVM issue, string? snapshotName = null, string? viewpointName = null)
    {
      var view = new ViewPoint(false);
      if (snapshotName != null)
        view.Snapshot = snapshotName;
      if (viewpointName != null)
        view.Viewpoint = viewpointName;
      var viewVm = ViewPointVM.FromModel(view);
      viewVm.SnapshotPath = Path.Combine(bcf.TempPath, issue.Model.Topic.Guid, view.Snapshot);
      issue.Viewpoints.Add(viewVm);
      return viewVm;
    }

    private static string CreateIssueDirectory(BcfFileVM bcf, Guid topicGuid)
    {
      var issueDir = Path.Combine(bcf.TempPath, topicGuid.ToString());
      Directory.CreateDirectory(issueDir);
      return issueDir;
    }

    [Test]
    public void merge_does_nothing_when_target_temp_path_missing()
    {
      // given
      var target = CreateBcf(Path.Combine(_baseTemp, "target"), false);
      var source = CreateBcf(Path.Combine(_baseTemp, "source"));
      AddIssue(source, Guid.NewGuid());

      // when
      target.MergeBcfFile(new[] { source });

      // then
      Assert.That(target.Issues, Is.Empty);
      Assert.That(target.HasBeenSaved, Is.True);
      Assert.That(Directory.Exists(source.TempPath), Is.True);
    }

    [Test]
    public void merge_adds_new_issue_and_moves_its_directory()
    {
      // given
      var topicGuid = Guid.NewGuid();
      var (target, source, targetTemp, sourceTemp) = CreateTargetAndSource();
      var issue = AddIssue(source, topicGuid);
      var viewVm = AddView(source, issue);
      var sourceIssueDir = CreateIssueDirectory(source, topicGuid);
      File.WriteAllText(Path.Combine(sourceIssueDir, viewVm.Snapshot), "img");

      // when
      target.MergeBcfFile(new[] { source });

      // then
      Assert.That(target.Issues, Has.Count.EqualTo(1));
      var mergedDir = Path.Combine(targetTemp, topicGuid.ToString());
      Assert.That(Directory.Exists(mergedDir), Is.True);
      Assert.That(File.Exists(Path.Combine(mergedDir, viewVm.Snapshot)), Is.True);
      Assert.That(Directory.Exists(sourceIssueDir), Is.False);
      Assert.That(Directory.Exists(sourceTemp), Is.False);
      Assert.That(target.Issues[0].Viewpoints[0].SnapshotPath, Is.EqualTo(Path.Combine(mergedDir, viewVm.Snapshot)));
      Assert.That(target.HasBeenSaved, Is.False);
    }

    [Test]
    public void merge_appends_new_comments_and_sorts_by_date_descending()
    {
      // given
      var topicGuid = Guid.NewGuid();
      var existingId = Guid.Parse("d6c8d1de-e197-4657-b7ae-6ea03b7f4a00");
      var newId = Guid.Parse("ba48f036-489c-4d4e-9340-8efd18f62b57");
      var older = new DateTime(2024, 1, 1);
      var newer = new DateTime(2024, 1, 2);

      var (target, source, _, _) = CreateTargetAndSource();
      var targetIssue = AddIssue(target, topicGuid);
      targetIssue.Comment.Add(CommentVM.FromModel(BuildComment(existingId, "existing", older)));

      var sourceIssue = AddIssue(source, topicGuid);
      sourceIssue.Comment.Add(CommentVM.FromModel(BuildComment(newId, "new", newer)));

      // when
      target.MergeBcfFile(new[] { source });

      // then
      var merged = target.Issues.Single();
      Assert.That(merged.Comment, Has.Count.EqualTo(2));
      Assert.That(merged.Comment.Select(c => c.Guid), Is.EqualTo(new[] { newId.ToString(), existingId.ToString() }));
      Assert.That(target.HasBeenSaved, Is.False);
    }

    [Test]
    public void merge_ignores_duplicate_comments()
    {
      // given
      var topicGuid = Guid.NewGuid();
      var commentId = Guid.Parse("d6c8d1de-e197-4657-b7ae-6ea03b7f4a00");

      var (target, source, _, _) = CreateTargetAndSource();
      var targetIssue = AddIssue(target, topicGuid);
      targetIssue.Comment.Add(CommentVM.FromModel(BuildComment(commentId, "existing", DateTime.Now)));

      var sourceIssue = AddIssue(source, topicGuid);
      sourceIssue.Comment.Add(CommentVM.FromModel(BuildComment(commentId, "duplicate", DateTime.Now)));

      // when
      target.MergeBcfFile(new[] { source });

      // then
      var merged = target.Issues.Single();
      Assert.That(merged.Comment, Has.Count.EqualTo(1));
      Assert.That(merged.Comment.Single().Guid, Is.EqualTo(commentId.ToString()));
    }

    [Test]
    public void merge_adds_new_view_with_renamed_files()
    {
      // given
      var topicGuid = Guid.NewGuid();
      var (target, source, targetTemp, _) = CreateTargetAndSource();

      AddIssue(target, topicGuid);
      var targetIssueDir = CreateIssueDirectory(target, topicGuid);
      //the target's first view snapshot, which must survive the merge
      File.WriteAllText(Path.Combine(targetIssueDir, "snapshot.png"), "target");

      // merge renames viewpoint to {guid}.bcfv and snapshot to {guid}.png so the incoming
      // snapshot.png does not overwrite the target's first view snapshot with the same name
      var sourceIssue = AddIssue(source, topicGuid);
      var viewVm = AddView(source, sourceIssue, "snapshot.png", "viewpoint.bcfv");
      var sourceIssueDir = CreateIssueDirectory(source, topicGuid);
      var sourceSnapshot = Path.Combine(sourceIssueDir, viewVm.Snapshot);
      File.WriteAllText(sourceSnapshot, "source");

      // when
      target.MergeBcfFile(new[] { source });

      // then
      var expectedSnapshotName = viewVm.Guid + ".png";
      var expectedViewpointName = viewVm.Guid + ".bcfv";
      var merged = target.Issues.Single();
      Assert.That(merged.Viewpoints, Has.Count.EqualTo(1));
      var mergedView = merged.Viewpoints[0];
      Assert.That(mergedView.Viewpoint, Is.EqualTo(expectedViewpointName));
      Assert.That(mergedView.Snapshot, Is.EqualTo(expectedSnapshotName));
      Assert.That(mergedView.SnapshotPath, Is.EqualTo(Path.Combine(targetTemp, topicGuid.ToString(), expectedSnapshotName)));
      Assert.That(File.ReadAllText(Path.Combine(targetIssueDir, expectedSnapshotName)), Is.EqualTo("source"));
      Assert.That(File.ReadAllText(Path.Combine(targetIssueDir, "snapshot.png")), Is.EqualTo("target"));
      Assert.That(File.Exists(sourceSnapshot), Is.False);
      Assert.That(target.HasBeenSaved, Is.False);
    }

    [Test]
    public void merge_adds_view_when_snapshot_file_missing()
    {
      // given
      var topicGuid = Guid.NewGuid();
      var (target, source, targetTemp, _) = CreateTargetAndSource();

      AddIssue(target, topicGuid);
      CreateIssueDirectory(target, topicGuid);

      var sourceIssue = AddIssue(source, topicGuid);
      var viewVm = AddView(source, sourceIssue);
      viewVm.SnapshotPath = "";

      // when
      target.MergeBcfFile(new[] { source });

      // then
      var merged = target.Issues.Single();
      Assert.That(merged.Viewpoints, Has.Count.EqualTo(1));
      var mergedView = merged.Viewpoints[0];
      Assert.That(mergedView.Viewpoint, Is.EqualTo(viewVm.Guid + ".bcfv"));
      Assert.That(mergedView.Snapshot, Is.EqualTo(viewVm.Guid + ".png"));
      Assert.That(mergedView.SnapshotPath, Is.EqualTo(Path.Combine(targetTemp, topicGuid.ToString(), viewVm.Guid + ".png")));
    }

    [Test]
    public void merge_does_not_duplicate_existing_views()
    {
      // given
      var topicGuid = Guid.NewGuid();
      var viewGuid = Guid.NewGuid();
      var (target, source, _, _) = CreateTargetAndSource();

      var targetIssue = AddIssue(target, topicGuid);
      targetIssue.Viewpoints.Add(ViewPointVM.FromModel(new ViewPoint(false) { Guid = viewGuid.ToString() }));

      var sourceIssue = AddIssue(source, topicGuid);
      sourceIssue.Viewpoints.Add(ViewPointVM.FromModel(new ViewPoint(false) { Guid = viewGuid.ToString() }));

      // when
      target.MergeBcfFile(new[] { source });

      // then
      var merged = target.Issues.Single();
      Assert.That(merged.Viewpoints, Has.Count.EqualTo(1));
      Assert.That(merged.Viewpoints.Single().Guid, Is.EqualTo(viewGuid.ToString()));
    }

    // TODO: even if there is nothing to merge
    [Test]
    public void merge_sets_has_been_saved_false()
    {
      // given
      var topicGuid = Guid.NewGuid();
      var (target, source, _, _) = CreateTargetAndSource();
      AddIssue(target, topicGuid);
      AddIssue(source, topicGuid);

      // when
      target.MergeBcfFile(new[] { source });

      // then
      Assert.That(target.HasBeenSaved, Is.False);
    }

    [Test]
    public void merge_combines_multiple_source_files()
    {
      // given
      var guid1 = Guid.NewGuid();
      var guid2 = Guid.NewGuid();
      var target = CreateBcf(Path.Combine(_baseTemp, "target"));

      var source1 = CreateBcf(Path.Combine(_baseTemp, "source1"));
      AddIssue(source1, guid1);
      CreateIssueDirectory(source1, guid1);

      var source2 = CreateBcf(Path.Combine(_baseTemp, "source2"));
      AddIssue(source2, guid2);
      CreateIssueDirectory(source2, guid2);

      // when
      target.MergeBcfFile(new[] { source1, source2 });

      // then
      Assert.That(target.Issues, Has.Count.EqualTo(2));
      Assert.That(target.Issues.Select(i => i.Model.Topic.Guid), Is.EquivalentTo(new[] { guid1.ToString(), guid2.ToString() }));
      Assert.That(Directory.Exists(Path.Combine(target.TempPath, guid1.ToString())), Is.True);
      Assert.That(Directory.Exists(Path.Combine(target.TempPath, guid2.ToString())), Is.True);
      Assert.That(Directory.Exists(source1.TempPath), Is.False);
      Assert.That(Directory.Exists(source2.TempPath), Is.False);
    }
  }
}