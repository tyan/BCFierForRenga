using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Serialization;
using Bcfier.Bcf.Bcf21;

namespace Bcfier.Bcf.Adapters
{
  /// <summary>
  /// Schema-valid BCF 2.1 codec: writes/reads .bcf zip containers whose XML is valid
  /// against the official release_2_1 XSDs. The only save codec of the adapter (step 3);
  /// the legacy <c>BcfSerializer</c> remains a read-only fallback for old files.
  /// Same zip layout as the legacy codec (bcf.version, project.bcfp, guid/markup.bcf,
  /// guid/*.bcfv, snapshots). SheetCamera inside *.bcfv is a BCFier extension declared
  /// via &lt;ExtensionSchema&gt; in project.bcfp.
  /// </summary>
  internal static class Bcf21ValidSerializer
  {
    /// <summary>
    /// URI of the BCFier extension schema (SheetCamera etc.), written into project.bcfp
    /// per the BCF 2.1 ExtensionSchema mechanism.
    /// </summary>
    public const string ExtensionSchemaUri = "https://bcfier.com/schemas/bcfier-extension-2.1.xsd";

    public static string FileExtension
    {
      get { return ".bcf"; }
    }

    /// <summary>
    /// Serializes the container into a schema-valid .bcf file. Viewpoint payloads (.bcfv)
    /// and snapshots are taken from the container's TempPath folder (guid subfolders),
    /// as written by <see cref="Bcfier.Bcf.ViewModel.BcfFileVM"/> when adding views.
    /// </summary>
    public static bool Save(Bcf21Container bcf, string filename)
    {
      if (!Directory.Exists(bcf.TempPath))
        Directory.CreateDirectory(bcf.TempPath);

      var bcfProject = new ProjectExtension
      {
        Project = new Project
        {
          Name = string.IsNullOrEmpty(bcf.ProjectName) ? bcf.Filename : bcf.ProjectName,
          ProjectId = bcf.ProjectId.Equals(Guid.Empty) ? Guid.NewGuid().ToString() : bcf.ProjectId.ToString()
        },
        // Declare the BCFier extension (SheetCamera) so third-party tools can detect it.
        ExtensionSchema = ExtensionSchemaUri
      };
      var bcfVersion = new Bcf21.Version { VersionId = "2.1", DetailedVersion = "2.1" };

      var serializerP = new XmlSerializer(typeof(ProjectExtension));
      using (var writerP = new FileStream(Path.Combine(bcf.TempPath, "project.bcfp"), FileMode.Create))
      {
        serializerP.Serialize(writerP, bcfProject);
      }

      var serializerVers = new XmlSerializer(typeof(Bcf21.Version));
      using (var writerVers = new FileStream(Path.Combine(bcf.TempPath, "bcf.version"), FileMode.Create))
      {
        serializerVers.Serialize(writerVers, bcfVersion);
      }

      var serializerV = new XmlSerializer(typeof(VisualizationInfo));
      var serializerM = new XmlSerializer(typeof(Markup));

      var i = 0;
      foreach (var issue in bcf.Issues)
      {
        // set topic index
        issue.Topic.Index = i;
        issue.Topic.IndexSpecified = true;
        i++;

        string issuePath = Path.Combine(bcf.TempPath, issue.Topic.Guid);
        if (!Directory.Exists(issuePath))
          Directory.CreateDirectory(issuePath);

        // set viewpoint index
        for (var l = 0; l < issue.Viewpoints.Length; l++)
          issue.Viewpoints[l].Index = l;

        // serialize markup with updated content
        using (var writerM = new FileStream(Path.Combine(issuePath, "markup.bcf"), FileMode.Create))
        {
          serializerM.Serialize(writerM, issue);
        }

        // serialize viewpoints (payloads already in TempPath as *.bcfv)
        foreach (var bcfViewpoint in issue.Viewpoints)
        {
          var visInfo = bcfViewpoint.VisInfo;
          if (visInfo == null)
            continue;
          using (var writerV = new FileStream(Path.Combine(issuePath, bcfViewpoint.Viewpoint), FileMode.Create))
          {
            serializerV.Serialize(writerV, visInfo);
          }
        }
      }

      // overwrite, without doubts
      if (File.Exists(filename))
        File.Delete(filename);

      // backslashes in entry names would break non-Windows readers (BCFier issue #11)
      ZipFile.CreateFromDirectory(bcf.TempPath, filename, CompressionLevel.Optimal, false, new Bcfier.Bcf.ZipEncoder());

      bcf.Filename = Path.GetFileNameWithoutExtension(filename);
      bcf.Fullname = Path.GetFullPath(filename);
      return true;
    }

    /// <summary>
    /// Reads a schema-valid .bcf file into a container. Viewpoint payloads (.bcfv) and
    /// snapshots are extracted to the container's TempPath (guid subfolders), where the
    /// rest of the application expects them.
    /// </summary>
    public static Bcf21Container Load(string filePath)
    {
      if (!File.Exists(filePath) || !String.Equals(Path.GetExtension(filePath), FileExtension, StringComparison.InvariantCultureIgnoreCase))
        return null;

      var bcf = new Bcf21Container();
      bcf.Filename = Path.GetFileNameWithoutExtension(filePath);
      bcf.Fullname = filePath;

      using (ZipArchive archive = ZipFile.OpenRead(filePath))
      {
        archive.ExtractToDirectory(bcf.TempPath);
      }

      var projectFile = Path.Combine(bcf.TempPath, "project.bcfp");
      if (File.Exists(projectFile))
      {
        var project = DeserializeProject(projectFile);
        var projectId = Guid.NewGuid();
        if (Guid.TryParse(project.Project.ProjectId, out projectId))
          bcf.ProjectId = projectId;
      }

      // Add issues for each subfolder
      foreach (var dirInfo in new DirectoryInfo(bcf.TempPath).GetDirectories())
      {
        // An issue needs at least the markup file
        var markupFile = Path.Combine(dirInfo.FullName, "markup.bcf");
        if (!File.Exists(markupFile))
          continue;

        var issue = DeserializeMarkup(markupFile);
        Trace.Assert(issue != null);

        // Topic is required by the BCF 2.1 schema (minOccurs="1"); an archive without it is
        // invalid — fail with a clear error instead of an NRE in the ordering below.
        if (issue.Topic == null)
          throw new InvalidDataException($"markup.bcf without Topic: '{markupFile}'");

        DeserializeViewpoints(issue, dirInfo);

        bcf.Issues.Add(issue);
      }

      bcf.Issues = bcf.Issues.OrderBy(x => x.Topic.Index).ToList();
      return bcf;
    }

    private static void DeserializeViewpoints(Markup issue, DirectoryInfo issueDirInfo)
    {
      // viewpoint payloads are separate .bcfv files referenced by markup
      if (issue.Viewpoints == null || issue.Viewpoints.Length == 0)
        return;

      foreach (var viewpoint in issue.Viewpoints)
      {
        string viewpointpath = Path.Combine(issueDirInfo.FullName, viewpoint.Viewpoint);
        if (!File.Exists(viewpointpath))
          throw new InvalidDataException($"Missing '{viewpoint.Viewpoint}' referenced in markup.");

        viewpoint.VisInfo = DeserializeViewpoint(viewpointpath);
      }
    }

    private static VisualizationInfo DeserializeViewpoint(string path)
    {
      using (var viewpointFile = new FileStream(path, FileMode.Open))
      {
        var serializerS = new XmlSerializer(typeof(VisualizationInfo));
        return serializerS.Deserialize(viewpointFile) as VisualizationInfo;
      }
    }

    private static Markup DeserializeMarkup(string path)
    {
      using (var markupFile = new FileStream(path, FileMode.Open))
      {
        var serializerM = new XmlSerializer(typeof(Markup));
        return serializerM.Deserialize(markupFile) as Markup;
      }
    }

    private static ProjectExtension DeserializeProject(string path)
    {
      using (var markupFile = new FileStream(path, FileMode.Open))
      {
        var serializerM = new XmlSerializer(typeof(ProjectExtension));
        return serializerM.Deserialize(markupFile) as ProjectExtension;
      }
    }
  }
}
