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
}
