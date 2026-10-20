using Bcfier.Bcf;
using Bcfier.Bcf.Adapters;
using Bcfier.Bcf.Model;
using System;
using System.IO;
using System.Linq;

namespace Tests
{
  public class TestBcf21FileAdapter
  {
    private readonly string _twoViewsBcfPath = Path.Combine(AppContext.BaseDirectory, "data", "V_2_1_old", "2views_Maisonette.bcf");
    private readonly string _drawing2CommentsBcfPath = Path.Combine(AppContext.BaseDirectory, "data", "V_2_1_old", "drawing_2comments_Maisonette.bcf");
    private readonly string _noSnapshotBcfPath = Path.Combine(AppContext.BaseDirectory, "data", "V_2_1_old", "no_snapshot.bcf");

    private readonly List<string> _loadedTempPaths = new List<string>();

    [TearDown]
    public void TearDown()
    {
      foreach (var tempPath in _loadedTempPaths)
      {
        if (Directory.Exists(tempPath))
          Directory.Delete(tempPath, true);
      }
    }

    private BcfProject LoadThroughAdapter(Bcf21FileAdapter adapter, string path)
    {
      var project = adapter.Load(path);
      _loadedTempPaths.Add(project.TempPath);
      return project;
    }

    [Test]
    public void detect_returns_bcf21_for_legacy_file()
    {
      // given
      var adapter = new Bcf21FileAdapter();

      // when
      var detected = adapter.Detect(_twoViewsBcfPath);

      // then
      Assert.That(detected, Is.EqualTo(ProjectFormat.Bcf21));
      Assert.That(adapter.Format, Is.EqualTo(ProjectFormat.Bcf21));
    }

    [Test]
    public void factory_creates_bcf21_adapter_for_legacy_format()
    {
      // when
      var adapter = ProjectAdapterFactory.Create(ProjectFormat.Bcf21Legacy);
      var adapter21 = ProjectAdapterFactory.Create(ProjectFormat.Bcf21);

      // then
      Assert.That(adapter, Is.TypeOf<Bcf21FileAdapter>());
      Assert.That(adapter21, Is.TypeOf<Bcf21FileAdapter>());
    }

    [Test]
    public void load_maps_topic_and_viewpoints_from_bcf2()
    {
      // given
      var adapter = new Bcf21FileAdapter();

      // when
      var project = LoadThroughAdapter(adapter, _twoViewsBcfPath);

      // then
      Assert.That(project, Is.Not.Null);
      Assert.That(project.Fullname, Is.EqualTo(Path.GetFullPath(_twoViewsBcfPath)));
      Assert.That(project.Issues, Has.Count.EqualTo(1));

      var issue = project.Issues.Single();
      Assert.That(issue.Topic.Guid, Is.EqualTo("95c35bf4-97f1-4f11-a4b7-ca723b342ce8"));
      Assert.That(issue.Topic.Index, Is.EqualTo(0));
      Assert.That(issue.Viewpoints, Has.Count.EqualTo(2));
      Assert.That(issue.Comments, Is.Empty);

      Assert.That(issue.Viewpoints.Select(v => v.Guid.ToString()),
        Is.EquivalentTo(new[]
        {
          "b8bcbaba-c94d-46f0-8c53-63c63235446e",
          "d66b0ff3-9172-4393-980a-fe1ba798a1fd"
        }));
      Assert.That(issue.Viewpoints.All(v => v.ViewpointFileName != null), Is.True);
      Assert.That(issue.Viewpoints.All(v => v.Data != null), Is.True);
    }

    [Test]
    public void load_maps_comments_and_viewpoint_guids()
    {
      // given
      var adapter = new Bcf21FileAdapter();

      // when
      var project = LoadThroughAdapter(adapter, _drawing2CommentsBcfPath);

      // then
      var issue = project.Issues.Single();
      Assert.That(issue.Topic.Guid, Is.EqualTo("08e954f4-a064-4a22-8e1e-253296423604"));
      Assert.That(issue.Comments, Has.Count.EqualTo(2));

      var first = issue.Comments.Single(c => c.Guid == Guid.Parse("d6c8d1de-e197-4657-b7ae-6ea03b7f4a00"));
      Assert.That(first.Author, Is.EqualTo("Коля"));
      Assert.That(first.Comment, Is.EqualTo("Комментарий 1"));
      Assert.That(first.ViewpointGuid, Is.EqualTo(Guid.Parse("d3ce8d2d-cca4-42fd-9577-2ffa981219e1")));

      Assert.That(issue.Comments.All(c => c.ViewpointGuid == Guid.Parse("d3ce8d2d-cca4-42fd-9577-2ffa981219e1")), Is.True);
    }

    [Test]
    public void load_without_snapshot_leaves_snapshot_file_name_null()
    {
      // given
      var adapter = new Bcf21FileAdapter();

      // when
      var project = LoadThroughAdapter(adapter, _noSnapshotBcfPath);

      // then
      var viewpoint = project.Issues.Single().Viewpoints.Single();
      Assert.That(viewpoint.SnapshotFileName, Is.Null);
    }

    [Test]
    public void load_keeps_temp_files_for_later_save()
    {
      // given
      var adapter = new Bcf21FileAdapter();

      // when
      var project = LoadThroughAdapter(adapter, _twoViewsBcfPath);

      // then
      Assert.That(Directory.Exists(project.TempPath), Is.True);
      var markupPath = Path.Combine(project.TempPath, project.Issues.Single().Topic.Guid, "markup.bcf");
      Assert.That(File.Exists(markupPath), Is.True);
    }
  }
}