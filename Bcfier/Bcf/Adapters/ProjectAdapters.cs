using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using Bcfier.Bcf.Model;

namespace Bcfier.Bcf.Adapters
{
  /// <summary>
  /// Adapter around the BCF 2.1 persistence (BcfSerializer + Bcf2.* / Bcf21 codec).
  /// Handles both the legacy (schema-violating) and the schema-valid 2.1 variants;
  /// <see cref="Detect"/> tells them apart and <see cref="Load"/> records the detected
  /// sub-format on <see cref="BcfProject.SourceFormat"/> so the matching codec is used on save.
  /// Lives in Bcfier.Bcf.Adapters (not in the domain model).
  /// </summary>
  public class Bcf21FileAdapter : IProjectAdapter
  {
    public ProjectFormat Format { get { return ProjectFormat.Bcf21; } }

    public BcfProject Load(string path)
    {
      var detected = Detect(path);
      var bcf = BcfSerializer.load(path);
      if (bcf == null)
        return null;
      var project = Bcf21Mapper.ToDomain(bcf);
      project.SourceFormat = detected;
      return project;
    }

    public bool Save(BcfProject project, string path)
    {
      if (project == null)
        return false;
      var bcf = Bcf21Mapper.FromDomain(project);
      
      return BcfSerializer.save(bcf, path);
    }

    /// <summary>
    /// Detects the BCF sub-version and 2.1 sub-format of a .bcf archive. The version check is
    /// based on what must be in the archive (bcf.version with VersionId) and the recorded
    /// version — not on heuristics over the markup content. Only BCF 2.1 is supported; the
    /// legacy sub-format (schema-violating 2.1) is told apart from the conformant one against
    /// the official markup.xsd (release_2_1): the plural element names
    /// <c>DocumentReferences</c>/<c>RelatedTopics</c> are not part of the schema (singular
    /// <c>DocumentReference</c>/<c>RelatedTopic</c> is) and a missing <c>CreationAuthor</c>
    /// (minOccurs="1" in the schema) both mark a legacy file.
    /// Any legacy marker in any topic markup -> <see cref="ProjectFormat.Bcf21Legacy"/>,
    /// otherwise -> <see cref="ProjectFormat.Bcf21"/>.
    /// </summary>
    public ProjectFormat Detect(string path)
    {
      if (path == null || !File.Exists(path))
        throw new FileNotFoundException("BCF archive not found.", path);

      using (var archive = ZipFile.OpenRead(path))
      {
        var versionFile = FindSingleEntry(archive.Entries, "bcf.version");
        if (versionFile == null)
          throw new FileFormatException("Bcf archive should contain bcf.version: " + path);

        var versionDoc = LoadDocument(versionFile);
        var versionIdAttr = versionDoc.Root != null ? versionDoc.Root.Attribute("VersionId") : null;
        if (versionIdAttr == null)
          throw new FileFormatException("bcf.version file invalid format: " + path);

        if (versionIdAttr.Value != "2.1")
          throw new NotSupportedException("Unsupported file format. Only BCF 2.1 is supported.");

        foreach (var markupEntry in archive.Entries)
        {
          if (string.IsNullOrEmpty(markupEntry.Name) || !markupEntry.Name.Equals("markup.bcf", StringComparison.OrdinalIgnoreCase))
            continue;

          var markupDoc = LoadDocument(markupEntry);
          if (markupDoc == null)
            continue;

          if (IsLegacyMarkup(markupDoc))
            return ProjectFormat.Bcf21Legacy;
        }
      }

      return ProjectFormat.Bcf21;
    }

    private static ZipArchiveEntry FindSingleEntry(IEnumerable<ZipArchiveEntry> entries, string name)
    {
      return entries.SingleOrDefault(entry => string.Equals(entry.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    private static XDocument LoadDocument(ZipArchiveEntry entry)
    {
      using (var stream = entry.Open())
      {
        return XDocument.Load(stream);
      }
    }

    private static bool IsLegacyMarkup(XDocument markup)
    {
      var topic = markup.Root != null ? markup.Root.Element("Topic") : null;
      if (topic == null)
        return false;

      if (topic.Element("DocumentReferences") != null)
        return true;
      if (topic.Element("RelatedTopics") != null)
        return true;

      // CreationAuthor is required in the schema (minOccurs="1"); legacy files omit it.
      return topic.Element("CreationAuthor") == null;
    }
  }

  /// <summary>
  /// Creates the adapter matching a given BCF format.
  /// </summary>
  public static class ProjectAdapterFactory
  {
    public static IProjectAdapter Create(ProjectFormat format)
    {
      switch (format)
      {
        case ProjectFormat.Bcf21Legacy:
        case ProjectFormat.Bcf21:
          return new Bcf21FileAdapter();
        default:
          throw new System.NotSupportedException("Unsupported project format: " + format);
      }
    }
  }
}
