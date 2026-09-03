using Bcfier.Bcf;
using Bcfier.Bcf.ViewModel;
using System;
using System.IO;
using System.Linq;

namespace Tests
{
  public class TestDeserializeBcf
  {
    private readonly string _noSnapshotBcfPath = Path.Combine(AppContext.BaseDirectory, "data", "no_snapshot.bcf");
    private readonly string _missingBcfvPath = Path.Combine(AppContext.BaseDirectory, "data", "missing_bcfv.bcf");

    [TearDown]
    public void CleanUp()
    {
      var tempDir = Path.Combine(AppContext.BaseDirectory, "test_temp");
      if (Directory.Exists(tempDir))
        Directory.Delete(tempDir, true);
    }

    [Test]
    public void deserialize_bcf_without_snapshot_does_not_throw()
    {
      // given
      var container = new BcfContainerVM();

      // when / then
      Assert.That(() => container.OpenFile(_noSnapshotBcfPath), Throws.Nothing);
    }

    [Test]
    public void deserialize_bcf_without_snapshot_leaves_snapshot_path_null()
    {
      // given
      var container = new BcfContainerVM();
      container.OpenFile(_noSnapshotBcfPath);

      // when
      var viewpoint = container.BcfFiles.Single().Issues.Single().Viewpoints.Single();

      // then
      Assert.That(viewpoint.SnapshotPath, Is.Null);
    }

    [Test]
    public void load_bcf_preserves_issue_and_viewpoint_counts()
    {
      // given
      var container = new BcfContainerVM();
      container.OpenFile(_noSnapshotBcfPath);

      // when
      var bcf = container.BcfFiles.Single();
      var issue = bcf.Issues.Single();

      // then
      Assert.That(bcf.Issues, Has.Count.EqualTo(1));
      Assert.That(issue.Viewpoints, Has.Count.EqualTo(1));
      Assert.That(issue.Comment, Has.Count.EqualTo(0));
      Assert.That(issue.Topic.Guid, Is.EqualTo("2f0e1b14-3d8f-4a0b-9c61-1b53a27e3c4a"));
      Assert.That(issue.Topic.Title, Is.EqualTo("Issue without snapshot"));
    }

    [Test]
    public void save_and_reload_preserves_issue_title_and_counts()
    {
      // given
      var container = new BcfContainerVM();
      container.OpenFile(_noSnapshotBcfPath);

      var bcf = container.BcfFiles.Single();
      var outputDir = Path.Combine(AppContext.BaseDirectory, "test_temp");
      Directory.CreateDirectory(outputDir);
      var outPath = Path.Combine(outputDir, "no_snapshot_out.bcf");
      bcf.Fullname = outPath;

      // when
      container.SaveFile(bcf);

      // reload the saved file
      var reloaded = new BcfContainerVM();
      reloaded.OpenFile(outPath);
      var reloadedIssue = reloaded.BcfFiles.Single().Issues.Single();

      // then
      Assert.That(reloadedIssue.Topic.Title, Is.EqualTo("Issue without snapshot"));
      Assert.That(reloadedIssue.Viewpoints, Has.Count.EqualTo(1));
      Assert.That(reloadedIssue.Comment, Has.Count.EqualTo(0));
    }

    [Test]
    public void save_bcf_without_snapshot_does_not_throw()
    {
      // given
      var container = new BcfContainerVM();
      container.OpenFile(_noSnapshotBcfPath);

      var bcf = container.BcfFiles.Single();
      var outputDir = Path.Combine(AppContext.BaseDirectory, "test_temp");
      Directory.CreateDirectory(outputDir);
      bcf.Fullname = Path.Combine(outputDir, "no_snapshot_out.bcf");

      // when / then
      Assert.That(() => container.SaveFile(bcf), Throws.Nothing);
    }

    [Test]
    public void save_and_reload_bcf_without_snapshot_leaves_snapshot_path_null()
    {
      // given
      var container = new BcfContainerVM();
      container.OpenFile(_noSnapshotBcfPath);

      var bcf = container.BcfFiles.Single();
      var outputDir = Path.Combine(AppContext.BaseDirectory, "test_temp");
      Directory.CreateDirectory(outputDir);
      var outPath = Path.Combine(outputDir, "no_snapshot_out.bcf");
      bcf.Fullname = outPath;

      // when
      container.SaveFile(bcf);

      // reload the saved file
      var reloaded = new BcfContainerVM();
      reloaded.OpenFile(outPath);

      // then
      var viewpoint = reloaded.BcfFiles.Single().Issues.Single().Viewpoints.Single();
      Assert.That(viewpoint.SnapshotPath, Is.Null);
    }

    [Test]
    public void load_bcf_with_missing_viewpoint_file_throws_invalid_data_exception()
    {
      // when / then
      Assert.That(() => BcfSerializer.load(_missingBcfvPath), Throws.InstanceOf<System.IO.InvalidDataException>());
    }
  }
}
