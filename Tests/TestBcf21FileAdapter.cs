using Bcfier.Bcf;
using Bcfier.Bcf.Adapters;
using Bcfier.Bcf.Model;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Schema;
using System.Xml.Linq;

namespace Tests
{
  public class TestBcf21FileAdapter
  {
    // legacy fixtures: used only where the legacy sub-format itself is under test (detection and load)
    private readonly string _legacyTwoViewsBcfPath = Path.Combine(AppContext.BaseDirectory, "data", "V_2_1_old", "TwoViewsMaisonette.bcf");
    private readonly string _legacyDrawingTwoCommentsBcfPath = Path.Combine(AppContext.BaseDirectory, "data", "V_2_1_old", "DrawingTwoCommentsMaisonette.bcf");
    private readonly string _legacyCameraBcfPath = Path.Combine(AppContext.BaseDirectory, "data", "V_2_1_old", "CameraMaisonette.bcf");
    private readonly string _legacyMultipleMarkupsBcfPath = Path.Combine(AppContext.BaseDirectory, "data", "V_2_1_old", "MultipleMarkups.bcf");
    private readonly string _noTopicBcfPath = Path.Combine(AppContext.BaseDirectory, "data", "V_2_1_old", "NoTopic.bcf");

    // conformant 2.1 fixtures: used everywhere else
    private readonly string _conformantTwoViewsBcfPath = Path.Combine(AppContext.BaseDirectory, "data", "V_2_1", "TwoViewsMaisonette.bcf");
    private readonly string _conformantDrawingTwoCommentsBcfPath = Path.Combine(AppContext.BaseDirectory, "data", "V_2_1", "DrawingTwoCommentsMaisonette.bcf");
    private readonly string _conformantCameraBcfPath = Path.Combine(AppContext.BaseDirectory, "data", "V_2_1", "CameraMaisonette.bcf");
    private readonly string _conformantNoSnapshotBcfPath = Path.Combine(AppContext.BaseDirectory, "data", "V_2_1", "NoSnapshot.bcf");
    private readonly string _conformantNoCommentsViewpointsBcfPath = Path.Combine(AppContext.BaseDirectory, "data", "V_2_1", "NoCommentsViewpoints.bcf");
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
          // conformant 2.1: CreationAuthor present, no plural legacy elements
          new BcfIssue(new BcfTopic { CreationAuthor = "someone@example.com" })
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
      // legacy fixtures carry viewpoint payloads without a Guid attribute; the schema-valid
      // save fills it (required by visinfo.xsd), so it must be non-empty after the round-trip
      Assert.That(issue.Viewpoints.Single(v => v.Guid == originalIssue.Viewpoints[0].Guid).Data.Guid,
        Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void roundtrip_preserves_default_visibility()
    {
      // given
      var adapter = new Bcf21FileAdapter();
      var project = LoadThroughAdapter(adapter, _conformantCameraBcfPath);
      var viewpoint = project.Issues.Single().Viewpoints.Single();
      Assert.That(viewpoint.Data.Components, Is.Not.Null, "pre-condition: fixture must carry components");
      Assert.That(viewpoint.Data.Components.Visibility, Is.Not.Null, "pre-condition: fixture must carry visibility");
      Assert.That(viewpoint.Data.Components.Visibility.DefaultVisibility, Is.True,
        "pre-condition: fixture must carry DefaultVisibility=true");
      Directory.CreateDirectory(_outputDir);
      var outPath = Path.Combine(_outputDir, "roundtrip_visibility.bcf");

      // when
      Assert.That(adapter.Save(project, outPath), Is.True);
      var reloaded = LoadThroughAdapter(new Bcf21FileAdapter(), outPath);

      // then
      var visibility = reloaded.Issues.Single().Viewpoints.Single().Data.Components.Visibility;
      Assert.That(visibility, Is.Not.Null);
      Assert.That(visibility.DefaultVisibility, Is.True,
        "DefaultVisibility=true was lost on save: DefaultVisibilitySpecified is not set");
    }

    [Test]
    public void roundtrip_preserves_comment_modified_date()
    {
      // given
      var adapter = new Bcf21FileAdapter();
      var modifiedDate = new DateTime(2026, 10, 15, 12, 30, 45, DateTimeKind.Local);
      var project = CreateConformantProject();
      project.Issues.Single().Comments.Add(new BcfComment
      {
        ModifiedDate = modifiedDate,
      });
      Directory.CreateDirectory(_outputDir);
      var outPath = Path.Combine(_outputDir, "roundtrip_comment_modified.bcf");

      // when
      Assert.That(adapter.Save(project, outPath), Is.True);
      var reloaded = LoadThroughAdapter(new Bcf21FileAdapter(), outPath);

      // then
      var comment = reloaded.Issues.Single().Comments.Single();
      Assert.That(comment.ModifiedDate, Is.EqualTo(modifiedDate),
        "comment ModifiedDate was lost on save: ModifiedDateSpecified is not set");
    }

    [Test]
    public void roundtrip_preserves_topic_modified_and_due_date()
    {
      // given
      var adapter = new Bcf21FileAdapter();
      var modifiedDate = new DateTime(2026, 10, 15, 12, 30, 45, DateTimeKind.Local);
      var dueDate = new DateTime(2026, 11, 20, 18, 0, 0, DateTimeKind.Local);
      var project = CreateConformantProject();
      var topic = project.Issues.Single().Topic;
      topic.ModifiedDate = modifiedDate;
      topic.DueDate = dueDate;
      Directory.CreateDirectory(_outputDir);
      var outPath = Path.Combine(_outputDir, "roundtrip_topic_dates.bcf");

      // when
      Assert.That(adapter.Save(project, outPath), Is.True);
      var reloaded = LoadThroughAdapter(new Bcf21FileAdapter(), outPath);

      // then
      var topicReloaded = reloaded.Issues.Single().Topic;
      Assert.That(topicReloaded.ModifiedDate, Is.EqualTo(modifiedDate),
        "topic ModifiedDate was lost on save: ModifiedDateSpecified is not set");
      Assert.That(topicReloaded.DueDate, Is.EqualTo(dueDate),
        "topic DueDate was lost on save: DueDateSpecified is not set");
    }

    [Test]
    public void load_markup_without_comments_viewpoints_works()
    {
      // given: a conformant fixture whose markup.bcf has no Comment and no Viewpoints elements
      // (allowed by the BCF 2.1 schema, written by third-party tools)
      var adapter = new Bcf21FileAdapter();

      // when
      var project = LoadThroughAdapter(adapter, _conformantNoCommentsViewpointsBcfPath);

      // then
      var issue = project.Issues.Single();
      Assert.That(issue.Comments, Is.Empty);
      Assert.That(issue.Viewpoints, Is.Empty);
      Assert.That(issue.Topic.Guid, Is.EqualTo("11111111-2222-4333-8444-555555555555"));
      Assert.That(project.SourceFormat, Is.EqualTo(ProjectFormat.Bcf21));
    }

    [Test]
    public void load_markup_without_topic_throws_invalid_data()
    {
      // given: an archive whose markup.bcf has no Topic element
      // (Topic is required by the BCF 2.1 schema: minOccurs="1")
      var adapter = new Bcf21FileAdapter();

      // when / then: the invalid archive is rejected at load, not silently mapped to
      // an issue with a null Topic (a domain invariant)
      Assert.Throws<InvalidDataException>(() => adapter.Load(_noTopicBcfPath));
    }

    // --- step 3: save always schema-valid ---

    [Test]
    public void save_produces_xsd_valid_xml()
    {
      // given
      var adapter = new Bcf21FileAdapter();
      var project = CreateConformantProject();
      var issue = project.Issues.Single();
      issue.Viewpoints.Add(new BcfViewpoint
      {
        ViewpointFileName = "viewpoint.bcfv",
        Data = new BcfViewpointData
        {
          Components = new BcfComponentsData
          {
            Visibility = new BcfComponentVisibilityData { DefaultVisibility = true },
            Selection = { new BcfComponentData { IfcGuid = "0qGg0wkWnDpON43MgbjXly" } }
          },
          PerspectiveCamera = new BcfPerspectiveCameraData
          {
            CameraViewPoint = new BcfPointData { X = 0, Y = 0, Z = 1 },
            CameraDirection = new BcfDirectionData { X = 0, Y = 0, Z = -1 },
            CameraUpVector = new BcfDirectionData { X = 0, Y = 1, Z = 0 },
            FieldOfView = 45
          },
          Lines = { new BcfLineData { StartPoint = new BcfPointData(), EndPoint = new BcfPointData { X = 1 } } },
          ClippingPlanes = { new BcfClippingPlaneData { Location = new BcfPointData(), Direction = new BcfDirectionData { Z = -1 } } }
        }
      });
      issue.Comments.Add(new BcfComment { Date = DateTime.Now, Author = "a@b.c", Comment = "hello" });
      Directory.CreateDirectory(_outputDir);
      var outPath = Path.Combine(_outputDir, "xsd_valid.bcf");

      // when
      Assert.That(adapter.Save(project, outPath), Is.True);

      // then: every XML in the archive validates against the official release_2_1 XSDs
      // (*.bcfv against the visinfo extended with the BCFier SheetCamera element)
      AssertXsdValid(outPath);
    }

    [Test]
    public void save_produces_xsd_valid_xml_with_sheet_camera()
    {
      // given: SheetCamera is a BCFier extension (declared via ExtensionSchema in project.bcfp)
      var adapter = new Bcf21FileAdapter();
      var project = CreateConformantProject();
      project.Issues.Single().Viewpoints.Add(new BcfViewpoint
      {
        ViewpointFileName = "viewpoint.bcfv",
        Data = new BcfViewpointData
        {
          PerspectiveCamera = new BcfPerspectiveCameraData
          {
            CameraViewPoint = new BcfPointData(),
            CameraDirection = new BcfDirectionData { Z = -1 },
            CameraUpVector = new BcfDirectionData { Y = 1 },
            FieldOfView = 45
          },
          SheetCamera = new BcfSheetCameraData
          {
            TopLeft = new BcfPointData { X = 1, Y = 2 },
            BottomRight = new BcfPointData { X = 3, Y = 4 },
            SheetID = 7,
            SheetName = "A-101"
          }
        }
      });
      Directory.CreateDirectory(_outputDir);
      var outPath = Path.Combine(_outputDir, "xsd_valid_sheet.bcf");

      // when
      Assert.That(adapter.Save(project, outPath), Is.True);

      // then
      AssertXsdValid(outPath);
    }

    [Test]
    public void save_declares_extension_schema()
    {
      // given
      var adapter = new Bcf21FileAdapter();
      var project = CreateConformantProject();
      Directory.CreateDirectory(_outputDir);
      var outPath = Path.Combine(_outputDir, "extension_schema.bcf");

      // when
      Assert.That(adapter.Save(project, outPath), Is.True);

      // then: project.bcfp declares the BCFier extension
      using (var archive = ZipFile.OpenRead(outPath))
      {
        var entry = archive.Entries.Single(e => e.Name == "project.bcfp");
        string content;
        using (var stream = entry.Open())
        using (var reader = new StreamReader(stream))
          content = reader.ReadToEnd();
        Assert.That(content, Does.Contain("<ExtensionSchema>https://bcfier.com/schemas/bcfier-extension-2.1.xsd</ExtensionSchema>"));
      }
    }

    [Test]
    public void save_writes_no_empty_list_elements()
    {
      // given: a viewpoint without lines/planes/bitmaps and a topic without extras —
      // the output must not contain empty <Lines/>, <ClippingPlanes/>, <Selection/> etc.
      // (they violate the official schema, which requires at least one child element)
      var adapter = new Bcf21FileAdapter();
      var project = CreateConformantProject();
      project.Issues.Single().Viewpoints.Add(new BcfViewpoint
      {
        ViewpointFileName = "viewpoint.bcfv",
        Data = new BcfViewpointData
        {
          PerspectiveCamera = new BcfPerspectiveCameraData
          {
            CameraViewPoint = new BcfPointData(),
            CameraDirection = new BcfDirectionData { Z = -1 },
            CameraUpVector = new BcfDirectionData { Y = 1 },
            FieldOfView = 45
          }
        }
      });
      Directory.CreateDirectory(_outputDir);
      var outPath = Path.Combine(_outputDir, "no_empty_lists.bcf");
      Assert.That(adapter.Save(project, outPath), Is.True);

      // when
      string visInfo;
      using (var archive = ZipFile.OpenRead(outPath))
      {
        var entry = archive.Entries.Single(e => e.Name.EndsWith(".bcfv"));
        using (var stream = entry.Open())
        using (var reader = new StreamReader(stream))
          visInfo = reader.ReadToEnd();
      }

      // then
      Assert.That(visInfo, Does.Not.Contain("<Lines />"));
      Assert.That(visInfo, Does.Not.Contain("<Lines/>"));
      Assert.That(visInfo, Does.Not.Contain("<ClippingPlanes />"));
      Assert.That(visInfo, Does.Not.Contain("<ClippingPlanes/>"));
      Assert.That(visInfo, Does.Not.Contain("<Selection />"));
      Assert.That(visInfo, Does.Not.Contain("<Selection/>"));
      Assert.That(visInfo, Does.Not.Contain("<Exceptions />"));
      Assert.That(visInfo, Does.Not.Contain("<Exceptions/>"));
    }

    [Test]
    public void roundtrip_conformant_preserves_data()
    {
      // given
      var adapter = new Bcf21FileAdapter();
      var project = CreateConformantProject();
      var issue = project.Issues.Single();
      issue.Viewpoints.Add(new BcfViewpoint
      {
        ViewpointFileName = "viewpoint.bcfv",
        Data = new BcfViewpointData
        {
          Guid = Guid.NewGuid().ToString(),
          Components = new BcfComponentsData
          {
            Visibility = new BcfComponentVisibilityData { DefaultVisibility = true }
          },
          PerspectiveCamera = new BcfPerspectiveCameraData
          {
            CameraViewPoint = new BcfPointData { X = 1.5, Y = -2.5, Z = 0.25 },
            CameraDirection = new BcfDirectionData { X = 0, Y = 0, Z = -1 },
            CameraUpVector = new BcfDirectionData { X = 0, Y = 1, Z = 0 },
            FieldOfView = 60
          }
        }
      });
      issue.Comments.Add(new BcfComment
      {
        Date = new DateTime(2026, 11, 1, 10, 0, 0, DateTimeKind.Local),
        Author = "author@example.com",
        Comment = "roundtrip comment",
        ViewpointGuid = issue.Viewpoints.Single().Guid
      });
      Directory.CreateDirectory(_outputDir);
      var outPath = Path.Combine(_outputDir, "roundtrip_conformant.bcf");

      // when
      Assert.That(adapter.Save(project, outPath), Is.True);
      var reloaded = LoadThroughAdapter(new Bcf21FileAdapter(), outPath);

      // then
      Assert.That(reloaded.SourceFormat, Is.EqualTo(ProjectFormat.Bcf21));
      var reloadedIssue = reloaded.Issues.Single();
      Assert.That(reloadedIssue.Topic.Guid, Is.EqualTo(issue.Topic.Guid));
      Assert.That(reloadedIssue.Topic.CreationAuthor, Is.EqualTo("someone@example.com"));
      Assert.That(reloadedIssue.Comments, Has.Count.EqualTo(1));
      Assert.That(reloadedIssue.Comments.Single().Comment, Is.EqualTo("roundtrip comment"));
      Assert.That(reloadedIssue.Comments.Single().ViewpointGuid, Is.EqualTo(issue.Viewpoints.Single().Guid));
      var data = reloadedIssue.Viewpoints.Single().Data;
      Assert.That(data.Components.Visibility.DefaultVisibility, Is.True);
      Assert.That(data.PerspectiveCamera.CameraViewPoint.X, Is.EqualTo(1.5));
      Assert.That(data.PerspectiveCamera.FieldOfView, Is.EqualTo(60));
    }

    [Test]
    public void roundtrip_preserves_sheet_camera()
    {
      // given
      var adapter = new Bcf21FileAdapter();
      var project = CreateConformantProject();
      var sheetCamera = new BcfSheetCameraData
      {
        TopLeft = new BcfPointData { X = 1, Y = 2, Z = 0 },
        BottomRight = new BcfPointData { X = 3, Y = 4, Z = 0 },
        SheetID = 42,
        SheetName = "A-101"
      };
      project.Issues.Single().Viewpoints.Add(new BcfViewpoint
      {
        ViewpointFileName = "viewpoint.bcfv",
        Data = new BcfViewpointData
        {
          PerspectiveCamera = new BcfPerspectiveCameraData
          {
            CameraViewPoint = new BcfPointData(),
            CameraDirection = new BcfDirectionData { Z = -1 },
            CameraUpVector = new BcfDirectionData { Y = 1 },
            FieldOfView = 45
          },
          SheetCamera = sheetCamera
        }
      });
      Directory.CreateDirectory(_outputDir);
      var outPath = Path.Combine(_outputDir, "roundtrip_sheet.bcf");

      // when: save (schema-valid) and reload
      Assert.That(adapter.Save(project, outPath), Is.True);
      var reloaded = LoadThroughAdapter(new Bcf21FileAdapter(), outPath);

      // then: the sheet camera survives the schema-valid round-trip
      var data = reloaded.Issues.Single().Viewpoints.Single().Data;
      Assert.That(data.SheetCamera, Is.Not.Null);
      Assert.That(data.SheetCamera.SheetID, Is.EqualTo(42));
      Assert.That(data.SheetCamera.SheetName, Is.EqualTo("A-101"));
      Assert.That(data.SheetCamera.TopLeft.X, Is.EqualTo(1));
      Assert.That(data.SheetCamera.BottomRight.Y, Is.EqualTo(4));
    }

    [Test]
    public void legacy_file_migrates_to_valid_on_save()
    {
      // given: a legacy (schema-violating) file opened through the adapter
      var adapter = new Bcf21FileAdapter();
      var project = LoadThroughAdapter(adapter, _legacyCameraBcfPath);
      Assert.That(project.SourceFormat, Is.EqualTo(ProjectFormat.Bcf21Legacy));
      Directory.CreateDirectory(_outputDir);
      var outPath = Path.Combine(_outputDir, "migrated.bcf");

      // when: saved (always schema-valid) and reloaded
      Assert.That(adapter.Save(project, outPath), Is.True);
      var reloaded = LoadThroughAdapter(new Bcf21FileAdapter(), outPath);

      // then: the file is now conformant 2.1; data survived the migration
      Assert.That(reloaded.SourceFormat, Is.EqualTo(ProjectFormat.Bcf21));
      Assert.That(adapter.Detect(outPath), Is.EqualTo(ProjectFormat.Bcf21));
      Assert.That(reloaded.Issues.Count, Is.EqualTo(project.Issues.Count));
      var originalViewpoint = project.Issues.Single().Viewpoints.Single();
      var reloadedViewpoint = reloaded.Issues.Single().Viewpoints.Single();
      Assert.That(reloadedViewpoint.ViewpointFileName, Is.EqualTo(originalViewpoint.ViewpointFileName));
      Assert.That(reloadedViewpoint.Data.PerspectiveCamera.CameraViewPoint.X,
        Is.EqualTo(originalViewpoint.Data.PerspectiveCamera.CameraViewPoint.X));
      Assert.That(reloadedViewpoint.Data.Components.Visibility.DefaultVisibility,
        Is.EqualTo(originalViewpoint.Data.Components.Visibility.DefaultVisibility));
      // and the migrated file is schema-valid
      AssertXsdValid(outPath);
    }

    [Test]
    public void save_fills_required_creation_author()
    {
      // given: a topic without CreationAuthor (required, minOccurs=1 in markup.xsd)
      var adapter = new Bcf21FileAdapter();
      var project = new BcfProject
      {
        TempPath = Path.Combine(_outputDir, "temp", Guid.NewGuid().ToString()),
        Issues =
        {
            new BcfIssue(new BcfTopic { CreationAuthor = null, ModifiedAuthor = null })
            {
            Comments =
            {
              new BcfComment
              {
                Date = DateTime.Now,
                Author = null,
                Comment = null,
                ModifiedAuthor = null
              }
            }
          }
        }
      };
      Directory.CreateDirectory(_outputDir);
      var outPath = Path.Combine(_outputDir, "creation_author.bcf");

      // when
      Assert.That(adapter.Save(project, outPath), Is.True);

      // then: CreationAuthor falls back to "unknown" and the markup validates
      AssertXsdValid(outPath);
      string markup;
      using (var archive = ZipFile.OpenRead(outPath))
      {
        var entry = archive.Entries.Single(e => e.Name == "markup.bcf");
        using (var stream = entry.Open())
        using (var reader = new StreamReader(stream))
          markup = reader.ReadToEnd();
      }
      Assert.That(markup, Does.Contain("<CreationAuthor>unknown</CreationAuthor>"));
    }

    [Test]
    public void detect_after_valid_save_returns_bcf21()
    {
      // given: a file saved by the schema-valid codec
      var adapter = new Bcf21FileAdapter();
      var project = CreateConformantProject();
      Directory.CreateDirectory(_outputDir);
      var outPath = Path.Combine(_outputDir, "detect_after_save.bcf");
      Assert.That(adapter.Save(project, outPath), Is.True);

      // when / then
      Assert.That(adapter.Detect(outPath), Is.EqualTo(ProjectFormat.Bcf21));
    }

    private void AssertXsdValid(string bcfPath)
    {
      var schemasDir = Path.Combine(AppContext.BaseDirectory, "Schemas", "release_2_1");
      var totalErrors = 0;
      using (var archive = ZipFile.OpenRead(bcfPath))
      {
        foreach (var entry in archive.Entries)
        {
          string xsdFile;
          if (entry.Name == "markup.bcf")
            xsdFile = "markup.xsd";
          else if (entry.Name == "project.bcfp")
            xsdFile = "project.xsd";
          else if (entry.Name == "bcf.version")
            xsdFile = "version.xsd";
          else if (entry.Name.EndsWith(".bcfv", StringComparison.OrdinalIgnoreCase))
            xsdFile = "visinfo_bcfier.xsd";
          else
            continue;

          using (var schemaStream = File.OpenRead(Path.Combine(schemasDir, xsdFile)))
          {
            var schema = XmlSchema.Read(schemaStream, null);
            Assert.That(schema, Is.Not.Null, $"schema '{xsdFile}' must be readable");
            var set = new System.Xml.Schema.XmlSchemaSet();
            set.Add(schema!);

            var entryErrors = 0;
            var entryMessages = new List<string>();
            using (var entryStream = entry.Open())
            {
              var doc = System.Xml.Linq.XDocument.Load(entryStream);
              doc.Validate(set, (o, e) => { entryErrors++; entryMessages.Add(e.Message); });
            }
            Assert.That(entryErrors, Is.Zero,
              $"entry '{entry.FullName}' must be valid against '{xsdFile}': {string.Join("; ", entryMessages)}");
            totalErrors += entryErrors;
          }
        }
      }
      Assert.That(totalErrors, Is.Zero);
    }

    private const string Bcf21VersionXml =
      "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
      "<Version VersionId=\"2.1\"><DetailedVersion>2.1</DetailedVersion></Version>";

    private static void CreateBcfZip(string path, Action<BcfZipBuilder> configure)
    {
      using (var archive = ZipFile.Open(path, ZipArchiveMode.Create))
      {
        configure(new BcfZipBuilder(archive));
      }
    }

    private class BcfZipBuilder
    {
      private readonly ZipArchive _archive;

      public BcfZipBuilder(ZipArchive archive)
      {
        _archive = archive;
      }

      public void AddFile(string name, string content)
      {
        var entry = _archive.CreateEntry(name);
        using (var writer = new StreamWriter(entry.Open()))
        {
          writer.Write(content);
        }
      }
    }
  }
}
