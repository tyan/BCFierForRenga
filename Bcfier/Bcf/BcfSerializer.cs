using Bcfier.Bcf.Bcf2;
using System;
using System.Collections.ObjectModel;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Windows.Controls;

namespace Bcfier.Bcf
{
  internal class BcfSerializer
  {
    public static string FileExtension
    {
      get { return ".bcf"; }
    }
    public static bool save(BcfFile bcf, string filename)
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
        ExtensionSchema = ""

      };
      var bcfVersion = new Bcf2.Version { VersionId = "2.1", DetailedVersion = "2.1" };

      var serializerP = new XmlSerializer(typeof(ProjectExtension));
      Stream writerP = new FileStream(Path.Combine(bcf.TempPath, "project.bcfp"), FileMode.Create);
      serializerP.Serialize(writerP, bcfProject);
      writerP.Close();

      var serializerVers = new XmlSerializer(typeof(Bcf2.Version));
      Stream writerVers = new FileStream(Path.Combine(bcf.TempPath, "bcf.version"), FileMode.Create);
      serializerVers.Serialize(writerVers, bcfVersion);
      writerVers.Close();

      var serializerV = new XmlSerializer(typeof(VisualizationInfo));
      var serializerM = new XmlSerializer(typeof(Markup));

      var i = 0;
      foreach (var issue in bcf.Issues)
      {
        //set topic index
        issue.Topic.Index = i;
        issue.Topic.IndexSpecified = true;
        i++;

        // serialize the object, and close the TextWriter
        string issuePath = Path.Combine(bcf.TempPath, issue.Topic.Guid);
        if (!Directory.Exists(issuePath))
          Directory.CreateDirectory(issuePath);

        //set viewpoint index
        for (var l = 0; l < issue.Viewpoints.Count; l++)
        {
          issue.Viewpoints[l].Index = l;
          issue.Viewpoints[l].IndexSpecified = true;
        }

        //BCF 1 compatibility
        //there needs to be a view whose viewpoint and snapshot are named as follows and not with a guid
        //uniqueness is still guarenteed by the guid field
        if (issue.Viewpoints.Any() && (issue.Viewpoints.Count == 1 || issue.Viewpoints.All(o => o.Viewpoint != "viewpoint.bcfv")))
        {
          if (File.Exists(Path.Combine(issuePath, issue.Viewpoints[0].Viewpoint)))
            File.Delete(Path.Combine(issuePath, issue.Viewpoints[0].Viewpoint));
          issue.Viewpoints[0].Viewpoint = "viewpoint.bcfv";
          if (File.Exists(Path.Combine(issuePath, issue.Viewpoints[0].Snapshot)))
            File.Move(Path.Combine(issuePath, issue.Viewpoints[0].Snapshot), Path.Combine(issuePath, "snapshot.png"));
          issue.Viewpoints[0].Snapshot = "snapshot.png";
        }
        //serialize markup with updated content
        Stream writerM = new FileStream(Path.Combine(issuePath, "markup.bcf"), FileMode.Create);
        serializerM.Serialize(writerM, issue);
        writerM.Close();
        //serialize views
        foreach (var bcfViewpoint in issue.Viewpoints)
        {
          Stream writerV = new FileStream(Path.Combine(issuePath, bcfViewpoint.Viewpoint), FileMode.Create);
          serializerV.Serialize(writerV, bcfViewpoint.VisInfo);
          writerV.Close();
        }
      }

      //overwrite, without doubts
      if (File.Exists(filename))
        File.Delete(filename);

      //added encoder to address backslashes issue #11
      //issue: https://github.com/teocomi/BCFier/issues/11
      //ref: http://stackoverflow.com/questions/27289115/system-io-compression-zipfile-net-4-5-output-zip-in-not-suitable-for-linux-mac
      ZipFile.CreateFromDirectory(bcf.TempPath, filename, CompressionLevel.Optimal, false, new ZipEncoder());

      bcf.HasBeenSaved = true;
      bcf.Filename = Path.GetFileNameWithoutExtension(filename);
      return true;
    }

    public static BcfFile load(string filePath)
    {
      if (!File.Exists(filePath) || !String.Equals(Path.GetExtension(filePath), FileExtension, StringComparison.InvariantCultureIgnoreCase))
        return null;

      var bcf = new BcfFile();
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
        //An issue needs at least the markup file
        var markupFile = Path.Combine(dirInfo.FullName, "markup.bcf");
        if (!File.Exists(markupFile))
          continue;

        var issue = DeserializeMarkup(markupFile);
        Trace.Assert(issue != null);

        DeserializeViewpoints(issue, dirInfo);
        
        issue.Comment = new ObservableCollection<Comment>(issue.Comment.OrderBy(x => x.Date));
        issue.Viewpoints = new ObservableCollection<ViewPoint>(issue.Viewpoints.OrderBy(x => x.Index));

        //register the collectionchanged events,
        //it is needed since deserialization overwrites the ones set in the constructor
        issue.RegisterEvents();

        //ViewComment stuff
        bcf.Issues.Add(issue);
      }

      bcf.Issues = new ObservableCollection<Markup>(bcf.Issues.OrderBy(x => x.Topic.Index));
      return bcf;
    }

    private static void DeserializeViewpoints(Markup issue, DirectoryInfo issueDirInfo)
    {
      //Is a BCF 2 file, has multiple viewpoints
      if (issue.Viewpoints != null && issue.Viewpoints.Any())
      {
        foreach (var viewpoint in issue.Viewpoints)
        {
          string viewpointpath = Path.Combine(issueDirInfo.FullName, viewpoint.Viewpoint);
          if (File.Exists(viewpointpath))
          {
            //deserializing the viewpoint into the issue
            viewpoint.VisInfo = DeserializeViewpoint(viewpointpath);
            viewpoint.SnapshotPath = Path.Combine(issueDirInfo.FullName, viewpoint.Snapshot);
          }
        }
      }
      //Is a BCF 1 file, only one viewpoint
      //there is no Viewpoints tag in the markup
      //update it to BCF 2
      else
      {
        issue.Viewpoints = new ObservableCollection<ViewPoint>();
        string viewpointFile = Path.Combine(issueDirInfo.FullName, "viewpoint.bcfv");
        if (File.Exists(viewpointFile))
        {
          issue.Viewpoints.Add(new ViewPoint(true)
          {
            VisInfo = DeserializeViewpoint(viewpointFile),
            SnapshotPath = Path.Combine(issueDirInfo.FullName, "snapshot.png"),
          });
          //update the comments
          foreach (var comment in issue.Comment)
          {
            comment.Viewpoint = new CommentViewpoint();
            comment.Viewpoint.Guid = issue.Viewpoints.First().Guid;
          }
        }
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
