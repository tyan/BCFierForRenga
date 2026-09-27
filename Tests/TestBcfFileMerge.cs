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

    [Test]
    public void merge_does_nothing_when_target_temp_path_missing()
    {
      // given
      var target = CreateBcf(Path.Combine(_baseTemp, "target"), false);
      var source = CreateBcf(Path.Combine(_baseTemp, "source"));
      AddIssue(source, Guid.NewGuid());

      // when / then
      Assert.That(() => target.MergeBcfFile(new[] { source }), Throws.Nothing);
      Assert.That(target.Issues, Is.Empty);
      Assert.That(target.HasBeenSaved, Is.True);
    }

    [Test]
    public void merge_adds_new_issue_and_moves_its_directory()
    {
      // given
      var topicGuid = Guid.NewGuid();
      var targetTemp = Path.Combine(_baseTemp, "target");
      var sourceTemp = Path.Combine(_baseTemp, "source");
      var target = CreateBcf(targetTemp);
      var source = CreateBcf(sourceTemp);

      var sourceIssue = AddIssue(source, topicGuid);
      var view = new ViewPoint(false);
      sourceIssue.Viewpoints.Add(ViewPointVM.FromModel(view));

      var sourceIssueDir = Path.Combine(sourceTemp, topicGuid.ToString());
      Directory.CreateDirectory(sourceIssueDir);
      File.WriteAllText(Path.Combine(sourceIssueDir, view.Snapshot), "img");

      // when
      target.MergeBcfFile(new[] { source });

      // then
      Assert.That(target.Issues, Has.Count.EqualTo(1));
      Assert.That(target.Model.Issues, Has.Count.EqualTo(1));
      var mergedDir = Path.Combine(targetTemp, topicGuid.ToString());
      Assert.That(Directory.Exists(mergedDir), Is.True);
      Assert.That(File.Exists(Path.Combine(mergedDir, view.Snapshot)), Is.True);
      Assert.That(Directory.Exists(sourceIssueDir), Is.False);
      Assert.That(Directory.Exists(sourceTemp), Is.False);
      Assert.That(target.Issues[0].Viewpoints[0].SnapshotPath, Is.EqualTo(Path.Combine(mergedDir, view.Snapshot)));
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

      var target = CreateBcf(Path.Combine(_baseTemp, "target"));
      var targetIssue = AddIssue(target, topicGuid);
      targetIssue.Comment.Add(CommentVM.FromModel(BuildComment(existingId, "existing", older)));

      var source = CreateBcf(Path.Combine(_baseTemp, "source"));
      var sourceIssue = AddIssue(source, topicGuid);
      sourceIssue.Comment.Add(CommentVM.FromModel(BuildComment(newId, "new", newer)));

      // when
      target.MergeBcfFile(new[] { source });

      // then
      var merged = target.Issues.Single();
      Assert.That(merged.Comment, Has.Count.EqualTo(2));
      Assert.That(merged.Comment.Select(c => c.Guid), Is.EqualTo(new[] { newId.ToString(), existingId.ToString() }));
      Assert.That(merged.Model.Comment.Select(c => c.Guid), Is.EqualTo(new[] { newId.ToString(), existingId.ToString() }));
      Assert.That(target.HasBeenSaved, Is.False);
    }

    [Test]
    public void merge_ignores_duplicate_comments()
    {
      // given
      var topicGuid = Guid.NewGuid();
      var commentId = Guid.Parse("d6c8d1de-e197-4657-b7ae-6ea03b7f4a00");

      var target = CreateBcf(Path.Combine(_baseTemp, "target"));
      var targetIssue = AddIssue(target, topicGuid);
      targetIssue.Comment.Add(CommentVM.FromModel(BuildComment(commentId, "existing", DateTime.Now)));

      var source = CreateBcf(Path.Combine(_baseTemp, "source"));
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
      var targetTemp = Path.Combine(_baseTemp, "target");
      var sourceTemp = Path.Combine(_baseTemp, "source");
      var target = CreateBcf(targetTemp);
      var source = CreateBcf(sourceTemp);

      var targetIssue = AddIssue(target, topicGuid);
      Directory.CreateDirectory(Path.Combine(targetTemp, topicGuid.ToString()));

      var sourceIssue = AddIssue(source, topicGuid);
      var view = new ViewPoint(false);
      var sourceIssueDir = Path.Combine(sourceTemp, topicGuid.ToString());
      Directory.CreateDirectory(sourceIssueDir);
      var sourceSnapshot = Path.Combine(sourceIssueDir, view.Snapshot);
      File.WriteAllText(sourceSnapshot, "img");
      var viewVm = ViewPointVM.FromModel(view);
      viewVm.SnapshotPath = sourceSnapshot;
      sourceIssue.Viewpoints.Add(viewVm);

      // when
      target.MergeBcfFile(new[] { source });

      // then
      var merged = target.Issues.Single();
      Assert.That(merged.Viewpoints, Has.Count.EqualTo(1));
      var mergedView = merged.Viewpoints[0];
      var expectedSnapshotPath = Path.Combine(targetTemp, topicGuid.ToString(), view.Guid + ".png");
      Assert.That(mergedView.Viewpoint, Is.EqualTo(view.Guid + ".bcfv"));
      Assert.That(mergedView.Snapshot, Is.EqualTo(view.Guid + ".png"));
      Assert.That(mergedView.SnapshotPath, Is.EqualTo(expectedSnapshotPath));
      Assert.That(File.Exists(expectedSnapshotPath), Is.True);
      Assert.That(File.Exists(sourceSnapshot), Is.False);
      Assert.That(merged.Model.Viewpoints, Has.Count.EqualTo(1));
      Assert.That(target.HasBeenSaved, Is.False);
    }

    [Test]
    public void merge_skips_view_without_snapshot_file()
    {
      // given
      var topicGuid = Guid.NewGuid();
      var targetTemp = Path.Combine(_baseTemp, "target");
      var sourceTemp = Path.Combine(_baseTemp, "source");
      var target = CreateBcf(targetTemp);
      var source = CreateBcf(sourceTemp);

      AddIssue(target, topicGuid);
      Directory.CreateDirectory(Path.Combine(targetTemp, topicGuid.ToString()));

      var sourceIssue = AddIssue(source, topicGuid);
      var view = new ViewPoint(false);
      var viewVm = ViewPointVM.FromModel(view);
      viewVm.SnapshotPath = "";
      sourceIssue.Viewpoints.Add(viewVm);

      // when / then
      Assert.That(() => target.MergeBcfFile(new[] { source }), Throws.Nothing);
      var merged = target.Issues.Single();
      Assert.That(merged.Viewpoints, Has.Count.EqualTo(1));
      var mergedView = merged.Viewpoints[0];
      Assert.That(mergedView.Viewpoint, Is.EqualTo(view.Guid + ".bcfv"));
      Assert.That(mergedView.Snapshot, Is.EqualTo(view.Guid + ".png"));
      Assert.That(mergedView.SnapshotPath, Is.EqualTo(Path.Combine(targetTemp, topicGuid.ToString(), view.Guid + ".png")));
    }

    [Test]
    public void merge_does_not_duplicate_existing_views()
    {
      // given
      var topicGuid = Guid.NewGuid();
      var viewGuid = Guid.NewGuid();
      var target = CreateBcf(Path.Combine(_baseTemp, "target"));
      var source = CreateBcf(Path.Combine(_baseTemp, "source"));

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
      var target = CreateBcf(Path.Combine(_baseTemp, "target"));
      var source = CreateBcf(Path.Combine(_baseTemp, "source"));
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
      var targetTemp = Path.Combine(_baseTemp, "target");
      var source1Temp = Path.Combine(_baseTemp, "source1");
      var source2Temp = Path.Combine(_baseTemp, "source2");
      var target = CreateBcf(targetTemp);

      var source1 = CreateBcf(source1Temp);
      var issue1 = AddIssue(source1, guid1);
      Directory.CreateDirectory(Path.Combine(source1Temp, guid1.ToString()));
      issue1.Viewpoints.Add(ViewPointVM.FromModel(new ViewPoint(false)));

      var source2 = CreateBcf(source2Temp);
      var issue2 = AddIssue(source2, guid2);
      Directory.CreateDirectory(Path.Combine(source2Temp, guid2.ToString()));
      issue2.Viewpoints.Add(ViewPointVM.FromModel(new ViewPoint(false)));

      // when
      target.MergeBcfFile(new[] { source1, source2 });

      // then
      Assert.That(target.Issues, Has.Count.EqualTo(2));
      Assert.That(target.Issues.Select(i => i.Model.Topic.Guid), Is.EquivalentTo(new[] { guid1.ToString(), guid2.ToString() }));
      Assert.That(Directory.Exists(Path.Combine(targetTemp, guid1.ToString())), Is.True);
      Assert.That(Directory.Exists(Path.Combine(targetTemp, guid2.ToString())), Is.True);
      Assert.That(Directory.Exists(source1Temp), Is.False);
      Assert.That(Directory.Exists(source2Temp), Is.False);
    }
  }
}
