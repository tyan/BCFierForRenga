using Bcfier.Bcf;
using Bcfier.Bcf.Adapters;
using Bcfier.Bcf.Model;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Tests
{
  public class TestBcf21FileAdapter
  {
    // legacy fixtures: used only where the legacy sub-format itself is under test (detection and load)
    private readonly string _legacyTwoViewsBcfPath = Path.Combine(AppContext.BaseDirectory, "data", "V_2_1_old", "TwoViewsMaisonette.bcf");
    private readonly string _legacyDrawingTwoCommentsBcfPath = Path.Combine(AppContext.BaseDirectory, "data", "V_2_1_old", "DrawingTwoCommentsMaisonette.bcf");
    private readonly string _legacyMultipleMarkupsBcfPath = Path.Combine(AppContext.BaseDirectory, "data", "V_2_1_old", "MultipleMarkups.bcf");

    // conformant 2.1 fixtures: used everywhere else
    private readonly string _conformantTwoViewsBcfPath = Path.Combine(AppContext.BaseDirectory, "data", "V_2_1", "TwoViewsMaisonette.bcf");
    private readonly string _conformantDrawingTwoCommentsBcfPath = Path.Combine(AppContext.BaseDirectory, "data", "V_2_1", "DrawingTwoCommentsMaisonette.bcf");
    private readonly string _conformantNoSnapshotBcfPath = Path.Combine(AppContext.BaseDirectory, "data", "V_2_1", "NoSnapshot.bcf");
    private readonly string _conformantEmptyBcfPath = Path.Combine(AppContext.BaseDirectory, "data", "V_2_1", "Empty.bcf");

    private readonly List<string> _loadedTempPaths = new List<string>();
    private readonly string _outputDir = Path.Combine(AppContext.BaseDirectory, "test_adapter_out");

    [TearDown]
    public void TearDown()
    {
      foreach (var tempPath in _loadedTempPaths)
      {
        if (Directory.Exists(tempPath))
          Directory.Delete(tempPath, true);
      }
      if (Directory.Exists(_outputDir))
        Directory.Delete(_outputDir, true);
    }

    private BcfProject LoadThroughAdapter(Bcf21FileAdapter adapter, string path)
    {
      var project = adapter.Load(path);
      _loadedTempPaths.Add(project.TempPath);
      return project;
    }

    private BcfProject CreateConformantProject()
    {
      return new BcfProject
      {
        TempPath = Path.Combine(_outputDir, "temp", Guid.NewGuid().ToString()),
        Issues =
        {
          new BcfIssue
          {
            // conformant 2.1: CreationAuthor present, no plural legacy elements
            Topic = new BcfTopic { CreationAuthor = "someone@example.com" }
          }
        }
      };
    }

    private string CreateBcfFile(string fileName, params string[] entryNames)
    {
      Directory.CreateDirectory(_outputDir);
      var outPath = Path.Combine(_outputDir, fileName);
      using (var archive = ZipFile.Open(outPath, ZipArchiveMode.Create))
      {
        foreach (var entryName in entryNames)
          archive.CreateEntry(entryName);
      }
      return outPath;
    }

    [Test]
    public void detect_returns_bcf21_legacy_for_legacy_file()
    {
      // given
      var adapter = new Bcf21FileAdapter();

      // when
      var detected = adapter.Detect(_legacyTwoViewsBcfPath);

      // then
      Assert.That(detected, Is.EqualTo(ProjectFormat.Bcf21Legacy));
    }

    [Test]
    public void detect_returns_bcf21_for_conformant_fixture()
    {
      // given: a static conformant fixture from data/V_2_1, not produced by the adapter's save
      var adapter = new Bcf21FileAdapter();

      // when
      var detected = adapter.Detect(_conformantTwoViewsBcfPath);

      // then
      Assert.That(detected, Is.EqualTo(ProjectFormat.Bcf21));
    }

    [Test]
    public void save_of_conformant_project_is_detected_as_bcf21()
    {
      // given
      var adapter = new Bcf21FileAdapter();
      var outPath = Path.Combine(_outputDir, "conformant.bcf");
      Directory.CreateDirectory(_outputDir);

      // invariant: the output of Save must not carry legacy markers,
      // otherwise every reopened file is silently classified as legacy and re-migrated on the next save
      var project = CreateConformantProject();
      Assert.That(adapter.Save(project, outPath), Is.True);

      // when
      var detected = adapter.Detect(outPath);

      // then
      Assert.That(detected, Is.EqualTo(ProjectFormat.Bcf21));
    }

    [Test]
    public void detect_multi_topic_archive_does_not_throw()
    {
      // given
      var adapter = new Bcf21FileAdapter();

      // when
      var detected = adapter.Detect(_legacyMultipleMarkupsBcfPath);

      // then
      Assert.That(detected, Is.EqualTo(ProjectFormat.Bcf21Legacy));
    }

    [Test]
    public void detect_missing_bcf_version_throws_file_format()
    {
      // given: a zip without bcf.version (the strict contract requires it)
      var outPath = CreateBcfFile("missing_version.bcf", "project.bcfp");

      // when / then
      var adapter = new Bcf21FileAdapter();
      Assert.Throws<FileFormatException>(() => adapter.Detect(outPath),
        "an archive without bcf.version must be rejected: the version is read from the archive, not guessed");
    }

    [Test]
    public void detect_invalid_version_file_throws_file_format()
    {
      // given: bcf.version without the VersionId attribute
      Directory.CreateDirectory(_outputDir);
      var outPath = Path.Combine(_outputDir, "invalid_version.bcf");
      using (var archive = ZipFile.Open(outPath, ZipArchiveMode.Create))
      {
        var entry = archive.CreateEntry("bcf.version");
        using (var writer = new StreamWriter(entry.Open()))
          writer.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?><Version><DetailedVersion>2.1</DetailedVersion></Version>");
      }

      // when / then
      var adapter = new Bcf21FileAdapter();
      Assert.Throws<FileFormatException>(() => adapter.Detect(outPath));
    }

    [Test]
    public void load_bcf30_file_throws_not_supported()
    {
      // given: a BCF 3.0 zip (bcf.version VersionId="3.0", markup.xml)
      Directory.CreateDirectory(_outputDir);
      var outPath = Path.Combine(_outputDir, "bcf30.bcf");
      using (var archive = ZipFile.Open(outPath, ZipArchiveMode.Create))
      {
        var versionEntry = archive.CreateEntry("bcf.version");
        using (var writer = new StreamWriter(versionEntry.Open()))
          writer.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?><Version VersionId=\"3.0\"><DetailedVersion>3.0</DetailedVersion></Version>");
        archive.CreateEntry("guid-1/markup.xml");
      }

      // when / then
      var adapter = new Bcf21FileAdapter();
      Assert.Throws<NotSupportedException>(() => adapter.Detect(outPath));
    }

    [Test]
    public void detect_missing_file_throws_file_not_found()
    {
      // given
      var adapter = new Bcf21FileAdapter();
      var missingPath = Path.Combine(_outputDir, "does_not_exist.bcf");

      // when / then
      Assert.Throws<FileNotFoundException>(() => adapter.Detect(missingPath));
    }

    [Test]
    public void load_not_a_zip_throws_invalid_data()
    {
      // given: a file with the .bcf extension that is not a zip archive
      Directory.CreateDirectory(_outputDir);
      var outPath = Path.Combine(_outputDir, "not_a_zip.bcf");
      File.WriteAllText(outPath, "this is not a zip archive");

      // when / then: the error contract is an exception (not a null return)
      var adapter = new Bcf21FileAdapter();
      Assert.Throws<InvalidDataException>(() => adapter.Load(outPath));
    }

    [Test]
    public void load_sets_source_format_legacy_file()
    {
      // given
      var adapter = new Bcf21FileAdapter();

      // when
      var project = LoadThroughAdapter(adapter, _legacyTwoViewsBcfPath);

      // then
      Assert.That(project.SourceFormat, Is.EqualTo(ProjectFormat.Bcf21Legacy));
    }

    [Test]
    public void load_sets_source_format_for_conformant_file()
    {
      // given
      var adapter = new Bcf21FileAdapter();

      // when
      var loaded = LoadThroughAdapter(adapter, _conformantTwoViewsBcfPath);

      // then
      Assert.That(loaded.SourceFormat, Is.EqualTo(ProjectFormat.Bcf21));
    }

    [Test]
    public void load_conformant_fixture_maps_topic_and_viewpoints()
    {
      // given
      var adapter = new Bcf21FileAdapter();

      // when
      var project = LoadThroughAdapter(adapter, _conformantTwoViewsBcfPath);

      // then
      Assert.That(project.Issues, Has.Count.EqualTo(1));
      var issue = project.Issues.Single();
      Assert.That(issue.Topic.Guid, Is.EqualTo("95c35bf4-97f1-4f11-a4b7-ca723b342ce8"));
      Assert.That(issue.Topic.CreationAuthor, Is.EqualTo("author@example.com"));
      Assert.That(issue.Viewpoints, Has.Count.EqualTo(2));
      Assert.That(issue.Viewpoints.All(v => v.ViewpointFileName != null), Is.True);
      Assert.That(issue.Viewpoints.All(v => v.Data != null), Is.True);
    }

    [Test]
    public void load_empty_conformant_archive_returns_empty_project()
    {
      // given: bcf.version + project.bcfp, no topic folders
      var adapter = new Bcf21FileAdapter();

      // when
      var project = LoadThroughAdapter(adapter, _conformantEmptyBcfPath);

      // then
      Assert.That(project.Issues, Is.Empty);
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
      var project = LoadThroughAdapter(adapter, _legacyTwoViewsBcfPath);

      // then
      Assert.That(project, Is.Not.Null);
      Assert.That(project.Fullname, Is.EqualTo(Path.GetFullPath(_legacyTwoViewsBcfPath)));
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
      var project = LoadThroughAdapter(adapter, _legacyDrawingTwoCommentsBcfPath);

      // then
      var issue = project.Issues.Single();
      Assert.That(issue.Topic.Guid, Is.EqualTo("08e954f4-a064-4a22-8e1e-253296423604"));
      Assert.That(issue.Comments, Has.Count.EqualTo(2));

      var first = issue.Comments.Single(c => c.Guid == Guid.Parse("d6c8d1de-e197-4657-b7ae-6ea03b7f4a00"));
      Assert.That(first.Author, Is.EqualTo("Коля"));
      Assert.That(first.Comment, Is.EqualTo("Комментарий 1"));
      Assert.That(first.ViewpointGuid, Is.EqualTo(Guid.Parse("d3ce8d2d-cca4-42fd-9577-2ffa981219e1")));
    }

    [Test]
    public void load_without_snapshot_leaves_snapshot_file_name_null()
    {
      // given: a conformant fixture with a viewpoint but no snapshot file
      var adapter = new Bcf21FileAdapter();

      // when
      var project = LoadThroughAdapter(adapter, _conformantNoSnapshotBcfPath);

      // then
      var viewpoint = project.Issues.Single().Viewpoints.Single();
      Assert.That(viewpoint.SnapshotFileName, Is.Null);
      Assert.That(viewpoint.ViewpointFileName, Is.EqualTo("viewpoint.bcfv"));
    }

    [Test]
    public void load_keeps_temp_files_for_later_save()
    {
      // given
      var adapter = new Bcf21FileAdapter();

      // when
      var project = LoadThroughAdapter(adapter, _conformantTwoViewsBcfPath);

      // then
      Assert.That(Directory.Exists(project.TempPath), Is.True);
      var markupPath = Path.Combine(project.TempPath, project.Issues.Single().Topic.Guid, "markup.bcf");
      Assert.That(File.Exists(markupPath), Is.True);
    }

    [Test]
    public void save_and_reload_roundtrip_preserves_issue_state()
    {
      // given
      var adapter = new Bcf21FileAdapter();
      var project = LoadThroughAdapter(adapter, _conformantDrawingTwoCommentsBcfPath);
      Directory.CreateDirectory(_outputDir);
      var outPath = Path.Combine(_outputDir, "roundtrip.bcf");

      // when
      var saved = adapter.Save(project, outPath);
      var reloaded = LoadThroughAdapter(new Bcf21FileAdapter(), outPath);

      // then
      Assert.That(saved, Is.True);
      Assert.That(File.Exists(outPath), Is.True);
      Assert.That(reloaded.Issues, Has.Count.EqualTo(1));

      var originalIssue = project.Issues.Single();
      var reloadedIssue = reloaded.Issues.Single();
      Assert.That(reloadedIssue.Topic.Guid, Is.EqualTo(originalIssue.Topic.Guid));
      Assert.That(reloadedIssue.Topic.Index, Is.EqualTo(originalIssue.Topic.Index));
      Assert.That(reloadedIssue.Topic.CreationDate, Is.EqualTo(originalIssue.Topic.CreationDate));
      Assert.That(reloadedIssue.Comments.Select(c => c.Guid), Is.EquivalentTo(originalIssue.Comments.Select(c => c.Guid)));
      Assert.That(reloadedIssue.Viewpoints.Select(v => v.Guid), Is.EquivalentTo(originalIssue.Viewpoints.Select(v => v.Guid)));
      Assert.That(reloadedIssue.Viewpoints.Select(v => v.ViewpointFileName), Is.EquivalentTo(originalIssue.Viewpoints.Select(v => v.ViewpointFileName)));
      Assert.That(reloadedIssue.Viewpoints.Select(v => v.SnapshotFileName), Is.EquivalentTo(originalIssue.Viewpoints.Select(v => v.SnapshotFileName)));
    }

    [Test]
    public void save_and_reload_roundtrip_preserves_header_files()
    {
      // given
      var adapter = new Bcf21FileAdapter();
      var project = LoadThroughAdapter(adapter, _conformantDrawingTwoCommentsBcfPath);
      Assert.That(project.Issues.Single().HeaderFiles, Has.Count.EqualTo(1),
        "pre-condition: source corpus must carry a header file");
      Directory.CreateDirectory(_outputDir);
      var outPath = Path.Combine(_outputDir, "roundtrip_header.bcf");

      // when
      adapter.Save(project, outPath);
      var reloaded = LoadThroughAdapter(new Bcf21FileAdapter(), outPath);

      // then
      var originalIssue = project.Issues.Single();
      var reloadedIssue = reloaded.Issues.Single();
      Assert.That(reloadedIssue.HeaderFiles, Has.Count.EqualTo(1));
      Assert.That(reloadedIssue.HeaderFiles[0].Date, Is.EqualTo(originalIssue.HeaderFiles[0].Date));
      Assert.That(reloadedIssue.HeaderFiles[0].Filename, Is.EqualTo(originalIssue.HeaderFiles[0].Filename));
      Assert.That(reloadedIssue.HeaderFiles[0].Reference, Is.EqualTo(originalIssue.HeaderFiles[0].Reference));
    }

    [Test]
    public void save_and_reload_roundtrip_preserves_comments()
    {
      // given
      var adapter = new Bcf21FileAdapter();
      var project = LoadThroughAdapter(adapter, _conformantDrawingTwoCommentsBcfPath);
      Directory.CreateDirectory(_outputDir);
      var outPath = Path.Combine(_outputDir, "roundtrip_comments.bcf");

      // when
      adapter.Save(project, outPath);
      var reloaded = LoadThroughAdapter(new Bcf21FileAdapter(), outPath);

      // then
      var original = project.Issues.Single();
      var issue = reloaded.Issues.Single();
      Assert.That(issue.Comments, Has.Count.EqualTo(2));

      var reloadedComment = issue.Comments.Single(c => c.Guid == Guid.Parse("d6c8d1de-e197-4657-b7ae-6ea03b7f4a00"));
      var originalComment = original.Comments.Single(c => c.Guid == Guid.Parse("d6c8d1de-e197-4657-b7ae-6ea03b7f4a00"));
      Assert.That(reloadedComment.Author, Is.EqualTo(originalComment.Author));
      Assert.That(reloadedComment.Comment, Is.EqualTo(originalComment.Comment));
      Assert.That(reloadedComment.Date, Is.EqualTo(originalComment.Date));
      Assert.That(reloadedComment.ViewpointGuid, Is.EqualTo(originalComment.ViewpointGuid));
    }

    [Test]
    public void save_and_reload_roundtrip_preserves_viewpoint_data()
    {
      // given
      var adapter = new Bcf21FileAdapter();
      var project = LoadThroughAdapter(adapter, _conformantTwoViewsBcfPath);
      Directory.CreateDirectory(_outputDir);
      var outPath = Path.Combine(_outputDir, "roundtrip_views.bcf");

      // when
      adapter.Save(project, outPath);
      var reloaded = LoadThroughAdapter(new Bcf21FileAdapter(), outPath);

      // then
      var originalIssue = project.Issues.Single();
      var issue = reloaded.Issues.Single();
      Assert.That(issue.Viewpoints, Has.Count.EqualTo(2));
      Assert.That(issue.Viewpoints.All(v => v.Data != null), Is.True);
      Assert.That(issue.Viewpoints.Single(v => v.Guid == originalIssue.Viewpoints[0].Guid).Data.Guid,
        Is.EqualTo(originalIssue.Viewpoints[0].Data.Guid));
    }
  }
}
