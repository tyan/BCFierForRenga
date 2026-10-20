using System;
using System.Collections.Generic;

namespace Bcfier.Bcf.Model
{
  /// <summary>
  /// Version-neutral domain model of a single BCF issue (aggregate root:
  /// topic + comments + viewpoints). Pure POCO — no serialization attributes.
  /// </summary>
  public class BcfIssue
  {
    public BcfTopic Topic { get; set; }
    public List<BcfComment> Comments { get; set; } = new List<BcfComment>();
    public List<BcfViewpoint> Viewpoints { get; set; } = new List<BcfViewpoint>();
    public List<BcfHeaderFile> HeaderFiles { get; set; } = new List<BcfHeaderFile>();
    public int Index { get; set; }
    public bool HasBeenSaved { get; set; }
    public string TempPath { get; set; }

    public BcfIssue()
    {
    }

    public BcfIssue(DateTime created)
    {
      Topic = new BcfTopic(created);
    }
  }

  /// <summary>
  /// Version-neutral domain model of a BCF header file reference (Header/File in 2.1/3.0 markup).
  /// Pure POCO — no serialization attributes.
  /// </summary>
  public class BcfHeaderFile
  {
    public string Filename { get; set; }
    public DateTime Date { get; set; }
    public string Reference { get; set; }
    public string IfcProject { get; set; }
    public string IfcSpatialStructureElement { get; set; }
    public bool IsExternal { get; set; }
  }
}
