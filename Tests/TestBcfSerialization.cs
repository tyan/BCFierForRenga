using Bcfier.Bcf;
using System;
using System.IO;
using System.Linq;

namespace Tests
{
  public class TestDeserializeBcf_V_2_1_old
  {
    private readonly string _noSnapshotBcfPath = Path.Combine(AppContext.BaseDirectory, "data", "V_2_1_old", "no_snapshot.bcf");
    private readonly string _missingBcfvPath = Path.Combine(AppContext.BaseDirectory, "data", "V_2_1_old", "missing_bcfv.bcf");
    private readonly string _drawing2CommentsBcfPath = Path.Combine(AppContext.BaseDirectory, "data", "V_2_1_old", "drawing_2comments_Maisonette.bcf");
    private readonly string _twoViewsBcfPath = Path.Combine(AppContext.BaseDirectory, "data", "V_2_1_old", "2views_Maisonette.bcf");

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
      var container = new BcfContainer();

      // when / then
      Assert.That(() => container.OpenFile(_noSnapshotBcfPath), Throws.Nothing);
    }

    [Test]
    public void deserialize_bcf_without_snapshot_leaves_snapshot_path_null()
    {
      // given
      var container = new BcfContainer();
      container.OpenFile(_noSnapshotBcfPath);

      // when
      var viewpoint = container.BcfFiles.Single().Issues.Single().Viewpoints.Single();

      // then
      Assert.That(viewpoint.SnapshotPath, Is.Null);
    }

    [Test]
    public void load_issue_and_viewpoint_state()
    {
      // given
      var container = new BcfContainer();
      container.OpenFile(_twoViewsBcfPath);

      // when
      var bcf = container.BcfFiles.Single();
      var issue = bcf.Issues.Single();

      // then
      Assert.That(bcf.Issues, Has.Count.EqualTo(1));
      Assert.That(issue.Topic.Guid, Is.EqualTo("95c35bf4-97f1-4f11-a4b7-ca723b342ce8"));
      Assert.That(issue.Topic.Index, Is.EqualTo(0));
      Assert.That(issue.Viewpoints, Has.Count.EqualTo(2));
      Assert.That(issue.Comment, Has.Count.EqualTo(0));
      Assert.That(issue.Viewpoints.Select(v => v.Guid),
        Is.EquivalentTo(new[]
        {
          "b8bcbaba-c94d-46f0-8c53-63c63235446e",
          "d66b0ff3-9172-4393-980a-fe1ba798a1fd"
        }));
      Assert.That(issue.Viewpoints.All(v => v.SnapshotPath != null), Is.True);
    }

    [Test]
    public void load_issue_comment_and_viewpoint_state()
    {
      // given
      var container = new BcfContainer();
      container.OpenFile(_drawing2CommentsBcfPath);

      // when
      var bcf = container.BcfFiles.Single();
      var issue = bcf.Issues.Single();

      // then
      Assert.That(bcf.Issues, Has.Count.EqualTo(1));
      Assert.That(issue.Topic.Guid, Is.EqualTo("08e954f4-a064-4a22-8e1e-253296423604"));
      Assert.That(issue.Topic.Index, Is.EqualTo(0));
      Assert.That(issue.Viewpoints, Has.Count.EqualTo(1));
      Assert.That(issue.Comment, Has.Count.EqualTo(2));
      Assert.That(issue.Comment.Select(c => c.Guid),
        Is.EquivalentTo(new[]
        {
          "d6c8d1de-e197-4657-b7ae-6ea03b7f4a00",
          "ba48f036-489c-4d4e-9340-8efd18f62b57"
        }));
      Assert.That(issue.Comment.All(c => c.Viewpoint != null && c.Viewpoint.Guid == "d3ce8d2d-cca4-42fd-9577-2ffa981219e1"), Is.True);
      var firstComment = issue.Comment.Single(c => c.Guid == "d6c8d1de-e197-4657-b7ae-6ea03b7f4a00");
      Assert.That(firstComment.Author, Is.EqualTo("Коля"));
      Assert.That(firstComment.Comment1, Is.EqualTo("Комментарий 1"));
      var secondComment = issue.Comment.Single(c => c.Guid == "ba48f036-489c-4d4e-9340-8efd18f62b57");
      Assert.That(secondComment.Author, Is.EqualTo("Юра"));
      Assert.That(secondComment.Comment1, Is.EqualTo("Обсудим?"));
      Assert.That(issue.Viewpoints.Single().SnapshotPath, Is.Not.Null);
      var viewComments = issue.ViewComments;
      Assert.That(viewComments, Has.Count.EqualTo(2));
      Assert.That(viewComments.First().Comments, Has.Count.EqualTo(2));
    }

    [Test]
    public void save_and_reload_preserves_issue_title_and_counts()
    {
      // given
      var container = new BcfContainer();
      container.OpenFile(_drawing2CommentsBcfPath);

      var bcf = container.BcfFiles.Single();
      var originalTitle = bcf.Issues.Single().Topic.Title;
      var outputDir = Path.Combine(AppContext.BaseDirectory, "test_temp");
      Directory.CreateDirectory(outputDir);
      var outPath = Path.Combine(outputDir, "drawing_2comments_out.bcf");
      bcf.Fullname = outPath;

      // when
      container.SaveFile(bcf);

      // reload the saved file
      var reloaded = new BcfContainer();
      reloaded.OpenFile(outPath);
      var reloadedIssue = reloaded.BcfFiles.Single().Issues.Single();

      // then
      Assert.That(reloadedIssue.Topic.Title, Is.EqualTo(originalTitle));
      Assert.That(reloadedIssue.Viewpoints, Has.Count.EqualTo(1));
      Assert.That(reloadedIssue.Comment, Has.Count.EqualTo(2));
    }

    [Test]
    public void save_bcf_without_snapshot_does_not_throw()
    {
      // given
      var container = new BcfContainer();
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
      var container = new BcfContainer();
      container.OpenFile(_noSnapshotBcfPath);

      var bcf = container.BcfFiles.Single();
      var outputDir = Path.Combine(AppContext.BaseDirectory, "test_temp");
      Directory.CreateDirectory(outputDir);
      var outPath = Path.Combine(outputDir, "no_snapshot_out.bcf");
      bcf.Fullname = outPath;

      // when
      container.SaveFile(bcf);

      // reload the saved file
      var reloaded = new BcfContainer();
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
